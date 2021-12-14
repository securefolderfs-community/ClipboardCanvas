using Microsoft.UI.Xaml.Controls;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Pages.SettingsPages;
using ClipboardCanvas.DataModels.Navigation;

namespace ClipboardCanvas.AttachedProperties
{
    public class SettingsFrameNavigationAttachedProperty : BaseAttachedProperty<SettingsFrameNavigationAttachedProperty, SettingsFrameNavigationDataModel, Frame>
    {
        protected override void OnValueChanged(Frame sender, SettingsFrameNavigationDataModel newValue)
        {
            switch (newValue.pageType)
            {
                case SettingsPageType.General:
                    {
                        sender.Navigate(typeof(SettingsGeneralPage), newValue, newValue.transitionInfo);
                        break;
                    }

                case SettingsPageType.Pasting:
                    {
                        sender.Navigate(typeof(SettingsPastingPage), newValue, newValue.transitionInfo);
                        break;
                    }

                case SettingsPageType.Notifications:
                    {
                        sender.Navigate(typeof(SettingsNotificationsPage), newValue, newValue.transitionInfo);
                        break;
                    }

                case SettingsPageType.About:
                    {
                        sender.Navigate(typeof(SettingsAboutPage), newValue, newValue.transitionInfo);
                        break;
                    }
            }
        }
    }
}
