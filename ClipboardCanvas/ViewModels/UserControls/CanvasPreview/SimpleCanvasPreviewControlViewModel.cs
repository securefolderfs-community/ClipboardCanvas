using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.ViewModels.UserControls.SimpleCanvasDisplay;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasPreview
{
    public class SimpleCanvasPreviewControlViewModel : BaseReadOnlyCanvasPreviewControlViewModel<BaseReadOnlyCanvasViewModel>
    {
        #region Constructor

        public SimpleCanvasPreviewControlViewModel(IBaseCanvasPreviewControlView view)
            : base(view)
        {
        }

        #endregion

        #region Override

        protected override bool InitializeViewModelFromContentType(BaseContentTypeModel contentType)
        {
            // Try for infinite canvas
            if (InitializeViewModelForType<InfiniteCanvasContentType, ThumbnailSimpleCanvasViewModel>(contentType, () => new ThumbnailSimpleCanvasViewModel(view)))
            {
                return true;
            }

            // Try for image
            if (InitializeViewModelForType<ImageContentType, ThumbnailSimpleCanvasViewModel>(contentType, () => new ThumbnailSimpleCanvasViewModel(view)))
            {
                return true;
            }

            // Try for text
            if (InitializeViewModelForType<TextContentType, TextSimpleCanvasViewModel>(contentType, () => new TextSimpleCanvasViewModel(view)))
            {
                return true;
            }

            // Try for media
            if (InitializeViewModelForType<MediaContentType, ThumbnailSimpleCanvasViewModel>(contentType, () => new ThumbnailSimpleCanvasViewModel(view)))
            {
                return true;
            }

            // Try for WebView
            if (InitializeViewModelForType<WebViewContentType, ThumbnailSimpleCanvasViewModel>(contentType, () => new ThumbnailSimpleCanvasViewModel(view)))
            {
                return true;
            }

            // Try for markdown // TODO: Add simple markdown
            if (InitializeViewModelForType<MarkdownContentType, TextSimpleCanvasViewModel>(contentType, () => new TextSimpleCanvasViewModel(view)))
            {
                return true;
            }

            // Try fallback
            if (InitializeViewModelForType<FallbackContentType, ThumbnailSimpleCanvasViewModel>(contentType, () => new ThumbnailSimpleCanvasViewModel(view)))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
