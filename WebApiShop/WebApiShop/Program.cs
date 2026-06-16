using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Repositories;
using Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NLog.Web;
using WebApiShop;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();

builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();

//at home
//builder.Services.AddDbContext<WebApiShopContext>(static option => option.UseSqlServer("Data Source=(local);Initial Catalog=WebApiShop;Integrated Security=True;Trust Server Certificate=True"));
//IN SEMINARY
//builder.Services.AddDbContext<WebApiShopContext>(static option => option.UseSqlServer("Data Source=srv2\\pupils;Initial Catalog=329389860_WebApiShop;Integrated Security=True;Trust Server Certificate=True"));
//builder.Configuration.GetConnectionString("Home");
builder.Services.AddDbContext<WebApiShopContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Home")));


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetSection("Redis")["ConnectionString"])
);

// Add services to the container.

// JWT Authentication - reads token from HttpOnly cookie
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        // Extract JWT from HttpOnly cookie instead of Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Cookies.TryGetValue("jwt", out var cookieToken))
                    ctx.Token = cookieToken;
                return Task.CompletedTask;
            }
        };
    });

// Authorization policies based on claims
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("isAdmin", "True"));
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApiShop", Version = "v1" });
    // Allow Swagger to authorize with JWT via the Authorize button
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token here (without the 'Bearer ' prefix)"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure CORS for Angular client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "http://localhost:4201") // Angular dev server ports
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Required for HttpOnly cookies cross-origin
        });
});

var app = builder.Build();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Internal Server Error\"}");
    });
});

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.MapOpenApi();
    //app.UseSwaggerUI(options =>
    //{
    //    options.SwaggerEndpoint("/openapi/v1.json", "My API V1");
    //});
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAngularClient");

// Configure SPA support
app.UseDefaultFiles(); // Serves index.html as default file
app.UseStaticFiles(); // Serves static files from wwwroot

app.UseMiddleware<NotFoundMiddleware>();

app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();

app.UseMiddleware<RatingMiddleware>();

app.UseMiddleware<RateLimitMiddleware>();

app.MapControllers();

// SPA fallback routing - serves index.html for any unmatched routes
// This enables Angular routing to work correctly
app.MapFallbackToFile("index.html");

app.Run();