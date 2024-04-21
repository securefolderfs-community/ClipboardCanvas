﻿using ClipboardCanvas.Shared.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

namespace ClipboardCanvas.WinUI.Imaging
{
    /// <inheritdoc cref="IImage"/>
    public sealed class ImageBitmap : IImage
    {
        public BitmapImage Source { get; }

        public ImageBitmap(BitmapImage source)
        {
            Source = source;
        }

        public ImageBitmap(Stream stream)
        {
            Source = new BitmapImage();
            Source.SetSource(stream.AsRandomAccessStream());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}