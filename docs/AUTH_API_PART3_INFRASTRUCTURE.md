# Hướng Dẫn Xây Dựng API Login & Register - Phần 3: Infrastructure Layer

## 5. Bước 3: Infrastructure Layer

Infrastructure Layer chứa implementation của các interfaces được định nghĩa trong Domain/Application layers.

### 5.1 Password Hasher

```csharp
// File: src/BookStation.Infrastructure/Services/PasswordHasher.cs

using BookStation.Application.Users.Commands;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace BookStation.Infrastructure.Services;

/// <summary>
/// Password hashing service sử dụng PBKDF2.
/// 
/// PBKDF2 (Password-Based Key Derivation Function 2):
/// - Chậm có chủ đích để chống brute-force
/// - Sử dụng salt để chống rainbow table
/// - Được NIST và OWASP khuyến nghị
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    // ===== CONSTANTS =====
    private const int SaltSize = 128 / 8;       // 16 bytes
    private const int HashSize = 256 / 8;       // 32 bytes
    private const int Iterations = 100000;      // Số vòng lặp (càng cao càng an toàn nhưng chậm)

    /// <summary>
    /// Hash một password.
    /// 
    /// Output format: [salt (16 bytes)][hash (32 bytes)] -> Base64
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        // ===== STEP 1: Generate random salt =====
        // Salt khác nhau cho mỗi password, chống rainbow table
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // ===== STEP 2: Hash password với salt =====
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,  // Pseudo-random function
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // ===== STEP 3: Combine salt + hash =====
        // Lưu cả salt để có thể verify sau này
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        // ===== STEP 4: Convert to Base64 =====
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Xác minh password với hash đã lưu.
    /// </summary>
    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        if (string.IsNullOrEmpty(passwordHash))
            throw new ArgumentNullException(nameof(passwordHash));

        // ===== STEP 1: Decode Base64 =====
        byte[] hashBytes = Convert.FromBase64String(passwordHash);

        // ===== STEP 2: Extract salt (first 16 bytes) =====
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        // ===== STEP 3: Hash input password với same salt =====
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // ===== STEP 4: Compare hashes byte-by-byte =====
        // Constant-time comparison để chống timing attack
        for (int i = 0; i < HashSize; i++)
        {
            if (hashBytes[i + SaltSize] != hash[i])
                return false;
        }

        return true;
    }
}
```

**Security Notes:**
- **Salt**: Random, unique cho mỗi password
- **Iterations**: 100,000 vòng - balance giữa security và performance
- **HMACSHA256**: Secure hash function
- **Constant-time comparison**: Chống timing attack

### 5.2 JWT Token Generator

```csharp
// File: src/BookStation.Infrastructure/Authentication/JwtSettings.cs

namespace BookStation.Infrastructure.Authentication;

/// <summary>
/// Cấu hình JWT từ appsettings.json.
/// Sử dụng Options pattern.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Secret key để ký token (ít nhất 64 bytes cho HS256).
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Issuer - ai phát hành token (thường là tên app).
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Audience - token được phát hành cho ai.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian hết hạn (phút).
    /// </summary>
    public int ExpirationInMinutes { get; set; } = 60;
}
```

```csharp
// File: src/BookStation.Infrastructure/Authentication/JwtTokenGenerator.cs

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookStation.Infrastructure.Authentication;

/// <summary>
/// Interface để Application layer không phụ thuộc vào Infrastructure.
/// </summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(
        long userId, 
        string email, 
        IEnumerable<string> roles, 
        IEnumerable<string>? permissions = null);
}

/// <summary>
/// Service generate JWT token.
/// 
/// JWT Structure:
/// - Header: { "alg": "HS256", "typ": "JWT" }
/// - Payload: { claims... }
/// - Signature: HMACSHA256(base64(header) + "." + base64(payload), secret)
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(
        long userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions = null)
    {
        // ===== STEP 1: Tạo claims =====
        var claims = new List<Claim>
        {
            // Standard claims
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),  // Subject
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  // JWT ID
            
            // Custom claims
            new(ClaimTypes.NameIdentifier, userId.ToString()),
        };

        // ===== STEP 2: Thêm role claims =====
        // Mỗi role là một claim riêng
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // ===== STEP 3: Thêm permission claims (RBAC) =====
        if (permissions != null)
        {
            claims.AddRange(permissions.Select(permission => 
                new Claim("permission", permission)));
        }

        // ===== STEP 4: Tạo signing credentials =====
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(
            key, 
            SecurityAlgorithms.HmacSha256);

        // ===== STEP 5: Tạo token =====
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        // ===== STEP 6: Serialize thành string =====
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**JWT Token Example:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiIxIiwiZW1haWwiOiJ1c2VyQGV4YW1wbGUuY29tIiwianRpIjoiZ3VpZCIsInJvbGUiOlsiVXNlciJdLCJwZXJtaXNzaW9uIjpbImJvb2tzLnZpZXciXSwiZXhwIjoxNjQwMDAwMDAwfQ.
SIGNATURE
```

