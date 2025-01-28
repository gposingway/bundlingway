using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bundlingway.Utilities
{
    public class InstallLogger
    {
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();

        public void Log(string category, string item, string description = "")
        {
            if (!_logEntries.Any(e => e.Category == category && e.Item == item && e.Description == description))
            {
                _logEntries.Add(new LogEntry
                {
                    Category = category,
                    Item = item,
                    Description = description
                });
            }
        }

        public void WriteLogToConsole()
        {
            var groupedEntries = _logEntries
                .GroupBy(e => e.Category)
                .OrderBy(g => g.Key);

            foreach (var group in groupedEntries)
            {
                Console.WriteLine($"[{group.Key}]");
                foreach (var entry in group.OrderBy(e => e.Item).ThenBy(e => e.Description))
                {
                    var description = string.IsNullOrEmpty(entry.Description) ? "" : $": {entry.Description}";
                    Console.WriteLine($"\t{entry.Item}{description}");
                }
            }
        }

        public void WriteLogToFile(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                var groupedEntries = _logEntries
                    .GroupBy(e => e.Category)
                    .OrderBy(g => g.Key);

                foreach (var group in groupedEntries)
                {
                    writer.WriteLine($"[{group.Key}]");
                    foreach (var entry in group.OrderBy(e => e.Item).ThenBy(e => e.Description))
                    {
                        var description = string.IsNullOrEmpty(entry.Description) ? "" : $": {entry.Description}";
                        writer.WriteLine($"\t{entry.Item}{description}");
                    }
                }
            }
        }

        private class LogEntry
        {
            public string Category { get; set; }
            public string Item { get; set; }
            public string Description { get; set; }
        }
    }
}
