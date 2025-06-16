using Bundlingway.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Service locator for managing service dependencies.
    /// This is an interim solution to migrate away from static dependencies.
    /// Eventually, this should be replaced with proper dependency injection.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();
        private static readonly object _lock = new();

        /// <summary>
        /// Registers a service instance.
        /// </summary>
        /// <typeparam name="TInterface">Service interface type</typeparam>
        /// <param name="implementation">Service implementation</param>
        public static void Register<TInterface>(TInterface implementation) where TInterface : class
        {
            lock (_lock)
            {
                _services[typeof(TInterface)] = implementation;
            }
        }

        /// <summary>
        /// Gets a service instance.
        /// </summary>
        /// <typeparam name="TInterface">Service interface type</typeparam>
        /// <returns>Service instance</returns>
        /// <exception cref="InvalidOperationException">Thrown when service is not registered</exception>
        public static TInterface GetService<TInterface>() where TInterface : class
        {
            lock (_lock)
            {
                if (_services.TryGetValue(typeof(TInterface), out var service))
                {
                    return (TInterface)service;
                }
            }

            throw new InvalidOperationException($"Service of type {typeof(TInterface).Name} is not registered.");
        }

        /// <summary>
        /// Tries to get a service instance.
        /// </summary>
        /// <typeparam name="TInterface">Service interface type</typeparam>
        /// <returns>Service instance or null if not found</returns>
        public static TInterface? TryGetService<TInterface>() where TInterface : class
        {
            lock (_lock)
            {
                if (_services.TryGetValue(typeof(TInterface), out var service))
                {
                    return (TInterface)service;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a service is registered.
        /// </summary>
        /// <typeparam name="TInterface">Service interface type</typeparam>
        /// <returns>True if service is registered, false otherwise</returns>
        public static bool IsRegistered<TInterface>() where TInterface : class
        {
            lock (_lock)
            {
                return _services.ContainsKey(typeof(TInterface));
            }
        }

        /// <summary>
        /// Clears all registered services.
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _services.Clear();
            }
        }

        /// <summary>
        /// Initializes core services with default implementations.
        /// This method sets up the basic services needed for the application.
        /// </summary>
        public static void InitializeCoreServices(string configFilePath, frmLanding? mainForm = null)
        {
            // Configuration service
            var configService = new ConfigurationService(configFilePath);
            Register<IConfigurationService>(configService);

            // File system service
            Register<IFileSystemService>(new FileSystemService());

            // HTTP client service
            Register<IHttpClientService>(new HttpClientService());

            // UI services (WinForms specific for now)
            Register<IUserNotificationService>(new UI.WinForms.WinFormsNotificationService(mainForm));
            Register<IProgressReporter>(new UI.WinForms.WinFormsProgressReporter(mainForm));
        }
    }
}
