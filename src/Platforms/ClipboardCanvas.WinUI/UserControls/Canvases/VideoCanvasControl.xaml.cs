using ClipboardCanvas.Sdk.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls.Canvases
{
    public sealed partial class VideoCanvasControl : UserControl
    {
        public VideoCanvasControl()
        {
            InitializeComponent();
        }

        private void TransportControls_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        public bool IsImmersed
        {
            get => (bool)GetValue(IsImmersedProperty);
            set => SetValue(IsImmersedProperty, value);
        }
        public static readonly DependencyProperty IsImmersedProperty =
            DependencyProperty.Register(nameof(IsImmersed), typeof(bool), typeof(VideoCanvasControl), new PropertyMetadata(false));

        public IMediaSource? MediaSource
        {
            get => (IMediaSource?)GetValue(MediaSourceProperty);
            set => SetValue(MediaSourceProperty, value);
        }
        public static readonly DependencyProperty MediaSourceProperty =
            DependencyProperty.Register(nameof(MediaSource), typeof(IMediaSource), typeof(VideoCanvasControl), new PropertyMetadata(null));
    }
}
