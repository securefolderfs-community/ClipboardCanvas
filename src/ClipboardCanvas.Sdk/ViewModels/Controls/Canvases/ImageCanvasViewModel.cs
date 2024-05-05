using ClipboardCanvas.Sdk.Enums;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MimeTypes;
using OwlCore.Storage;
using System;
using System.Net.Http.Headers;
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
            PrimaryActions = new()
            {
                new()
                {
                    Name = "Share",
                    Icon = MediaService.GetIcon(IconType.Share),
                    Command = new AsyncRelayCommand(async () =>
                    {
                        // TODO
                        await Console.Out.WriteLineAsync();
                    })
                },
                new()
                {
                    Name = "Open",
                    Icon = MediaService.GetIcon(IconType.Open),
                    Command = new AsyncRelayCommand(async () =>
                    {
                        await Console.Out.WriteLineAsync();
                    })
                },
                new ToggleViewModel()
                {
                    Name = "Edit",
                    Icon = MediaService.GetIcon(IconType.Edit),
                    Command = new AsyncRelayCommand(async () =>
                    {
                        IsEditing = !IsEditing;
                    })
                }
            };

            if (Storable is not IFile file || Image is not null)
                return;

            Image = await MediaService.ReadImageAsync(file, cancellationToken);
        }

        public static async Task<ImageCanvasViewModel> ParseAsync(IClipboardData data, IDataSourceModel sourceModel, CancellationToken cancellationToken)
        {
            var mediaService = Ioc.Default.GetRequiredService<IMediaService>();
            var image = await data.GetImageAsync(cancellationToken);
            if (sourceModel.Source is not IModifiableFolder modifiableFolder)
                return new(image, sourceModel);

            var extension = MimeTypeMap.GetExtension(data.Classification.MimeType, false);
            var file = await modifiableFolder.CreateFileAsync("TODO" + extension, false, cancellationToken);
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
