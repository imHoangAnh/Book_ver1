# Hướng Dẫn Xây Dựng API Login & Register - Phần 5: Summary & Best Practices

## 8. Tổng Kết

### 8.1 Kiến Trúc Tổng Quan

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              CLIENT                                      │
│                    (Browser, Mobile App, etc.)                           │
└─────────────────────────────────────┬───────────────────────────────────┘
                                      │ HTTP Request
                                      ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         PublicApi Layer                                  │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────┐ │
│  │  AuthController │  │ Authorization   │  │  Swagger/OpenAPI       │ │
│  │  - Register     │  │ - JWT Bearer    │  │                        │ │
│  │  - Login        │  │ - Permissions   │  │                        │ │
│  │  - Profile      │  │                 │  │                        │ │
│  └────────┬────────┘  └─────────────────┘  └─────────────────────────┘ │
└───────────┼─────────────────────────────────────────────────────────────┘
            │ MediatR.Send(command)
            ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        Application Layer                                 │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                     MediatR Pipeline                             │   │
│  │  Request → ValidationBehavior → LoggingBehavior →               │   │
│  │            TransactionBehavior → Handler → Response              │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────────┐  │
│  │ Commands         │  │ Handlers         │  │ Validators           │  │
│  │ - RegisterUser   │  │ - RegisterUser   │  │ - RegisterUser       │  │
│  │ - Login          │  │ - Login          │  │                      │  │
│  └──────────────────┘  └────────┬─────────┘  └──────────────────────┘  │
└──────────────────────────────────┼──────────────────────────────────────┘
                                   │ Repository calls
                                   ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         Domain Layer                                     │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────────┐  │
│  │ Entities         │  │ Value Objects    │  │ Repository           │  │
│  │ - User           │  │ - Email          │  │ Interfaces           │  │
│  │ - Role           │  │ - PhoneNumber    │  │ - IUserRepository    │  │
│  │ - Permission     │  │                  │  │ - IRoleRepository    │  │
│  └──────────────────┘  └──────────────────┘  └──────────────────────┘  │
│  ┌──────────────────┐  ┌──────────────────┐                            │
│  │ Domain Events    │  │ Enums            │                            │
│  │ - UserCreated    │  │ - EUserStatus    │                            │
│  │ - UserVerified   │  │                  │                            │
│  └──────────────────┘  └──────────────────┘                            │
└─────────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      Infrastructure Layer                                │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────────┐  │
│  │ Repositories     │  │ Services         │  │ Authentication       │  │
│  │ - UserRepository │  │ - PasswordHasher │  │ - JwtTokenGenerator  │  │
│  │ - RoleRepository │  │                  │  │ - JwtSettings        │  │
│  └──────────────────┘  └──────────────────┘  └──────────────────────┘  │
│  ┌──────────────────┐  ┌──────────────────┐                            │
│  │ Persistence      │  │ Data             │                            │
│  │ - WriteDbContext │  │ - RbacDataSeeder │                            │
│  │ - Configurations │  │                  │                            │
│  └──────────────────┘  └──────────────────┘                            │
└─────────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                           Database                                       │
│                        (SQL Server)                                      │
│  ┌─────────┐  ┌─────────┐  ┌─────────────┐  ┌───────────────────────┐  │
│  │ Users   │  │ Roles   │  │ Permissions │  │ UserRoles/RolePerms   │  │
│  └─────────┘  └─────────┘  └─────────────┘  └───────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Flow Chi Tiết

#### Register Flow

```
1. Client gửi POST /api/auth/register
   Body: { email, password, fullName?, phone? }

2. AuthController.Register() nhận request
   → Tạo RegisterUserCommand
   → Gọi _mediator.Send(command)

3. MediatR Pipeline:
   a. ValidationBehavior:
      - RegisterUserCommandValidator kiểm tra:
        ✓ Email not empty, valid format, max 255 chars
        ✓ Password min 8 chars, có uppercase, lowercase, number
        ✓ FullName max 255 chars (nếu có)
        ✓ Phone valid format (nếu có)
      - Nếu lỗi → throw ValidationException → 400 Bad Request

   b. LoggingBehavior:
      - Log request info

   c. TransactionBehavior:
      - Gọi handler
      - Sau handler: SaveChangesAsync()

4. RegisterUserCommandHandler.Handle():
   a. Kiểm tra email unique
      - Nếu đã tồn tại → throw InvalidOperationException
   b. Tạo Value Objects (Email, PhoneNumber)
   c. Hash password (PBKDF2)
   d. User.Create() → raise UserCreatedEvent
   e. _userRepository.AddAsync(user)
   f. Trả về RegisterUserResponse

5. TransactionBehavior:
   - SaveChangesAsync() → lưu vào DB
   - Publish domain events

6. Controller trả về 201 Created
   Response: { userId, email, isVerified: false }
```

