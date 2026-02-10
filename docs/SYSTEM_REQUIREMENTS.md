# Yêu cầu Hệ thống và Quy tắc Nghiệp vụ (System Requirements & Business Rules)

Tài liệu này tổng hợp toàn bộ các yêu cầu chức năng, phi chức năng và quy tắc nghiệp vụ đã được triển khai trong hệ thống BookStation cho đến thời điểm hiện tại.

## 1. Yêu cầu Chức năng (Functional Requirements)

### 1.1. Quản lý Người dùng (User Management)
- **Đăng ký (Register)**: Người dùng có thể tạo tài khoản mới với email, mật khẩu, và họ tên.
- **Đăng nhập (Login)**: Xác thực người dùng bằng email và mật khẩu, cấp phát JWT token để truy cập API.
- **Quản lý Hồ sơ (Profile)**:
  - Cập nhật thông tin cá nhân: Họ tên, số điện thoại, ngày sinh, giới tính, bio, avatar.
  - Quản lý địa chỉ giao hàng: Thêm, sửa, xóa, đặt địa chỉ mặc định.
- **Vai trò (Roles)**: Hỗ trợ người dùng thông thường, người bán (Seller), và người giao hàng (Shipper).

### 1.2. Quản lý Sản phẩm (Catalog)
- **Quản lý Sách (Book)**:
  - Tạo sách mới (Draft status).
  - Thêm/Xóa biến thể (Variant) - ví dụ: Bìa mềm, Bìa cứng, Ebook.
  - Thêm/Xóa tác giả (Author) với vai trò cụ thể.
  - Thêm/Xóa danh mục (Category).
  - Xuất bản sách (Publish) - chuyển trạng thái sang Active.
  - Ngừng kinh doanh sách (Discontinue) hoặc đánh dấu hết hàng (Out of Stock).
- **Tìm kiếm & Xem Sách**:
  - Tìm kiếm sách theo từ khóa, danh mục, nhà xuất bản, khoảng giá.
  - Xem chi tiết sách bao gồm thông tin tác giả, nhà xuất bản, các biến thể và giá bán.

### 1.3. Quản lý Giỏ hàng (Cart)
- Mỗi người dùng có một giỏ hàng riêng.
- Thêm sản phẩm (biến thể sách) vào giỏ với số lượng cụ thể.
- Cập nhật số lượng sản phẩm trong giỏ.
- Xóa sản phẩm khỏi giỏ.
- Tính tổng tiền tạm tính của giỏ hàng.

### 1.4. Quản lý Đơn hàng (Order)
- **Đặt hàng (Place Order)**: Tạo đơn hàng từ các sản phẩm đã chọn, áp dụng địa chỉ giao hàng và thêm ghi chú.
- **Xử lý Đơn hàng**:
  - Xác nhận đơn hàng (Confirm): Chuyển trạng thái từ Pending sang Confirmed.
  - Bắt đầu xử lý (Start Processing): Chuyển sang Processing.
  - Giao hàng (Ship): Chuyển sang Shipped.
  - Hoàn tất (Deliver): Chuyển sang Delivered.
  - Hủy đơn (Cancel): Người dùng hoặc hệ thống có thể hủy đơn khi chưa giao thành công.
- **Thanh toán**: Hỗ trợ ghi nhận thanh toán cho đơn hàng.

### 1.5. Khuyến mãi (Voucher)
- **Quản lý Voucher**: Tạo voucher giảm giá theo phần trăm hoặc số tiền cố định.
- **Áp dụng Voucher**: Người dùng có thể áp dụng mã giảm giá vào đơn hàng nếu thỏa mãn điều kiện.

### 1.6. Đánh giá (Review)
- Người dùng có thể viết đánh giá cho sách (Rating 1-5 sao, nội dung text).
- Đánh giá có thể ẩn danh hoặc chứa spoiler.
- Hệ thống hỗ trợ duyệt (Approve) hoặc từ chối (Reject) đánh giá.
- Người dùng khác có thể bình chọn hữu ích (Helpful Vote) cho đánh giá.

