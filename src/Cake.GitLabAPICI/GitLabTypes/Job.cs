namespace Cake.GitLabAPICI.GitLabTypes
{
    using Newtonsoft.Json;

    public class Job
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("stage")]
        public string Stage { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("commit")]
        public Commit Commit { get; set; }

        [JsonProperty("pipeline")]
        public Pipeline Pipeline { get; set; }
    }
}
