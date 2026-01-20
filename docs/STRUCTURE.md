# Cấu Trúc Dự Án BookStation

Dự án tuân theo cấu trúc Folder của Clean Architecture. Dưới đây là sơ đồ cây thư mục và giải thích chi tiết.

```
BookStation-src/
├── docs/                           # Tài liệu dự án (Architecture, Setup, Domain...)
├── src/                            # Mã nguồn chính
│   ├── BookStation.Core/           # [Shared Kernel] Chứa các thành phần dùng chung
│   │   ├── Interfaces/             # IUnitOfWork, IRepository...
│   │   └── Base/                   # BaseEntity, ValueObject...
│   │
│   ├── BookStation.Domain/         # [Domain Layer] Logic nghiệp vụ cốt lõi
│   │   ├── Entities/               # Các Aggregate Roots (User, Order, Book...)
│   │   ├── ValueObjects/           # Các Value Objects (ISBN, Email...)
│   │   ├── Enums/                  # Các Enum định nghĩa trạng thái
│   │   └── Repositories/           # Interfaces cho Repositories (IOrderRepository...)
│   │
│   ├── BookStation.Application/    # [Application Layer - Write Side] Xử lý nghiệp vụ
│   │   ├── Commands/               # Các CQRS Commands (CreateOrderCommand...)
│   │   ├── Common/                 # Behaviors, Interfaces, Mappings
│   │   └── DomainServices/         # Các services phối hợp nhiều entities
│   │
│   ├── BookStation.Query/          # [Application Layer - Read Side] Truy vấn dữ liệu
│   │   ├── Queries/                # Các CQRS Queries (GetBookByIdQuery...)
│   │   └── DTOs/                   # Data Transfer Objects cho kết quả trả về
│   │
│   ├── BookStation.Infrastructure/ # [Infrastructure Layer] Triển khai kỹ thuật
│   │   ├── Persistence/            # DbContext, Configurations (EF Core)
│   │   ├── Repositories/           # Implementation của Repository Interfaces
│   │   └── Services/               # Implementation của Services (EmailService...)
│   │
│   └── BookStation.PublicApi/      # [Presentation Layer] API RESTful
│       ├── Controllers/            # API Controllers
│       ├── Middleware/             # Exception Handling, Logging...
│       └── appsettings.json        # Cấu hình ứng dụng
│
├── .gitignore
├── BookStation.sln                 # Solution file
└── README.md                       # Trang chủ tài liệu
```

## Chú Thích Quan Trọng

- **Dependency Rule**: Các dependency chỉ được trỏ từ vòng ngoài vào vòng trong. (Infrastructure -> Application -> Domain). Domain không được biết gì về Infrastructure.
- **Tách Biệt Query**: Project `BookStation.Query` được tách riêng để nhấn mạnh mô hình CQRS. Trong một số dự án khác, nó có thể nằm chung trong Application, nhưng ở đây việc tách riêng giúp code Read/Write rõ ràng hơn.
