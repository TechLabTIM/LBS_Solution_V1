using Microsoft.Extensions.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Serilog for logging to a file
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Log to console
    .WriteTo.File("logs/api-.txt", rollingInterval: RollingInterval.Day) // Log to a file daily
    .CreateLogger();

// Clear default logging providers and add Serilog
builder.Host.UseSerilog();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod();
        })
);

builder.Services.AddHttpClient("LBSClient", client =>
{
    client.Timeout = TimeSpan.FromMinutes(3);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

 //app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
