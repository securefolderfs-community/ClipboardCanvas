using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml;

using ClipboardCanvas.ViewModels.Pages;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.ViewModels;
using ClipboardCanvas.UserControls.SimpleCanvasDisplay;
using System.Linq;
using ClipboardCanvas.Extensions;
using System;
using System.Diagnostics;

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

        public ISearchControlModel SearchControlModel => CanvasPreviewSearchControl.ViewModel;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DisplayFrameParameterDataModel navigationParameter = e.Parameter as DisplayFrameParameterDataModel;
            AssociatedCollectionModel = navigationParameter.collectionModel;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            this.ViewModel.Dispose();

            base.OnNavigatingFrom(e);
        }

        public CollectionPreviewPage()
        {
            this.InitializeComponent();

            this.ViewModel = new CollectionPreviewPageViewModel(this);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.InitializeItems();
            this.ViewModel.CheckSearchContext();
        }

        public void PrepareConnectedAnimation(int itemIndex)
        {
            UIElement sourceAnimationControl = ((ItemsGrid.ContainerFromIndex(itemIndex) as ContentControl).ContentTemplateRoot as FrameworkElement).FindName("SimpleCanvasPreviewControl") as UIElement;

            ConnectedAnimation connectedAnimation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(
                Constants.UI.Animations.CONNECTED_ANIMATION_COLLECTION_PREVIEW_ITEM_OPEN_REQUESTED_TOKEN,
                sourceAnimationControl);

            connectedAnimation.Configuration = new DirectConnectedAnimationConfiguration();
        }

        public void ScrollIntoItemView(CollectionPreviewItemViewModel sourceViewModel)
        {
            ItemsGrid.ScrollIntoView(sourceViewModel);
        }
    }
}
