using LanLinker.Core.Events;
using LanLinker.Core.Protos;

namespace LanLinker.Core.Interfaces;

public interface IGlobalChatService
{
    Task SendGlobalMessageAsync(NetworkMessage netMessage);

    event EventHandler<GlobalChatMessageEventArgs> GlobalMessageReceived;
}