### 5.3 Repository Implementations

```csharp
// File: src/BookStation.Infrastructure/Repositories/UserRepository.cs

using BookStation.Domain.Entities.UserAggregate;
using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Infrastructure.Repositories;

/// <summary>
/// Implementation của IUserRepository.
/// Sử dụng Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly WriteDbContext _context;

    public UserRepository(WriteDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy user theo ID.
    /// </summary>
    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// Lấy user theo email.
    /// Dùng cho Login.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    /// <summary>
    /// Kiểm tra email đã tồn tại chưa.
    /// Dùng cho Register.
    /// </summary>
    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        return !await _context.Users
            .AnyAsync(u => u.Email.Value == email, cancellationToken);
    }

    /// <summary>
    /// Lấy user kèm theo roles.
    /// Dùng cho Login để lấy JWT claims.
    /// </summary>
    public async Task<User?> GetWithRolesAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// Lấy user kèm theo seller profile.
    /// </summary>
    public async Task<User?> GetWithSellerProfileAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// Thêm user mới.
    /// Chỉ track, chưa lưu vào DB.
    /// </summary>
    public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Cập nhật user.
    /// </summary>
    public void Update(User entity)
    {
        _context.Users.Update(entity);
    }

    /// <summary>
    /// Xóa user.
    /// </summary>
    public void Delete(User entity)
    {
        _context.Users.Remove(entity);
    }
}
```

```csharp
// File: src/BookStation.Infrastructure/Repositories/RoleRepository.cs

using BookStation.Domain.Entities.UserAggregate;
using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Infrastructure.Repositories;

/// <summary>
/// Implementation của IRoleRepository.
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly WriteDbContext _context;

    public RoleRepository(WriteDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <summary>
    /// Lấy role theo tên.
    /// Dùng khi assign default role cho user mới.
    /// </summary>
    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    /// <summary>
    /// Lấy role kèm permissions.
    /// Dùng cho Login để build JWT claims.
    /// </summary>
    public async Task<Role?> GetWithPermissionsAsync(long roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }

    public async Task<List<Role>> GetAllActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AnyAsync(r => r.Name == name, cancellationToken);
    }

    public async Task AddAsync(Role entity, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(entity, cancellationToken);
    }

    public void Update(Role entity)
    {
        _context.Roles.Update(entity);
    }

    public void Delete(Role entity)
    {
        _context.Roles.Remove(entity);
    }
}
```

### 5.4 EF Core DbContext

```csharp
// File: src/BookStation.Infrastructure/Persistence/WriteDbContext.cs

using BookStation.Domain.Entities.UserAggregate;
using BookStation.Core.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Infrastructure.Persistence;

/// <summary>
/// DbContext chính cho write operations.
/// Implement IUnitOfWork để coordinate transactions.
/// </summary>
public class WriteDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    public WriteDbContext(DbContextOptions<WriteDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    // ===== DbSets =====
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    /// <summary>
    /// Cấu hình entity mappings.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply tất cả configurations từ assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WriteDbContext).Assembly);
    }

    /// <summary>
    /// Override SaveChangesAsync để publish domain events.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Publish domain events TRƯỚC khi save
        await PublishDomainEventsAsync(cancellationToken);

        // Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Publish tất cả domain events từ aggregate roots.
    /// </summary>
    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        // Lấy tất cả aggregate roots có domain events
        var aggregateRoots = ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // Gom tất cả domain events
        var domainEvents = aggregateRoots
            .SelectMany(ar => ar.DomainEvents)
            .ToList();

        // Clear events từ aggregate roots
        aggregateRoots.ForEach(ar => ar.ClearDomainEvents());

        // Publish từng event
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
```

