using ClipboardCanvas.ApplicationSettings;
using ClipboardCanvas.Logging;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ClipboardCanvas
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static SettingsInstance AppSettings = new SettingsInstance();

        public static ILogger ExceptionLogger = new ExceptionLogger();

        public static bool IsInRestrictedAccessMode { get; set; } = false;

        public static string AppVersion = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

#if !DEBUG
            AppCenter.Start("c7fb111e-c2ba-4c4e-80f9-a919c9939224", typeof(Analytics), typeof(Crashes));
#endif
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) => LogException(e.Exception);

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e) => LogException(e.Exception);

        private void LogException(Exception e)
        {
#if DEBUG
            Debug.WriteLine("--------- UNHANDLED EXCEPTION ---------");
            if (e != null)
            {
                Debug.WriteLine($"\n>>>> HRESULT: {e.HResult}\n");
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
#else
            LogExceptionToFile(e);
#endif
        }

        private void LogExceptionToFile(Exception e)
        {
            string exceptionString = "";

            exceptionString += $"HRESULT {e.HResult}\n";
            exceptionString += $"MESSAGE {e.Message}\n";
            exceptionString += $"STACKTRACE {e.StackTrace}\n";
            exceptionString += $"SOURCE {e.Source}\n";

            ExceptionLogger.Log(exceptionString);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

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
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
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
