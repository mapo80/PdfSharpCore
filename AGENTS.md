# Build and Test

This repository contains a multi-project .NET solution.

## Prerequisites

1. Install the .NET 8.0 SDK. On Ubuntu 24.04 or later:

   ```bash
   wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   rm packages-microsoft-prod.deb
   sudo apt-get update
   sudo apt-get install -y dotnet-sdk-8.0
   ```

2. Install Ghostscript and Microsoft TrueType fonts (required for image-based tests):

   ```bash
   sudo apt-get install -y ghostscript ttf-mscorefonts-installer
   fc-cache -f -v
   ```

## Build

Restore dependencies and compile the solution:

```bash
dotnet build PdfSharpCore.sln
```

## Test

Run the test project (net8.0 target):

```bash
dotnet test --framework net8.0 PdfSharpCore.Test/PdfSharpCore.Test.csproj
```
