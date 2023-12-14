using Bot.App.ServiceManager.Model;
using Bot.App.ServiceManager.Service;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<JwtSetting>(o => builder.Configuration.GetSection(nameof(JwtSetting)).Bind(o));
builder.Services.AddScoped<TokenManagerService>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
