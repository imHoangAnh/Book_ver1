# Hướng Dẫn Xây Dựng API Login & Register - Phần 2: Application Layer

## 4. Bước 2: Application Layer

Application Layer chứa use cases của ứng dụng, sử dụng CQRS pattern với MediatR.

### 4.1 Tổng Quan CQRS Pattern

```
CQRS = Command Query Responsibility Segregation

┌─────────────────────────────────────────────────────┐
│                      MediatR                         │
├─────────────────────────────────────────────────────┤
│                                                      │
│   Commands (Write)          Queries (Read)          │
│   ┌──────────────┐         ┌──────────────┐        │
│   │ LoginCommand │         │ GetUserQuery │        │
│   │ RegisterCmd  │         │ SearchQuery  │        │
│   └──────┬───────┘         └──────┬───────┘        │
│          │                        │                 │
│          ▼                        ▼                 │
│   ┌──────────────┐         ┌──────────────┐        │
│   │   Handler    │         │   Handler    │        │
│   │ (có side     │         │ (chỉ đọc,    │        │
│   │  effects)    │         │  tối ưu)     │        │
│   └──────────────┘         └──────────────┘        │
│                                                      │
└─────────────────────────────────────────────────────┘
```

### 4.2 Register User Command

#### RegisterUserCommand.cs

```csharp
// File: src/BookStation.Application/Users/Commands/RegisterUserCommand.cs

using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Command để đăng ký người dùng mới.
/// 
/// Command là DTO chứa dữ liệu cần thiết cho use case.
/// Implement IRequest<T> để MediatR có thể dispatch.
/// </summary>
public record RegisterUserCommand(
    string Email,
    string Password,
    string? FullName = null,
    string? Phone = null
) : IRequest<RegisterUserResponse>;

/// <summary>
/// Response trả về sau khi đăng ký thành công.
/// </summary>
public record RegisterUserResponse(
    long UserId,
    string Email,
    bool IsVerified
);
```

**Giải thích:**
- **Record**: Sử dụng record cho immutability và value equality
- **IRequest<T>**: Interface của MediatR, T là kiểu trả về
- **Primary Constructor**: C# 10+ syntax cho ngắn gọn

#### RegisterUserCommandValidator.cs

```csharp
// File: src/BookStation.Application/Users/Commands/RegisterUserCommandValidator.cs

using FluentValidation;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Validator cho RegisterUserCommand.
/// 
/// FluentValidation cho phép định nghĩa validation rules rõ ràng,
/// tách biệt khỏi business logic.
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        // ===== EMAIL VALIDATION =====
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("Email is required")
            .EmailAddress()
                .WithMessage("Invalid email format")
            .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters");

        // ===== PASSWORD VALIDATION =====
        // Password policy: ít nhất 8 ký tự, có chữ hoa, chữ thường, số
        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("Password is required")
            .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one number");

        // ===== FULL NAME VALIDATION (Optional) =====
        RuleFor(x => x.FullName)
            .MaximumLength(255)
                .WithMessage("Full name cannot exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));

        // ===== PHONE VALIDATION (Optional) =====
        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9\s\-\.]{9,20}$")
                .WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}
```

**Giải thích:**
- **AbstractValidator<T>**: Base class của FluentValidation
- **RuleFor**: Định nghĩa rule cho từng property
- **When**: Conditional validation - chỉ validate khi có giá trị
- **Chaining**: Các rule có thể chain với nhau

#### RegisterUserCommandHandler.cs

