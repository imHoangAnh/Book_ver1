# Kiến Trúc Hệ Thống BookStation

## Tổng Quan

Dự án BookStation được thiết kế dựa trên các nguyên tắc của **Clean Architecture** (Kiến trúc Sạch), kết hợp với **Domain-Driven Design (DDD)** và **CQRS** (Command Query Responsibility Segregation).

Mục tiêu chính của kiến trúc này là tạo ra một hệ thống:
- **Độc lập với Framework**: Kiến trúc không phụ thuộc vào sự tồn tại của một thư viện feature-laden nào.
- **Có thể Test được**: Business rules có thể được test mà không cần UI, Database, Web Server, hay bất kỳ thành phần bên ngoài nào.
- **Độc lập với UI**: UI có thể thay đổi dễ dàng mà không ảnh hưởng đến phần còn lại của hệ thống.
- **Độc lập với Database**: Có thể đổi từ SQL Server sang Mongo, BigTable, CouchDB, etc., mà không ảnh hưởng đến business rules.
- **Độc lập với Agent bên ngoài**: Business rules không biết gì về interfaces của thế giới bên ngoài.

## Các Layer (Tầng)

Dự án được chia thành 4 layer chính, đi từ trong ra ngoài:

### 1. Domain Layer (`BookStation.Domain`)
Đây là trái tim của phần mềm. Nó chứa các logic nghiệp vụ cốt lõi (Enterprise Business Rules).
- **Thành phần**: Entities, Aggregates, Value Objects, Domain Events, Repository Interfaces, Domain Services.
- **Phụ thuộc**: Không phụ thuộc vào bất kỳ layer nào khác.

### 2. Application Layer (`BookStation.Application` & `BookStation.Query`)
Layer này chứa các logic ứng dụng (Application Business Rules). Nó điều phối luồng dữ liệu đến và đi từ các entities trong Domain layer.
- **Thành phần**:
    - **Commands & Handlers** (Write side): Thực hiện các thay đổi trạng thái hệ thống.
    - **Queries & Handlers** (Read side): Thực hiện việc truy vấn dữ liệu (nằm trong `BookStation.Query`).
    - **Interfaces**: Định nghĩa các cổng (Ports) cho Infrastructure.
    - **DTOs**: Data Transfer Objects.
- **Phụ thuộc**: Chỉ phụ thuộc vào Domain Layer.

### 3. Infrastructure Layer (`BookStation.Infrastructure`)
Layer này triển khai các chi tiết kỹ thuật mà Application Layer cần thông qua các interfaces.
- **Thành phần**: Entity Framework Core DbContext, Repositories Implementation, Services Implementation (Email, File Storage...), Identity Services.
- **Phụ thuộc**: Phụ thuộc vào Domain và Application Layer.

### 4. Presentation Layer (`BookStation.PublicApi`)
Đây là layer giao tiếp với người dùng hoặc các hệ thống khác.
- **Thành phần**: API Controllers, Middleware, Filters, Swagger Configurations.
- **Phụ thuộc**: Phụ thuộc vào Application Layer (để gửi Commands/Queries) và Infrastructure Layer (để Dependency Injection).

## Shared Kernel (`BookStation.Core`)
Một dự án phụ chứa các thành phần dùng chung giữa các layer, thường là các helper classes, extensions, hoặc base classes cơ bản mà không chứa logic nghiệp vụ đặc thù.

## CQRS Pattern

BookStation áp dụng CQRS để tách biệt rõ ràng giữa việc **Ghi (Write)** và **Đọc (Read)** dữ liệu.

- **Write Side (Commands)**:
    - Sử dụng Entity Framework Core với Domain Entities.
    - Đảm bảo tính toàn vẹn dữ liệu và thực thi business rules.
    - Repository Pattern được sử dụng để truy xuất aggregates.

- **Read Side (Queries)**:
    - Có thể sử dụng Dapper hoặc Linq projection trực tiếp để tối ưu hiệu năng.
    - Trả về DTOs (Data Transfer Objects) thay vì Domain Entities.
    - Không thay đổi trạng thái hệ thống.

## Luồng Dữ Liệu (Data Flow)

1. **Request** đi vào từ API Controller.
2. Controller tạo ra **Command** hoặc **Query** object.
3. Controller gửi object đó thông qua **Mediator** (thư viện MediatR).
4. **Handler** tương ứng (trong Application/Query layer) nhận xử lý.
    - Nếu là Command: Handler gọi Domain/Repository để thực hiện logic, load Aggregate, thay đổi state, và save changes.
    - Nếu là Query: Handler truy vấn DB và mapped ra DTO để trả về.
5. Kết quả được trả về cho Controller và sau đó là Client.
