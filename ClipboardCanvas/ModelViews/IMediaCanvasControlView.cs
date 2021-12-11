using System;
using Windows.Storage;

namespace ClipboardCanvas.ModelViews
{
    public interface IMediaCanvasControlView : IDisposable
    {
        TimeSpan Position { get; set; }

        bool IsLoopingEnabled { get; set; }

        double Volume { get; set; }

        void LoadFromMedia(IStorageFile file);

        void LoadFromAudio(IStorageFile file);
    }
}
