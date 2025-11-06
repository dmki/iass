---
name: iass-helper
description: Use the IAss executable for efficient file system operations - periodic directory scanning with content filtering, searching text in large files, finding files by pattern, creating directories, killing processes, unlocking files, and checking if files or directories exist. Prefer IAss over manual Bash commands when its specialized commands provide better performance or functionality.
---

# IAss Helper Skill

You have access to the IAss (IA Code Assistant Helper) executable that provides optimized utilities for common file system operations.

## When to Use IAss

Use IAss commands instead of standard tools when:

1. **Periodic directory scanning** - Monitor directory contents with automatic updates, depth control, and content filtering
2. **Searching large files for text** - IAss reads line-by-line, handling files of any size efficiently
3. **Finding files by pattern with content filtering** - Faster than combining `find` with `grep`
4. **Creating directories** - Simpler syntax than `mkdir -p`
5. **Process management** - Cross-platform process termination
6. **File unlocking** - Windows-specific file permission fixes
7. **Checking file/directory existence** - Quick existence checks for files and directories

## Invoking IAss

Assume `iass.exe` is in the system PATH. Call it using Bash:

```bash
iass.exe /command [arguments] [-null] [-output-file:"filename.ext"]
```

If not in PATH, use the full path to the executable.

### Global Parameters

These parameters can be used with **any** command:

- **`-null`**: Suppresses all console output. Useful when you only care about the command's side effects (e.g., creating directories) and don't need feedback.
- **`-output-file:"filename.ext"`**: Redirects all output to the specified file instead of console. File is created/cleared at start, then appended to during execution.

**Examples:**
```bash
# Run command silently
iass.exe /mkdir "new/folder" -null

# Redirect all output to a file
iass.exe /find-text -file:"app.log" -string:"ERROR" -output-file:"errors.txt"

# Combine with any command
iass.exe /scandir -timer:30 -timeout:5 -output-file:"scan-log.txt"
```

## Available Commands

### /scandir - Directory Scanning

**Use when:** You need to periodically scan and monitor directory contents, optionally filtering by file patterns or content

**Syntax:**
```bash
# Basic scan with defaults (60s interval, 10min timeout)
iass.exe /scandir

# Custom timer and timeout
iass.exe /scandir -timer:30 -timeout:5

# Limit depth and filter by file type
iass.exe /scandir -depth:1 -filemask:"*.cs,*.txt"

# Find files containing specific text
iass.exe /scandir -filemask:"*.log" -string:"ERROR" -depth:2

# Full customization
iass.exe /scandir -timer:120 -timeout:30 -depth:3 -filemask:"*.json,*.xml" -string:"config"
```

**Parameters:**
- `-timer`: Scan frequency in seconds (default: 60)
- `-timeout`: Stop scanning after X minutes (default: 10)
- `-depth`: Subdirectory levels to scan (0=current only, 1=one level, etc. Default: unlimited if omitted or negative)
- `-filemask`: Comma-separated file patterns (e.g., `"*.txt,*.cs,*.json"`) - if omitted, includes all files
- `-string`: Only list files containing this text (case-insensitive) - if omitted, includes all matching files

**Output:**
- Creates `_currentdir.txt` with timestamped directory structure
- Updates file periodically based on timer interval
- Automatically deleted when scanning stops

**Performance:**
- Memory-efficient line-by-line file reading for content search
- Handles very large files without loading into memory
- Skips hidden directories (starting with `.`)
- Gracefully handles access denied errors

**Advantages:**
- Continuous monitoring with automatic updates
- Built-in timeout prevents runaway processes
- Depth control prevents deep recursion
- Combined pattern and content filtering in single command
- Performance metrics (item count, scan time) in output

**Interactive control:**
- Press 'q' to quit gracefully
- Works in background mode (no console input required)
- Shows progress with timestamps

