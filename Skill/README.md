# IAss Helper Skill for Claude Code

This skill teaches Claude Code how to use the IAss (IA Code Assistant Helper) executable for efficient file system operations.

## What This Skill Does

The IAss Helper skill enables Claude Code to:
- Search for text within files using memory-efficient line-by-line reading
- Find files by pattern with optional content filtering
- Create directories
- Terminate processes
- Unlock files (Windows)

## Prerequisites

Before installing this skill, you must:

1. **Build IAss** (if not already done):
   ```bash
   cd path/to/IAss
   dotnet build --configuration Release
   ```

2. **Add IAss to PATH** (recommended) or note the full path to the executable:
   - **Windows**: Add `D:\Work\Utilities\IAss\IAss\bin\Release\net9.0\` to your PATH environment variable
   - **Alternative**: Use the full path when calling iass.exe
   - Make sure that bash doesn't convert slashes to windows paths. Use `export MSYS_NO_PATHCONV=1` in Git Bash or set system environment variable `MSYS_NO_PATHCONV=1=1`

3. **Verify Installation**:
   ```bash
   iass.exe
   ```
   Should display usage information.

## Installation

### Option 1: Manual Installation

1. Locate your Claude Code skills directory:
   - **Windows**: `%USERPROFILE%\.claude\skills\`
   - **macOS/Linux**: `~/.claude/skills/`

2. Create a new directory for this skill:
   ```bash
   mkdir ~/.claude/skills/iass-helper
   ```
   (On Windows: `mkdir %USERPROFILE%\.claude\skills\iass-helper`)

3. Copy the `SKILL.md` file to the new directory:
   ```bash
   cp SKILL.md ~/.claude/skills/iass-helper/
   ```
   (On Windows: `copy SKILL.md %USERPROFILE%\.claude\skills\iass-helper\`)

4. Restart Claude Code or reload skills (if applicable)

### Option 2: Copy Entire Skill Directory

1. Copy this entire `Skill` directory to Claude Code's skills directory:
   ```bash
   cp -r Skill ~/.claude/skills/iass-helper
   ```
   (On Windows: `xcopy Skill %USERPROFILE%\.claude\skills\iass-helper /E /I`)

## Usage

The skill must be invoked at the start of each Claude Code session. There are three ways to do this:

### Option 1: Slash Command (Recommended for Manual Use)

1. Copy the `iass.md` command file to your Claude Code commands directory:
   ```bash
   cp iass.md ~/.claude/commands/
   ```
   (On Windows: `copy iass.md %USERPROFILE%\.claude\commands\`)

2. At the start of any Claude Code session where you need IAss, type:
   ```
   /iass
   ```

This is the recommended approach when deploying to other machines, as it's simple and doesn't require additional configuration.

### Option 2: Automatic Loading via Hook (Advanced)

For automatic loading at the start of every session, add a hook to your Claude Code settings:

1. Edit `~/.claude/settings.json` (Windows: `%USERPROFILE%\.claude\settings.json`)

2. Add the following hook configuration:
   ```json
   {
     "hooks": {
       "userPromptSubmitHook": {
         "command": "echo Loading iass-helper skill... && exit 0",
         "onSuccess": "Please invoke the iass-helper skill to enable IAss commands."
       }
     }
   }
   ```

3. The skill will be automatically loaded at the start of each session.

### Option 3: Manual Invocation in Chat

You can also ask Claude Code directly:
```
Please invoke the iass-helper skill
```

### After Loading

Once the skill is loaded for the session, Claude Code will automatically know when to use IAss commands. You can simply ask:

- "Search for 'ConfigManager' in all C# files"
- "Find the error messages in app.log"
- "Locate all JSON files containing 'connectionString'"
- "Create the directory structure for my new component"

Claude Code will use the appropriate IAss commands based on the skill instructions.

## Verifying the Skill is Loaded

After installation, you can verify the skill is available by asking Claude Code:

```
What skills do you have available?
```

or

```
Do you have the iass-helper skill?
```

## Configuration

The skill assumes `iass.exe` is in your PATH. If you prefer to use a specific path, you can modify the `SKILL.md` file and replace:

```bash
iass.exe /command
```

with:

```bash
D:\Work\Utilities\IAss\IAss\bin\Release\net9.0\iass.exe /command
```

## Troubleshooting

### "iass.exe not found"

- Ensure IAss is built: `dotnet build --configuration Release`
- Add the executable directory to your PATH, or
- Edit `SKILL.md` to use the full path to iass.exe

### "Command is disabled in configuration"

Check `iass.conf.json` in the IAss executable directory and ensure the relevant feature is enabled:

```json
{
  "features": {
    "findText": true,
    "findFile": true,
    "mkdir": true,
    "killproc": true,
    "unlock": true
  }
}
```

### Skill Not Loading

- Ensure `SKILL.md` is in the correct directory (`~/.claude/skills/iass-helper/`)
- Verify the YAML frontmatter is properly formatted
- Restart Claude Code

## Updating the Skill

To update the skill after modifications:

1. Edit the `SKILL.md` file in the skills directory
2. Reload Claude Code or restart it
3. Test the updated behavior

## Uninstallation

To remove the skill:

```bash
rm -rf ~/.claude/skills/iass-helper
```
(On Windows: `rmdir /S %USERPROFILE%\.claude\skills\iass-helper`)

## Additional Resources

- [IAss Project Documentation](../CLAUDE.md) - Architecture and development guide
- [IAss README](../README.md) - Main project documentation
- [Claude Skills Repository](https://github.com/anthropics/skills) - Official skills documentation
