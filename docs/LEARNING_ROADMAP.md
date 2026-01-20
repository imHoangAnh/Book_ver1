# 🎓 Lộ Trình Học Tập & Xây Dựng Lại BookStation

Tài liệu này hướng dẫn bạn từng bước xây dựng lại dự án **BookStation** từ con số 0. Mục tiêu là giúp bạn hiểu sâu về **Clean Architecture**, **DDD**, **CQRS** và các công nghệ hiện đại trong .NET.

---

## 📅 Phase 1: Khởi tạo & Foundation (Nền móng)

Trong giai đoạn này, bạn sẽ thiết lập cấu trúc Solution và các class cơ sở (Building Blocks) cho DDD.

### 1.1. Tạo Solution & Projects
- [ ] Tạo Solution trống (`BookStation`)
- [ ] Tạo thư mục `src`
- [ ] Tạo class libs: 
    - `BookStation.Core`
    - `BookStation.Domain`
    - `BookStation.Application`
    - `BookStation.Query`
    - `BookStation.Infrastructure`
- [ ] Tạo Web API project: `BookStation.PublicApi`
- [ ] Thiết lập Project References (Core -> Domain -> Application/Query -> Infrastructure -> PublicApi)
- [ ] Thiết lập **Central Package Management** (`Directory.Packages.props`) để quản lý version nuget tập trung.

### 1.2. Xây dựng Core (Shared Kernel)
- [ ] Tạo `Entity<TId>` và `IEntity` (quản lý Id, equality)
- [ ] Tạo `AggregateRoot<TId>` và `IAggregateRoot` (quản lý Domain Events)
- [ ] Tạo `ValueObject` (base class cho value objects)
- [ ] Tạo `IDomainEvent` & `DomainEvent` base class (dùng MediatR `INotification`)
- [ ] Tạo `IUnitOfWork` & `IWriteOnlyRepository` interfaces
- [ ] Tạo `Result` pattern & Custom Exceptions (`DomainException`)

---

## 📅 Phase 2: Domain Modeling (Trái tim của hệ thống)

Giai đoạn này tập trung hoàn toàn vào Business Logic, không quan tâm database hay API.

### 2.1. Value Objects & Enums
- [ ] Định nghĩa các Enums (`UserStatus`, `BookStatus`, `OrderStatus`,...)
- [ ] Tạo Value Objects quan trọng:
    - `Email` (validate format)
    - `PhoneNumber` (validate format)
    - `Address` (cấu trúc địa chỉ)
    - `Money` (số tiền + loại tiền tệ)
    - `ISBN` (mã sách)

### 2.2. Entities & Aggregates
- [ ] **Book Aggregate**: `Book`, `BookVariant` (giá, sku), `Author`, `Category`
- [ ] **User Aggregate**: `User`, `Role`, `UserRole`
- [ ] **Order Aggregate**: `Order`, `OrderItem`, `Payment`, `Shipment`
- [ ] Định nghĩa các Repository Interfaces trong Domain (VD: `IUserRepository`, `IBookRepository`)
- [ ] Thêm các Domain Events (VD: `OrderCreatedEvent`, `UserRegisteredEvent`)

---

## 📅 Phase 3: Application Layer (CQRS - Write Side)

Triển khai Use Cases thay đổi dữ liệu (Command).

### 3.1. Setup MediatR & Behaviors
- [ ] Cài đặt `MediatR` và `FluentValidation`
- [ ] Tạo `ValidationBehavior`: Tự động validate trước khi xử lý
- [ ] Tạo `LoggingBehavior`: Log request/response
- [ ] Tạo `TransactionBehavior`: Tự động commit transaction sau khi handler chạy xong

### 3.2. Implement Commands
- [ ] **User**: `RegisterUserCommand` + `Handler` + `Validator`
- [ ] **Book**: `CreateBookCommand` (nhập sách mới)
- [ ] **Order**: `CreateOrderCommand` (mua hàng, check tồn kho)

---

## 📅 Phase 4: Infrastructure (Persistence & Services)

Kết nối với thế giới bên ngoài (Database, 3rd party services).

### 4.1. Entity Framework Core
- [ ] Cài đặt `EF Core` & `SQL Server` provider
- [ ] Tạo `WriteDbContext`: inherit `DbContext`, implement `IUnitOfWork`
- [ ] Viết **Configurations** (Fluent API) cho từng Entity (map Value Objects dùng `OwnsOne`, config Key, Index)
- [ ] Implement Repositories (`UserRepository`, `BookRepository`)

### 4.2. Services
- [ ] Implement `PasswordHasher` (PBKDF2)
- [ ] Implement `DateTimeProvider` (nếu cần test thời gian)

---

## 📅 Phase 5: Query Layer (CQRS - Read Side)

Tối ưu hóa cho việc đọc dữ liệu.

### 5.1. Setup Read Model
- [ ] Tạo `IReadDbContext` interface (chỉ có `IQueryable`, không có `Add/Update/Delete`)
- [ ] Cho `WriteDbContext` implement interface này

### 5.2. Implement Queries
- [ ] Tạo DTOs (`BookDto`, `OrderDto`) - Phẳng hóa cấu trúc dữ liệu để dễ hiển thị
- [ ] Implement `GetBookByIdQuery` & Handler
- [ ] Implement `SearchBooksQuery` (Filter, Paging, Sorting)

---

## 📅 Phase 6: Public API (Presentation)

Mở cổng cho client giao tiếp.

### 6.1. Setup API
- [ ] Config `Program.cs`: DI, Swagger, Middleware
- [ ] Viết Controllers (`UsersController`, `BooksController`, `OrdersController`)
- [ ] Test thử API bằng Swagger/Postman

---

## 📅 Phase 7: Advanced & Production Ready (Nâng cao)

Những phần làm cho project trở nên "Pro".

### 7.1. Authentication & Authorization
- [ ] Setup **JWT Authentication** (`JwtTokenGenerator`, Auth Middleware)
- [ ] Implement **Permission-based Authorization** (Custom Attributes, Policy Provider)

### 7.2. Performance & Scalability
- [ ] Tích hợp **Redis**: Implement `ICacheService` dùng Redis
- [ ] Tích hợp **Dapper**: Viết Raw SQL cho các query phức tạp cần tốc độ cao

### 7.3. Migrations & Deployment
- [ ] Tạo EF Core Migrations
- [ ] Chạy update database
- [ ] Viết `Docker-compose` (optional) để chạy SQL Server + Redis

---

## 🛠 Lời khuyên khi học

1.  **Đừng copy-paste mù quáng**: Hãy gõ lại code, hoặc ít nhất là đọc hiểu từng dòng tại sao lại viết như vậy.
2.  **Tập trung vào "Tại sao"**: Tại sao lại tách `Domain` và `Application`? Tại sao dùng `ValueObject` cho Email thay vì String?
3.  **Debug**: Chạy debug step-by-step qua luồng `Controller -> MediatR -> Behavior -> Handler -> Repository -> DB` để hiểu luồng đi của dữ liệu.
4.  **Thay đổi yêu cầu**: Tự đặt ra bài toán mới (VD: Thêm tính năng "Wishlist") và tự implement để kiểm chứng kiến thức.

Chúc bạn học tốt! 🚀
