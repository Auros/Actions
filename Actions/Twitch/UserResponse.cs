using Newtonsoft.Json;

namespace Actions.Twitch
{
    internal struct UserResponse
    {
        [JsonProperty("data")]
        internal User[] Users { get; set; }
    }
}