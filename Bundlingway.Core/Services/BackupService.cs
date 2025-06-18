using System.IO;
using System.Threading.Tasks;
using Bundlingway.Core.Interfaces;
using static Bundlingway.Core.Constants;
using Bundlingway.Core.Utilities;

namespace Bundlingway.Core.Services
{
    public class BackupService : IBackupService
    {
        private readonly IAppEnvironmentService _envService;
        public BackupService(IAppEnvironmentService envService)
        {
            _envService = envService;
        }

        public async Task BackupDataAsync()
        {
            var target = Path.Combine(_envService.BundlingwayDataFolder, Folders.Backup);
            if (string.IsNullOrEmpty(target))
            {
                await Bundlingway.Core.Utilities.UI.Announce("Backup path is invalid.");
                return;
            }
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }
            var source1 = Path.Combine(_envService.BundlingwayDataFolder, Folders.Cache);
            if (Directory.Exists(source1))
            {
                foreach (var file in Directory.GetFiles(source1))
                {
                    var destFile = Path.Combine(target, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
            }
            var source2 = Path.Combine(_envService.BundlingwayDataFolder, Folders.Packages);
            if (Directory.Exists(source2))
            {
                foreach (var folder in Directory.GetDirectories(source2))
                {
                    var source = Path.Combine(folder, Folders.SourcePackage);
                    if (Path.GetFileName(folder).Equals(Folders.SinglePresets))
                    {
                        foreach (var acceptableFile in AcceptableFilesInPresetFolder)
                            foreach (var file in Directory.GetFiles(folder, acceptableFile))
                            {
                                var destFile = Path.Combine(target, Path.GetFileName(file));
                                File.Copy(file, destFile, true);
                            }
                    }
                    else if (Directory.Exists(source))
                    {
                        foreach (var file in Directory.GetFiles(source))
                        {
                            var destFile = Path.Combine(target, Path.GetFileName(file));
                            File.Copy(file, destFile, true);
                        }
                    }
                }
            }
            await Bundlingway.Core.Utilities.UI.Announce("Backup complete!");
            System.Diagnostics.Process.Start("explorer.exe", target);
        }
    }
}
