# BaseRepository Pattern - DRY Implementation

## 🎯 Problem Solved

**Issue**: The `CreateDbContext()` method and connection string management would be duplicated across multiple repositories, violating the **DRY (Don't Repeat Yourself)** principle.

**Solution**: Created a `BaseRepository` class that centralizes common database context management functionality.

## ✅ Implementation

### 1. **BaseRepository Class**

**Location**: `ResumeInOneMinute.Repository/Repositories/BaseRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ResumeInOneMinute.Repository.DataContexts;

namespace ResumeInOneMinute.Repository.Repositories;

/// <summary>
/// Base repository class providing common database context management functionality
/// </summary>
public abstract class BaseRepository
{
    protected readonly IConfiguration Configuration;
    private readonly string _connectionString;

    protected BaseRepository(IConfiguration configuration)
    {
        Configuration = configuration;
        _connectionString = configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("PostgreSQL connection string not configured");
    }

    /// <summary>
    /// Creates a new instance of ApplicationDbContext with manual control
    /// </summary>
    protected ApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(_connectionString);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
```

### 2. **Updated AccountRepository**

**Before** (Violates DRY):
```csharp
public class AccountRepository : IAccountRepository
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public AccountRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("PostgreSQL connection string not configured");
    }

    // ... methods ...

    private ApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(_connectionString);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
```

**After** (Follows DRY):
```csharp
public class AccountRepository : BaseRepository, IAccountRepository
{
    public AccountRepository(IConfiguration configuration) : base(configuration)
    {
    }

    // ... methods using CreateDbContext() from BaseRepository ...
}
```

## 📊 Benefits

### 1. **DRY Principle**
- ✅ `CreateDbContext()` method defined **once** in `BaseRepository`
- ✅ Connection string management centralized
- ✅ No code duplication across repositories

### 2. **Maintainability**
- ✅ Changes to DbContext creation logic in **one place**
- ✅ Easy to add features (logging, caching, etc.)
- ✅ Consistent behavior across all repositories

### 3. **Scalability**
- ✅ New repositories just inherit from `BaseRepository`
- ✅ Automatic access to `CreateDbContext()` method
- ✅ Access to `Configuration` for other settings

### 4. **Flexibility**
- ✅ Can add more helper methods to `BaseRepository`
- ✅ Can override methods in derived classes if needed
- ✅ Protected access allows customization

## 🚀 Usage in New Repositories

When creating a new repository, simply inherit from `BaseRepository`:

```csharp
public class UserProfileRepository : BaseRepository, IUserProfileRepository
{
    public UserProfileRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<UserProfile?> GetProfileByUserIdAsync(long userId)
    {
        using (var context = CreateDbContext()) // Inherited from BaseRepository
        {
            return await context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }

    public async Task<bool> UpdateProfileAsync(UserProfile profile)
    {
        using (var context = CreateDbContext()) // Inherited from BaseRepository
        {
            context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
```

## 🔧 What's Available in BaseRepository

### Protected Members (Accessible in Derived Classes)

1. **`Configuration`** - IConfiguration instance
   ```csharp
   protected readonly IConfiguration Configuration;
   ```
   Use for accessing any configuration settings:
   ```csharp
   var jwtSettings = Configuration.GetSection("JwtSettings");
   var someValue = Configuration["SomeKey"];
   ```

2. **`CreateDbContext()`** - Creates new DbContext instance
   ```csharp
   protected ApplicationDbContext CreateDbContext()
   ```
   Use in `using` statements for database operations:
   ```csharp
   using (var context = CreateDbContext())
   {
       // Database operations
   }
   ```

### Private Members (Internal to BaseRepository)

1. **`_connectionString`** - PostgreSQL connection string
   - Loaded once in constructor
   - Used by `CreateDbContext()`
   - Not accessible in derived classes (encapsulated)

## 📝 Pattern Examples

### Example 1: Simple Query
```csharp
public async Task<User?> GetUserAsync(long userId)
{
    using (var context = CreateDbContext())
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }
}
```

### Example 2: Insert Operation
```csharp
public async Task<bool> CreateUserAsync(User user)
{
    using (var context = CreateDbContext())
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return true;
    }
}
```

### Example 3: Update Operation
```csharp
public async Task<bool> UpdateUserAsync(User user)
{
    using (var context = CreateDbContext())
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return true;
    }
}
```

### Example 4: Complex Operation with Multiple Entities
```csharp
public async Task<bool> CreateUserWithProfileAsync(User user, UserProfile profile)
{
    using (var context = CreateDbContext())
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();

        profile.UserId = user.UserId;
        context.UserProfiles.Add(profile);
        await context.SaveChangesAsync();

        return true;
    }
}
```

## 🎨 Architecture

```
┌─────────────────────────────────────────┐
│         BaseRepository (Abstract)        │
│  ┌────────────────────────────────────┐ │
│  │ + Configuration: IConfiguration    │ │
│  │ - _connectionString: string        │ │
│  │                                    │ │
│  │ # CreateDbContext(): DbContext     │ │
│  └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
                    ▲
                    │ Inherits
                    │
    ┌───────────────┴───────────────┐
    │                               │
┌───┴────────────────┐  ┌──────────┴──────────────┐
│ AccountRepository  │  │ UserProfileRepository   │
│                    │  │                         │
│ + RegisterAsync()  │  │ + GetProfileAsync()     │
│ + LoginAsync()     │  │ + UpdateProfileAsync()  │
│ + GetUserAsync()   │  │ + DeleteProfileAsync()  │
└────────────────────┘  └─────────────────────────┘
```

## 🔐 Best Practices

### 1. **Always Use `using` Statements**
```csharp
using (var context = CreateDbContext())
{
    // Operations
} // Auto-disposed
```

### 2. **Keep DbContext Lifetime Short**
```csharp
// ✅ Good - Short lifetime
public async Task<User?> GetUser(long id)
{
    using (var context = CreateDbContext())
    {
        return await context.Users.FindAsync(id);
    }
}

// ❌ Bad - Don't store DbContext as field
private ApplicationDbContext _context; // Don't do this!
```

### 3. **Access Configuration Through Protected Property**
```csharp
// ✅ Good
var settings = Configuration.GetSection("MySettings");

// ❌ Bad - Don't create new IConfiguration
// var config = new ConfigurationBuilder()... // Don't do this!
```

### 4. **Override Only When Necessary**
```csharp
// Only override if you need different behavior
protected override ApplicationDbContext CreateDbContext()
{
    var context = base.CreateDbContext();
    // Custom configuration
    return context;
}
```

## 📦 Files Structure

```
ResumeInOneMinute.Repository/
├── Repositories/
│   ├── BaseRepository.cs           ✅ NEW - Base class
│   ├── AccountRepository.cs        ✅ UPDATED - Inherits from BaseRepository
│   └── [Future repositories...]    ✅ Will inherit from BaseRepository
└── DataContexts/
    └── ApplicationDbContext.cs
```

## ✨ Summary

### What Changed
1. ✅ Created `BaseRepository` abstract class
2. ✅ Moved `CreateDbContext()` to `BaseRepository`
3. ✅ Moved connection string management to `BaseRepository`
4. ✅ Made `Configuration` available to all repositories
5. ✅ Updated `AccountRepository` to inherit from `BaseRepository`

### Benefits Achieved
- ✅ **DRY Principle** - No code duplication
- ✅ **Single Source of Truth** - DbContext creation in one place
- ✅ **Easy Maintenance** - Update once, affects all repositories
- ✅ **Consistent Behavior** - All repositories use same pattern
- ✅ **Scalable** - New repositories automatically get functionality

### How to Create New Repository
```csharp
public class YourRepository : BaseRepository, IYourRepository
{
    public YourRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<YourEntity> YourMethod()
    {
        using (var context = CreateDbContext())
        {
            // Your database operations
        }
    }
}
```

**That's it!** No need to duplicate `CreateDbContext()` or connection string management. 🎉
