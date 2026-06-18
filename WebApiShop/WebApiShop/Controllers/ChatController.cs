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

    public ChatController(IHttpClientFactory factory, IProductService productService)
    {
        _http = factory.CreateClient();
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest req)
    {
        // שולף מוצרים אמיתיים מה-DB
        var products = await _productService.GetAllProductsAsync();
        var productList = products.Select(p => new
        {
            name        = p.ProductName,
            price       = p.Price,
            description = p.Description,
            category    = p.CategoryId,
            inStock     = true   // עדכן אם יש שדה מלאי
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

        var data = await res.Content.ReadFromJsonAsync<ChatResponse>();
        return Ok(data);
    }
}

public record ChatRequest(
    string Message,
    List<HistoryItem> History);

public record HistoryItem(string Role, string Content);
public record ChatResponse(string Reply);
