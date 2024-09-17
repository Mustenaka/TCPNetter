using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using System.Collections.Concurrent;
using System.Text;
using TCPNetterServer.Model;
using Newtonsoft.Json;

namespace TCPNetterServer.Message;

public class MessageDecoder : ByteToMessageDecoder
{
    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, System.Collections.Generic.List<object> output)
    {
        // Assuming messages are JSON strings terminated by newline
        //int index = input.IndexOf(0, input.ReaderIndex, input.ReadableBytes); // Find newline
        var index = input.IndexOf(input.ReaderIndex, input.WriterIndex, (byte)'\n');
        if (index < 0)
            return; // Not enough data yet

        var length = index - input.ReaderIndex;
        var jsonMessage = input.ReadSlice(length).ToString(Encoding.UTF8);
        input.SkipBytes(1); // Skip newline character

        try
        {
            var message = JsonConvert.DeserializeObject<MessageModel>(jsonMessage);
            if (message != null)
            {
                output.Add(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing message: {ex.Message}");
        }
    }
}

public class MessageEncoder : MessageToByteEncoder<MessageModel>
{
    protected override void Encode(IChannelHandlerContext context, MessageModel message, IByteBuffer output)
    {
        var jsonMessage = JsonConvert.SerializeObject(message);
        var jsonBytes = Encoding.UTF8.GetBytes(jsonMessage + "\n");
        output.WriteBytes(jsonBytes);
    }
}

public class EchoServerHandler : SimpleChannelInboundHandler<MessageModel>
{
    private static readonly ConcurrentDictionary<IChannel, CancellationTokenSource> _channelTokens = new();

    protected override void ChannelRead0(IChannelHandlerContext context, MessageModel message)
    {
        switch (message.MessageType)
        {
            case "Heartbeats":
                Console.WriteLine("Heartbeat received from device: " + message.DeviceName);
                break;
            case "Echo":
                Console.WriteLine("Echo message received from device: " + message.DeviceName);
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = message.Id,
                    DeviceName = message.DeviceName,
                    MessageType = "Echo",
                    Message = "Echo: " + message.Message
                });
                break;
            case "Command":
                Console.WriteLine("Command received: " + message.Message);
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = message.Id,
                    DeviceName = message.DeviceName,
                    MessageType = "Command",
                    Message = "Command executed"
                });
                break;
            case "Message":
                Console.WriteLine("Message received: " + message.Message);
                // 回传
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = message.Id,
                    DeviceName = message.DeviceName,
                    MessageType = "Message",
                    Message = "Message received"
                });
                break;
            default:
                Console.WriteLine("Unknown message type");
                break;
        }
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        Console.WriteLine(@$"{context.Name} + Client disconnected.");
        // Handle disconnection and cleanup if needed
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine($"Exception: {exception.Message}");
        context.CloseAsync();
    }

    public static void StartHeartbeatMonitor(IChannel channel)
    {
        var cts = new CancellationTokenSource();
        _channelTokens[channel] = cts;

        Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                // 心跳包，每1秒发送一次
                await Task.Delay(1000); // Send heartbeat every 5 seconds
                if (channel.Active)
                {
                    await channel.WriteAndFlushAsync(new MessageModel
                    {
                        MessageType = "Heartbeats"
                    });
                }
            }
        });
    }
}