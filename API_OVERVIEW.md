# Tài Liệu API - BookStation (Cập nhật)

Tài liệu này tổng hợp các API chính đã được phát triển, bao gồm luồng Seller mới, hệ thống thanh toán, Review và Quản lý User.

*Note: User mặc định Active ngay khi đăng ký. Organization đã được loại bỏ, mỗi Seller là một Shop độc lập.*

## 1. Authentication & Users
| Method | Endpoint | Mô tả | Yêu cầu Auth |
|--------|----------|-------|--------------|
| `POST` | `/api/auth/register` | Đăng ký người dùng mới (**Active ngay**) | Không |
| `POST` | `/api/auth/login` | Đăng nhập hệ thống (trả về JWT) | Không |
| `GET` | `/api/auth/profile` | Lấy thông tin cá nhân | Có |
| `POST` | `/api/auth/address/new` | Thêm địa chỉ giao hàng mới | Có |

## 2. Admin User Management (Quản lý User - Admin)
*Dành cho Admin để quản lý trạng thái người dùng (Ban, Suspend).*
*Áp dụng cho cả User và Seller (vì Seller cũng là User).*

| Method | Endpoint | Input (Body) | Mô tả |
|--------|----------|--------------|-------|
| `POST` | `/api/users/{id}/ban` | `BanUserRequest` (Reason) | **Ban user vĩnh viễn** nếu vi phạm. Status chuyển sang `Banned`. |
| `POST` | `/api/users/{id}/suspend` | `SuspendUserRequest` (Reason, SuspendUntil) | **Tạm khóa user** đến thời điểm `SuspendUntil`. Status chuyển sang `Suspended`. |
| `POST` | `/api/users/{id}/unban` | - | Mở khóa tài khoản. |

## 3. Seller Management (Quản lý Người bán)
*Dành cho Admin duyệt đơn đăng ký Seller.*

| Method | Endpoint | Mô tả | Yêu cầu Auth |
|--------|----------|-------|--------------|
| `POST` | `/api/sellers/{userId}/approve` | Phê duyệt user trở thành Seller | Admin |
| `POST` | `/api/sellers/{userId}/reject` | Từ chối/Hủy quyền Seller | Admin |

## 4. Book Management (Quản lý Sách & Tìm kiếm)
*API tìm kiếm (Public) và quản lý sách (Seller).*

| Method | Endpoint | Input (Body/Query) | Mô tả |
|--------|----------|--------------------|-------|
| `GET` | `/api/books` | `SearchTerm`, `CategoryId`, `PublisherId`, `MinPrice`, `MaxPrice`, `SortBy`, `PageNumber`... | **Tìm kiếm & Lọc sách**. Hiển thị tất cả sách Active trong hệ thống. |
| `GET` | `/api/books/{id}` | - | Lấy chi tiết sách. |
| `POST` | `/api/books` | `CreateBookCommand` | **Bước 1:** Tạo sách nháp (Draft). |
| `POST` | `/api/books/{id}/variants` | `AddBookVariantCommand` | **Bước 2:** Thêm phiên bản và giá. |
| `POST` | `/api/books/{id}/publish` | - | **Bước 3:** Công khai sách. |

## 5. Product Reviews (Đánh giá)
*User đã mua hàng (Delivered) mới được đánh giá.*

| Method | Endpoint | Input (Body) | Mô tả |
|--------|----------|--------------|-------|
| `POST` | `/api/reviews` | `CreateReviewRequest` (BookId, Rating 1-5, Content, HasSpoiler) | Tạo đánh giá cho sách. Hệ thống sẽ kiểm tra user đã mua sách chưa. |
| `GET` | `/api/reviews/{id}` | - | Xem chi tiết review. |

## 6. Checkout & Orders (Mua hàng)
*Luồng đặt hàng và thanh toán.*

| Method | Endpoint | Input (Body) | Mô tả |
|--------|----------|--------------|-------|
| `GET` | `/api/checkout/vouchers` | `orderAmount`, `sellerId` (Query param) | Lấy danh sách voucher khả dụng. |
| `POST` | `/api/checkout/validate-voucher` | `VoucherCode`, `OrderAmount` | Kiểm tra và tính giảm giá voucher. |
| `POST` | `/api/checkout` | `CheckoutCommand` (Items, AddressId, PaymentMethod...) | **Thực hiện đặt hàng**. |

## 7. Payments (Thanh toán)
*Xử lý giao dịch thanh toán (tự động).*

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| `GET` | `/api/payments/vnpay-return` | Callback URL nhận kết quả từ VNPay. |
| `GET` | `/api/payments/vnpay-ipn` | Instant Payment Notification từ VNPay. |
| `GET` | `/api/payments/{orderId}/status` | Kiểm tra trạng thái thanh toán. |

## 8. Voucher Management (Quản lý Voucher)
*Admin tạo voucher sàn (SellerId=null), Seller tạo voucher shop (SellerId=user.Id).*

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| `GET` | `/api/vouchers` | Lấy danh sách voucher. |
| `POST` | `/api/vouchers` | Tạo voucher mới. Admin để SellerId=null. Seller logic tự gán SellerId. |
| `GET` | `/api/vouchers/{id}` | Lấy chi tiết voucher. |
| `POST` | `/api/vouchers/{id}/status` | Activate/Deactivate voucher. |

---
*Lưu ý: Hệ thống chỉ có 1 Admin duy nhất. Cần login đúng tài khoản Admin để test các API quản lý.*
