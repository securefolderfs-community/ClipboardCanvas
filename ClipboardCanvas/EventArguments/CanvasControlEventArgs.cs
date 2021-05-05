using System;
using Windows.Storage;
using ClipboardCanvas.Models;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using Windows.ApplicationModel.DataTransfer;

namespace ClipboardCanvas.EventArguments
{
    public class OpenOpenNewCanvasRequestedEventArgs : EventArgs
    {
        public OpenOpenNewCanvasRequestedEventArgs()
        {
            // TODO: Maybe add something useful there?
        }
    }

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

    public class PasteRequestedEventArgs : EventArgs
    {
        public readonly bool hasContent;

        public readonly DataPackageView forwardedDataPackage;

        public PasteRequestedEventArgs(bool hasContent, DataPackageView forwardedDataPackage)
        {
            this.hasContent = hasContent;
            this.forwardedDataPackage = forwardedDataPackage;
        }
    }

    public class FileCreatedEventArgs : EventArgs
    {
        public readonly ICollectionsContainerModel associatedContainer;

        public readonly BasePastedContentTypeDataModel contentType;

        public readonly StorageFile file;

        public FileCreatedEventArgs(ICollectionsContainerModel associatedContainer, BasePastedContentTypeDataModel contentType, StorageFile file)
        {
            this.associatedContainer = associatedContainer;
            this.contentType = contentType;
            this.file = file;
        }
    }

    public class FileModifiedEventArgs : EventArgs
    {
        public readonly ICollectionsContainerModel associatedContainer;

        public readonly IStorageFile file;

        public FileModifiedEventArgs(IStorageFile file, ICollectionsContainerModel associatedContainer)
        {
            this.file = file;
            this.associatedContainer = associatedContainer;
        }
    }

    public class FileDeletedEventArgs : EventArgs
    {
        public readonly ICollectionsContainerModel associatedContainer;

        public readonly IStorageFile file;

        public FileDeletedEventArgs(IStorageFile file, ICollectionsContainerModel associatedContainer)
        {
            this.file = file;
            this.associatedContainer = associatedContainer;
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
}
