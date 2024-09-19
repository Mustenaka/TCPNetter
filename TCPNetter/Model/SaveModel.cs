namespace TCPNetterServerGUI.Server.Model;

public class SaveModel
{
    /// <summary>
    /// 设备名称
    /// </summary>
    public string? DeviceName { get; set; }
    /// <summary>
    /// 数据来源ID(通道ID)，每次都会变化
    /// </summary>
    public string? Id { get; set; }
    /// <summary>
    /// 数据信息
    /// </summary>
    public string? Message { get; set; }
    /// <summary>
    /// 时间戳
    /// </summary>
    public string? Datetime { get; set; }
}