### 5.5 EF Core Configurations

```csharp
// File: src/BookStation.Infrastructure/Persistence/Configurations/UserConfiguration.cs

using BookStation.Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStation.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration cho User entity.
/// Sử dụng Fluent API thay vì Data Annotations.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ===== TABLE =====
        builder.ToTable("Users");

        // ===== PRIMARY KEY =====
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();  // Auto-increment

        // ===== EMAIL (Value Object) =====
        // OwnsOne: Value Object được map vào cùng table
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")  // Column name trong DB
                .HasMaxLength(255)
                .IsRequired();

            // Unique index cho email
            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });

        // ===== PASSWORD =====
        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        // ===== FULL NAME =====
        builder.Property(u => u.FullName)
            .HasMaxLength(255);

        // ===== PHONE (Value Object) =====
        builder.OwnsOne(u => u.Phone, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("Phone")
                .HasMaxLength(20);
        });

        // ===== STATUS =====
        builder.Property(u => u.IsVerified)
            .HasDefaultValue(false);

        builder.Property(u => u.Status)
            .HasConversion<string>()  // Enum -> String
            .HasMaxLength(50);

        // ===== TIMESTAMPS =====
        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // ===== RELATIONSHIPS =====
        // User -> UserRoles (1-N)
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== IGNORE =====
        // Domain events không lưu vào DB
        builder.Ignore(u => u.DomainEvents);
    }
}
```

```csharp
// File: src/BookStation.Infrastructure/Persistence/Configurations/RoleConfiguration.cs

using BookStation.Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStation.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration cho Role entity.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Roles_Name");

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.IsSystemRole)
            .HasDefaultValue(false);

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        // Role -> UserRoles (1-N)
        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Role -> RolePermissions (1-N)
        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Configuration cho Permission entity.
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("IX_Permissions_Name");

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Category)
            .HasMaxLength(50)
            .IsRequired();
    }
}

/// <summary>
/// Configuration cho RolePermission junction table.
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        // Composite primary key
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 5.6 DI Configuration cho Infrastructure Layer

```csharp
// File: src/BookStation.Infrastructure/ConfigureServices.cs

using BookStation.Application.Users.Commands;
using BookStation.Core.SharedKernel;
using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Authentication;
using BookStation.Infrastructure.Persistence;
using BookStation.Infrastructure.Repositories;
using BookStation.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookStation.Infrastructure;

/// <summary>
/// Extension method để register tất cả services của Infrastructure layer.
/// </summary>
public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ===== DATABASE =====
        services.AddDbContext<WriteDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(WriteDbContext).Assembly.FullName)));

        // Register DbContext as IUnitOfWork
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<WriteDbContext>());

        // ===== REPOSITORIES =====
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        // ===== SERVICES =====
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // ===== JWT =====
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
```

---

## Database Schema

```sql
-- Users table
CREATE TABLE Users (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    FullName NVARCHAR(255) NULL,
    Phone NVARCHAR(20) NULL,
    IsVerified BIT NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT IX_Users_Email UNIQUE (Email)
);

-- Roles table
CREATE TABLE Roles (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsSystemRole BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT IX_Roles_Name UNIQUE (Name)
);

-- Permissions table
CREATE TABLE Permissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Category NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT IX_Permissions_Name UNIQUE (Name)
);

-- UserRoles junction table
CREATE TABLE UserRoles (
    UserId BIGINT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);

-- RolePermissions junction table
CREATE TABLE RolePermissions (
    RoleId BIGINT NOT NULL,
    PermissionId INT NOT NULL,
    
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE
);
```

---

*Tiếp tục ở phần 4: API Layer & Configuration...*
