using System;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Windows.Foundation;
using System.Collections.Generic;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Numerics;

using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Models;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.DataModels;

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

        private async void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            _canvasPanel = sender as Canvas;
            await this.ViewModel.CanvasLoaded();
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            Point dropPoint = e.GetPosition(_canvasPanel);

            FrameworkElement element = e.DataView.Properties[Constants.UI.CanvasContent.INFINITE_CANVAS_DRAGGED_OBJECT_ID] as FrameworkElement;
            if (element?.DataContext == null && e.DataView != null)
            {
                // Save position and dataPackage
                this.ViewModel.DataPackageComparisionDataModel = new InteractableCanvasDataPackageComparisionDataModel(e.DataView, dropPoint);
            }
            else
            {
                int indexOfItem = this.ViewModel.Items.IndexOf(element.DataContext as InteractableCanvasControlItemViewModel);
                FrameworkElement container = ItemsHolder.ContainerFromIndex(indexOfItem) as FrameworkElement;

                double dropX = dropPoint.X - _savedClickPosition.X;
                double dropY = dropPoint.Y - _savedClickPosition.Y;

                double leftMostBound = dropX + container.ActualWidth;
                double topMostBound = dropY + container.ActualHeight;

                // Align
                if (leftMostBound > _canvasPanel.ActualWidth)
                {
                    dropX = _canvasPanel.ActualWidth - container.ActualWidth;
                }
                else if (dropX < 0.0d)
                {
                    dropX = 0.0d;
                }

                if (topMostBound > _canvasPanel.ActualHeight)
                {
                    dropY = _canvasPanel.ActualHeight - container.ActualHeight;
                }
                else if (dropY < 0.0d)
                {
                    dropY = 0.0d;
                }

                Canvas.SetLeft(container, dropX);
                Canvas.SetTop(container, dropY);

                element.Opacity = 1.0d;

                // Update the ZIndex of the element - set it on top
                SetOnTop(container); // TODO: Fix this - sometimes other item can shown higher

                this.ViewModel.ItemRearranged();
            }
        }

        private void SetOnTop(UIElement element)
        {
            if (element == null)
            {
                return;
            }

            int elementZIndex = Canvas.GetZIndex(element);
            int highestZIndex = Math.Max(elementZIndex, 1);

            foreach (object item in ItemsHolder.Items)
            {
                UIElement uiItemContainer = ItemsHolder.ContainerFromItem(item) as UIElement;

                int itemZIndex = Canvas.GetZIndex(uiItemContainer);
                highestZIndex = Math.Max(itemZIndex, highestZIndex);

                if (itemZIndex > elementZIndex)
                {
                    Canvas.SetZIndex(uiItemContainer, itemZIndex - 1);
                }
            }

            // Set the highest ZIndex
            Canvas.SetZIndex(element, highestZIndex);
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
                    await dragDataProvider.SetDragData(args.Data);
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

        private async void RootContentGrid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is InteractableCanvasControlItemViewModel itemViewModel)
            {
                await itemViewModel.OpenFile();
            }
        }

        public Vector2 GetItemPosition(InteractableCanvasControlItemViewModel itemViewModel)
        {
            int indexOfItem = this.ViewModel.Items.IndexOf(itemViewModel);

            if (ItemsHolder.ContainerFromIndex(indexOfItem) is UIElement container)
            {
                float x = (float)Canvas.GetLeft(container);
                float y = (float)Canvas.GetTop(container);

                return new Vector2(x, y);
            }

            return new Vector2(0, 0);
        }

        public void SetItemPosition(InteractableCanvasControlItemViewModel itemViewModel, Vector2 position)
        {
            int indexOfItem = this.ViewModel.Items.IndexOf(itemViewModel);

            if (ItemsHolder.ContainerFromIndex(indexOfItem) is UIElement container)
            {
                Canvas.SetLeft(container, (double)position.X);
                Canvas.SetTop(container, (double)position.Y);
            }
        }

        public async Task<(IBuffer buffer, uint pixelWidth, uint pixelHeight)> GetCanvasImageBuffer()
        {
            try
            {
                if (RootGrid == null)
                {
                    return (null, 0, 0);
                }

                RenderTargetBitmap rtb = new RenderTargetBitmap();
                await rtb.RenderAsync(RootGrid);

                IBuffer pixelBuffer = await rtb.GetPixelsAsync();

                return (pixelBuffer, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight);
            }
            catch
            {
                return (null, 0, 0);
            }
        }

        public void SetOnTop(InteractableCanvasControlItemViewModel itemViewModel)
        {
            SetOnTop(ItemsHolder.ContainerFromItem(itemViewModel) as UIElement);
        }

        public int GetCanvasTopIndex(InteractableCanvasControlItemViewModel itemViewModel)
        {
            if (ItemsHolder.ContainerFromItem(itemViewModel) is UIElement container)
            {
                return Canvas.GetZIndex(container);
            }

            return 0;
        }

        public void SetCanvasTopIndex(InteractableCanvasControlItemViewModel itemViewModel, int topIndex)
        {
            if (ItemsHolder.ContainerFromItem(itemViewModel) is UIElement container)
            {
                Canvas.SetZIndex(container, topIndex);
            }
        }
    }
}
