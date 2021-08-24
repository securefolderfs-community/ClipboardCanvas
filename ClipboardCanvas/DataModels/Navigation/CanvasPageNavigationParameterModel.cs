using System;

using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.DataModels.Navigation
{
    public class CanvasPageNavigationParameterModel : BaseDisplayFrameParameterDataModel
    {
        public IDisposable CollectionPreviewIDisposable { get; set; }

        public CanvasPageNavigationParameterModel(ICollectionModel collectionModel, CanvasType canvasType)
            : base(collectionModel, canvasType)
        {
        }
    }
}
