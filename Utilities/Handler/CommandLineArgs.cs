using Bundlingway.Utilities.Extensions;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using System.Windows;
using Bundlingway.Core.Interfaces;
using Bundlingway.Core.Services;

namespace Bundlingway.Utilities.Handler
{
    public static class CommandLineArgs
    {
        /// <summary>
        /// Processes command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A string result, if applicable, or null.</returns>
        [Obsolete("Use ICommandLineService.ProcessAsync instead.")]
        public static async Task<string> ProcessAsync(string[] args)
        {
            throw new NotImplementedException("Use ICommandLineService.ProcessAsync instead.");
        }
    }
}
