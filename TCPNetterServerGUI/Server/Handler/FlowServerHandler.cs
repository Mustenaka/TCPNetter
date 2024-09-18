using DotNetty.Buffers;
using DotNetty.Handlers.Flow;
using DotNetty.Transport.Channels;
using System.Text;

namespace TCPNetterServerGUI.Server.Handler;

public class FlowServerHandler : FlowControlHandler
{
    //客户端超时次数
    private const int MAX_OVERTIME = 3;  //超时次数超过该值则注销连接


    public static IChannelHandlerContext Current;
    //服务启动
    public override void ChannelActive(IChannelHandlerContext context)
    {
        Console.WriteLine(@"--- Server is active ---" + context.Channel.RemoteAddress);
        Current = context;
    }
    //服务关闭
    public override void ChannelInactive(IChannelHandlerContext context)
    {
        Console.WriteLine(@$"--- {context.Name} is inactive ---" + context.Channel.RemoteAddress);
    }
    //收到消息
    public override void ChannelRead(IChannelHandlerContext context, object msg)
    {
        var buffer = msg as IByteBuffer;
        if (buffer != null)
        {
            var message = buffer.ToString(Encoding.UTF8);
            if (!string.Equals("heartbeat", message))
            {
                Console.WriteLine(message);
            }
            //服务端收到客户端发送的心跳消息后，回复一条信息
            else
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes("reply");
                context.WriteAndFlushAsync(messageBytes);
            }
        }
    }

    public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

    //客户端长时间没有Write，会触发此事件
    public override void UserEventTriggered(IChannelHandlerContext context, object evt)
    {
        base.UserEventTriggered(context, evt);
    }

    //捕获异常
    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine(@"--- Server is active ---" + exception);
        //context.CloseAsync();
    }

    //客户端连接
    public override void HandlerAdded(IChannelHandlerContext context)
    {
        Console.WriteLine($"Client {context} is Connected!");
        base.HandlerAdded(context);
    }

    //客户端断开
    public override void HandlerRemoved(IChannelHandlerContext context)
    {
        Console.WriteLine($"Client {context} is Disconnected.");
        base.HandlerRemoved(context);
    }
}