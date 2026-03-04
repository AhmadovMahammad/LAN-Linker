using LanLinker.Core.Protos;

namespace LanLinker.Core.Events;

public class GlobalChatMessageEventArgs(string senderDeviceId, UserActivityMessage message) : EventArgs
{
    public string SenderDeviceId { get; } = senderDeviceId;
    public UserActivityMessage Message { get; } = message;
}