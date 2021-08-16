using System;
using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.DataModels.PastedContentDataModels;
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
        public readonly BaseContentTypeModel contentDataModel;

        public readonly bool isFilled;

        public readonly bool pastedAsReference;

        public readonly bool canPasteReference;

        public readonly bool isInfiniteCanvas;

        public ContentLoadedEventArgs(BaseContentTypeModel contentDataModel, bool isFilled, bool pastedAsReference, bool canPasteReference, bool isInfiniteCanvas = false)
        {
            this.contentDataModel = contentDataModel;
            this.isFilled = isFilled;
            this.pastedAsReference = pastedAsReference;
            this.canPasteReference = canPasteReference;
            this.isInfiniteCanvas = isInfiniteCanvas;
        }
    }

    public class ContentStartedLoadingEventArgs : EventArgs
    {
        public readonly BaseContentTypeModel contentDataModel;

        public ContentStartedLoadingEventArgs(BaseContentTypeModel contentDataModel)
        {
            this.contentDataModel = contentDataModel;
        }
    }

    public class PasteInitiatedEventArgs : BaseCanvasControlEventArgs
    {
        public readonly bool pasteInNewCanvas;

        public readonly DataPackageView forwardedDataPackage;

        public PasteInitiatedEventArgs(bool pasteInNewCanvas, DataPackageView forwardedDataPackage, ICollectionModel collectionModel)
            : base(collectionModel)
        {
            this.pasteInNewCanvas = pasteInNewCanvas;
            this.forwardedDataPackage = forwardedDataPackage;
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

        public readonly bool showErrorImage;

        public ErrorOccurredEventArgs(SafeWrapperResult error, string errorMessage, bool showErrorImage = true)
        {
            this.error = error;
            this.errorMessage = errorMessage;
            this.showErrorImage = showErrorImage;
        }
    }

    public class ProgressReportedEventArgs : EventArgs
    {
        public readonly float value;

        public readonly bool isIndeterminate;

        public readonly CanvasPageProgressType progressType;

        public ProgressReportedEventArgs(float value, bool isIndeterminate, CanvasPageProgressType progressType)
        {
            this.value = value;
            this.isIndeterminate = isIndeterminate;
            this.progressType = progressType;
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
