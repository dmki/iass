using System.Diagnostics;

namespace IAss
{
    public class DirectoryScanner
    {
        private readonly ConfigManager _configManager;
        private Timer? _timer;

        public DirectoryScanner(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        public void StartScanning()
        {
            var config = _configManager.GetConfig();
            if (!config.Features.DirectoryScan)
            {
                OutputManager.WriteLine("Directory scanning is disabled in configuration.");
                return;
            }

            // Run immediately on startup
            ScanAndSave();
            
            // Then start the timer
            int intervalSeconds = config.DirectoryScanInterval;
            _timer = new Timer(ScanAndSave, null, TimeSpan.FromSeconds(intervalSeconds), TimeSpan.FromSeconds(intervalSeconds));
            
            OutputManager.WriteLine($"Directory scanning started with {intervalSeconds} second interval.");
        }

        private void ScanAndSave(object? state = null)
        {
            try
            {
                var currentDir = Environment.CurrentDirectory;
                var outputFilePath = "_currentdir.txt";
                
                // Read file filter patterns if file exists
                var filterFilePath = Path.Combine(Environment.CurrentDirectory, "filefilter.txt");
                var excludePatterns = new List<string>();
                
                if (File.Exists(filterFilePath))
                {
                    excludePatterns = File.ReadAllLines(filterFilePath)
                        .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                        .Select(line => line.Trim())
                        .ToList();
                }
                
                // Add default patterns if no filter file exists
                if (excludePatterns.Count == 0 && !File.Exists(filterFilePath))
                {
                    excludePatterns.Add(".*"); // Exclude directories starting with dot
                }
                
                // Walk through directory tree and collect paths
                var allPaths = new List<string>();
                CollectPaths(currentDir, currentDir, excludePatterns, allPaths);
                
                // Write to file with timestamp header
                var content = $"Current state of {currentDir} as of {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{string.Join("\n", allPaths)}";
                File.WriteAllText(outputFilePath, content);

                OutputManager.WriteLine($"Directory scan saved to {outputFilePath} at {DateTime.Now:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error during directory scan: {ex.Message}");
            }
        }

        private void CollectPaths(string rootDir, string currentDir, List<string> excludePatterns, List<string> allPaths)
        {
            try
            {
                // Check if current directory should be excluded (skip check for root directory itself)
                var relativeDir = Path.GetRelativePath(rootDir, currentDir);
                
                // Don't exclude the root directory itself (which will have relative path of ".")
                if (relativeDir != "." && ShouldExcludePath(relativeDir, excludePatterns))
                {
                    return;
                }

                // Add current directory to the list
                allPaths.Add(currentDir);

                // Get all files in current directory
                var files = Directory.GetFiles(currentDir);
                foreach (var file in files)
                {
                    var relativeFile = Path.GetRelativePath(rootDir, file);
                    if (!ShouldExcludePath(relativeFile, excludePatterns))
                    {
                        allPaths.Add(file);
                    }
                }

                // Recursively process subdirectories
                var subDirs = Directory.GetDirectories(currentDir);
                foreach (var subDir in subDirs)
                {
                    CollectPaths(rootDir, subDir, excludePatterns, allPaths);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we don't have access to
                OutputManager.WriteLine($"Access denied to directory: {currentDir}");
            }
            catch (DirectoryNotFoundException)
            {
                // Skip directories that no longer exist
                OutputManager.WriteLine($"Directory not found: {currentDir}");
            }
        }

        private bool ShouldExcludePath(string path, List<string> excludePatterns)
        {
            if (excludePatterns.Count == 0)
            {
                return false;
            }
            
            // Get the name of the directory or file for pattern matching
            var name = Path.GetFileName(path);
            
            foreach (var pattern in excludePatterns)
            {
                // If pattern ends with '*', match prefix (applies to directory/file name only)
                if (pattern.EndsWith("*"))
                {
                    var prefix = pattern.Substring(0, pattern.Length - 1);
                    if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                // If pattern starts with '*', match suffix (applies to directory/file name only)
                else if (pattern.StartsWith("*"))
                {
                    var suffix = pattern.Substring(1);
                    if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                // Exact match (applies to directory/file name only)
                else if (string.Equals(name, pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            
            return false;
        }

        public void StopScanning()
        {
            // Delete the file on stop
            var outputFilePath = "_currentdir.txt";
            if (File.Exists(outputFilePath))
            {
                OutputManager.WriteLine($"Deleting {outputFilePath}...");
                File.Delete(outputFilePath);
            }
            _timer?.Dispose();
        }
    }
}