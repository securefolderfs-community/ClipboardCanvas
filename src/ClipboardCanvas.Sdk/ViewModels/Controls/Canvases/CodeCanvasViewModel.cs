using ClipboardCanvas.Sdk.Enums;
using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Menu;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public partial class CodeCanvasViewModel : BaseCanvasViewModel, IPersistable
    {
        [ObservableProperty] private string? _Text;
        [ObservableProperty] private string? _Language;

        private IMediaService MediaService { get; } = Ioc.Default.GetRequiredService<IMediaService>();

        public CodeCanvasViewModel(IFile codeFile, IDataSourceModel sourceModel)
            : base(codeFile, sourceModel)
        {
            Language = Path.GetExtension(codeFile.Id);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            PrimaryActions = new()
            {
                new MenuActionViewModel()
                {
                    Name = "Share",
                    Icon = MediaService.GetIcon(IconType.Share),
                    Command = new AsyncRelayCommand(async () =>
                    {
                        // TODO
                        await Console.Out.WriteLineAsync();
                    })
                },
                new MenuActionViewModel()
                {
                    Name = "Open",
                    Icon = MediaService.GetIcon(IconType.Open),
                    Command = OpenCommand
                },
                new MenuToggleViewModel()
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
    }
}
