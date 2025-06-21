using Bundlingway.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Main application host implementation. Coordinates all core services and manages application lifecycle.
    /// </summary>
    public class ApplicationHost : IApplicationHost
    {
        private readonly IServiceProvider _serviceProvider;
        private bool _isHeadless;
        private bool _isInitialized;

        public ApplicationHost(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool IsHeadless => _isHeadless;
        public bool IsInitialized => _isInitialized;
        public event EventHandler? ApplicationShutdown;

        public async Task InitializeAsync()
        {
            // Initialize core services if needed
            // Example: await _serviceProvider.GetRequiredService<IConfigurationService>().LoadAsync();
            _isInitialized = true;
            await Task.CompletedTask;
        }

        public async Task StartUIAsync()
        {
            _isHeadless = false;
            // UI startup logic can be handled in Program.cs
            await Task.CompletedTask;
        }

        public async Task StartHeadlessAsync(string[] args)
        {
            _isHeadless = true;
            // Headless/CLI startup logic here
            await Task.CompletedTask;
        }

        public async Task ShutdownAsync()
        {
            // Graceful shutdown logic for all services
            ApplicationShutdown?.Invoke(this, EventArgs.Empty);
            await Task.CompletedTask;
        }

        public T GetService<T>() where T : class
        {
            return _serviceProvider.GetService(typeof(T)) as T;
        }
    }
}
