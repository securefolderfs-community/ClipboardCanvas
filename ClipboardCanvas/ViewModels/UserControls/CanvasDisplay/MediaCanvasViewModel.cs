using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Core;
using System.Threading;
using Windows.Storage;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Contexts.Operations;
using System.IO;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class MediaCanvasViewModel : BaseCanvasViewModel
    {
        private bool _webViewNeedsUpdate;

        private bool _webViewLoaded;

        #region Properties

        private MediaContentType MediaContentType => ContentType as MediaContentType;

        private MediaPasteModel MediaPasteModel => canvasPasteModel as MediaPasteModel;

        private TimeSpan Position
        {
            get => ControlView.Position;
            set => ControlView.Position = value;
        }

        private bool IsLoopingEnabled
        {
            get => ControlView.IsLoopingEnabled;
            set => ControlView.IsLoopingEnabled = value;
        }

        private double Volume
        {
            get => ControlView.Volume;
            set => ControlView.Volume = value;
        }

        public static List<string> Extensions = new List<string>() {
            // Video
            ".mp4", ".webm", ".ogg", ".mov", ".qt", ".mp4", ".m4v", ".mp4v", ".3g2", ".3gp2", ".3gp", ".3gpp", ".mkv",
            // Audio
            ".mp3", ".m4a", ".wav", ".wma", ".aac", ".adt", ".adts", ".cda",
        };

        public static List<string> AudioExtensions = new List<string>()
        {
            // Audio
            ".mp3", ".m4a", ".wav", ".wma", ".aac", ".adt", ".adts", ".cda",
        };

        private MediaSource _ContentMedia;
        public MediaSource ContentMedia
        {
            get => _ContentMedia;
            set => SetProperty(ref _ContentMedia, value);
        }

        private bool _ContentMediaLoad;
        public bool ContentMediaLoad
        {
            get => _ContentMediaLoad;
            set => SetProperty(ref _ContentMediaLoad, value);
        }

        private bool _ContentWebViewLoad;
        public bool ContentWebViewLoad
        {
            get => _ContentWebViewLoad;
            set => SetProperty(ref _ContentWebViewLoad, value);
        }

        public IMediaCanvasControlView ControlView { get; set; }

        #endregion

        #region Constructor

        public MediaCanvasViewModel(IBaseCanvasPreviewControlView view, BaseContentTypeModel contentType)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, contentType, view)
        {
        }

        #endregion

        #region Override

        public override async Task<SafeWrapperResult> TryLoadExistingData(CanvasItem canvasItem, BaseContentTypeModel contentType, CancellationToken cancellationToken)
        {
            SafeWrapperResult result = await base.TryLoadExistingData(canvasItem, contentType, cancellationToken);

            if (result)
            {
                UpdateMediaControl();
            }

            return result;
        }

        protected override Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            return Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            ContentWebViewLoad = true;
            ContentMediaLoad = true;
            //ContentMedia = MediaSource.CreateFromStorageFile(await SourceFile);

            if (!_webViewLoaded)
            {
                _webViewNeedsUpdate = true;
            }
            else
            {
                await UpdateWebViewContent(true);
            }

            return SafeWrapperResult.SUCCESS;
        }

        protected override async Task OnReferencePasted()
        {
            await base.OnReferencePasted();

            // Change the source
            _ContentMedia = MediaSource.CreateFromStorageFile(await SourceFile);
        }

        protected override IPasteModel SetCanvasPasteModel()
        {
            return new MediaPasteModel(CanvasItemReceiver ?? AssociatedCollection, new StatusCenterOperationReceiver());
        }

        #endregion

        #region Public Helpers

        public async Task NotifyWebViewLoaded()
        {
            _webViewLoaded = true;

            await UpdateWebViewContent();
        }

        private async Task UpdateWebViewContent(bool force = false)
        {
            if (_webViewLoaded && (_webViewNeedsUpdate || force))
            {
                string ext = Path.GetExtension((await SourceFile).Path).ToLower();
                if (AudioExtensions.Contains(ext))
                {
                    ControlView.LoadFromAudio(await SourceFile);
                }
                else
                {
                    ControlView.LoadFromMedia(await SourceFile);
                }
            }
        }

        public void UpdateMediaControl()
        {
            if (ControlView != null && MediaContentType != null)
            {
                this.Position = MediaContentType.savedPosition;
                this.IsLoopingEnabled = CanvasSettings.MediaCanvas_IsLoopingEnabled;
                this.Volume = CanvasSettings.MediaCanvas_UniversalVolume;
            }
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            if (ControlView != null && ContentMediaLoad)
            {
                if (collectionItemViewModel?.ContentType is MediaContentType mediaContentType)
                {
                    mediaContentType.savedPosition = Position;
                }

                CanvasSettings.MediaCanvas_IsLoopingEnabled = IsLoopingEnabled;
                CanvasSettings.MediaCanvas_UniversalVolume = Volume;
            }

            base.Dispose();

            ContentMedia?.Dispose();
            ControlView?.Dispose();
            ContentMediaLoad = false;
            ContentMedia = null;
            ContentWebViewLoad = false;
        }

        #endregion
    }
}
