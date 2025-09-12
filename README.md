# Track³

Track³ (Track Cubed) is a cross-platform mobile application built with .NET MAUI that allows users to save and organize various digital items—such as links, images, songs, or documents—as "Cubed Items" or "Cubes". Each item can be categorized, tagged, described, and easily searched for later.

The application is powered by a secure ASP.NET Core Web API backend connected to an Azure SQL database, with user authentication handled by Microsoft Entra ID.

## Core Features

*   **Cross-Platform:** Runs on Windows and Android from a single codebase.
*   **Secure Authentication:** User sign-in and API security are managed by Microsoft Entra ID.
*   **Persistent Login:** Users remain logged in across application restarts on desktop and mobile.
*   **Full CRUD Functionality:** Users can Create, Read, Update, and Delete their own "Cubed Items."
*   **Dynamic Tagging System:**
    *   Add custom tags to any item.
    *   Tags are displayed on the main list for easy identification.
    *   Features a predictive text system to suggest existing tags as the user types.
*   **Database-Driven Categories:**
    *   Assign an item type from a predefined list (e.g., Link, Image, Song).
    *   This list is managed in the database and fetched dynamically.
    *   Supports fully custom user-entered item types.
*   **Advanced Search & Filtering:**
    *   Full-text search across item names, descriptions, notes, and links.
    *   Filter items by their specific type.
    *   Filter items by tags, with support for **inclusive (OR)** and **exclusive (AND)** search modes.
*   **Modern UI:**
    *   Features a pull-to-refresh list for fetching the latest data.
    *   Adaptive UI that fully respects the user's Light or Dark system theme on all platforms.

## Technology Stack

*   **Frontend:** .NET MAUI
*   **Backend:** ASP.NET Core Web API
*   **Database:** Azure SQL Server (managed with Entity Framework Core)
*   **Authentication:** Microsoft Entra ID
*   **MVVM Framework:** Community Toolkit MVVM (Source Generators)
*   **Primary Language:** C#
*   **Hosting:** Azure App Service (or Azure Container Apps)

## Project Structure

The solution is divided into three main projects:

*   **`TrackCubed.Shared/`**: A class library containing shared models and DTOs (Data Transfer Objects) used by both the API and the MAUI app to ensure consistency and prevent code duplication.
*   **`TrackCubed.Api/`**: The ASP.NET Core Web API project. It handles all business logic, database interactions, and API security.
*   **`TrackCubed.Maui/`**: The .NET MAUI project. It contains all the UI (Views), application logic (ViewModels), and services for the client-side application.

---

## Setup and Configuration

Follow these steps to get the project running from a fresh clone.

### 1. Prerequisites

