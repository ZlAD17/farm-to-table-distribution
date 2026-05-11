using FarmToTable.Api.Data;
using FarmToTable.Api.Middleware;
using FarmToTable.Api.Repositories;
using FarmToTable.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "Farm-to-Table Distribution API",
        Version     = "v1",
        Description = "ASP.NET Core 8 backend for the Regional Farm-to-Table Distribution platform."
    });
});

// Register the DB helper as a singleton (it only holds the connection string)
builder.Services.AddSingleton<DatabaseHelper>();

// Repositories
builder.Services.AddScoped<FarmRepository>();
builder.Services.AddScoped<RestaurantRepository>();
builder.Services.AddScoped<DriverRepository>();
builder.Services.AddScoped<CropRepository>();
builder.Services.AddScoped<HarvestBatchRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<DeliveryTripRepository>();
builder.Services.AddScoped<ReportRepository>();

// Services
builder.Services.AddScoped<FarmService>();
builder.Services.AddScoped<RestaurantService>();
builder.Services.AddScoped<DriverService>();
builder.Services.AddScoped<CropService>();
builder.Services.AddScoped<HarvestBatchService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<DeliveryTripService>();
builder.Services.AddScoped<ReportService>();

// ─── App ────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
