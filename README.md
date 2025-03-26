# Event Management System

## Overview
This repository contains the source code for the Event Management System, a web-based application built using ASP.NET MVC.

## Getting Started
### Prerequisites
- .NET Framework 4.7.2
- Visual Studio
- SQL Server

### Cloning the Repository
```sh
git clone https://github.com/kwanele-Cyber/EventManagementSystem.git
cd EventManagementSystem/EventMangementSystem
```

### Restoring Dependencies
This project uses NuGet for dependency management. Since the `packages` folder is not included in the repository, you need to restore the required packages using:
```sh
nuget restore EventMangementSystem.sln
```

Alternatively, in Visual Studio:
1. Open the solution (`EventMangementSystem.sln`).
2. Go to `Tools` > `NuGet Package Manager` > `Manage NuGet Packages for Solution`.
3. Click `Restore` to download missing dependencies.

### Configuring Web.config
Before running the application, update the `Web.config` file located at `EventManagementSystem/EventMangementSystem/Web.config`:

#### Database Configuration
Modify the `<connectionStrings>` section to specify your database details:
```xml
<connectionStrings>
    <add name="DefaultConnection" connectionString="Data Source=YOUR_DATABASE_SERVER;Initial Catalog=YOUR_DATABASE_NAME;Integrated Security=True" providerName="System.Data.SqlClient" />
</connectionStrings>
```

#### App Settings
Set the required values for third-party integrations like PayPal and Facebook:
```xml
<appSettings>
    <add key="FacebookAppId" value="YOUR_FACEBOOK_APP_ID" />
    <add key="FacebookAppSecret" value="YOUR_FACEBOOK_APP_SECRET" />
    <add key="PayPalClientId" value="YOUR_PAYPAL_CLIENT_ID" />
    <add key="PayPalClientSecret" value="YOUR_PAYPAL_CLIENT_SECRET" />
</appSettings>
```

#### Email Configuration
For email notifications, update the SMTP settings:
```xml
<system.net>
    <mailSettings>
        <smtp deliveryMethod="Network">
            <network host="smtp.gmail.com" port="587" enableSsl="true" userName="YOUR_EMAIL" password="YOUR_PASSWORD" />
        </smtp>
    </mailSettings>
</system.net>
```

### Running the Application
1. Open the solution in Visual Studio.
2. Build the project (`Ctrl + Shift + B`).
3. Press `F5` to start debugging or `Ctrl + F5` to run without debugging.

## License
This project is licensed under the MIT License.

