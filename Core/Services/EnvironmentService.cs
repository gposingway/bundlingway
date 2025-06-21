using Bundlingway.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Provides environment-specific configuration and health checks.
    /// </summary>
    public class EnvironmentService
    {
        private readonly ILogger<EnvironmentService> _logger;
        private readonly IConfigurationService _configService;
        private readonly IAppEnvironmentService _envService;

        public EnvironmentService(ILogger<EnvironmentService> logger, IConfigurationService configService, IAppEnvironmentService envService)
        {
            _logger = logger;
            _configService = configService;
            _envService = envService;
        }

        public void LogEnvironmentInfo()
        {
            _logger.LogInformation($"OS: {Environment.OSVersion}");
            _logger.LogInformation($"Machine: {Environment.MachineName}");
            _logger.LogInformation($"User: {Environment.UserName}");
            _logger.LogInformation($"App Version: {_envService.AppVersion}");
        }

        public bool IsProduction => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        public bool IsDevelopment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        public void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_envService.AppVersion))
            {
                _logger.LogWarning("AppVersion is not set in environment.");
            }
        }

        public void HealthCheck()
        {
            if (!System.IO.Directory.Exists(_envService.BundlingwayDataFolder))
            {
                _logger.LogError($"Data folder missing: {_envService.BundlingwayDataFolder}");
            }
        }
    }
}
