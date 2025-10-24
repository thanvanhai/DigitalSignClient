using DigitalSignClient.Services;
using DigitalSignClient.ViewModels;
using DigitalSignClient.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace DigitalSignClient
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            // Services
            services.AddSingleton<ApiService>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<DocumentListViewModel>();

            // Views
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            // Show login window
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}