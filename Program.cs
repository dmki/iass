using IAss;

namespace IAss;

class Program
{
    static void Main(string[] args)
    {
        // Parse global parameters (-null and -output-file)
        var (suppressOutput, outputFile, filteredArgs) = ParseGlobalParameters(args);

        // Initialize output manager with global parameters
        OutputManager.Initialize(suppressOutput, outputFile);

        // Initialize configuration manager
        var configManager = new ConfigManager();

        if (filteredArgs.Length > 0)
        {
            var command = filteredArgs[0].ToLowerInvariant();
            var remainingArgs = filteredArgs.Skip(1).ToArray();

            switch (command)
            {
                case "/scandir":
                case "--scandir":
                    {
                        var parameters = ParseNamedParameters(remainingArgs);

                        // Parse optional parameters with defaults
                        var timer = parameters.ContainsKey("timer") && int.TryParse(parameters["timer"], out var t) ? t : 60;
                        var timeout = parameters.ContainsKey("timeout") && int.TryParse(parameters["timeout"], out var to) ? to : 10;
                        var depth = parameters.ContainsKey("depth") && int.TryParse(parameters["depth"], out var d) ? d : -1;
                        var fileMask = parameters.ContainsKey("filemask") ? parameters["filemask"] : null;
                        var searchString = parameters.ContainsKey("string") ? parameters["string"] : null;

                        var scanDirCmd = new ScanDirCommand(configManager);
                        scanDirCmd.Execute(timer, timeout, depth, fileMask, searchString);
                    }
                    break;

                case "/mkdir":
                case "--mkdir":
                    if (remainingArgs.Length > 0)
                    {
                        var mkdirCmd = new MkdirCommand(configManager);
                        mkdirCmd.Execute(remainingArgs[0]);
                    }
                    else
                    {
                        OutputManager.WriteLine("Usage: iass.exe /mkdir [path]");
                    }
                    break;

                case "/killproc":
                case "--killproc":
                    if (remainingArgs.Length > 0)
                    {
                        var killProcCmd = new KillProcCommand(configManager);
                        killProcCmd.Execute(remainingArgs[0]);
                    }
                    else
                    {
                        OutputManager.WriteLine("Usage: iass.exe /killproc [id or name]");
                    }
                    break;

                case "/unlock":
                case "--unlock":
                    if (remainingArgs.Length > 0)
                    {
                        var unlockCmd = new UnlockCommand(configManager);
                        unlockCmd.Execute(remainingArgs[0]);
                    }
                    else
                    {
                        OutputManager.WriteLine("Usage: iass.exe /unlock [file name]");
                    }
                    break;

                case "/find-text":
                case "--find-text":
                    {
                        var parameters = ParseNamedParameters(remainingArgs);
                        if (parameters.ContainsKey("file") && parameters.ContainsKey("string"))
                        {
                            var findTextCmd = new FindTextCommand(configManager);
                            findTextCmd.Execute(parameters["file"], parameters["string"]);
                        }
                        else
                        {
                            OutputManager.WriteLine("Usage: iass.exe /find-text -file:\"myfile.txt\" -string:\"something\"");
                        }
                    }
                    break;

                case "/find-file":
                case "--find-file":
                    {
                        var parameters = ParseNamedParameters(remainingArgs);
                        if (parameters.ContainsKey("filemask"))
                        {
                            var searchString = parameters.ContainsKey("string") ? parameters["string"] : null;
                            var depth = parameters.ContainsKey("depth") && int.TryParse(parameters["depth"], out var d) ? d : -1;

                            var findFileCmd = new FindFileCommand(configManager);
                            findFileCmd.Execute(parameters["filemask"], searchString, depth);
                        }
                        else
                        {
                            OutputManager.WriteLine("Usage: iass.exe /find-file -filemask:\"*.txt\" [-string:\"something\"] [-depth:0]");
                        }
                    }
                    break;
                case "/check-file":
                case "--check-file":
                    {
                        var parameters = ParseNamedParameters(remainingArgs);
                        if (parameters.ContainsKey("file"))
                        {
                            var checkFileCmd = new CheckFileCommand(configManager);
                            checkFileCmd.Execute(parameters["file"]);
                        }
                        else
                        {
                            OutputManager.WriteLine("Usage: iass.exe /check-file -file:\"myfile.txt\"");
                        }
                    }
                    break;

                case "/check-dir":
                case "--check-dir":
                    {
                        var parameters = ParseNamedParameters(remainingArgs);
                        if (parameters.ContainsKey("dir"))
                        {
                            var checkDirCmd = new CheckDirCommand(configManager);
                            checkDirCmd.Execute(parameters["dir"]);
                        }
                        else
                        {
                            OutputManager.WriteLine("Usage: iass.exe /check-dir -dir:\"mydir\"");
                        }
                    }
                    break;

                case "/datetime":
                case "--datetime":
                    {
                        var dateTimeCmd = new DateTimeCommand(configManager);
                        dateTimeCmd.Execute();
                    }
                    break;

                default:
                    OutputManager.WriteLine($"Unknown command: {command}");
                    ShowUsage();
                    break;
            }
        }
        else
        {
            // Show usage when no arguments provided
            ShowUsage();
        }
    }

