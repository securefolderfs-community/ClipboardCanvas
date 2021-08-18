using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Core;
using System.Threading;
using Windows.Storage;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Contexts.Operations;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class MediaCanvasViewModel : BaseCanvasViewModel
    {
        #region Properties

        private MediaContentType MediaContentType => contentType as MediaContentType;

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

        public static List<string> Extensions => new List<string>() {
            // Video
            ".mp4", ".webm", ".ogg", ".mov", ".qt", ".mp4", ".m4v", ".mp4v", ".3g2", ".3gp2", ".3gp", ".3gpp", ".mkv",
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
            ContentMediaLoad = true;
            ContentMedia = MediaSource.CreateFromStorageFile(await sourceFile);

            return await Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override async Task OnReferencePasted()
        {
            // Change the source
            _ContentMedia = MediaSource.CreateFromStorageFile(await sourceFile);
        }

        protected override IPasteModel SetCanvasPasteModel()
        {
            return new MediaPasteModel(associatedCollection, new StatusCenterOperationReceiver());
        }

        #endregion

        #region Public Helpers

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
            ContentMedia = null;
        }

        #endregion
    }
}
