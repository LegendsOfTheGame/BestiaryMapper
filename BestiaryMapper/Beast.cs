using System.Text.Json.Serialization;

namespace BestiaryMapper
{
    public class Beast
    {
        [JsonPropertyName("No")]
        public int No { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("Source")]
        public string Source { get; set; } = "";

        [JsonPropertyName("Zone")]
        public string? Zone { get; set; }

        [JsonPropertyName("X")]
        public float? X { get; set; }

        [JsonPropertyName("Y")]
        public float? Y { get; set; }

        [JsonPropertyName("DutyNote")]
        public string? DutyNote { get; set; }

        [JsonPropertyName("MinLevel")]
        public int MinLevel { get; set; }

        [JsonPropertyName("Gourd")]
        public string? Gourd { get; set; }

        public bool HasMapLocation => Zone != null && X.HasValue && Y.HasValue;

        public string LocationText => HasMapLocation
            ? $"{Zone} ({X:0}, {Y:0})"
            : DutyNote ?? Zone ?? "";
    }
}
