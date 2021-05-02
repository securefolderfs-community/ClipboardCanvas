using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Pages;
using ClipboardCanvas.Pages.SettingsPages;

namespace ClipboardCanvas.AttachedProperties
{
    public class DisplayFrameNavigationAttachedProperty : BaseAttachedProperty<DisplayFrameNavigationAttachedProperty, DisplayFrameNavigationDataModel, Frame>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not Frame frame || e.NewValue is not DisplayFrameNavigationDataModel navigationDataModel)
            {
                return;
            }

            switch (navigationDataModel.pageType)
            {
                case DisplayPageType.HomePage:
                    {
                        frame.Navigate(typeof(HomePage), navigationDataModel, navigationDataModel.transitionInfo);
                        break;
                    }

                case DisplayPageType.CanvasPage:
                    {
                        frame.Navigate(typeof(CanvasPage), navigationDataModel, navigationDataModel.transitionInfo);
                        break;
                    }

                case DisplayPageType.CollectionsPreview:
                    {
                        frame.Navigate(typeof(CollectionPreviewPage), navigationDataModel, navigationDataModel.transitionInfo);
                        break;
                    }
            }
        }
    }

    public class SettingsFrameNavigationAttachedProperty : BaseAttachedProperty<SettingsFrameNavigationAttachedProperty, SettingsFrameNavigationDataModel, Frame>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not Frame frame || e.NewValue is not SettingsFrameNavigationDataModel navigationDataModel)
            {
                return;
            }

            switch (navigationDataModel.pageType)
            {
                case SettingsPageType.General:
                    {
                        frame.Navigate(typeof(SettingsGeneralPage), navigationDataModel, navigationDataModel.transitionInfo);
                        break;
                    }

                case SettingsPageType.Pasting:
                    {
                        frame.Navigate(typeof(SettingsPastingPage), navigationDataModel, navigationDataModel.transitionInfo);
                        break;
                    }

                case SettingsPageType.About:
                    {
                        frame.Navigate(typeof(SettingsAboutPage), navigationDataModel, navigationDataModel.transitionInfo);
                        break;
                    }
            }
        }
    }
}
