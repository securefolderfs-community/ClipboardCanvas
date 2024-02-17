using ClipboardCanvas.Sdk.ViewModels.Views;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.Sdk.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject
    {
        [ObservableProperty] private MainAppViewModel _AppViewModel; // TODO: Limitation since the UI requires specific type for x:Bind on DependencyProperty

        public MainViewModel()
        {
            _AppViewModel = new MainAppViewModel();
        }
    }
}
