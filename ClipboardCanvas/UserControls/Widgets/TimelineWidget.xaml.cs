using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.Widgets.Timeline;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.Widgets
{
    public sealed partial class TimelineWidget : UserControl
    {
        public TimelineWidgetViewModel ViewModel
        {
            get => (TimelineWidgetViewModel)DataContext;
            set => DataContext = value;
        }

        public TimelineWidget()
        {
            this.InitializeComponent();

            this.ViewModel = new TimelineWidgetViewModel();
        }
    }
}