    static (bool suppressOutput, string? outputFile, string[] filteredArgs) ParseGlobalParameters(string[] args)
    {
        bool suppressOutput = false;
        string? outputFile = null;
        var filteredArgsList = new List<string>();

        foreach (var arg in args)
        {
            var lowerArg = arg.ToLowerInvariant();

            if (lowerArg == "-null")
            {
                suppressOutput = true;
            }
            else if (lowerArg.StartsWith("-output-file:"))
            {
                var parts = arg.Substring(1).Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    outputFile = parts[1];
                    // Remove surrounding quotes if present
                    if (outputFile.StartsWith("\"") && outputFile.EndsWith("\"") && outputFile.Length > 1)
                    {
                        outputFile = outputFile.Substring(1, outputFile.Length - 2);
                    }
                }
            }
            else
            {
                // Not a global parameter, keep it for command processing
                filteredArgsList.Add(arg);
            }
        }

        return (suppressOutput, outputFile, filteredArgsList.ToArray());
    }

    static Dictionary<string, string> ParseNamedParameters(string[] args)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var arg in args)
        {
            if (arg.StartsWith("-"))
            {
                var parts = arg.Substring(1).Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    var key = parts[0];
                    var value = parts[1];

                    // Remove surrounding quotes if present
                    if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length > 1)
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    parameters[key] = value;
                }
            }
        }

        return parameters;
    }

    static void ShowUsage()
    {
        OutputManager.WriteLine("IAss - IA Code Assistant Helper");
        OutputManager.WriteLine();
        OutputManager.WriteLine("Usage:");
        OutputManager.WriteLine("  iass.exe                    - Show this help");
        OutputManager.WriteLine();
        OutputManager.WriteLine("Commands:");
        OutputManager.WriteLine("  /scandir [-timer:60] [-timeout:10] [-depth:0] [-filemask:\"*.txt\"] [-string:\"text\"]");
        OutputManager.WriteLine("                              - Start directory scanning");
        OutputManager.WriteLine("                                -timer: scan frequency in seconds (default: 60)");
        OutputManager.WriteLine("                                -timeout: stop after X minutes (default: 10)");
        OutputManager.WriteLine("                                -depth: subdirectory levels, 0=current only (default: all)");
        OutputManager.WriteLine("                                -filemask: comma-separated file patterns (e.g., \"*.txt,*.cs\")");
        OutputManager.WriteLine("                                -string: only list files containing this text");
        OutputManager.WriteLine();
        OutputManager.WriteLine("  /mkdir [path]               - Create directory if it doesn't exist");
        OutputManager.WriteLine();
        OutputManager.WriteLine("  /killproc [id|name]         - Terminate process by name or ID");
        OutputManager.WriteLine();
        OutputManager.WriteLine("  /unlock [file]              - Unblock file");
        OutputManager.WriteLine();
        OutputManager.WriteLine("  /find-text -file:\"myfile.txt\" -string:\"something\"");
        OutputManager.WriteLine("                              - Find lines containing text in a file. Output starts with line number.");
        OutputManager.WriteLine();
        OutputManager.WriteLine("  /find-file -filemask:\"*.txt\" [-string:\"something\"] [-depth:0]");
        OutputManager.WriteLine("                              - Find files by mask, optionally filter by content. Output is bare list of files with full paths.");
        OutputManager.WriteLine();
        OutputManager.WriteLine("  /check-file -file:\"myfile.txt\"");
        OutputManager.WriteLine("                              - Checks if file exists, returns \"exists\" or \"missing\" text message.");
        OutputManager.WriteLine();
        OutputManager.WriteLine("  /check-dir -dir:\"mydir\"");
        OutputManager.WriteLine("                              - Checks if directory exists, returns \"exists\" or \"missing\" text message.");
        OutputManager.WriteLine();
        OutputManager.WriteLine("  /datetime                   - Returns current UTC date and time in ISO 8601 format (yyyy-MM-ddTHH:mm:ssZ).");
        OutputManager.WriteLine();
        OutputManager.WriteLine("Global parameters (can be used with any command):");
        OutputManager.WriteLine("  -null                       - Suppress all output");
        OutputManager.WriteLine("  -output-file:\"file.txt\"     - Redirect all output to file");
        OutputManager.WriteLine();
        OutputManager.WriteLine("Examples:");
        OutputManager.WriteLine("  iass.exe /scandir");
        OutputManager.WriteLine("  iass.exe /scandir -timer:30 -timeout:5 -depth:1");
        OutputManager.WriteLine("  iass.exe /scandir -filemask:\"*.cs,*.txt\" -string:\"TODO\"");
        OutputManager.WriteLine("  iass.exe /find-file -filemask:\"*.cs\" -string:\"class Program\" -depth:2");
        OutputManager.WriteLine("  iass.exe /find-text -file:\"app.log\" -string:\"ERROR\" -output-file:\"errors.txt\"");
        OutputManager.WriteLine("  iass.exe /mkdir \"new/folder\" -null");
        OutputManager.WriteLine("  iass.exe /killproc notepad.exe");
        OutputManager.WriteLine("  iass.exe /check-file -file:\"codefile.cs\"");
        OutputManager.WriteLine("  iass.exe /check-dir -dir:\"src/components\"");
    }
}
