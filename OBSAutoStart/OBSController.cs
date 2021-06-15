using System;
using UnityEngine;
using OBSWebsocketDotNet;
using System.Threading.Tasks;
#nullable enable
namespace OBSAutoStart {
    internal class OBSController : MonoBehaviour {
        internal static GameObject? instance;
        private OBSWebsocket? obs;
        public bool TryConnect()
        {
            string serverAddress = Plugin.config.ServerAddress;
            if(serverAddress == null || serverAddress.Length == 0)
            {
                Plugin.log?.Error($"ServerAddress cannot be null or empty.");
                return false;
            }
            if (obs != null && !obs.IsConnected)
            {
                try
                {
                    obs.Connect(serverAddress, Plugin.config.ServerPassword);
                    Plugin.log?.Info($"Finished attempting to connect to {serverAddress}");
                }
                catch (AuthFailureException)
                {
                    Plugin.log?.Error($"Authentication failed connecting to server {serverAddress}.");
                    return false;
                }
                catch (ErrorResponseException ex)
                {
                    Plugin.log?.Error($"Failed to connect to server {serverAddress}: {ex.Message}.");
                    return false;
                }
                catch (Exception ex)
                {
                    Plugin.log?.Error($"Failed to connect to server {serverAddress}: {ex.Message}.");
                    Plugin.log?.Debug(ex);
                    return false;
                }
                if (obs.IsConnected)
                    Plugin.log?.Info($"Connected to OBS @ {serverAddress}");
            }
            else
                Plugin.log?.Info("TryConnect: OBS is already connected.");
            return obs?.IsConnected ?? false;
        }

        private DateTime? lastAttempt;
        private void Update()
        {
            var now = DateTime.Now;
            if (obs != null && !obs.IsConnected && (lastAttempt == null || DateTime.Now - lastAttempt > TimeSpan.FromSeconds(5)))
            {
                try {
                    Plugin.log?.Info($"Attempting to connect to {Plugin.config.ServerAddress}");
                    if (TryConnect()) {
                        Plugin.log?.Info($"OBS {obs.GetVersion().OBSStudioVersion} is connected.");
                        var recordingStatus = obs.GetRecordingStatus();
                        if (!recordingStatus.IsRecording) {
                            Plugin.log?.Info($"OBS start recording.");
                            obs.StartRecording();
                        } else if (recordingStatus.IsRecordingPaused) {
                            Plugin.log?.Info($"OBS resume recording.");
                            obs.ResumeRecording();
                        }
                    }
                    lastAttempt = now;
                }
                catch (Exception ex)
                {
                    Plugin.log?.Error($"Error in RepeatTryConnect: {ex.Message}");
                    Plugin.log?.Debug(ex);
                }
            }
        }
        private void Awake() {
            if (instance != null) {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            GameObject.DontDestroyOnLoad(gameObject);
            instance = gameObject;
            obs = new OBSWebsocket();
        }
        private void OnDisable()
        {
            Plugin.log?.Debug("Stopping OBS recording");
            if (obs != null && obs.IsConnected) {
                obs.StopRecording();
            }
            instance = null;
            obs = null;
        }
    }
}
