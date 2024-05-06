using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using ClipboardCanvas.Shared.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls
{
    public sealed partial class CanvasDisplayControl : UserControl
    {
        public CanvasDisplayControl()
        {
            InitializeComponent();
        }

        private async void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (!CanvasViewModel?.IsEditing ?? true)
                return;

            var key = args.KeyboardAccelerator.Key;
            var control = args.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Control);

            switch ((key, control))
            {
                case (VirtualKey.S, true):
                    if (CanvasViewModel is IPersistable persistable)
                    {
                        args.Handled = true;
                        await persistable.SaveAsync();
                    }
                    break;
            }
        }

        public BaseCanvasViewModel? CanvasViewModel
        {
            get => (BaseCanvasViewModel?)GetValue(CanvasViewModelProperty);
            set => SetValue(CanvasViewModelProperty, value);
        }
        public static readonly DependencyProperty CanvasViewModelProperty =
            DependencyProperty.Register(nameof(CanvasViewModel), typeof(BaseCanvasViewModel), typeof(CanvasDisplayControl), new PropertyMetadata(null));
    }
}
