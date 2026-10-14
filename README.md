# RaceDay API

## 1. Project Overview

RaceDay API is a web API developed using ASP.NET Core. It provides endpoints that allow applications to communicate with the RaceDay system and perform operations through HTTP requests.

The API uses Entity Framework Core to communicate with a SQL Server database. Swagger is used to document the API endpoints and test API requests.

## 2. Technologies Used

- **C#** – Programming language.
- **ASP.NET Core Web API** – Framework used to develop the API.
- **Entity Framework Core** – Used for database operations.
- **SQL Server** – Database management system.
- **Swagger / Swashbuckle** – Used to document and test API endpoints.
- **xUnit or the configured test framework** – Used for unit testing, depending on the test project configuration.
- **Visual Studio** – Development environment.

## 3. Project Features

The project includes the following features:

- REST API endpoints through controllers.
- Database connectivity using Entity Framework Core.
- SQL Server connection configuration.
- Swagger documentation for viewing and testing endpoints.
- Session services configured in the application.
- Unit tests for checking the application's functionality.

## 4. Requirements

Before running the project, ensure that you have installed:

- Visual Studio with the ASP.NET and web development workload.
- The .NET SDK version required by the project.
- SQL Server.
- SQL Server Management Studio (SSMS), if you want to manage the database through a graphical interface.

## 5. How to Run the Project

1. Open the RaceDay solution (`.sln` or `.slnx`) in Visual Studio.
2. In Solution Explorer, right-click the `RaceDay.API` project.
3. Select **Set as Startup Project**.
4. Check that the database connection string in `appsettings.json` is configured correctly.
5. Ensure that SQL Server is running and the required database is available.
6. Select the HTTPS launch profile.
7. Click the green **Run** button or press **F5**.

The API should start, and the browser should open the Swagger page automatically if the launch settings are configured correctly.

## 6. Swagger Documentation

Swagger provides an interface for viewing and testing the API endpoints.

The local development addresses configured for this project are:

- HTTPS: `https://localhost:7132/swagger`
- HTTP: `http://localhost:5254/swagger`

The actual address depends on the launch profile selected in Visual Studio.

To test an endpoint:

1. Open Swagger.
2. Expand the endpoint you want to test.
3. Click **Try it out**.
4. Enter any required information.
5. Click **Execute**.
6. Review the response and HTTP status code.

## 7. Database Configuration

The project uses Entity Framework Core with SQL Server.

The database context is `RaceDayDbContext`, located in the `RaceDay.API.Data` namespace.

The connection string is configured in `appsettings.json` under `DefaultConnection`.

Example configuration:


    "ConnectionStrings": {
        "DefaultConnection": "Server=Paropt06\\SQLEXPRESS01;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
    },


## 8. API Endpoints

The API endpoints are implemented in the controller classes.

To view the available endpoints, run the project and open Swagger.

Swagger displays the HTTP methods, routes, parameters, and response information for the endpoints discovered by the application.

## 9. Unit Testing

The RaceDay project includes a test project for checking API functionality.

To run the unit tests:

1. Open the solution in Visual Studio.
2. Select **Test → Test Explorer**.
3. Click **Run All**.
4. Review the results to confirm that the tests pass.

Unit tests help identify errors and verify that the tested functionality behaves as expected.

## 10. Troubleshooting

### Swagger does not open automatically

- Confirm that `launchBrowser` is set to `true` in `launchSettings.json`.
- Confirm that `launchUrl` is set to `swagger`.
- Ensure that `RaceDay.API` is the startup project.
- Check that the selected launch profile is correct.

### Database connection fails

- Confirm that SQL Server is running.
- Check the `DefaultConnection` connection string.
- Verify that the database exists and that the account has permission to access it.

### No endpoints appear in Swagger

- Check that the controller classes are included in the API project.
- Confirm that controllers use the appropriate routing attributes.
- Ensure that `builder.Services.AddControllers()` and `app.MapControllers()` are present in `Program.cs`.

## 11. Conclusion

RaceDay API provides a foundation for developing and testing web API functionality using ASP.NET Core, Entity Framework Core, and SQL Server. Swagger simplifies API testing and documentation, while unit tests help verify the application's functionality.
