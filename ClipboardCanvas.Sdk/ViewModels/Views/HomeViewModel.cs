using ClipboardCanvas.Sdk.ViewModels.Widgets;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class HomeViewModel : ObservableObject, IViewDesignation, IAsyncInitialize
    {
        [ObservableProperty] private string _Title;

        public ObservableCollection<BaseWidgetViewModel> Widgets { get; } = new();

        public HomeViewModel()
        {
            Title = "Clipboard Canvas";
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            Widgets.Add(new CollectionsWidgetViewModel().WithInitAsync(cancellationToken));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void OnAppearing()
        {
        }

        /// <inheritdoc/>
        public void OnDisappearing()
        {
        }
    }
}
