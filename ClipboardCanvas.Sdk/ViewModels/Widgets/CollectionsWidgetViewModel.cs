using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Extensions;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.Sdk.AppModels;
using OwlCore.Storage.Memory;
using System;
using OwlCore.Storage;
using OwlCore.Storage.SystemIO;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public sealed partial class CollectionsWidgetViewModel : BaseWidgetViewModel, IAsyncInitialize
    {
        private readonly NavigationViewModel _navigationViewModel;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public ObservableCollection<CollectionItemViewModel> Items { get; } = new();

        public CollectionsWidgetViewModel(NavigationViewModel navigationViewModel)
        {
            _navigationViewModel = navigationViewModel;
            Title = "Collections";
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Example, don't use Environment paths nor SystemFolder in the Sdk to get folders!
            var musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            var musicFolder = new SystemFolder(musicPath);

            Items.Add(new(new CollectionModel(musicFolder), _navigationViewModel));
            //Items.Add(new(new MemoryFolder("", "Vacations"), null));
            //Items.Add(new(new MemoryFolder("", "Work"), null));
            //Items.Add(new(new MemoryFolder("", "Media"), null));

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task AddCollectionAsync(CancellationToken cancellationToken)
        {
            var folder = await FileExplorerService.PickFolderAsync(cancellationToken);
            if (folder is not IModifiableFolder modifiableFolder)
                return;

            Items.Add(new CollectionItemViewModel(new CollectionModel(modifiableFolder), _navigationViewModel).WithInitAsync(cancellationToken));
        }
    }
}
