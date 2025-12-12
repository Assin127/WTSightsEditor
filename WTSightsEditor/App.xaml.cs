using System;
using System.Windows;
using WTSightsEditor.Services;

namespace WTSightsEditor
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Initialize localization service
                var localizationService = LocalizationService.Instance;

                // Handle unhandled exceptions
                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    MessageBox.Show($"Unhandled exception: {args.ExceptionObject}",
                        "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                };

                DispatcherUnhandledException += (sender, args) =>
                {
                    MessageBox.Show($"Dispatcher unhandled exception: {args.Exception.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    args.Handled = true;
                };

                // Initialize localization after main window is loaded
                this.Activated += OnApplicationActivated;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize application: {ex.Message}",
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void OnApplicationActivated(object? sender, EventArgs e)
        {
            // Initialize localization once application is activated
            var localizationService = LocalizationService.Instance;
            localizationService.Initialize();

            // Remove event handler to avoid multiple initializations
            this.Activated -= OnApplicationActivated;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Save language preference on exit
            LocalizationService.Instance.SaveLanguagePreference();

            base.OnExit(e);
        }
    }
}