*   [.NET SDK](https://dotnet.microsoft.com/download) (latest version)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/) with the ".NET Multi-platform App UI development" workload installed.
*   An active [Azure Subscription](https://azure.microsoft.com/free/).
*   Access to [Microsoft Entra ID](https://entra.microsoft.com/) within your Azure tenant.

### 2. Azure Resource Setup

1.  **Create an Azure SQL Database:**
    *   In the Azure portal, create an Azure SQL server and a database (e.g., `TrackCubedDb`).
    *   **Important:** Note down the **server name**, **database name**, **admin login**, and **password**.
    *   Navigate to the SQL server's **Networking** page and check the box for **"Allow Azure services and resources to access this server"**.
    *   Add a firewall rule to allow your local machine's IP address to connect for running database migrations.
    *   From the database page, copy the **ADO.NET connection string**.

### 3. Microsoft Entra ID App Registrations

You need to create **two** App Registrations.

#### A. Backend API (`TrackCubed Web API`)

1.  Create a new App Registration named `TrackCubed Web API`.
2.  Supported account types: `Accounts in this organizational directory only`.
3.  Go to **Expose an API**, set the **Application ID URI** (e.g., `api://<your-api-client-id>`), and save it.
4.  Add a scope named `CubedItems.ReadWrite`.
5.  From the **Overview** page, copy the **Application (client) ID** and **Directory (tenant) ID**.

#### B. Frontend Client (`TrackCubed MAUI App`)

1.  Create a new App Registration named `TrackCubed MAUI App`.
2.  Go to **Authentication**:
    *   Add a platform and select **Mobile and desktop applications**.
    *   Add the redirect URI: `msal<your-maui-client-id>://auth`
    *   Add the redirect URI: `http://localhost` (for the Windows platform).
3.  Go to **API permissions**:
    *   Click "Add a permission" -> "My APIs".
    *   Select your `TrackCubed Web API`.
    *   Grant delegated permission for the `CubedItems.ReadWrite` scope and grant admin consent.
4.  From the **Overview** page, copy the **Application (client) ID**.

### 4. Backend (`.Api`) Configuration

1.  In the `TrackCubed.Api` project, right-click and select **Manage User Secrets**. This will open `secrets.json`.
2.  Add your Azure SQL connection string and Entra ID details here:
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=tcp:your-server.database.windows.net...;Password=YourPassword;"
      },
      "AzureAd": {
        "Instance": "https://login.microsoftonline.com/",
        "TenantId": "<YOUR-TENANT-ID>",
        "ClientId": "<YOUR-API-CLIENT-ID>",
        "Audience": "api://<YOUR-API-CLIENT-ID>"
      }
    }
    ```
3.  **Run Database Migrations:**
    *   Open the Package Manager Console in Visual Studio.
    *   Set the "Default project" to `TrackCubed.Api`.
    *   Run `Update-Database`. This will create all necessary tables, including seeding the initial `ItemTypes` table.

### 5. Frontend (`.Maui`) Configuration

1.  In the `TrackCubed.Maui` project, create a configuration file (e.g., `EntraIdConstants.cs`) with your Entra ID details.
2.  **Android Specific:**
    *   In `Platforms/Android/AndroidManifest.xml`, ensure the `BrowserTabActivity` with the correct `msal...` scheme is present.
    *   In `Platforms/Android/MainActivity.cs`, ensure the `OnActivityResult` override is present.
3.  **Cross-Platform Build:** In `TrackCubed.Maui.csproj`, ensure the `PropertyGroup` for Android is configured for Multidex and sufficient Java Heap Size.

---

## How to Run

1.  **Start the Backend:** Set the `TrackCubed.Api` project as the startup project and run it (F5 or Ctrl+F5).
2.  **Start the Frontend:** Right-click the `TrackCubed.Maui` project and set it as the startup project. Select your target (Windows Machine or an Android Emulator) and run it.

## Deployment to Azure

1.  **Publish the API:** Right-click the `TrackCubed.Api` project and select "Publish." Follow the prompts to create and deploy to an Azure App Service or Container App.
2.  **Configure the Deployed API:** In the App Service/Container App's **Configuration** page:
    *   Add your `DefaultConnection` to the **Connection strings** section.
    *   Add the `AzureAd__ClientId`, `AzureAd__TenantId`, and `AzureAd__Audience` settings to the **Application settings** section (using double underscores `__` as delimiters).
3.  **Update the MAUI App:** In `MauiProgram.cs`, update the `HttpClient`'s `BaseAddress` to point to your live URL.

## Future Work / Roadmap

*   [x] **Implement Full CRUD Functionality**
*   [x] **Build out the tagging system (add/remove tags, filter by tags)**
*   [x] **Implement a full-text search and filtering feature**
*   [ ] Implement specific views/handling for different item types (e.g., image previews, document icons).
*   [ ] Create a User Profile / Settings page.
*   [ ] Build a UI for managing the predefined Item Types.
*   [ ] Implement enhanced UI feedback (toasts/snackbars instead of pop-up alerts).
*   [ ] Investigate offline caching/synchronization for viewing items without a connection.
*   [ ] Add Share functionality to share Cubed Items with other apps and/or social media.