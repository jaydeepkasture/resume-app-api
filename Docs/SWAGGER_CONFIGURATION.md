# Swagger Configuration Guide

## Overview
The Resume In One Minute API now has a fully configured Swagger/OpenAPI documentation interface with enhanced features for better developer experience.

## Access Swagger UI

### Development Environment
- **URL**: `http://localhost:5299`
- **Route**: Root URL (/)
- Swagger UI is set as the default landing page

### Production Environment (Optional)
- **URL**: `http://localhost:5299/api-docs`
- **Route**: /api-docs
- Can be disabled by removing the production Swagger configuration in Program.cs

## Features Implemented

### 1. **Enhanced API Documentation**
- ✅ Comprehensive API description with markdown support
- ✅ Getting Started guide
- ✅ Response format documentation
- ✅ Authentication instructions
- ✅ Contact information and license details

### 2. **JWT Authentication Integration**
- ✅ "Authorize" button in Swagger UI
- ✅ Bearer token authentication
- ✅ Automatic token injection in requests
- ✅ Clear instructions for token usage

### 3. **XML Documentation**
- ✅ XML comments enabled for all endpoints
- ✅ Detailed parameter descriptions
- ✅ Request/Response examples in remarks
- ✅ Multiple response code documentation

### 4. **Swagger Annotations**
- ✅ SwaggerOperation attributes for detailed endpoint info
- ✅ SwaggerResponse attributes for response documentation
- ✅ SwaggerTag for controller categorization
- ✅ ProducesResponseType for type safety

### 5. **Enhanced UI Features**
- ✅ Custom document title
- ✅ Request duration display
- ✅ Deep linking support
- ✅ Filter/search functionality
- ✅ Schema validator
- ✅ Collapsible sections
- ✅ Custom CSS styling

### 6. **Custom Styling**
- ✅ Professional color scheme
- ✅ Improved button styles
- ✅ Better code highlighting
- ✅ Enhanced readability
- ✅ Custom scrollbars

## How to Use Swagger UI

### Step 1: Access Swagger
1. Start the application: `dotnet run --project ResumeInOneMinute`
2. Open browser to: `http://localhost:5299`
3. You'll see the Swagger UI with all API endpoints

### Step 2: Test Registration
1. Expand the **POST /api/Account/register** endpoint
2. Click **"Try it out"** button
3. Edit the request body:
   ```json
   {
     "email": "test@example.com",
     "password": "SecurePass123"
   }
   ```
4. Click **"Execute"**
5. View the response with JWT token

### Step 3: Authenticate
1. Copy the `token` value from the registration response
2. Click the **"Authorize"** button at the top of the page
3. In the popup, enter: `Bearer {your-token}`
   - Example: `Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...`
4. Click **"Authorize"**
5. Click **"Close"**
6. The lock icon will turn green, indicating you're authenticated

### Step 4: Test Login
1. Expand the **POST /api/Account/login** endpoint
2. Click **"Try it out"**
3. Enter credentials:
   ```json
   {
     "email": "test@example.com",
     "password": "SecurePass123"
   }
   ```
4. Click **"Execute"**
5. View the response

### Step 5: Test Protected Endpoints (Future)
- Once authenticated, you can test any protected endpoints
- The JWT token will be automatically included in requests
- Look for the lock icon 🔒 on protected endpoints

## Configuration Details

### Program.cs Configuration

#### Swagger Generation
```csharp
builder.Services.AddSwaggerGen(c =>
{
    // API Information
    c.SwaggerDoc("v1", new OpenApiInfo { ... });
    
    // XML Documentation
    c.IncludeXmlComments(xmlPath);
    
    // JWT Security
    c.AddSecurityDefinition("Bearer", ...);
    c.AddSecurityRequirement(...);
    
    // Enhanced Features
    c.EnableAnnotations();
    c.OrderActionsBy(apiDesc => apiDesc.RelativePath);
    c.CustomSchemaIds(type => type.FullName);
});
```

#### Swagger UI
```csharp
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resume In One Minute API v1");
    c.RoutePrefix = string.Empty;
    c.DocumentTitle = "Resume In One Minute API Documentation";
    c.DocExpansion(DocExpansion.List);
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.InjectStylesheet("/swagger-ui/custom.css");
});
```

### Controller Annotations

#### XML Documentation
```csharp
/// <summary>
/// Brief description
/// </summary>
/// <remarks>
/// Detailed description with examples
/// </remarks>
/// <param name="dto">Parameter description</param>
/// <returns>Return value description</returns>
/// <response code="200">Success description</response>
/// <response code="400">Error description</response>
```

