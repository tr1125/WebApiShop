using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Repositories;
using Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NLog.Web;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();

builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();


//at home
//builder.Services.AddDbContext<WebApiShopContext>(static option => option.UseSqlServer("Data Source=(local);Initial Catalog=WebApiShop;Integrated Security=True;Trust Server Certificate=True"));
//IN SEMINARY
//builder.Services.AddDbContext<WebApiShopContext>(static option => option.UseSqlServer("Data Source=srv2\\pupils;Initial Catalog=329389860_WebApiShop;Integrated Security=True;Trust Server Certificate=True"));
//builder.Configuration.GetConnectionString("Home");
builder.Services.AddDbContext<WebApiShopContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Home")));

// Add services to the container.

builder.Services.AddControllers();

//builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

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

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }