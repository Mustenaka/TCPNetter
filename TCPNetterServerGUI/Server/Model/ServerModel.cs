using DotNetty.Transport.Channels;

namespace TCPNetterServerGUI.Server.Model;

public class ServerModel : MessageModel
{
    /// <summary>
    /// 通道信息
    /// </summary>
    public IChannel Channel { get; set; }
}