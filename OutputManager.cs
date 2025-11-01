namespace IAss
{
    public static class OutputManager
    {
        private static bool _suppressOutput = false;
        private static string? _outputFile = null;
        private static readonly object _fileLock = new object();

        /// <summary>
        /// Initialize the output manager with global parameters
        /// </summary>
        /// <param name="suppressOutput">True if -null parameter was specified</param>
        /// <param name="outputFile">Output file path if -output-file parameter was specified</param>
        public static void Initialize(bool suppressOutput, string? outputFile)
        {
            _suppressOutput = suppressOutput;
            _outputFile = outputFile;

            // Clear the output file if it exists
            if (!string.IsNullOrWhiteSpace(_outputFile))
            {
                try
                {
                    File.WriteAllText(_outputFile, string.Empty);
                }
                catch (Exception ex)
                {
                    // If we can't initialize the output file, fall back to console
                    Console.WriteLine($"Warning: Could not initialize output file '{_outputFile}': {ex.Message}");
                    Console.WriteLine("Falling back to console output.");
                    _outputFile = null;
                }
            }
        }

        /// <summary>
        /// Write a line to the configured output (console, file, or null)
        /// </summary>
        /// <param name="message">The message to write</param>
        public static void WriteLine(string message = "")
        {
            // If -null is specified, don't output anything
            if (_suppressOutput)
            {
                return;
            }

            // If output file is specified, write to file
            if (!string.IsNullOrWhiteSpace(_outputFile))
            {
                try
                {
                    lock (_fileLock)
                    {
                        File.AppendAllText(_outputFile, message + Environment.NewLine);
                    }
                }
                catch
                {
                    // Silently fail - we're in redirect mode, don't want to spam console
                    // If file write fails, we just lose that line
                }
            }
            else
            {
                // Default: write to console
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Check if output is being suppressed
        /// </summary>
        public static bool IsSuppressed => _suppressOutput;

        /// <summary>
        /// Check if output is being redirected to a file
        /// </summary>
        public static bool IsRedirected => !string.IsNullOrWhiteSpace(_outputFile);
    }
}
