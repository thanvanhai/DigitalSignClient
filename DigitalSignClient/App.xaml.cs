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
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            // 🔹 Đăng ký tất cả API Services
            services.AddApiServices();

            // 🔹 Đăng ký ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<DocumentListViewModel>();
            services.AddTransient<DocumentTypeViewModel>();
            services.AddTransient<WorkflowConnectionViewModel>();
            services.AddTransient<WorkflowNodeViewModel>();   
            services.AddTransient<WorkflowDesignerViewModel>();     

            // 🔹 Đăng ký Views
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            // 🔹 Mở cửa sổ đăng nhập
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (ServiceProvider is IDisposable disposable)
                disposable.Dispose();

            base.OnExit(e);
        }
    }
}
