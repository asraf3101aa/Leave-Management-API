# 🗓️ Leave Management API

A production-ready, multi-tenant **Leave Management** REST API built with **ASP.NET Core (.NET 10)** following **Clean Architecture** principles.

---

## 📐 Architecture

```
LeaveManagement/
├── LeaveManagement.Domain          # Entities, enums — no dependencies
├── LeaveManagement.Application     # Use-cases, interfaces, DTOs, CQRS commands/queries
├── LeaveManagement.Infrastructure  # EF Core, Identity, services, messaging
└── LeaveManagement.Api             # Controllers, middleware, authorization
```

| Layer | Responsibility |
|-------|---------------|
| **Domain** | Core business entities (`LeaveRequest`, `LeaveType`, `Tenant`) and enums |
| **Application** | CQRS handlers (MediatR), service interfaces, FluentValidation |
| **Infrastructure** | EF Core with two DbContexts, ASP.NET Identity, RabbitMQ email queue |
| **API** | Controllers, permission-based authorization, cookie JWT, response factory |

---

## 🛠️ Tech Stack

| Concern | Technology |
|---------|-----------|
| Framework | ASP.NET Core 10 |
| ORM | Entity Framework Core 10 (Npgsql) |
| Database | PostgreSQL |
| CQRS | MediatR |
| Validation | FluentValidation |
| Auth | Cookie-based JWT (ASP.NET Identity) |
| Authorization | Custom permission-based RBAC |
| Messaging | RabbitMQ (email queue) |

---

## 🔑 Key Features

- **Multi-tenancy** — each tenant has its own isolated database (dynamic connection string via `X-Tenant-Id` header)
- **RBAC** — fine-grained permission system (`Permissions.LeaveRequests.Approve`, etc.) attached to roles per-tenant
- **Leave Request Lifecycle** with dedicated endpoints:
  ```
  Pending ──[submit]──► InReview ──[approve]──► Approved
      │                     └──[reject]──► Rejected
      └────────────────[reject]──────────► Rejected
  ```
- **Pagination** on all list endpoints
- **Soft deletes** on users, roles, leave types
- **Invitation system** — invite user by email, accept via token

---

