using System.Diagnostics;

namespace IAss
{
    public class ScanDirCommand
    {
        private readonly ConfigManager _configManager;
        private Timer? _timer;
        private DateTime _endTime;
        private bool _isRunning;

        public ScanDirCommand(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        public bool Execute(int timerSeconds = 60, int timeoutMinutes = 10, int depth = -1,
            string? fileMask = null, string? searchString = null)
        {
            var config = _configManager.GetConfig();
            if (!config.Features.DirectoryScan)
            {
                OutputManager.WriteLine("Directory scanning is disabled in configuration.");
                return false;
            }

            try
            {
                // Parse file masks if provided
                var fileMasks = ParseFileMasks(fileMask);

                // Calculate end time
                _endTime = DateTime.Now.AddMinutes(timeoutMinutes);
                _isRunning = true;

                // Convert depth: less than 1 means current directory only
                int scanDepth = depth < 1 ? 0 : depth;

                OutputManager.WriteLine("Directory scan parameters:");
                OutputManager.WriteLine($"  Timer interval: {timerSeconds} seconds");
                OutputManager.WriteLine($"  Timeout: {timeoutMinutes} minutes (ends at {_endTime:HH:mm:ss})");
                OutputManager.WriteLine($"  Depth: {scanDepth} {(scanDepth == 0 ? "(current directory only)" : $"level(s)")}");
                if (fileMasks.Count > 0)
                {
                    OutputManager.WriteLine($"  File mask(s): {string.Join(", ", fileMasks)}");
                }
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    OutputManager.WriteLine($"  Search string: '{searchString}'");
                }
                OutputManager.WriteLine();

                // Run immediately on startup
                ScanAndSave(scanDepth, fileMasks, searchString);

                // Check if we've already exceeded timeout after first scan
                if (DateTime.Now >= _endTime)
                {
                    OutputManager.WriteLine("Timeout reached. Exiting...");
                    Cleanup();
                    return true;
                }

                // Start the timer for periodic scans
                _timer = new Timer(
                    state => TimerCallback(scanDepth, fileMasks, searchString),
                    null,
                    TimeSpan.FromSeconds(timerSeconds),
                    TimeSpan.FromSeconds(timerSeconds)
                );

                OutputManager.WriteLine("IAss directory scanning running. Press 'q' to quit...");
                OutputManager.WriteLine();

                // Keep the application running until timeout or user quits
                try
                {
                    while (_isRunning && DateTime.Now < _endTime)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);
                            if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                            {
                                OutputManager.WriteLine("User requested shutdown...");
                                break;
                            }
                        }
                        else
                        {
                            // Sleep briefly to avoid busy-waiting
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // If there's no console (e.g., when run in background), just wait for timeout
                    OutputManager.WriteLine("Running in background mode (no interactive console)...");

                    while (_isRunning && DateTime.Now < _endTime)
                    {
                        Thread.Sleep(1000);
                    }
                }

                if (DateTime.Now >= _endTime)
                {
                    OutputManager.WriteLine($"\nTimeout reached after {timeoutMinutes} minutes. Exiting...");
                }

                Cleanup();
                return true;
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error during directory scanning: {ex.Message}");
                Cleanup();
                return false;
            }
        }

        private void TimerCallback(int depth, List<string> fileMasks, string? searchString)
        {
            // Check if we've exceeded timeout
            if (DateTime.Now >= _endTime)
            {
                _isRunning = false;
                return;
            }

            ScanAndSave(depth, fileMasks, searchString);
        }

        private void ScanAndSave(int depth, List<string> fileMasks, string? searchString)
        {
            try
            {
                var currentDir = Environment.CurrentDirectory;
                var outputFilePath = "_currentdir.txt";

                var sw = Stopwatch.StartNew();

                // Collect all matching paths
                var allPaths = new List<string>();
                CollectPaths(currentDir, currentDir, depth, 0, fileMasks, searchString, allPaths);

                // Write to file with timestamp header
                var content = $"Current state of {currentDir} as of {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{string.Join("\n", allPaths)}";
                File.WriteAllText(outputFilePath, content);

                sw.Stop();
                OutputManager.WriteLine($"Directory scan saved to {outputFilePath} at {DateTime.Now:HH:mm:ss} ({allPaths.Count} items, {sw.ElapsedMilliseconds}ms)");
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error during directory scan: {ex.Message}");
            }
        }

        private void CollectPaths(string rootDir, string currentDir, int maxDepth, int currentDepth,
            List<string> fileMasks, string? searchString, List<string> allPaths)
        {
            try
            {
                // Check if we've exceeded timeout during scan
                if (DateTime.Now >= _endTime)
                {
                    return;
                }

                // Add current directory to the list
                allPaths.Add(currentDir);

                // Get all files in current directory
                IEnumerable<string> files;

                if (fileMasks.Count > 0)
                {
                    // Apply file masks
                    var matchedFiles = new List<string>();
                    foreach (var mask in fileMasks)
                    {
                        try
                        {
                            matchedFiles.AddRange(Directory.EnumerateFiles(currentDir, mask));
                        }
                        catch
                        {
                            // Skip errors for individual mask patterns
                        }
                    }
                    files = matchedFiles.Distinct();
                }
                else
                {
                    // No mask - get all files
                    files = Directory.EnumerateFiles(currentDir);
                }

                foreach (var file in files)
                {
                    // If search string is specified, check if file contains it
                    if (!string.IsNullOrWhiteSpace(searchString))
                    {
                        if (!FileContainsString(file, searchString))
                        {
                            continue; // Skip files that don't contain the search string
                        }
                    }

                    allPaths.Add(file);
                }

                // Recursively process subdirectories if we haven't reached max depth
                if (currentDepth < maxDepth)
                {
                    var subDirs = Directory.GetDirectories(currentDir);
                    foreach (var subDir in subDirs)
                    {
                        // Skip hidden directories (starting with .)
                        var dirName = Path.GetFileName(subDir);
                        if (dirName.StartsWith("."))
                        {
                            continue;
                        }

                        CollectPaths(rootDir, subDir, maxDepth, currentDepth + 1, fileMasks, searchString, allPaths);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we don't have access to
            }
            catch (DirectoryNotFoundException)
            {
                // Skip directories that no longer exist
            }
        }

        private bool FileContainsString(string filePath, string searchString)
        {
            try
            {
                // Read file line by line for memory efficiency with large files
                foreach (var line in File.ReadLines(filePath))
                {
                    if (line.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                // Skip files we can't read (access denied, binary files, etc.)
                return false;
            }
        }

        private List<string> ParseFileMasks(string? fileMask)
        {
            var masks = new List<string>();

            if (string.IsNullOrWhiteSpace(fileMask))
            {
                return masks;
            }

            // Split by comma and trim whitespace
            var parts = fileMask.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    masks.Add(trimmed);
                }
            }

            return masks;
        }

        private void Cleanup()
        {
            _isRunning = false;
            _timer?.Dispose();

            // Delete the output file
            var outputFilePath = "_currentdir.txt";
            if (File.Exists(outputFilePath))
            {
                try
                {
                    OutputManager.WriteLine($"Deleting {outputFilePath}...");
                    File.Delete(outputFilePath);
                }
                catch (Exception ex)
                {
                    OutputManager.WriteLine($"Warning: Could not delete {outputFilePath}: {ex.Message}");
                }
            }

            OutputManager.WriteLine("Directory scanning stopped.");
        }
    }
}
