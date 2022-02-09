using System.Threading.Tasks;
using Actions.Dashboard;
using CatCore.Models.Twitch.Helix.Responses;

namespace Actions.Twitch
{
    internal class TwitchActionUser : IActionUser
    {
        private readonly ISocialPlatform _socialPlatform;

        public string ID { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? ProfilePictureURL { get; set; } = null!;

        public TwitchActionUser(ISocialPlatform socialPlatform, UserData user)
        {
            ID = user.UserId;
            Name = user.DisplayName;
            ProfilePictureURL = user.ProfileImageUrl;
            _socialPlatform = socialPlatform;
        }

        public Task Ban(float? length)
        {
            if (_socialPlatform is TwitchSocialPlatform twitchPlatform)
            {
                twitchPlatform.SendCommand(length.HasValue ? $"timeout {Name} {length}" : $"ban {Name}");
            }
            return Task.CompletedTask;
        }
    }
}