# Architecture Diagram

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Client Applications                      │
│                  (Browser, Mobile App, Postman)                  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ HTTP/HTTPS
                             │
┌────────────────────────────▼────────────────────────────────────┐
│                      ASP.NET Core Web API                        │
│                     (ResumeInOneMinute)                          │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐    │
│  │                    Middleware Pipeline                  │    │
│  │  1. Global Exception Handler                           │    │
│  │  2. Audit Log Middleware                               │    │
│  │  3. CORS                                               │    │
│  │  4. Response Caching                                   │    │
│  │  5. Rate Limiter (by User ID)                          │    │
│  │  6. Authentication (JWT)                               │    │
│  │  7. Authorization                                      │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐    │
│  │                      Controllers                        │    │
│  │  • AuthController (Register, Login)                    │    │
│  │  • UserController (Profile, Update, Change Password)   │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐    │
│  │                   Application Services                  │    │
│  │  • UserService (CRUD Operations)                       │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐    │
│  │                 Infrastructure Services                 │    │
│  │  • AuthService (Password Hash, JWT Token)              │    │
│  │  • MongoDbService (Error & Audit Logging)              │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                  │
└──────────────────┬───────────────────────────┬──────────────────┘
                   │                           │
                   │                           │
        ┌──────────▼──────────┐     ┌─────────▼──────────┐
        │   PostgreSQL DB     │     │    MongoDB         │
        │  (resume_app_dev)   │     │ (resume_docs_dev)  │
        │                     │     │                    │
        │  Tables:            │     │  Collections:      │
        │  • Users            │     │  • ErrorLogs       │
        │    - Id             │     │  • AuditLogs       │
        │    - Email          │     │                    │
        │    - PasswordHash   │     │                    │
        │    - PasswordSalt   │     │                    │
        │    - FirstName      │     │                    │
        │    - LastName       │     │                    │
        │    - PhoneNumber    │     │                    │
        │    - Role           │     │                    │
        │    - IsActive       │     │                    │
        │    - CreatedAt      │     │                    │
        │    - UpdatedAt      │     │                    │
        │    - LastLoginAt    │     │                    │
        └─────────────────────┘     └────────────────────┘
```

## Request Flow

### 1. User Registration Flow
```
Client
  │
  │ POST /api/auth/register
  │ { firstName, lastName, email, password }
  │
  ▼
AuthController
  │
  ├─► Check if email exists (UserService)
  │
  ├─► Create password hash (AuthService)
  │   └─► HMACSHA512(password + salt)
  │
  ├─► Create user (UserService)
  │   └─► Save to PostgreSQL
  │
  ├─► Generate JWT token (AuthService)
  │
  ├─► Log audit (MongoDbService)
  │   └─► Save to MongoDB
  │
  └─► Return { token, user }
```

### 2. User Login Flow
```
Client
  │
  │ POST /api/auth/login
  │ { email, password }
  │
  ▼
AuthController
  │
  ├─► Get user by email (UserService)
  │
  ├─► Verify password (AuthService)
  │   └─► Compare HMACSHA512 hashes
  │
  ├─► Update last login (UserService)
  │
  ├─► Generate JWT token (AuthService)
  │
  ├─► Log audit (MongoDbService)
  │   └─► Save to MongoDB
  │
  └─► Return { token, user }
```

### 3. Protected Endpoint Flow (e.g., Get Profile)
```
Client
  │
  │ GET /api/user/profile
  │ Authorization: Bearer {token}
  │
  ▼
Rate Limiter
  │
  ├─► Check user request count
  │   └─► Allow or return 429
  │
  ▼
Authentication Middleware
  │
  ├─► Validate JWT token
  │   └─► Extract user claims
  │
  ▼
UserController
  │
  ├─► Get user by ID (UserService)
  │   └─► Query PostgreSQL
  │
  └─► Return user profile
```

### 4. Error Handling Flow
```
Any Endpoint
  │
  │ Exception occurs
  │
  ▼
Global Exception Handler
  │
  ├─► Catch exception
  │
  ├─► Log to console
  │
  ├─► Create error log
  │   └─► Save to MongoDB (MongoDbService)
  │
  └─► Return standardized error response
      { success: false, message, errors }
