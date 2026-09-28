using CommunityToolkit.Mvvm.ComponentModel;

namespace Chittak.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = Chittak.Protocol.Spike.NsecSpike.Run(); // S0-08 spike
}
