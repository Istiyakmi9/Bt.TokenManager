using Bt.Lib.Common.Service.Configserver;
using Bt.Lib.Common.Service.Model;
using Bt.TokenManager.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<TokenManagerService>();
builder.Services.AddControllers();

builder.Services.AddSingleton<IFetchGithubConfigurationService>(x =>
    FetchGithubConfigurationService.getInstance(ApplicationNames.EMSTUM)
);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
