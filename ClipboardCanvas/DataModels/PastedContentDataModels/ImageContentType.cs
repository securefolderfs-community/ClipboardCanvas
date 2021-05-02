using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.DataModels.PastedContentDataModels
{
    /// <summary>
    /// Used for images that are directly copied from clipboard
    /// </summary>
    public sealed class ImageContentType : BasePastedContentTypeDataModel
    {
        public override bool Equals(BasePastedContentTypeDataModel other)
        {
            if (other is ImageContentType thisOther)
            {
                return true;
            }

            return false;
        }
    }
}
