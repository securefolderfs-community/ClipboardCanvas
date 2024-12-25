using ClipboardCanvas.Sdk.ViewModels.Views;
using ClipboardCanvas.Shared.Extensions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CanvasView : Page
    {
        private bool _isImmersed;

        public CanvasViewModel? ViewModel
        {
            get => DataContext.TryCast<CanvasViewModel>();
            set => DataContext = value;
        }

        public CanvasView()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is CanvasViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private void Root_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel?.CurrentCanvasViewModel is null)
                return;

            _isImmersed = !_isImmersed;
            ViewModel?.ChangeImmersion(_isImmersed);
        }

        private async void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var key = args.KeyboardAccelerator.Key;
            var control = args.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Control);

            switch ((key, control))
            {
                case (VirtualKey.V, true):
                    if (ViewModel is null)
                        break;

                    args.Handled = true;
                    await ViewModel.PasteFromClipboardCommand.ExecuteAsync(null);
                    break;
            }
        }
    }
}
