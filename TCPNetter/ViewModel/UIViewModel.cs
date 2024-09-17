using System.Collections.ObjectModel;
using System.ComponentModel;
using TCPNetter.Model;

namespace TCPNetter.ViewModel;

public class UIViewModel : INotifyPropertyChanged
{
    private ObservableCollection<UIModel> _uiModels;

    public ObservableCollection<UIModel> UIModels
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
        UIModels = new ObservableCollection<UIModel>
        {
            new UIModel { Id = "Device1",  Message = "Connected" },
            new UIModel { Id = "Device2",  Message = "Disconnected" }
        };

        //UIModels = new ObservableCollection<UIModel>();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}