# 1mincv.com API

## Overview
This is a comprehensive ASP.NET Core Web API with user authentication, authorization, rate limiting, caching, and audit logging.

## Features
- ✅ User Registration & Login
- ✅ JWT Authentication & Authorization
- ✅ Password Hashing (HMACSHA512) with PasswordHash and PasswordSalt
- ✅ User Profile Management (Get & Update)
- ✅ Password Change
- ✅ Rate Limiting by User ID
- ✅ Response Caching
- ✅ Global Exception Handling
- ✅ Error Logging to MongoDB
- ✅ Audit Logging to MongoDB
- ✅ Swagger/OpenAPI Documentation
- ✅ PostgreSQL Database with EF Core (Code First)

## Technology Stack
- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL (User Data)
- **NoSQL**: MongoDB (Logs & Audit Trails)
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **API Documentation**: Swagger/OpenAPI

## Database Configuration

### PostgreSQL
- **Host**: localhost
- **Port**: 5432
- **Database**: resume_app_dev
- **Username**: postgres
- **Password**: htmltopdffile@007

### MongoDB
- **URI**: mongodb://localhost:27017/resume_documents_dev
- **Collections**:
  - ErrorLogs - Stores all application errors
  - AuditLogs - Stores user activity audit trails

## API Endpoints

### Authentication Endpoints

#### 1. Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1234567890",
  "password": "Password@123",
  "confirmPassword": "Password@123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Registration successful",
  "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "+1234567890",
    "role": "User",
    "isActive": true,
    "emailConfirmed": false,
    "createdAt": "2026-01-05T18:33:20.000Z",
    "lastLoginAt": null
  }
}
```

#### 2. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "Password@123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "+1234567890",
    "role": "User",
    "isActive": true,
    "emailConfirmed": false,
    "createdAt": "2026-01-05T18:33:20.000Z",
    "lastLoginAt": "2026-01-05T18:35:10.000Z"
  }
}
```

### User Profile Endpoints (Requires Authentication)

#### 3. Get Profile
```http
GET /api/user/profile
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Profile retrieved successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "+1234567890",
    "role": "User",
    "isActive": true,
    "emailConfirmed": false,
    "createdAt": "2026-01-05T18:33:20.000Z",
    "lastLoginAt": "2026-01-05T18:35:10.000Z"
  }
}
```

#### 4. Update Profile
```http
PUT /api/user/profile
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+1234567890"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "John",
    "lastName": "Smith",
    "email": "john.doe@example.com",
    "phoneNumber": "+1234567890",
    "role": "User",
    "isActive": true,
    "emailConfirmed": false,
    "createdAt": "2026-01-05T18:33:20.000Z",
    "lastLoginAt": "2026-01-05T18:35:10.000Z"
  }
}
```

#### 5. Change Password
```http
POST /api/user/change-password
Authorization: Bearer {token}
Content-Type: application/json

{
  "currentPassword": "Password@123",
  "newPassword": "NewPassword@456",
  "confirmPassword": "NewPassword@456"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Password changed successfully"
}
```

## Password Requirements
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character (@$!%*?&)

## Rate Limiting
- **Policy**: Fixed Window
- **Limit**: 100 requests per user
- **Window**: 60 seconds
- **Response Code**: 429 (Too Many Requests)

## Security Features

### Password Storage
- Passwords are hashed using HMACSHA512
- Each password has a unique salt
- Stored in `PasswordHash` and `PasswordSalt` columns

### JWT Token
- **Algorithm**: HS512
- **Expiration**: 60 minutes
- **Claims**: UserId, Email, Name, Role

### Authorization
- All `/api/user/*` endpoints require authentication
- JWT token must be included in the Authorization header

## Error Handling
- Global exception handler middleware
- All errors logged to MongoDB
- Standardized error responses
- Appropriate HTTP status codes

## Audit Logging
- All authenticated user actions are logged
- Tracks: POST, PUT, DELETE operations
- Stores: User info, action, entity type, old/new values, timestamp, IP address

## Running the Application

### Prerequisites
1. .NET 8.0 SDK
2. PostgreSQL Server (running on localhost:5432)
3. MongoDB Server (running on localhost:27017)

### Steps
1. Ensure PostgreSQL and MongoDB are running
2. Navigate to the project directory
3. Run the application:
   ```bash
   cd ResumeInOneMinute
   dotnet run
   ```
4. The application will:
   - Automatically apply database migrations
   - Create the Users table in PostgreSQL
   - Start the API server
   - Open Swagger UI at the root URL

### Access Swagger UI
Once the application is running, navigate to:
```
https://localhost:{port}/
```

The Swagger UI will be displayed at the root of the application, allowing you to:
- View all available endpoints
- Test API calls directly
- Authenticate using JWT tokens
- See request/response schemas

### Testing with Swagger
1. First, register a new user using `/api/auth/register`
2. Copy the JWT token from the response
3. Click the "Authorize" button in Swagger UI
4. Enter: `Bearer {your-token}`
5. Now you can test protected endpoints

## Project Structure
```
ResumeInOneMinute/
├── ResumeInOneMinute/              # Main API Project
│   ├── Controllers/
│   │   ├── AuthController.cs       # Registration & Login
│   │   └── UserController.cs       # Profile Management
│   ├── Program.cs                  # Application Configuration
│   └── appsettings.json            # Configuration Settings
├── ResumeInOneMinute.Domain/       # Domain Entities
│   ├── Entities/
│   │   └── User.cs                 # User Entity
│   └── Models/
│       ├── ErrorLog.cs             # Error Log Model
│       └── AuditLog.cs             # Audit Log Model
├── ResumeInOneMinute.Contracts/    # DTOs
│   └── DTOs/
│       ├── UserRequests.cs         # Request DTOs
│       └── UserResponses.cs        # Response DTOs
├── ResumeInOneMinute.Infrastructure/ # Infrastructure Services
│   ├── Data/
│   │   └── ApplicationDbContext.cs # EF Core DbContext
│   ├── Services/
│   │   ├── AuthService.cs          # Authentication Service
│   │   └── MongoDbService.cs       # MongoDB Service
│   ├── Middleware/
│   │   ├── GlobalExceptionHandlerMiddleware.cs
│   │   └── AuditLogMiddleware.cs
│   └── Configuration/
│       ├── JwtSettings.cs
│       └── MongoDbSettings.cs
└── ResumeInOneMinute.Application/  # Application Services
    └── Services/
        └── UserService.cs          # User Service
```

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123456789!@#",
    "Issuer": "1mincv.com",
    "Audience": "1mincv.comUsers",
    "ExpirationInMinutes": 60
  }
}
```

### Rate Limit Settings (appsettings.json)
```json
{
  "RateLimitSettings": {
    "PermitLimit": 100,
    "Window": 60,
    "QueueLimit": 0
  }
}
```

## Troubleshooting

### Database Connection Issues
- Ensure PostgreSQL is running on localhost:5432
- Verify the database credentials in appsettings.json
- Check if the database `resume_app_dev` exists

### MongoDB Connection Issues
- Ensure MongoDB is running on localhost:27017
- The database and collections will be created automatically

### Migration Issues
- Migrations are applied automatically on application startup
- Check the console output for migration errors

## Future Enhancements
- Email confirmation
- Password reset functionality
- Refresh tokens
- Role-based authorization
- Two-factor authentication
- API versioning
- Distributed caching with Redis

## Support
For issues or questions, please contact the development team.
