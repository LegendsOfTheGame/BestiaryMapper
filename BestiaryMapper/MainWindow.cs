using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace BestiaryMapper
{
    class MainWindow : Window, IDisposable
    {
        private readonly Plugin plugin;
        private readonly BeastMapService beastMapService;
        private readonly List<Beast> beasts;

        private bool visible = false;
        public bool Visible
        {
            get => visible;
            set => visible = value;
        }

        private string searchText = "";

        public MainWindow(Plugin plugin, BeastMapService beastMapService, List<Beast> beasts)
            : base("Bestiary Mapper##main_window")
        {
            this.plugin = plugin;
            this.beastMapService = beastMapService;
            this.beasts = beasts;
        }

        public void Dispose() { }

        public override void Draw()
        {
            if (!Visible) return;

            ImGui.SetNextWindowSize(new Vector2(640, 480), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(480, 300), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin(plugin.Name, ref visible))
            {
                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint("##search", "Search by name, source, or zone...", ref searchText, 128);

                var filtered = string.IsNullOrWhiteSpace(searchText)
                    ? beasts.ToList()
                    : beasts.Where(b =>
                        b.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        b.Source.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        (b.Zone?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                      .ToList();

                if (ImGui.BeginTable("##beast_table", 7,
                        ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY |
                        ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable))
                {
                    ImGui.TableSetupColumn("No.", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.DefaultSort, 32);
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 110);
                    ImGui.TableSetupColumn("Tamed From", ImGuiTableColumnFlags.WidthFixed, 140);
                    ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Lvl", ImGuiTableColumnFlags.WidthFixed, 32);
                    ImGui.TableSetupColumn("Map", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 48);
                    ImGui.TableSetupColumn("Kornago Gourd", ImGuiTableColumnFlags.WidthFixed, 160);
                    ImGui.TableHeadersRow();

                    ApplySort(filtered);

                    foreach (var beast in filtered)
                    {
                        ImGui.TableNextRow();

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(beast.No.ToString());

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(beast.Name);

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(beast.Source);

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(beast.LocationText);

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(beast.MinLevel.ToString());

                        ImGui.TableNextColumn();
                        if (beast.HasMapLocation)
                        {
                            if (ImGui.SmallButton($"Flag##{beast.No}"))
                                beastMapService.TryOpenOnMap(beast);
                        }
                        else
                        {
                            ImGui.TextDisabled("--");
                        }

                        ImGui.TableNextColumn();
                        ImGui.TextWrapped(beast.Gourd ?? "--");
                    }

                    ImGui.EndTable();
                }
            }
            ImGui.End();
        }

        private static void ApplySort(List<Beast> list)
        {
            var sortSpecs = ImGui.TableGetSortSpecs();
            if (sortSpecs.SpecsCount == 0) return;

            var spec = sortSpecs.Specs;
            var descending = spec.SortDirection == ImGuiSortDirection.Descending;

            Comparison<Beast> comparer = spec.ColumnIndex switch
            {
                0 => (a, b) => a.No.CompareTo(b.No),
                1 => (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase),
                2 => (a, b) => string.Compare(a.Source, b.Source, StringComparison.OrdinalIgnoreCase),
                3 => (a, b) => string.Compare(a.LocationText, b.LocationText, StringComparison.OrdinalIgnoreCase),
                4 => (a, b) => a.MinLevel.CompareTo(b.MinLevel),
                6 => (a, b) => string.Compare(a.Gourd ?? "", b.Gourd ?? "", StringComparison.OrdinalIgnoreCase),
                _ => (a, b) => a.No.CompareTo(b.No),
            };

            list.Sort((a, b) => descending ? -comparer(a, b) : comparer(a, b));
            sortSpecs.SpecsDirty = false;
        }
    }
}
