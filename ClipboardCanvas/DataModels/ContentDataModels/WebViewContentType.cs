using ClipboardCanvas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.DataModels.PastedContentDataModels
{
    public sealed class WebViewContentType : BaseContentTypeModel
    {
        public readonly WebViewCanvasMode mode;

        public WebViewContentType(WebViewCanvasMode mode)
        {
            this.mode = mode;
        }
    }
}
