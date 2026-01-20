# 🔐 RBAC (Role-Based Access Control) - Hướng Dẫn

Tài liệu này mô tả cách thức hoạt động của hệ thống phân quyền dựa trên **Roles** (vai trò) và **Permissions** (quyền hạn) trong BookStation.

---

## 📖 Khái niệm RBAC

**RBAC** (Role-Based Access Control) là mô hình phân quyền trong đó:
- **Users** (người dùng) được gán vào các **Roles** (vai trò)
- **Roles** có tập hợp các **Permissions** (quyền hạn)
- User có tất cả permissions của các roles mà họ được gán

```
User ──has──> Roles ──has──> Permissions
```

### Ví dụ:
- User "John" có role "Seller"
- Role "Seller" có permissions: `books.create`, `books.update`, `inventory.view`
- → John có thể tạo sách, sửa sách, và xem tồn kho

---

## 🏗️ Cấu trúc RBAC trong BookStation

### 1. Entities

#### **Role** (Vai trò)
```csharp
public class Role : Entity<long>
{
    public string Name { get; private set; }  // VD: "Admin", "Seller"
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }  // Không thể xóa
    public bool IsActive { get; private set; }
    
    // Relationships
    public IReadOnlyList<UserRole> UserRoles { get; }
    public IReadOnlyList<RolePermission> RolePermissions { get; }
    
    // Methods
    public void AddPermission(int permissionId);
    public void RemovePermission(int permissionId);
}
```

#### **Permission** (Quyền hạn)
```csharp
public class Permission : Entity<int>
{
    public string Name { get; private set; }  // VD: "books.create"
    public string Description { get; private set; }
    public string Category { get; private set; }  // VD: "Books"
    
    // Predefined permissions
    public static class Permissions
    {
        public const string BooksCreate = "books.create";
        public const string OrdersViewAll = "orders.viewall";
        // ...
    }
}
```

#### **RolePermission** (Junction Table)
```csharp
public class RolePermission
{
    public long RoleId { get; private set; }
    public int PermissionId { get; private set; }
    
    public Role? Role { get; private set; }
    public Permission? Permission { get; private set; }
}
```

---

## 🎭 Predefined Roles & Permissions

### System Roles

| Role | Description | Key Permissions |
|------|-------------|-----------------|
| **Admin** | Quản trị viên hệ thống | ALL permissions |
| **User** | Người dùng thông thường | books.view, orders.view, orders.create |
| **Seller** | Người bán hàng | books.*, orders.viewall, inventory.* |
| **Shipper** | Người giao hàng | shipments.*, orders.viewall |
| **Warehouse** | Nhân viên kho | inventory.*, books.view |

### Permission Categories

#### 📚 Books
- `books.view` - Xem sách
- `books.create` - Tạo sách mới
- `books.update` - Cập nhật sách
- `books.delete` - Xóa sách

#### 🛒 Orders
- `orders.view` - Xem đơn hàng của mình
- `orders.create` - Tạo đơn hàng
- `orders.update` - Cập nhật đơn hàng
- `orders.cancel` - Hủy đơn hàng
- `orders.viewall` - Xem tất cả đơn hàng *(Admin, Seller)*
- `orders.manage` - Quản lý tất cả đơn hàng *(Admin, Seller)*

#### 👥 Users
- `users.view` - Xem người dùng
- `users.create` - Tạo người dùng
- `users.update` - Cập nhật người dùng
- `users.delete` - Xóa người dùng

#### 📦 Inventory
- `inventory.view` - Xem tồn kho
- `inventory.update` - Cập nhật tồn kho

---

## 🔧 Cách sử dụng RBAC

### 1. Seed Initial Data

Chạy seeder khi khởi động ứng dụng:

```csharp
// Program.cs
var app = builder.Build();

// Seed RBAC data
await app.SeedRbacDataAsync();

app.Run();
```

### 2. Authorization trong Controllers

#### A. Dùng Role-based Authorization

```csharp
[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    // Chỉ Admin và Seller mới được tạo sách
    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> CreateBook(CreateBookCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.BookId }, result);
    }
}
```

#### B. Dùng Permission-based Authorization (Tinh vi hơn)

```csharp
[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    // Chỉ user có permission "books.create" mới được tạo sách
    [HttpPost]
    [RequirePermission("books.create")]
    public async Task<IActionResult> CreateBook(CreateBookCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.BookId }, result);
    }
    
    // Chỉ Admin mới được xóa sách
    [HttpDelete("{id}")]
    [RequirePermission("books.delete")]
    public async Task<IActionResult> DeleteBook(long id)
    {
        // ...
    }
}
```

### 3. Check Permissions trong Code

