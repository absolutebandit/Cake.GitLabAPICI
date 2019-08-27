namespace Cake.GitLabAPICI.Responses
{
    using System;
    using Newtonsoft.Json;
    using GitLabTypes;

    public class TriggerPipelineResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("ref")]
        public string Ref { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("before_sha")]
        public string BeforeSha { get; set; }

        [JsonProperty("tag")]
        public bool HasTag { get; set; }

        [JsonProperty("yaml_errors")]
        public string YamlErrors { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonConverter(typeof(MinDateTimeConverter))]
        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonConverter(typeof(MinDateTimeConverter))]
        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonConverter(typeof(MinDateTimeConverter))]
        [JsonProperty("started_at")]
        public DateTime? StartedAt { get; set; }

        [JsonConverter(typeof(MinDateTimeConverter))]
        [JsonProperty("finished_at")]
        public DateTime? FinishedAt { get; set; }

        [JsonConverter(typeof(MinDateTimeConverter))]
        [JsonProperty("committed_at")]
        public DateTime? CommittedAt { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("coverage")]
        public string Coverage { get; set; }

        [JsonProperty("web_url")]
        public Uri WebUrl { get; set; }
    }
}
