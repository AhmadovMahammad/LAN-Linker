using Google.Protobuf;
using LanLinker.Core.Events;
using LanLinker.Core.Interfaces;
using LanLinker.Core.Models;
using LanLinker.Core.Protos;

namespace LanLinker.Network.Services;

public class GlobalChatService : IGlobalChatService
{
    private readonly Identity _identity;

    private readonly IUdpBroadcastService _udpBroadcastService;

    public GlobalChatService(Identity identity, IUdpBroadcastService udpBroadcastService)
    {
        _identity = identity;

        _udpBroadcastService = udpBroadcastService;

        _udpBroadcastService.MessageReceived += (_, args) => HandleReceivedBytes(args.Buffer);
    }

    public async Task SendGlobalMessageAsync(NetworkMessage netMessage)
    {
        await _udpBroadcastService.SendAsync(netMessage.ToByteArray());
    }

    public event EventHandler<GlobalChatMessageEventArgs>? GlobalMessageReceived;

    private void HandleReceivedBytes(byte[] buffer)
    {
        try
        {
            NetworkMessage networkMessage = NetworkMessage.Parser.ParseFrom(buffer);

            if (networkMessage.Header.DeviceId == Guid.Empty.ToString() ||
                networkMessage.Header.DeviceId == _identity.DeviceId)
            {
                return;
            }

            if (networkMessage.PayloadCase != NetworkMessage.PayloadOneofCase.UserActivity)
            {
                return;
            }

            GlobalMessageReceived?.Invoke
            (
                this,
                new GlobalChatMessageEventArgs
                (
                    networkMessage.Header.DeviceId, networkMessage.UserActivity
                )
            );
        }
        catch
        {
            // malformed packets are silently dropped
        }
    }
}