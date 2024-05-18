using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views.Settings
{
    public sealed partial class GeneralSettingsViewModel : ObservableObject, IViewDesignation, IAsyncInitialize
    {
        private readonly CultureInfo _currentCulture;
        private bool _noNotify;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private ObservableCollection<LanguageViewModel> _Languages;
        [ObservableProperty] private LanguageViewModel? _SelectedLanguage;
        [ObservableProperty] private bool _IsRestartRequired;

        private ILocalizationService LocalizationService { get; } = Ioc.Default.GetRequiredService<ILocalizationService>();

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        public GeneralSettingsViewModel()
        {
            Languages = new();
            _currentCulture = LocalizationService.CurrentCulture;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var item in LocalizationService.AppLanguages)
                Languages.Add(new(item));

            // Add wildcard language
            Languages.Add(new(CultureInfo.InvariantCulture, "Not seeing your language?"));

            // Set selected language
            _noNotify = true;
            SelectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Equals(LocalizationService.CurrentCulture));
            _noNotify = false;
        }

        /// <inheritdoc/>
        public void OnAppearing()
        {
        }

        /// <inheritdoc/>
        public void OnDisappearing()
        {
        }

        async partial void OnSelectedLanguageChanged(LanguageViewModel? value)
        {
            if (value is null || _noNotify)
                return;

            if (value.CultureInfo.Equals(CultureInfo.InvariantCulture))
            {
                // Wildcard
                await ApplicationService.OpenUriAsync(new("https://github.com/d2dyno1/ClipboardCanvas"));

                SelectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Equals(LocalizationService.CurrentCulture));
                if (SelectedLanguage is not null)
                    await LocalizationService.SetCultureAsync(SelectedLanguage.CultureInfo);
            }
            else
            {
                await LocalizationService.SetCultureAsync(value.CultureInfo);
                IsRestartRequired = !_currentCulture.Equals(SelectedLanguage?.CultureInfo);
            }
        }
    }
}
