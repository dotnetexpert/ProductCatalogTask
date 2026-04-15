Product Management System
A full-stack sample application demonstrating modern development practices with ASP.NET Core 8 and Angular 21.


Default Credentials

Username = superadmin
Password : Admin@123

Tech Stack
Backend
Framework: .NET 8 / ASP.NET Core Web API

Pattern: CQRS with MediatR

Object Mapping: AutoMapper

Authentication: Cookie-based (JWT stored in HttpOnly cookies)

API Documentation: Swagger (Swashbuckle)

Monitoring: HealthChecks & HealthChecks UI

Persistence: File-based JSON Repository (with EF Core support ready)

Frontend
Framework: Angular 21 (Standalone Components)

API Client: NSwag generated TypeScript client

State Management: Angular Signals

Styling: CSS3 / Generic UI Components

Architecture Overview
The system follows a layered architecture with a CQRS-style flow:

API Layer (Products.Api): Handles HTTP requests, exception handling, and CORS.

Application Layer (Products.Application): Contains features, MediatR handlers, and business logic.

Domain Layer (Products.Domain): Core entities and repository interfaces.

Infrastructure Layer (Products.Infrastructure): Data persistence implementation (JSON file).

Shared Kernel: Common wrappers and generic results.

Request Flow
Controller → MediatR (Command/Query) → Handler → IProductRepository → ProductRepository (JSON/EF)

Authentication Mechanism
Secure Login: Validates against static credentials and generates a JWT.

Cookie Security: The JWT is written into an HttpOnly cookie named accessToken.

Client Handling: Angular uses an AuthInterceptor with withCredentials: true to ensure cookies are included in cross-origin requests.

State: UI state is managed via Angular Signals and persisted in localStorage for route guarding.

API Endpoints
Auth
POST /api/Auth/login - Login and set cookie

POST /api/Auth/logout - Clear auth cookie

Products
POST /Products/get-all - List products (supports dynamic filtering/sorting)

POST /Products/Create - Create product

PUT /Products/{id} - Update product

DELETE /Products/{id} - Delete product

System
GET /api/health/details - Health status

GET /health-ui - Health Dashboard

Project Structure
Plaintext
├── Products.Api
│   ├── Products.Api            # API Host & Controllers
│   ├── Products.Application    # MediatR Handlers & Logic
│   ├── Products.Domain         # Entities & Interfaces
│   ├── Products.Infrastructure # Persistence Logic
│   └── Products.SharedKernel   # Shared DTOs
└── Products.WebApp             # Angular Standalone Application
Setup Instructions
Backend Setup
Configure the JSON file path in appsettings.json under ProductSettings:FilePath.

Run the project using dotnet run.

Access Swagger at /swagger (Development mode).

Frontend Setup
Navigate to Products.WebApp.

Install dependencies: npm install.

Generate API client: npm run generate-api.

Start the app: npm start.

Features
Generic Table: Reusable Angular component for data grids.

Dynamic Querying: Backend supports reflection-based sorting and filtering on JSON data.

Error Handling: Global middleware for consistent API responses.

Environment Support: Pre-configured staging, production, and development environments.