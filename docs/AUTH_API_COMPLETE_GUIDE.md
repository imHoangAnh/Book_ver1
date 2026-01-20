# Hướng Dẫn Xây Dựng API Login & Register - BookStation

## Mục Lục

1. [Tổng Quan Kiến Trúc](#1-tổng-quan-kiến-trúc)
2. [Chuẩn Bị Môi Trường](#2-chuẩn-bị-môi-trường)
3. [Bước 1: Domain Layer](#3-bước-1-domain-layer)
4. [Bước 2: Application Layer](#4-bước-2-application-layer)
5. [Bước 3: Infrastructure Layer](#5-bước-3-infrastructure-layer)
6. [Bước 4: API Layer](#6-bước-4-api-layer)
7. [Bước 5: Configuration & DI](#7-bước-5-configuration--di)
8. [Testing API](#8-testing-api)
9. [Flow Diagram](#9-flow-diagram)

---

## 1. Tổng Quan Kiến Trúc

### 1.1 Clean Architecture + CQRS + DDD

BookStation sử dụng kiến trúc phân tầng:

```
┌─────────────────────────────────────────────────────────────┐
│                     PublicApi Layer                         │
│              (Controllers, Authorization)                   │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                        │
│         (Commands, Handlers, Validators, Behaviors)         │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                           │
│      (Entities, Value Objects, Events, Repositories)        │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                      │
│    (EF Core, JWT, Password Hasher, Repository Impl)         │
├─────────────────────────────────────────────────────────────┤
│                       Core Layer                            │
│           (Shared Kernel: Entity, AggregateRoot...)         │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 Luồng Request Login/Register

```
HTTP Request
     │
     ▼
┌─────────────┐
│ Controller  │  ← Nhận request, gọi MediatR
└─────────────┘
     │
     ▼
┌─────────────┐
│  MediatR    │  ← Dispatch command đến handler
└─────────────┘
     │
     ▼
┌─────────────────────┐
│ Pipeline Behaviors  │  ← Validation, Logging, Transaction
└─────────────────────┘
     │
     ▼
┌─────────────────────┐
│  Command Handler    │  ← Business logic chính
└─────────────────────┘
     │
     ▼
┌─────────────────────┐
│    Repository       │  ← Data access
└─────────────────────┘
     │
     ▼
┌─────────────────────┐
│   Database (EF)     │
└─────────────────────┘
```

---

## 2. Chuẩn Bị Môi Trường

### 2.1 NuGet Packages Cần Thiết

**BookStation.Core:**
```xml
<PackageReference Include="MediatR.Contracts" Version="2.0.1" />
```

**BookStation.Domain:**
```xml
<!-- Không có dependency ngoài Core -->
```

**BookStation.Application:**
```xml
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
```

**BookStation.Infrastructure:**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0-preview.1.25081.2" />
<PackageReference Include="Microsoft.AspNetCore.Cryptography.KeyDerivation" Version="10.0.0-preview.1.25120.3" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.3.1" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.3.1" />
```

**BookStation.PublicApi:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0-preview.1.25120.3" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
```

---

## 3. Bước 1: Domain Layer

Domain Layer chứa business logic thuần túy, không phụ thuộc vào framework.

### 3.1 Value Objects

Value Objects là các đối tượng được định nghĩa bởi giá trị, không phải identity.

#### Email.cs

```csharp
// File: src/BookStation.Domain/ValueObjects/Email.cs

using System.Text.RegularExpressions;
using BookStation.Core.SharedKernel;

namespace BookStation.Domain.ValueObjects;

/// <summary>
/// Value object đại diện cho địa chỉ email.
/// Đảm bảo email luôn hợp lệ khi được tạo.
/// </summary>
public sealed partial class Email : ValueObject
{
    private const int MaxLength = 255;

    /// <summary>
    /// Giá trị email đã được chuẩn hóa (lowercase, trim).
    /// </summary>
    public string Value { get; }

    // Private constructor - buộc phải dùng factory method
    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method tạo Email value object.
    /// </summary>
    /// <exception cref="ArgumentException">Khi email không hợp lệ.</exception>
    public static Email Create(string email)
    {
        // Validation 1: Không được rỗng
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        // Validation 2: Không vượt quá độ dài tối đa
        if (email.Length > MaxLength)
            throw new ArgumentException($"Email cannot exceed {MaxLength} characters.", nameof(email));

        // Validation 3: Đúng format email
        if (!EmailRegex().IsMatch(email))
            throw new ArgumentException("Email format is invalid.", nameof(email));

        // Chuẩn hóa: lowercase và trim
        return new Email(email.ToLowerInvariant().Trim());
    }

    /// <summary>
    /// Try-pattern cho trường hợp không muốn throw exception.
    /// </summary>
    public static bool TryCreate(string email, out Email? result)
    {
        try
        {
            result = Create(email);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    // ValueObject yêu cầu implement method này để so sánh
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    // Implicit conversion để tiện sử dụng
    public static implicit operator string(Email email) => email.Value;

    // Generated Regex cho performance (C# 7+)
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
```

**Giải thích:**
- **Value Object**: Đối tượng bất biến, được so sánh bằng giá trị
- **Factory Method**: `Create()` đảm bảo không bao giờ có Email không hợp lệ
- **Encapsulation**: Private constructor ngăn tạo trực tiếp

#### PhoneNumber.cs

```csharp
// File: src/BookStation.Domain/ValueObjects/PhoneNumber.cs

using System.Text.RegularExpressions;
using BookStation.Core.SharedKernel;

namespace BookStation.Domain.ValueObjects;

public sealed partial class PhoneNumber : ValueObject
{
    private const int MinLength = 9;
    private const int MaxLength = 20;

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number cannot be empty.", nameof(phone));

        // Làm sạch: bỏ space, dấu gạch
        var cleaned = phone.Replace(" ", "").Replace("-", "").Replace(".", "");

        if (cleaned.Length < MinLength || cleaned.Length > MaxLength)
            throw new ArgumentException(
                $"Phone number must be between {MinLength} and {MaxLength} digits.", 
                nameof(phone));

        if (!PhoneRegex().IsMatch(cleaned))
            throw new ArgumentException("Phone number format is invalid.", nameof(phone));

        return new PhoneNumber(cleaned);
    }

    public static bool TryCreate(string phone, out PhoneNumber? result)
    {
        try
        {
            result = Create(phone);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;

    [GeneratedRegex(@"^\+?[0-9]+$", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();
}
```

### 3.2 Enums

```csharp
// File: src/BookStation.Domain/Enums/UserEnums.cs

namespace BookStation.Domain.Enums;

/// <summary>
/// Trạng thái tài khoản người dùng.
/// </summary>
public enum EUserStatus
{
    /// <summary>Tài khoản đang hoạt động.</summary>
    Active = 0,

    /// <summary>Tài khoản không hoạt động.</summary>
    Inactive = 1,

    /// <summary>Tài khoản bị cấm.</summary>
    Banned = 2,

    /// <summary>Tài khoản bị tạm ngưng.</summary>
    Suspended = 3,

    /// <summary>Đang chờ xác minh.</summary>
    Pending = 4
}
```

### 3.3 User Entity (Aggregate Root)

```csharp
// File: src/BookStation.Domain/Entities/UserAggregate/User.cs

using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// User entity - Aggregate Root cho quản lý người dùng.
/// Aggregate Root là entity chính quản lý toàn bộ aggregate.
/// </summary>
public class User : AggregateRoot<long>
{
    // ===== PROPERTIES =====
    
    /// <summary>
    /// Email của người dùng (Value Object).
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Mật khẩu đã được hash.
    /// </summary>
    public string PasswordHash { get; private set; } = null!;

    /// <summary>
    /// Họ tên đầy đủ.
    /// </summary>
    public string? FullName { get; private set; }

    /// <summary>
    /// Số điện thoại (Value Object, nullable).
    /// </summary>
    public PhoneNumber? Phone { get; private set; }

    /// <summary>
    /// Đã xác minh email chưa.
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Trạng thái tài khoản.
    /// </summary>
    public EUserStatus Status { get; private set; }

    // ===== NAVIGATION PROPERTIES =====
    
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();

    public SellerProfile? SellerProfile { get; private set; }
    public ShipperProfile? ShipperProfile { get; private set; }

    // ===== CONSTRUCTORS =====
    
    /// <summary>
    /// Private constructor cho EF Core.
    /// EF Core cần constructor không tham số để hydrate entity từ DB.
    /// </summary>
    private User() { }

    // ===== FACTORY METHOD =====
    
    /// <summary>
    /// Factory method tạo User mới.
    /// Sử dụng factory method thay vì public constructor để:
    /// 1. Đảm bảo invariants
    /// 2. Có thể raise domain event
    /// 3. Kiểm soát cách tạo object
    /// </summary>
    public static User Create(
        Email email,
        string passwordHash,
        string? fullName = null,
        PhoneNumber? phone = null)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            Phone = phone,
            IsVerified = false,           // Mặc định chưa xác minh
            Status = EUserStatus.Pending  // Mặc định đang chờ
        };

        // Raise domain event để các subscriber khác xử lý
        // (ví dụ: gửi email chào mừng)
        user.AddDomainEvent(new UserCreatedEvent(user));

        return user;
    }

    // ===== BEHAVIOR METHODS =====
    
    /// <summary>
    /// Cập nhật thông tin profile.
    /// </summary>
    public void UpdateProfile(string? fullName, PhoneNumber? phone)
    {
        FullName = fullName;
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserUpdatedEvent(Id, nameof(UpdateProfile)));
    }

    /// <summary>
    /// Đổi mật khẩu.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserPasswordChangedEvent(Id));
    }

    /// <summary>
    /// Xác minh email.
    /// </summary>
    public void Verify()
    {
        if (IsVerified)
            return;

        IsVerified = true;
        Status = EUserStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserVerifiedEvent(Id));
    }

    /// <summary>
    /// Gán role cho user.
    /// </summary>
    public void AssignRole(Role role)
    {
        // Kiểm tra đã có role này chưa
        if (_userRoles.Any(ur => ur.RoleId == role.Id))
            return;

        _userRoles.Add(UserRole.Create(Id, role.Id));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Xóa role khỏi user.
    /// </summary>
    public void RemoveRole(int roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Kiểm tra user có role nào đó không.
    /// </summary>
    public bool HasRole(string roleName)
    {
        return _userRoles.Any(ur => 
            ur.Role?.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Vô hiệu hóa tài khoản.
    /// </summary>
    public void Deactivate()
    {
        if (Status == EUserStatus.Inactive)
            return;

        Status = EUserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserDeactivatedEvent(Id));
    }

    /// <summary>
    /// Kích hoạt tài khoản.
    /// </summary>
    public void Activate()
    {
        if (Status == EUserStatus.Active)
            return;

        Status = EUserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cấm tài khoản.
    /// </summary>
    public void Ban(string reason)
    {
        Status = EUserStatus.Banned;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserBannedEvent(Id, reason));
    }
}
```

**Giải thích chi tiết:**

1. **Aggregate Root**: User là aggregate root, quản lý toàn bộ User aggregate (bao gồm UserRole)
2. **Private Setter**: Tất cả property đều có private set để đảm bảo encapsulation
3. **Factory Method**: `Create()` là cách duy nhất tạo User mới
4. **Domain Events**: Mỗi thay đổi quan trọng đều raise event
5. **Behavior Methods**: Logic business nằm trong entity, không phải service

### 3.4 Domain Events

```csharp
// File: src/BookStation.Domain/Entities/UserAggregate/UserEvents.cs

using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Base event cho các sự kiện liên quan đến User.
/// </summary>
public abstract class UserBaseEvent : DomainEvent
{
    public long UserId { get; }

    protected UserBaseEvent(long userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Event khi User mới được tạo.
/// Subscriber có thể: gửi email welcome, tạo profile mặc định, etc.
/// </summary>
public sealed class UserCreatedEvent : UserBaseEvent
{
    public string Email { get; }

    public UserCreatedEvent(User user) : base(user.Id)
    {
        Email = user.Email.Value;
    }
}

/// <summary>
/// Event khi User được cập nhật.
/// </summary>
public sealed class UserUpdatedEvent : UserBaseEvent
{
    public string UpdateType { get; }

    public UserUpdatedEvent(long userId, string updateType) : base(userId)
    {
        UpdateType = updateType;
    }
}

/// <summary>
/// Event khi mật khẩu được thay đổi.
/// Subscriber có thể: gửi email thông báo, invalidate sessions, etc.
/// </summary>
public sealed class UserPasswordChangedEvent : UserBaseEvent
{
    public UserPasswordChangedEvent(long userId) : base(userId)
    {
    }
}

/// <summary>
/// Event khi User được xác minh.
/// </summary>
public sealed class UserVerifiedEvent : UserBaseEvent
{
    public UserVerifiedEvent(long userId) : base(userId)
    {
    }
}

/// <summary>
/// Event khi User bị vô hiệu hóa.
/// </summary>
public sealed class UserDeactivatedEvent : UserBaseEvent
{
    public UserDeactivatedEvent(long userId) : base(userId)
    {
    }
}

/// <summary>
/// Event khi User bị cấm.
/// </summary>
public sealed class UserBannedEvent : UserBaseEvent
{
    public string Reason { get; }

    public UserBannedEvent(long userId, string reason) : base(userId)
    {
        Reason = reason;
    }
}
```

### 3.5 RBAC Entities (Role, Permission)

```csharp
// File: src/BookStation.Domain/Entities/UserAggregate/Role.cs

using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Role entity cho RBAC (Role-Based Access Control).
/// </summary>
public class Role : Entity<long>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyList<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() { }

    public static Role Create(string name, string? description = null, bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        return new Role
        {
            Name = name,
            Description = description,
            IsSystemRole = isSystemRole,
            IsActive = true
        };
    }

    public void AddPermission(int permissionId)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permissionId))
            return;

        _rolePermissions.Add(RolePermission.Create(Id, permissionId));
    }

    public void RemovePermission(int permissionId)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
        }
    }

    /// <summary>
    /// Các role hệ thống được định nghĩa sẵn.
    /// </summary>
    public static class SystemRoles
    {
        public const string Admin = "Admin";
        public const string Seller = "Seller";
        public const string User = "User";
        public const string Shipper = "Shipper";
        public const string Warehouse = "Warehouse";
    }
}
```

```csharp
// File: src/BookStation.Domain/Entities/UserAggregate/Permission.cs

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Permission entity cho phân quyền chi tiết.
/// </summary>
public class Permission : Entity<int>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Category { get; private set; } = null!;

    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyList<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Permission() { }

    public static Permission Create(string name, string description, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty.", nameof(name));

        return new Permission
        {
            Name = name.ToLowerInvariant().Trim(),
            Description = description.Trim(),
            Category = category.Trim()
        };
    }

    /// <summary>
    /// Các permission được định nghĩa sẵn.
    /// </summary>
    public static class Permissions
    {
        // Books
        public const string BooksView = "books.view";
        public const string BooksCreate = "books.create";
        public const string BooksUpdate = "books.update";
        public const string BooksDelete = "books.delete";

        // Orders
        public const string OrdersView = "orders.view";
        public const string OrdersCreate = "orders.create";
        public const string OrdersViewAll = "orders.viewall";

        // Users
        public const string UsersView = "users.view";
        public const string UsersCreate = "users.create";
        public const string UsersUpdate = "users.update";
        public const string UsersDelete = "users.delete";
    }
}
```

```csharp
// File: src/BookStation.Domain/Entities/UserAggregate/UserRole.cs

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Junction entity cho quan hệ nhiều-nhiều User-Role.
/// </summary>
public class UserRole
{
    public long UserId { get; private set; }
    public int RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public Role? Role { get; private set; }

    private UserRole() { }

    internal static UserRole Create(long userId, int roleId)
    {
        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        };
    }
}
```

```csharp
// File: src/BookStation.Domain/Entities/UserAggregate/RolePermission.cs
// (Nằm trong Permission.cs)

/// <summary>
/// Junction entity cho quan hệ nhiều-nhiều Role-Permission.
/// </summary>
public class RolePermission
{
    public long RoleId { get; private set; }
    public int PermissionId { get; private set; }

    public Role? Role { get; private set; }
    public Permission? Permission { get; private set; }

    private RolePermission() { }

    public static RolePermission Create(long roleId, int permissionId)
    {
        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
    }
}
```

### 3.6 Repository Interfaces

```csharp
// File: src/BookStation.Domain/Repositories/IUserRepository.cs

using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.UserAggregate;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface cho User aggregate.
/// Định nghĩa trong Domain layer, implement trong Infrastructure.
/// </summary>
public interface IUserRepository : IWriteOnlyRepository<User, long>
{
    /// <summary>
    /// Lấy user theo email.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra email đã tồn tại chưa.
    /// </summary>
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy user kèm theo roles.
    /// </summary>
    Task<User?> GetWithRolesAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy user kèm theo seller profile.
    /// </summary>
    Task<User?> GetWithSellerProfileAsync(long id, CancellationToken cancellationToken = default);
}
```

```csharp
// File: src/BookStation.Domain/Repositories/IRoleRepository.cs

using BookStation.Domain.Entities.UserAggregate;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface cho Role aggregate.
/// </summary>
public interface IRoleRepository : IWriteOnlyRepository<Role, long>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Role?> GetWithPermissionsAsync(long roleId, CancellationToken cancellationToken = default);
    Task<List<Role>> GetAllActiveRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
```

---

*Tiếp tục ở phần 2: Application Layer...*
