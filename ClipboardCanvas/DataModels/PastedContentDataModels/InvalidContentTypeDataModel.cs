using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.DataModels.PastedContentDataModels
{
    public sealed class InvalidContentTypeDataModel : BasePastedContentTypeDataModel
    {
        public readonly bool needsReinitialization;

        public InvalidContentTypeDataModel(bool needsReinitialization)
        {
            this.needsReinitialization = needsReinitialization;
        }
    }
}
