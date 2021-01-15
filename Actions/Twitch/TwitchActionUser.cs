using Actions.Dashboard;
using System.Threading.Tasks;

namespace Actions.Twitch
{
    internal class TwitchActionUser : IActionUser
    {
        private readonly ISocialPlatform _socialPlatform;

        public string ID { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? ProfilePictureURL { get; set; } = null!;

        public TwitchActionUser(ISocialPlatform socialPlatform, User user)
        {
            ID = user.ID;
            Name = user.DisplayName;
            ProfilePictureURL = user.ProfileImageURL;
            _socialPlatform = socialPlatform;
        }

        public Task Ban(float? length)
        {
            if (_socialPlatform is TwitchSocialPlatform twitchPlatform)
            {
                if (length.HasValue)
                {
                    twitchPlatform.SendCommand($"timeout {Name} {length}");
                }
                else
                {
                    twitchPlatform.SendCommand($"ban {Name}");
                }
            }
            return Task.CompletedTask;
        }
    }
}