using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public sealed partial class TextCanvasViewModel : BaseCanvasViewModel
    {
        [ObservableProperty] private string? _Text;

        public TextCanvasViewModel(string text, IDataSourceModel collectionModel)
            : base(collectionModel)
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
            if (Storable is not IFile file)
                return;

            Text = await file.ReadAllTextAsync(null, cancellationToken);
        }
    }
}
