using System.Collections.Concurrent;
using TCPNetterServerGUI.Server.Model;

namespace TCPNetterServerGUI.Tools;

/// <summary>
/// 扩展工具类
/// </summary>
public static class Extend
{
    /// <summary>
    /// 通过targetId获取对应的服务模型
    /// </summary>
    /// <param name="clients"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static ServerModel? GetServerModel(this ConcurrentDictionary<string, ServerModel> clients, string id)
    {
        if (clients.TryGetValue(id, out var serverModel))
        {
            return serverModel;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 通过设备名称获取对应的通道ID
    /// </summary>
    /// <param name="clients"></param>
    /// <param name="deviceName"></param>
    /// <returns></returns>
    public static string? GetChannelID(this ConcurrentDictionary<string, ServerModel> clients, string deviceName)
    {
        return (from client in clients where client.Value.DeviceName == deviceName select client.Key).FirstOrDefault();
    }
}