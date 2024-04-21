using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls
{
    public sealed partial class AnimatedCornerRadiusControl : UserControl
    {
        public AnimatedCornerRadiusControl()
        {
            InitializeComponent();
        }

        public object? ControlContent
        {
            get => (object?)GetValue(ControlContentProperty);
            set => SetValue(ControlContentProperty, value);
        }
        public static readonly DependencyProperty ControlContentProperty =
            DependencyProperty.Register(nameof(ControlContent), typeof(object), typeof(AnimatedCornerRadiusControl), new PropertyMetadata(null));

        public int TopLeftCorner
        {
            get => (int)GetValue(TopLeftCornerProperty);
            set => SetValue(TopLeftCornerProperty, value);
        }
        public static readonly DependencyProperty TopLeftCornerProperty =
            DependencyProperty.Register(nameof(TopLeftCorner), typeof(int), typeof(AnimatedCornerRadiusControl),
                new PropertyMetadata(0, (s, e) =>
                {
                    ((AnimatedCornerRadiusControl)s).MainContent.CornerRadius = new CornerRadius(
                        double.Parse(e.NewValue.ToString()),
                        ((AnimatedCornerRadiusControl)s).CornerRadius.TopRight,
                        ((AnimatedCornerRadiusControl)s).CornerRadius.BottomRight,
                        ((AnimatedCornerRadiusControl)s).CornerRadius.BottomLeft);
                }));

        public int TopRightCorner
        {
            get => (int)GetValue(TopRightCornerProperty);
            set => SetValue(TopRightCornerProperty, value);
        }
        public static readonly DependencyProperty TopRightCornerProperty =
            DependencyProperty.Register(nameof(TopRightCorner), typeof(int), typeof(AnimatedCornerRadiusControl),
                new PropertyMetadata(0, (s, e) =>
                {
                    ((AnimatedCornerRadiusControl)s).MainContent.CornerRadius = new CornerRadius(
                        ((AnimatedCornerRadiusControl)s).CornerRadius.TopLeft,
                        double.Parse(e.NewValue.ToString()),
                        ((AnimatedCornerRadiusControl)s).CornerRadius.BottomRight,
                        ((AnimatedCornerRadiusControl)s).CornerRadius.BottomLeft);
                }));

        public int BottomRightCorner
        {
            get => (int)GetValue(BottomRightCornerProperty);
            set => SetValue(BottomRightCornerProperty, value);
        }
        public static readonly DependencyProperty BottomRightCornerProperty =
            DependencyProperty.Register(nameof(BottomRightCorner), typeof(int), typeof(AnimatedCornerRadiusControl),
                new PropertyMetadata(0, (s, e) =>
                {
                    ((AnimatedCornerRadiusControl)s).MainContent.CornerRadius = new CornerRadius(
                        ((AnimatedCornerRadiusControl)s).CornerRadius.TopLeft,
                        ((AnimatedCornerRadiusControl)s).CornerRadius.TopRight,
                        double.Parse(e.NewValue.ToString()),
                        ((AnimatedCornerRadiusControl)s).CornerRadius.BottomLeft);
                }));

        public int BottomLeftCorner
        {
            get => (int)GetValue(BottomLeftCornerProperty);
            set => SetValue(BottomLeftCornerProperty, value);
        }
        public static readonly DependencyProperty BottomLeftCornerProperty =
            DependencyProperty.Register(nameof(BottomLeftCorner), typeof(int), typeof(AnimatedCornerRadiusControl),
                new PropertyMetadata(0, (s, e) =>
                {
                    ((AnimatedCornerRadiusControl)s).MainContent.CornerRadius = new CornerRadius(
                        ((AnimatedCornerRadiusControl)s).CornerRadius.TopLeft,
                        ((AnimatedCornerRadiusControl)s).CornerRadius.TopRight,
                        ((AnimatedCornerRadiusControl)s).CornerRadius.BottomRight,
                        double.Parse(e.NewValue.ToString()));
                }));
    }
}
