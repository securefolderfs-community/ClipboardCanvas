using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public partial class ImageCanvasViewModel : BaseCanvasViewModel
    {
        [ObservableProperty] private IImage? _Image;

        private IMediaService MediaService { get; } = Ioc.Default.GetRequiredService<IMediaService>();

        public ImageCanvasViewModel(IImage image, IDataSourceModel sourceModel)
            : base(sourceModel)
        {
            Image = image;
        }

        public ImageCanvasViewModel(IFile imageFile, IDataSourceModel sourceModel)
            : base(imageFile, sourceModel)
        {
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (Storable is not IFile file)
                return;

            Image = await MediaService.ReadImageAsync(file, cancellationToken);
        }

        public static async Task<ImageCanvasViewModel> ParseAsync(IClipboardData data, IDataSourceModel sourceModel, CancellationToken cancellationToken)
        {
            var mediaService = Ioc.Default.GetRequiredService<IMediaService>();
            var image = await data.GetImageAsync(cancellationToken);
            var file = await sourceModel.CreateFileAsync("TODO", false, cancellationToken);

            await mediaService.SaveImageAsync(image, file, cancellationToken);
            return new(file, sourceModel)
            {
                Image = image
            };
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Image?.Dispose();
        }
    }
}
