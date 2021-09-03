using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Enums;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.Contexts.Operations;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class WebViewCanvasViewModel : BaseCanvasViewModel
    {
        #region Members

        private bool _webViewNeedsUpdate;

        private bool _webViewIsLoaded;

        #endregion

        #region Properties

        private WebViewPasteModel WebViewPasteModel => canvasPasteModel as WebViewPasteModel;

        private WebViewContentType WebViewContentType => ContentType as WebViewContentType;

        public WebViewCanvasMode Mode => WebViewContentType.mode;

        public static List<string> Extensions => new List<string>() {
            ".html", ".htm", Constants.FileSystem.WEBSITE_LINK_FILE_EXTENSION,
        };

        private bool _ContentWebViewLoad;
        public bool ContentWebViewLoad
        {
            get => _ContentWebViewLoad;
            set => SetProperty(ref _ContentWebViewLoad, value);
        }

        private string _HtmlText;
        public string TextHtml
        {
            get => WebViewPasteModel?.HtmlText ?? _HtmlText;
        }

        private string _Source;
        public string Source
        {
            get => WebViewPasteModel?.Source ?? _Source;
        }

        public IWebViewCanvasControlView ControlView { get; set; }

        #endregion

        #region Constructor

        public WebViewCanvasViewModel(IBaseCanvasPreviewControlView view, BaseContentTypeModel contentType)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, contentType, view)
        {
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            if (item is not StorageFile file)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<string> text = await FilesystemOperations.ReadFileText(file);

            if (Mode == WebViewCanvasMode.ReadWebsite)
            {
                _Source = text;
            }
            else
            {
                _HtmlText = text;
            }

            return text;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            ContentWebViewLoad = true;

            if (!_webViewIsLoaded)
            {
                _webViewNeedsUpdate = true;
            }
            else
            {
                if (Mode == WebViewCanvasMode.ReadWebsite)
                {
                    ControlView.NavigateToSource(Source);
                }
                else
                {
                    ControlView.NavigateToHtml(TextHtml);
                }
            }

            return await Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override IPasteModel SetCanvasPasteModel()
        {
            return new WebViewPasteModel(Mode, AssociatedCollection, new StatusCenterOperationReceiver());
        }

        #endregion

        #region Public Helpers

        public void NotifyWebViewLoaded()
        {
            _webViewIsLoaded = true;

            if (_webViewNeedsUpdate)
            {
                _webViewNeedsUpdate = false;

                if (Mode == WebViewCanvasMode.ReadWebsite)
                {
                    ControlView.NavigateToSource(Source);
                }
                else
                {
                    ControlView.NavigateToHtml(TextHtml);
                }
            }
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            this.ControlView?.Dispose();

            this.ControlView = null;
            this.ContentWebViewLoad = false;
        }

        #endregion
    }
}
