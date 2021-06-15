using BeatSaberMarkupLanguage.Attributes;
namespace OBSAutoStart {
    public class PluginConfig {
        [UIValue(nameof(ServerAddress))]
        public virtual string ServerAddress { get; set; } = "ws://127.0.0.1:4444";
        [UIValue(nameof(ServerPassword))]
        public virtual string ServerPassword { get; set; } = string.Empty;
    }
}
