using Microsoft.EntityFrameworkCore;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Services.Impl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<YieldDbContext>(options
    => options.UseInMemoryDatabase("YieldDb"));

builder.Services.Configure<YieldCurveSourcesConfig>(
    builder.Configuration.GetSection("YieldCurveSources"));

builder.Services.AddHttpClient<TreasuryXmlService>();
builder.Services.AddHostedService<TreasuryXmlService>();

builder.Services.AddHttpClient<SP500Service>();
builder.Services.AddHostedService<SP500Service>();

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
