using System;

namespace ClipboardCanvas.DataModels.PastedContentDataModels
{
    public sealed class MediaContentType : BaseContentTypeModel
    {
        public TimeSpan savedPosition = TimeSpan.Zero;
    }
}
