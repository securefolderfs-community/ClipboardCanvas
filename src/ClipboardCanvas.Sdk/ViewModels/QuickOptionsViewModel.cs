using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClipboardCanvas.Sdk.ViewModels
{
    public sealed partial class QuickOptionsViewModel : ObservableObject
    {
        [ObservableProperty] private ICommand? _CreateNewDocumentCommand;

        [RelayCommand]
        private Task OpenSettingsAsync()
        {
            // TODO: Open settings dialog
            return Task.CompletedTask;
        }
    }
}
