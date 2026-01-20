# Hướng Dẫn Cài Đặt & Chạy BookStation

## Yêu Cầu Hệ Thống (Prerequisites)

Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt:

1.  **.NET 8.0 SDK**: [Tải về tại đây](https://dotnet.microsoft.com/download/dotnet/8.0).
2.  **SQL Server**: LocalDB (đi kèm Visual Studio) hoặc SQL Server Express / Developer Edition.
3.  **IDE**: Visual Studio 2022, Visual Studio Code, hoặc Rider.
4.  **Git**: Để quản lý mã nguồn.

## Các Bước Cài Đặt (Setup Steps)

### 1. Clone Repository
```bash
git clone <repository-url>
cd BookStation-src
```

### 2. Cấu Hình Database (appsettings.json)
Mở file `src/BookStation.PublicApi/appsettings.json`. Tìm phần `ConnectionStrings` và điều chỉnh chuỗi kết nối phù hợp với SQL Server của bạn.

Mặc định:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=BookStationDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 3. Khởi Tạo Cơ Sở Dữ Liệu (Migrations)
Sử dụng Entity Framework Core Tools để tạo database từ các entities.

Mở terminal tại thư mục gốc của solution và chạy:

```bash
# Di chuyển vào thư mục Infrastructure (nơi chứa DbContext)
cd src/BookStation.Infrastructure

# Tạo migration đầu tiên (nếu chưa có)
dotnet ef migrations add InitialCreate --startup-project ../BookStation.PublicApi

# Cập nhật database
dotnet ef database update --startup-project ../BookStation.PublicApi
```

> **Lưu ý**: Nếu bạn gặp lỗi "dotnet ef not found", hãy cài đặt công cụ EF Core global:
> `dotnet tool install --global dotnet-ef`

### 4. Chạy Ứng Dụng

#### Sử dụng Visual Studio:
- Mở `BookStation.sln`.
- Set `BookStation.PublicApi` là **Startup Project**.
- Nhấn `F5` hoặc `Ctrl+F5`.

#### Sử dụng CLI:
```bash
cd src/BookStation.PublicApi
dotnet run
```
Hoặc chạy với chế độ Hot Reload (tự động reload khi sửa code):
```bash
dotnet watch run
```

### 5. Truy Cập Swagger
Sau khi ứng dụng chạy thành công, mở trình duyệt và truy cập:
- URL: `https://localhost:7000/swagger` (Port có thể thay đổi tùy cấu hình launchSettings.json).
- Tại đây bạn có thể xem danh sách API và test thử.

## Cấu Trúc Script Chạy Nhanh (Optional)

Bạn có thể tạo file `run.bat` hoặc `run.ps1` ở thư mục gốc để chạy nhanh:

```powershell
dotnet run --project src/BookStation.PublicApi/BookStation.PublicApi.csproj
```
