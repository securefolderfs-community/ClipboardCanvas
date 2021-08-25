using Windows.UI.Xaml.Controls;

using ClipboardCanvas.ViewModels.UserControls.OOBE;
using ClipboardCanvas.ModelViews;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.OOBE
{
    public sealed partial class IntroductionScreenPanel : UserControl, IIntroductionScreenPaneView
    {
        public IntroductionScreenPanelViewModel ViewModel
        {
            get => (IntroductionScreenPanelViewModel)DataContext;
            set => DataContext = value;
        }

        public IntroductionScreenPanel()
        {
            this.InitializeComponent();

            this.ViewModel = new IntroductionScreenPanelViewModel(this);
        }

        public bool IntroductionPanelLoad
        {
            get => MainPage.Instance.IntroductionPanelLoad;
            set => MainPage.Instance.IntroductionPanelLoad = value;
        }

        public int ItemsCount
        {
            get => IntroductionFilpView.Items.Count;
        }
    }
}
