# BookStation - E-Commerce Platform

BookStation là một hệ thống thương mại điện tử bán sách hiện đại, được xây dựng dựa trên các tiêu chuẩn kỹ thuật cao cấp: **Clean Architecture**, **CQRS** (Command Query Responsibility Segregation), và **Domain-Driven Design (DDD)**.

Dự án này phục vụ như một template mẫu mực cho việc xây dựng các ứng dụng .NET Enterprise, đảm bảo tính dễ bảo trì, mở rộng và kiểm thử.

## 📚 Tài Liệu Chi Tiết

Hệ thống tài liệu đầy đủ được đặt trong thư mục `docs/`:

- **[Kiến Trúc Hệ Thống (Architecture)](docs/ARCHITECTURE.md)**: Chi tiết về Clean Architecture, phân chia Layer, và CQRS flow.
- **[Cấu Trúc Dự Án (Project Structure)](docs/STRUCTURE.md)**: Giải thích cấu trúc thư mục và các file quan trọng.
- **[Domain Model & Bounded Contexts](docs/DOMAIN.md)**: Chi tiết về các thực thể nghiệp vụ (Catalog, Sales, Identity...).
- **[Hướng Dẫn Cài Đặt (Setup Guide)](docs/SETUP.md)**: Các bước chi tiết để cài đặt môi trường và chạy dự án.

## 🚀 Tính Năng Chính

- **Quản lý Danh mục Sách (Catalog)**: Sách, Tác giả, Thể loại, Kho hàng.
- **Quy trình Đặt hàng (Sales)**: Giỏ hàng, Checkout, quản lý Đơn hàng.
- **Identity & Phân quyền**: Người dùng, Roles, Profiles.
- **Đánh giá & Review**: Hệ thống review sản phẩm.
- **Clean API**: RESTful API với tài liệu Swagger đầy đủ.

## 🛠️ Công Nghệ Sử Dụng

- **Core**: .NET 8.0, C# 12
- **Data**: Entity Framework Core 8, SQL Server
- **Architecture**: Clean Architecture, DDD, CQRS
- **Libraries**: MediatR, FluentValidation, AutoMapper
- **API**: ASP.NET Core Web API, Swagger/OpenAPI

## ⚡ Quick Start

Để chạy nhanh dự án (yêu cầu đã cài .NET 8 SDK và SQL Server):

1. **Clone project**:
   ```bash
   git clone <repo-url>
   cd BookStation-src
   ```

2. **Cập nhật connection string** trong `src/BookStation.PublicApi/appsettings.json`.

3. **Chạy Migration & Start App**:
   ```bash
   cd src/BookStation.Infrastructure
   dotnet ef database update --startup-project ../BookStation.PublicApi
   cd ../BookStation.PublicApi
   dotnet run
   ```

Xem hướng dẫn chi tiết tại **[Setup Guide](docs/SETUP.md)**.

## 📝 TODO Checklist

- [ ] Implement JWT Authentication hoàn chỉnh.
- [ ] Thêm Integration Tests.
- [ ] Cấu hình Redis Caching.
- [ ] Xây dựng Client App (React/Next.js/Blazor).
- [ ] Dockerize ứng dụng.

## 📄 License

MIT License.

---
*BookStation Project - 2026*
