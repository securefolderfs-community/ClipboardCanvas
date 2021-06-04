using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.ModelViews
{
    public interface IMediaCanvasControlView
    {
        TimeSpan Position { get; set; }

        bool IsLoopingEnabled { get; set; }

        public double Volume { get; set; }
    }
}
