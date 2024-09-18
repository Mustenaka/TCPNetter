using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Concurrency;
using DotNetty.Handlers.Flow;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using DotNetty.Transport.Channels.Pool;
using Newtonsoft.Json;
using TCPNetterServerGUI.Server.Model;

namespace TCPNetterServerGUI.Server.Handler;

public class NetterServerHandler : ChannelHandlerAdapter
{
    //private static readonly DefaultChannelGroup channelGroup = new DefaultChannelGroup(
    //    new SingleThreadEventExecutor("GroupEventExecutor", TimeSpan.FromSeconds(1))
    //    );

    // 定义一个TaskCompletionSource来等待回传消息
    private TaskCompletionSource<MessageModel> _responseTcs;

    private static readonly ConcurrentDictionary<string, ServerModel> Clients = new ConcurrentDictionary<string, ServerModel>();
    private static readonly ConcurrentDictionary<string, DateTime> LastHeartbeat = new ConcurrentDictionary<string, DateTime>();

    private MainForm _mainForm;

    public NetterServerHandler(MainForm mainForm)
    {
        _mainForm = mainForm;
    }

    //public override void HandlerAdded(IChannelHandlerContext context)
    //{
    //    IChannel channel = context.Channel;
    //    channelGroup.WriteAndFlushAsync(@$"[Server]:{channel.RemoteAddress} join.");
    //    channelGroup.Add(channel);
    //}

    //public override void HandlerRemoved(IChannelHandlerContext context)
    //{
    //    IChannel channel = context.Channel;
    //    channelGroup.WriteAndFlushAsync(@$"[Server]:{channel.RemoteAddress} leave.");
    //}

