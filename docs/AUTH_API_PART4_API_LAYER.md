# Hướng Dẫn Xây Dựng API Login & Register - Phần 4: API Layer & Configuration

## 6. Bước 4: API Layer

API Layer là entry point cho HTTP requests, chịu trách nhiệm routing, authentication, và response formatting.

### 6.1 Auth Controller

```csharp
// File: src/BookStation.PublicApi/Controllers/AuthController.cs

using BookStation.Application.Users.Commands;
using BookStation.PublicApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStation.PublicApi.Controllers;

/// <summary>
/// Controller xử lý authentication: Register, Login, Profile.
/// 
/// Route: /api/auth
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor - MediatR được inject qua DI.
    /// </summary>
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Đăng ký người dùng mới.
    /// 
    /// POST /api/auth/register
    /// Body: { "email": "user@example.com", "password": "Password123", "fullName": "John Doe", "phone": "+84123456789" }
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]  // Không cần authentication
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        try
        {
            // Gửi command qua MediatR
            var result = await _mediator.Send(command);
            
            // Trả về 201 Created với location header
            return CreatedAtAction(
                actionName: nameof(GetProfile), 
                routeValues: new { }, 
                value: result);
        }
        catch (InvalidOperationException ex)
        {
            // Email đã tồn tại hoặc lỗi business logic khác
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Đăng nhập và nhận JWT token.
    /// 
    /// POST /api/auth/login
    /// Body: { "email": "user@example.com", "password": "Password123" }
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy profile của user hiện tại.
    /// Yêu cầu JWT token trong header Authorization.
    /// 
    /// GET /api/auth/profile
    /// Header: Authorization: Bearer {token}
    /// </summary>
    [HttpGet("profile")]
    [Authorize]  // Yêu cầu authentication
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetProfile()
    {
        // Đọc thông tin từ JWT claims
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
        var permissions = User.FindAll("permission")
            .Select(c => c.Value)
            .ToList();

        return Ok(new
        {
            userId,
            email,
            roles,
            permissions
        });
    }

    /// <summary>
    /// Endpoint chỉ cho admin (example).
    /// Yêu cầu permission "orders.viewall".
    /// 
    /// GET /api/auth/admin-only
    /// </summary>
    [HttpGet("admin-only")]
    [RequirePermission("orders.viewall")]  // Custom attribute
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "You have admin permissions!" });
    }
}
```

### 6.2 Permission-Based Authorization

```csharp
// File: src/BookStation.PublicApi/Authorization/PermissionAuthorization.cs

using Microsoft.AspNetCore.Authorization;

namespace BookStation.PublicApi.Authorization;

/// <summary>
/// Custom attribute để yêu cầu permission.
/// Usage: [RequirePermission("books.create")]
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        // Map đến policy có cùng tên
        Policy = permission;
    }
}

/// <summary>
/// Authorization requirement cho permission.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Handler kiểm tra user có permission hay không.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Tìm permission claim trong JWT
        var hasPermission = context.User.Claims
            .Any(c => c.Type == "permission" && c.Value == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        // Nếu không có permission, requirement fail (403 Forbidden)

        return Task.CompletedTask;
    }
}
```

### 6.3 Configuration (appsettings.json)

```json
// File: src/BookStation.PublicApi/appsettings.json

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BookStationDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast64BytesLongForHS256Algorithm123456789!",
    "Issuer": "BookStation",
    "Audience": "BookStationUsers",
    "ExpirationInMinutes": 60
  }
}
```

**Quan trọng về JWT Secret:**
- Ít nhất 64 bytes cho HS256
- Không được commit vào source control
- Sử dụng User Secrets hoặc Environment Variables trong production

### 6.4 Program.cs (Entry Point)

```csharp
// File: src/BookStation.PublicApi/Program.cs

using BookStation.Application;
using BookStation.Infrastructure;
using BookStation.Infrastructure.Authentication;
using BookStation.Infrastructure.Data;
using BookStation.PublicApi.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. ADD SERVICES =====

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===== 2. CONFIGURE JWT =====

// Bind JwtSettings từ appsettings.json
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings not configured");

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // true trong production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validate issuer
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        
        // Validate audience
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        
        // Validate lifetime
        ValidateLifetime = true,
        
        // Validate signing key
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        
        // Không cho phép clock skew (token hết hạn là hết hạn)
        ClockSkew = TimeSpan.Zero
    };
});

// ===== 3. CONFIGURE AUTHORIZATION =====

builder.Services.AddAuthorization(options =>
{
    // Đăng ký policies cho permissions
    options.AddPolicy("books.create", policy => 
        policy.Requirements.Add(new PermissionRequirement("books.create")));
    options.AddPolicy("books.update", policy => 
        policy.Requirements.Add(new PermissionRequirement("books.update")));
    options.AddPolicy("books.delete", policy => 
        policy.Requirements.Add(new PermissionRequirement("books.delete")));
    options.AddPolicy("orders.viewall", policy => 
        policy.Requirements.Add(new PermissionRequirement("orders.viewall")));
    // Thêm policies khác khi cần...
});

// Register authorization handler
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// ===== 4. CONFIGURE SWAGGER =====

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BookStation API",
        Version = "v1",
        Description = "BookStation E-Commerce API with JWT Authentication"
    });

    // Thêm JWT authentication vào Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===== 5. ADD APPLICATION & INFRASTRUCTURE SERVICES =====

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ===== 6. CONFIGURE CORS =====

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===== BUILD APP =====

var app = builder.Build();

// ===== 7. SEED DATA =====

// Seed Roles & Permissions on startup
await app.SeedRbacDataAsync();

// ===== 8. CONFIGURE PIPELINE =====

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStation API V1");
        c.RoutePrefix = string.Empty; // Swagger ở root URL
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Authentication PHẢI trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

### 6.5 RBAC Data Seeder

```csharp
// File: src/BookStation.Infrastructure/Data/RbacDataSeeder.cs

