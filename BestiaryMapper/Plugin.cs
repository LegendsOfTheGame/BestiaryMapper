using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BestiaryMapper
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Bestiary Mapper";

        private const string CommandName = "/bestiary";

        internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        internal static ICommandManager CommandManager { get; private set; } = null!;
        internal static IDataManager DataManager { get; private set; } = null!;
        internal static IGameGui GameGui { get; private set; } = null!;
        internal static IPluginLog PluginLog { get; private set; } = null!;

        private Configuration Configuration { get; init; }
        private BeastMapService BeastMapService { get; init; }
        private MainWindow MainWindow { get; init; }

        public Plugin(
            IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager,
            IDataManager dataManager,
            IGameGui gameGui,
            IPluginLog pluginLog)
        {
            PluginInterface = pluginInterface;
            CommandManager = commandManager;
            DataManager = dataManager;
            GameGui = gameGui;
            PluginLog = pluginLog;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            BeastMapService = new BeastMapService(dataManager, gameGui, pluginLog);
            var beasts = LoadBeasts();
            MainWindow = new MainWindow(this, BeastMapService, beasts);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens the Bestiary Mapper"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenMainUi += DrawMainUI;
        }

        public void Dispose()
        {
            MainWindow.Dispose();
            CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args) => DrawMainUI();

        private void DrawUI() => MainWindow.Draw();

        private void DrawMainUI() => MainWindow.Visible = true;

        private static System.Collections.Generic.List<Beast> LoadBeasts()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = Array.Find(assembly.GetManifestResourceNames(), n => n.EndsWith("data.json", StringComparison.Ordinal));
            if (resourceName == null) return new();

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return new();

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<System.Collections.Generic.List<Beast>>(json) ?? new();
        }
    }
}