---

## 2. Quy tắc Nghiệp vụ (Business Rules)

### 2.1. User (Người dùng)
- **Email**: Phải chuẩn định dạng và là duy nhất trong hệ thống.
- **Mật khẩu**: Được băm (hashed) trước khi lưu trữ (sử dụng BCrypt).
- **Trạng thái**:
  - Mới tạo -> Active (mặc định hiện tại) hoặc Pending Verification (tùy cấu hình).
  - Có thể bị Ban hoặc Suspend.
  - Khi đổi Email, trạng thái Verified bị reset.

### 2.2. Book (Sách đơn - Single)
- **Trạng thái**: Draft -> Active (Published) -> Inactive / OutOfStock / Discontinued.
- **Xuất bản**: Chỉ được phép Publish khi sách đã có ít nhất một biến thể (Variant).
- **Tác giả**: Mỗi tác giả gán vào sách có vai trò cụ thể (Author, Editor, Translator, etc.).
- **Biến thể vật lý (Variant)**: Bìa mềm, Bìa cứng, Ebook, Audiobook...
  - Tên biến thể phải duy nhất trong phạm vi sách đó.
  - Giá bán phải > 0.
  - SKU (mã kho) nên là duy nhất trong hệ thống.
- **Ví dụ**: "Đắc Nhân Tâm" với 2 biến thể: Bìa mềm (80k), Bìa cứng (120k).

### 2.3. BookBundle (Bộ sách & Combo)
Hệ thống hỗ trợ **2 loại Bundle** do Seller tạo:

| Loại | Mô tả | Ví dụ |
|------|-------|-------|
| **BundleSet** | Bộ sách cố định, khách mua nguyên bộ | "Bộ Harry Potter 7 tập" |
| **Combo** | Combo khuyến mãi, khách chọn sách từ danh sách | "Chọn 3 cuốn bất kỳ, giảm 20%" |

#### 2.3.1. BundleSet (Bộ sách cố định)
- **Seller tạo** bộ sách gồm nhiều cuốn cụ thể.
- Khách hàng mua **nguyên bộ**, không chọn từng cuốn.
- Phải chứa **ít nhất 2 sách**.
- Các sách trong bộ phải đều **Active** và **còn hàng**.
- **Giá bộ** có thể thấp hơn tổng giá lẻ (ưu đãi khi mua bộ).
- Khi một sách trong bộ hết hàng/Inactive -> Bộ tự động **Unavailable**.
- **Tồn kho** = MIN(tồn kho của các sách thành phần).
- **Ví dụ**: "Bộ Harry Potter 7 tập" = 500k (tiết kiệm 100k so với mua lẻ).

#### 2.3.2. Combo (Combo khuyến mãi)
- **Seller tạo** chương trình combo với:
  - `EligibleBooks`: Danh sách các sách được tham gia combo.
  - `RequiredQuantity`: Số lượng sách cần mua để được giảm (ví dụ: 3).
  - `DiscountType`: Giảm theo % hoặc số tiền cố định.
  - `DiscountValue`: Giá trị giảm.
  - `StartDate`, `EndDate`: Thời gian hiệu lực.
- Khách hàng **chọn sách từ danh sách** Seller đã định sẵn.
- Giảm giá tự động áp dụng khi khách chọn đủ số sách yêu cầu.
- **Ví dụ**: "Combo 3 cuốn tự chọn - Giảm 20%" (chọn 3 từ 20 cuốn tham gia).

#### 2.3.3. Quy tắc chung cho Bundle
- **Tên Bundle** phải duy nhất trong phạm vi Seller.
- Chỉ Seller sở hữu các sách mới được thêm vào Bundle.
- Bundle có thể **Active/Inactive** độc lập với sách thành phần.
- Khi xóa sách khỏi hệ thống, cần kiểm tra và cập nhật các Bundle liên quan.

