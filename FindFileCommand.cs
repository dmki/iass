namespace IAss
{
    public class FindFileCommand
    {
        private readonly ConfigManager _configManager;

        public FindFileCommand(ConfigManager configManager)
        {
            _configManager = configManager;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileMask"></param>
        /// <param name="searchString"></param>
        /// <param name="depth">How many levels of sub-folders to scan. Negative value means unlimited, 0 means current directory only.</param>
        /// <returns></returns>
        public bool Execute(string fileMask, string? searchString = null, int depth = -1)
        {
            var config = _configManager.GetConfig();
            if (!config.Features.FindFile)
            {
                OutputManager.WriteLine("FindFile command is disabled in configuration.");
                return false;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(fileMask))
                {
                    OutputManager.WriteLine("Error: No file mask specified for find-file command.");
                    return false;
                }

                // If depth is not specified, search only current directory. If it is negative, search unlimited depth.
                int searchDepth = depth < 1 ? 0 : depth;
                if (depth < 0)
                {
                    searchDepth = 100; //int.MaxValue;
                }

                var currentDir = Environment.CurrentDirectory;
                var matchingFiles = new List<string>();

                OutputManager.WriteLine($"Searching for files matching '{fileMask}' (depth: {searchDepth})...");
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    OutputManager.WriteLine($"Filtering by content: '{searchString}'");
                }
                OutputManager.WriteLine();

                // Search for files recursively with depth limit
                SearchFiles(currentDir, fileMask, searchString, searchDepth, 0, matchingFiles);

                if (matchingFiles.Count == 0)
                {
                    OutputManager.WriteLine("No matching files found.");
                }
                else
                {
                    foreach (var file in matchingFiles)
                    {
                        OutputManager.WriteLine(file);
                    }
                    OutputManager.WriteLine();
                    OutputManager.WriteLine($"Total: {matchingFiles.Count} file(s) found");
                }

                return true;
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error searching for files: {ex.Message}");
                return false;
            }
        }

        private void SearchFiles(string directory, string fileMask, string? searchString, int maxDepth, int currentDepth, List<string> results)
        {
            try
            {
                // Search files in current directory
                var files = Directory.EnumerateFiles(directory, fileMask);

                foreach (var file in files)
                {
                    // If no search string specified, add all matching files
                    if (string.IsNullOrWhiteSpace(searchString))
                    {
                        results.Add(Path.GetFullPath(file));
                    }
                    else
                    {
                        // Check if file contains the search string (line by line)
                        if (FileContainsString(file, searchString))
                        {
                            results.Add(Path.GetFullPath(file));
                        }
                    }
                }

                // If we haven't reached max depth, search subdirectories
                if (currentDepth < maxDepth)
                {
                    var subdirectories = Directory.EnumerateDirectories(directory);
                    foreach (var subdirectory in subdirectories)
                    {
                        SearchFiles(subdirectory, fileMask, searchString, maxDepth, currentDepth + 1, results);
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
                // Read file line by line without loading into memory
                foreach (var line in File.ReadLines(filePath))
                {
                    if (line.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                // Skip files we can't read
                return false;
            }
            catch (IOException)
            {
                // Skip files with IO errors (locked, etc.)
                return false;
            }
            catch (Exception)
            {
                // Skip other problematic files
                return false;
            }
        }
    }
}
