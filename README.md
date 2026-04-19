# DesignRoom Studio

**DesignRoom Studio** is a full-stack interior design e-commerce platform built as a learning project. It lets users browse home-decor products, arrange them freely on an interactive room canvas, and place real orders -- all in one seamless experience. Behind the elegant Angular frontend sits a robust ASP.NET Core 9 REST API engineered around clean architecture, asynchronous design, and production-grade practices.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Backend - ASP.NET Core 9 REST API](#backend--aspnet-core-9-rest-api)
   - [Layered Architecture and Dependency Injection](#layered-architecture-and-dependency-injection)
   - [Asynchronous Design and Scalability](#asynchronous-design-and-scalability)
   - [Database First with Entity Framework Core](#database-first-with-entity-framework-core)
   - [DTOs and AutoMapper](#dtos-and-automapper)
   - [Configuration via AppSettings](#configuration-via-appsettings)
   - [Middleware and Error Handling](#middleware-and-error-handling)
   - [NLog Structured Logging](#nlog-structured-logging)
   - [Password Strength Validation](#password-strength-validation)
   - [Order Integrity Guard](#order-integrity-guard)
   - [API Endpoints](#api-endpoints)
4. [Frontend - Angular 21 SPA](#frontend--angular-21-spa)
   - [The Interactive Design Canvas](#the-interactive-design-canvas)
   - [Components](#components)
   - [Services and State Management](#services-and-state-management)
   - [Guest Mode](#guest-mode)
5. [Testing](#testing)
   - [Unit Tests](#unit-tests)
   - [Integration Tests](#integration-tests)
6. [Quick Start](#quick-start)
7. [Tech Stack](#tech-stack)

---

## Project Overview

DesignRoom Studio combines two ideas into one product:

- **E-commerce**: browse, filter, and purchase home-decor products across 12 categories (sofas, tables, lamps, carpets, curtains, clocks, and more).
- **Visual room designer**: drag furniture onto a perspective room canvas, resize pieces, swap walls and floors, and watch the total price update in real time -- then convert the canvas directly into an order.

Users can explore the designer as guests without creating an account, and administrators get a full back-office panel for managing products, categories, orders, and users.

---

## Architecture

```
+---------------------------------------------+
|              Angular 21 SPA                 |
|  Components . Services . RxJS Observables   |
+-------------------+-------------------------+
                    | HTTP / JSON (DTOs)
+-------------------v-------------------------+
|         ASP.NET Core 9 REST API             |
|                                             |
|  Controllers  (HTTP layer)                  |
|       |  DI                                 |
|  Services     (business logic)              |
|       |  DI                                 |
|  Repositories (data access)                 |
|       |  EF Core                            |
|  SQL Server Database                        |
+---------------------------------------------+
```

The solution is split into five C# projects:

| Project | Role |
|---|---|
| `WebApiShop` | ASP.NET Core host: controllers, middleware, DI wiring |
| `Services` | Business logic, AutoMapper profiles |
| `Repositories` | EF Core DbContext, repository implementations |
| `Entities` | Database entity classes |
| `DTOs` | Data Transfer Objects shared between layers |

---

## Backend - ASP.NET Core 9 REST API

The backend is a **RESTful API written in C#** targeting **.NET 9**, exposing JSON endpoints consumed by the Angular frontend.

### Layered Architecture and Dependency Injection

The server is strictly divided into three layers -- **Controllers -> Services -> Repositories** -- with each layer depending only on the interface of the layer below it, never on a concrete class. This loose coupling is achieved entirely through ASP.NET Core's built-in **Dependency Injection** container.

Every service and repository is registered as **`Scoped`** in `Program.cs` -- one registration per interface/implementation pair, covering Products, Users, Orders, Categories, Ratings, and Password. Because each layer depends on an abstraction (interface), individual layers can be replaced, mocked in tests, or extended without touching the rest of the codebase -- a textbook application of the **Dependency Inversion Principle**.

---

### Asynchronous Design and Scalability

Every controller action, service method, and repository call is written as **`async`/`await`** using `Task<T>` return types. This means the server never blocks a thread while waiting for a database query -- the thread is returned to the pool and reused for other incoming requests. The result is significantly higher **throughput and scalability** under concurrent load without the complexity of manual threading.

```csharp
public async Task<ActionResult<List<ProductDTO>>> GetProducts(...)
{
    var products = await _productService.GetProductsByConditions(...);
    return Ok(products);
}
```

---

### Database First with Entity Framework Core

The database schema was designed first in SQL Server, and **Entity Framework Core 9** was used in **Database First** mode -- generating C# entity classes from the existing schema via `Scaffold-DbContext`. EF Core acts as the **ORM** (Object-Relational Mapper), translating LINQ queries into optimized SQL without writing raw SQL strings.

The connection string is stored externally in `appsettings.json` and injected at startup:

```csharp
builder.Services.AddDbContext<WebApiShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Home")));
```

---

### DTOs and AutoMapper

Raw entity classes are never sent over the wire. Each layer communicates through dedicated **Data Transfer Objects** (defined in the `DTOs` project), which decouple the internal data model from the public API contract and prevent over-posting or accidental data exposure.

Crucially, every DTO is defined as a C# **`record`** rather than a regular class:

```csharp
public record ProductDTO(
    int ProductId = 0,
    string ProductName = "",
    double Price = 0,
    int CategoryId = 0,
    string? Description = "",
    string? ImageURL = "",
    string? Color = ""
);
```

Records are immutable by design and semantically communicate *"this object exists only to carry data"* -- not to hold behaviour. Because records use positional constructors rather than settable properties, **AutoMapper** is essential to bridge the gap between mutable entities and immutable DTOs.

Mapping between entities and DTOs is handled automatically by **AutoMapper**, configured in `Services/MyMapper.cs`:

```csharp
CreateMap<Product, ProductDTO>().ReverseMap();
CreateMap<User, UserDTO>()
    .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName.Trim()));
CreateMap<OrderDTO, Order>()
    .ForMember(d => d.OrderId, o => o.Ignore()); // DB generates the ID
```

AutoMapper is registered to scan all assemblies automatically:

```csharp
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
```

---

### Configuration via AppSettings

All environment-specific settings -- database connection strings, logging targets, and CORS origins -- are stored in `appsettings.json` and `appsettings.Development.json`. No secrets are hard-coded in the application. The `appsettings` file includes commented alternatives for different deployment environments (home, school lab), making local development frictionless.

---

### Middleware and Error Handling

Two custom middleware components sit in the ASP.NET Core request pipeline:

**`NotFoundMiddleware`** -- runs after all other middleware. If a response completes with HTTP `404`, it intercepts the response, logs the missing path at `Error` level, and serves a custom `404.html` page from `wwwroot` (or a plain-text fallback if the file is absent). This ensures clients always receive a meaningful error response.

**`RatingMiddleware`** -- runs on every request before the main pipeline. It captures metadata (host, HTTP method, path, referer, user-agent, UTC timestamp) and asynchronously persists a `Rating` record to the database -- providing a complete audit trail of all API traffic. The rating service is injected via **method injection** into `Invoke()`, which is the correct pattern for scoped services inside singleton middleware:

```csharp
public async Task Invoke(HttpContext httpContext, IRatingService ratingService)
{
    var rating = new Rating { Host = ..., Method = ..., Path = ... };
    await ratingService.AddRating(rating);
    await _next(httpContext);
}
```

---

### NLog Structured Logging

The project uses **NLog** for structured, production-grade logging, configured via `nlog.config` and wired into the host with `builder.Host.UseNLog()`. Every controller logs method entry and exit; middleware logs 404 paths; the order service logs warnings when a submitted total does not match the server-calculated total. Log output is written to rolling files under `logs/`.

---

### Password Strength Validation

Password strength is evaluated server-side by a dedicated `PasswordService` (backed by the `zxcvbn` algorithm) and exposed via `POST /api/password`. The Angular frontend calls this endpoint on every keystroke in any password field, displaying a live colored strength meter. The same service is consulted during user registration and profile updates -- if the password scores below level 3, the operation is rejected with HTTP `400`.

---

### Order Integrity Guard

When a new order is submitted, `OrderService` independently recalculates the total by fetching each product's price from the database and multiplying by the submitted quantity. If the client-submitted `orderSum` differs from the server-calculated value, the service **overwrites the submitted total with the correct value** and logs a `Warning` -- ensuring the final order stored in the database is always financially accurate, regardless of what the client sends.

---

### API Endpoints

The API exposes resources under `/api/products`, `/api/users`, `/api/orders`, `/api/categories`, and `/api/password`, supporting standard GET / POST / PUT / DELETE operations with filtering, pagination, and role-scoped access where relevant.

Full interactive documentation is available via **Swagger UI** at `/swagger` when running in the Development environment.

---

## Frontend - Angular 21 SPA

The frontend is an **Angular 21** single-page application built with Standalone Components, TypeScript, Tailwind CSS, and Bootstrap 5. It communicates with the backend exclusively through typed HTTP services backed by **RxJS observables**, giving every component a reactive, real-time view of application state.

### The Interactive Design Canvas

The crown jewel of DesignRoom Studio is the **visual room designer** -- a full-screen, three-panel experience that lets users build their dream room before ever reaching the checkout.

**The room canvas** renders a perspective room view split into a *wall zone* (upper area, for wall art, clocks, curtains, windows, and wallpapers) and a *floor zone* (lower area, for furniture, rugs, and plants). Users can:

- **Drag any product** from the right-hand catalog panel onto the canvas using **Angular CDK Drag-Drop** -- the item lands where they drop it.
- **Resize freely** -- custom `mousedown`/`mousemove`/`mouseup` host-listener logic tracks a `ResizeSession` per item, giving pixel-perfect control over width and height.
- **Swap walls and floors** -- clicking a wall or floor texture instantly updates the room background, stored per-user in `localStorage`.
- **Delete items** individually via a hover-revealed trash icon, or wipe the entire canvas with "Clear All".
- **Watch the total update live** -- a top bar shows the current furniture count and cumulative price in real time.

The **left panel** is a rich product filter sidebar: live name search, multi-select category checkboxes, min/max price range, color filter, and description keyword search -- all wired to paginated API calls with a "Load More" button.

Canvas state is automatically persisted by `DesignRoomStateService` on every change, so users never lose their design between page refreshes. When a user is ready to buy, the canvas flows directly into the **Personal Area / Checkout** page, where canvas items become order line items.

---

### Components

| Component | Description |
|---|---|
| `CanvasComponent` | The interactive room designer -- the application's centerpiece |
| `AuthComponent` | Combined login / register form with live password-strength meter |
| `PersonalAreaComponent` | Order history (with admin status management) and checkout from canvas |
| `AdminComponent` | Full CRUD for products and categories; order and user management |
| `ProfileComponent` | Edit personal details with live password-strength validation |

---

### Services and State Management

| Service | Responsibility |
|---|---|
| `DesignRoomStateService` | Singleton canvas state -- holds `items$` as a `BehaviorSubject`, persists to `localStorage` (users) or `sessionStorage` (guests) |
| `ProductService` | Exposes `products$` and `categories$` as observables; builds `HttpParams` from `ProductFilter` |
| `AuthService` | Manages the authenticated `User` via a `BehaviorSubject`; persists session to `localStorage` |
| `OrderService` | Thin HTTP wrapper for all order endpoints; converts `Date` to ISO string before POST |

---

### Guest Mode

The designer is fully accessible without an account. Clicking "Continue as Guest" skips authentication and opens the canvas immediately -- the session is stored in `sessionStorage` so it is private to the tab and cleared automatically when closed. Guests can explore the entire product catalog and design a room; they are prompted to sign in only when placing an order.

---

## Testing

### Unit Tests

Located in `TestWebApiShop/UnitTests/`. Built with **xUnit** and **Moq** -- each test constructs the service under test with mocked repository dependencies and a real `IMapper` (configured with `MyMapper`), so both business logic and mapping behaviour are verified. All six services are covered: Products, Users, Orders, Categories, Ratings, and Password, with happy-path and unhappy-path scenarios throughout.

### Integration Tests

Located in `TestWebApiShop/IntegrationTests/`. Built with **xUnit** and **EF Core's In-Memory database provider** -- these tests exercise the actual repository implementations against a real (in-memory) database, with no mocking. Each test class spins up a fresh `WebApiShopContext` with a unique `Guid` database name, guaranteeing full isolation. All five repositories are covered: Products, Users, Orders, Categories, and Ratings.

Together, the two suites cover the full vertical slice from business logic down to data access.

---

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- Node.js 18+
- SQL Server

### Backend

```bash
cd WebApiShop/WebApiShop
dotnet restore
dotnet run
# API: https://localhost:5001
# Swagger: https://localhost:5001/swagger
```

### Frontend

```bash
cd ClientApp
npm install
npm start
# App: http://localhost:4200
```

### Run Tests

```bash
cd WebApiShop/TestWebApiShop
dotnet test
```

---

## Tech Stack

**Backend**
- ASP.NET Core 9.0 (C#)
- Entity Framework Core 9.0 -- Database First
- SQL Server
- AutoMapper
- NLog
- Swagger / OpenAPI
- xUnit and Moq (testing)

**Frontend**
- Angular 21 / TypeScript 5.9
- Angular CDK (Drag-Drop)
- RxJS
- Tailwind CSS / Bootstrap 5

---

*Version 1.0.0 -- Status: In Development*
