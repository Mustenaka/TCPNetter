namespace TCPNetterServerGUI.Server.Model;

public class MessageModel
{
    /// <summary>
    /// 数据来源ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 数据类型，有
    /// - Heartbeats 心跳包
    /// - Echo 回响包
    /// - Command 指令包
    /// - Message 消息包
    /// </summary>
    public string? MessageType { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// 数据信息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 命令内容，用于处理Commad指令。结合 MessageType 为 Command 时
    /// </summary>
    public string? Command { get; set; }

    /// <summary>
    /// 用于指向服务。结合 MessageType 为 Command 时
    /// </summary>
    public string? Target { get; set; }
}