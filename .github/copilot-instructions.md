# DesignRoom Studio — Repository Instructions for GitHub Copilot

## Application Purpose
Full-stack interior-design e-commerce platform. Users browse/filter 12 décor categories, place items on an interactive room canvas with real-time pricing, and convert the canvas directly into an order. Guests may use the designer without registering.

## Tech Stack
| Layer | Technology | Version |
|---|---|---|
| Frontend | Angular + RxJS | 21 / ~7.8 |
| Styling | Bootstrap + Tailwind CSS | 5.3 / 3.4 |
| Backend | ASP.NET Core Web API | .NET 9 |
| ORM | Entity Framework Core (Database-First) | 9.0 |
| Auth | JWT in HttpOnly cookie (`jwt`) | JwtBearer 9.0 |
| Caching | Redis via StackExchange.Redis | latest |
| Mapping | AutoMapper (all assemblies scanned) | latest |
| Logging | NLog.Web.AspNetCore | 6.1 |
| Password | Zxcvbn (min score 3 enforced) | latest |
| API Docs | Swagger / Swashbuckle | 9.0 |
| Database | SQL Server (two environments) | — |

## Solution Structure
```
ClientApp/               Angular 21 SPA (ng serve -o on :4200)
WebApiShop/
  WebApiShop/            Host: Controllers, Middleware, Program.cs
  Services/              Business logic + AutoMapper profile (MyMapper.cs)
  Repositories/          EF Core DbContext + repository implementations
  Entities/              Database entity POCOs (Database-First; never hand-edit migrations)
  DTOs/                  Shared request/response DTOs
  TestWebApiShop/        Unit tests + Integration tests
```

## Architecture Rules (enforce strictly)
- **Flow**: Angular → Controller → Service → Repository → EF Core → SQL Server
- Controllers contain **zero business logic** — call the injected service only.
- Services contain **zero data-access logic** — call the injected repository only.
- All layers use **constructor injection**; no service-locator or static calls.
- Every async method must `await` its calls; no `.Result` / `.Wait()`.

## Coding Guidelines

### Naming Conventions
| Symbol | Convention | Example |
|---|---|---|
| Private fields | `_camelCase` | `_userService`, `_logger` |
| Methods | `PascalCase` async | `GetUserById`, `AddProduct` |
| DTOs | `<Entity>DTO` / `<Entity>RequestDTO` | `UserDTO`, `UserRequestDTO` |
| Interfaces | `I<Name>Service` / `I<Name>Repository` | `IUserService` |
| Controllers | `<Entity>sController` | `UsersController` |

### Controller Standards
- Inherit `ControllerBase`; decorate with `[Route("api/[controller]")]` and `[ApiController]`.
- All actions return `Task<ActionResult<T>>` (use `Task<IActionResult>` only when no body).
- Wrap every action body in `try/catch`; log with `_logger`; return `StatusCode(500, new { message })` on unhandled exceptions.
- Authorization: `[Authorize]` for authenticated users, `[AdminOnly]` for admin-only endpoints. Never leave admin endpoints unprotected.

### Caching (Redis cache-aside — see ProductsController for reference)
- Build a composite string key from all query parameters.
- On miss: call service → serialize with `ReferenceHandler.Preserve` → store with `AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ttl)`.
- TTL read from `IConfiguration["Redis:TTLMinutes"]` (default 10).

### Auth / JWT
- Tokens generated in `UserService.GenerateToken(UserDTO)`.
- Cookie name: `jwt`; flags set via `JwtCookieOptions()` helper (Secure + SameSite=None in Production, Secure=false + SameSite=Lax in Development).

### Error Handling
```csharp
try { ... }
catch (Exception ex)
{
    _logger.LogError(ex, "Descriptive message about the operation");
    return StatusCode(500, new { message = ex.Message });
}
```

## Environment Setup Notes
- Two SQL Server connection strings in `appsettings.Development.json`: `Home` (local `(local)`) and `School` (seminary server). Switch in `Program.cs` line ~36.
- Redis: `localhost:6379` with password `RedisSecret123!` (Development only).
- CORS configured for `http://localhost:4200` and `http://localhost:4201` with `AllowCredentials()`.
- Run backend: `dotnet run` from `WebApiShop/WebApiShop/`.
- Run frontend: `ng serve -o` from `ClientApp/`.
- **Never commit** `appsettings.Development.json` secrets to a public repository.

## Agent Directives (Copilot must follow these for every non-trivial task)

### Planning Pattern — think before you type
1. Before editing any file, state the full list of files to be changed and why.
2. Identify which layer(s) are affected (Controller / Service / Repository / Entity / DTO).
3. Check for existing interfaces and DTOs before creating new ones.
4. Confirm the change does not break the layer boundary rules above.

### Reflection Pattern — validate before finishing
1. After completing edits, mentally trace the request/response flow end-to-end.
2. Run `dotnet build` (backend) and `ng build` (frontend) to confirm no compile errors.
3. Run `dotnet test` to confirm existing tests still pass.
4. If a test fails, fix it before reporting the task as done — never leave a red build.
5. Review that all new public methods have a matching interface declaration.
