using Newtonsoft.Json;

namespace Actions.Twitch
{
    internal struct User
    {
        public string ID { get; set; }
        public string Login { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("profile_image_url")]
        public string? ProfileImageURL { get; set; }
    }
}