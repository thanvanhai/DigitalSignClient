using DigitalSignClient.Services.Handlers;
using DigitalSignClient.Services.Implementations;
using DigitalSignClient.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;

namespace DigitalSignClient.Services
{
    public static class ServiceRegistration
    {
        public static void AddApiServices(this IServiceCollection services)
        {
            services.AddSingleton<ApiManager>();
            services.AddTransient<AuthTokenHandler>();

            // AuthService (không cần token)
            services.AddHttpClient<IAuthService, AuthService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7159/api/");
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });

            // DocumentService (có token)
            services.AddHttpClient<IDocumentService, DocumentService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7159/api/");
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }).AddHttpMessageHandler<AuthTokenHandler>();

            // DocumentTypeService (có token) - THÊM ĐOẠN NÀY
            services.AddHttpClient<IDocumentTypeService, DocumentTypeService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7159/api/");
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }).AddHttpMessageHandler<AuthTokenHandler>();
            // WorkflowService (có token)
            services.AddHttpClient<IWorkflowService, WorkflowService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7159/api/");
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }).AddHttpMessageHandler<AuthTokenHandler>();
        }
    }
}
