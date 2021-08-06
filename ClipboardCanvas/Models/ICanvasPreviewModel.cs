﻿using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using System.Threading;
using System.Collections.Generic;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.DataModels.PastedContentDataModels;

namespace ClipboardCanvas.Models
{
    public interface ICanvasPreviewModel : IReadOnlyCanvasPreviewModel
    {
        event EventHandler<PasteInitiatedEventArgs> OnPasteInitiatedEvent;

        event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;

        event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;

        /// <summary>
        /// Attempts to paste data from provided <see cref="DataPackageView"/> <paramref name="dataPackage"/>
        /// </summary>
        /// <param name="dataPackage">The data to paste</param>
        /// <returns></returns>
        Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to save current data to file in associated collection
        /// </summary>
        /// <returns></returns>
        Task<SafeWrapperResult> TrySaveData();

        /// <summary>
        /// Overrides the reference and pastes the file to the collection
        /// </summary>
        /// <returns></returns>
        Task<SafeWrapperResult> PasteOverrideReference();

        /// <summary>
        /// Opens new canvas
        /// </summary>
        void OpenNewCanvas();

        /// <summary>
        /// Gets suggested actions based on this canvas
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions();
    }
}
