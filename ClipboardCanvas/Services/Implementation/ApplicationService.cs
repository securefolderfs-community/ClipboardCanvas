using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;

using ClipboardCanvas.DataModels;

namespace ClipboardCanvas.Services.Implementation
{
    public class ApplicationService : IApplicationService
    {
        public bool IsWindowActivated { get; private set; }

        public string AppVersion { get; } = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        public bool IsInRestrictedAccessMode { get; set; }

        public AppLanguageModel AppLanguage
        {
            get => AppLanguages.FirstOrDefault(item => item.Id == ApplicationLanguages.PrimaryLanguageOverride) ?? AppLanguages.FirstOrDefault();
            set => ApplicationLanguages.PrimaryLanguageOverride = value.Id;
        }

        public List<AppLanguageModel> AppLanguages { get; } = ApplicationLanguages.ManifestLanguages.Select((item) => new AppLanguageModel(item)).ToList();

        public AppLanguageModel CurrentAppLanguage { get; } = new AppLanguageModel(ApplicationLanguages.PrimaryLanguageOverride);

        public IntPtr GetHwnd(Window wnd)
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(wnd);
            return hwnd;
        }

        public ApplicationService()
        {
            MainWindow.Instance.Activated -= Current_Activated;
            MainWindow.Instance.Activated += Current_Activated;
        }

        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            IsWindowActivated = e.WindowActivationState != WindowActivationState.Deactivated;
        }
    }
}
