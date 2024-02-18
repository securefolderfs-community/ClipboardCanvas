using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.UI.Helpers;
using ClipboardCanvas.UI.ServiceImplementation;
using ClipboardCanvas.WinUI.ServiceImplementation;
using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using OwlCore.Storage.SystemIO;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.WinUI.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class WindowsLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        public override Task<IServiceCollection> ConfigureAsync(CancellationToken cancellationToken = default)
        {
#if !DEBUG
            try
            {
                // Start AppCenter
                var appCenterKey = ApiKeys.GetAppCenterKey();
                if (!string.IsNullOrEmpty(appCenterKey) || !AppCenter.Configured)
                    AppCenter.Start(appCenterKey, typeof(Analytics), typeof(Crashes));
            }
            catch (Exception)
            {
            }
#endif

#if UNPACKAGED
            var settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), ClipboardCanvas.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
#else
            var settingsFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
#endif

            var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
            return Task.FromResult(ConfigureServices(settingsFolder));
        }

        /// <inheritdoc/>
        protected override IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return new ServiceCollection()
                    
                    // Singleton services
                    .AddSingleton<ICollectionPersistenceService, CollectionPersistenceService>(_ => new(settingsFolder))
                    .AddSingleton<IFileExplorerService, WindowsFileExplorerService>()
                    .AddSingleton<IStorageService, SystemStorageService>()
                    .AddSingleton<ISettingsService, SettingsService>(_ => new(settingsFolder))

                    // Transient services
                    .AddTransient<INavigationService, WindowsNavigationService>()
                    ;
        }
    }
}
