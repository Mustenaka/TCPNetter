//using System.Text;

//namespace TCPNetterServerGUI.Server.Handler;

///// <summary> Netty消息 </summary>
//public class NettyMessage
//{
//    /// <summary> 消息头 </summary>
//    public NettyHeader Header { get; init; } = default!;

//    /// <summary> 消息体(可空,可根据具体业务而定) </summary>
//    public byte[]? Body { get; init; }

//    /// <summary> 消息头转为字节数组 </summary>
//    public byte[] GetHeaderBytes()
//    {
//        var headerString = Header.ToString();
//        return Encoding.UTF8.GetBytes(headerString);
//    }

//    /// <summary> 是否同步消息 </summary>
//    public bool IsSync() => Header.Sync;

//    /// <summary> 创建Netty消息工厂方法 </summary>
//    public static NettyMessage Create(string endpoint, bool sync = false, byte[]? body = null)
//    {
//        return new NettyMessage
//        {
//            Header = new NettyHeader { EndPoint = endpoint, Sync = sync },
//            Body = body
//        };
//    }

//    /// <summary> 序列化为JSON字符串 </summary>
//    public override string ToString() => Header.ToString();
//}

///// <summary> Netty消息头 </summary>
//public class NettyHeader
//{
//    /// <summary> 请求消息唯一标识 </summary>
//    public Guid RequestId { get; init; } = Guid.NewGuid();

//    /// <summary> 是否同步消息, 默认false是异步消息 </summary>
//    public bool Sync { get; init; }

//    /// <summary> 终结点 (借鉴MVC,约定为Control/Action模式) </summary>
//    public string EndPoint { get; init; } = string.Empty;

//    /// <summary> 序列化为JSON字符串 </summary>
//    public override string ToString() => this.ToJsonString();
//}