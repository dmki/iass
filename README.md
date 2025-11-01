# IAss - Intelligent Assistant for Claude Code on Windows

On Windows, Claude Code often struggles with simplest operations, such as directory creation, process termination, searching for files and content in files. With new Skills feature, you can teach Claude to use other apps to perform particular operations, and iAss is such app. You install it, then instruct Claude to use it instead of bash / powershell, and Claude should consume less context and perform basic operations flawlessly.

This is a .NET 9 CLI application designed to assist AI code assistants with common operations that are often difficult in Windows environments. It provides a set of command-line utilities optimized for performance and reliability, consuming less context than traditional bash/PowerShell commands.

The application comes with an installer that sets up the PATH environment variable and integrates with Claude Code as a Skill through the "/iass" command.

## Features

IAss provides the following core utilities:

- **Directory Scanning** (`/scandir`): Periodically scan the current directory with optional file filtering and content search
- **Directory Creation** (`/mkdir`): Create directories and nested paths
- **Process Management** (`/killproc`): Terminate processes by name or ID
- **File Unlocking** (`/unlock`): Unblock files that cannot be written to
- **Text Search** (`/find-text`): Search for text patterns within a single file
- **File Search** (`/find-file`): Search for files by name pattern with optional content filtering

All features can be enabled/disabled via `iass.conf.json`, and most support global output control parameters.

## Installation

1. Ensure you have .NET 9 SDK installed
2. Clone or download the repository
3. Build the project: `dotnet build`
4. The executable will be available in `bin/Debug/net9.0/IAss.exe`

## Usage

Run `iass.exe` with no arguments to display the help menu. All commands support optional global parameters for output control.

### Directory Scanning (/scandir)

Start periodic directory scanning with optional filtering and content search.

**Syntax:**
```bash
iass.exe /scandir [-timer:60] [-timeout:10] [-depth:0] [-filemask:"*.txt"] [-string:"text"]
```

**Parameters:**
- `-timer`: Scan frequency in seconds (default: 60)
- `-timeout`: Stop scanning after X minutes (default: 10)
- `-depth`: Subdirectory depth limit, 0 = current directory only (default: all)
- `-filemask`: Comma-separated file patterns, e.g., "*.txt,*.cs" (optional)
- `-string`: Only list files containing this text (optional)

**Examples:**
```bash
iass.exe /scandir
iass.exe /scandir -timer:30 -timeout:5 -depth:1
iass.exe /scandir -filemask:"*.cs,*.txt" -string:"TODO"
```

**Output:**
- Creates/updates `_currentdir.txt` with directory structure and timestamp
- Respects `filefilter.txt` patterns if present
- Includes file matches when `-string` parameter is specified

### Create Directory (/mkdir)

Create a directory if it doesn't already exist.

**Syntax:**
```bash
iass.exe /mkdir [path]
```

**Examples:**
```bash
iass.exe /mkdir "new/folder"
iass.exe /mkdir "D:\path\to\new\dir" -null
```

### Terminate Process (/killproc)

Terminate a process by name or ID.

**Syntax:**
```bash
iass.exe /killproc [id or name]
```

**Examples:**
```bash
iass.exe /killproc notepad.exe
iass.exe /killproc 1234
```

**Note:** May require elevated privileges for system processes.

### Unlock File (/unlock)

Unblock a file that is locked or cannot be written to.

**Syntax:**
```bash
iass.exe /unlock [file]
```

**Examples:**
```bash
iass.exe /unlock "D:\path\to\file.txt"
iass.exe /unlock locked_file.dll
```

### Find Text in File (/find-text)

Search for a text string within a single file and return all matching lines.

**Syntax:**
```bash
iass.exe /find-text -file:"path/to/file.txt" -string:"search term"
```

**Parameters:**
- `-file`: Path to the file to search (required)
- `-string`: Text to search for (required, case-insensitive)

**Output:**
- Returns line number and full line content for each match
- Handles large files efficiently with line-by-line reading

**Examples:**
```bash
iass.exe /find-text -file:"app.log" -string:"ERROR"
iass.exe /find-text -file:"source.cs" -string:"class Program" -output-file:"results.txt"
```

### Find Files (/find-file)

Search for files matching a pattern, optionally filtering by content.

**Syntax:**
```bash
iass.exe /find-file -filemask:"*.txt" [-string:"search term"] [-depth:0]
```

**Parameters:**
- `-filemask`: File pattern to match, e.g., `*.txt`, `test*.cs` (required)
- `-string`: Optional text to search within files (case-insensitive)
- `-depth`: Subdirectory depth, 0 = current directory only (default: -1 = all)

**Output:**
- Returns full file paths for all matches
- When `-string` is specified, only files containing that text are returned
- Handles large files efficiently with line-by-line reading

**Examples:**
```bash
iass.exe /find-file -filemask:"*.cs"
iass.exe /find-file -filemask:"*.cs" -string:"class Program" -depth:2
iass.exe /find-file -filemask:"*.log" -string:"WARNING" -output-file:"warnings.txt"
```

### Global Parameters

All commands support the following global parameters:

- `-null`: Suppress all output
- `-output-file:"filename"`: Redirect all output to a file

**Examples:**
```bash
iass.exe /mkdir "folder" -null
iass.exe /find-text -file:"log.txt" -string:"error" -output-file:"results.txt"
```

