using System.Threading.Tasks;

namespace Bundlingway.Core.Interfaces
{
    /// <summary>
    /// Service interface for ReShade operations.
    /// </summary>
    public interface IReShadeService
    {
        Task GetRemoteInfoAsync();
        Task GetLocalInfoAsync();
        Task UpdateAsync();
    }
}
