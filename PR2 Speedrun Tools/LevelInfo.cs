using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR2_Speedrun_Tools
{
    public class SearchLevelsModel
    {
        [JsonProperty("levels")]
        public LevelInfo[] Levels { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }
    }

    public class LevelInfo
    {
        [JsonProperty("level_id")]
        public long LevelId { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("play_count")]
        public long PlayCount { get; set; }

        [JsonProperty("min_level")]
        public long MinLevel { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("live")]
        public long Live { get; set; }

        [JsonProperty("pass")]
        public bool Pass { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_group")]
        public string UserGroup { get; set; }
    }
}
