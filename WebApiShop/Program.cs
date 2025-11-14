using Repositories;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddScoped(IUserRepository );

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
