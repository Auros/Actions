using System;
using System.Threading.Tasks;
using CatCore.Services.Multiplexer;

namespace Actions.Dashboard
{
    public interface ISocialPlatform
    {
        bool Initialized { get; }
        Task SendMessage(string msg);
        Task SendCommand(string cmd);
        Task<IActionUser?> GetUser(string id);

        event Action<IActionUser>? ChannelActivity;
        event Action<MultiplexedPlatformService, MultiplexedMessage>? Messaged;
    }
}