#### Login Flow

```
1. Client gửi POST /api/auth/login
   Body: { email, password }

2. AuthController.Login() nhận request
   → Tạo LoginCommand
   → Gọi _mediator.Send(command)

3. MediatR Pipeline (như trên)

4. LoginCommandHandler.Handle():
   a. Tìm user theo email
      - Không tìm thấy → throw UnauthorizedAccessException
   b. Verify password
      - Sai password → throw UnauthorizedAccessException
   c. Kiểm tra status
      - Không Active → throw UnauthorizedAccessException
   d. Lấy user với roles
   e. Lấy permissions từ tất cả roles
   f. Generate JWT token với:
      - UserId, Email
      - Roles (ClaimTypes.Role)
      - Permissions (custom claim)
   g. Trả về LoginResponse

5. Controller trả về 200 OK
   Response: { userId, email, token, expiresAt, roles }
```

### 8.3 File Structure

```
BookStation-src/
├── src/
│   ├── BookStation.Core/
│   │   └── SharedKernel/
│   │       ├── Entity.cs
│   │       ├── AggregateRoot.cs
│   │       ├── ValueObject.cs
│   │       ├── DomainEvent.cs
│   │       ├── IUnitOfWork.cs
│   │       └── IWriteOnlyRepository.cs
│   │
│   ├── BookStation.Domain/
│   │   ├── Entities/
│   │   │   └── UserAggregate/
│   │   │       ├── User.cs
│   │   │       ├── Role.cs
│   │   │       ├── Permission.cs
│   │   │       ├── UserRole.cs
│   │   │       └── UserEvents.cs
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs
│   │   │   └── PhoneNumber.cs
│   │   ├── Enums/
│   │   │   └── UserEnums.cs
│   │   └── Repositories/
│   │       ├── IUserRepository.cs
│   │       └── IRoleRepository.cs
│   │
│   ├── BookStation.Application/
│   │   ├── Users/
│   │   │   └── Commands/
│   │   │       ├── RegisterUserCommand.cs
│   │   │       ├── RegisterUserCommandHandler.cs
│   │   │       ├── RegisterUserCommandValidator.cs
│   │   │       ├── LoginCommand.cs
│   │   │       └── LoginCommandHandler.cs
│   │   ├── Behaviors/
│   │   │   ├── ValidationBehavior.cs
│   │   │   ├── LoggingBehavior.cs
│   │   │   └── TransactionBehavior.cs
│   │   └── ConfigureServices.cs
│   │
│   ├── BookStation.Infrastructure/
│   │   ├── Authentication/
│   │   │   ├── JwtSettings.cs
│   │   │   └── JwtTokenGenerator.cs
│   │   ├── Services/
│   │   │   └── PasswordHasher.cs
│   │   ├── Repositories/
│   │   │   ├── UserRepository.cs
│   │   │   └── RoleRepository.cs
│   │   ├── Persistence/
│   │   │   ├── WriteDbContext.cs
│   │   │   └── Configurations/
│   │   │       ├── UserConfiguration.cs
│   │   │       └── RoleConfiguration.cs
│   │   ├── Data/
│   │   │   └── RbacDataSeeder.cs
│   │   └── ConfigureServices.cs
│   │
│   └── BookStation.PublicApi/
│       ├── Controllers/
│       │   └── AuthController.cs
│       ├── Authorization/
│       │   └── PermissionAuthorization.cs
│       ├── Program.cs
│       └── appsettings.json
│
└── docs/
    └── AUTH_API_*.md (documentation)
```

---

## 9. Best Practices & Security

### 9.1 Password Security

