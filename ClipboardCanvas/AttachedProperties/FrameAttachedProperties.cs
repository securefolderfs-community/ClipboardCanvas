using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Pages;
using ClipboardCanvas.Pages.SettingsPages;
using ClipboardCanvas.DataModels.Navigation;

namespace ClipboardCanvas.AttachedProperties
{
    public class DisplayFrameNavigationAttachedProperty : BaseAttachedProperty<DisplayFrameNavigationAttachedProperty, DisplayFrameNavigationDataModel, Frame>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DisplayFrameNavigationDataModel navigationDataModel = e.NewValue as DisplayFrameNavigationDataModel;
            Frame frame = sender as Frame;

            switch (navigationDataModel.pageType)
            {
                case DisplayPageType.HomePage:
                    {
                        frame.Navigate(typeof(HomePage), navigationDataModel.parameter, navigationDataModel.transitionInfo);
                        break;
                    }

                case DisplayPageType.CanvasPage:
                    {
                        frame.Navigate(typeof(CanvasPage), navigationDataModel.parameter, navigationDataModel.transitionInfo);
                        break;
                    }

                case DisplayPageType.CollectionsPreview:
                    {
                        frame.Navigate(typeof(CollectionPreviewPage), navigationDataModel.parameter, navigationDataModel.transitionInfo);
                        break;
                    }
            }
        }
    }

    public class SettingsFrameNavigationAttachedProperty : BaseAttachedProperty<SettingsFrameNavigationAttachedProperty, SettingsFrameNavigationDataModel, Frame>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SettingsFrameNavigationDataModel navigationDataModel = e.NewValue as SettingsFrameNavigationDataModel;
            Frame frame = sender as Frame;

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
