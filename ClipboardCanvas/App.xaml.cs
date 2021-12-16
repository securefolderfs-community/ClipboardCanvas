using System;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Notifications;
using ClipboardCanvas.GlobalizationExtensions;

using ClipboardCanvas.Services;
using ClipboardCanvas.Services.Implementation;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window m_window;

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

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
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

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) => LogException(e.Exception);

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) => LogException(e.Exception, () => e.Handled = true);

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
                                Text = "ClipboardCanvasCrashTitle".GetLocalized2()
                            },
                            new AdaptiveText()
                            {
                                Text = "ClipboardCanvasCrashSubtitle".GetLocalized2()
                            }
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("ClipboardCanvasCrashReportIssue".GetLocalized2(), Constants.UI.Notifications.TOAST_NOTIFICATION_ERROR_ARGUMENT)
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
    }
}
