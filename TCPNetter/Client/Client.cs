using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using TCPNetter.Model;

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
        try
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
        }catch (Exception err)
        {
            Console.WriteLine(err.Message);
        }
    }

    public async Task<MessageModel?> ReceiveMessageAsync()
    {
        try
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
                        var receivedModel = JsonSerializer.Deserialize<MessageModel>(message);
                        if (receivedModel != null)
                        {
                            Console.WriteLine(
                                @$"Received from server: MessageType={receivedModel.MessageType}, Message={receivedModel.Message}");
                        }

                        return receivedModel;
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine(@$"Error deserializing response: {ex.Message}");
                        Console.WriteLine(@"Raw response: " + message);
                    }
                }
            }

            return null;
        }
        catch (Exception err)
        {
            Console.WriteLine(err.Message);
            return null;
        }
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