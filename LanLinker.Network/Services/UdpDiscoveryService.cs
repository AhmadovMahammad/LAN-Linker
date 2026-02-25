using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using LanLinker.Core;
using LanLinker.Core.Interfaces;
using LanLinker.Core.Models;
using LanLinker.Core.Protos;

namespace LanLinker.Network.Services;

public class UdpDiscoveryService(string deviceId, string deviceName, string userName, int port = 5000)
    : IUdpDiscoveryService
{
    private readonly IPEndPoint _broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, port);
    private readonly IPEndPoint _listerEndpoint = new IPEndPoint(IPAddress.Any, port);
    private UdpClient? _udpBroadcaster;
    private UdpClient? _udpListener;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _udpListener = new UdpClient();
        _udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _udpListener.Client.Bind(_listerEndpoint);

        _udpBroadcaster = new UdpClient();
        _udpBroadcaster.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

        _ = Task.Run(() => StartListeningLoop(cancellationToken), cancellationToken);
        _ = Task.Run(() => StartBroadcastingLoop(cancellationToken), cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _udpListener?.Dispose();
        _udpBroadcaster?.Dispose();

        OnPeerDisconnected?.Invoke(Guid.Empty);

        return Task.CompletedTask;
    }

    public event Action<Peer>? OnPeerConnected;
    public event Action<Guid>? OnPeerDisconnected;
    public event Action<Exception>? OnCriticalError;

    private async Task StartBroadcastingLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                NetworkMessage announcementMessage = CreateAnnouncementMessage();

                byte[] messageBytes = announcementMessage.ToByteArray();

                if (_udpBroadcaster != null)
                {
                    await _udpBroadcaster.SendAsync(messageBytes, _broadcastEndpoint, cancellationToken);
                    Console.WriteLine($"[{DateTime.UtcNow}] announcement message sent.");
                }

                await Task.Delay(AppSettings.AnnouncementIntervalTime, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Console.WriteLine($"[UDP Broadcaster Error] {e.Message}");
            OnCriticalError?.Invoke(e);
        }
    }

    private async Task StartListeningLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_udpListener == null)
                {
                    continue;
                }

                UdpReceiveResult udpReceiveResult = await _udpListener.ReceiveAsync(cancellationToken);

                HandleReceivedBytes(udpReceiveResult.Buffer, udpReceiveResult.RemoteEndPoint);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Console.WriteLine($"[UDP Listener Error] {e.Message}");
            OnCriticalError?.Invoke(e);
        }
    }

    private void HandleReceivedBytes(byte[] buffer, IPEndPoint remoteEndPoint)
    {
        try
        {
            NetworkMessage networkMessage = NetworkMessage.Parser.ParseFrom(buffer);

            if (networkMessage.Header.DeviceId == Guid.Empty.ToString() || networkMessage.Header.DeviceId == deviceId)
            {
                return;
            }

            if (networkMessage.PayloadCase == NetworkMessage.PayloadOneofCase.PeerAnnouncement)
            {
                PeerAnnouncementMessage? payload = networkMessage.PeerAnnouncement;

                Peer peer = new Peer
                {
                    DeviceId = networkMessage.Header.DeviceId,
                    DeviceName = payload.DeviceName,
                    UserName = payload.UserName,
                    IpAddress = remoteEndPoint.Address.ToString(),
                    Port = port,
                    LastSeenAt = DateTime.UtcNow
                };

                OnPeerConnected?.Invoke(peer);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[UDP Parse Error] Bad packet received: {e.Message}");
        }
    }

    private NetworkMessage CreateAnnouncementMessage()
    {
        return new NetworkMessage
        {
            Header = new MessageHeader
            {
                MessageId = Guid.NewGuid().ToString(),
                DeviceId = deviceId,
                RecipientDeviceId = string.Empty,
                MessageType = MessageType.PeerAnnouncement,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            },
            PeerAnnouncement = new PeerAnnouncementMessage
            {
                DeviceName = deviceName,
                UserName = userName,
            }
        };
    }
}