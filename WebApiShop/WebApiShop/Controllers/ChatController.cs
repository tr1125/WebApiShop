using Microsoft.AspNetCore.Mvc;
using Services;
using System.Text.Json;
namespace WebApiShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly HttpClient _http;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ChatController(IHttpClientFactory factory, IProductService productService, ICategoryService categoryService)
    {
        _http = factory.CreateClient();
        _productService = productService;
        _categoryService = categoryService;
    }

    [HttpGet("debug-products")]
    public async Task<IActionResult> DebugProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        var sample = products.Take(5).Select(p => new {
            p.ProductId, p.ProductName, p.ImageURL, p.Color
        });
        return Ok(new { total = products.Count, sample });
    }

    [HttpPost("debug-payload")]
    public async Task<IActionResult> DebugPayload()
    {
        var products = await _productService.GetAllProductsAsync();
        var productList = products.Select(p => new
        {
            productId   = p.ProductId,
            name        = p.ProductName,
            imageURL    = p.ImageURL,
            color       = p.Color,
        }).Take(5).ToList();
        return Ok(new { total = products.Count, sample = productList });
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest req)
    {
        // שולף מוצרים אמיתיים מה-DB
        var products = await _productService.GetAllProductsAsync();
        var categories = await _categoryService.GetAllCategories();
        var catNameMap = categories.ToDictionary(c => c.CetegoryId, c => c.CategoryName?.ToLower() ?? "");

        var productList = products.Select(p => new
        {
            productId    = p.ProductId,
            name         = p.ProductName,
            price        = p.Price,
            description  = p.Description,
            category     = p.CategoryId != 0 && catNameMap.ContainsKey(p.CategoryId) ? catNameMap[p.CategoryId] : "",
            imageURL     = p.ImageURL,
            color        = p.Color,
            inStock      = true
        }).ToList();

        var payload = new
        {
            message  = req.Message,
            history  = req.History,
            products = productList
        };

        var res = await _http.PostAsJsonAsync(
            "http://localhost:8002/chat",
            payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        if (!res.IsSuccessStatusCode)
        {
            var errorBody = await res.Content.ReadAsStringAsync();
            return StatusCode(500, $"AI service error: {errorBody}");
        }

        var data = await res.Content.ReadFromJsonAsync<ChatResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return Ok(data);
    }
}

public record ChatRequest(
    string Message,
    List<HistoryItem> History);

public record HistoryItem(string Role, string Content);
public record ChatResponse(string Reply, List<CanvasAction>? CanvasActions, string? FloorImageURL, string? WallImageURL);
public record CanvasAction(
    string   Id,
    string   Type,
    int      ProductId,
    string   Label,
    int      X,
    int      Y,
    int      Width,
    int      Height,
    string?  ImageURL,
    double?  Price,
    string?  Color);