```csharp
// ✅ DO: Sử dụng PBKDF2 với đủ iterations
private const int Iterations = 100000;

// ✅ DO: Sử dụng random salt cho mỗi password
byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

// ❌ DON'T: Hash đơn giản
var hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));

// ❌ DON'T: Salt cố định
var salt = Encoding.UTF8.GetBytes("fixed-salt");
```

### 9.2 JWT Security

```csharp
// ✅ DO: Secret key đủ dài (64+ bytes cho HS256)
"Secret": "YourSuperSecretKeyThatIsAtLeast64BytesLongForHS256Algorithm!"

// ✅ DO: Validate tất cả các thông số
ValidateIssuer = true,
ValidateAudience = true,
ValidateLifetime = true,
ValidateIssuerSigningKey = true,

// ✅ DO: Không cho clock skew
ClockSkew = TimeSpan.Zero

// ❌ DON'T: Lưu secret trong code
const string Secret = "my-secret"; // BAD!

// ❌ DON'T: Disable validation
ValidateIssuer = false // BAD!
```

### 9.3 Error Handling

```csharp
// ✅ DO: Không tiết lộ thông tin nhạy cảm
if (user == null)
    throw new UnauthorizedAccessException("Invalid email or password.");

if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
    throw new UnauthorizedAccessException("Invalid email or password."); // Same message!

// ❌ DON'T: Tiết lộ email có tồn tại
if (user == null)
    throw new UnauthorizedAccessException("Email not found."); // BAD!
```

### 9.4 Value Objects

```csharp
// ✅ DO: Validation trong factory method
public static Email Create(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        throw new ArgumentException("Email cannot be empty.");
    // ... more validation
    return new Email(email.ToLowerInvariant().Trim());
}

// ✅ DO: Private constructor
private Email(string value) { Value = value; }

// ❌ DON'T: Public constructor
public Email(string value) { Value = value; } // BAD!
```

### 9.5 Repository Pattern

```csharp
// ✅ DO: Repository chỉ track, không save
public async Task AddAsync(User entity, CancellationToken cancellationToken)
{
    await _context.Users.AddAsync(entity, cancellationToken);
    // KHÔNG gọi SaveChanges ở đây
}

// ✅ DO: TransactionBehavior gọi SaveChanges
var response = await next();
await _unitOfWork.SaveChangesAsync(cancellationToken);
return response;
```

### 9.6 CQRS

```csharp
// ✅ DO: Commands cho write, Queries cho read
public record RegisterUserCommand(...) : IRequest<RegisterUserResponse>;
public record GetUserByIdQuery(long Id) : IRequest<UserDto>;

// ✅ DO: Handler chỉ làm một việc
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    // Chỉ xử lý đăng ký user
}
```

---

## 10. Checklist Triển Khai

### Phase 1: Core & Domain
- [ ] Tạo project structure
- [ ] Implement Core SharedKernel (Entity, AggregateRoot, ValueObject)
- [ ] Implement Domain Entities (User, Role, Permission)
- [ ] Implement Value Objects (Email, PhoneNumber)
- [ ] Implement Domain Events
- [ ] Define Repository Interfaces

### Phase 2: Application
- [ ] Setup MediatR
- [ ] Implement RegisterUserCommand & Handler
- [ ] Implement RegisterUserCommandValidator
- [ ] Implement LoginCommand & Handler
- [ ] Implement Pipeline Behaviors
- [ ] Configure DI

### Phase 3: Infrastructure
- [ ] Implement PasswordHasher
- [ ] Implement JwtTokenGenerator
- [ ] Implement UserRepository
- [ ] Implement RoleRepository
- [ ] Setup EF Core DbContext
- [ ] Create Entity Configurations
- [ ] Implement RbacDataSeeder
- [ ] Configure DI

### Phase 4: API
- [ ] Create AuthController
- [ ] Configure JWT Authentication
- [ ] Configure Authorization Policies
- [ ] Setup Swagger with JWT
- [ ] Configure CORS
- [ ] Setup appsettings.json

### Phase 5: Testing
- [ ] Test Register với Swagger
- [ ] Test Login với Swagger
- [ ] Test Protected endpoints
- [ ] Test Validation errors
- [ ] Test Error responses

---

## 11. Tài Liệu Tham Khảo

- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [JWT.io](https://jwt.io/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)

---

**Kết thúc hướng dẫn.**

*Nếu có thắc mắc, hãy review lại các phần trước hoặc tham khảo source code trong project.*