#### Swagger Attributes
```csharp
[SwaggerOperation(
    Summary = "Short summary",
    Description = "Detailed description",
    OperationId = "UniqueOperationId",
    Tags = new[] { "TagName" }
)]
[SwaggerResponse(200, "Success message", typeof(ResponseType))]
[SwaggerResponse(400, "Error message", typeof(ErrorType))]
[ProducesResponseType(typeof(ResponseType), StatusCodes.Status200OK)]
```

## API Documentation Structure

### Account Controller
- **Tag**: Account
- **Description**: User authentication and account management
- **Endpoints**:
  - `POST /api/Account/register` - Register new user
  - `POST /api/Account/login` - Login with credentials

### Request Examples

#### Register
```json
POST /api/Account/register
{
  "email": "user@example.com",
  "password": "SecurePassword123"
}
```

#### Login
```json
POST /api/Account/login
{
  "email": "user@example.com",
  "password": "SecurePassword123"
}
```

### Response Format
All endpoints return:
```json
{
  "status": true/false,
  "message": "Descriptive message",
  "data": {
    "token": "JWT token",
    "user": {
      "userId": 1,
      "globalUserId": "uuid",
      "email": "user@example.com",
      "isActive": true,
      "createdAt": "2026-01-07T06:35:00Z"
    }
  }
}
```

## Custom CSS Styling

### Location
- File: `wwwroot/swagger-ui/custom.css`
- Automatically injected into Swagger UI

### Features
- Professional color scheme (blue/green theme)
- Enhanced button styles
- Improved code highlighting
- Better readability
- Custom scrollbars
- Responsive design

### Customization
To modify the styling:
1. Edit `wwwroot/swagger-ui/custom.css`
2. Rebuild the application
3. Refresh the browser

## NuGet Packages

### Required Packages
- `Swashbuckle.AspNetCore` (6.6.2)
- `Swashbuckle.AspNetCore.Annotations` (6.6.2)

### Installation
Already included in the project. If needed:
```bash
dotnet add package Swashbuckle.AspNetCore --version 6.6.2
dotnet add package Swashbuckle.AspNetCore.Annotations --version 6.6.2
```

## Best Practices

### 1. XML Documentation
- Always add `<summary>` for brief descriptions
- Use `<remarks>` for detailed information and examples
- Document all parameters with `<param>`
- Document return values with `<returns>`
- Document all response codes with `<response>`

### 2. Swagger Attributes
- Use `[SwaggerOperation]` for operation-level details
- Use `[SwaggerResponse]` for response documentation
- Use `[ProducesResponseType]` for type safety
- Use `[SwaggerTag]` for controller categorization

### 3. Response Types
- Always specify response types in attributes
- Use consistent response models
- Document all possible status codes
- Provide meaningful descriptions

### 4. Examples
- Include request examples in `<remarks>`
- Show sample JSON in documentation
- Demonstrate authentication flow
- Provide error examples

## Troubleshooting

### Issue: Swagger UI not loading
**Solution**: 
- Ensure `app.UseSwagger()` is called before `app.UseSwaggerUI()`
- Check that you're accessing the correct URL
- Verify the application is running

### Issue: XML comments not showing
**Solution**:
- Ensure `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is in .csproj
- Rebuild the application
- Check that XML file is generated in bin folder

### Issue: Custom CSS not applied
**Solution**:
- Ensure `app.UseStaticFiles()` is called before Swagger middleware
- Verify CSS file exists in `wwwroot/swagger-ui/custom.css`
- Clear browser cache
- Check browser console for 404 errors

### Issue: Authentication not working
**Solution**:
- Ensure you're using the format: `Bearer {token}`
- Copy the entire token without truncation
- Check token hasn't expired (60 minutes)
- Verify JWT configuration in appsettings.json

## Advanced Configuration

### Adding New Endpoints
1. Create controller method with XML documentation
2. Add Swagger attributes
3. Rebuild application
4. Swagger will automatically detect and document

### Versioning
To add API versioning:
```csharp
c.SwaggerDoc("v2", new OpenApiInfo { 
    Title = "Resume In One Minute API", 
    Version = "v2" 
});
```

### Custom Filters
Add custom operation filters:
```csharp
c.OperationFilter<YourCustomFilter>();
```

### Multiple Swagger Documents
```csharp
c.SwaggerDoc("auth", new OpenApiInfo { Title = "Auth API" });
c.SwaggerDoc("resume", new OpenApiInfo { Title = "Resume API" });
```

## Security Considerations

### Production Deployment
- Consider disabling Swagger in production
- Or restrict access to authenticated users
- Use HTTPS only
- Implement rate limiting

### Configuration
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

## Summary

✅ **Swagger is fully configured** with:
- Comprehensive API documentation
- JWT authentication integration
- XML documentation support
- Enhanced UI features
- Custom styling
- Professional appearance

🚀 **Access it now**: `http://localhost:5299`

📚 **Documentation includes**:
- Getting started guide
- Request/response examples
- Authentication instructions
- Error handling
- Response format standards
