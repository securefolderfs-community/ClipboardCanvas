using ClipboardCanvas.Shared.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls.Canvases
{
    public sealed partial class ImageCanvasControl : UserControl
    {
        private Point _lastPoint;
        private bool _isDragging;

        public ImageCanvasControl()
        {
            InitializeComponent();
            IsEditing = false;
        }

        private void Scroller_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Scroller.CapturePointer(e.Pointer);
            _isDragging = IsZoomingEnabled;
            _lastPoint = e.GetCurrentPoint((UIElement)sender).Position;
        }

        private void Image_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
        }

        private void Scroller_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            Scroller.ReleasePointerCapture(e.Pointer);
        }

        private void Scroller_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsZoomingEnabled)
                return;

            if (!_isDragging)
                return;

            var currentPoint = e.GetCurrentPoint((UIElement)sender).Position;
            var dx = (currentPoint.X - _lastPoint.X) * 3.0f;
            var dy = (currentPoint.Y - _lastPoint.Y) * 3.0f;

            Scroller.ChangeView(
                Scroller.HorizontalOffset - dx,
                Scroller.VerticalOffset - dy,
                null);

            _lastPoint = currentPoint;
        }

        private void Scroller_Loaded(object sender, RoutedEventArgs e)
        {
            ControlImage.Height = Scroller.ViewportHeight;
            ControlImage.Width = Scroller.ViewportWidth;
        }

        public IImage? Image
        {
            get => (IImage?)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(IImage), typeof(ImageCanvasControl), new PropertyMetadata(null, (s, e) =>
            {
                if (s is not ImageCanvasControl control)
                    return;

                control.ControlImage.Height = control.Scroller.ViewportHeight;
                control.ControlImage.Width = control.Scroller.ViewportWidth;
            }));

        public bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(ImageCanvasControl), new PropertyMetadata(false));

        public bool WasAltered
        {
            get => (bool)GetValue(WasAlteredProperty);
            set => SetValue(WasAlteredProperty, value);
        }
        public static readonly DependencyProperty WasAlteredProperty =
            DependencyProperty.Register(nameof(WasAltered), typeof(bool), typeof(ImageCanvasControl), new PropertyMetadata(false));

        public bool IsZoomingEnabled
        {
            get => (bool)GetValue(IsZoomingEnabledProperty);
            set => SetValue(IsZoomingEnabledProperty, value);
        }
        public static readonly DependencyProperty IsZoomingEnabledProperty =
            DependencyProperty.Register(nameof(IsZoomingEnabled), typeof(bool), typeof(ImageCanvasControl), new PropertyMetadata(false, (s, e) =>
            {
                if (s is not ImageCanvasControl control)
                    return;

                control.Scroller.ZoomMode = (bool)e.NewValue ? ZoomMode.Enabled : ZoomMode.Disabled;
            }));
    }
}
