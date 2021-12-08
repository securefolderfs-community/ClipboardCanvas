using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.Settings
{
    [ContentProperty(Name = nameof(ActionElement))]
    public sealed partial class SettingBlockControl : UserControl
    {
        public SettingBlockControl()
        {
            this.InitializeComponent();
        }


        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }
        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(SettingBlockControl), new PropertyMetadata(null));


        public ICommand ExpanderExpandingCommand
        {
            get { return (ICommand)GetValue(ExpanderExpandingCommandProperty); }
            set { SetValue(ExpanderExpandingCommandProperty, value); }
        }
        public static readonly DependencyProperty ExpanderExpandingCommandProperty =
            DependencyProperty.Register("ExpanderExpandingCommand", typeof(ICommand), typeof(SettingBlockControl), new PropertyMetadata(null));


        public FrameworkElement ExpanderContent
        {
            get { return (FrameworkElement)GetValue(ExpanderContentProperty); }
            set { SetValue(ExpanderContentProperty, value); }
        }
        public static readonly DependencyProperty ExpanderContentProperty =
            DependencyProperty.Register("ExpanderContent", typeof(FrameworkElement), typeof(SettingBlockControl), new PropertyMetadata(null));


        public bool IsClickable
        {
            get => (bool)GetValue(IsClickableProperty);
            set => SetValue(IsClickableProperty, value);
        }
        public static readonly DependencyProperty IsClickableProperty =
            DependencyProperty.Register("IsClickable", typeof(bool), typeof(SettingBlockControl), new PropertyMetadata(false));


        public IconElement Icon
        {
            get { return (IconElement)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(IconElement), typeof(SettingBlockControl), new PropertyMetadata(null));


        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(SettingBlockControl), new PropertyMetadata(null));


        public FrameworkElement AdditionalDescription
        {
            get { return (FrameworkElement)GetValue(AdditionalDescriptionProperty); }
            set { SetValue(AdditionalDescriptionProperty, value); }
        }
        public static readonly DependencyProperty AdditionalDescriptionProperty =
            DependencyProperty.Register("AdditionalDescription", typeof(FrameworkElement), typeof(SettingBlockControl), new PropertyMetadata(null));


        public FrameworkElement ActionElement
        {
            get { return (FrameworkElement)GetValue(ActionElementProperty); }
            set { SetValue(ActionElementProperty, value); }
        }
        public static readonly DependencyProperty ActionElementProperty =
            DependencyProperty.Register("ActionElement", typeof(FrameworkElement), typeof(SettingBlockControl), new PropertyMetadata(null));


        public FrameworkElement AdditionalActionElement
        {
            get { return (FrameworkElement)GetValue(AdditionalActionElementProperty); }
            set { SetValue(AdditionalActionElementProperty, value); }
        }
        public static readonly DependencyProperty AdditionalActionElementProperty =
            DependencyProperty.Register("AdditionalActionElement", typeof(FrameworkElement), typeof(SettingBlockControl), new PropertyMetadata(null));

        private void Expander_Expanding(Microsoft.UI.Xaml.Controls.Expander sender, Microsoft.UI.Xaml.Controls.ExpanderExpandingEventArgs args)
        {
            ExpanderExpandingCommand?.Execute(null);
        }
    }
}
