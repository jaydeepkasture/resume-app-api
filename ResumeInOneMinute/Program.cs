using Microsoft.AspNetCore.Authentication.JwtBearer;
using Asp.Versioning;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Repository.Repositories;
using System.Text;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Load Shared Settings
builder.Configuration.AddJsonFile("appsettings.Shared.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();



// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// --- Configuration Validation ---
var requiredConfigs = new Dictionary<string, string>
{
    { "ConnectionStrings:PostgreSQL", "ConnectionStrings__PostgreSQL" },
    { "ConnectionStrings:MongoDB", "ConnectionStrings__MongoDB" },
    { "JwtSettings:SecretKey", "JwtSettings__SecretKey" },
    { "GoogleSettings:ClientId", "GoogleSettings__ClientId" },
    { "GroqSettings:ApiKey", "GroqSettings__ApiKey" },
    { "Razorpay:KeyId", "Razorpay__KeyId" },
    { "Razorpay:KeySecret", "Razorpay__KeySecret" },
    { "CorsSettings:AllowedOrigins", "CorsSettings__AllowedOrigins" },
    { "AppSettings:FrontendUrl", "AppSettings__FrontendUrl" },
    { "EmailSettings:Provider", "EmailSettings__Provider" }
};

// Add AWS or SMTP based on provider
var emailProvider = builder.Configuration["EmailSettings:Provider"];
if (emailProvider == "aws_ses")
{
    requiredConfigs.Add("AwsSettings:Region", "AwsSettings__Region");
    requiredConfigs.Add("AwsSettings:AccessKey", "AwsSettings__AccessKey");
    requiredConfigs.Add("AwsSettings:SecretKey", "AwsSettings__SecretKey");
    requiredConfigs.Add("AwsSettings:SenderEmail", "AwsSettings__SenderEmail");
}
else if (emailProvider == "google_workspace")
{
    requiredConfigs.Add("SmtpSettings:Host", "SmtpSettings__Host");
    requiredConfigs.Add("SmtpSettings:Port", "SmtpSettings__Port");
    requiredConfigs.Add("SmtpSettings:Username", "SmtpSettings__Username");
    requiredConfigs.Add("SmtpSettings:Password", "SmtpSettings__Password");
}

bool hasAllConfig = true;
Log.Information("--------------------------------------------------");
Log.Information("Validating Environment Configurations...");

foreach (var config in requiredConfigs)
{
    var value = builder.Configuration[config.Key];
    if (string.IsNullOrWhiteSpace(value))
    {
        Log.Error("CRITICAL MISSING CONFIG: {Key} (Ensure environment variable '{EnvVar}' is set)", config.Key, config.Value);
        hasAllConfig = false;
    }
}

if (!hasAllConfig)
{
    Log.Fatal("Application startup failed due to missing configurations. System shutting down.");
    return; // Stop the application
}

Log.Information("All required environment configurations found.");
Log.Information("Active CORS Allowed Origins: {Origins}", builder.Configuration["CorsSettings:AllowedOrigins"]);
Log.Information("--------------------------------------------------");
// --------------------------------

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

if (emailProvider == "google_workspace")
{
    builder.Services.AddScoped<IEmailService, ResumeInOneMinute.Infrastructure.Services.SmtpEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, ResumeInOneMinute.Infrastructure.Services.AwsSesEmailService>();
}

// Register Composite AI Service (Groq with Ollama fallback)
builder.Services.AddScoped<IOllamaService, ResumeInOneMinute.Infrastructure.Services.CompositeAIService>();
builder.Services.AddScoped<IGroqService, ResumeInOneMinute.Infrastructure.Services.GroqService>();

// Register Repositories (DbContext is created manually in repository)
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<IHtmlTemplateRepository, HtmlTemplateRepository>();
builder.Services.AddScoped<IBillingRepository, BillingRepository>();

// Register Services
builder.Services.AddScoped<IRazorpayService, ResumeInOneMinute.Infrastructure.Services.RazorpayService>();
builder.Services.AddScoped<ISubscriptionService, ResumeInOneMinute.Infrastructure.Services.SubscriptionService>();

// Configure MongoDB Settings
// Handled by MongoDbService via IConfiguration

// Configure JWT Settings

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;


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

// Rate limiting is now handled via custom middleware for plan-based dynamic limits
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add Response Caching
builder.Services.AddResponseCaching();

// Add Memory Cache
builder.Services.AddMemoryCache();


// Add CORS
string allowedOrigins = builder.Configuration["CorsSettings:AllowedOrigins"]!;

var allowedOriginArray = allowedOrigins
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
        Title = "1mincv.com API",
        Version = "v1",
        Description = @"
## 1mincv.com API Documentation

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
            Name = "1mincv.com Support",
            Email = "support@1mincv.com",
            Url = new Uri("https://1mincv.com")
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "1mincv.com API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
        
        // Enhanced UI settings
        c.DocumentTitle = "1mincv.com API Documentation";
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "1mincv.com API v1");
        c.RoutePrefix = "api-docs";
        c.DocumentTitle = "1mincv.com API Documentation";
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

// Plan-based Rate Limiting (Must be after Auth)
app.UseMiddleware<ResumeInOneMinute.Middleware.RateLimitingMiddleware>();

app.MapControllers();


app.Run();

