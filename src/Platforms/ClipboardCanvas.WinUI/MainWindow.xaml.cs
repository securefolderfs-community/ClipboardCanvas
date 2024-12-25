using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels;
using ClipboardCanvas.Shared.Extensions;
using ClipboardCanvas.WinUI.Helpers;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx
    {
        private static MainWindow? _Instance;
        public static MainWindow Instance => _Instance ??= new();

        public MainViewModel MainViewModel { get; } = new();

        public MainWindow()
        {
            InitializeComponent();
            EnsureEarlyWindow();
        }

        private async void EnsureEarlyWindow()
        {
            // Set persistence id
            PersistenceId = UI.Constants.Application.MAIN_WINDOW_ID;

            // Set title
            AppWindow.Title = "Clipboard Canvas";

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                // Extend title bar
                AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

                // Set window buttons background to transparent
                AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
            else
            {
                ExtendsContentIntoTitleBar = true;
                SetTitleBar(AppControl.CustomTitleBar);
            }

            // Set min size
            MinHeight = 572;
            MinWidth = 662;

            // Hook up event for window closing
            AppWindow.Closing += AppWindow_Closing;

            // Setup ThemeHelper
            WindowsThemeHelper.Instance.RegisterWindowInstance(Content as FrameworkElement, AppWindow);
            await WindowsThemeHelper.Instance.InitAsync();
        }

        private async void AppControl_Loaded(object sender, RoutedEventArgs e)
        {
            await MainViewModel.InitAsync();
        }

        private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            await settingsService.TrySaveAsync();
        }
    }
}
