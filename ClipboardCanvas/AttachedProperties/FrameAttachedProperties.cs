using Windows.UI.Xaml.Controls;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Pages;
using ClipboardCanvas.Pages.SettingsPages;
using ClipboardCanvas.DataModels.Navigation;

namespace ClipboardCanvas.AttachedProperties
{
    public class DisplayFrameNavigationAttachedProperty : BaseAttachedProperty<DisplayFrameNavigationAttachedProperty, DisplayFrameNavigationDataModel, Frame>
    {
        public override void OnValueChanged(Frame sender, DisplayFrameNavigationDataModel newValue)
        {
            switch (newValue.pageType)
            {
                case DisplayPageType.HomePage:
                    {
                        sender.Navigate(typeof(HomePage), newValue.parameter, newValue.transitionInfo);
                        break;
                    }

                case DisplayPageType.CanvasPage:
                    {
                        sender.Navigate(typeof(CanvasPage), newValue.parameter, newValue.transitionInfo);
                        break;
                    }

                case DisplayPageType.CollectionPreviewPage:
                    {
                        sender.Navigate(typeof(CollectionPreviewPage), newValue.parameter, newValue.transitionInfo);
                        break;
                    }
            }
        }
    }

    public class SettingsFrameNavigationAttachedProperty : BaseAttachedProperty<SettingsFrameNavigationAttachedProperty, SettingsFrameNavigationDataModel, Frame>
    {
        public override void OnValueChanged(Frame sender, SettingsFrameNavigationDataModel newValue)
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

                case SettingsPageType.About:
                    {
                        sender.Navigate(typeof(SettingsAboutPage), newValue, newValue.transitionInfo);
                        break;
                    }
            }
        }
    }
}
