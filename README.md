# .NET Application Template

This project serves as a **skeleton application** designed to be duplicated and used as a starting point for future .NET applications. It provides a pre-configured structure and commonly required features to accelerate development.

## Features Implemented

### Logging

- Pre-configured logging for **Development** and **Production** environments.

### Component Initialization

- Essential components are set up for seamless project startup and configuration.

### Foundation Integration

- Includes integration with the `Foundation` package, which offers:
  - Pagination utilities.
  - Search functionality.
  - Email system (commented, see below for enabling instructions).

### Database Connection

- Pre-configured connection to a database to streamline data integration.

### User Authentication

- Integrated **UserService** for handling user authentication.

### Modular Services

- The following services are included but commented out. Uncomment to enable:
  - **MailerService** for email functionality.
  - **PDF Service** for PDF generation.

### Health Checks

- Health check endpoints are set up to monitor application status.

### Testing

- Includes a sample **Test Project** to write and run unit tests.

---

## How to Use

1. **Clone the Repository**  
   Duplicate this repository to use it as a starting point for your project.

2. **Configure Components**  
   Modify the `appsettings.json` and other configuration files to suit your application's requirements.

3. **Enable Additional Features**  
   Uncomment the following lines in `Program.cs` and relevant controllers to enable additional services:

   - **MailerService**: Uncomment lines in the controller and `Program.cs` to activate email functionality. Check the AppSettings.cs file and appsettings.json to bind your template to mailer.
   - **PDF Service**: Uncomment lines in the service and controller for PDF generation.

4. **Foundation Utilities**  
   The `Foundation` package provides additional tools:

   - Pagination and search are ready to use.
   - Email system requires uncommenting to activate.

5. **Set Up Database**  
   Ensure your database connection strings are correctly configured in `appsettings.json`.

6. **Run Health Checks**  
   Use the pre-configured health check endpoints to verify the application's status.

7. **Add Tests**  
   Expand the included test project with unit and integration tests to cover your application's logic.

---

## Getting Started

To run the project locally:

1. Install the required .NET SDK version.
2. If necessary, remove "Galarne.Template.AspNet/Example" folder which contains a Model, Controller and "Galarne.Template.AspNet.Test/Example" which contains a Test file example, remove their references too
3. Setup database
   - For local create a docker database using
     `docker run -d --rm --name postgres_back -it -e POSTGRES_PASSWORD=password -e POSTGRES_USER=back -p 5401:5432 postgres`
4. Run the project : dotnet run
