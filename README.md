# Pet Feeding Tracker API
![Build Status](https://github.com/SnafTheCook/pet-feeding-tracker-api/actions/workflows/dotnet.yml/badge.svg)
![.NET 9](https://img.shields.io/badge/.NET-9.0-512bd4)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-blue)
![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white)
![Authentication](https://img.shields.io/badge/Auth-JWT%20%2B%20Refresh%20Tokens-green)

A unified cross-platform application designed to track pet feeding schedules in real-time. This project serves as a demonstration of backend architecture standards, including secure authentication, database concurrency management, and containerized deployment with multiple frontend clients (Blazor WebAssembly and Flutter).

## Technical Stack

* **Framework:** .NET 9 (ASP.NET Core Web API)
* **Database:** SQL Server via Entity Framework Core
* **Testing:** xUnit, Moq, FluentAssertions
* **Security:** JWT Authentication with Refresh Token rotation
* **Validation:** FluentValidation with custom Action Filters
* **Documentation:** Scalar and OpenAPI
* **Containerization:** Docker and Docker Compose
* **CI/CD:** GitHub Actions

## Architecture and Design Patterns

### Service Layer Pattern
The application implements a clear separation of concerns to ensure maintainability:
* **Controllers:** Designed as thin entry points that delegate business logic to the service layer.
* **Service Layer:** Encapsulates business rules and database operations, abstracting complexity from the API layer.
* **DTOs (Data Transfer Objects):** Used to decouple the internal database schema from the public-facing API, preventing unintended data exposure.

### Automated Unit Testing
The project includes a comprehensive test suite.
* **Service Testing:** Logic verification using an EF Core In-Memory database provider.
* **Controller Testing:** Verification of API contracts and HTTP responses using **Moq** to isolate the web layer from the service layer.
* **State Verification:** Deep-object assertions using **FluentAssertions** to ensure nested data integrity.

### Database Integrity
* **Concurrency Management:** Implemented optimistic concurrency using `RowVersion` (Timestamp) on the Pet entity. This prevents data loss during simultaneous updates by throwing a `DbUpdateConcurrencyException`.
* **Automated Migrations:** The application is configured to automatically apply EF Core migrations and seed initial data (including an Admin user and sample pet records) during the startup sequence.

### Security and Configuration
* **Authentication:** Implemented a JWT-based security flow using `SymmetricSecurityKey`. The system includes a secure Refresh Token implementation with rotation and reuse detection.
* **Configuration Safety:** The project follows the .NET configuration hierarchy. Sensitive credentials such as JWT secrets and connection strings are managed via User Secrets in development and injected as Environment Variables in Docker. No sensitive data is stored in source control.

### Middleware and Filtering
* **Global Error Handling:** Custom middleware captures unhandled exceptions to return standardized JSON responses and log server-side issues.
* **Validation Filter:** An `IAsyncActionFilter` intercepts incoming requests to execute FluentValidation rules. This ensures that the service layer only receives valid data, returning a 400 Bad Request automatically when rules are violated.

## DevOps and Deployment

The entire ecosystem is orchestrated using **Docker Compose** to provide a consistent development and production environment.

* **Containerization:** Separate Dockerfiles for the API, Blazor (Nginx), and Flutter Web (Nginx).
* **Networking:** Services communicate over an internal Docker bridge network, with frontends served via Nginx reverse proxies.
* **CI/CD:** Integrated GitHub Actions to automate build verification and unit testing (xUnit/Moq) on every pull request.

## Project Structure

```text
/
├── DidWeFeedTheCatToday.sln        # Root Solution
├── DidWeFeedTheCatToday/           # ASP.NET Core Web API
├── DidWeFeedTheCatToday.Client/    # Blazor WebAssembly Frontend
├── DidWeFeedTheCatToday.Mobile/    # Flutter Mobile/Web Frontend
├── DidWeFeedTheCatToday.Shared/    # Shared C# DTOs and Enums
└── DidWeFeedTheCatToday.Tests/     # xUnit Test Suite
```

## How to Run

### Requirements
* Docker Desktop
* .NET 9 SDK (for local development)

### Setup
1. Clone the repository.
2. Create a `.env` file in the root directory based on the `.env.example` provided.
3. Launch the ecosystem:
   ```bash
   docker-compose up --build
   ```

### Access Points
* **Blazor Web App:** http://localhost:5001
* **Flutter Web Demo:** http://localhost:5002
* **API Documentation (Scalar):** http://localhost:8080/scalar/v1

### Demo Credentials
The database is automatically seeded with a default administrative account for testing:
* **Username:** `admin`
* **Password:** `Admin123!`
