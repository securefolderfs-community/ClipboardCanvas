using System;
using Windows.Media.Core;
using IMediaSource = ClipboardCanvas.Sdk.Models.IMediaSource;

namespace ClipboardCanvas.WinUI.AppModels
{
    internal sealed class VideoSource : IMediaSource
    {
        private readonly IDisposable? _sourceDisposable;

        public MediaSource MediaSource { get; }

        public VideoSource(MediaSource mediaSource, IDisposable? sourceDisposable)
        {
            MediaSource = mediaSource;
            _sourceDisposable = sourceDisposable;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _sourceDisposable?.Dispose();
            MediaSource.Dispose();
        }
    }
}
