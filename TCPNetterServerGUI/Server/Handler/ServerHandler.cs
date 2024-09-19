using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;
using DotNetty.Transport.Channels;
using TCPNetterServerGUI.Server.Model;
using TCPNetterServerGUI.Tools;

namespace TCPNetterServerGUI.Server.Handler;

public class NetterServerHandler(MainForm mainForm) : ChannelHandlerAdapter
{
    private static readonly ConcurrentDictionary<string, List<SaveModel>> Historys = new ConcurrentDictionary<string, List<SaveModel>>();
    private static readonly ConcurrentDictionary<string, ServerModel> Clients = new ConcurrentDictionary<string, ServerModel>();
    private static readonly ConcurrentDictionary<string, DateTime> LastHeartbeat = new ConcurrentDictionary<string, DateTime>();

    /// <summary>
    /// 接受消息 | 这里在业务逻辑增长之下需要扩充设计模式
    /// </summary>
    /// <param name="context"></param>
    /// <param name="input"></param>
    public override void ChannelRead(IChannelHandlerContext context, object input)
    {
        var channelId = context.Channel.Id.AsShortText();

        var message = input as MessageModel;

        switch (message.MessageType)
        {
            case "Heartbeats":  // 接受的心跳数据
                Console.WriteLine(@$"-- Heartbeat -- received from {channelId}:{message.DeviceName}");
                LastHeartbeat[channelId] = DateTime.Now; // 更新心跳时间
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
                Console.WriteLine(@$"-- Echo -- message received from {channelId}:{message.DeviceName}");
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
                Console.WriteLine(@$"-- Command -- received:  from {channelId}:{message.DeviceName} | Command:{message.Command} to {message.Target} message: {message.Message}");
                switch (message.Command)
                {
                    case "Broadcast":   // 表示挂广播这个消息给其他受控制端口
                        NetterServerHandler.BroadcastMessage(message.Message).Wait();
                        break;
                    case "SendMessageById":     // 表示定点发送给某一特殊客户端消息(通过通道Id)
                        SendMessageAndWaitResponseAsync(message.Target, message.Message, channelId);
                        break;
                    case "SendMessageByName":   // 表示定点发送给某一特殊客户端消息(通过设备名称)
                        if (!string.IsNullOrEmpty(message.Target))
                        {
                            var targetId = Clients.GetChannelID(message.Target);
                            SendMessageAndWaitResponseAsync(targetId, message.Message, channelId);
                        }
                        break;
                    case "GetAll":  // 获取全部的服务信息
                        NetterServerHandler.GetAllClient(channelId).Wait();
                        break;
                    case "GetMyHistory":  // 根据自身获取历史记录
                        SendHistoryToClient(channelId).Wait();
                        break;
                    case "GetHistory":  // 根据ID获取历史记录
                        SendHistoryToClient(channelId, message.Target).Wait();
                        break;
                }
                break;
            case "Message": // 接受的消息数据
                Console.WriteLine(@$"-- Message -- received: from {channelId}:{message.DeviceName} | Message {message.Message}");
                // 更新UI
                mainForm.UpdateConnectionStatus(channelId!, message.DeviceName!, message.Message!);
                // 更新对应Model
                UpdateModel(channelId, message.DeviceName, message.Message);
                // 添加历史记录
                AddHistory(channelId, message.DeviceName, message.Message);
                // 返回信息 - 不再需要客户端返回
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = message.Id,
                    DeviceName = message.DeviceName,
                    MessageType = "NoCallback",
                    Message = "Message received" + message.Message,
                });
                break;
            case "Callback":    // 用于客户端表示确认收到，需要回应
                Console.WriteLine($@"-- Callback -- received: from {channelId}:{message.DeviceName} | Message {message.Message}");
                // 更新UI
                mainForm.UpdateConnectionStatus(channelId!, message.DeviceName!, message.Message!);
                // 如果这个消息携带目标ID，则需要根据这个目标ID返回这则Callback消息
                if (!string.IsNullOrEmpty(message.Target))
                {
                    SendCallBackMessageToClient(message.Target, message.Message!).Wait();
                }
                break;
            case "NoCallback": // 用于客户端表示确认收到，不需要回应
                Console.WriteLine($@"-- NoCallback -- received: from {channelId}:{message.DeviceName} | Message {message.Message}");
                break;
            default:    // 错误命令
                context.WriteAndFlushAsync(new MessageModel
                {
                    Id = channelId,
                    DeviceName = "",
                    MessageType = "Error",
                    Message = "Unknown Message type"
                });
                break;
        }
    }

    /// <summary>
    /// 消息完成读取
    /// </summary>
    /// <param name="context"></param>
    public override void ChannelReadComplete(IChannelHandlerContext context)
    {
        context.Flush();
        base.ChannelReadComplete(context);
    }

    /// <summary>
    /// 客户端初次链接服务器
    /// </summary>
    /// <param name="context"></param>
    public override void ChannelActive(IChannelHandlerContext context)
    {
        // 为每个客户端分配唯一ID，可以基于连接信息或生成唯一ID
        string clientId = context.Channel.Id.AsShortText(); // 示例：使用通道 ID

        Console.WriteLine(@$"Client connected: {context.Channel.RemoteAddress} : By id {clientId}");

        // 如果客户端已存在，说明是重连
        if (Clients.ContainsKey(clientId))
        {
            Console.WriteLine(@$"Client {clientId} reconnected.");
            mainForm.UpdateConnectionStatus(clientId, "FirstConnect", "Reconnected");
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
            mainForm.AddConnection(clientId, "FirstConnect", "Connected");
        }
    }

    /// <summary>
    /// 客户端退出链接
    /// </summary>
    /// <param name="context"></param>
    public override void ChannelInactive(IChannelHandlerContext context)
    {
        Console.WriteLine(@$"{context.Name} + Client disconnected.");

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
            mainForm.UpdateConnectionStatus(clientIdToRemove, null, "Disconnected - Waiting for Reconnect");

            // 使用一个延时任务处理超时
            Task.Delay(TimeSpan.FromSeconds(30)).ContinueWith(__ =>
            {
                if (Clients.TryGetValue(clientIdToRemove, out var serverModel))
                {
                    var channel = serverModel.Channel;
                    if (!channel.Active) // 检查客户端是否在超时后仍然未重连
                    {
                        Clients.TryRemove(clientIdToRemove, out _);
                        mainForm.RemoveConnection(clientIdToRemove);
                        Console.WriteLine(@$"Client {clientIdToRemove} | {channel.RemoteAddress} | {serverModel.DeviceName} removed after timeout.");
                    }
                }
            });
        }
    }

    /// <summary>
    /// 错误处理
    /// </summary>
    /// <param name="context"></param>
    /// <param name="exception"></param>
    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        // 为每个客户端分配唯一ID，可以基于连接信息或生成唯一ID
        string clientId = context.Channel.Id.AsShortText(); // 示例：使用通道 ID
        Console.WriteLine(@$"Exception: {exception.Message} : By id{clientId}");

        context.CloseAsync();
    }

    #region Functional

    /// <summary>
    /// 获取所有客户端信息
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public static async Task GetAllClient(string? clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            Console.WriteLine(@$"GetAllClient {clientId} is null or empty, fail to response");
            return;
        }

        // Linq获取全部models
        var models = Clients.Values.Select(serverModel => new MessageModel()
        {
            Id = serverModel.Id,
            MessageType = serverModel.MessageType,
            DeviceName = serverModel.DeviceName,
            Message = serverModel.Message,
            Command = serverModel.Command,
            Target = serverModel.Target,
        }).ToList();

        // 获取需要发送的serverModel
        var srvModel = Clients.GetServerModel(clientId);
        if (srvModel == null)
        {
            Console.WriteLine(@$"UpdateModel: Client with ID {clientId} not found.");
            return;
        }

        var channel = srvModel.Channel;
        await channel.WriteAndFlushAsync(models);

        Console.WriteLine(@$"GetAllClient {models.Count}");
    }

    /// <summary>
    /// 定向发送信息并且等待目标回复
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="message"></param>
    /// <param name="myId"></param>
    public async void SendMessageAndWaitResponseAsync(string? clientId, string? message, string? myId)
    {
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(message) || string.IsNullOrEmpty(myId))
        {
            Console.WriteLine(@$"SendMessageAndWaitResponseAsync: ID {clientId} is Empty");
            return;
        }

        // 发送消息
        await SendMessageToClient(clientId, message, myId);

        // 返回结果，注意，返回ID和目标ID需要替换一下
        var srvModel = Clients.GetServerModel(clientId);
        if (srvModel == null)
        {
            Console.WriteLine(@$"SendMessageAndWaitResponseAsync: Client with ID {clientId} not found.");
            return;
        }

        // 构造返回信息
        var channel = srvModel.Channel;
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

        Console.WriteLine(@$"SendMessageAndWaitResponseAsync: Client {myId} send ({message}) to {clientId}");
    }

    /// <summary>
    /// 修改本地存储的模型信息
    /// </summary>
    /// <param name="clientId">自身通道id</param>
    /// <param name="deviceName">设备名称</param>
    /// <param name="message">消息</param>
    /// <returns></returns>
    public Task UpdateModel(string? clientId, string? deviceName, string? message)
    {
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(deviceName) || string.IsNullOrEmpty(message))
        {
            Console.WriteLine(@$"UpdateModel: {deviceName} with {clientId} and {message} is null or empty)");
            return Task.CompletedTask;
        }

        var srvModel = Clients.GetServerModel(clientId);
        if (srvModel == null)
        {
            Console.WriteLine(@$"UpdateModel: Client with ID {clientId} not found.");
            return Task.CompletedTask;
        }

        var channel = srvModel.Channel;
        if (channel.Open)
        {
            srvModel.Id = clientId;
            srvModel.MessageType = "Message";
            srvModel.DeviceName = deviceName;
            srvModel.Message = message;
        }

        Console.WriteLine(@$"UpdateModel: ({clientId}) {deviceName} update {message}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// 发送Callback消息给客户端
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static async Task SendCallBackMessageToClient(string clientId, string message)
    {
        var srvModel = Clients.GetServerModel(clientId);
        if (srvModel == null)
        {
            Console.WriteLine(@$"SendCallBackMessageToClient: Client with ID {clientId} not found.");
            return;
        }

        var channel = srvModel.Channel;
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

        Console.WriteLine(@$"SendCallBackMessageToClient: send ({message}) to {clientId}");
    }

    /// <summary>
    /// 发送消息给客户端
    /// </summary>
    /// <param name="clientId">发起者通道id</param>
    /// <param name="message">信息内容</param>
    /// <param name="fromId">接收者通道id</param>
    /// <returns></returns>
    public static async Task SendMessageToClient(string clientId, string message, string fromId)
    {
        var srvModel = Clients.GetServerModel(clientId);
        if (srvModel == null)
        {
            Console.WriteLine(@$"SendMessageToClient: Client with ID {clientId} not found.");
            return;
        }

        var channel = srvModel.Channel;
        if (channel.Open)
        {
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

        Console.WriteLine(@$"SendMessageToClient: Client {fromId} send ({message}) to {clientId}");
    }

    /// <summary>
    /// 发送历史记录给客户端
    /// </summary>
    /// <param name="clientId">需要发送的客户端id</param>
    /// <returns></returns>
    public async Task SendHistoryToClient(string? clientId)
    {
        // 提前检查
        if (string.IsNullOrEmpty(clientId))
        {
            Console.WriteLine(@$"SendHistoryToClient: {clientId} is null or empty)");
            return;
        }

        var srvModel = Clients.GetServerModel(clientId);
        if (srvModel == null)
        {
            Console.WriteLine(@$"SendHistoryToClient: Client with ID {clientId} not found.");
            return;
        }

        var deviceName = GetDeviceNameByChannelId(clientId);
        var myLogs = GetAllHistoryByDeviceName(deviceName);

        var channel = srvModel.Channel;
        if (channel.Open)
        {
            await channel.WriteAndFlushAsync(myLogs);
        }

        Console.WriteLine(@$"SendMessageToClient: Client {deviceName} send history to {clientId}");
    }

    /// <summary>
    /// 发送历史记录给客户端
    /// </summary>
    /// <param name="clientId">需要发送的客户端id</param>
    /// <param name="deviceName">需要查询的设备名称</param>
    /// <returns></returns>
    public async Task SendHistoryToClient(string? clientId, string? deviceName)
    {
        // 提前检查
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(deviceName))
        {
            Console.WriteLine(@$"SendHistoryToClient: {clientId} is null or empty)");
            return;
        }

        var srvModel = Clients.GetServerModel(clientId);
        if (srvModel == null)
        {
            Console.WriteLine(@$"SendHistoryToClient: Client with ID {clientId} not found.");
            return;
        }

        var myLogs = GetAllHistoryByDeviceName(deviceName);

        var channel = srvModel.Channel;
        if (channel.Open)
        {
            await channel.WriteAndFlushAsync(myLogs);
        }

        Console.WriteLine(@$"SendMessageToClient: Client {deviceName} send history to {clientId}");
    }

    /// <summary>
    /// 广播
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static async Task BroadcastMessage(string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Console.WriteLine(@$"BroadcastMessage: message {message} is empty or null");
            return;
        }

        foreach (var srvModel in Clients.Values)
        {
            var channel = srvModel.Channel;
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

        Console.WriteLine(@$"BroadcastMessage: send {message} to all client({Clients.Count})");
    }

    #endregion

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
                    var clientId = kvp.Key;
                    var lastHeartbeatTime = kvp.Value;

                    if (DateTime.Now - lastHeartbeatTime <= TimeSpan.FromSeconds(30))
                    {
                        continue; // 设定30秒为超时
                    }

                    Console.WriteLine(@$"Client {clientId} is disconnected due to heartbeat timeout.");

                    // 标记为掉线
                    mainForm.UpdateConnectionStatus(clientId, null, "Disconnected - Timeout");

                    if (Clients.TryRemove(clientId, out _))
                    {
                        mainForm.RemoveConnection(clientId);
                    }
                }
            }
        });
    }

    /// <summary>
    /// 添加Message日志
    /// </summary>
    /// <param name="channelId">通道ID</param>
    /// <param name="deviceName">设备名称（这个是唯一主键）</param>
    /// <param name="message">消息</param>
    public void AddHistory(string? channelId, string? deviceName, string? message)
    {
        // 提前检查
        if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(deviceName) || string.IsNullOrEmpty(message))
        {
            Console.WriteLine(@$"AddMessageLog: {deviceName} with {channelId} is null or empty)");
            return;
        }

        // 根据channelId获取对应的serverModel
        var srvModel = Clients.GetServerModel(channelId);
        if (srvModel == null)
        {
            Console.WriteLine(@$"AddMessageLog: Client with ID {channelId} not found.");
            return;
        }

        // 生成日志
        var log = new SaveModel
        {
            DeviceName = deviceName,
            Id = channelId,
            Message = message,
            Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        };

        // 保存日志
        if (Historys.TryGetValue(deviceName, out var logs))
        {
            logs.Add(log);
        }
        else
        {
            Historys.TryAdd(deviceName, new List<SaveModel> { log });
        }

        Console.WriteLine(@$"AddHistory: {deviceName} - {log.Datetime} - {log.Message}");
    }

    /// <summary>
    /// 根据设备名称获取全部历史记录
    /// </summary>
    /// <returns></returns>
    public List<SaveModel> GetAllHistoryByDeviceName(string? deviceName)
    {
        // 提前检查
        if (string.IsNullOrEmpty(deviceName))
        {
            Console.WriteLine(@$"GetAllHistoryByDeviceName: {deviceName} is null or empty)");
            return new List<SaveModel>();
        }

        // 拿到全部logs
        if (Historys.TryGetValue(deviceName, out var logs))
        {
            return logs;
        }

        return new List<SaveModel>();   // 如果没有数据这个就是一个纯空的list
    }

    /// <summary>
    /// 通过通道ID获取设备名称
    /// </summary>
    /// <param name="channelId"></param>
    /// <returns></returns>
    public string? GetDeviceNameByChannelId(string? channelId)
    {
        if (string.IsNullOrEmpty(channelId))
        {
            Console.WriteLine(@$"GetDeviceNameByChannelId: {channelId} is null or empty)");
            return "";
        }

        // 根据channelId获取对应的serverModel
        var srvModel = Clients.GetServerModel(channelId);
        if (srvModel == null)
        {
            Console.WriteLine(@$"AddMessageLog: Client with ID {channelId} not found.");
            return "";
        }

        return srvModel.DeviceName;
    }
}