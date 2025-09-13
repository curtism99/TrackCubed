# Track³

Track³ (Track Cubed) is a cross-platform mobile application built with .NET MAUI that allows users to save and organize various digital items—such as links, images, songs, or documents—as "Cubed Items." Each item can be categorized, described, and tagged within a personal library, making it easy to find and reuse your important saved content.

The application is powered by a secure ASP.NET Core Web API backend connected to an Azure SQL database, with user authentication handled by Microsoft Entra ID.

## Core Features

*   **Cross-Platform:** Runs natively on Windows and Android from a single codebase.
*   **Secure Authentication:** User sign-in and API security are managed by Microsoft Entra ID.
*   **Persistent Login:** Users remain logged in across application restarts on both desktop and mobile platforms.
*   **Full CRUD Functionality:** Users can Create, Read, Update, and Delete their own "Cubed Items."
*   **Personalized Tagging System:**
    *   Add custom, user-owned tags to any item.
    *   Features a predictive text system to suggest your existing tags as you type.
*   **Database-Driven & Custom Categories:**
    *   Assign an item type from a predefined, database-managed list (e.g., Link, Image, Song).
    *   Supports fully custom, user-entered item types for maximum flexibility.
*   **Advanced Search & Filtering:**
    *   Full-text search across item names, descriptions, notes, and links.
    *   Filter items by their specific type.
    *   Filter items by tags with support for **inclusive (OR)** and **exclusive (AND)** search modes.
*   **Modern & Responsive UI:**
    *   Features a pull-to-refresh list for fetching the latest data.
    *   Adaptive UI that fully respects the user's Light or Dark system theme.
    *   Custom-designed navigation bar for a branded, consistent look and feel.
*   **Comprehensive Settings:**
    *   Users can manage their application theme (Light, Dark, or System Default).
    *   Includes a secure "Wipe Data" feature with a random phrase confirmation to start fresh.
    *   Provides "About" information, including the app version and a privacy policy link.

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

*   **`TrackCubed.Shared/`**: A class library containing shared models and DTOs (Data Transfer Objects) used by both the API and the MAUI app.
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
    *   Note down the **server name**, **database name**, **admin login**, and **password**.
    *   Navigate to the SQL server's **Networking** page and check the box for **"Allow Azure services and resources to access this server"**.
    *   Add a firewall rule to allow your local machine's IP address to connect.
    *   Copy the **ADO.NET connection string**.

### 3. Microsoft Entra ID App Registrations

Create **two** App Registrations for the frontend and backend. *Detailed instructions can be found in the project's development history.*

### 4. Backend (`.Api`) Configuration

1.  In the `TrackCubed.Api` project, configure your user secrets (`secrets.json`) with your Azure SQL connection string and Entra ID details.
2.  **Run Database Migrations:**
    *   Open the Package Manager Console in Visual Studio, target the `TrackCubed.Api` project.
    *   Run `Update-Database`. This will create all necessary tables and pre-populate the `ItemTypes` table.

### 5. Frontend (`.Maui`) Configuration

1.  In the `TrackCubed.Maui` project, configure your Entra ID constants file.
2.  Ensure Android-specific configurations (`AndroidManifest.xml`, `MainActivity.cs`, and `.csproj` settings for Multidex) are in place.

---

## How to Run

1.  **Start the Backend:** Set the `TrackCubed.Api` project as the startup project and run it.
2.  **Start the Frontend:** Set the `TrackCubed.Maui` project as the startup project. Select your target (Windows Machine or an Android Emulator) and run it.

## Deployment

1.  **Publish the API:** Publish the `TrackCubed.Api` project to an Azure App Service or Container App.
2.  **Configure in Azure:** In the App Service/Container App's **Configuration** page, securely add the `DefaultConnection` string and the `AzureAd__` settings.
3.  **Update the MAUI App:** In `MauiProgram.cs`, update the `HttpClient`'s `BaseAddress` to point to your live URL.
4.  **Package the Client:** Use Visual Studio's "Publish" feature to create a single-file `.exe` for Windows or a signed `.apk`/`.aab` for Android.

## Future Work / Roadmap

*   [x] **Implement Full CRUD Functionality**
*   [x] **Build out the tagging system (add/remove tags, filter by tags)**
*   [x] **Implement a full-text search and filtering feature**
*   [ ] **Create a "Tag Explorer" page:** A dedicated screen to view all of a user's tags, see how many items are associated with each, and initiate a search by tapping a tag.
*   [ ] **Implement specific views/handling for item types:** Add image previews, link previews, or custom icons based on the `ItemType`.
*   [ ] **Enhance UI Feedback:** Replace blocking `DisplayAlert` popups with non-intrusive "Toasts" or "Snackbars" for confirmations (e.g., "Item Saved!").
*   [ ] **Investigate Offline Caching:** Allow users to view and possibly edit items even without an internet connection, with synchronization later.