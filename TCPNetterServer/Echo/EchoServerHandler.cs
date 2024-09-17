using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System.Text;

namespace TCPNetterServer.Echo;

public class EchoServerHandler : ChannelHandlerAdapter
{
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        var byteBuffer = message as IByteBuffer;
        if (byteBuffer != null)
        {
            string received = byteBuffer.ToString(Encoding.UTF8);
            Console.WriteLine($"Received from client: {received}");

            // Echo back the message to the client
            var responseBytes = Encoding.UTF8.GetBytes("Echo: " + received);
            var responseBuffer = Unpooled.WrappedBuffer(responseBytes);
            context.WriteAndFlushAsync(responseBuffer);
        }
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine($"Exception: {exception.Message}");
        context.CloseAsync();
    }
}