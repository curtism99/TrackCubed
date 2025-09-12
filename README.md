# TrackCubed

TrackCubed is a cross-platform mobile application built with .NET MAUI that allows users to save and organize various digital items—such as links, images, songs, or documents—as "Cubed Items." Each item can be tagged, described, and easily searched for later.

The application is powered by a secure ASP.NET Core Web API backend connected to an Azure SQL database, with user authentication handled by Microsoft Entra ID.

## Core Features

*   **Cross-Platform:** Runs on Windows and Android from a single codebase.
*   **Secure Authentication:** User sign-in and API security are managed by Microsoft Entra ID.
*   **Persistent Login:** Users remain logged in across application restarts on desktop and mobile.
*   **CRUD Functionality:** Users can create, view, and delete their own "Cubed Items."
*   **Modern UI:** Features a pull-to-refresh list and an adaptive UI that respects the user's Light or Dark system theme.
*   **Scalable Backend:** The ASP.NET Core Web API and Azure SQL database provide a robust and scalable foundation.

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

## How to Run

1.  **Start the Backend:** Set the `TrackCubed.Api` project as the startup project and run it (F5 or Ctrl+F5). A browser window with the Swagger UI should appear.
2.  **Start the Frontend:** Right-click the `TrackCubed.Maui` project and set it as the startup project. Select your target (Windows Machine or an Android Emulator) from the debug dropdown and run it.

## Deployment to Azure

1.  **Publish the API:** Right-click the `TrackCubed.Api` project and select "Publish." Follow the prompts to create and deploy to an Azure App Service.
2.  **Configure the Deployed API:** In the Azure App Service's **Configuration** page:
    *   Add your `DefaultConnection` to the **Connection strings** section.
    *   Add the `AzureAd__ClientId`, `AzureAd__TenantId`, and `AzureAd__Audience` settings to the **Application settings** section (using double underscores `__` as delimiters).
3.  **Update the MAUI App:** In `MauiProgram.cs`, update the `HttpClient`'s `BaseAddress` to point to your live Azure App Service URL.

## Future Work / Roadmap

*   [ ] Build out the tagging system (add/remove tags, filter by tags).
*   [ ] Implement a full-text search feature.
*   [ ] Add support for different item types (e.g., images with uploads to Blob Storage).
*   [ ] Refine the UI/UX and add animations.