using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.Foundation;
using System.Numerics;

using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.ModelViews;
using Windows.UI.Core;
using ClipboardCanvas.Models;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class InteractableCanvasControl : UserControl, IInteractableCanvasControlView
    {
        private Canvas _canvasPanel;

        private Point _savedClickPosition;

        public InteractableCanvasControlViewModel ViewModel
        {
            get => (InteractableCanvasControlViewModel)DataContext;
            set => DataContext = value;
        }

        public InteractableCanvasControl()
        {
            this.InitializeComponent();

            this.ViewModel = new InteractableCanvasControlViewModel(this);
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            _canvasPanel = sender as Canvas;
            this.ViewModel.CanvasLoaded();
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            FrameworkElement element = e.DataView.Properties[Constants.UI.CanvasContent.INFINITE_CANVAS_DRAGGED_OBJECT_ID] as FrameworkElement;
            if (element?.DataContext == null)
            {
                return;
            }

            int indexOfItem = this.ViewModel.Items.IndexOf(element.DataContext as InteractableCanvasControlItemViewModel);
            UIElement container = ItemsHolder.ContainerFromIndex(indexOfItem) as UIElement;

            Point dropPoint = e.GetPosition(_canvasPanel);
            
            Canvas.SetLeft(container, dropPoint.X - _savedClickPosition.X);
            Canvas.SetTop(container, dropPoint.Y - _savedClickPosition.Y);

            element.Opacity = 1.0d;
            this.ViewModel.ItemRearranged();
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;

            // Hide element when dragging over the canvas
            if (e.DataView.Properties[Constants.UI.CanvasContent.INFINITE_CANVAS_DRAGGED_OBJECT_ID] is FrameworkElement element 
                && this.ViewModel.Items.Contains(element.DataContext as InteractableCanvasControlItemViewModel))
            {
                element.Opacity = 0.0d;
            }
        }

        private async void RootContentGrid_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            if (sender is FrameworkElement draggedElement)
            {
                Point point = args.GetPosition(draggedElement);
                _savedClickPosition = point;

                // Add the dragged element to properties which we can later retrieve it from
                args.Data.Properties.Add(Constants.UI.CanvasContent.INFINITE_CANVAS_DRAGGED_OBJECT_ID, draggedElement);

                // Also set data associated from the dragged element
                if (draggedElement.DataContext is IDragDataProviderModel dragDataProvider)
                {
                    args.Data.SetStorageItems(await dragDataProvider.GetDragData());
                }
            }
        }

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;

            if (e.DataView.Properties[Constants.UI.CanvasContent.INFINITE_CANVAS_DRAGGED_OBJECT_ID] is FrameworkElement draggedElement)
            {
                draggedElement.Opacity = 1.0d;
            }
        }

        public Vector2 GetItemPosition(InteractableCanvasControlItemViewModel itemViewModel)
        {
            int indexOfItem = this.ViewModel.Items.IndexOf(itemViewModel);
            UIElement container = ItemsHolder.ContainerFromIndex(indexOfItem) as UIElement;

            float x = (float)Canvas.GetLeft(container);
            float y = (float)Canvas.GetTop(container);

            return new Vector2(x, y);
        }

        public void SetItemPosition(InteractableCanvasControlItemViewModel itemViewModel, Vector2 position)
        {
            int indexOfItem = this.ViewModel.Items.IndexOf(itemViewModel);
            UIElement container = ItemsHolder.ContainerFromIndex(indexOfItem) as UIElement;

            Canvas.SetLeft(container, (double)position.X);
            Canvas.SetTop(container, (double)position.Y);
        }
    }
}
