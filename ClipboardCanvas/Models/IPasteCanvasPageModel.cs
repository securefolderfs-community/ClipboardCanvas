using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.Models
{
    public interface IPasteCanvasPageModel : IDisposable
    {
        ICanvasPreviewModel PasteCanvasModel { get; }

        Task SetTipText(string text);

        Task SetTipText(string text, TimeSpan tipShowDelay);
    }
}
