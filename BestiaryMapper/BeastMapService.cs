using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace BestiaryMapper
{
    /// <summary>
    /// Resolves a wiki zone name ("Central Shroud") to the TerritoryType/Map
    /// row pair a MapLinkPayload needs, and opens the in-game map flagged at
    /// a beast's coordinates. Resolution is by PlaceName match against the
    /// TerritoryType sheet rather than a hardcoded id table, so it keeps
    /// working across patches without maintenance.
    /// </summary>
    public unsafe class BeastMapService
    {
        private readonly IDataManager dataManager;
        private readonly IGameGui gameGui;
        private readonly IPluginLog pluginLog;
        private readonly Dictionary<string, (uint TerritoryId, uint MapId)> zoneCache = new(System.StringComparer.OrdinalIgnoreCase);

        public BeastMapService(IDataManager dataManager, IGameGui gameGui, IPluginLog pluginLog)
        {
            this.dataManager = dataManager;
            this.gameGui = gameGui;
            this.pluginLog = pluginLog;
        }

        /// <summary>
        /// Whether this beast has been tamed, read live from XBMManager rather than
        /// stored anywhere — Beast.No is the same row id the Master's Bestiary window
        /// itself uses (confirmed against MemoriaProbe's "bstpets" probe: NumUnlockedPets
        /// matched the window's own capture count exactly, and every row read the same
        /// yes/no the window showed).
        /// </summary>
        public bool IsCaptured(Beast beast)
        {
            var xbm = XBMManager.Instance();
            return xbm is not null && xbm->State == XBMManager.DataState.Received && xbm->IsPetUnlocked((uint)beast.No);
        }

        public bool TryOpenOnMap(Beast beast)
        {
            if (!beast.HasMapLocation) return false;
            if (!TryResolveZone(beast.Zone!, out var territoryId, out var mapId)) return false;

            var link = new MapLinkPayload(territoryId, mapId, beast.X!.Value, beast.Y!.Value);
            gameGui.OpenMapWithMapLink(link);
            return true;
        }

        private bool TryResolveZone(string zoneName, out uint territoryId, out uint mapId)
        {
            if (zoneCache.TryGetValue(zoneName, out var cached))
            {
                territoryId = cached.TerritoryId;
                mapId = cached.MapId;
                return true;
            }

            foreach (var territory in dataManager.GetExcelSheet<TerritoryType>())
            {
                if (territory.PlaceName.ValueNullable is not PlaceName placeName) continue;
                if (!string.Equals(placeName.Name.ToString(), zoneName, System.StringComparison.OrdinalIgnoreCase)) continue;
                if (territory.Map.ValueNullable is not Map map) continue;

                territoryId = territory.RowId;
                mapId = map.RowId;
                zoneCache[zoneName] = (territoryId, mapId);
                return true;
            }

            pluginLog.Warning($"BestiaryMapper: could not resolve zone '{zoneName}' to a TerritoryType.");
            territoryId = 0;
            mapId = 0;
            return false;
        }
    }
}
