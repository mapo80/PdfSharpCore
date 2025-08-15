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

2. Install Ghostscript and Microsoft TrueType fonts (required for image-based tests).
   The `ttf-mscorefonts-installer` package prompts for acceptance of the Microsoft EULA and will otherwise block waiting for input.
   To install non-interactively, pre-accept the license and then install:

   ```bash
   echo ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true | sudo debconf-set-selections
   sudo apt-get install -y ghostscript ttf-mscorefonts-installer
   fc-cache -f -v  # refresh the font cache
   ```
   (If you run the install command without pre-accepting the EULA, be prepared to confirm it manually when prompted.)

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