    public override void ChannelRead(IChannelHandlerContext context, object input)
    {
        var channelID = context.Channel.Id.AsShortText();

        var message = input as MessageModel;

        switch (message.MessageType)
        {
            case "Heartbeats":  // 接受的心跳数据
                Console.WriteLine(@"Heartbeat received from device: " + message.DeviceName);

                //LastHeartbeat[channelID] = DateTime.Now; // 更新心跳时间
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = message.Id,
                    DeviceName = message.DeviceName,
                    MessageType = "Heartbeats",
                    Message = "Heartbeat Ack",
                    Command = "",
                    Target = "",
                });
                break;
            case "Echo":    // 接受的回响数据
                Console.WriteLine(@"Echo message received from device: " + message.DeviceName);
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = message.Id,
                    DeviceName = message.DeviceName,
                    MessageType = message.MessageType,
                    Message = message.Message,
                    Command = message.Command,
                    Target = message.Target,
                });
                break;
            case "Command": // 接受的命令数据
                Console.WriteLine(@"Command received: " + message.Message);
                // TODO: 在这里处理具体的服务器逻辑
                switch (message.Command)
                {
                    case "Broadcast":   // 表示挂广播这个消息给其他受控制端口
                        NetterServerHandler.BroadcastMessage(message.Message);
                        break;
                    case "SendMessageToClient": // 表示定点发送给某一特殊客户端消息
                        SendMessageAndWaitResponseAsync(message.Target, message.Message, channelID);
                        break;
                    case "GetAll":  // 获取全部的服务信息
                        NetterServerHandler.GetAllClient(channelID);
                        break;
                    case "GetMyHistory":  // 根据自身获取历史记录
                        break;
                    case "GetHistory":  // 根据ID获取历史记录
                        break;
                }
                break;
            case "Message": // 接受的消息数据
                Console.WriteLine(@"Message received: " + message.Message);
                // TODO: 在这里处理具体的客户端逻辑
                // 更新UI
                _mainForm.UpdateConnectionStatus(channelID!, message.DeviceName!, message.Message!);
                // 更新对应Model
                UpdateModel(channelID, message.DeviceName, message.Message);
                // 返回信息 - 不再需要客户端返回
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = message.Id,
                    DeviceName = message.DeviceName,
                    MessageType = "NoCallback",
                    Message = "Message received kkk"
                });
                break;
            case "NoCallback":    // 用于客户端表示确认收到，不需要回应
                Console.WriteLine(@"NoCallback:" + message.Message);
                //_responseTcs?.TrySetResult(message);
                // 更新UI
                _mainForm.UpdateConnectionStatus(channelID!, message.DeviceName!, message.Message!);
                if (!string.IsNullOrEmpty(message.Target))
                {
                    SendCallBackMessageToClient(message.Target, message.Message);
                }
                break;
            default:
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = channelID,
                    DeviceName = "",
                    MessageType = "Error",
                    Message = "Unknow Message type"
                });
                break;
        }
    }

    public override void ChannelReadComplete(IChannelHandlerContext context)
    {
        context.Flush();
        base.ChannelReadComplete(context);
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
            var model = new ServerModel
            {
                Channel = context.Channel,
                Id = clientId,
            };
            Clients.TryAdd(clientId, model);
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

        if (clientIdToRemove != null)
        {
            // 在这里不立即移除客户端，而是将其标记为“掉线”并设定一个超时时间
            _mainForm.UpdateConnectionStatus(clientIdToRemove, null, "Disconnected - Waiting for Reconnect");

            // 使用一个延时任务处理超时
            Task.Delay(TimeSpan.FromSeconds(30)).ContinueWith(__ =>
            {
                if (Clients.TryGetValue(clientIdToRemove, out var serverModel))
                {
                    var channel = serverModel.Channel;
                    if (!channel.Active) // 检查客户端是否在超时后仍然未重连
                    {
                        Clients.TryRemove(clientIdToRemove, out _);
                        _mainForm.RemoveConnection(clientIdToRemove);
                        Console.WriteLine(@$"Client {clientIdToRemove} | {channel.RemoteAddress} | {serverModel.DeviceName} removed after timeout.");
                    }
                }
            });
        }
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        // 为每个客户端分配唯一ID，可以基于连接信息或生成唯一ID
        string clientId = context.Channel.Id.AsShortText(); // 示例：使用通道 ID
        Console.WriteLine(@$"Exception: {exception.Message} : By id{clientId}");

        context.CloseAsync();
    }

    public static async Task GetAllClient(string clientId)
    {
        var models = Clients.Values.Select(serverModel => new MessageModel()
        {
            Id = serverModel.Id,
            MessageType = serverModel.MessageType,
            DeviceName = serverModel.DeviceName,
            Message = serverModel.Message,
            Command = serverModel.Command,
            Target = serverModel.Target,
        })
            .ToList();

        if (Clients.TryGetValue(clientId, out var server))
        {
            var channel = server.Channel;

            await channel.WriteAndFlushAsync(models);

            Console.WriteLine(@$"Get all client {models.Count}");
        }
    }

    public async void SendMessageAndWaitResponseAsync(string clientId, string message, string myId)
    {
        // 创建一个新的TaskCompletionSource用于等待客户端响应
        _responseTcs = new TaskCompletionSource<MessageModel>();

        // 发送消息
        await SendMessageToClient(clientId, message, myId);

        //// 等待回传消息，设置超时时间为30秒
        //var response = await Task.WhenAny(_responseTcs.Task, Task.Delay(TimeSpan.FromSeconds(30)));

        // 返回结果，注意，返回ID和目标ID需要替换一下
        if (Clients.TryGetValue(myId, out var serverModel))
        {
            var channel = serverModel.Channel;
            if (channel.Open)
            {
                await channel.WriteAndFlushAsync(new MessageModel
                {
                    Id = myId,
                    DeviceName = "",
                    MessageType = "NoCallback",
                    Message = message,
                    Target = clientId,
                    Command = "SendMessageToClient",
                });
            }
        }
    }

    /// <summary>
    /// 发送消息给客户端
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task UpdateModel(string clientId, string deviceName, string message)
    {
        if (Clients.TryGetValue(clientId, out var serverModel))
        {
            var channel = serverModel.Channel;
            if (channel.Open)
            {
                serverModel.Id = clientId;
                serverModel.MessageType = "Message";
                serverModel.DeviceName = deviceName;
                serverModel.Message = message;
            }
        }
        else
        {
            Console.WriteLine(@$"Client with ID {clientId} not found.");
        }
    }

    /// <summary>
    /// 发送消息给客户端
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static async Task SendCallBackMessageToClient(string clientId, string message)
    {
        if (Clients.TryGetValue(clientId, out var serverModel))
        {
            var channel = serverModel.Channel;
            if (channel.Open)
            {
                var encodedMessage = Encoding.UTF8.GetBytes(message);
                await channel.WriteAndFlushAsync(new MessageModel
                {
                    Id = clientId,
                    DeviceName = "",
                    MessageType = "Callback",
                    Message = message,
                    Target = "",
                    Command = "",
                });
            }
        }
        else
        {
            Console.WriteLine(@$"Client with ID {clientId} not found.");
        }
    }

    /// <summary>
    /// 发送消息给客户端
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static async Task SendMessageToClient(string clientId, string message, string fromId)
    {
        if (Clients.TryGetValue(clientId, out var serverModel))
        {
            var channel = serverModel.Channel;
            if (channel.Open)
            {
                var encodedMessage = Encoding.UTF8.GetBytes(message);
                await channel.WriteAndFlushAsync(new MessageModel
                {
                    Id = clientId,
                    DeviceName = "",
                    MessageType = "Message",
                    Message = message,
                    Target = fromId,
                    Command = "",
                });
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
    public static async Task BroadcastMessage(string message)
    {
        var encodedMessage = Encoding.UTF8.GetBytes(message);

        foreach (var serverModel in Clients.Values)
        {
            var channel = serverModel.Channel;
            if (channel.Open)
            {
                await channel.WriteAndFlushAsync(new MessageModel
                {
                    Id = channel.Id.AsShortText(),
                    DeviceName = "",
                    MessageType = "Message",
                    Message = message,
                    Target = "",
                    Command = "",
                });
            }
        }
    }

    /// <summary>
    /// 心跳包任务：定期向所有活跃的客户端发送心跳包
    /// 打算直接用框架内的了，直接隐式心跳就不会重叠冲突
    /// </summary>
    public void StartHeartbeatMonitor()
    {
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
                        Console.WriteLine(@$"Client {clientId} is disconnected due to heartbeat timeout.");
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