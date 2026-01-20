---
description: Workflow chạy và test API Login, Register và Profile trong BookStation
---

# 🔐 Auth API Workflow - Login, Register & Profile

Workflow này hướng dẫn cách chạy và test các API xác thực trong BookStation.

## 📋 Prerequisites

1. **.NET 8.0 SDK** đã được cài đặt
2. **SQL Server** đang chạy (LocalDB hoặc SQL Server)
3. **Database đã được migration** (xem bước 1)

---

## 🚀 Các Bước Thực Hiện

### Bước 1: Cập nhật Database (nếu chưa có)

```bash
cd src/BookStation.Infrastructure
dotnet ef database update --startup-project ../BookStation.PublicApi
```

### Bước 2: Chạy ứng dụng

// turbo
```bash
cd src/BookStation.PublicApi
dotnet run
```

Hoặc với hot reload:
```bash
dotnet watch run
```

### Bước 3: Mở Swagger UI

Truy cập: `https://localhost:7000/swagger`

---

## 📝 Test APIs

### 1️⃣ Register - Đăng ký tài khoản mới

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "test@example.com",
  "password": "SecurePass123",
  "fullName": "Test User",
  "phone": "+84901234567"
}
```

**Expected Response (201 Created):**
```json
{
  "userId": 1,
  "email": "test@example.com",
  "isVerified": false
}
```

---

### 2️⃣ Login - Đăng nhập

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "test@example.com",
  "password": "SecurePass123"
}
```

**Expected Response (200 OK):**
```json
{
  "userId": 1,
  "email": "test@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-01-19T17:22:18Z",
  "roles": ["User"]
}
```

> ⚠️ **Lưu ý:** Lưu lại `token` để sử dụng cho API Profile

---

### 3️⃣ Profile - Xem thông tin người dùng

**Endpoint:** `GET /api/auth/profile`

**Headers:**
```
Authorization: Bearer <token_từ_login>
```

**Expected Response (200 OK):**
```json
{
  "userId": "1",
  "email": "test@example.com",
  "roles": ["User"],
  "permissions": []
}
```

---

## 🔧 Troubleshooting

### Lỗi 401 Unauthorized khi gọi Profile
- Kiểm tra token có đúng format: `Bearer <token>`
- Token có thể đã hết hạn (mặc định 60 phút)
- Đăng nhập lại để lấy token mới

### Lỗi 400 Bad Request khi Register
- Email có thể đã tồn tại trong hệ thống
- Password không đủ mạnh (yêu cầu tối thiểu 8 ký tự)

### Lỗi kết nối Database
- Kiểm tra connection string trong `appsettings.json`
- Đảm bảo SQL Server đang chạy

---

## 📁 Files liên quan

### Presentation Layer
- `src/BookStation.PublicApi/Controllers/AuthController.cs`

### Application Layer (CQRS)
- `src/BookStation.Application/Users/Commands/RegisterUserCommand.cs`
- `src/BookStation.Application/Users/Commands/RegisterUserCommandHandler.cs`
- `src/BookStation.Application/Users/Commands/RegisterUserCommandValidator.cs`
- `src/BookStation.Application/Users/Commands/LoginCommand.cs`
- `src/BookStation.Application/Users/Commands/LoginCommandHandler.cs`

### Domain Layer
- `src/BookStation.Domain/Entities/UserAggregate/User.cs`
- `src/BookStation.Domain/Entities/UserAggregate/Role.cs`
- `src/BookStation.Domain/ValueObjects/Email.cs`
- `src/BookStation.Domain/Repositories/IUserRepository.cs`
- `src/BookStation.Domain/Repositories/IRoleRepository.cs`

### Infrastructure Layer
- `src/BookStation.Infrastructure/Repositories/UserRepository.cs`
- `src/BookStation.Infrastructure/Services/PasswordHasher.cs`
- `src/BookStation.Infrastructure/Authentication/JwtTokenGenerator.cs`
- `src/BookStation.Infrastructure/Authentication/JwtSettings.cs`

### Configuration
- `src/BookStation.PublicApi/appsettings.json` (JWT Settings & Connection String)