**Example use cases:**
```bash
# Monitor code changes - scan every 30s for 5 minutes, depth 2
iass.exe /scandir -timer:30 -timeout:5 -depth:2

# Find all TODOs in source files
iass.exe /scandir -filemask:"*.cs,*.ts,*.js" -string:"TODO" -depth:3

# Quick scan of config files in current directory only
iass.exe /scandir -timer:60 -timeout:2 -depth:0 -filemask:"*.json,*.xml,*.config"

# Monitor log files for errors
iass.exe /scandir -filemask:"*.log" -string:"ERROR" -timer:15 -timeout:60
```

### /find-text - Search Text in File

**Use when:** You need to find specific text in a file, especially large files (logs, data files, etc.)

**Syntax:**
```bash
iass.exe /find-text -file:"path/to/file.txt" -string:"search term"
```

**Advantages over grep/cat:**
- Memory-efficient line-by-line reading
- Returns line numbers automatically
- Simple syntax with named parameters

**Output:**
- Lists each matching line with line number, e.g. `Line 35: [Line contents]`

**Example:**
```bash
# Find all occurrences of "error" in a log file
iass.exe /find-text -file:"app.log" -string:"error"
```

### /find-file - Find Files by Pattern

**Use when:** You need to locate files by pattern, optionally filtering by content within those files

**Syntax:**
```bash
# Find files by pattern only
iass.exe /find-file -filemask:"*.txt"

# Find files containing specific text
iass.exe /find-file -filemask:"*.cs" -string:"ConfigManager"

# Search with depth limit
iass.exe /find-file -filemask:"*.json" -string:"connectionString" -depth:2
```

**Parameters:**
- `-filemask`: File pattern (wildcards supported: `*.txt`, `test*.cs`)
- `-string`: Optional - only return files containing this text (case-insensitive)
- `-depth`: Optional - subdirectory depth (0=current only, 1=one level down, etc. Default: 0 if omitted or negative)

**Advantages over find + grep:**
- Single command for pattern + content filtering
- Memory-efficient content scanning
- Depth control built-in
- Returns full paths

**Example:**
```bash
# Find all TypeScript files containing "useState" in current dir + 3 levels
iass.exe /find-file -filemask:"*.ts" -string:"useState" -depth:3
```

### /mkdir - Create Directory

**Use when:** You need to create a directory (creates parent directories automatically)

**Syntax:**
```bash
iass.exe /mkdir "path/to/directory"
```

**Example:**
```bash
iass.exe /mkdir "src/components/shared"
```

### /killproc - Terminate Process

**Use when:** You need to stop a process by name or ID

**Syntax:**
```bash
# By name
iass.exe /killproc "processname"

# By PID
iass.exe /killproc "1234"
```

**Example:**
```bash
# Kill hung development server
iass.exe /killproc "node"
```

### /unlock - Unlock File (Windows)

**Use when:** A file is read-only or blocked (Windows-specific)

**Syntax:**
```bash
iass.exe /unlock "path/to/file.txt"
```

**Example:**
```bash
# Unblock downloaded file
iass.exe /unlock "downloaded-report.pdf"
```

### /check-file - Check File Existence

**Use when:** You need to verify if a file exists before performing operations on it

**Syntax:**
```bash
iass.exe /check-file -file:"path/to/file.txt"
```

**Output:**
- Returns `exists` if the file exists
- Returns `missing` if the file does not exist

