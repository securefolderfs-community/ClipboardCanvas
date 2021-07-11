using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Core;
using Windows.Storage;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.UnsafeNative;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class WebViewCanvasViewModel : BaseCanvasViewModel
    {
        #region Private Members

        private readonly IDynamicCanvasControlView _view;

        private readonly WebViewCanvasMode _mode;

        private bool _webViewNeedsUpdate;

        private bool _webViewIsLoaded;

        #endregion

        #region Protected Members

        protected override ICollectionModel AssociatedCollection => _view?.CollectionModel;

        #endregion

        #region Constructor

        public WebViewCanvasViewModel(IDynamicCanvasControlView view, WebViewCanvasMode mode)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, new WebViewContentType(mode))
        {
            this._view = view;
            this._mode = mode;
        }

        #endregion

        #region Public Properties

        public static List<string> Extensions => new List<string>() {
            ".html", ".htm", Constants.FileSystem.WEBSITE_LINK_FILE_EXTENSION,
        };

        private bool _ContentWebViewLoad;
        public bool ContentWebViewLoad
        {
            get => _ContentWebViewLoad;
            set => SetProperty(ref _ContentWebViewLoad, value);
        }

        public string TextHtml { get; private set; }

        public string Source { get; private set; }

        public IWebViewCanvasControlView ControlView { get; set; }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetDataInternal(DataPackageView dataPackage)
        {
            // We override the function there because if clipboard contains link, checking dataPackage.Contains() is true
            // for some reason for both StandardDataFormats.Text and StandardDataFormats.StorageItems -> Since we know it's the text we must check it first
            // to avoid exceptions and only then if not text, StandardDataFormats.StorageItems

            if (dataPackage.Contains(StandardDataFormats.Text))
            {
                // Check for text
                return await SetData(dataPackage);
            }
            else
            {
                // Check for storage items
                return await base.SetDataInternal(dataPackage);
            }
        }

        protected override async Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(() => dataPackage.GetTextAsync().AsTask());

            if (_mode == WebViewCanvasMode.ReadWebsite)
            {
                Source = text;
            }
            else
            {
                TextHtml = text;
            }

            return text;
        }

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            SafeWrapperResult result;

            if (associatedFile == null)
            {
                return ItemIsNotAFileResult;
            }

            if (_mode == WebViewCanvasMode.ReadWebsite)
            {
                result = await SafeWrapperRoutines.SafeWrapAsync(() => FileIO.WriteTextAsync(associatedFile, Source).AsTask());
            }
            else
            {
                result = await SafeWrapperRoutines.SafeWrapAsync(() => FileIO.WriteTextAsync(associatedFile, TextHtml).AsTask());
            }

            return result;
        }

        protected override async Task<SafeWrapperResult> SetData(IStorageItem item)
        {
            StorageFile file = item as StorageFile;
            if (file == null)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<string> text = SafeWrapperRoutines.SafeWrap(() => UnsafeNativeHelpers.ReadStringFromFile(file.Path));

            if (_mode == WebViewCanvasMode.ReadWebsite)
            {
                Source = text;
            }
            else
            {
                TextHtml = text;
            }

            return await Task.FromResult(text);
        }

        protected override async Task<SafeWrapper<StorageFile>> TrySetFileWithExtension()
        {
            SafeWrapper<StorageFile> file;

            if (_mode == WebViewCanvasMode.ReadWebsite)
            {
                file = await AssociatedCollection.GetOrCreateNewCollectionFileFromExtension(Constants.FileSystem.WEBSITE_LINK_FILE_EXTENSION);
            }
            else
            {
                file = await AssociatedCollection.GetOrCreateNewCollectionFileFromExtension(".html");
            }

            return file;
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
                if (_mode == WebViewCanvasMode.ReadWebsite)
                {
                    ControlView.NavigateToSource(Source);
                }
                else
                {
                    ControlView.NavigateToHtml(TextHtml);
                }
            }

            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        #endregion

        #region Public Helpers

        public void NotifyWebViewLoaded()
        {
            _webViewIsLoaded = true;

            if (_webViewNeedsUpdate)
            {
                _webViewNeedsUpdate = false;

                if (_mode == WebViewCanvasMode.ReadWebsite)
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

            this.ContentWebViewLoad = false;
        }

        #endregion
    }
}
