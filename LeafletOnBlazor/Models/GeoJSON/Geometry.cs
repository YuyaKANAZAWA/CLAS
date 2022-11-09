using System.Text.Json.Serialization;

namespace LeafletOnBlazor
{
    public class Geometry
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("coordinates")]
        public object[][][] Coordinates { get; set; }
    }
}