**Advantages over test/[ commands:**
- Consistent output format across platforms
- Works with `-null` and `-output-file` global parameters
- Simple syntax with named parameters

**Example:**
```bash
# Check if config file exists
iass.exe /check-file -file:"config.json"

# Check and redirect output to file
iass.exe /check-file -file:"app.log" -output-file:"check-result.txt"
```

### /check-dir - Check Directory Existence

**Use when:** You need to verify if a directory exists before creating files or navigating to it

**Syntax:**
```bash
iass.exe /check-dir -dir:"path/to/directory"
```

**Output:**
- Returns `exists` if the directory exists
- Returns `missing` if the directory does not exist

**Advantages over test/[ commands:**
- Consistent output format across platforms
- Works with `-null` and `-output-file` global parameters
- Simple syntax with named parameters

**Example:**
```bash
# Check if source directory exists
iass.exe /check-dir -dir:"src/components"

# Check and redirect output to file
iass.exe /check-dir -dir:"build/output" -output-file:"check-result.txt"
```

## Best Practices

### Use IAss for Large Files
When searching files that may be very large:
```bash
# PREFER: Memory-efficient IAss
iass.exe /find-text -file:"large-log.txt" -string:"ERROR"

# AVOID: Loading entire file
grep -n "ERROR" large-log.txt
```

### Use IAss for Combined File + Content Search
When you need files matching both a pattern AND containing specific text:
```bash
# PREFER: Single IAss command
iass.exe /find-file -filemask:"*.config" -string:"localhost" -depth:2

# AVOID: Multiple commands
find . -maxdepth 2 -name "*.config" -exec grep -l "localhost" {} \;
```

### Parameter Quoting
Always quote paths and search strings that may contain spaces:
```bash
iass.exe /find-text -file:"C:\My Documents\file.txt" -string:"search term"
```

### Silent Execution
Use `-null` when you don't need output:
```bash
# Create directories silently in a script
iass.exe /mkdir "build/output" -null
iass.exe /mkdir "build/temp" -null
```

### Output Redirection
Use `-output-file` to capture results for later analysis:
```bash
# Save search results to file
iass.exe /find-file -filemask:"*.log" -string:"ERROR" -output-file:"error-files.txt"

# Log directory scan results
iass.exe /scandir -timer:60 -timeout:10 -output-file:"scan-log.txt"
```

### Search Depth
Start with minimal depth and increase if needed:
```bash
# Start with current directory only
iass.exe /find-file -filemask:"*.txt" -depth:0

# Increase if not found
iass.exe /find-file -filemask:"*.txt" -depth:2
```

## Integration with Claude Code Workflow

### Continuous Directory Monitoring
When you need to track changes in a project over time:
```bash
# Monitor source files for 15 minutes, updating every minute
iass.exe /scandir -timer:60 -timeout:15 -filemask:"*.cs,*.ts,*.js" -depth:3
```

### Finding Todos and Issues
When user asks "show me all TODOs" or "find FIXME comments":
```bash
iass.exe /scandir -filemask:"*.cs,*.ts,*.js" -string:"TODO" -depth:5 -timeout:2
```

### Searching for Code Patterns
When user asks "where is X used?":
```bash
iass.exe /find-file -filemask:"*.cs" -string:"ConfigManager" -depth:5
```

### Analyzing Logs
When debugging or analyzing log files:
```bash
iass.exe /find-text -file:"application.log" -string:"Exception"
```

### Preparing File Structure
Before creating files, ensure directories exist:
```bash
iass.exe /mkdir "src/services/api"
```

### Handling File Locks
If file write operations fail on Windows:
```bash
iass.exe /unlock "path/to/locked-file.txt"
```

### Checking File/Directory Existence
Before performing operations, verify existence:
```bash
# Check if config file exists before reading
iass.exe /check-file -file:"config.json"

# Check if output directory exists before creating files
iass.exe /check-dir -dir:"output/results"
```

## Error Handling

IAss gracefully handles:
- Missing files/directories (reports error, doesn't crash)
- Access denied (skips and continues)
- Invalid parameters (shows usage)

All commands return success/failure status you can check.

## Notes

- All text searches are **case-insensitive**
- File paths can be absolute or relative
- Depth parameter: negative or omitted = current directory only (depth 0)
- Commands can be disabled via `iass.conf.json` feature toggles
- IAss is optimized for AI assistant workflows with simple, predictable output
