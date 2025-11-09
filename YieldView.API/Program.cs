using Microsoft.EntityFrameworkCore;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Extensions;
using YieldView.API.Logging;
using YieldView.API.Services.Contract;
using YieldView.API.Services.Impl;
using YieldView.API.Services.Impl.Providers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWindowsService();

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

builder.Services.AddHttpClient("default", client =>
{
  client.Timeout = TimeSpan.FromSeconds(30);
  client.DefaultRequestHeaders.Clear();
  client.DefaultRequestHeaders.Add("User-Agent",
      "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
      "(KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
  client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
  client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
});

builder.Services.AddScoped<SP500DataProvider>();
builder.Services.AddScoped<YieldSpreadProvider>();
builder.Services.AddScoped<StockDataProvider>();
builder.Services.AddScoped<FREDDataProvider>();

builder.Services.AddTransient<ICSVStockParser, CSVStockParser>();
builder.Services.AddTransient<IGDPParser, GDPParser>();
builder.Services.AddTransient<IWilshireParser, WilshireParser>();


builder.Services.AddBackgroundServiceWithInterface<TreasuryXmlService, ITreasuryXmlService>();
builder.Services.AddBackgroundServiceWithInterface<SP500Service, ISP500Service>();
builder.Services.AddBackgroundServiceWithInterface<StockService, IStockService>();
builder.Services.AddBackgroundServiceWithInterface<WilshireService, IWilshireService>();
builder.Services.AddBackgroundServiceWithInterface<GrossDomesticProductService, IGrossDomesticProductService>();


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
