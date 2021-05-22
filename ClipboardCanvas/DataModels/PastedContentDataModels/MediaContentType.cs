using System;

namespace ClipboardCanvas.DataModels.PastedContentDataModels
{
    public sealed class MediaContentType : BasePastedContentTypeDataModel
    {
        public TimeSpan savedPosition = TimeSpan.Zero;
    }
}
