# .NET SDK Version Management

## Overview

This project uses `global.json` to specify the required .NET SDK version with a flexible rollForward policy.

## Current Configuration

```json
{
  "sdk": {
    "version": "8.0.302",
    "rollForward": "latestPatch"
  }
}
```

## What This Means

### Minimum Version: 8.0.302
The project requires at least .NET SDK version 8.0.302.

### Roll Forward Policy: latestPatch
The `"rollForward": "latestPatch"` setting allows the .NET CLI to use **any newer patch version** within the same feature band (8.0.3xx).

**Examples of compatible versions:**
- ✅ 8.0.302 (minimum)
- ✅ 8.0.400
- ✅ 8.0.416 (latest as of Nov 2024)
- ✅ Any future 8.0.4xx, 8.0.5xx, etc.
- ❌ 8.0.201 (too old)
- ❌ 9.0.x (different major/minor)

### Why latestPatch?

**Benefits:**
1. **Security**: Automatically gets critical security patches
2. **Stability**: Only patches, no breaking changes
3. **Flexibility**: Works with newer SDK installations
4. **CI/CD**: Reduces pipeline failures from SDK version mismatches

**No Downside:** Patch versions only include bug fixes and security updates, never breaking changes.

## Checking Your SDK Version

```bash
dotnet --version
# Should output: 8.0.302 or higher (e.g., 8.0.416)
```

## Installing/Updating the SDK

If you don't have a compatible version:

1. Visit: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download the latest .NET 8 SDK installer
3. Install and verify: `dotnet --version`

## If You Get SDK Version Warnings

If you see warnings like:
```
The SDK version specified in global.json [8.0.302] was not found
```

**Solution 1 (Recommended):** Install .NET 8 SDK 8.0.302 or newer
```bash
# Windows (winget)
winget install Microsoft.DotNet.SDK.8

# macOS (brew)
brew install dotnet-sdk

# Or download from: https://dotnet.microsoft.com/download/dotnet/8.0
```

**Solution 2 (Not Recommended):** You can override, but you'll miss the benefits:
```bash
# This disables global.json - not recommended
dotnet build --ignore-failed-sources
```

## For Project Maintainers

### Updating the Required SDK Version

When updating to a newer .NET 8 SDK version:

```bash
# Update global.json to new minimum version
dotnet new globaljson --sdk-version 8.0.XXX --force

# Then manually add rollForward
# Edit global.json and add: "rollForward": "latestPatch"
```

### Rollforward Policy Options

- **latestPatch** (Current/Recommended): Latest patch in same feature band
- **patch**: Exact feature band, latest patch
- **feature**: Can roll to newer feature bands
- **minor/major**: More permissive (not recommended for stability)
- **disable**: Exact version only (too strict, not recommended)

**Why we chose latestPatch:** Balance of security, stability, and flexibility.

## References

- [.NET global.json documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json)
- [.NET SDK version selection](https://learn.microsoft.com/en-us/dotnet/core/versions/selection)
- [.NET 8 SDK releases](https://github.com/dotnet/core/blob/main/release-notes/8.0/8.0-supported-os.md)

---

**Last Updated:** November 18, 2024
