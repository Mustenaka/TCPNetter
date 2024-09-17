using System.Collections.Concurrent;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Newtonsoft.Json;
using TCPNetterServerGUI.Server.Model;

namespace TCPNetterServerGUI.Server.Handler;

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

public class NetterServerHandler : SimpleChannelInboundHandler<MessageModel>
{
    private static readonly ConcurrentDictionary<string, IChannel> Clients = new ConcurrentDictionary<string, IChannel>();
    private static readonly ConcurrentDictionary<string, DateTime> LastHeartbeat = new ConcurrentDictionary<string, DateTime>();

    private readonly CancellationTokenSource _heartbeatCancellationToken = new CancellationTokenSource();

    private MainForm _mainForm;

    public NetterServerHandler(MainForm mainForm)
    {
        _mainForm = mainForm;
    }

    protected override void ChannelRead0(IChannelHandlerContext context, MessageModel message)
    {
        var channelID = context.Channel.Id.AsShortText();
        switch (message.MessageType)
        {
            case "Heartbeats":
                Console.WriteLine("Heartbeat received from device: " + message.DeviceName);

                LastHeartbeat[channelID] = DateTime.Now; // 更新心跳时间
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = message.Id,
                    DeviceName = message.DeviceName,
                    MessageType = "Heartbeats",
                    Message = "Heartbeat Ack"
                });
                break;
            case "Echo":
                Console.WriteLine("Echo message received from device: " + message.DeviceName);
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = message.Id,
                    DeviceName = message.DeviceName,
                    MessageType = "Echo",
                    Message = message.Message
                });
                break;
            case "Command":
                Console.WriteLine("Command received: " + message.Message);
                //context.WriteAndFlushAsync(new MessageModel
                //{
                //    Id = message.Id,
                //    DeviceName = message.DeviceName,
                //    MessageType = "Command",
                //    Message = "Command executed"
                //});
                // TODO: 在这里处理具体的服务器逻辑
                if (message.Message == "Send")
                {

                }
                break;
            case "Message":
                Console.WriteLine("Message received: " + message.Message);
                // TODO: 在这里处理具体的客户端逻辑
                // 更新UI
                _mainForm.UpdateConnectionStatus(channelID!, message.DeviceName!, message.Message!);
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = message.Id,
                    DeviceName = message.DeviceName,
                    MessageType = "Message",
                    Message = "Message received kkk"
                });
                break;
            default:
                Console.WriteLine("Unknown message type");
                break;
        }
    }

    public override void ChannelActive(IChannelHandlerContext context)
    {
        // 为每个客户端分配唯一ID，可以基于连接信息或生成唯一ID
        string clientId = context.Channel.Id.AsShortText(); // 示例：使用通道 ID

        Console.WriteLine(@$"Client connected: {context.Channel.RemoteAddress} : By id {clientId}");

        // 如果客户端已存在，说明是重连
        if (Clients.ContainsKey(clientId))
        {
            Console.WriteLine(@$"Client {clientId} reconnected.");
            _mainForm.UpdateConnectionStatus(clientId, "FirstConnect", "Reconnected");
        }
        else
        {
            // 新连接，加入客户端列表
            Clients.TryAdd(clientId, context.Channel);
            _mainForm.AddConnection(clientId, "FirstConnect", "Connected");
        }
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        Console.WriteLine(@$"{context.Name} + Client disconnected.");

        // Handle disconnection and cleanup if needed
        // 移除客户端
        string clientIdToRemove = null;
        foreach (var kvp in Clients)
        {
            if (kvp.Value == context.Channel)
            {
                clientIdToRemove = kvp.Key;
                break;
            }
        }

        //if (clientIdToRemove != null)
        //{
        //    Clients.TryRemove(clientIdToRemove, out _);
        //}

        //// 更新UI，删除链接信息
        //_mainForm.RemoveConnection(clientIdToRemove!);

        if (clientIdToRemove != null)
        {
            // 在这里不立即移除客户端，而是将其标记为“掉线”并设定一个超时时间
            _mainForm.UpdateConnectionStatus(clientIdToRemove, null, "Disconnected - Waiting for Reconnect");

            // 使用一个延时任务处理超时
            Task.Delay(TimeSpan.FromSeconds(30)).ContinueWith(__ =>
            {
                if (Clients.TryGetValue(clientIdToRemove, out var channel))
                {
                    if (!channel.Active) // 检查客户端是否在超时后仍然未重连
                    {
                        Clients.TryRemove(clientIdToRemove, out _);
                        _mainForm.RemoveConnection(clientIdToRemove);
                        Console.WriteLine(@$"Client {clientIdToRemove} removed after timeout.");
                    }
                }
            });
        }
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        // 为每个客户端分配唯一ID，可以基于连接信息或生成唯一ID
        string clientId = context.Channel.Id.AsShortText(); // 示例：使用通道 ID

        Console.WriteLine($"Exception: {exception.Message} : By id{clientId}");
        //context.CloseAsync();
    }

    /// <summary>
    /// 发送消息给客户端
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task SendMessageToClient(string clientId, string message)
    {
        if (Clients.TryGetValue(clientId, out var channel))
        {
            if (channel.Open)
            {
                var encodedMessage = Encoding.UTF8.GetBytes(message);
                await channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(encodedMessage));
            }
        }
        else
        {
            Console.WriteLine(@$"Client with ID {clientId} not found.");
        }
    }

    /// <summary>
    /// 广播
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task BroadcastMessage(string message)
    {
        var encodedMessage = Encoding.UTF8.GetBytes(message);
        foreach (var channel in Clients.Values)
        {
            if (channel.Open)
            {
                await channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(encodedMessage));
            }
        }
    }

    /// <summary>
    /// 心跳包任务：定期向所有活跃的客户端发送心跳包
    /// </summary>
    public void StartHeartbeatMonitor()
    {
        //Task.Run(async () =>
        //{
        //    while (!_heartbeatCancellationToken.Token.IsCancellationRequested)
        //    {
        //        await Task.Delay(5000); // 每5秒发送一次心跳包

        //        foreach (var client in Clients)
        //        {
        //            if (client.Value.Open) // 检查通道是否活跃
        //            {
        //                var heartbeatMessage = new MessageModel
        //                {
        //                    MessageType = "Heartbeats",
        //                    Message = "Heartbeat from server"
        //                };

        //                await client.Value.WriteAndFlushAsync(heartbeatMessage);
        //            }
        //            else
        //            {
        //                // 如果客户端不再活跃，移除它
        //                Clients.TryRemove(client.Key, out _);
        //            }
        //        }
        //    }
        //}, _heartbeatCancellationToken.Token);

        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(5000); // 每5秒检查一次

                foreach (var kvp in LastHeartbeat)
                {
                    string clientId = kvp.Key;
                    DateTime lastHeartbeatTime = kvp.Value;

                    if (DateTime.Now - lastHeartbeatTime > TimeSpan.FromSeconds(30)) // 设定30秒为超时
                    {
                        Console.WriteLine($"Client {clientId} is disconnected due to heartbeat timeout.");
                        // 标记为掉线
                        _mainForm.UpdateConnectionStatus(clientId, null, "Disconnected - Timeout");

                        if (Clients.TryRemove(clientId, out _))
                        {
                            _mainForm.RemoveConnection(clientId);
                        }
                    }
                }
            }
        });

    }
}