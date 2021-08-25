using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.Settings
{
    public sealed partial class SettingBlockHeaderContentControl : UserControl
    {
        public SettingBlockHeaderContentControl()
        {
            this.InitializeComponent();
        }

        public IconElement Icon
        {
            get { return (IconElement)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(IconElement), typeof(SettingBlockHeaderContentControl), new PropertyMetadata(null));


        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(SettingBlockHeaderContentControl), new PropertyMetadata(null));


        public FrameworkElement AdditionalDescription
        {
            get { return (FrameworkElement)GetValue(AdditionalDescriptionProperty); }
            set { SetValue(AdditionalDescriptionProperty, value); }
        }
        public static readonly DependencyProperty AdditionalDescriptionProperty =
            DependencyProperty.Register("AdditionalDescription", typeof(FrameworkElement), typeof(SettingBlockHeaderContentControl), new PropertyMetadata(null));


        public FrameworkElement ActionElement
        {
            get { return (FrameworkElement)GetValue(ActionElementProperty); }
            set { SetValue(ActionElementProperty, value); }
        }
        public static readonly DependencyProperty ActionElementProperty =
            DependencyProperty.Register("ActionElement", typeof(FrameworkElement), typeof(SettingBlockHeaderContentControl), new PropertyMetadata(null));


        public FrameworkElement AdditionalActionElement
        {
            get { return (FrameworkElement)GetValue(AdditionalActionElementProperty); }
            set { SetValue(AdditionalActionElementProperty, value); }
        }
        public static readonly DependencyProperty AdditionalActionElementProperty =
            DependencyProperty.Register("AdditionalActionElement", typeof(FrameworkElement), typeof(SettingBlockHeaderContentControl), new PropertyMetadata(null));
    }
}