```csharp
// File: src/BookStation.Application/Users/Commands/RegisterUserCommandHandler.cs

using BookStation.Domain.Entities.UserAggregate;
using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Handler xử lý RegisterUserCommand.
/// 
/// Handler chứa business logic chính của use case.
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Constructor - Dependencies được inject qua DI container.
    /// </summary>
    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Handle method - Entry point của handler.
    /// </summary>
    /// <param name="request">Command chứa dữ liệu đăng ký</param>
    /// <param name="cancellationToken">Token để cancel operation</param>
    /// <returns>Response chứa thông tin user vừa tạo</returns>
    public async Task<RegisterUserResponse> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // ===== STEP 1: Kiểm tra email đã tồn tại chưa =====
        var isEmailUnique = await _userRepository.IsEmailUniqueAsync(
            request.Email, 
            cancellationToken);
            
        if (!isEmailUnique)
        {
            throw new InvalidOperationException(
                $"Email '{request.Email}' is already registered.");
        }

        // ===== STEP 2: Tạo Value Objects =====
        // Email.Create() sẽ validate và chuẩn hóa email
        var email = Email.Create(request.Email);
        
        // Phone là optional
        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            phone = PhoneNumber.Create(request.Phone);
        }

        // ===== STEP 3: Hash password =====
        // KHÔNG BAO GIỜ lưu plain text password!
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // ===== STEP 4: Tạo User entity =====
        // User.Create() là factory method, sẽ raise UserCreatedEvent
        var user = User.Create(email, passwordHash, request.FullName, phone);

        // ===== STEP 5: Gán role mặc định =====
        // TODO: Implement sau khi có RoleRepository
        // var defaultRole = await _roleRepository.GetByNameAsync("User", cancellationToken);
        // if (defaultRole != null)
        // {
        //     user.AssignRole(defaultRole);
        // }

        // ===== STEP 6: Lưu vào database =====
        // AddAsync chỉ track entity, chưa thực sự lưu
        // SaveChanges sẽ được gọi bởi TransactionBehavior
        await _userRepository.AddAsync(user, cancellationToken);

        // ===== STEP 7: Trả về response =====
        return new RegisterUserResponse(
            user.Id,
            user.Email.Value,
            user.IsVerified
        );
    }
}

/// <summary>
/// Interface cho password hashing service.
/// Định nghĩa trong Application layer, implement trong Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash một password.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Xác minh password với hash đã lưu.
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}
```

**Giải thích chi tiết:**

1. **IRequestHandler<TRequest, TResponse>**: Interface của MediatR
2. **Dependency Injection**: Repository và services được inject qua constructor
3. **Validation đã xảy ra trước đó**: ValidationBehavior đã chạy trước Handler
4. **Repository chỉ track**: `AddAsync` chưa lưu, `SaveChanges` sẽ được gọi bởi TransactionBehavior
5. **Domain Events**: `User.Create()` đã raise event, sẽ được publish khi SaveChanges

### 4.3 Login Command

#### LoginCommand.cs

```csharp
// File: src/BookStation.Application/Users/Commands/LoginCommand.cs

using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Command để đăng nhập.
/// </summary>
public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponse>;

/// <summary>
/// Response trả về sau khi đăng nhập thành công.
/// Chứa JWT token và thông tin user.
/// </summary>
public record LoginResponse(
    long UserId,
    string Email,
    string Token,
    DateTime ExpiresAt,
    List<string> Roles
);
```

#### LoginCommandHandler.cs

```csharp
// File: src/BookStation.Application/Users/Commands/LoginCommandHandler.cs

using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Authentication;
using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Handler xử lý LoginCommand.
/// 
/// Flow:
/// 1. Tìm user theo email
/// 2. Xác minh password
/// 3. Kiểm tra trạng thái user
/// 4. Lấy roles và permissions
/// 5. Generate JWT token
/// 6. Trả về response
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> Handle(
        LoginCommand request, 
        CancellationToken cancellationToken)
    {
        // ===== STEP 1: Tìm user theo email =====
        var user = await _userRepository.GetByEmailAsync(
            request.Email, 
            cancellationToken);
            
        if (user == null)
        {
            // Security: Không tiết lộ email có tồn tại hay không
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // ===== STEP 2: Xác minh password =====
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Security: Cùng một message với email không tồn tại
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // ===== STEP 3: Kiểm tra trạng thái user =====
        if (user.Status != Domain.Enums.EUserStatus.Active)
        {
            throw new UnauthorizedAccessException("User account is not active.");
        }

        // ===== STEP 4: Lấy user với roles =====
        var userWithRoles = await _userRepository.GetWithRolesAsync(
            user.Id, 
            cancellationToken);
            
        var roles = userWithRoles?.UserRoles
            .Select(ur => ur.Role!.Name)
            .ToList() ?? new List<string>();

        // ===== STEP 5: Lấy permissions từ tất cả roles (RBAC) =====
        var permissions = new List<string>();
        foreach (var userRole in userWithRoles?.UserRoles ?? [])
        {
            var roleWithPermissions = await _roleRepository.GetWithPermissionsAsync(
                userRole.RoleId, 
                cancellationToken);

            if (roleWithPermissions != null)
            {
                var rolePermissions = roleWithPermissions.RolePermissions
                    .Select(rp => rp.Permission!.Name)
                    .ToList();
                permissions.AddRange(rolePermissions);
            }
        }

        // Loại bỏ permissions trùng lặp
        permissions = permissions.Distinct().ToList();

        // ===== STEP 6: Generate JWT token =====
        var token = _jwtTokenGenerator.GenerateToken(
            user.Id, 
            user.Email.Value, 
            roles, 
            permissions);

        // ===== STEP 7: Trả về response =====
        return new LoginResponse(
            user.Id,
            user.Email.Value,
            token,
            DateTime.UtcNow.AddMinutes(60), // Phải match với JwtSettings.ExpirationInMinutes
            roles
        );
    }
}
```

