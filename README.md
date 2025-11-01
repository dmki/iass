# IAss - Intelligent Assistant for Claude Code on Windows

On Windows, Claude Code often struggles with simplest operations, such as directory creation, process termination, searching for files and content in files. With new Skills feature, you can teach Claude to use other apps to perform particular operations, and iAss is such app. You install it, then instruct Claude to use it instead of bash / powershell, and Claude should consume less context and perform basic operations flawlessly.

It is a .NET 9 CLI application, it comes with installer which sets up PATH and one environment variable, and command that you add to Claude Code ("/iass"), which activates this skill.

More features will be added as soon as there is the need - e.g. when we'll notice that Claude Code struggles with particular operations. If you'd like to add particular feature - feel free to discuss!

## Features

Note: When you execute this app with no command line parameters, it displays full help.

### Directory Scanning
- Periodically scans the current directory for sub-directories and files
- Saves the directory structure to `_currentdir.txt` at configurable intervals
- Includes a timestamp header: "Current state of [directory name] as of [current date and time]"

### Command Line Utilities
- **Directory Creation**: Create directories if they don't exist
- **Process Management**: Terminate processes by name or ID
- **File Unlocking**: Unblock files that cannot be written to
- **Find files by name and / or contents**: Will scan directory for files by specific mask and only return those which have particular string in them.

### Configuration
- All features can be enabled/disabled via `iass.conf.json`
- Directory scan interval is configurable

## Installation

1. Ensure you have .NET 9 SDK installed
2. Clone or download the repository
3. Build the project: `dotnet build`
4. The executable will be available in `bin/Debug/net9.0/IAss.exe`

## Usage

### Run Directory Scanner
```bash
dotnet run
```
Or execute the built application directly to start periodic directory scanning.

### Create Directory
```bash
dotnet run -- /mkdir [path]
```

### Kill Process
```bash
dotnet run -- /killproc [id or name]
```

### Unlock File
```bash
dotnet run -- /unlock [file name]
```

## Configuration

The application uses `iass.conf.json` for configuration:

```json
{
  "directoryScanInterval": 30,
  "features": {
    "directoryScan": true,
    "mkdir": true,
    "killproc": true,
    "unlock": true
  }
}
```

- `directoryScanInterval`: Time in seconds between directory scans (default: 30)
- `features`: Individual toggles for each feature

## Dependencies

- .NET 9 Runtime
- Microsoft.Extensions.Configuration.Json
- Microsoft.Extensions.Configuration.Binder
- System.Security.Principal.Windows
- System.ServiceProcess.ServiceController

## Building from Source

```bash
dotnet restore
dotnet build
```