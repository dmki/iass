using System.Text.Json.Serialization;

namespace IAss
{
    public class Config
    {
        [JsonPropertyName("directoryScanInterval")]
        public int DirectoryScanInterval { get; set; } = 30;

        [JsonPropertyName("features")]
        public FeaturesConfig Features { get; set; } = new FeaturesConfig();
    }

    public class FeaturesConfig
    {
        [JsonPropertyName("directoryScan")]
        public bool DirectoryScan { get; set; } = true;

        [JsonPropertyName("mkdir")]
        public bool Mkdir { get; set; } = true;

        [JsonPropertyName("killproc")]
        public bool KillProc { get; set; } = true;

        [JsonPropertyName("unlock")]
        public bool Unlock { get; set; } = true;

        [JsonPropertyName("findText")]
        public bool FindText { get; set; } = true;

        [JsonPropertyName("findFile")]
        public bool FindFile { get; set; } = true;

        [JsonPropertyName("checkFile")]
        public bool CheckFile { get; set; } = true;

        [JsonPropertyName("checkDir")]
        public bool CheckDir { get; set; } = true;

        [JsonPropertyName("dateTime")]
        public bool DateTime { get; set; } = true;
    }
}