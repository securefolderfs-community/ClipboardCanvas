using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.Models
{
    public interface IPasteCanvasPageModel : IDisposable
    {
        IPasteCanvasModel PasteCanvasModel { get; }

        void SetTipText(string text);
    }
}
