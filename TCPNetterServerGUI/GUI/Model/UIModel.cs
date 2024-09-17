using System.ComponentModel;

namespace TCPNetterServerGUI.GUI.Model;

public class UIModel : INotifyPropertyChanged
{
    private string? _id;

    private string? _deviceName;
    //private bool _isConnected;
    private string? _message;

    /// <summary>
    /// 链接id
    /// </summary>
    public string? Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged(nameof(Id));
        }
    }

    /// <summary>
    /// 名称 设备ID
    /// </summary>
    public string? DeviceName
    {
        get => _deviceName;
        set
        {
            _deviceName = value;
            OnPropertyChanged(nameof(DeviceName));
        }
    }

    ///// <summary>
    ///// 是否链接成功
    ///// </summary>
    //public bool IsConnected
    //{
    //    get => _isConnected;
    //    set
    //    {
    //        _isConnected = value;
    //        OnPropertyChanged(nameof(IsConnected));
    //    }
    //}

    /// <summary>
    /// 消息数据
    /// </summary>
    public string? Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged(nameof(Message));
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}