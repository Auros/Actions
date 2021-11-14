using System;
using Actions.Dashboard;
using Actions.UI;
using CatCore.Services.Multiplexer;
using Zenject;

namespace Actions.Managers
{
    internal class CommandCreator : IInitializable, IDisposable
    {
        private readonly Config _config;
        private readonly ActionMacroView _macroView;
        private readonly ISocialPlatform _socialPlatform;

        public CommandCreator(Config config, ActionMacroView macroView, ISocialPlatform socialPlatform)
        {
            _config = config;
            _macroView = macroView;
            _socialPlatform = socialPlatform;
        }

        public void Initialize()
        {
            _socialPlatform.Messaged += Messaged;
        }

        private void Messaged(MultiplexedPlatformService service, MultiplexedMessage msg)
        {
            const string cmd = "!bsan";
            if (string.Equals(_config.ChannelId, msg.Channel.Id, StringComparison.CurrentCultureIgnoreCase) && (msg.Sender.IsBroadcaster || (_config.AllowModsToCreate && msg.Sender.IsModerator)))
            {
                if (msg.Message.StartsWith(cmd))
                {
                    var msgParts = msg.Message.Split(' ');
                    if (msgParts.Length < 3)
                        return;

                    var name = msgParts[1];
                    var content = msg.Message.Substring(cmd.Length + 1 + name.Length + 1);
                    var macro = new Macro
                    {
                        Name = name,
                        Content = content,
                        IsCommand = content.StartsWith("/")
                    };
                    _config.Macros.Add(macro);
                    _macroView.AddMacro(macro);
                }
            }
        }

        public void Dispose()
        {
            _socialPlatform.Messaged -= Messaged;
        }
    }
}