using System.Collections.ObjectModel;
using System.ComponentModel;
using TCPNetterServerGUI.GUI.Model;

namespace TCPNetterServerGUI.GUI.ViewModel;

public class UIViewModel : INotifyPropertyChanged
{
    private AntdUI.AntList<UIModel> _uiModels;

    public AntdUI.AntList<UIModel> UIModels
    {
        get => _uiModels;
        set
        {
            if (_uiModels != value)
            {
                _uiModels = value;
                OnPropertyChanged(nameof(UIModels));
            }
        }
    }

    public UIViewModel()
    {
        // 初始化数据
        //UIModels = new ObservableCollection<UIModel>
        //{
        //    new UIModel { Id = "Device1",  Message = "Connected" },
        //    new UIModel { Id = "Device2",  Message = "Disconnected" }
        //};

        UIModels = new AntdUI.AntList<UIModel>();
    }

    // 添加新连接
    public void AddConnection(string id, string deviceName,string message)
    {
        var newConnection = new UIModel { Id = id, DeviceName = deviceName , Message = message};
        UIModels.Add(newConnection);
    }

    // 更新连接状态
    public void UpdateConnectionStatus(string id, string? deviceName, string? message)
    {
        var connection = UIModels.FirstOrDefault(c => c.Id == id);
        if (connection != null)
        {
            if (deviceName != null)
            {
                connection.DeviceName = deviceName;
            }

            if (message != null)
            {
                connection.Message = message;
            }
        }
    }

    // 删除连接
    public void RemoveConnection(string id)
    {
        var connection = UIModels.FirstOrDefault(c => c.Id == id);
        if (connection != null)
        {
            UIModels.Remove(connection);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}