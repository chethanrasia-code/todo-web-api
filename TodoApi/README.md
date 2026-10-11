# TodoApi

## Overview
TodoApi is an ASP.NET Core 10 Web API project built with Clean Architecture principles. It provides CRUD operations for Todo items and includes JWT-based API authentication support.

## Features
- CRUD operations for Todo items
- JWT authentication middleware and token support
- In-memory EF Core persistence for local development and tests
- Swagger/OpenAPI documentation
- Docker support

## Project Structure
```
TodoApi
├── src
│   ├── TodoApi.Api
│   ├── TodoApi.Application
│   ├── TodoApi.Domain
│   └── TodoApi.Infrastructure
├── tests
│   ├── TodoApi.UnitTests
│   └── TodoApi.IntegrationTests
├── Dockerfile
├── docker-compose.yml
├── .editorconfig
├── .gitignore
├── README.md
├── TodoApi.sln
└── .vs/
```

## Architecture Overview

This solution follows a Clean Architecture style with explicit separation of concerns:

- Domain layer: contains business entities and domain logic. The core model is `TodoItem` and exceptions such as `NotFoundException`.
- Application layer: defines use cases and contracts for repositories and services. It contains DTOs, interfaces, and the business workflow logic in `TodoService`.
- Infrastructure layer: contains persistence, auth, and token generation logic. This is where EF Core, repository implementations, and JWT signing are configured.
- API layer: hosts controllers, Swagger setup, health checks, and the HTTP request pipeline.

### Request flow
1. HTTP request enters the API host in the Startup/Program pipeline.
2. CORS, authentication, authorization, and exception handling are applied in order.
3. Controllers call services in the Application layer.
4. Services use repository abstractions to access EF Core data.
5. Results are returned to the client as DTOs and JSON responses.

### JWT generation and validation
JWT support is configured in the API startup pipeline with `AddJwtBearer()` and `TokenValidationParameters`.

Key details:
- `JwtSettings` supports both a shared symmetric key and optional RSA signing via a PFX certificate.
- `JwtTokenGenerator` selects the signing algorithm automatically:
  - symmetric key => HMAC SHA-256
  - PFX certificate => RSA SHA-256
- `AuthController` validates credentials via `IAuthService` and issues a token using `IJwtTokenGenerator`.
- The API uses JWT bearer authentication for protected endpoints and Swagger security configuration for testability.

### Global exception handling
The API includes a custom `GlobalExceptionMiddleware` that catches unhandled exceptions and writes a structured `application/problem+json` response.

Behavior:
- logs the exception with ILogger
- responds with HTTP 500
- returns a payload containing `type`, `title`, `status`, and `detail`

This provides consistent error responses across the application and avoids leaking unhandled stack traces to clients.

## Getting Started

### Prerequisites
- .NET 10 SDK
- Docker (optional)

### Setup
1. Open the solution in Visual Studio or VS Code:
   ```
   cd TodoApi
   ```

2. Restore dependencies:
   ```
   dotnet restore
   ```

3. Run the API:
   ```
   dotnet run --project src/TodoApi.Api
   ```

4. Open Swagger UI in the browser:
   ```
   https://localhost:5001/swagger
   ```

### Running with Docker
1. Build the Docker image:
   ```
   docker build -t todoapi .
   ```

2. Run the container:
   ```
   docker run -p 5000:80 todoapi
   ```

## API Endpoints
Base URL:
```text
https://localhost:5001/api/todos
```

Endpoints:
- `GET /api/todos` - retrieve all Todo items
- `GET /api/todos/{id}` - retrieve a single Todo item
- `POST /api/todos` - create a Todo item
- `PUT /api/todos/{id}` - update a Todo item
- `DELETE /api/todos/{id}` - delete a Todo item

## Testing
Run the solution tests with:
```bash
dotnet test
```

The project currently contains:
- unit tests in `tests/TodoApi.UnitTests`
- integration tests in `tests/TodoApi.IntegrationTests`

## Notes
- The application uses an in-memory database by default for local development and test execution.
- Visual Studio should be opened on the folder containing the solution file, not the parent directory above it.

## License
This project is licensed under the MIT License.