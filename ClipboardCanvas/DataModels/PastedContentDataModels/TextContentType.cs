using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.DataModels.PastedContentDataModels
{
    public sealed class TextContentType : BasePastedContentTypeDataModel
    {
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
