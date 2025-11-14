# AuthService API.

A centralized authentication and authorization service for multi-tenant, multi-application platforms. Built with ASP.NET Core 8, Entity Framework Core, and SQL Server.

## Overview

AuthService API provides user management, authentication, and role-based access control (RBAC) for multiple applications:

- User authentication (login, register, logout)
- Password management (forgot password / reset)
- Per-app user management with approval workflows
- Role and permission management and assignment
- JWT token generation and validation
- Email and phone verification
- Account status enforcement middleware

## Features

- Multi-app support with isolated roles and permissions per app
- Per-app account statuses: pending, approved, active, suspended, deleted
- Role-based access control (RBAC) per app
- Permission management and role-permission assignments
- User role assignments and unassignments
- JWT with claim-based authorization
- Swagger/OpenAPI documentation
- Docker Compose support
- Biz (business) layer + Repository pattern
- Soft delete for users

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (local or container)
- Visual Studio 2022 or VS Code

### Quick start

1. Clone the repo and checkout dev:
```bash
git clone https://github.com/itianremo/AuthSvc.git
cd AuthSvc
git checkout dev
```

2. Configure `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AuthServiceDb;User Id=sa;Password=YourStrongPassword123!;"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-chars-long",
    "Issuer": "AuthService",
    "Audience": "YourApps"
  }
}
```

3. Run EF Core migrations:
```bash
dotnet ef database update --project AuthService.Infrastructure
```

4. Start the API:
```bash
dotnet run --project AuthService.API
```

5. Open Swagger UI at `http://localhost:8080/swagger` (development environment).

## Project Structure (high level)

AuthService/
- AuthService.API/ — ASP.NET Core API
- Controllers/
 - `AuthController.cs` — login, register, logout, forgot-password
 - `UserController.cs` — user profile, update, verify, delete
 - `AppController.cs` — apps, pending users, approve, update status
 - `AssignController.cs` — assign/unassign roles, permissions, apps/users
 - `RoleController.cs` — role CRUD and queries
 - `PermissionController.cs` — permission CRUD
- Middlewares/
 - `AccountStatusMiddleware.cs`
- `Program.cs`
- AuthService.Application/ — DTOs, Biz services, interfaces
- DTOs/ (LoginDto, RegisterUserDto, ApproveUserDto, UpdateUserAppStatusDto, AssignRoleDto, UnassignUserRoleDto, AssignPermissionDto, UnassignPermissionDto, RoleDto, PermissionDto, UserListDto, etc.)
- Interfaces/ (IBiz<T>, IUserBiz, IAppBiz, IRoleBiz, IPermissionBiz, IUserAppBiz, IUserRoleBiz, IRolePermissionBiz, IUserAppStatusBiz)
- Services/ (Biz<T>, UserBiz, AppBiz, RoleBiz, PermissionBiz, UserAppBiz, UserRoleBiz, RolePermissionBiz, UserAppStatusBiz)
- DependencyInjection/ (`ApplicationServiceCollectionExtensions.cs`)
- AuthService.Domain/ — Entities and repository interfaces
- Entities/ (User, App, Role, Permission, UserApp, UserRole, RolePermission, UserAppStatus)
- Interfaces/ (IRepository<T>, IUserRepository, IAppRepository, IRoleRepository, IPermissionRepository, IUserAppRepository, IUserRoleRepository, IRolePermissionRepository, IUserAppStatusRepository)
- AuthService.Infrastructure/ — EF Core, DbContext, Repository implementations
- Data/ (`AuthDbContext.cs`)
- Repositories/ (Repository<T>, UserRepository, AppRepository, RoleRepository, PermissionRepository, UserAppRepository, UserRoleRepository, RolePermissionRepository, UserAppStatusRepository)
- DependencyInjection/ (`InfrastructureServiceCollectionExtensions.cs`)

## API Endpoints (summary)

- Auth
- POST `api/auth/login` — Login (uses `LoginDto`)
- POST `api/auth/register` — Register (uses `RegisterUserDto`)
- POST `api/auth/logout`— Logout (requires Authorization)
- POST `api/auth/forgot-password` — Forgot password

- Users
- GET `api/users/{userId}` — Get user profile
- GET `api/users` — List users (requires `CanManageUsers`)
- PUT `api/users/{userId}` — Update user basic info (`UpdateUserBasicInfoDto`)
- POST `api/users/verify-email` — Verify email (`VerifyEmailDto`)
- POST `api/users/verify-phone` — Verify phone (`VerifyPhoneDto`)
- DELETE `api/users/{userId}` — Soft delete user

- Apps
- GET `api/apps` — Listapps (requires `CanManageApps`)
- GET `api/apps/{appId}` — Get app
- POST `api/apps` — Create app (`CreateAppDto`, requires `CanManageApps`)
- GET `api/apps/{appId}/pending-users` — Pending users for app (requires `CanManageUsers`)
- POST `api/apps/approve-user` — Approve user (`ApproveUserDto`)
- POST `api/apps/update-user-status` —Update user app status (`UpdateUserAppStatusDto`)

