using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace BestiaryMapper
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        [NonSerialized]
        private IDalamudPluginInterface? pluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface) => this.pluginInterface = pluginInterface;

        public void Save()
        {
            if (this.pluginInterface is null)
                throw new InvalidOperationException("Plugin interface not initialized. Call Initialize before Save.");
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
