using System;
using Zenject;
using System.Collections.Generic;
using SiraUtil.Tools;

namespace Actions.Dashboard
{
    internal class PlatformManager : IInitializable, IDisposable
    {
        private readonly SiraLog _siraLog;
        public event Action<IActionUser>? ChannelActivity;
        private readonly List<ISocialPlatform> _platforms;

        /*public PlatformManager([InjectOptional] List<ISocialPlatform> platforms)
        {
            if (platforms is null) _platforms = new List<ISocialPlatform>();
            else _platforms = platforms;
        }*/

        public PlatformManager(SiraLog siraLog, ISocialPlatform platform)
        {
            _siraLog = siraLog;
            _platforms = new List<ISocialPlatform> { platform };
        }

        public void Initialize()
        {
            foreach (var platform in _platforms)
            {
                platform.ChannelActivity += Platform_ChannelActivity;
            }
        }

        private void Platform_ChannelActivity(IActionUser user)
        {
            // Combination Callback
            MainThreadInvoker.Invoke(() =>
            {
                ChannelActivity?.Invoke(user);
            });
        }

        public void Dispose()
        {
            foreach (var platform in _platforms)
            {
                platform.ChannelActivity -= Platform_ChannelActivity;
            }
        }
    }
}