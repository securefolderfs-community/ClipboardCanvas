using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views.Settings
{
    public sealed partial class AboutSettingsViewModel : ObservableObject, IViewDesignation
    {
        [ObservableProperty] private string? _AppVersion;
        [ObservableProperty] private string? _Title;

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        private IClipboardService ClipboardService { get; } = Ioc.Default.GetRequiredService<IClipboardService>();

        private IStorageService StorageService { get; } = Ioc.Default.GetRequiredService<IStorageService>();

        public AboutSettingsViewModel()
        {
            AppVersion = $"{ApplicationService.AppVersion} ({ApplicationService.Platform})";
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task CopyAppVersionAsync(CancellationToken cancellationToken)
        {
            await ClipboardService.SetTextAsync(AppVersion ?? string.Empty, cancellationToken);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task CopySystemVersionAsync(CancellationToken cancellationToken)
        {
            await ClipboardService.SetTextAsync(ApplicationService.GetSystemVersion(), cancellationToken);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenGitHubRepositoryAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS"));
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenPrivacyPolicyAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS/blob/master/PRIVACY.md"));
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task OpenLogLocationAsync(CancellationToken cancellationToken)
        {
            var appFolder = await StorageService.GetAppFolderAsync(cancellationToken);
            await FileExplorerService.OpenInFileExplorerAsync(appFolder);
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
