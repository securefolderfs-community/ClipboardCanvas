using ClipboardCanvas.Sdk.Enums;
using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon;
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
    public partial class CodeCanvasViewModel : BaseCanvasViewModel
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

            if (Storable is not IFile file || Text is not null)
                return;

            Text = await file.ReadAllTextAsync(null, cancellationToken);
        }
    }
}
