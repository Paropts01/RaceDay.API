
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Register Entity Framework Core and SQL Server.
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Azure Blob Storage.
// Register BlobServiceClient with a validated connection string
builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();

    // Read from any of the common key shapes. The last one is what
    // docker-compose.yml currently produces: AzureBlob__ConnectionString
    var connectionString =
        cfg["AzureBlob:ConnectionString"]
        ?? cfg["AzureBlobStorage:ConnectionString"]
        ?? cfg.GetConnectionString("AzureBlob")
        ?? cfg.GetConnectionString("AzureBlobStorage");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        // Log every key we tried so the failure is obvious in the container log
        var attempted = new[]
        {
            "AzureBlob:ConnectionString",
            "AzureBlobStorage:ConnectionString",
            "ConnectionStrings:AzureBlob",
            "ConnectionStrings:AzureBlobStorage"
        };
        throw new InvalidOperationException(
            "Azure Blob Storage connection string is missing. " +
            "Checked configuration keys: " + string.Join(", ", attempted) +
            ". Set the environment variable AzureBlob__ConnectionString " +
            "or the user secret 'AzureBlob:ConnectionString'.");
    }

    return new BlobServiceClient(connectionString);
});

// Register the Azure Blob Storage service.
builder.Services.AddScoped<AzureBlobStorageService>();

// Register session services.
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register controllers and Swagger.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "RaceDay API",
        Version = "v1"
    });
});

// Build the application.
var app = builder.Build();

// Configure Swagger.
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "RaceDay API v1");

    options.RoutePrefix = "swagger";
});

// Configure middleware.
app.UseHttpsRedirection();

app.UseSession();

app.UseAuthorization();

// Map API controllers.
app.MapControllers();

// Start the application.
app.Run();