```csharp
public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, CreateBookResponse>
{
    public async Task<CreateBookResponse> Handle(...)
    {
        // Get current user từ HttpContext
        var userId = _httpContextAccessor.HttpContext.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Check if user has permission
        var hasPermission = _httpContextAccessor.HttpContext.User
            .HasClaim("permission", "books.create");
        
        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You don't have permission to create books.");
        }
        
        // ... business logic
    }
}
```

### 4. Quản lý Roles & Permissions (Admin)

#### Tạo Role mới
```csharp
[HttpPost("api/admin/roles")]
[RequirePermission("users.create")]
public async Task<IActionResult> CreateRole(CreateRoleCommand command)
{
    var role = Role.Create(command.Name, command.Description);
    
    // Assign permissions
    foreach (var permissionId in command.PermissionIds)
    {
        role.AddPermission(permissionId);
    }
    
    await _roleRepository.AddAsync(role);
    await _unitOfWork.SaveChangesAsync();
    
    return Ok();
}
```

#### Gán User vào Role
```csharp
[HttpPost("api/admin/users/{userId}/roles")]
[RequirePermission("users.update")]
public async Task<IActionResult> AssignRoleToUser(long userId, AssignRoleCommand command)
{
    var user = await _userRepository.GetByIdAsync(userId);
    user.AddRole(command.RoleId);
    
    await _unitOfWork.SaveChangesAsync();
    return Ok();
}
```

---

## 🔑 JWT Token với RBAC

Khi user login, JWT token sẽ chứa:
- **User ID**
- **Email**
- **Roles** (claims với type `ClaimTypes.Role`)
- **Permissions** (claims với type `"permission"`)

### Example Token Claims:
```json
{
  "sub": "123",
  "email": "seller@example.com",
  "role": ["Seller"],
  "permission": [
    "books.create",
    "books.update",
    "orders.viewall",
    "inventory.view"
  ],
  "exp": 1704500000
}
```

### Decode Token:
```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var email = User.FindFirst(ClaimTypes.Email)?.Value;
var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
var permissions = User.FindAll("permission").Select(c => c.Value).ToList();
```

---

## 📊 Database Schema

```sql
-- Roles table
CREATE TABLE Roles (
    Id BIGINT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    IsSystemRole BIT DEFAULT 0,
    IsActive BIT DEFAULT 1
);

-- Permissions table
CREATE TABLE Permissions (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    Category NVARCHAR(50) NOT NULL
);

-- Role-Permission junction table
CREATE TABLE RolePermissions (
    RoleId BIGINT NOT NULL,
    PermissionId INT NOT NULL,
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE
);

-- User-Role junction table
CREATE TABLE UserRoles (
    UserId BIGINT NOT NULL,
    RoleId BIGINT NOT NULL,
    AssignedAt DATETIME2 DEFAULT GETUTCDATE(),
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);
```

---

## 🎯 Best Practices

### 1. Principle of Least Privilege
Chỉ cấp permission tối thiểu cần thiết cho mỗi role.

### 2. Don't Delete System Roles
System roles (`Admin`, `User`, `Seller`) không được xóa vì nhiều logic phụ thuộc vào chúng.

### 3. Permission Naming Convention
- Format: `<resource>.<action>`
- Examples: `books.create`, `orders.viewall`
- Lowercase, dùng dot (.)

### 4. Combine Roles & Permissions
- Dùng **Roles** cho authorization cơ bản (`[Authorize(Roles = "Admin")]`)
- Dùng **Permissions** cho fine-grained control (`[RequirePermission("books.delete")]`)

### 5. Cache Permissions
Permissions ít thay đổi, nên cache lại trong JWT token hoặc memory cache để tránh query DB nhiều lần.

---

## 🔄 Migration Flow

1. Tạo migration:
```bash
dotnet ef migrations add AddRbacSupport --startup-project ../BookStation.PublicApi
```

2. Update database:
```bash
dotnet ef database update --startup-project ../BookStation.PublicApi
```

3. Seed data sẽ tự động chạy khi application khởi động (nếu đã config trong `Program.cs`).

---

## ✅ Testing RBAC

### Test với Postman/Swagger:

#### 1. Login as Admin
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@bookstation.com",
  "password": "Admin@123"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "roles": ["Admin"]
}
```

#### 2. Use Token to Access Protected Endpoint
```http
POST /api/books
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "title": "Clean Architecture",
  "isbn": "978-0134494166"
}
```

#### 3. Test Forbidden Access (403)
Login as User, then try to delete a book (requires `books.delete` permission):
```http
DELETE /api/books/1
Authorization: Bearer <user_token>

Response: 403 Forbidden
```

---

Hệ thống RBAC của BookStation giúp bạn kiểm soát quyền truy cập một cách linh hoạt và bảo mật! 🚀