```

### 5. Audit Logging Flow
```
Any POST/PUT/DELETE Request
  │
  │ Authenticated user action
  │
  ▼
Audit Log Middleware
  │
  ├─► Extract user info from JWT
  │
  ├─► Capture request details
  │
  ├─► Create audit log
  │   └─► Save to MongoDB (MongoDbService)
  │
  └─► Continue to controller
```

## Security Layers

```
┌─────────────────────────────────────────────────────────┐
│ Layer 1: Rate Limiting                                  │
│ • 100 requests per 60 seconds per user                  │
│ • Prevents brute force attacks                          │
└─────────────────────────────────────────────────────────┘
                         │
┌─────────────────────────▼───────────────────────────────┐
│ Layer 2: JWT Authentication                             │
│ • Token validation                                      │
│ • Claims extraction                                     │
│ • Expiration check (60 minutes)                         │
└─────────────────────────────────────────────────────────┘
                         │
┌─────────────────────────▼───────────────────────────────┐
│ Layer 3: Authorization                                  │
│ • Role-based access control                             │
│ • User-specific data access                             │
└─────────────────────────────────────────────────────────┘
                         │
┌─────────────────────────▼───────────────────────────────┐
│ Layer 4: Password Security                              │
│ • HMACSHA512 hashing                                    │
│ • Unique salt per password                              │
│ • Stored as byte arrays                                 │
└─────────────────────────────────────────────────────────┘
                         │
┌─────────────────────────▼───────────────────────────────┐
│ Layer 5: Audit & Error Logging                          │
│ • All actions logged to MongoDB                         │
│ • Complete audit trail                                  │
│ • Error tracking and monitoring                         │
└─────────────────────────────────────────────────────────┘
```

## Data Flow

```
┌──────────────┐
│   Client     │
└──────┬───────┘
       │
       │ 1. Request with credentials
       │
┌──────▼───────────────────────────────────────────┐
│              API Gateway                          │
│  (Middleware: CORS, Rate Limit, Auth)            │
└──────┬───────────────────────────────────────────┘
       │
       │ 2. Validated request
       │
┌──────▼───────────────────────────────────────────┐
│            Controllers                            │
│  (Business logic, validation)                    │
└──────┬───────────────────────────────────────────┘
       │
       │ 3. Service calls
       │
┌──────▼───────────────────────────────────────────┐
│          Application Services                     │
│  (UserService, AuthService)                      │
└──────┬───────────────────────────────────────────┘
       │
       │ 4. Data operations
       │
┌──────▼──────────┐         ┌─────────────────────┐
│   PostgreSQL    │         │      MongoDB        │
│  (User Data)    │         │  (Logs & Audit)     │
└─────────────────┘         └─────────────────────┘
```

## Technology Stack

```
┌─────────────────────────────────────────────────────┐
│                   Presentation Layer                 │
│  • Swagger UI                                       │
│  • RESTful API Endpoints                            │
└─────────────────────────────────────────────────────┘
                         │
┌─────────────────────────▼───────────────────────────┐
│                   Application Layer                  │
│  • ASP.NET Core 8.0                                 │
│  • Controllers                                      │
│  • Middleware Pipeline                              │
└─────────────────────────────────────────────────────┘
                         │
┌─────────────────────────▼───────────────────────────┐
│                    Business Layer                    │
│  • Application Services                             │
│  • Domain Logic                                     │
│  • DTOs (Data Transfer Objects)                     │
└─────────────────────────────────────────────────────┘
                         │
┌─────────────────────────▼───────────────────────────┐
│                 Infrastructure Layer                 │
│  • EF Core (PostgreSQL)                             │
│  • MongoDB Driver                                   │
│  • JWT Authentication                               │
│  • Password Hashing                                 │
└─────────────────────────────────────────────────────┘
                         │
┌─────────────────────────▼───────────────────────────┐
│                     Data Layer                       │
│  • PostgreSQL (Relational Data)                     │
│  • MongoDB (Document Data)                          │
└─────────────────────────────────────────────────────┘
```
