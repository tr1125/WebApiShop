using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Repositories;
using Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserRepository,UserRepository>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IPasswordService, PasswordService>();
//at home
builder.Services.AddDbContext<WebApiShopContext>(static option => option.UseSqlServer("Data Source=(local);Initial Catalog=WebApiShop;Integrated Security=True;Trust Server Certificate=True"));
//IN SEMINARY
//builder.Services.AddDbContext<_329389860_WebApiShopContext>(static option => option.UseSqlServer("Data Source=srv2\\pupils;Initial Catalog=329389860_WebApiShop;Integrated Security=True;Trust Server Certificate=True"));



//builder.

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();