# API Registration Guide - Shipping Providers

## 📋 Table of Contents
1. [GHN (Giao Hàng Nhanh)](#ghn-registration)
2. [GHTK (Giao Hàng Tiết Kiệm)](#ghtk-registration)
3. [ViettelPost](#viettelpost-registration)
4. [Testing Credentials](#testing-credentials)
5. [Quick Implementation Guide](#quick-implementation)

---

## 1. GHN (Giao Hàng Nhanh) Registration {#ghn-registration}

### Step 1: Create Account
1. Truy cập: https://sso.ghn.vn/register
2. Điền thông tin:
   - Họ tên
   - Số điện thoại
   - Email
   - Mật khẩu
3. Xác nhận OTP qua SMS
4. Đăng nhập vào dashboard: https://khachhang.ghn.vn/

### Step 2: Create Shop (Cửa hàng)
1. Vào **Cài đặt** → **Thông tin shop**
2. Click **Tạo shop mới**
3. Điền thông tin:
   ```
   Tên shop: BookStation
   Địa chỉ lấy hàng: 54 Nguyễn Lương Bằng, Đống Đa, Hà Nội
   Số điện thoại: 0987654321
   ```
4. **Lưu ý Shop ID** - Cần dùng cho API

### Step 3: Get API Token
1. Vào **Cài đặt** → **Tích hợp API**
2. Click **Tạo Token**
3. **Lưu Token ngay** (chỉ hiển thị 1 lần!)
4. Copy Token vào notepad

### Step 4: Get District/Ward IDs
GHN yêu cầu `district_id` và `ward_code` thay vì tên.

**Cách lấy:**
```http
GET https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/province
Headers:
  Token: your-token-here

→ Lấy danh sách tỉnh/thành

GET https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/district
Headers:
  Token: your-token-here
Body: { "province_id": 202 } // Hà Nội

→ Lấy danh sách quận/huyện với district_id

GET https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/ward
Headers:
  Token: your-token-here
Body: { "district_id": 1442 } // Đống Đa

→ Lấy ward_code
```

**Hoặc dùng tool:** https://github.com/ghn-vn/address-list

### Step 5: Test API
```bash
curl -X POST https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee \
  -H "Token: YOUR_TOKEN" \
  -H "ShopId: YOUR_SHOP_ID" \
  -H "Content-Type: application/json" \
  -d '{
    "service_type_id": 2,
    "from_district_id": 1442,
    "to_district_id": 1542,
    "weight": 1000
  }'
```

### Credentials for appsettings.json
```json
{
  "Shipping": {
    "GHN": {
      "ApiUrl": "https://dev-online-gateway.ghn.vn/shiip/public-api",
      "Token": "YOUR_GHN_TOKEN_HERE",
      "ShopId": "123456"
    }
  }
}
```

---

## 2. GHTK (Giao Hàng Tiết Kiệm) Registration {#ghtk-registration}

### Step 1: Create Account
1. Truy cập: https://khachhang.giaohangtietkiem.vn/register
2. Điền form đăng ký:
   - Họ tên
   - Số điện thoại
   - Email
   - Mật khẩu
3. Xác nhận email

### Step 2: Verify Account (KYC)
1. Đăng nhập: https://khachhang.giaohangtietkiem.vn/
2. Vào **Tài khoản** → **Xác minh tài khoản**
3. Upload:
   - CMND/CCCD mặt trước
   - CMND/CCCD mặt sau
   - Ảnh selfie với CMND
4. Chờ duyệt (1-2 ngày làm việc)

### Step 3: Add Pickup Address
1. Vào **Cài đặt** → **Địa chỉ lấy hàng**
2. Thêm địa chỉ:
   ```
   Tên: Kho BookStation
   Địa chỉ: 54 Nguyễn Lương Bằng, Đống Đa, Hà Nội
   SĐT: 0987654321
   ```
3. Đặt làm địa chỉ mặc định

### Step 4: Get API Token
1. Vào **Cài đặt** → **API**
2. Tab **Token**
3. Click **Lấy token**
4. Copy token (dạng: `xxxxxxxxxxxxxxxxxxxxxxxxxxx`)

### Step 5: Test API
```bash
curl -X POST https://services.giaohangtietkiem.vn/services/shipment/fee \
  -H "Token: YOUR_GHTK_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "pick_province": "Hà Nội",
    "pick_district": "Quận Đống Đa",
    "province": "Hà Nội",
    "district": "Quận Ba Đình",
    "weight": 1000,
    "value": 200000
  }'
```

### Credentials for appsettings.json
```json
{
  "Shipping": {
    "GHTK": {
      "ApiUrl": "https://services.giaohangtietkiem.vn/services",
      "Token": "YOUR_GHTK_TOKEN_HERE"
    }
  }
}
```

**Lưu ý:** GHTK dùng tên tỉnh/quận text thay vì ID

---

## 3. ViettelPost Registration {#viettelpost-registration}

### Step 1: Contact Sales
ViettelPost yêu cầu ký hợp đồng trước.

**Option A: Online Registration**
1. Truy cập: https://viettelpost.vn/dang-ky-tai-khoan
2. Điền form đăng ký doanh nghiệp
3. Đợi gọi lại từ sales (1-3 ngày)

**Option B: Direct Contact**
- Hotline: 1900 8095
- Email: viettelpost.business@viettel.com.vn
- Yêu cầu:
  - Giấy phép kinh doanh
  - Hợp đồng thuê văn phòng/kho

### Step 2: Sign Contract
1. Sales gửi hợp đồng
2. Ký hợp đồng (online hoặc offline)
3. Nộp phí dịch vụ (nếu có)
4. Nhận tài khoản

### Step 3: Get API Credentials
Sau khi ký hợp đồng, bạn nhận:
- **Username** (email hoặc mã khách hàng)
- **Password**
- **Partner Code** (mã đối tác)

### Step 4: Get Access Token
ViettelPost dùng JWT token (expire sau 24h).

```bash
# Login to get token
curl -X POST https://partner.viettelpost.vn/v2/user/Login \
  -H "Content-Type: application/json" \
  -d '{
    "USERNAME": "YOUR_USERNAME",
    "PASSWORD": "YOUR_PASSWORD"
  }'

# Response:
# {
#   "status": 200,
#   "data": {
#     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#     "expired": 86400
#   }
# }
```

### Step 5: Test API
```bash
curl -X POST https://partner.viettelpost.vn/v2/order/getPriceAll \
  -H "Token: YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "SENDER_PROVINCE": 1,
    "SENDER_DISTRICT": 6,
    "RECEIVER_PROVINCE": 1,
    "RECEIVER_DISTRICT": 18,
    "PRODUCT_TYPE": "HH",
    "PRODUCT_WEIGHT": 1000,
    "PRODUCT_PRICE": 200000,
    "MONEY_COLLECTION": 200000,
    "TYPE": 1
  }'
```

### Credentials for appsettings.json
```json
{
  "Shipping": {
    "ViettelPost": {
      "ApiUrl": "https://partner.viettelpost.vn/v2",
      "Username": "YOUR_USERNAME",
      "Password": "YOUR_PASSWORD",
      "PartnerCode": "YOUR_PARTNER_CODE"
    }
  }
}
```

**Lưu ý:** Cần implement token refresh vì JWT expire sau 24h.

---

## 4. Testing Credentials {#testing-credentials}

### GHN Test Environment
```
API URL: https://dev-online-gateway.ghn.vn/shiip/public-api
Test Token: sẽ được cấp khi đăng ký
Test Shop ID: tạo shop test trong dashboard
```

**Đặc điểm:**
- ✅ Không tính phí thật
- ✅ Có tracking code thật
- ⚠️ Không ship thật
- ✅ Webhook hoạt động

### GHTK Test Environment
GHTK không có sandbox riêng, nhưng:
- Tạo đơn trên test account
- Hủy trước khi shipper lấy hàng → không mất phí
- Hoặc set `is_freeship: 1` để test

### ViettelPost Test
- Yêu cầu môi trường test từ sales
- Hoặc dùng production với đơn test rồi hủy

---

## 5. Quick Implementation Guide {#quick-implementation}

### Step 1: Add Secrets to appsettings.json

**Development (appsettings.Development.json):**
```json
{
  "Shipping": {
    "GHN": {
      "ApiUrl": "https://dev-online-gateway.ghn.vn/shiip/public-api",
      "Token": "your-dev-token",
      "ShopId": "123456"
    },
    "GHTK": {
      "ApiUrl": "https://services.giaohangtietkiem.vn/services",
      "Token": "your-ghtk-token"
    },
    "ViettelPost": {
      "ApiUrl": "https://partner.viettelpost.vn/v2",
      "Username": "your-username",
      "Password": "your-password"
    }
  }
}
```

**Production:** Dùng User Secrets hoặc Azure Key Vault
```bash
dotnet user-secrets set "Shipping:GHN:Token" "real-token"
```

### Step 2: Create HTTP Clients

```csharp
// Program.cs
builder.Services.AddHttpClient<GHNService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Shipping:GHN:ApiUrl"]);
    client.DefaultRequestHeaders.Add("Token", builder.Configuration["Shipping:GHN:Token"]);
    client.DefaultRequestHeaders.Add("ShopId", builder.Configuration["Shipping:GHN:ShopId"]);
});

builder.Services.AddHttpClient<GHTKService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Shipping:GHTK:ApiUrl"]);
    client.DefaultRequestHeaders.Add("Token", builder.Configuration["Shipping:GHTK:Token"]);
});

builder.Services.AddHttpClient<ViettelPostService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Shipping:ViettelPost:ApiUrl"]);
});
```

### Step 3: Implement Simple GHN Service

```csharp
public class GHNService
{
    private readonly HttpClient _httpClient;

    public GHNService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> CalculateFee(int weight, int fromDistrict, int toDistrict)
    {
        var response = await _httpClient.PostAsJsonAsync("/v2/shipping-order/fee", new
        {
            service_type_id = 2,
            from_district_id = fromDistrict,
            to_district_id = toDistrict,
            weight = weight
        });

        var result = await response.Content.ReadFromJsonAsync<GHNFeeResponse>();
        return result.Data.Total;
    }
}

public class GHNFeeResponse
{
    public int Code { get; set; }
    public GHNFeeData Data { get; set; }
}

public class GHNFeeData
{
    public decimal Total { get; set; }
}
```

### Step 4: Test
```csharp
// Test endpoint
[HttpGet("test-shipping")]
public async Task<IActionResult> TestShipping([FromServices] GHNService ghn)
{
    var fee = await ghn.CalculateFee(
        weight: 1000,
        fromDistrict: 1442, // Đống Đa, HN
        toDistrict: 1542    // Ba Đình, HN
    );
    
    return Ok(new { fee });
}
```

---

## 🔒 Security Best Practices

### 1. Never Commit Secrets
```gitignore
# Add to .gitignore
appsettings.Development.json
appsettings.Production.json
```

### 2. Use User Secrets (Development)
```bash
dotnet user-secrets init
dotnet user-secrets set "Shipping:GHN:Token" "your-token"
```

### 3. Use Environment Variables (Production)
```bash
# Linux/Docker
export Shipping__GHN__Token="your-token"

# Windows
$env:Shipping__GHN__Token="your-token"
```

### 4. Use Azure Key Vault (Enterprise)
```csharp
builder.Configuration.AddAzureKeyVault(
    vaultUri: new Uri("https://your-vault.vault.azure.net/"),
    credential: new DefaultAzureCredential()
);
```

---

## 📞 Support Contacts

### GHN
- Hotline: 1900 636677
- Email: hotro@ghn.vn
- Docs: https://api.ghn.vn/home/docs

### GHTK
- Hotline: 1900 2157
- Email: hotro@giaohangtietkiem.vn
- Docs: https://docs.giaohangtietkiem.vn/

### ViettelPost
- Hotline: 1900 8095
- Email: viettelpost.business@viettel.com.vn
- Docs: https://viettelpost.vn/huong-dan-tich-hop-api

---

## ✅ Checklist

Before going to production:

- [ ] Register all 3 providers
- [ ] Verify accounts (GHTK KYC, ViettelPost contract)
- [ ] Get API credentials for all
- [ ] Test in sandbox/dev environment
- [ ] Store secrets securely (User Secrets/Key Vault)
- [ ] Configure webhook URLs
- [ ] Add rate limiting
- [ ] Set up monitoring & alerts
- [ ] Document error handling
- [ ] Train support team

---

**Next:** See [SHIPPING_INTEGRATION_PLAN.md](./SHIPPING_INTEGRATION_PLAN.md) for full implementation details.
