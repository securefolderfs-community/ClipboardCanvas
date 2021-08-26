using System;
using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.EventArguments.CanvasControl
{
    public abstract class BaseCanvasControlEventArgs : EventArgs
    {
        public readonly ICollectionModel collectionModel;

        public BaseCanvasControlEventArgs(ICollectionModel collectionModel)
        {
            this.collectionModel = collectionModel;
        }
    }

    public class ContentLoadedEventArgs : EventArgs
    {
        public readonly BaseContentTypeModel contentType;

        public readonly bool isFilled;

        public readonly bool pastedAsReference;

        public readonly bool canPasteReference;

        public ContentLoadedEventArgs(BaseContentTypeModel contentType, bool isFilled, bool pastedAsReference, bool canPasteReference)
        {
            this.contentType = contentType;
            this.isFilled = isFilled;
            this.pastedAsReference = pastedAsReference;
            this.canPasteReference = canPasteReference;
        }
    }

    public class ContentStartedLoadingEventArgs : EventArgs
    {
        public readonly BaseContentTypeModel contentType;

        public ContentStartedLoadingEventArgs(BaseContentTypeModel contentType)
        {
            this.contentType = contentType;
        }
    }

    public class PasteInitiatedEventArgs : BaseCanvasControlEventArgs
    {
        public readonly bool pasteInNewCanvas;

        public readonly DataPackageView forwardedDataPackage;

        public readonly BaseContentTypeModel contentType;

        public PasteInitiatedEventArgs(bool pasteInNewCanvas, DataPackageView forwardedDataPackage, BaseContentTypeModel contentType, ICollectionModel collectionModel)
            : base(collectionModel)
        {
            this.pasteInNewCanvas = pasteInNewCanvas;
            this.forwardedDataPackage = forwardedDataPackage;
            this.contentType = contentType;
        }
    }

    public class FileCreatedEventArgs : EventArgs
    {
        public readonly BaseContentTypeModel contentType;

        public readonly IStorageItem item;

        public FileCreatedEventArgs(BaseContentTypeModel contentType, IStorageItem item)
        {
            this.contentType = contentType;
            this.item = item;
        }
    }

    public class FileModifiedEventArgs : EventArgs
    {
        public readonly IStorageItem item;

        public FileModifiedEventArgs(IStorageItem item)
        {
            this.item = item;
        }
    }

    public class FileDeletedEventArgs : BaseCanvasControlEventArgs
    {
        public readonly IStorageItem item;

        public FileDeletedEventArgs(IStorageItem item, ICollectionModel collectionModel)
            : base(collectionModel)
        {
            this.item = item;
        }
    }

    public class ErrorOccurredEventArgs : EventArgs
    {
        public readonly SafeWrapperResult error;

        public readonly string errorMessage;

        public readonly bool showEmptyCanvas;

        public readonly TimeSpan errorMessageAutoHide;

        public ErrorOccurredEventArgs(SafeWrapperResult error, string errorMessage, bool showEmptyCanvas = true)
            : this(error, errorMessage, TimeSpan.Zero, showEmptyCanvas)
        {
        }

        public ErrorOccurredEventArgs(SafeWrapperResult error, string errorMessage, TimeSpan errorMessageAutoHide, bool showEmptyCanvas = true)
        {
            this.error = error;
            this.errorMessage = errorMessage;
            this.errorMessageAutoHide = errorMessageAutoHide;
            this.showEmptyCanvas = showEmptyCanvas;
        }
    }

    public class ProgressReportedEventArgs : EventArgs
    {
        public readonly float value;

        public readonly BaseContentTypeModel contentType;

        public ProgressReportedEventArgs(float value, BaseContentTypeModel contentType)
        {
            this.value = value;
            this.contentType = contentType;
        }
    }

    public class TipTextUpdateRequestedEventArgs : EventArgs
    {
        public readonly string infoText;

        public readonly TimeSpan tipShowDelay;

        public TipTextUpdateRequestedEventArgs(string infoText)
            : this(infoText, TimeSpan.Zero)
        {
        }

        public TipTextUpdateRequestedEventArgs(string infoText, TimeSpan tipShowDelay)
        {
            this.infoText = infoText;
            this.tipShowDelay = tipShowDelay;
        }
    }

    public class OpenNewCanvasRequestedEventArgs : EventArgs
    {
        public readonly CanvasType canvasType;

        public OpenNewCanvasRequestedEventArgs()
        {
            this.canvasType = CanvasHelpers.GetDefaultCanvasType();
        }

        public OpenNewCanvasRequestedEventArgs(CanvasType canvasType)
        {
            this.canvasType = canvasType;
        }
    }
}
