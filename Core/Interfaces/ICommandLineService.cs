namespace Bundlingway.Core.Interfaces
{
    public interface ICommandLineService
    {
        Task<string?> ProcessAsync(string[] args);
    }
}
