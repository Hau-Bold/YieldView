using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Logging;
using YieldView.API.Services.Contract;
using YieldView.API.Services.Impl;
using YieldView.API.Services.Impl.Providers;

var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFile(builder.Configuration.GetSection("Logging:File"));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<YieldDbContext>(options
    => options.UseInMemoryDatabase("YieldDb"));

builder.Services.Configure<YieldCurveSourcesConfig>(
    builder.Configuration.GetSection("YieldCurveSources"));

builder.Services.AddScoped<SP500DataProvider>();
builder.Services.AddScoped<YieldSpreadProvider>();
builder.Services.AddScoped<StockDataProvider>();
builder.Services.AddScoped<GDPDataProvider>();

builder.Services.AddTransient<ICSVStockParser, CSVStockParser>();
builder.Services.AddTransient<IGDPParser, GDPParser>();

builder.Services.AddHttpClient<TreasuryXmlService>();
builder.Services.AddHostedService<TreasuryXmlService>();


builder.Services.AddHttpClient<SP500Service>();
builder.Services.AddHostedService<SP500Service>();

builder.Services.AddHttpClient<BiduStockService>();
builder.Services.AddHostedService<BiduStockService>();

builder.Services.AddHttpClient<PlugStockService>();
builder.Services.AddHostedService<PlugStockService>();

builder.Services.AddHttpClient<GrossDomesticProductService>();
builder.Services.AddHostedService<GrossDomesticProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//avoid cross origin blocked by browser!
app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthorization();

app.MapControllers();

app.Run();
