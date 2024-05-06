using ClipboardCanvas.Sdk.Enums;
using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public sealed partial class TextCanvasViewModel : BaseCanvasViewModel, IPersistable
    {
        [ObservableProperty] private string? _Text;

        private IMediaService MediaService { get; } = Ioc.Default.GetRequiredService<IMediaService>();

        public TextCanvasViewModel(string text, IDataSourceModel sourceModel)
            : base(sourceModel)
        {
            Text = text;
        }

        public TextCanvasViewModel(IFile textFile, IDataSourceModel sourceModel)
            : base(textFile, sourceModel)
        {
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Move to UI
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
                    Command = OpenCommand
                },
                new ToggleViewModel()
                {
                    Name = "Edit",
                    Icon = MediaService.GetIcon(IconType.Edit),
                    Command = EditCommand
                }
            };

            if (Storable is not IFile file || Text is not null)
                return;

            Text = await file.ReadAllTextAsync(null, cancellationToken);
        }

        /// <inheritdoc/>
        [RelayCommand]
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            if (Storable is not IFile file)
                return;

            // Set WasAltered beforehand to account for any updates
            WasAltered = false;

            if (Text is null)
                return;

            await file.WriteAllTextAsync(Text, null, cancellationToken);
        }

        public static async Task<TextCanvasViewModel> ParseAsync(IClipboardData data, IDataSourceModel sourceModel, CancellationToken cancellationToken)
        {
            var text = await data.GetTextAsync(cancellationToken);
            if (sourceModel.Source is not IModifiableFolder modifiableFolder)
                return new(text, sourceModel);

            var file = await modifiableFolder.CreateFileAsync("TODO", false, cancellationToken);
            await file.WriteAllTextAsync(text, null, cancellationToken);
            return new(file, sourceModel)
            {
                Text = text,
            };
        }
    }
}
