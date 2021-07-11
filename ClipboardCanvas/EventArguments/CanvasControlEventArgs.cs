using System;
using Windows.Storage;
using ClipboardCanvas.Models;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using Windows.ApplicationModel.DataTransfer;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.EventArguments.CanvasControl
{
    public class ContentLoadedEventArgs : EventArgs
    {
        public readonly BasePastedContentTypeDataModel contentDataModel;

        public readonly bool isFilled;

        public readonly bool pastedByReference;

        public ContentLoadedEventArgs(BasePastedContentTypeDataModel contentDataModel, bool isFilled, bool pastedByReference)
        {
            this.contentDataModel = contentDataModel;
            this.isFilled = isFilled;
            this.pastedByReference = pastedByReference;
        }
    }

    public class ContentStartedLoadingEventArgs : EventArgs
    {
        public readonly BasePastedContentTypeDataModel contentDataModel;

        public ContentStartedLoadingEventArgs(BasePastedContentTypeDataModel contentDataModel)
        {
            this.contentDataModel = contentDataModel;
        }
    }

    public class PasteInitiatedEventArgs : EventArgs
    {
        public readonly bool isFilled;

        public readonly DataPackageView forwardedDataPackage;

        public PasteInitiatedEventArgs(bool isFilled, DataPackageView forwardedDataPackage)
        {
            this.isFilled = isFilled;
            this.forwardedDataPackage = forwardedDataPackage;
        }
    }

    public class FileCreatedEventArgs : EventArgs
    {
        public readonly BasePastedContentTypeDataModel contentType;

        public readonly IStorageItem item;

        public FileCreatedEventArgs(BasePastedContentTypeDataModel contentType, IStorageItem item)
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

    public class FileDeletedEventArgs : EventArgs
    {
        public readonly IStorageItem item;

        public FileDeletedEventArgs(IStorageItem item)
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
        public OpenNewCanvasRequestedEventArgs()
        {
        }
    }
}
