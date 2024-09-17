using System.ComponentModel;

namespace TCPNetter.Model;

public class UIModel : INotifyPropertyChanged
{
    private string? _id;
    //private bool _isConnected;
    private string? _message;

    /// <summary>
    /// 你的设备ID
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