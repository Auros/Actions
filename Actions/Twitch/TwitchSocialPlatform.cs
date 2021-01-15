using System;
using Zenject;
using ChatCore;
using System.Linq;
using IPA.Utilities;
using SiraUtil.Tools;
using Newtonsoft.Json;
using System.Threading;
using Actions.Dashboard;
using ChatCore.Interfaces;
using System.Threading.Tasks;
using ChatCore.Services.Twitch;
using System.Collections.Generic;
using SiraUtil;

namespace Actions.Twitch
{
    internal class TwitchSocialPlatform : ISocialPlatform, IInitializable, IDisposable
    {
        private readonly Http _http;
        private readonly SiraLog _siraLog;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private string? Channel { get; set; }
        private string? ClientID { get; set; }
        private string? AuthToken { get; set; }
        public bool Initialized => !(ClientID is null || AuthToken is null || Channel is null);

        private TwitchService _twitchService = null!;
        private ChatCoreInstance _chatCoreInstance = null!;
        public event Action<IActionUser>? ChannelActivity;
        private readonly Dictionary<string, IActionUser> _userCache = new Dictionary<string, IActionUser>();

        public TwitchSocialPlatform(SiraLog siraLog, Http http)
        {
            _http = http;
            _siraLog = siraLog;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Initialize()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            _chatCoreInstance = ChatCoreInstance.Create();
            _twitchService = _chatCoreInstance.RunTwitchServices();
            _twitchService.OnTextMessageReceived += MessageReceived;
            string? authToken = _twitchService.GetProperty<string, TwitchService>("OAuthToken")?.Replace("oauth:", "");
            if (authToken is null)
            {
                return;
            }
            AuthToken = authToken;
            _siraLog.Debug($"Fetching OAuth Validation.");
            HttpResponse response = await _http.GetAsync("https://id.twitch.tv/oauth2/validate", AuthToken!);
            if (response.Successful)
            {
                _siraLog.Debug("Success. Deserializing response.");
                var validation = JsonConvert.DeserializeObject<ValidationResponse>(response.Content!);

                await Utilities.AwaitSleep(2000);
                Channel = _twitchService.Channels.Values.FirstOrDefault()?.Id;
                ClientID = validation.ClientID;

                _siraLog.Debug($"Channel: {Channel}");
                _siraLog.Debug($"ClientID: {ClientID}");
            }
            else
            {
                _siraLog.Error(response.Content);
            }
        }

        private async void MessageReceived(IChatService service, IChatMessage message)
        {
            IActionUser? user = await GetUser(message.Sender.DisplayName);
            if (user is null)
                return;
            ChannelActivity?.Invoke(user);
        }

        public void Dispose()
        {
            _twitchService.OnTextMessageReceived -= MessageReceived;
            _chatCoreInstance.StopTwitchServices();
            _cancellationTokenSource.Cancel();
        }

        public async Task<IActionUser?> GetUser(string login)
        {
            if (_userCache.TryGetValue(login, out IActionUser usr))
            {
                return usr;
            }
            if (Initialized)
            {
                _siraLog.Debug($"Fetching User [{login}]");
                var response = await _http.GetAsync($"https://api.twitch.tv/helix/users?login={login}", AuthToken!, ClientID);
                if (response.Successful)
                {
                    User user = JsonConvert.DeserializeObject<UserResponse>(response.Content!).Users[0];
                    _siraLog.Debug($"Successfully fetched user [{user.DisplayName}].");
                    usr = new TwitchActionUser(this, user);
                    if (!_userCache.ContainsKey(login))
                    {
                        _userCache.Add(login, usr);
                    }
                    return usr;
                }
                return null;
            }
            return null;
        }

        public Task SendMessage(string msg)
        {
            if (Initialized)
                _twitchService.SendTextMessage(msg, Channel!);
            return Task.CompletedTask;
        }

        public void SendCommand(string command)
        {
            if (Initialized)
                _twitchService.SendCommand(command, Channel!);
        }
    }
}