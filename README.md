# Track³

Track³ (Track Cubed) is a cross-platform mobile application built with .NET MAUI that allows users to save and organize various digital items—such as links, images, songs, or documents—as "Cubed Items." Each item can be categorized, described, and tagged within a personal library, making it easy to find and reuse your important saved content.

The application is powered by a secure ASP.NET Core Web API backend connected to an Azure SQL database, with user authentication handled by Microsoft Entra ID.

## Core Features

*   **Cross-Platform:** Runs natively on Windows and Android from a single codebase.
*   **Secure Authentication:** User sign-in and API security are managed by Microsoft Entra ID.
*   **Persistent Login:** Users remain logged in across application restarts on both desktop and mobile platforms.
*   **Full CRUD Functionality:** Users can Create, Read, Update, and Delete their own "Cubed Items."
*   **Personalized & Private Data:**
    *   All user-generated content, including **tags and custom item types**, is owned by and private to the user, linked via foreign keys.
    *   A robust "Wipe Data" feature allows users to securely delete all of their content from the database.
*   **Advanced Tagging System:**
    *   Add custom tags to any item.
    *   A predictive text system suggests a user's own existing tags as they type on both the main filter and add/edit pages.
*   **Dynamic & Relational Categories:**
    *   Assign an item type from a predefined system list or create custom, user-owned types that automatically appear in the dropdown for future use.
    *   Uses a foreign key relationship for performance and data integrity.
*   **High-Performance UI & Data Handling:**
    *   Features an **infinite scrolling** list that loads data in pages, ensuring fast startup times even with thousands of items.
    *   Client-side search and filtering inputs are **debounced** to prevent excessive API calls and maintain a smooth UI.
    *   Includes a pull-to-refresh gesture for fetching the latest data.
*   **Advanced Search & Filtering:**
    *   Full-text search across all relevant item fields.
    *   Filter items by any system or custom item type.
    *   Filter items by tags, with support for **inclusive (OR)** and **exclusive (AND)** search modes.
*   **Modern & Responsive UI:**
    *   Adaptive UI that fully respects the user's Light or Dark system theme.
    *   Custom-designed, branded navigation bar for a consistent look and feel.
    *   Data-driven icons (emojis) thematically represent different item types.
*   **Comprehensive Settings:**
    *   Users can manage their application theme (Light, Dark, or System Default).
    *   Provides a "Data Management" section to clean up unused, orphaned tags and custom item types.
    *   Includes "About" information, app version, and a privacy policy link.

## Technology Stack

*   **Frontend:** .NET MAUI
    *   _UI Controls:_ FFImageLoading.Maui (for performant, cached images)
*   **Backend:** ASP.NET Core Web API
    *   _HTML Parsing:_ HtmlAgilityPack (for link previews)
*   **Database:** Azure SQL Server (managed with Entity Framework Core)
*   **Authentication:** Microsoft Entra ID
*   **MVVM Framework:** Community Toolkit MVVM (Source Generators)
*   **Primary Language:** C#
*   **Hosting:** Azure App Service (or Azure Container Apps)

## Project Structure

The solution is divided into three main projects:

*   **`TrackCubed.Shared/`**: A class library containing shared models (database entities) and DTOs used by both the API and MA-UI apps.
*   **`TrackCubed.Api/`**: The ASP.NET Core Web API project. Handles all business logic, database interactions, and API security.
*   **`TrackCubed.Maui/`**: The .NET MAUI project. Contains all the UI (Views), application logic (ViewModels), and services for the client app.

---

## Setup and Configuration

Follow these steps to get the project running from a fresh clone.

### 1. Prerequisites

*   [.NET SDK](https://dotnet.microsoft.com/download) (latest version)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/) with the ".NET Multi-platform App UI development" workload.
*   An active [Azure Subscription](https://azure.microsoft.com/free/).
*   Access to [Microsoft Entra ID](https://entra.microsoft.com/).

### 2. Azure Resource Setup

1.  **Create an Azure SQL Database:** Create a server and database, configure networking to allow access, and copy the **ADO.NET connection string**.
2.  **(Optional for Previews)** Create an Azure Blob Storage account if you plan to store uploaded images.

### 3. Microsoft Entra ID App Registrations

Create **two** App Registrations for the frontend client and backend API.

### 4. Backend (`.Api`) Configuration

1.  In the `TrackCubed.Api` project, configure your user secrets (`secrets.json`) with your Azure SQL connection string and Entra ID details.
2.  **Run Database Migrations:** Open the Package Manager Console, target the `TrackCubed.Api` project, and run `Update-Database`. This will create all necessary tables and pre-populate the `SystemItemTypes` table.

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
*   [ ] **Implement Link Previews:** Re-enable and debug the client-side UI for displaying fetched link preview images and metadata.
*   [ ] **Enhance UI Feedback:** Replace blocking `DisplayAlert` popups with non-intrusive "Toasts" or "Snackbars."
*   [ ] **Support Image Uploads:** Implement functionality for the "Image" `ItemType` to allow users to select an image from their device, upload it to Azure Blob Storage, and store the URL.
*   [ ] **Investigate Offline Caching:** Allow users to view and possibly edit items even without an internet connection, with synchronization later.