using ClipboardCanvas.Helpers.SafetyHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace ClipboardCanvas.Models
{
    public interface IDynamicPasteCanvasModel : IPasteCanvasEventsModel
    {
        IPasteCanvasModel PasteCanvasModel { get; }
    }
}
