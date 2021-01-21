using System;
using Zenject;
using Actions.UI;
using Actions.Dashboard;
using ChatCore.Interfaces;

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

        private void Messaged(IChatService service, IChatMessage msg)
        {
            const string cmd = "!actionscreate";
            if ((_config.AllowModsToCreate && msg.Sender.IsModerator) || msg.Sender.IsBroadcaster)
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