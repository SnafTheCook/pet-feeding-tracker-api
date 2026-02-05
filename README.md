# Pet Feeding Tracker API
![Build Status](https://github.com/SnafTheCook/pet-feeding-tracker-api/actions/workflows/dotnet.yml/badge.svg)
![.NET 9](https://img.shields.io/badge/.NET-9.0-512bd4)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-blue)
![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white)
![Authentication](https://img.shields.io/badge/Auth-JWT%20%2B%20Refresh%20Tokens-green)

A .NET 9 Web API built to manage and track pet feeding schedules. This project serves as a demonstration of backend architecture standards, including secure authentication, database concurrency management, and containerized deployment.

## Technical Stack

* **Framework:** .NET 9 (ASP.NET Core Web API)
* **Database:** SQL Server via Entity Framework Core
* **Security:** JWT Authentication with Refresh Token rotation
* **Validation:** FluentValidation with custom Action Filters
* **Documentation:** Scalar and OpenAPI
* **Containerization:** Docker and Docker Compose

## Architecture and Design Patterns

### Service Layer Pattern
The application implements a clear separation of concerns to ensure maintainability:
* **Controllers:** Designed as thin entry points that delegate business logic to the service layer.
* **Service Layer:** Encapsulates business rules and database operations, abstracting complexity from the API layer.
* **DTOs (Data Transfer Objects):** Used to decouple the internal database schema from the public-facing API, preventing unintended data exposure.

### Database Integrity
* **Concurrency Management:** Implemented optimistic concurrency using `RowVersion` (Timestamp) on the Pet entity. This prevents data loss during simultaneous updates by throwing a `DbUpdateConcurrencyException`.
* **Automated Migrations:** The application is configured to automatically apply EF Core migrations and seed initial data (including an Admin user and sample pet records) during the startup sequence.

### Security and Configuration
* **Authentication:** Implemented a JWT-based security flow using `SymmetricSecurityKey`. The system includes a secure Refresh Token implementation with rotation and reuse detection.
* **Configuration Safety:** The project follows the .NET configuration hierarchy. Sensitive credentials such as JWT secrets and connection strings are managed via User Secrets in development and injected as Environment Variables in Docker. No sensitive data is stored in source control.

### Middleware and Filtering
* **Global Error Handling:** Custom middleware captures unhandled exceptions to return standardized JSON responses and log server-side issues.
* **Validation Filter:** An `IAsyncActionFilter` intercepts incoming requests to execute FluentValidation rules. This ensures that the service layer only receives valid data, returning a 400 Bad Request automatically when rules are violated.

## Local Setup and Deployment

The project is ready for containerized deployment using Docker Compose.

### Running with Docker

1. **Environment Configuration:**
   * Copy `.env.example` to a new file named `.env`.
   * Provide values for `DB_PASSWORD` and `JWT_TOKEN`.

2. **Launch:**  
  
    ```docker-compose up --build```

4. **Endpoints:**
   * Scalar Documentation: http://localhost:8080/scalar
   * OpenAPI Specification: http://localhost:8080/openapi/v1.json

## Project Roadmap
  * **Unit Testing:** Implement a testing suite using xUnit and Moq to validate service layer logic.
  * **Mapping Libraries:** Transition from manual DTO mapping to Mapperly or AutoMapper to improve maintainability.
  * **CORS Configuration:** Define restricted origin policies for frontend integration.

## Developer Note

This project was developed as part of a career transition from Unity Game Development (C#) to Enterprise .NET Development.




