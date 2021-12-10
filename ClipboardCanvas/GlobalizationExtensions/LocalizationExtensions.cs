using Windows.UI;
using Windows.ApplicationModel.Resources;

namespace ClipboardCanvas.GlobalizationExtensions
{
    public static class LocalizationExtensions
    {
        private static readonly ResourceLoader IndependentLoader;

        static LocalizationExtensions()
        {
            IndependentLoader = ResourceLoader.GetForViewIndependentUse();
        }

        public static string GetLocalized2(this string resourceKey, UIContext uiContext = null)
        {
            if (uiContext is not null)
            {
                return ResourceLoader.GetForUIContext(uiContext).GetString(resourceKey);
            }

            return IndependentLoader?.GetString(resourceKey);
        }
    }
}
