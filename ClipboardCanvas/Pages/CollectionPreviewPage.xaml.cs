using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml;
using System.Linq;

using ClipboardCanvas.ViewModels.Pages;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.ViewModels;
using ClipboardCanvas.DataModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClipboardCanvas.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CollectionPreviewPage : Page, ICollectionPreviewPageView
    {
        private CanvasItem _canvasItemToScrollTo;

        private bool _suppressDispose;

        public CollectionPreviewPageViewModel ViewModel
        {
            get => (CollectionPreviewPageViewModel)DataContext;
            set => DataContext = value;
        }

        public ICollectionModel AssociatedCollectionModel { get; private set; }

        public ISearchControlModel SearchControlModel => CanvasPreviewSearchControl.ViewModel;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            CollectionPreviewPageNavigationParameterModel navigationParameter = e.Parameter as CollectionPreviewPageNavigationParameterModel;
            AssociatedCollectionModel = navigationParameter.collectionModel;
            _canvasItemToScrollTo = navigationParameter.itemToSelect;
        }

        public CollectionPreviewPage()
        {
            this.InitializeComponent();

            this.ViewModel = new CollectionPreviewPageViewModel(this);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (!_suppressDispose)
            {
                this.ViewModel.Dispose();
            }
            else
            {
                this._suppressDispose = false;
            }

            base.OnNavigatingFrom(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.Initialize();
            this.ViewModel.CheckSearchContext();
        }

        public void PrepareConnectedAnimation(int itemIndex)
        {
            // Prevent crash when canvas preview is null
            if (ViewModel.Items[itemIndex].ReadOnlyCanvasPreviewModel is IReadOnlyCanvasPreviewModel canvasPreviewModel
                && canvasPreviewModel.IsContentLoaded)
            {
                UIElement sourceAnimationControl = ((ItemsGrid.ContainerFromIndex(itemIndex) as ContentControl).ContentTemplateRoot as FrameworkElement).FindName("SimpleCanvasPreviewControl") as UIElement;

                ConnectedAnimation connectedAnimation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(
                    Constants.UI.Animations.CONNECTED_ANIMATION_COLLECTION_PREVIEW_ITEM_OPEN_REQUESTED_TOKEN,
                    sourceAnimationControl);

                connectedAnimation.Configuration = new DirectConnectedAnimationConfiguration();
                _suppressDispose = true;
            }
        }

        public void ScrollItemToView(CollectionPreviewItemViewModel itemToScrollTo)
        {
            ItemsGrid.ScrollIntoView(itemToScrollTo);
        }

        public void ScrollToItemOnInitialization(CollectionPreviewItemViewModel itemToScrollTo)
        {
            if (itemToScrollTo != null && _canvasItemToScrollTo == null)
            {
                ScrollItemToView(itemToScrollTo);
            }
            else if (_canvasItemToScrollTo != null)
            {
                itemToScrollTo = this.ViewModel.Items.FirstOrDefault((item) => item.CollectionItemViewModel.AssociatedItem.Path == _canvasItemToScrollTo.AssociatedItem.Path);

                if (itemToScrollTo != null)
                {
                    this.ViewModel.SelectedItem = itemToScrollTo;
                    ScrollItemToView(itemToScrollTo);
                }
            }    
        }

        private void RootPanel_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CollectionPreviewItemViewModel itemViewModel)
            {
                this.ViewModel.OpenItem(itemViewModel);
            }
        }
    }
}
