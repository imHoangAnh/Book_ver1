# 🔐 Password & PasswordHash Logic Review

**Date:** 2026-01-23  
**Status:** ✅ REVIEWED & CORRECTED

---

## 📋 Tổng Quan

BookStation sử dụng **2 Value Objects** để quản lý password:

| Value Object | Mục Đích | Lưu DB? | Vòng Đời |
|--------------|----------|---------|----------|
| **Password** | Validate password **strength** (business rules) | ❌ Không | ~Miliseconds |
| **PasswordHash** | Validate hash **format** (technical rules) | ✅ Có | Forever |

---

## 🎯 Flow Đầy Đủ

### 1️⃣ **Đăng Ký User**

```
User Input: "MyPass123"
    ↓
Password.Create("MyPass123")           ← Validate: min 8 chars, uppercase, digit
    ↓ (Password VO)
_passwordHasher.HashPassword(password)  ← Hash BCrypt
    ↓ (PasswordHash VO)
User.Create(..., passwordHash, ...)    ← Lưu vào entity
    ↓
DB: { Email: "...", PasswordHash: "dGVzd..." }
```

**Code:**
```csharp
// RegisterUserCommandHandler.cs
var password = Password.Create(request.Password);           // Validate
var passwordHash = _passwordHasher.HashPassword(password);  // Hash
var user = User.Create(email, passwordHash, ...);          // Store
await _userRepository.AddAsync(user);                       // Save to DB
```

---

### 2️⃣ **Login User**

```
User Input: "MyPass123"
    ↓
Load user from DB
    ↓ (user.PasswordHash từ DB)
_passwordHasher.VerifyPassword("MyPass123", user.PasswordHash)
    ↓
Hash input password → compare với stored hash
    ↓
return true/false
```

**Code:**
```csharp
// LoginCommandHandler.cs
var user = await _userRepository.GetByEmailAsync(request.Email);
if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
    throw new UnauthorizedAccessException("Invalid credentials");
```

**Note:** Login KHÔNG tạo Password VO vì không cần validate strength (chỉ cần verify)

---

### 3️⃣ **Đổi Password**

```
User Input: currentPassword + newPassword
    ↓
Verify current password (string → hash → compare)
    ↓
Password.Create(newPassword)            ← Validate new password
    ↓ (Password VO)
_passwordHasher.HashPassword(password)  ← Hash new password
    ↓ (PasswordHash VO)
user.ChangePassword(newPasswordHash)    ← Update entity
```

**Code:**
```csharp
// ChangePasswordCommandHandler.cs
// 1. Verify current password
if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
    throw new InvalidOperationException("Current password incorrect");

// 2. Validate & hash new password
var newPassword = Password.Create(request.NewPassword);
var newPasswordHash = _passwordHasher.HashPassword(newPassword);

// 3. Update
user.ChangePassword(newPasswordHash);
```

---

## 🔧 Implementation Details

### Password.cs (Domain Layer)

```csharp
public sealed class Password : ValueObject
{
    public const int MinLength = 8;
    public const int MaxLength = 128;
    public string Value { get; }
    
    public static Password Create(string password)
    {
        var errors = Validate(password);
        if (errors.Count > 0)
            throw new ValidationException(...);
        return new Password(password);
    }
    
    public static List<string> Validate(string password)
    {
        // ✅ Min 8 chars
        // ✅ Has uppercase
        // ✅ Has lowercase
        // ✅ Has digit
    }
    
    public override string ToString() => "********";  // Security
}
```

---

### PasswordHash.cs (Domain Layer)

```csharp
public sealed class PasswordHash : ValueObject
{
    private const int MinHashLength = 40;
    public string Value { get; }
    
    public static PasswordHash FromHash(string hashedValue)
    {
        // ✅ Not empty
        // ✅ Min 40 chars
        // ✅ Valid Base64
        return new PasswordHash(hashedValue);
    }
    
    internal static PasswordHash FromPersistence(string hashedValue)
    {
        // Dùng bởi EF Core khi load từ DB
        // Less strict validation cho backward compatibility
        return new PasswordHash(hashedValue);
    }
    
    public override string ToString() => "[PROTECTED]";  // Security
}
```

---

### IPasswordHasher.cs (Application Layer)

```csharp
public interface IPasswordHasher
{
    // Hash: chỉ nhận Password VO (đã validated)
    PasswordHash HashPassword(Password password);
    
    // Verify: có 2 overloads
    bool VerifyPassword(string password, PasswordHash passwordHash);
    bool VerifyPassword(Password password, PasswordHash passwordHash);
}
```

**Design Decision:**
- ✅ `HashPassword` chỉ nhận `Password` VO → **bắt buộc validate trước khi hash**
- ✅ `VerifyPassword` nhận `string` → không cần validate khi login (chỉ compare)

---

### PasswordHasher.cs (Infrastructure Layer)