## Configuration

The application uses `iass.conf.json` for configuration, located in the application's base directory. The configuration file is created automatically with defaults if it doesn't exist.

**Configuration Structure:**
```json
{
  "directoryScanInterval": 30,
  "features": {
    "directoryScan": true,
    "mkdir": true,
    "killproc": true,
    "unlock": true,
    "findText": true,
    "findFile": true
  }
}
```

**Configuration Options:**
- `directoryScanInterval`: Time in seconds between directory scans when using `/scandir` (default: 30)
- `features`: Individual toggles to enable/disable each command
  - `directoryScan`: Enable `/scandir` command
  - `mkdir`: Enable `/mkdir` command
  - `killproc`: Enable `/killproc` command
  - `unlock`: Enable `/unlock` command
  - `findText`: Enable `/find-text` command
  - `findFile`: Enable `/find-file` command

### File Filtering for Directory Scanning

Create a `filefilter.txt` file in the current directory to exclude files and directories from scanning. This file is used by the `/scandir` command.

**Filter Pattern Syntax:**
- **Prefix wildcard**: `name*` excludes items starting with "name"
- **Suffix wildcard**: `*.ext` excludes items ending with ".ext"
- **Exact match**: `dirname` excludes exact matches
- **Comments**: Lines starting with `#` are treated as comments
- **Default behavior**: If no `filefilter.txt` exists, dot-prefixed directories (`.git`, `.vs`, etc.) are automatically excluded

**Example filefilter.txt:**
```
# Exclude version control directories
.git
.github
.vs

# Exclude common build directories
bin*
obj*
node_modules*

# Exclude temporary files
*.tmp
*.log
~*
```

## Building from Source

### Prerequisites
- .NET 9 SDK installed

### Build Steps

**Debug Build:**
```bash
dotnet build
```

**Release Build:**
```bash
dotnet build --configuration Release
```

The compiled executable will be available at:
- Debug: `bin/Debug/net9.0/IAss.exe`
- Release: `bin/Release/net9.0/IAss.exe`

## Manual installation (when installer is not present or too much hassle)

1. Ensure the directory with iass.exe is in system-wide PATH
2. Create environment variable MSYS_NO_PATHCONV and set its value to 1
(this disables git bash conversion of "/" command line parameters to fully qualified directory names. In other words, now you can use "/" in command line parameters)
3. Create skills directory in your %userprofile%\.claude directory.
4. In skills directory, create "iass-helper" directory.
5. Copy "skill.md" file from this project / release archive to iass-helper directory.
6. Copy "iass.md" file to your %userprofile%\.claude\commands directory. This will enable the "/iass" command in Claude Code.
7. Execute iass.exe to ensure it can run. It might ask for .NET 9 runtime if it's not installed.

Then, in new session of Claude Code in new terminal session, check that you have /iass command. Run it, then ask Claude Code to create new directory. It should execute "iass.exe /mkdir [directory name]" instead of PowerShell / bash command. If directory can't be created - check the output. If output is simple iass' help, it means you didn't set up the environment variable. But if the directory was created, the assistant is functional and ready to serve.

## Development Modes

When working with the project using `dotnet run`:

**Interactive Directory Scanning:**
```bash
dotnet run
```
Press 'q' to quit.

**Debug Mode with Timeout:**
```bash
dotnet run -- /debug
```
Runs directory scanning with a 1-minute timeout (useful for testing).

**Execute Specific Commands:**
```bash
dotnet run -- /mkdir [path]
dotnet run -- /killproc [id or name]
dotnet run -- /unlock [file name]
dotnet run -- /find-text -file:"myfile.txt" -string:"something"
dotnet run -- /find-file -filemask:"*.txt" -string:"something" -depth:0
```

## Dependencies

- .NET 9 Runtime
- Microsoft.Extensions.Configuration.Json
- Microsoft.Extensions.Configuration.Binder
- System.Security.Principal.Windows
- System.ServiceProcess.ServiceController

## Platform Considerations

IAss is designed specifically for Windows environments and includes Windows-specific functionality:

- **File Operations**: Uses Windows file locking APIs
- **Process Management**: Uses Windows Process Control
- **Privileges**: Some operations (like unlocking system files or terminating system processes) may require elevated privileges
- **Path Handling**: Automatically resolves relative paths to absolute paths using Windows conventions

## Troubleshooting

**"Feature is disabled in configuration"**
- Check `iass.conf.json` and ensure the feature toggle is set to `true`

**"Access Denied" errors**
- Some operations require elevated (admin) privileges
- Try running the command prompt as Administrator

**File not found with absolute path**
- Verify the path uses correct Windows path separators or use quotes around the path

**Directory scanning not creating _currentdir.txt**
- Ensure `directoryScan` feature is enabled in `iass.conf.json`
- Check that the current directory is writable

## Contributing

To add new commands to IAss:

1. Create a new command class file (e.g., `MyCommand.cs`)
2. Implement the command with a constructor taking `ConfigManager`
3. Add the feature toggle to `FeaturesConfig` in `Config.cs`
4. Add the command case to the switch statement in `Program.cs`
5. Add usage text to the `ShowUsage` method in `Program.cs`
6. Update this README with the new command documentation