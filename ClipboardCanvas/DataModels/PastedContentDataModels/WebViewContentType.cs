using ClipboardCanvas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.DataModels.PastedContentDataModels
{
    public class WebViewContentType : BasePastedContentTypeDataModel
    {
        public readonly WebViewCanvasMode mode;

        public WebViewContentType(WebViewCanvasMode mode)
        {
            this.mode = mode;
        }

        public override bool Equals(BasePastedContentTypeDataModel other)
        {
            if (other is BasePastedContentTypeDataModel thisOther)
            {
                return true;
            }

            return false;
        }
    }
}
