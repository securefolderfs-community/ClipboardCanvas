using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public sealed partial class TextCanvasViewModel : BaseCanvasViewModel
    {
        [ObservableProperty] private string? _Text;

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
            if (Storable is not IFile file || Text is null)
                return;

            Text = await file.ReadAllTextAsync(null, cancellationToken);
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
