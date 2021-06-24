using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.Pages;
using Windows.UI.Xaml.Navigation;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.DataModels.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClipboardCanvas.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CollectionPreviewPage : Page, ICollectionPreviewPageView
    {
        public CollectionPreviewPageViewModel ViewModel
        {
            get => (CollectionPreviewPageViewModel)DataContext;
            set => DataContext = value;
        }

        public ICollectionModel AssociatedCollectionModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DisplayFrameNavigationParameterDataModel navigationParameter = e.Parameter as DisplayFrameNavigationParameterDataModel;
            AssociatedCollectionModel = navigationParameter.collectionModel;
        }

        public CollectionPreviewPage()
        {
            this.InitializeComponent();

            this.ViewModel = new CollectionPreviewPageViewModel(this);
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.ViewModel.Initialize();
        }
    }
}