## ⚙️ Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/)
- [RabbitMQ](https://www.rabbitmq.com/) *(optional — for email queue)*

### 1. Configure

Copy `appsettings.template.json` → `appsettings.json` and fill in your values:

```bash
cp LeaveManagement.Api/appsettings.template.json LeaveManagement.Api/appsettings.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=LeaveManagement;Username=postgres;Password=yourpassword"
  },
  "JwtSettings": {
    "Key": "YOUR_JWT_SECRET_KEY_MIN_32_CHARS",
    "RefreshKey": "YOUR_JWT_REFRESH_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "LeaveManagement",
    "Audience": "LeaveManagementUsers"
  }
}
```

### 2. Create Migrations

**Master DB** (Identity, Tenants, Roles):
```bash
dotnet ef migrations add InitialCreate \
  --context MasterDbContext \
  --project LeaveManagement.Infrastructure \
  --startup-project LeaveManagement.Api \
  --output-dir Persistence/Migrations/Master
```

**Tenant DB** (Leave Requests, Leave Types — per-tenant schema):
```bash
dotnet ef migrations add InitialCreate \
  --context TenantDbContext \
  --project LeaveManagement.Infrastructure \
  --startup-project LeaveManagement.Api \
  --output-dir Persistence/Migrations/Tenant
```

### 3. Apply Migrations & Seed

```bash
dotnet ef database update \
  --context MasterDbContext \
  --project LeaveManagement.Infrastructure \
  --startup-project LeaveManagement.Api
```

> The app seeds a default **SuperAdmin** user on first run:
> - Email: `admin@leaveflow.com`
> - Password: `SuperAdmin123!`

### 4. Run

```bash
dotnet run --project LeaveManagement.Api
```

---

## 🌐 API Overview

Full spec: [`docs/openapi.yaml`](./docs/openapi.yaml)

> Import `docs/openapi.yaml` into [Postman](https://www.postman.com/) or view it at [editor.swagger.io](https://editor.swagger.io/).

### Authentication

All auth is **cookie-based JWT**. Call `/api/auth/login` — the `AccessToken` and `RefreshToken` are set as HttpOnly cookies automatically.

```http
POST /api/auth/login
Content-Type: application/json

{ "email": "admin@leaveflow.com", "password": "SuperAdmin123!" }
```

### Tenant Context

All tenant-scoped endpoints require:
```http
X-Tenant-Id: <your-tenant-guid>
```

### Endpoints at a glance

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/auth/login` | Login |
| `POST` | `/api/auth/logout` | Logout |
| `POST` | `/api/auth/refresh` | Refresh access token |
| `GET` | `/api/auth/me` | Current user profile + permissions |
| `GET` | `/api/auth/my-tenants` | Tenants user belongs to (paginated) |
| `POST` | `/api/auth/invite-user` | Invite a user to current tenant |
| `POST` | `/api/auth/accept-invitation` | Accept invite, create account |
| `PATCH` | `/api/auth/users/{id}/toggle-active` | Enable/disable a user |
| `DELETE` | `/api/auth/users/{id}` | Soft-delete a user |
| — | | |
| `GET` | `/api/leaverequests` | List leave requests (paginated) |
| `POST` | `/api/leaverequests` | Create a leave request |
| `GET` | `/api/leaverequests/{id}` | Get leave request details |
| `PUT` | `/api/leaverequests/{id}` | Edit a **Pending** request |
| `DELETE` | `/api/leaverequests/{id}` | Delete a leave request |
| `PATCH` | `/api/leaverequests/{id}/submit` | Submit for review |
| `PATCH` | `/api/leaverequests/{id}/approve` | Approve |
| `PATCH` | `/api/leaverequests/{id}/reject` | Reject |
| — | | |
| `GET` | `/api/leavetypes` | List leave types (paginated) |
| `POST` | `/api/leavetypes` | Create leave type |
| `GET` | `/api/leavetypes/{id}` | Get leave type |
| `PUT` | `/api/leavetypes/{id}` | Update leave type |
| `DELETE` | `/api/leavetypes/{id}` | Soft-delete leave type |
| — | | |
| `GET` | `/api/roles` | List roles |
| `POST` | `/api/roles` | Create role |
| `POST` | `/api/roles/set-default` | Set default tenant role |
| `POST` | `/api/roles/{name}/permissions` | Set role permissions |
| `DELETE` | `/api/roles/{name}` | Delete role |

---

## 🔐 Permissions

Permissions are assigned per role per tenant. Available permissions:

| Resource | Permissions |
|----------|------------|
| `LeaveRequests` | `View` `Create` `Edit` `Delete` `Approve` |
| `LeaveTypes` | `View` `Create` `Edit` `Delete` |
| `Roles` | `View` `Create` `Edit` `Delete` |
| `Users` | `View` `Create` `Edit` `Delete` |

Permission string format: `Permissions.<Resource>.<Action>`
Example: `Permissions.LeaveRequests.Approve`

---

## 📦 Project Structure (detailed)

```
LeaveManagement.Application/
├── Constants/          Permissions, Roles constants
├── DTOs/               LeaveRequestDto, LeaveRequestListDto, etc.
├── Features/
│   ├── Auth/           Login, Refresh, GetMe, GetUserTenants, Invite commands/queries
│   ├── LeaveRequests/  Create, Update, Delete, Submit, Approve, Reject, Get, GetList
│   ├── LeaveTypes/     LeaveTypeCommands, LeaveTypeQueries, validators
│   └── Roles/          CreateRole, SetDefaultRole, UpdatePermissions, GetRoles, DeleteRole
├── Interfaces/         IAuthService, ILeaveService, IApplicationDbContext, IMasterDbContext
├── Models/             Email models
└── Responses/          ApiResponse<T>, PaginatedData<T>, PaginationMeta

LeaveManagement.Infrastructure/
├── Identity/           ApplicationUser, ApplicationRole, TenantUserRole, ApplicationRoleClaim
├── Messaging/          RabbitMQ publisher, consumer, MockEmailSender
├── Persistence/
│   ├── MasterDbContext.cs      Identity + Tenants + Roles DB
│   ├── TenantDbContext.cs      Per-tenant DB (LeaveRequests, LeaveTypes)
│   ├── MasterDbContextFactory.cs
│   ├── TenantDbContextFactory.cs
│   └── DbSeeder.cs             Seeds SuperAdmin user and role
└── Services/
    ├── AuthService.cs
    ├── LeaveService.cs
    └── TenantService.cs
```

---

## 🧪 Running Migrations for a New Tenant

When a new tenant is onboarded, create their isolated database and run the `TenantDbContext` migration against the tenant's connection string to provision their schema.

---

## 📄 License

MIT
