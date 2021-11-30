using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using Windows.System;
using Windows.Storage;

using ClipboardCanvas.Services;
using ClipboardCanvas.Services.Implementation;
using Microsoft.Toolkit.Uwp;

namespace ClipboardCanvas
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// <see cref="IServiceProvider"/> to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // Configure exception handlers
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            if (Constants.Debugging.FIRST_CHANCE_EXCEPTION_DEBUGGING)
            {
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            }

            // Configure services
            Services = ConfigureServices();
            Ioc.Default.ConfigureServices(Services);

            // Start AppCenter
#if !DEBUG
            AppCenter.Start("c7fb111e-c2ba-4c4e-80f9-a919c9939224", typeof(Analytics), typeof(Crashes));
#endif
        }

        private IServiceProvider ConfigureServices()
        {
            ServiceCollection services = new ServiceCollection();

            services
                .AddSingleton<INavigationService, NavigationService>()
                .AddSingleton<ITimelineService, TimelineService>()
                .AddSingleton<IStatusCenterService, StatusCenterService>()
                .AddSingleton<IAutopasteService, AutopasteService>()
                .AddSingleton<IDialogService, DialogService>()
                .AddSingleton<INotificationService, NotificationService>()
                .AddSingleton<IApplicationService, ApplicationService>()
                .AddSingleton<ILogger, ExceptionToFileLogger>()

                // Settings services
                .AddSingleton<IUserSettingsService, UserSettingsService>((sp) => new UserSettingsService(sp.GetService<IApplicationService>()))
                .AddSingleton<ICollectionsSettingsService, CollectionsSettingsService>()
                .AddSingleton<ITimelineSettingsService, TimelineSettingsService>()
                .AddSingleton<ICanvasSettingsService, CanvasSettingsService>()
                .AddSingleton<IAutopasteSettingsService, AutopasteSettingsService>()
                .AddSingleton<IApplicationSettingsService, ApplicationSettingsService>();

            return services.BuildServiceProvider();
        }

        protected override async void OnActivated(IActivatedEventArgs e)
        {
            var rootFrame = EnsureWindowIsInitialized();

            switch (e.Kind)
            {
                case ActivationKind.ToastNotification:
                    {
                        var notificationEventArgs = e as ToastNotificationActivatedEventArgs;
                        switch (notificationEventArgs.Argument)
                        {
                            case Constants.UI.Notifications.TOAST_NOTIFICATION_ERROR_ARGUMENT:
                                {
                                    await Launcher.LaunchUriAsync(new Uri(ApplicationData.Current.LocalFolder.Path));
                                    await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
                                    break;
                                }
                        }

                        break;
                    }
            }

            base.OnActivated(e);

            // Ensure the current window is active.
            rootFrame.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
            Window.Current.Activate();
        }

        private Frame EnsureWindowIsInitialized()
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (Window.Current.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.CacheSize = 1;
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) => LogException(e.Exception);

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e) => LogException(e.Exception, () => e.Handled = true);

        private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e) => LogException(e.Exception);

        private void LogException(Exception e, Action tryHandleException = null)
        {
#if DEBUG
            if (tryHandleException == null)
            {
                tryHandleException = () => { };
            }

            Debug.WriteLine("--------- UNHANDLED EXCEPTION ---------");
            if (e != null)
            {
                Debug.WriteLine($"\n>>>> HRESULT: {e.HResult} (0x{e.HResult.ToString("X")})\n");
                if (!string.IsNullOrEmpty(e.Message))
                {
                    Debug.WriteLine("\n--- MESSAGE ---");
                    Debug.WriteLine(e.Message);
                }
                if (!string.IsNullOrEmpty(e.StackTrace))
                {
                    Debug.WriteLine("\n--- STACKTRACE ---");
                    Debug.WriteLine(e.StackTrace);
                }
                if (!string.IsNullOrEmpty(e.Source))
                {
                    Debug.WriteLine("\n--- SOURCE ---");
                    Debug.WriteLine(e.Source);
                }
                if (e.InnerException != null)
                {
                    Debug.WriteLine("\n--- INNER ---");
                    Debug.WriteLine(e.InnerException);
                }
                if (!string.IsNullOrEmpty(e.HelpLink))
                {
                    Debug.WriteLine("\n--- HELP LINK ---");
                    Debug.WriteLine(e.HelpLink);
                }
            }
            else
            {
                Debug.WriteLine("\nException is null!\n");
            }

            Debug.WriteLine("---------------------------------------");

            Debugger.Break(); // Please check "Output Window" for exception details (View -> Output Window) (CTRL + ALT + O)

            if (false) // Can only step-in manually in debug mode
            {
#pragma warning disable CS0162 // Unreachable code detected
                tryHandleException();
#pragma warning restore CS0162 // Unreachable code detected
            }
#else
            LogExceptionToFile(e);

            IUserSettingsService userSettingsService = null;
            try
            {
                userSettingsService = Ioc.Default.GetService<IUserSettingsService>();
            }
            catch { }

            bool pushErrorNotification = userSettingsService?.PushErrorNotification ?? false;

            if (pushErrorNotification)
            {
                PushErrorNotification();
            }
#endif
        }

        private void LogExceptionToFile(Exception e)
        {
            string exceptionString = "";

            exceptionString += DateTime.Now.ToString(Constants.FileSystem.EXCEPTION_BLOCK_DATE_FORMAT);
            exceptionString += "\n";
            exceptionString += $">>> HRESULT {e.HResult}\n";
            exceptionString += $">>> MESSAGE {e.Message}\n";
            exceptionString += $">>> STACKTRACE {e.StackTrace}\n";
            exceptionString += $">>> INNER {e.InnerException}\n";
            exceptionString += $">>> SOURCE {e.Source}\n\n";

            ILogger logger;
            try
            {
                logger = Ioc.Default.GetService<ILogger>(); // Try get Ioc logger
            }
            catch
            {
                logger = new ExceptionToFileLogger(); // Use default logger
            }

            logger.LogToFile(exceptionString);
        }

        private void PushErrorNotification()
        {
            // Create custom notification
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "ClipboardCanvasCrashTitle".GetLocalized()
                            },
                            new AdaptiveText()
                            {
                                Text = "ClipboardCanvasCrashSubtitle".GetLocalized()
                            }
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("ClipboardCanvasCrashReportIssue".GetLocalized(), Constants.UI.Notifications.TOAST_NOTIFICATION_ERROR_ARGUMENT)
                        {
                            ActivationType = ToastActivationType.Foreground
                        }
                    }
                }
            };

            // Compile the notification to native ToastNotification
            ToastNotification toastNotificationNative = new ToastNotification(toastContent.GetXml());

            // Push the native ToastNotification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotificationNative);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            SystemInformation.Instance.TrackAppUse(e);
                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
            if (Window.Current.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.CacheSize = 1;

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                CoreApplication.EnablePrelaunch(true);
                

                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments, new SuppressNavigationTransitionInfo());
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
