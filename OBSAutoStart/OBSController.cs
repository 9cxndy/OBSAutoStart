using System;
using UnityEngine;
using OBSWebsocketDotNet;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace OBSAutoStart {
    internal class OBSController : MonoBehaviour {
        internal static GameObject? instance;
        private Task? task;
        private CancellationTokenSource? cancelSource;
        public bool TryConnect(OBSWebsocket obs)
        {
            string serverAddress = Plugin.config.ServerAddress;
            if(serverAddress == null || serverAddress.Length == 0)
            {
                Plugin.log?.Error($"ServerAddress cannot be null or empty.");
                return false;
            }
            if (!obs.IsConnected)
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

        private void OBSTask(CancellationToken cancel) {
            var obs = new OBSWebsocket();
            while (!cancel.IsCancellationRequested) {
                if (!obs.IsConnected)
                {
                    try {
                        Plugin.log?.Info($"Attempting to connect to {Plugin.config.ServerAddress}");
                        if (TryConnect(obs)) {
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
                    }
                    catch (Exception ex)
                    {
                        Plugin.log?.Error($"Error in RepeatTryConnect: {ex.Message}");
                        Plugin.log?.Debug(ex);
                    }
                }
                try {
                    Task.Delay(2000, cancel).Wait();
                } catch (Exception)
                {

                }
            }
            if (obs.IsConnected) {
                obs.StopRecording();
            }
            obs.Disconnect();
        }
        private void Awake() {
            if (instance != null) {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            GameObject.DontDestroyOnLoad(gameObject);
            instance = gameObject;
            cancelSource = new CancellationTokenSource();
            task = Task.Run(() => OBSTask(cancelSource.Token));
        }
        private void OnDestroy()
        {
            Plugin.log?.Debug("Stopping OBS recording");
            cancelSource?.Cancel();
            cancelSource = null;
            instance = null;
            Task.WaitAll(task);
            task = null;
        }
    }
}
