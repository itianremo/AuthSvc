# Auth Service

A centralized authentication and authorization service for multi-tenant, multi-application platforms. Built with ASP.NET Core and EF Core, designed for scalability, modularity, and future extensibility.

## Features

- Register/login via phone or email
- Per-app JWTs and refresh tokens
- Role-based access control per app
- Per-app account status
- Editable user profile via User Service
- OAuth2/OpenID integration (planned)
- Docker Compose orchestration

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server
- Docker (optional for orchestration)

### Setup

1. Clone the repository  
2. Configure `appsettings.json` with your database and JWT settings  
3. Run EF Core migrations:

   ```bash
   Update-Database
   ```

4. Launch the service:

   ```bash
   dotnet run
   ```

5. Access Swagger at `https://localhost:5001/swagger`

## API Endpoints

### Register

```http
POST /api/user/register
Content-Type: application/json

{
  "email": "rami@myapp.com",
  "phoneNumber": "+201234567890",
  "password": "SecurePass123!",
  "appId": "15585edb-c292-480d-8ae8-f1201adac050"
}
```

### Login

```http
POST /api/user/login
Content-Type: application/json

{
  "emailOrPhone": "rami@myapp.com",
  "password": "SecurePass123!",
  "appId": "15585edb-c292-480d-8ae8-f1201adac050"
}
```

### Assign Role

```http
POST /api/roles/assign
Content-Type: application/json

{
  "userId": "329cea9c-7ec6-4d30-90bf-951ddf2ce046",
  "appId": "15585edb-c292-480d-8ae8-f1201adac050",
  "roleName": "Admin"
}
```

### Refresh Token

```http
POST /api/token/refresh
Content-Type: application/json

{
  "refreshToken": "859f31bd-8432-44fb-817a-a7761aed8726",
  "appId": "15585edb-c292-480d-8ae8-f1201adac050"
}
```

## Database Schema

The service uses a modular, multi-tenant schema with the following core tables:

- Users
- Apps
- UserApps
- Roles
- Permissions
- RolePermissions
- UserRoles

## Docker Compose

The service can be orchestrated locally using Docker Compose. The setup includes:

- Auth Service (ASP.NET Core)
- SQL Server
- RabbitMQ (optional for future event-driven features)

Example `docker-compose.yml` snippet:

```yaml
services:
  auth-service:
    build: .
    ports:
      - "5000:80"
    environment:
      - ConnectionStrings__Default=Server=db;Database=AuthDb;User Id=sa;Password=YourStrong!Passw0rd;
      - Jwt__Secret=your-secret-key
      - Jwt__Issuer=AuthService
      - Jwt__Audience=YourApps
    depends_on:
      - db

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong!Passw0rd"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
```

## License

This project is licensed under the MIT License.
