using IPA;
using IPA.Logging;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Settings;
using IPA.Config;
using IPA.Config.Stores;

namespace OBSAutoStart
{

    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin instance { get; private set; }
        internal static IPALogger log;
        internal static string Name => "OBSAutoStart";
        internal static PluginConfig config;

        [Init]
        public void Init(IPALogger logger, Config conf)
        {
            instance = this;
            log = logger;
            log.Debug("Logger initialized.");
            config = conf.Generated<PluginConfig>();
            BSMLSettings.instance.AddSettingsMenu("OBSAutoStart", "OBSAutoStart.Resources.UI.SettingsView.bsml", config);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            log.Debug("OnApplicationStart");
            new GameObject("OBSControl_OBSController").AddComponent<OBSController>();
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            log.Debug("OnApplicationQuit");
            GameObject.Destroy(OBSController.instance?.gameObject);
        }

    }
}
