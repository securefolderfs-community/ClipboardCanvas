using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;

using ClipboardCanvas.ViewModels.Pages;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Models;
using ClipboardCanvas.DataModels.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClipboardCanvas.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CanvasPage : Page, ICanvasPageView
    {
        private IDisposable _collectionPreviewIDisposable;

        private ConnectedAnimation _connectedAnimation;

        public CanvasPageViewModel ViewModel
        {
            get => (CanvasPageViewModel)DataContext;
            set => DataContext = value;
        }

        public ICanvasPreviewModel CanvasPreviewModel => CanvasPreviewControl?.ViewModel;

        public ICollectionModel AssociatedCollectionModel { get; private set; }

        public CanvasPage()
        {
            this.InitializeComponent();

            this.ViewModel = new CanvasPageViewModel(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            CanvasPageNavigationParameterModel navigationParameter = e.Parameter as CanvasPageNavigationParameterModel;
            AssociatedCollectionModel = navigationParameter.collectionModel;
            this.ViewModel.RequestedCanvasType = navigationParameter.canvasType;

            if (navigationParameter.CollectionPreviewIDisposable != null)
            {
                this._collectionPreviewIDisposable = navigationParameter.CollectionPreviewIDisposable;
            }

            // Set connected animation
            _connectedAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation(
                Constants.UI.Animations.CONNECTED_ANIMATION_COLLECTION_PREVIEW_ITEM_OPEN_REQUESTED_TOKEN);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (_collectionPreviewIDisposable != null)
            {
                // In-case the user closed the page before connected animation finished, we need to Dispose
                _collectionPreviewIDisposable.Dispose();
                _collectionPreviewIDisposable = null;
            }

            base.OnNavigatingFrom(e);
        }

        private void PastedAsReference_Click(object sender, RoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout(PastedAsReference);
        }

        private void CanvasContextMenu_Opening(object sender, object e)
        {
            ViewModel.CanvasContextMenuOpeningCommand.Execute(null);
        }

        public void FinishConnectedAnimation()
        {
            // Animate connected animation if available
            if (_connectedAnimation != null)
            {
                _connectedAnimation.TryStart(CanvasPreviewControl);
            }

            if (_collectionPreviewIDisposable != null)
            {
                _collectionPreviewIDisposable.Dispose();
                _collectionPreviewIDisposable = null;
            }
        }
    }
}
