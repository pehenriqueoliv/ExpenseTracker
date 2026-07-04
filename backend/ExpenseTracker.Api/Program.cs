using System.Text.Json.Serialization;
using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// CORS policy name for the local front-end.
const string CorsFrontend = "cors-frontend";

// ---------------------------------------------------------------------------
// 1) Service registration in the dependency injection container.
// ---------------------------------------------------------------------------

// EF Core + SQLite. The connection string comes from appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Business services. Scoped = one instance per request (same lifetime as the
// DbContext).
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITotalsService, TotalsService>();

// Controllers + serialization of the enum as text ("Expense"/"Income") in JSON.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Problem Details (RFC 7807) as the default error format, including the automatic
// 400 responses produced by DTO validation.
builder.Services.AddProblemDetails();

// Register the global exception handler that maps domain exceptions to Problem Details.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Swagger/OpenAPI for testing the API from the browser.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS enabled for the local React front-end (Vite uses port 5173).
builder.Services.AddCors(options =>
    options.AddPolicy(CorsFrontend, policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// ---------------------------------------------------------------------------
// 2) HTTP pipeline setup (middleware order matters).
// ---------------------------------------------------------------------------

// Apply pending migrations on startup, creating the SQLite file and the schema
// automatically so the API runs on the first try without manual steps.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Translates unhandled exceptions into Problem Details (uses GlobalExceptionHandler).
app.UseExceptionHandler();

// Swagger UI to make it easy to test the API from the browser.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsFrontend);

app.MapControllers();

app.Run();

// Exposes the implicit Program class (top-level statements) so the integration
// tests can bootstrap the app with WebApplicationFactory<Program>.
public partial class Program { }
