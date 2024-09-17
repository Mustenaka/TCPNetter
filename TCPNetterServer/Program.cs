using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using TCPNetterServer.Message;

namespace TCPNetterServer
{
    internal class Program
    {
        private static async Task RunServerAsync()
        {
            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        var pipeline = channel.Pipeline;
                        pipeline.AddLast(new MessageDecoder());
                        pipeline.AddLast(new MessageEncoder());
                        pipeline.AddLast(new EchoServerHandler());

                        EchoServerHandler.StartHeartbeatMonitor(channel); // Start heartbeat monitoring
                    }));

                var channel = await bootstrap.BindAsync(8007);
                Console.WriteLine("Server started. Listening on port 8007.");
                await channel.CloseCompletion;
            }
            finally
            {
                await Task.WhenAll(
                    bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
                );
            }
        }

        private static void Main(string[] args)
        {
            RunServerAsync().Wait();
        }
    }
}