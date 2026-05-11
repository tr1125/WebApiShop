---
applyTo: "WebApiShop/WebApiShop/Controllers/**/*.cs"
---

# API Controllers — Path-Specific Instructions

These rules apply to every file under `WebApiShop/WebApiShop/Controllers/`.  
They are **non-negotiable** and take precedence over any general suggestion.

## 1. Mandatory Base Class
Every controller **must** inherit `ControllerBase` (not `Controller`).  
Views are not used in this API; inheriting `Controller` is an error.

```csharp
// ✅ correct
[Route("api/[controller]")]
[ApiController]
public class ExampleController : ControllerBase { ... }

// ❌ wrong
public class ExampleController : Controller { ... }
```

## 2. Return Types
Every action method **must** return `Task<ActionResult<T>>` where `T` is the DTO type.  
Use `Task<IActionResult>` only for actions that truly return no body (e.g., a bare `NoContent()`).

```csharp
// ✅ correct
[HttpGet("{id}")]
public async Task<ActionResult<ProductDTO>> GetProductById(int id) { ... }

// ❌ wrong — too vague
public async Task<IActionResult> GetProductById(int id) { ... }

// ❌ wrong — not async
public ActionResult<ProductDTO> GetProductById(int id) { ... }
```

## 3. No Business Logic in Controllers
Controllers are **thin orchestrators** only.  
All logic lives in the Service layer.  Permitted operations inside a controller action:

| Allowed | Not Allowed |
|---|---|
| Call one service method | Any `if` / `switch` on data content |
| Read `HttpContext` / route values | Database queries |
| Set / delete cookies | Password hashing or validation |
| Return an `ActionResult` | Mapping between types |
| Log entry/exit/errors | Sending emails or notifications |

```csharp
// ✅ correct
[HttpPost]
public async Task<ActionResult<UserDTO>> AddUser(UserRequestDTO user)
{
    try
    {
        var result = await _userService.AddUserToFile(user);
        if (result == null) return BadRequest("Registration failed.");
        Response.Cookies.Append("jwt", result.Value.Token, JwtCookieOptions());
        return CreatedAtAction(nameof(GetUserById), new { id = result.Value.User.Id }, result.Value.User);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error adding user");
        return StatusCode(500, new { message = ex.Message });
    }
}

// ❌ wrong — business logic inside controller
[HttpPost]
public async Task<ActionResult<UserDTO>> AddUser(UserRequestDTO user)
{
    if (user.Password.Length < 8) return BadRequest("Too short"); // ← belongs in service
    var hashed = BCrypt.HashPassword(user.Password);              // ← belongs in service
    ...
}
```

## 4. Error Handling Template
Every action must follow this exact structure:

```csharp
try
{
    // single service call + return result
}
catch (Exception ex)
{
    _logger.LogError(ex, "Brief description of the failing operation");
    return StatusCode(500, new { message = ex.Message });
}
```

## 5. Required Attributes
| Attribute | Scope | Notes |
|---|---|---|
| `[Route("api/[controller]")]` | Class | Always use token-based route |
| `[ApiController]` | Class | Enables automatic model validation |
| `[Authorize]` | Action | Any endpoint requiring a logged-in user |
| `[AdminOnly]` | Action | Any endpoint restricted to admins (adds on top of `[Authorize]`) |
| `[HttpGet]` / `[HttpPost]` / … | Action | Always explicit; never implicit |

## 6. Constructor Injection
Inject only interfaces, never concrete classes.  
Declare all injected fields as `private readonly`.

```csharp
private readonly IProductService _productService;
private readonly ILogger<ProductsController> _logger;

public ProductsController(IProductService productService, ILogger<ProductsController> logger)
{
    _productService = productService;
    _logger = logger;
}
```
