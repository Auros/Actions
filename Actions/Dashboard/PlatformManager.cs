using System;
using Zenject;
using SiraUtil.Logging;
using System.Collections.Generic;

namespace Actions.Dashboard
{
    internal class PlatformManager : IInitializable, IDisposable
    {
        private readonly Config _config;
        private readonly List<ISocialPlatform> _platforms;
        public event Action<IActionUser>? ChannelActivity;

        /*public PlatformManager([InjectOptional] List<ISocialPlatform> platforms)
        {
            if (platforms is null) _platforms = new List<ISocialPlatform>();
            else _platforms = platforms;
        }*/

        public PlatformManager(Config config, SiraLog siraLog, ISocialPlatform platform)
        {
            _config = config;
            _platforms = new List<ISocialPlatform> { platform };
        }

        public void Initialize()
        {
            foreach (var platform in _platforms)
                platform.ChannelActivity += Platform_ChannelActivity;
        }

        private void Platform_ChannelActivity(IActionUser user)
        {
            // Combination Callback
            MainThreadInvoker.Invoke(() =>
            {
                ChannelActivity?.Invoke(user);
            });
        }

        public void SendMessage(string msg)
        {
            foreach (var platform in _platforms)
                platform.SendMessage((_config.PrefixForTTS ? "! " : "") + msg);
        }

        public void SendCommand(string cmd)
        {
            foreach (var platform in _platforms)
                platform.SendCommand(cmd.TrimStart('/'));
        }

        public void Dispose()
        {
            foreach (var platform in _platforms)
                platform.ChannelActivity -= Platform_ChannelActivity;
        }
    }
}