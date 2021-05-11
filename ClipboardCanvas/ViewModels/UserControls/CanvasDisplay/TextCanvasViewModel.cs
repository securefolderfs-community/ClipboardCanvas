﻿using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.EventArguments.CanvasControl;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class TextCanvasViewModel : BasePasteCanvasViewModel
    {
        #region Private Members

        private IDynamicPasteCanvasControlView _view;

        #endregion

        #region Protected Properties

        protected override ICollectionsContainerModel AssociatedContainer => _view?.CollectionContainer;

        #endregion

        #region Public Properties

        private bool _ContentTextLoad;
        public bool ContentTextLoad
        {
            get => _ContentTextLoad;
            private set => SetProperty(ref _ContentTextLoad, value);
        }

        private string _ContentText;
        public string ContentText
        {
            get => _ContentText;
            set => SetProperty(ref _ContentText, value);
        }

        public static List<string> Extensions => new List<string>() {
            ".txt"
        };

        #endregion

        #region Constructor

        public TextCanvasViewModel(IDynamicPasteCanvasControlView view)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter) // TODO: Use custom exception reporter
        {
            this._view = view;
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(
                           () => dataPackage.GetTextAsync().AsTask());

            if (!text)
            {
                Debugger.Break();
                return (SafeWrapperResult)text;
            }

            _ContentText = text;

            return (SafeWrapperResult)text;
        }

        protected override async Task<SafeWrapperResult> SetData(IStorageFile file)
        {
            SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(async () => await FileIO.ReadTextAsync(file));

            this._ContentText = text;

            return text;
        }

        protected override async Task<SafeWrapper<StorageFile>> TrySetFileWithExtension()
        {
            SafeWrapper<StorageFile> file;

            file = await AssociatedContainer.GetEmptyFileToWrite(".txt");

            return file;
        }

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                await FileIO.WriteTextAsync(sourceFile, ContentText);
            }, errorReporter);

            return result;
        }

        protected override Task<SafeWrapperResult> TryFetchDataToView()
        {
            ContentTextLoad = true;

            OnPropertyChanged(nameof(ContentText));

            return Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override bool CanPasteAsReference()
        {
            return sourceFile != null;
        }

        #endregion

        #region Public Helpers

        public static async Task<bool> CanLoadAsText(StorageFile file)
        {
            // Check if exceeds maximum fileSize or is zero
            long fileSize = await file.GetFileSize();
            if (fileSize > Constants.CanvasContent.FALLBACK_TEXTLOAD_MAX_FILESIZE || fileSize == 0L)
            {
                return false;
            }

            try
            {
                // Check if file is binary
                string text = await FileIO.ReadTextAsync(file);
                if (text.Contains("\0\0\0\0"))
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #endregion
    }
}
