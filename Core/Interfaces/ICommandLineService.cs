namespace Bundlingway.Core.Interfaces
{
    public interface ICommandLineService
    {
        Task<string?> ProcessAsync(string[] args);
        Task<string?> HandleProtocolAsync(string protocolUrl);
    }
}
