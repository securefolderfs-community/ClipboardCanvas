using System.Collections.Generic;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Views.Browser;
using ClipboardCanvas.Shared.Extensions;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using OwlCore.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls.Canvases
{
    public sealed partial class FolderCanvasControl : UserControl
    {
        public FolderCanvasControl()
        {
            InitializeComponent();
        }

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        private async void TreeItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is not TreeViewItem { DataContext: BrowserItemViewModel itemViewModel } treeItem)
                return;

            if (itemViewModel is FolderViewModel folderViewModel)
            {
                if (folderViewModel.Items.IsEmpty())
                    await folderViewModel.InitAsync();

                //treeItem.IsExpanded = !treeItem.IsExpanded;
            }
            else if (itemViewModel is FileViewModel { Inner: IFile file })
                await ApplicationService.LaunchHandlerAsync(file, default);

        }

        public IList<BrowserItemViewModel>? ItemsSource
        {
            get => (IList<BrowserItemViewModel>?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IList<BrowserItemViewModel>), typeof(FolderCanvasControl), new PropertyMetadata(null));
    }
}
