# Domain Model BookStation

Tài liệu này mô tả chi tiết các **Bounded Contexts** và **Aggregates** trong hệ thống BookStation.

## Bounded Contexts & Aggregates

Hệ thống được chia thành các vùng nghiệp vụ (Aggregate Roots) sau, nằm trong `BookStation.Domain/Entities`:

### 1. Catalog (Quản lý Danh mục)
Quản lý thông tin về sách và các thuộc tính liên quan.
- **CatalogAggregate**: Chứa thông tin sách (`Book`), biến thể sách (`BookVariant`), tác giả (`Author`), danh mục (`Category`), nhà xuất bản (`Publisher`).
- **Nghiệp vụ**: Quản lý kho (Inventory), phân loại sách.

### 2. User / Identity (Người dùng & Định danh)
Quản lý thông tin tài khoản và phân quyền.
- **UserAggregate**: Đại diện cho người dùng hệ thống (`User`, `Role`).
- **Profiles**: Có thể mở rộng cho `SellerProfile` (người bán) hoặc `ShipperProfile` (người giao hàng).

### 3. Sales / Order (Bán hàng & Đơn hàng)
Quản lý quy trình đặt hàng và thanh toán.
- **OrderAggregate**: Aggregate Root là `Order`.
- **Thành phần**: `OrderItem` (chi tiết đơn hàng), thông tin thanh toán (`Payment`), hoàn tiền (`Refund`).
- **Nghiệp vụ**: Xử lý trạng thái đơn hàng (Pending, Paid, Shipped, Cancelled), áp dụng giảm giá.

### 4. Cart (Giỏ hàng)
Quản lý giỏ hàng tạm thời của người dùng.
- **CartAggregate**: `Cart`, `CartItem`.
- **Nghiệp vụ**: Thêm/sửa/xóa sản phẩm trong giỏ, tính tổng tạm tính.

### 5. Review / Social (Đánh giá & Xã hội)
Quản lý tương tác người dùng với sản phẩm.
- **ReviewAggregate**: `Review` (đánh giá sao, bình luận).
- **Post**: Bài đăng (nếu có).

### 6. Marketing / Voucher
Quản lý khuyến mãi.
- **VoucherAggregate**: `Voucher` (mã giảm giá), điều kiện áp dụng.

### 7. Shipping (Vận chuyển)
Quản lý giao vận.
- **ShipmentAggregate**: `Shipment` (lô hàng), tracking.

### 8. Organization
Quản lý thông tin tổ chức/cửa hàng (nếu là multi-tenant hoặc marketplace).
- **OrganizationAggregate**: `Organization`.

## Common Value Objects

Các Value Objects được sử dụng chung trong toàn bộ Domain (`BookStation.Domain/ValueObjects`):

- **ISBN**: Value Object đặc biệt để validate và lưu trữ mã ISBN (10 hoặc 13 số) của sách. Đảm bảo tính hợp lệ của mã sách quốc tế.
- **Email**: Đảm bảo định dạng email hợp lệ.
- **Address**: Cấu trúc địa chỉ chuẩn (Đường, Phường/Xã, Quận/Huyện, Tỉnh/Thành phố).
- **Money**: Xử lý tiền tệ, bao gồm Số tiền (Amount) và Loại tiền (Currency), tránh lỗi làm tròn số thực.
- **PhoneNumber**: Validate số điện thoại.

## Domain Events

Domain Events được sử dụng để thông báo rằng "một điều gì đó quan trọng đã xảy ra" trong Domain.
Ví dụ:
- `OrderCreatedDomainEvent`: Khi đơn hàng được tạo.
- `PaymentCompletedDomainEvent`: Khi thanh toán thành công.

Các events này được dispatch và xử lý bởi các Event Handlers (thường là trong Application Layer) để thực hiện các side-effects (như gửi email, cập nhật báo cáo) mà không làm ô nhiễm logic chính của Aggregate.
