using System.Threading;
using System.Threading.Tasks;
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

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