**Giải thích:**

1. **Security - Same Error Message**: Không tiết lộ email có tồn tại hay không
2. **Status Check**: Chỉ cho phép Active user đăng nhập
3. **RBAC**: Lấy tất cả permissions từ tất cả roles của user
4. **JWT Contains**: userId, email, roles, permissions

### 4.4 Pipeline Behaviors

Behaviors là middleware trong MediatR pipeline, chạy trước/sau handler.

```
Request → Validation → Logging → Transaction → Handler → Response
              ↓            ↓           ↓
         (throw if    (log request)  (save changes)
          invalid)
```

#### ValidationBehavior.cs

```csharp
// File: src/BookStation.Application/Behaviors/ValidationBehavior.cs

using FluentValidation;
using MediatR;

namespace BookStation.Application.Behaviors;

/// <summary>
/// Behavior tự động validate tất cả commands/queries.
/// Chạy TRƯỚC handler.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// FluentValidation tự động inject tất cả validators cho TRequest.
    /// </summary>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Nếu không có validator nào, skip
        if (!_validators.Any())
        {
            return await next();
        }

        // Tạo validation context
        var context = new ValidationContext<TRequest>(request);

        // Chạy tất cả validators song song
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Gom tất cả lỗi
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // Nếu có lỗi, throw exception
        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        // Không có lỗi, tiếp tục đến handler
        return await next();
    }
}
```

#### TransactionBehavior.cs

```csharp
// File: src/BookStation.Application/Behaviors/TransactionBehavior.cs

using BookStation.Core.SharedKernel;
using MediatR;

namespace BookStation.Application.Behaviors;

/// <summary>
/// Behavior tự động gọi SaveChanges sau khi handler thành công.
/// Chạy SAU handler.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Chỉ áp dụng cho Commands, không áp dụng cho Queries
        var requestName = typeof(TRequest).Name;
        if (requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
        {
            return await next();
        }

        // Gọi handler
        var response = await next();

        // SaveChanges - đồng thời publish domain events
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
```

**Giải thích:**
- **Query không cần SaveChanges**: Query chỉ đọc, không thay đổi data
- **Auto SaveChanges**: Handler không cần gọi SaveChanges, behavior làm điều đó
- **Domain Events**: SaveChangesAsync trong WriteDbContext sẽ publish events

#### LoggingBehavior.cs

```csharp
// File: src/BookStation.Application/Behaviors/LoggingBehavior.cs

using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BookStation.Application.Behaviors;

/// <summary>
/// Behavior log thông tin request để monitoring.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        _logger.LogInformation(
            "Handling {RequestName} - {@Request}", 
            requestName, 
            request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            
            stopwatch.Stop();
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms", 
                requestName, 
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex, 
                "Error handling {RequestName} after {ElapsedMs}ms", 
                requestName, 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

### 4.5 DI Configuration cho Application Layer

```csharp
// File: src/BookStation.Application/ConfigureServices.cs

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;

namespace BookStation.Application;

/// <summary>
/// Extension method để register tất cả services của Application layer.
/// </summary>
public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ===== MEDIATR =====
        // Scan assembly và register tất cả handlers
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // ===== FLUENT VALIDATION =====
        // Scan assembly và register tất cả validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // ===== PIPELINE BEHAVIORS =====
        // Thứ tự register là thứ tự chạy
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }
}
```

---

## Tóm Tắt Application Layer

| Component | Responsibility |
|-----------|---------------|
| **Command** | DTO chứa dữ liệu cho write operation |
| **Query** | DTO chứa dữ liệu cho read operation |
| **Handler** | Business logic xử lý command/query |
| **Validator** | Validation rules cho command/query |
| **Behavior** | Cross-cutting concerns (validation, logging, transaction) |

**Flow của một Request:**

```
1. Controller nhận HTTP request
2. Controller tạo Command/Query
3. Controller gọi _mediator.Send(command)
4. MediatR dispatch qua pipeline:
   a. ValidationBehavior: Validate command
   b. LoggingBehavior: Log request
   c. TransactionBehavior: Wrap trong transaction
   d. Handler: Xử lý business logic
5. TransactionBehavior: SaveChanges, publish domain events
6. Controller trả về HTTP response
```

---

*Tiếp tục ở phần 3: Infrastructure Layer...*
