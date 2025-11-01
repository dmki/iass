namespace IAss
{
    public class FindTextCommand
    {
        private readonly ConfigManager _configManager;

        public FindTextCommand(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        public bool Execute(string filePath, string searchString)
        {
            var config = _configManager.GetConfig();
            if (!config.Features.FindText)
            {
                OutputManager.WriteLine("FindText command is disabled in configuration.");
                return false;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    OutputManager.WriteLine("Error: No file path specified for find-text command.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(searchString))
                {
                    OutputManager.WriteLine("Error: No search string specified for find-text command.");
                    return false;
                }

                var fullPath = Path.GetFullPath(filePath);

                if (!File.Exists(fullPath))
                {
                    OutputManager.WriteLine($"Error: File not found: {fullPath}");
                    return false;
                }

                // Read file line by line without loading into memory
                int lineNumber = 0;
                int matchCount = 0;

                OutputManager.WriteLine($"Searching for '{searchString}' in {fullPath}:");
                OutputManager.WriteLine();

                // File.ReadLines reads lazily line by line
                foreach (var line in File.ReadLines(fullPath))
                {
                    lineNumber++;
                    if (line.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    {
                        matchCount++;
                        OutputManager.WriteLine($"Line {lineNumber}: {line}");
                    }
                }

                if (matchCount == 0)
                {
                    OutputManager.WriteLine($"No matches found for '{searchString}'");
                }
                else
                {
                    OutputManager.WriteLine();
                    OutputManager.WriteLine($"Total: {matchCount} match(es) found");
                }

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                OutputManager.WriteLine($"Error: Access denied to file: {filePath}");
                return false;
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error searching file: {ex.Message}");
                return false;
            }
        }
    }
}
