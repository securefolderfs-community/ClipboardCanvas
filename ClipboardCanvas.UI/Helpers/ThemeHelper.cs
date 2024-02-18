using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.Services.Settings;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Extensions;
using ClipboardCanvas.UI.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.UI.Helpers
{
    /// <summary>
    /// Represents a helper class used for manipulating application themes.
    /// </summary>
    public abstract class ThemeHelper : ObservableObject, IAsyncInitialize
    {
        protected IAppSettings AppSettings { get; } = Ioc.Default.GetRequiredService<ISettingsService>().AppSettings;

        private ThemeType _CurrentTheme;
        /// <summary>
        /// Gets the current theme used by the app.
        /// </summary>
        public virtual ThemeType CurrentTheme
        {
            get => _CurrentTheme;
            protected set => SetProperty(ref _CurrentTheme, value);
        }

        /// <summary>
        /// Updates the UI to reflect the new changes, if necessary.
        /// </summary>
        public abstract void UpdateTheme();

        /// <summary>
        /// Updates the application's theme to specified <paramref name="themeType"/>.
        /// </summary>
        /// <param name="themeType">The theme to set for the app.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task SetThemeAsync(ThemeType themeType, CancellationToken cancellationToken = default)
        {
            CurrentTheme = themeType;
            AppSettings.ApplicationTheme = ConvertThemeType(themeType);

            UpdateTheme();
            return AppSettings.TrySaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task InitAsync(CancellationToken cancellationToken = default)
        {
            CurrentTheme = ConvertThemeString(AppSettings.ApplicationTheme);
            UpdateTheme();

            return Task.CompletedTask;
        }

        protected static string? ConvertThemeType(ThemeType themeType)
        {
            return themeType switch
            {
                ThemeType.Light => Constants.AppThemes.LIGHT_THEME,
                ThemeType.Dark => Constants.AppThemes.DARK_THEME,
                _ => null
            };
        }

        protected static ThemeType ConvertThemeString(string? themeString)
        {
            return themeString switch
            {
                Constants.AppThemes.LIGHT_THEME => ThemeType.Light,
                Constants.AppThemes.DARK_THEME => ThemeType.Dark,
                _ => ThemeType.Default
            };
        }
    }
}
