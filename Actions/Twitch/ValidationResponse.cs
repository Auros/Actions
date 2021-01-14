using Newtonsoft.Json;

namespace Actions.Twitch
{
    internal struct ValidationResponse
    {
        public string[] Scopes { get; set; }

        [JsonProperty("client_id")]
        public string ClientID { get; set; }

        [JsonProperty("user_id")]
        public string UserID { get; set; }

        public string Login { get; set; }
    }
}