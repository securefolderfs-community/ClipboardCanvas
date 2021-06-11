using System;

namespace ClipboardCanvas.ModelViews
{
    public interface IMediaCanvasControlView
    {
        TimeSpan Position { get; set; }

        bool IsLoopingEnabled { get; set; }

        double Volume { get; set; }
    }
}