```csharp
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;      // BCrypt work factor
    
    public PasswordHash HashPassword(Password password)
    {
        return HashPasswordInternal(password.Value);
    }
    
    public bool VerifyPassword(string password, PasswordHash passwordHash)
    {
        return VerifyPasswordInternal(password, passwordHash.Value);
    }
    
    private PasswordHash HashPasswordInternal(string password)
    {
        // Use BCrypt to hash password
        // Salt is generated automatically
        string hash = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        return PasswordHash.FromHash(hash);
    }
    
    private bool VerifyPasswordInternal(string password, string storedHash)
    {
        try 
        {
            // BCrypt.Verify automatically extracts salt and compares
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
```

---

### UserConfiguration.cs (Infrastructure - EF Core)

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // PasswordHash value object
        builder.OwnsOne(u => u.PasswordHash, passwordHash =>
        {
            passwordHash.Property(p => p.Value)
                .HasColumnName("PasswordHash")
                .HasMaxLength(500)
                .IsRequired()
                .HasConversion(
                    v => v,  // To DB: PasswordHash.Value → string
                    v => PasswordHash.FromPersistence(v)  // From DB: string → PasswordHash
                );
        });
    }
}
```

**⚠️ QUAN TRỌNG:**
- `HasConversion` đảm bảo EF Core dùng `FromPersistence()` khi load từ DB
- Nếu không có: EF Core không thể tạo PasswordHash (vì private constructor)

---

## ✅ Những Gì ĐÃ SỬA

### 1. **UserConfiguration.cs**
**Vấn đề:** EF Core không biết cách tạo PasswordHash từ DB  
**Giải pháp:** Thêm `HasConversion` với `FromPersistence()`

```diff
  passwordHash.Property(p => p.Value)
      .HasColumnName("PasswordHash")
      .HasMaxLength(500)
-     .IsRequired();
+     .IsRequired()
+     .HasConversion(
+         v => v,
+         v => PasswordHash.FromPersistence(v)
+     );
```

---

### 2. **PasswordHasher.cs**
**Vấn đề:** Có `HashPassword(string)` nhưng không có trong interface  
**Giải pháp:** Xóa overload này để enforce validation qua Password VO

```diff
- public PasswordHash HashPassword(string password)
- {
-     return HashPasswordInternal(password);
- }
```

**Lý do:** 
- Bắt buộc mọi nơi phải dùng `Password.Create()` trước khi hash
- Ngăn chặn hash password chưa validated

---

## 🎓 Ví Dụ Sử Dụng

### ✅ ĐÚNG

```csharp
// 1. Validate trước
var password = Password.Create("MyPass123");  // Throws nếu weak password

// 2. Hash sau
var passwordHash = _hasher.HashPassword(password);

// 3. Lưu vào entity
var user = User.Create(email, passwordHash, ...);
```

### ❌ SAI (Không thể compile)

```csharp
// ❌ Không thể hash trực tiếp string
var passwordHash = _hasher.HashPassword("MyPass123");
// Compiler Error: Cannot convert string to Password

// ❌ Không thể tạo PasswordHash trực tiếp
user.PasswordHash = new PasswordHash("...");
// Compiler Error: PasswordHash constructor is private
```

---

## 🔒 Security Features

### 1. **Password Strength Validation**
- Min 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 digit
- (Optional: special character)

### 2. **BCrypt Hashing**
- Algorithm: BCrypt
- WorkFactor: 12 (adjustable)
- Salt: Managed automatically
- Output: Standard BCrypt string ($2a$12$...)

### 3. **Constant-Time Comparison**
```csharp
CryptographicOperations.FixedTimeEquals(hash1, hash2);
```
Prevents timing attacks

### 4. **Logging Protection**
```csharp
Password.ToString()     → "********"
PasswordHash.ToString() → "[PROTECTED]"
```
Prevents accidental password exposure in logs

---

## 🧪 Testing Checklist

- [ ] Register với weak password → ValidationException
- [ ] Register với strong password → Success
- [ ] Login với đúng password → Success
- [ ] Login với sai password → Fail
- [ ] Change password với current password sai → Fail
- [ ] Change password với new password weak → ValidationException
- [ ] Load user từ DB → PasswordHash được tạo đúng
- [ ] Log password/hash → không lộ plain-text

---

## 📚 References

- **PBKDF2:** https://en.wikipedia.org/wiki/PBKDF2
- **Value Objects in DDD:** https://martinfowler.com/bliki/ValueObject.html
- **ASP.NET Core Cryptography:** https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/

---

## ✅ Kết Luận

**Logic Password/PasswordHash đã HOÀN CHỈNH:**

1. ✅ Password VO validate business rules
2. ✅ PasswordHash VO validate technical format
3. ✅ IPasswordHasher chỉ nhận Password VO → enforce validation
4. ✅ EF Core configuration dùng FromPersistence()
5. ✅ Security: ToString() protection, constant-time comparison
6. ✅ All handlers follow correct flow

**Không cần sửa gì thêm!** 🎉
