using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Enums;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasPreview
{
    public class CanvasPreviewControlViewModel : BaseCanvasPreviewControlViewModel, IDisposable
    {
        #region Constructor

        public CanvasPreviewControlViewModel(IBaseCanvasPreviewControlView view)
            :base(view)
        {
        }

        #endregion

        #region Private Helpers

        protected override bool InitializeViewModelFromContentType(BasePastedContentTypeDataModel contentType)
        {
            // Try for image
            if (InitializeViewModelForType<ImageContentType, ImageCanvasViewModel>(contentType, () => new ImageCanvasViewModel(view)))
            {
                return true;
            }

            // Try for text
            if (InitializeViewModelForType<TextContentType, TextCanvasViewModel>(contentType, () => new TextCanvasViewModel(view)))
            {
                return true;
            }

            // Try for media
            if (InitializeViewModelForType<MediaContentType, MediaCanvasViewModel>(contentType, () => new MediaCanvasViewModel(view)))
            {
                return true;
            }

            // Try for WebView
            if (InitializeViewModelForType<WebViewContentType, WebViewCanvasViewModel>(contentType, () => new WebViewCanvasViewModel(view, (contentType as WebViewContentType)?.mode ?? WebViewCanvasMode.Unknown)))
            {
                return true;
            }

            // Try for markdown
            if (InitializeViewModelForType<MarkdownContentType, MarkdownCanvasViewModel>(contentType, () => new MarkdownCanvasViewModel(view)))
            {
                return true;
            }

            // Try fallback
            if (InitializeViewModelForType<FallbackContentType, FallbackCanvasViewModel>(contentType, () => new FallbackCanvasViewModel(view)))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
