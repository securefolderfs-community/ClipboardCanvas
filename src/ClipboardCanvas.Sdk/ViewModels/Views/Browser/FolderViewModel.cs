using OwlCore.Storage;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views.Browser
{
    public partial class FolderViewModel(IFolder folder) : BrowserItemViewModel(folder)
    {
        /// <summary>
        /// Gets the items in this folder.
        /// </summary>
        public ObservableCollection<BrowserItemViewModel> Items { get; } = new();

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (Inner is not IFolder folder)
                return;

            Items.Clear();
            await foreach (var item in folder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                Items.Add(item switch
                {
                    IFile file => new FileViewModel(file),
                    IFolder folder2 => new FolderViewModel(folder2),
                    _ => throw new ArgumentOutOfRangeException(nameof(item))
                });
            }

            // TODO: Load thumbnail
        }
    }
}
