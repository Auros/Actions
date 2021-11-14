using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Actions.Dashboard;
using CatCore;
using CatCore.Models.EventArgs;
using CatCore.Models.Twitch;
using CatCore.Models.Twitch.IRC;
using CatCore.Services.Multiplexer;
using CatCore.Services.Twitch.Interfaces;
using SiraUtil.Tools;
using Zenject;

namespace Actions.Twitch
{
    internal class TwitchSocialPlatform : ISocialPlatform, IInitializable, IDisposable
    {
        private readonly Config _config;
        private readonly SiraLog _siraLog;
        
        private CatCoreInstance _chatCoreInstance = null!;
        private ITwitchService _twitchService = null!;
        private ITwitchChannelManagementService _twitchChannelManagementService = null!;
        private ITwitchHelixApiService _twitchHelixApiService = null!;

        private TwitchChannel? _channel;
        private readonly Dictionary<string, IActionUser> _userCache = new Dictionary<string, IActionUser>();

        public event Action<IActionUser>? ChannelActivity;
        public IReadOnlyList<TwitchChannel> Channels = new List<TwitchChannel>();
        public bool Initialized => _twitchService.LoggedIn && _config.ChannelId != null;

        public TwitchSocialPlatform(Config config, SiraLog siraLog)
        {
            _config = config;
            _siraLog = siraLog;
            
            _config.Updated -= OnConfigUpdated;
            _config.Updated += OnConfigUpdated;
        }

        public event Action<MultiplexedPlatformService, MultiplexedMessage>? Messaged;
        public void Initialize()
        {
            _ = InitializeAsync();
        }

        private Task InitializeAsync()
        {
            _chatCoreInstance = CatCoreInstance.Create();
            _twitchService = _chatCoreInstance.RunTwitchServices();
            _twitchHelixApiService = _twitchService.GetHelixApiService();
            _twitchChannelManagementService = _twitchService.GetChannelManagementService();
            
            Channels = _twitchChannelManagementService.GetAllActiveChannels();

            if (_config.ChannelId != null)
            {
                _channel = Channels.FirstOrDefault(twitchChannel => twitchChannel.Id == _config.ChannelId);
            }
            
            if (_channel == null)
            {
                _channel = (TwitchChannel?)_twitchService.DefaultChannel ?? Channels.FirstOrDefault();
                _config.ChannelId = _channel?.Id;
            }

            _siraLog.Debug($"Channel: {_channel?.Name ?? "None"}");

            _twitchService.OnTextMessageReceived += MessageReceived;
            _twitchChannelManagementService.ChannelsUpdated += TwitchChannelManagementServiceOnChannelsUpdated;

            return Task.CompletedTask;
        }

        private async void MessageReceived(ITwitchService service, TwitchMessage message)
        {
            MainThreadInvoker.Invoke(() => Messaged?.Invoke(MultiplexedPlatformService.From<ITwitchService, TwitchChannel,TwitchMessage>(service), MultiplexedMessage.From<TwitchMessage, TwitchChannel>(message)));
            IActionUser? user = await GetUser(message.Sender.UserName);
            if (user is null)
                return;
            ChannelActivity?.Invoke(user);
        }

        public void Dispose()
        {
            _config.Updated -= OnConfigUpdated;
            
            _twitchChannelManagementService.ChannelsUpdated -= TwitchChannelManagementServiceOnChannelsUpdated;
            _twitchService.OnTextMessageReceived -= MessageReceived;
            _chatCoreInstance.StopTwitchServices();
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
                var response = await _twitchHelixApiService.FetchUserInfo(userIds: new[] { login });
                if (response != null)
                {
                    var userData = response.Value.Data[0];
                    _siraLog.Debug($"Successfully fetched user [{userData.DisplayName}].");
                    usr = new TwitchActionUser(this, userData);
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
            {
                _channel?.SendMessage(msg);
            }

            return Task.CompletedTask;
        }

        public Task SendCommand(string command)
        {
            return SendMessage('/' + command);
        }
        
        private void OnConfigUpdated(Config config)
        {
            var channelId = config.ChannelId;
            if (channelId != _channel?.Id)
            {
                _channel = channelId != null
                    ? Channels.FirstOrDefault(twitchChannel => twitchChannel.Id == channelId)
                    : null;

                _siraLog.Debug($"Changed target channel to {_channel?.Name}");
            }
            else
            {
                _siraLog.Debug("No change");
            }
        }
        
        private void TwitchChannelManagementServiceOnChannelsUpdated(object sender, TwitchChannelsUpdatedEventArgs e)
        {
            if (_channel != null && e.DisabledChannels.ContainsKey(_channel.Id))
            {
                _siraLog.Debug($"Our target channel {_channel.Name} got disabled");

                // TODO: Look into how we actually want to handle this behavior later down the line.
                _channel = null;
                _config.ChannelId = null;
            }
        }
    }
}