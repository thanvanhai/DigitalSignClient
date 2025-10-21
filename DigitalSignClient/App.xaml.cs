// App.xaml.cs
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

            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Show login window
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Services - Singleton để giữ token
            services.AddSingleton<ApiService, ApiService>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();

            // Views
            services.AddTransient<LoginWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}