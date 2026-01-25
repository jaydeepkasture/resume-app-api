# Authentication API Implementation

## Overview
This document describes the complete implementation of the Login and Registration API for the Resume In One Minute application.

## Architecture

### Project Structure
```
ResumeInOneMinute/
├── ResumeInOneMinute/                  # Main API Project
│   ├── Controllers/
│   │   └── AccountController.cs        # Authentication endpoints
│   └── Program.cs                      # Application configuration
├── ResumeInOneMinute.Domain/           # Domain Layer
│   ├── Model/
│   │   ├── User.cs                     # User entity
│   │   ├── UserProfile.cs              # User profile entity
│   │   └── Response.cs                 # Standard response wrapper
│   ├── DTO/
│   │   ├── RegisterDto.cs              # Registration request DTO
│   │   ├── LoginDto.cs                 # Login request DTO
│   │   ├── UserDto.cs                  # User response DTO
│   │   └── AuthResponseDto.cs          # Authentication response DTO
│   └── Interface/
│       └── IAccountRepository.cs       # Repository contract
└── ResumeInOneMinute.Repository/       # Data Access Layer
    ├── DataContexts/
    │   └── ApplicationDbContext.cs     # EF Core DbContext
    └── Repositories/
        └── AccountRepository.cs        # Business logic implementation
```

## Database Schema

### Tables Created
The application uses the following PostgreSQL tables in the `auth` schema:

#### auth.users
```sql
CREATE TABLE auth.users (
    user_id        BIGSERIAL PRIMARY KEY,
    global_user_id UUID         NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    email          VARCHAR(255) NOT NULL UNIQUE,
    password_hash  BYTEA        NOT NULL,
    password_salt  BYTEA        NOT NULL,
    is_active      BOOLEAN      NOT NULL        DEFAULT TRUE,
    created_at     TIMESTAMPTZ  NOT NULL        DEFAULT (NOW() AT TIME ZONE 'UTC'),
    updated_at     TIMESTAMPTZ
);
```

#### auth.user_profiles
```sql
CREATE TABLE auth.user_profiles (
    user_profile_id      BIGSERIAL PRIMARY KEY,
    user_id              BIGINT      NOT NULL,
    global_user_profile_ UUID        NOT NULL,
    first_name           VARCHAR(100),
    last_name            VARCHAR(100),
    phone                VARCHAR(10),
    country_code         VARCHAR(3),
    created_at           TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT fk_profile_user FOREIGN KEY (user_id) REFERENCES auth.users (user_id) ON DELETE CASCADE
);
```

## API Endpoints

### 1. Register User
**Endpoint:** `POST /api/Account/register`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "YourPassword123"
}
```

**Success Response (200 OK):**
```json
{
  "status": true,
  "message": "Registration successful",
  "data": {
    "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "userId": 1,
      "globalUserId": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "isActive": true,
      "createdAt": "2026-01-07T06:35:00Z"
    }
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Email already exists",
  "data": null
}
```

**Validation Rules:**
- Email is required and must be valid format
- Password is required and must be at least 6 characters

### 2. Login User
**Endpoint:** `POST /api/Account/login`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "YourPassword123"
}
```

**Success Response (200 OK):**
```json
{
  "status": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "userId": 1,
      "globalUserId": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "isActive": true,
      "createdAt": "2026-01-07T06:35:00Z"
    }
  }
}
```

**Error Response (401 Unauthorized):**
```json
{
  "status": false,
  "message": "Invalid email or password",
  "data": null
}
```

## Security Features

### Password Hashing
- Uses **HMACSHA512** algorithm for password hashing
- Each password has a unique salt
- Password hash and salt stored as byte arrays in database
- Passwords are never stored in plain text

### JWT Authentication
- Token-based authentication using JWT
- Token includes user claims:
  - User ID (NameIdentifier)
  - Email
  - Global User ID
- Token expiration: 60 minutes (configurable)
- Tokens signed with HS512 algorithm

### Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=resume_app_dev;Username=postgres;Password=htmltopdffile@007"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123456789!@#",
    "Issuer": "ResumeInOneMinute",
    "Audience": "ResumeInOneMinuteUsers",
    "ExpirationInMinutes": 60
  }
}
```

## Business Logic Flow

### Registration Flow
1. **Validate Input**: Check email format and password requirements
2. **Check Duplicate**: Verify email doesn't already exist
3. **Hash Password**: Generate unique salt and hash password using HMACSHA512
4. **Create User**: Insert user record into `auth.users` table
5. **Create Profile**: Insert user profile record into `auth.user_profiles` table
6. **Generate Token**: Create JWT token with user claims
7. **Return Response**: Send token and user data in standardized Response format

### Login Flow
1. **Validate Input**: Check email format and password presence
2. **Find User**: Query user by email (case-insensitive)
3. **Verify Password**: Compare hashed password with stored hash
4. **Check Active Status**: Ensure user account is active
5. **Update Last Login**: Set UpdatedAt timestamp
6. **Generate Token**: Create JWT token with user claims
7. **Return Response**: Send token and user data in standardized Response format

## Key Implementation Details

### Repository Pattern
- **Controller**: Handles HTTP requests/responses only
- **Repository**: Contains all business logic
- **Interface**: Defines contract for dependency injection

### Response Standardization
All API responses use the `Response<T>` class:
```csharp
public class Response<T>
{
    public bool Status { get; set; }      // true = success, false = failure
    public string Message { get; set; }   // User-friendly message
    public T Data { get; set; }           // Response data
}
```

### EF Core Configuration
- Code-First approach with explicit column mappings
- PostgreSQL-specific features (UUID generation, timezone handling)
- Proper foreign key relationships and cascade deletes
- UTC timestamps for all datetime fields

## Testing

### Using PowerShell
Run the test script:
```powershell
.\test-auth-api.ps1
```

### Using Swagger
1. Navigate to `http://localhost:5299`
2. Expand `/api/Account/register` or `/api/Account/login`
3. Click "Try it out"
4. Enter request body
5. Click "Execute"

### Using Postman/cURL

**Register:**
```bash
curl -X POST http://localhost:5299/api/Account/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test@123456"}'
```

**Login:**
```bash
curl -X POST http://localhost:5299/api/Account/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test@123456"}'
```

## Running the Application

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL database running on localhost:5432
- Database `resume_app_dev` created

### Steps
1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Build solution:**
   ```bash
   dotnet build
   ```

3. **Run application:**
   ```bash
   dotnet run --project ResumeInOneMinute
   ```

4. **Access Swagger UI:**
   Open browser to `http://localhost:5299`

## NuGet Packages Used

### Main API Project
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.11)
- Microsoft.EntityFrameworkCore (8.0.11)
- Microsoft.EntityFrameworkCore.Design (8.0.11)
- Npgsql.EntityFrameworkCore.PostgreSQL (8.0.11)
- Swashbuckle.AspNetCore (6.6.2)

### Repository Project
- Microsoft.EntityFrameworkCore (8.0.11)
- Microsoft.EntityFrameworkCore.Design (8.0.11)
- Microsoft.Extensions.Configuration.Abstractions (8.0.0)
- Npgsql.EntityFrameworkCore.PostgreSQL (8.0.11)
- System.IdentityModel.Tokens.Jwt (8.0.2)

## Future Enhancements
- Email verification
- Password reset functionality
- Refresh tokens
- Two-factor authentication
- Account lockout after failed attempts
- Password complexity requirements
- User profile update endpoints
- Change password endpoint