### 2.3. Order (Đơn hàng)
- **Luồng trạng thái chặt chẽ**:
  - `Pending` -> `Confirmed` -> `Processing` -> `Shipped` -> `Delivered`.
  - `Cancelled`: Chỉ được hủy khi ở trạng thái `Pending`, `Confirmed`, hoặc `Processing`. Đơn đã giao (`Delivered`) không thể hủy.
- **Chỉnh sửa đơn hàng**: Chỉ được phép thêm/bớt sản phẩm hoặc voucher khi đơn ở trạng thái `Pending`.
- **Tổng tiền**:
  - `TotalAmount` = Tổng tiền các món hàng.
  - `FinalAmount` = `TotalAmount` - `DiscountAmount`. Không được âm.
- **Thanh toán**: Đơn hàng được coi là thanh toán đủ khi tổng tiền đã trả (`TotalPaid`) >= `FinalAmount`.

### 2.4. Voucher (Mã giảm giá)
- **Hiệu lực**:
  - Phải trong khoảng thời gian `StartDate` đến `EndDate`.
  - `UsageLimit`: Nếu > 0, tổng số lần sử dụng không được vượt quá giới hạn.
  - `IsActive`: Phải true.
- **Điều kiện áp dụng**: Tổng tiền đơn hàng phải >= `MinOrderAmount`.
- **Giới hạn người dùng**: Mỗi người dùng chỉ được sử dụng một voucher cụ thể một lần duy nhất.
- **Tính toán giảm giá**:
  - Giảm theo %: Không vượt quá `MaxDiscountAmount` (nếu có).
  - Giảm tiền mặt: Không vượt quá tổng tiền đơn hàng.

### 2.5. Cart (Giỏ hàng)
- **Số lượng**: Nếu cập nhật số lượng <= 0, sản phẩm sẽ tự động bị xóa khỏi giỏ.
- **Cập nhật**: Thời gian `LastActivityAt` được cập nhật mỗi khi có thay đổi trong giỏ.

### 2.6. Review (Đánh giá)
- **Rating**: Bắt buộc từ 1 đến 5.
- **Trạng thái**: Mặc định là `Approved` ngay khi tạo (có thể thay đổi logic kiểm duyệt sau này).

---

## 3. Yêu cầu Phi chức năng (Non-functional Requirements)

### 3.1. Công nghệ & Kiến trúc
- **Backend**: .NET 8 / C#.
- **Kiến trúc**: Clean Architecture + CQRS (Command Query Responsibility Segregation).
  - **Commands**: Sử dụng MediatR Handlers, thay đổi trạng thái hệ thống qua Entity Framework Core.
  - **Queries**: Sử dụng `IReadDbContext` (projection trực tiếp) hoặc Dapper (cho các query phức tạp/hiệu năng cao).
- **Database**: Microsoft SQL Server.

### 3.2. Hiệu năng & Caching
- **Caching**: Sử dụng Redis cho Distributed Caching (có fallback về In-Memory Cache nếu Redis chưa cấu hình).
- **Database Optimization**: Sử dụng `AsNoTracking()` cho các truy vấn đọc để tăng tốc độ.

### 3.3. Bảo mật (Security)
- **Authentication**: JWT (JSON Web Tokens). Access Token có thời hạn ngắn, Refresh Token dùng để cấp lại Access Token.
- **Password**: Sử dụng thuật toán BCrypt để băm mật khẩu, không lưu plain text.
- **Validation**: Đầu vào API được kiểm tra chặt chẽ (FluentValidation) trước khi xử lý.

### 3.4. Tích hợp bên thứ 3 (Integrations)
- **Lưu trữ ảnh**: Cloudinary.
- **Thanh toán**: VNPay.
- **Message Broker**: (Dự kiến/Có sẵn hạ tầng) Có cấu hình cho Event Publishing (MediatR notifications trong process).

### 3.5. Mã nguồn & Chất lượng
- **Domain-Driven Design (DDD)**: Logic nghiệp vụ tập trung trong Domain Entity (Aggregate Root, Entity, Value Object).
- **Mã lỗi**: Sử dụng chuẩn `Result<T>` pattern để trả về lỗi thay vì ném Exceptions không kiểm soát.
