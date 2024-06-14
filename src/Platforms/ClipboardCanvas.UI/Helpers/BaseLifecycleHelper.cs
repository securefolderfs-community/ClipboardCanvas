using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.UI.Helpers
{
    public abstract class BaseLifecycleHelper
    {
        public virtual Task<IServiceCollection> ConfigureAsync(CancellationToken cancellationToken = default)
        {
            var settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), Constants.FileNames.SETTINGS_FOLDER_NAME);
            var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
            
            return Task.FromResult(ConfigureServices(settingsFolder));
        }

        /// <summary>
        /// Logs the exception to a common output.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public virtual void LogException(Exception? ex) // TODO: Maybe use ILogger
        {
            var formattedException = ExceptionHelpers.FormatException(ex);
            Debug.WriteLine(formattedException);

            // Please check the "Output Window" for exception details (On Visual Studio, go to View -> Output Window or Ctr+Alt+O)
            Debugger.Break();
        }

        /// <summary>
        /// Configures services for the app.
        /// </summary>
        /// <param name="settingsFolder">The folder where app settings reside.</param>
        /// <returns>A new instance of <see cref="IServiceCollection"/> containing the services.</returns>
        protected abstract IServiceCollection ConfigureServices(IModifiableFolder settingsFolder);
    }
}
