using ClipboardCanvas.Sdk.ViewModels.Views;
using ClipboardCanvas.UI.Helpers;
using ClipboardCanvas.UI.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls.Host
{
    public sealed partial class MainAppHost : UserControl
    {
        public MainAppHost()
        {
            InitializeComponent();

            OpenSettingsItem.KeyboardAccelerators.Add(new KeyboardAccelerator()
            {
                Key = (VirtualKey)188,
                Modifiers = VirtualKeyModifiers.Control
            });
        }

        private void Navigation_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not INavigationControl navigationControl)
                return;

            ViewModel.NavigationService.SetupNavigation(navigationControl);
            _ = ViewModel.InitAsync();
        }

        public MainAppViewModel ViewModel
        {
            get => (MainAppViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainAppViewModel), typeof(MainAppHost), new PropertyMetadata(null));
    }
}
