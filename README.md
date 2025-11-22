# Auth Service
A centralized authentication and authorization service for multi-tenant, multi-application platforms. Built with ASP.NET Core and EF Core, designed for scalability, modularity, and future extensibility.

## Executive Summary

Auth Service is designed as a plug‑and‑play authentication and authorization backbone for multi‑tenant platforms. It enforces immutability for system‑defined entities, provides runtime seeding for rapid onboarding, and scales with parallelized metrics and Docker‑based orchestration. This architecture is enterprise‑ready, ensuring clarity, extensibility, and operational transparency for both developers and executives.

# Features
Register/login via phone or email

Per-app JWTs and refresh tokens

Role-based access control per app

Per-app account status (enum enforced)

Editable user profile via User Service

OAuth2/OpenID integration (planned)

Docker Compose orchestration with external networks

Runtime seeding via DbInitializer

Dashboard metrics with parallelized queries

Unified error responses across controllers

Strongly typed claims (sub, AppId)

# Getting Started
## Prerequisites
.NET 8 SDK

SQL Server

Docker (optional for orchestration)

## Setup
Clone the repository

Configure appsettings.json with your database and JWT settings

Run EF Core migrations:

```bash Update-Database

Launch the service:

```bash dotnet run

Access Swagger at https://localhost:5001/swagger

## API Endpoints
Auth: Register, login, refresh, logout

Users: CRUD, role assignment, verification

Roles: CRUD, permissions, users

Permissions: CRUD, role linking

Apps: CRUD, statuses

Dashboard: Metrics

## Database Schema
### Core tables:

Users

Apps

UserApps

Roles

Permissions

RolePermissions

UserRoles

PasswordResetTokens

### Runtime Seeding
DbInitializer seeds apps, users, roles, permissions, and statuses at startup using InitConfig. System-defined entities are immutable.

## Scalability
Dashboard metrics run queries in parallel.

Ready for caching (Redis, MemoryCache).

"Audit logging planned via external microservice."

## Error Handling
All controllers return consistent error responses in the format:

```json { "code": "ErrorType", "message": "Human-readable explanation", "details": "Optional extra info" }

## Claims
JWT claims are standardized:

sub → User ID

AppId → Application ID

permissions → JSON array of permission names

## Docker Compose
Supports external network integration for multi-service orchestration:

```yaml networks: svcauth-net: external: true

## License
MIT License