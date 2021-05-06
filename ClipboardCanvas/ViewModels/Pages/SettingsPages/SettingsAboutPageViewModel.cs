using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsAboutPageViewModel : ObservableObject
    {
        public string VersionNumber
        {
            get => "1";
            //get => $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";
        }
    }
}
