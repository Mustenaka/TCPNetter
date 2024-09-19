using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using TCPNetter.Model;
using TCPNetterServerGUI.Server.Model;

namespace TCPNetter.Client;

public class Client
{
    private TcpClient _client;
    private NetworkStream _stream;
    private StringBuilder _messageBuffer = new StringBuilder(); // 用于存储未解析的消息

    public Client()
    {
        _client = new TcpClient();
    }

    public bool IsServerCloese()
    {
        try
        {
            if (_client == null || !_client.Connected)
            {
                return true;  // 如果 TcpClient 不存在或未连接，返回 true 表示服务器关闭
            }

            // 检查网络流的状态，通过发送一个空字节数组来检测是否断开
            if (_client.Client.Poll(0, SelectMode.SelectRead))
            {
                // 如果有数据可读但返回 0，说明连接已经断开
                byte[] buffer = new byte[1];
                if (_client.Client.Receive(buffer, SocketFlags.Peek) == 0)
                {
                    return true; // 返回 true，表示连接已断开
                }
            }

            return false; // 返回 false，表示服务器还在
        }
        catch (SocketException)
        {
            return true;  // 捕获异常，返回 true 表示连接断开
        }
    }

    public async Task ConnectAsync(string ip, int port)
    {
        await _client.ConnectAsync(ip, port);
        _stream = _client.GetStream();
        Console.WriteLine(@$"Connected to server: {ip}:{port}");
    }

    public async Task SendTemplateMessageAsync()
    {
        // Create a MessageModel instance
        var messageModel = new MessageModel
        {
            Id = "12345",
            MessageType = "Message", // Change as needed: Heartbeats, Echo, Command, Message
            DeviceName = "ClientDevice",
            Message = "Hello from client!"
        };

        // Serialize the MessageModel to JSON using System.Text.Json
        var jsonMessage = JsonSerializer.Serialize(messageModel);

        // Convert the JSON string to bytes and append a newline
        byte[] buffer = Encoding.UTF8.GetBytes(jsonMessage + "\n");

        // Send the message
        await _stream.WriteAsync(buffer, 0, buffer.Length);
        Console.WriteLine(@$"Message sent: {jsonMessage}");
    }

    public async Task SendMessageAsync(MessageModel messageModel)
    {
        if (_stream == null)
        {
            return;
        }

        // Serialize the MessageModel to JSON using System.Text.Json
        var jsonMessage = JsonSerializer.Serialize(messageModel);

        // Convert the JSON string to bytes and append a newline
        byte[] buffer = Encoding.UTF8.GetBytes(jsonMessage + "\n");

        // Send the message
        await _stream.WriteAsync(buffer, 0, buffer.Length);
        Console.WriteLine(@$"Message sent: {jsonMessage}");
    }

    public async Task<object?> ReceiveMessageAsync()
    {
        if (_stream == null)
        {
            return null;
        }

        byte[] buffer = new byte[1024]; // 缓冲区
        int bytesRead;

        while ((bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            // 将收到的数据转换为字符串并添加到消息缓冲区中
            _messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

            // 处理消息，直到找到一个完整的JSON消息（由换行符\n结束）
            while (true)
            {
                string? message = ExtractMessage();
                if (message == null)
                    break;

                try
                {
                    // 先尝试反序列化为单个MessageModel
                    var receivedModel = JsonSerializer.Deserialize<MessageModel>(message);
                    if (receivedModel != null)
                    {
                        Console.WriteLine(
                            @$"Received from server: MessageType={receivedModel.MessageType}, Message={receivedModel.Message}");
                        return receivedModel;
                    }
                }
                catch (JsonException)
                {
                    // 如果反序列化为MessageModel失败，尝试反序列化为List<MessageModel>
                    try
                    {
                        var receivedSaveModelList = JsonSerializer.Deserialize<List<SaveModel>>(message);
                        if (receivedSaveModelList != null)
                        {
                            var isSaveType = true;
                            foreach (var model in receivedSaveModelList)
                            {
                                Console.WriteLine(
                                    @$"Received from server: MessageType={model.DeviceName}, Message={model.Message}");

                                if (string.IsNullOrEmpty(model.Datetime))
                                {
                                    isSaveType = false;
                                    break;
                                }
                            }

                            if (isSaveType)
                            {
                                return receivedSaveModelList;
                            }
                        }

                        var receivedModelList = JsonSerializer.Deserialize<List<MessageModel>>(message);
                        if (receivedModelList != null)
                        {
                            Console.WriteLine(@$"Received a list of {receivedModelList.Count} messages from server.");
                            foreach (var model in receivedModelList)
                            {
                                Console.WriteLine(
                                    @$"Received from server: MessageType={model.MessageType}, Message={model.Message}");
                            }
                            return receivedModelList;
                        }
                    }
                    catch (JsonException ex)
                    {
                        // 如果两种反序列化都失败，捕获异常并打印错误信息
                        Console.WriteLine(@$"Error deserializing response: {ex.Message}");
                        Console.WriteLine(@"Raw response: " + message);
                    }
                }
            }
        }

        return null;
    }

    private string? ExtractMessage()
    {
        // 查找消息结束符（换行符 \n）
        int newlineIndex = _messageBuffer.ToString().IndexOf('\n');
        if (newlineIndex >= 0)
        {
            // 提取完整的消息（去掉换行符）
            string completeMessage = _messageBuffer.ToString(0, newlineIndex).Trim();

            // 从缓冲区移除已经处理过的消息
            _messageBuffer.Remove(0, newlineIndex + 1);

            return completeMessage; // 返回提取出的完整消息
        }

        // 如果没有找到换行符，返回null表示消息不完整
        return null;
    }

    public void Close()
    {
        _stream.Close();
        _client.Close();
    }
}