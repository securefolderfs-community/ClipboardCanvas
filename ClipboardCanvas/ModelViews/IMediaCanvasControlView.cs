using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.ModelViews
{
    public interface IMediaCanvasControlView : IDisposable
    {
        TimeSpan Position { get; set; }

        bool IsLoopingEnabled { get; set; }

        double Volume { get; set; }

        Task LoadFromMedia(IStorageFile file);

        Task LoadFromAudio(IStorageFile file);
    }
}
