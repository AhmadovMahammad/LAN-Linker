// See https://aka.ms/new-console-template for more information

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using LanLinker.Core.Protos;

namespace LanLinker.Core.Test;

internal abstract class Program
{
    private const string MetadataTyping = "Typing";
    
    public static void Main(string[] args)
    {
        string deviceId = Guid.NewGuid().ToString();

        NetworkMessage networkMessage = new NetworkMessage()
        {
            Header = new MessageHeader
            {
                MessageId = Guid.NewGuid().ToString(),
                DeviceId = deviceId,
                RecipientDeviceId = string.Empty,
                MessageType = MessageType.UserActivity,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            },
            UserActivity = new UserActivityMessage
            {
                UserMessage = "Hello World!",
                Metadata =
                {
                    { MetadataTyping, "True" }
                }
            }
        };

        using MemoryStream memoryStream = new MemoryStream();

        networkMessage.WriteDelimitedTo(memoryStream);

        byte[] bufferData = memoryStream.ToArray();

        using MemoryStream readStream = new MemoryStream(bufferData);

        NetworkMessage? receivedMessage = NetworkMessage.Parser.ParseDelimitedFrom(readStream);

        if (receivedMessage == null)
        {
            Console.WriteLine("Could not parse network message.");
            return;
        }

        Console.WriteLine($"Header ID: {receivedMessage.Header.MessageId}");

        Console.WriteLine($"Sender ID: {receivedMessage.Header.DeviceId}");

        Console.WriteLine($"Message: {receivedMessage.UserActivity.UserMessage}");

        Console.WriteLine($"Metadata: {receivedMessage.UserActivity.Metadata[MetadataTyping]}");

        DateTime dt = receivedMessage.Header.Timestamp.ToDateTime();

        Console.WriteLine($"Time: {dt.ToLocalTime()}");
    }
}