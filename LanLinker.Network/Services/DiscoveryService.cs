using System.Net;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using LanLinker.Core;
using LanLinker.Core.Events;
using LanLinker.Core.Interfaces;
using LanLinker.Core.Models;
using LanLinker.Core.Protos;

namespace LanLinker.Network.Services;

public class DiscoveryService : IDiscoveryService
{
    private readonly Identity _identity;

    private readonly IUdpBroadcastService _udpBroadcastService;

    public DiscoveryService(Identity identity, IUdpBroadcastService udpBroadcastService)
    {
        _identity = identity;
        
        _udpBroadcastService = udpBroadcastService;

        _udpBroadcastService.MessageReceived += (_, args) => HandleReceivedBytes(args.Buffer, args.IpEndPoint);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => StartBroadcastingLoop(cancellationToken), cancellationToken);

        return Task.CompletedTask;
    }

    public event EventHandler<PeerEventArgs>? PeerAnnounced;

    private async Task StartBroadcastingLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] messageBytes = CreateAnnouncementMessage().ToByteArray();

                await _udpBroadcastService.SendAsync(messageBytes);

                await Task.Delay(AppSettings.AnnouncementIntervalTime, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void HandleReceivedBytes(byte[] buffer, IPEndPoint remoteEndPoint)
    {
        try
        {
            NetworkMessage networkMessage = NetworkMessage.Parser.ParseFrom(buffer);

            if (networkMessage.Header.DeviceId == Guid.Empty.ToString() ||
                networkMessage.Header.DeviceId == _identity.DeviceId)
            {
                return;
            }

            if (networkMessage.PayloadCase != NetworkMessage.PayloadOneofCase.PeerAnnouncement)
            {
                return;
            }

            PeerAnnouncementMessage payload = networkMessage.PeerAnnouncement;

            Peer peer = new Peer
            {
                DeviceId = networkMessage.Header.DeviceId,
                DeviceName = payload.DeviceName,
                UserName = payload.UserName,
                IpAddress = remoteEndPoint.Address.ToString(),
                Port = 5000,
                LastSeenAt = DateTime.UtcNow
            };

            PeerAnnounced?.Invoke(this, new PeerEventArgs(peer));
        }
        catch
        {
            // malformed packets are silently dropped
        }
    }

    private NetworkMessage CreateAnnouncementMessage()
    {
        return new NetworkMessage
        {
            Header = new MessageHeader
            {
                MessageId = Guid.NewGuid().ToString(),
                DeviceId = _identity.DeviceId,
                RecipientDeviceId = string.Empty,
                MessageType = MessageType.PeerAnnouncement,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            },
            PeerAnnouncement = new PeerAnnouncementMessage
            {
                DeviceName = _identity.DeviceName,
                UserName = _identity.UserName
            }
        };
    }
}
