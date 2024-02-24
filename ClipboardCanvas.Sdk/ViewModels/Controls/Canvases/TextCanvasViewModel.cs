using ClipboardCanvas.Sdk.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public sealed partial class TextCanvasViewModel : BaseCanvasViewModel
    {
        [ObservableProperty] private string? _Text;

        public TextCanvasViewModel(ICanvasSourceModel collectionModel)
            : base(collectionModel)
        {
        }
    }
}
