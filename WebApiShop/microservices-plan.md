# Microservices Architecture Plan — WebApiShop

## Background

The current project is a monolith — a single application containing all logic, all layers, and all data in one place. This document proposes how to split it into independent microservices.

---

## Current Database Structure

```
Categories
    └── Products
            └── Order_items
                    └── Orders
                            └── Users
Ratings (standalone)
```

---

## Proposed Services

### 1. User Service
Responsible for everything related to users and authentication.

**Tables:** `Users`

**Endpoints:**
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | /auth/register | Register a new user | Public |
| POST | /auth/login | Login | Public |
| GET | /users/{id} | Get user by ID | User |
| PUT | /users/{id} | Update user details | User |
| GET | /users | Get all users | Admin |
| PUT | /users/{id}/promote | Promote user to admin | Admin |

---

### 2. Catalog Service
Responsible for products and categories.

**Tables:** `Products`, `Categories`

**Endpoints:**
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | /products | Get products with filters | Public |
| GET | /products/{id} | Get product by ID | Public |
| POST | /products | Add product | Admin |
| PUT | /products/{id} | Update product | Admin |
| DELETE | /products/{id} | Delete product | Admin |
| GET | /categories | Get all categories | Public |
| POST | /categories | Add category | Admin |

---

### 3. Order Service
Responsible for orders and order items.

**Tables:** `Orders`, `Order_items`

**Endpoints:**
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | /orders | Get all orders | Admin |
| GET | /orders/{id} | Get order by ID | User |
| GET | /orders/user/{userId} | Get orders by user | User |
| POST | /orders | Create order | User |
| PUT | /orders/{id} | Update order | Admin |
| DELETE | /orders/{id} | Delete order | Admin |

---

### 4. Rating Service
Responsible for request logs and ratings.

**Tables:** `Ratings`

**Endpoints:**
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | /ratings | Get all ratings | Admin |

---

## API Gateway

The client (Angular) does not communicate directly with each service. All requests go through a single entry point — the **API Gateway** — which routes them to the correct service.

```
Client (Angular)
      ↓
API Gateway
  ├── /auth/*        → User Service
  ├── /users/*       → User Service
  ├── /products/*    → Catalog Service
  ├── /categories/*  → Catalog Service
  ├── /orders/*      → Order Service
  └── /ratings/*     → Rating Service
```

---

## Inter-Service Communication

Each service has its own database, so when one service needs data from another, it must communicate via HTTP.

**Example 1 — Order Service needs product data:**
When a user places an order, the Order Service must verify the product exists and fetch its price.
```
Order Service → GET /products/{id} → Catalog Service
```

**Example 2 — Order Service needs to verify the user:**
```
Order Service → GET /users/{id} → User Service
```

---

## Shared Infrastructure

### Redis
Each service uses Redis for caching with its own key prefix to avoid collisions:
- `catalog:product:{id}`
- `catalog:products:{filters}`
- `user:{id}`
- `order:{id}`

### JWT Authentication
The User Service creates the JWT token upon login or registration. Every other service validates the token independently — no need to call the User Service on every request.

### Rate Limiting
Each service implements its own rate limiting middleware using Redis, keyed by client IP.

---

## Advantages

- Each service can be deployed and updated independently
- If the Rating Service crashes, all other services continue to work
- Each service can be scaled independently based on load
- Smaller, more focused codebases are easier to maintain and test

## Challenges

- Inter-service communication adds network latency and complexity
- Distributed transactions are harder to manage (e.g. creating an order that affects product inventory)
- More infrastructure to deploy and monitor
- Data consistency across services requires careful design
