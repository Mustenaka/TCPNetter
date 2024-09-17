using System.Net.Sockets;
using System.Text;

namespace TCPNetter.Test;

public class TestConnect
{
    public async void TestConnectToServer()
    {
        using (TcpClient client = new TcpClient())
        {
            // 连接到DotNetty服务器
            await client.ConnectAsync("127.0.0.1", 8007);
            Console.WriteLine("Connected to server.");

            using (NetworkStream stream = client.GetStream())
            {
                // 发送消息
                string message = "Hello, DotNetty!";
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
                Console.WriteLine($"Sent: {message}");

                // 接收回显消息
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {response}");
            }
        }
    }
}