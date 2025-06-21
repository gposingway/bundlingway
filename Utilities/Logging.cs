namespace Bundlingway.Utilities
{
    public class Logging
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
                Serilog.Log.Information($"[{group.Key}]");
                foreach (var entry in group.OrderBy(e => e.Item).ThenBy(e => e.Description))
                {
                    var description = string.IsNullOrEmpty(entry.Description) ? "" : $": {entry.Description}";
                    Serilog.Log.Information($"\t{entry.Item}{description}");
                }
            }
        }

        public void WriteLogToFile(string filePath)
        {
            if (!_logEntries.Any()) return;

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
            public required string Category { get; set; }
            public required string Item { get; set; }
            public required string Description { get; set; }
        }
    }
}
