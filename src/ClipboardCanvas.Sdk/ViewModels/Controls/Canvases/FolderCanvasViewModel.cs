using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.ViewModels.Views.Browser;
using OwlCore.Storage;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public sealed partial class FolderCanvasViewModel : BaseCanvasViewModel
    {
        public ObservableCollection<BrowserItemViewModel> Items { get; }

        public FolderCanvasViewModel(IFolder folder, IDataSourceModel sourceModel)
            : base(folder, sourceModel)
        {
            Items = new();
        }

        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (Storable is not IFolder folder)
                return;

            var folderViewModel = new FolderViewModel(folder);
            _ = folderViewModel.InitAsync(cancellationToken);
            Items.Add(folderViewModel);
        }
    }
}