- Assignments
- POST `api/assign/roles` — Assign role to user (`AssignRoleDto`, requires `CanManageAssigns`)
- DELETE `api/assign/roles` — Unassign role from user (`UnassignUserRoleDto`)
- POST `api/assign/permissions` — Assign permission to role (`AssignPermissionDto`)
- DELETE `api/assign/permissions` — Unassign permission from role (`UnassignPermissionDto`)
- POST `api/assign/apps` — Assign user to app (`UpdateUserAppStatusDto`)
- DELETE `api/assign/apps` — Unassign user from app (`UpdateUserAppStatusDto`)

- Roles
- GET `api/roles` — List roles (requires `CanManageRoles`)
- GET `api/roles/app/{appId}` — Roles by app
- GET `api/roles/{roleId}` — Get role
- POST `api/roles` — Create role (`CreateRoleDto`)
- PUT `api/roles/{roleId}` — Update role (`UpdateRoleDto`)
- DELETE `api/roles/{roleId}` — Delete role

- Permissions
- GET `api/permissions` — List permissions (requires `CanManagePermissions`)
- GET `api/permissions/{permissionId}` — Get permission
- POST `api/permissions` — Create permission (`CreatePermissionDto`)
- PUT `api/permissions/{permissionId}` — Update permission (`CreatePermissionDto`)
- DELETE `api/permissions/{permissionId}` — Delete permission

## DTOs

Primary DTOs live in `AuthService.Application/DTOs` and include:
- `LoginDto`, `RegisterUserDto`, `RefreshTokenDto`
- `ApproveUserDto`, `UpdateUserAppStatusDto`, `UpdateUserBasicInfoDto`
- `AssignRoleDto`, `UnassignUserRoleDto`, `AssignPermissionDto`, `UnassignPermissionDto`
- `RoleDto`, `CreateRoleDto`, `UpdateRoleDto`
- `PermissionDto`, `CreatePermissionDto`
- `UserListDto`, `VerifyEmailDto`, `VerifyPhoneDto`
- `AccountStatus` constants

## Account Status Values

Defined in `AccountStatus.cs`:
- `active`, `suspended`, `pending`, `need email verify`, `need phone verify`, `deleted`, `approved`

## Authorization Policies

Configured in `Program.cs` and used across controllers:

- `CanManageAssigns` — requires `SuperAccess` or `ManageAssigns`
- `CanManageApps` — requires `SuperAccess` or `ManageApps`
- `CanManageUsers` — requires `SuperAccess` or `ManageUsers`
- `CanManageRoles` — requires `SuperAccess` or `ManageRoles`
- `CanManagePermissions` — requires `SuperAccess` or `ManagePermissions`

## Patterns & Architecture

- Biz layer (`Biz<T>`) provides business logic classes and exposes typed repositories.
- Repository pattern (`IRepository<T>` / `Repository<T>`) abstracts EF Core access.
- Dependency injection configured in:
  - `AuthService.Application.DependencyInjection.ApplicationServiceCollectionExtensions`
  - `AuthService.Infrastructure.DependencyInjection.InfrastructureServiceCollectionExtensions`

## Dependency Injection (high level)

Application registration (example):
services.AddScoped(typeof(IBiz<>), typeof(Biz<>)); services.AddScoped<IUserBiz, UserBiz>(); services.AddScoped<IAppBiz, AppBiz>(); // etc...

Infrastructure registration (example):
services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); services.AddScoped<IUserRepository, UserRepository>(); services.AddScoped<IAppRepository, AppRepository>(); // etc...

## Database Schema (core tables)

- Users
- Apps
- UserApps
- Roles
- Permissions
- UserRoles
- RolePermissions
- UserAppStatus

## Docker Compose (local)

Example service definition (adjust secrets in production):
version: '3.8' services: auth-service: build: . ports: - "8080:80" environment: ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=AuthServiceDb;User Id=sa;Password=YourStrongPassword123!;" Jwt__Key: "your-super-secret-key-min-32-chars-long" Jwt__Issuer: "AuthService" Jwt__Audience: "YourApps" depends_on: - sqlserver
sqlserver: image: mcr.microsoft.com/mssql/server:2022-latest environment: ACCEPT_EULA: "Y" SA_PASSWORD: "YourStrongPassword123!" ports: - "1433:1433"

Start:
docker-compose up -d

## Development

- Apply migrations:
dotnet ef migrations add <Name> --project AuthService.Infrastructure dotnet ef database update --project AuthService.Infrastructure
- Run tests:

## Error codes

Standard HTTP codes are used:
- 200 OK, 201 Created, 204 No Content
- 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 409 Conflict, 500 Internal Server Error

## Contributing

1. Create a feature branch: `git checkout -b feature/my-feature`
2. Commit: `git commit -m "Add my feature"`
3. Push and create a pull request

## License

MIT — see LICENSE file.

## Support

Open an issue on the repository for bugs or feature requests.
