# Event Management System

## Overview

This repository contains the Event Management System project built with .NET Framework. The project uses various NuGet packages listed in the `packages.config` file to function properly.

## Getting Started

To successfully build and run this project, please follow the steps below to restore the required NuGet packages.

## Package Restoration

Since the `packages` folder is not included in the repository, you need to restore the packages using the `packages.config` file. The file is located in the following path:

```
EventManagementSystem/EventManagementSystem/packages.config
```

### Steps to Restore Packages

1. **Using Visual Studio:**

   - Open the solution in Visual Studio.
   - Right-click on the solution in **Solution Explorer**.
   - Select **Restore NuGet Packages**.

2. **Using NuGet CLI:**

   - Open a terminal or command prompt.
   - Navigate to the project directory:
     ```bash
     cd EventManagementSystem/EventManagementSystem
     ```
   - Run the following command:
     ```bash
     nuget restore packages.config -PackagesDirectory packages
     ```

3. **Using dotnet CLI:** (For SDK-style projects)

   - Open a terminal and navigate to the project directory.
   - Run:
     ```bash
     dotnet restore
     ```

### Troubleshooting

- **NuGet CLI Not Found:** Download it from [NuGet Official Site](https://www.nuget.org/downloads) and ensure it's in your system's PATH.
- **Clearing Cache:** If you encounter issues, try clearing the NuGet cache:
  ```bash
  nuget locals all -clear
  ```

## Additional Information

If you experience any issues, please create an issue on this repository.

---

