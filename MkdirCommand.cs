namespace IAss
{
    public class MkdirCommand
    {
        private readonly ConfigManager _configManager;

        public MkdirCommand(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        public bool Execute(string path)
        {
            var config = _configManager.GetConfig();
            if (!config.Features.Mkdir)
            {
                OutputManager.WriteLine("Mkdir command is disabled in configuration.");
                return false;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    OutputManager.WriteLine("Error: No path specified for mkdir command.");
                    return false;
                }

                var dirPath = Path.GetFullPath(path);
                
                if (Directory.Exists(dirPath))
                {
                    OutputManager.WriteLine($"Directory already exists: {dirPath}");
                    return true;
                }

                Directory.CreateDirectory(dirPath);
                OutputManager.WriteLine($"Directory created: {dirPath}");
                return true;
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error creating directory: {ex.Message}");
                return false;
            }
        }
    }
}