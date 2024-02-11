using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public sealed partial class CollectionItemViewModel : ObservableObject
    {
        public IFolder Folder { get; }

        [ObservableProperty] private string? _Name;
        [ObservableProperty] private IImage? _Icon;

        public CollectionItemViewModel(IFolder folder, string? name = null)
        {
            Folder = folder;
            Name = name ?? folder.Name;
        }
    }
}
