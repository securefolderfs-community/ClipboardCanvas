using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ClipboardCanvas.Services.Implementation
{
    public class ApplicationService : IApplicationService
    {
        public bool IsWindowActivated { get; private set; }

        public string AppVersion { get; } = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        public bool IsInRestrictedAccessMode { get; set; }

        public ApplicationService()
        {
            Window.Current.Activated -= Current_Activated;
            Window.Current.Activated += Current_Activated;
        }

        private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            IsWindowActivated = e.WindowActivationState != CoreWindowActivationState.Deactivated;
        }
    }
}
