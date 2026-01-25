using Microsoft.AspNetCore.Authentication.JwtBearer;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Repository.DataContexts;
using ResumeInOneMinute.Repository.Repositories;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new MediaTypeApiVersionReader("x-api-version"));
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add Data Protection services
builder.Services.AddDataProtection();

// Register MongoDB Service
builder.Services.AddSingleton<IMongoDbService, ResumeInOneMinute.Infrastructure.Services.MongoDbService>();

// Register Common Services
builder.Services.AddSingleton<ResumeInOneMinute.Infrastructure.CommonServices.EncryptionHelper>();
builder.Services.AddScoped<IEmailService, ResumeInOneMinute.Infrastructure.Services.EmailService>();
builder.Services.AddScoped<IOllamaService, ResumeInOneMinute.Infrastructure.Services.OllamaService>();

// Register Repositories (DbContext is created manually in repository)
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IResumeRepository, ResumeInOneMinute.Repository.Repositories.ResumeRepository>();
builder.Services.AddScoped<IHtmlTemplateRepository, HtmlTemplateRepository>();

// Configure MongoDB Settings
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB");
var mongoUrl = new MongoDB.Driver.MongoUrl(mongoConnectionString);

// Configure JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add Rate Limiting by User ID
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Rate limit policy by user ID
    options.AddPolicy("userPolicy", context =>
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(userId, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue<int>("RateLimitSettings:PermitLimit"),
                Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimitSettings:Window")),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = builder.Configuration.GetValue<int>("RateLimitSettings:QueueLimit")
            });
    });
});

// Add Response Caching
builder.Services.AddResponseCaching();

// Add Memory Cache
builder.Services.AddMemoryCache();


// Add CORS
string MyAllowSpecificOrigins = "http://localhost:1800";
var allowedOriginArray = MyAllowSpecificOrigins
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(x => x.Trim())
    .Where(x => !string.IsNullOrEmpty(x))
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(allowedOriginArray)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Resume In One Minute API",
        Version = "v1",
        Description = @"
## Resume In One Minute API Documentation

A comprehensive API for managing user authentication and resume generation.

### Features
- **User Registration & Login** with JWT authentication
- **Secure Password Storage** using HMACSHA512 hashing
- **Token-based Authentication** for protected endpoints
- **PostgreSQL Database** for reliable data storage
- **Standardized Response Format** for all endpoints

### Getting Started
1. Register a new user account using `/api/Account/register`
2. Login with your credentials using `/api/Account/login`
3. Copy the JWT token from the response
4. Click the 'Authorize' button above and enter: `Bearer {your-token}`
5. You can now access protected endpoints

### Response Format
All endpoints return a standardized response:
```json
{
  ""status"": true/false,
  ""message"": ""Descriptive message"",
  ""data"": { ... }
}
```

### Authentication
Use the JWT token received from login/register in the Authorization header:
```
Authorization: Bearer {your-token}
```
",
        Contact = new OpenApiContact
        {
            Name = "Resume In One Minute Support",
            Email = "support@resumeinone.com",
            Url = new Uri("https://github.com/resumeinone")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Enable XML documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      
Enter 'Bearer' [space] and then your token in the text input below.

Example: 'Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            Array.Empty<string>()
        }
    });

    // Enable annotations for better documentation
    c.EnableAnnotations();

    // Order actions by their relative path
    c.OrderActionsBy(apiDesc => apiDesc.RelativePath);

    // Use full schema names to avoid conflicts
    c.CustomSchemaIds(type => type.FullName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = false;
    });
    
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resume In One Minute API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
        
        // Enhanced UI settings
        c.DocumentTitle = "Resume In One Minute API Documentation";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelExpandDepth(2);
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
        
        // Inject custom CSS for better appearance
        c.InjectStylesheet("/swagger-ui/custom.css");
    });
}
else
{
    // Enable Swagger in production as well (optional - remove if you don't want this)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resume In One Minute API v1");
        c.RoutePrefix = "api-docs";
        c.DocumentTitle = "Resume In One Minute API Documentation";
    });
}

// Global Exception Handler Middleware (must be first)

// Audit Log Middleware

// Enable static files for custom Swagger CSS
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Enable Response Caching
app.UseResponseCaching();

// Rate Limiting
app.UseRateLimiter();

// Decrypt Token (Auth Middleware)
app.UseMiddleware<ResumeInOneMinute.Middleware.TokenDecryptionMiddleware>();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();