using BookStation.Domain.Entities.UserAggregate;
using BookStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookStation.Infrastructure.Data;

/// <summary>
/// Seed data ban đầu cho RBAC (Roles và Permissions).
/// Chạy một lần khi application startup.
/// </summary>
public class RbacDataSeeder
{
    private readonly WriteDbContext _context;

    public RbacDataSeeder(WriteDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Seed theo thứ tự: Permissions -> Roles -> Assign
        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await AssignPermissionsToRolesAsync();

        await _context.SaveChangesAsync();
    }

    private async Task SeedPermissionsAsync()
    {
        if (await _context.Permissions.AnyAsync())
            return; // Đã seed rồi

        var permissions = new List<Permission>
        {
            // Books
            Permission.Create("books.view", "View books", "Books"),
            Permission.Create("books.create", "Create books", "Books"),
            Permission.Create("books.update", "Update books", "Books"),
            Permission.Create("books.delete", "Delete books", "Books"),

            // Orders
            Permission.Create("orders.view", "View own orders", "Orders"),
            Permission.Create("orders.create", "Create orders", "Orders"),
            Permission.Create("orders.update", "Update own orders", "Orders"),
            Permission.Create("orders.cancel", "Cancel own orders", "Orders"),
            Permission.Create("orders.viewall", "View all orders", "Orders"),
            Permission.Create("orders.manage", "Manage all orders", "Orders"),

            // Users
            Permission.Create("users.view", "View users", "Users"),
            Permission.Create("users.create", "Create users", "Users"),
            Permission.Create("users.update", "Update users", "Users"),
            Permission.Create("users.delete", "Delete users", "Users"),
        };

        await _context.Permissions.AddRangeAsync(permissions);
    }

    private async Task SeedRolesAsync()
    {
        if (await _context.Roles.AnyAsync())
            return;

        var roles = new List<Role>
        {
            Role.Create(Role.SystemRoles.Admin, "Administrator with full access", isSystemRole: true),
            Role.Create(Role.SystemRoles.User, "Regular user", isSystemRole: true),
            Role.Create(Role.SystemRoles.Seller, "Seller who can manage books", isSystemRole: true),
        };

        await _context.Roles.AddRangeAsync(roles);
    }

    private async Task AssignPermissionsToRolesAsync()
    {
        var roles = await _context.Roles.Include(r => r.RolePermissions).ToListAsync();
        var permissions = await _context.Permissions.ToListAsync();

        var admin = roles.First(r => r.Name == Role.SystemRoles.Admin);
        var user = roles.First(r => r.Name == Role.SystemRoles.User);
        var seller = roles.First(r => r.Name == Role.SystemRoles.Seller);

        // Admin: TẤT CẢ permissions
        foreach (var permission in permissions)
        {
            admin.AddPermission(permission.Id);
        }

        // User: Permissions cơ bản
        user.AddPermission(GetPermissionId(permissions, "books.view"));
        user.AddPermission(GetPermissionId(permissions, "orders.view"));
        user.AddPermission(GetPermissionId(permissions, "orders.create"));
        user.AddPermission(GetPermissionId(permissions, "orders.cancel"));

        // Seller: Books + Orders
        seller.AddPermission(GetPermissionId(permissions, "books.view"));
        seller.AddPermission(GetPermissionId(permissions, "books.create"));
        seller.AddPermission(GetPermissionId(permissions, "books.update"));
        seller.AddPermission(GetPermissionId(permissions, "orders.viewall"));
        seller.AddPermission(GetPermissionId(permissions, "orders.manage"));
    }

    private static int GetPermissionId(List<Permission> permissions, string permissionName)
    {
        return permissions.First(p => p.Name == permissionName).Id;
    }
}

/// <summary>
/// Extension để chạy seeder từ Program.cs.
/// </summary>
public static class RbacDataSeederExtensions
{
    public static async Task SeedRbacDataAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        var seeder = new RbacDataSeeder(context);
        await seeder.SeedAsync();
    }
}
```

---

## 7. Bước 5: Testing API

### 7.1 Chạy Application

```bash
# Từ thư mục src/BookStation.PublicApi
dotnet run
```

Application sẽ chạy tại:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001 (root)

### 7.2 Test với Swagger UI

1. Mở browser, vào https://localhost:5001
2. Tìm endpoint `/api/auth/register`
3. Click "Try it out"
4. Nhập body:
```json
{
  "email": "test@example.com",
  "password": "Password123",
  "fullName": "Test User",
  "phone": "+84123456789"
}
```
5. Click "Execute"

### 7.3 Test với cURL

**Register:**
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123",
    "fullName": "Test User"
  }'
```

**Login:**
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123"
  }'
```

**Get Profile (với token):**
```bash
curl -X GET https://localhost:5001/api/auth/profile \
  -H "Authorization: Bearer {YOUR_JWT_TOKEN}"
```

### 7.4 Test với .http file (Visual Studio / VS Code)

```http
# File: src/BookStation.PublicApi/BookStation.PublicApi.http

@baseUrl = https://localhost:5001
@token = YOUR_TOKEN_HERE

### Register
POST {{baseUrl}}/api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Password123",
  "fullName": "Test User",
  "phone": "+84123456789"
}

### Login
POST {{baseUrl}}/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Password123"
}

### Get Profile
GET {{baseUrl}}/api/auth/profile
Authorization: Bearer {{token}}

### Admin Only (requires orders.viewall permission)
GET {{baseUrl}}/api/auth/admin-only
Authorization: Bearer {{token}}
```

---

*Tiếp tục ở phần 5: Summary & Best Practices...*
