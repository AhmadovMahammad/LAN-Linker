using LanLinker.Core.Events;

namespace LanLinker.Core.Interfaces;

public interface IGlobalChatService
{
    Task SendGlobalMessageAsync(string message);

    event EventHandler<GlobalChatMessageEventArgs> GlobalMessageReceived;
}