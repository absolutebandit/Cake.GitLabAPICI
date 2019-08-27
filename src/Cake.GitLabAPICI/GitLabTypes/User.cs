namespace Cake.GitLabAPICI.GitLabTypes
{
    using Newtonsoft.Json;

    public class User
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
