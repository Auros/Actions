using System;
using Zenject;
using ChatCore;
using SiraUtil;
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

namespace Actions.Twitch
{
    internal class TwitchSocialPlatform : ISocialPlatform, IInitializable, IDisposable
    {
        private readonly Http _http;
        private readonly Config _config;
        private readonly SiraLog _siraLog;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private string? ClientID { get; set; }
        private string? AuthToken { get; set; }
        public bool Initialized => !(ClientID is null || AuthToken is null || _config.Channel is null);

        private TwitchService _twitchService = null!;
        public event Action<IActionUser>? ChannelActivity;
        private ChatCoreInstance _chatCoreInstance = null!;
        public IReadOnlyList<string> Channels = new List<string>();
        private readonly Dictionary<string, IActionUser> _userCache = new Dictionary<string, IActionUser>();

        public TwitchSocialPlatform(Http http, Config config, SiraLog siraLog)
        {
            _http = http;
            _config = config;
            _siraLog = siraLog;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public event Action<IChatService, IChatMessage>? Messaged;
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

                var list = new List<string>();
                foreach (var channel in _twitchService.Channels.Values)
                    list.Add(channel.Id);
                Channels = list;

                _config.Channel = (string.IsNullOrWhiteSpace(_config.Channel) ? _twitchService.Channels.Values.FirstOrDefault()?.Id : _config.Channel) ?? "None";
                ClientID = validation.ClientID;

                _siraLog.Debug($"Channel: {_config.Channel}");
                _siraLog.Debug($"ClientID: {ClientID}");
            }
            else
            {
                _siraLog.Error(response.Content);
            }
        }

        private async void MessageReceived(IChatService service, IChatMessage message)
        {
            MainThreadInvoker.Invoke(() =>
            {
                Messaged?.Invoke(service, message);
            });
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
                if (login == "[tmi.twitch.tv]") return null;
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
            if (Initialized && _config.Channel != "None")
                _twitchService.SendTextMessage(msg, _config.Channel!);
            return Task.CompletedTask;
        }

        public Task SendCommand(string command)
        {
            if (Initialized && _config.Channel != "None")
                _twitchService.SendCommand(command, _config.Channel!);
            return Task.CompletedTask;
        }
    }
}