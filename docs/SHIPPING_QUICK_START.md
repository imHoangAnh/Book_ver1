# Shipping Integration - Quick Start Guide

Hướng dẫn nhanh để bắt đầu với shipping integration.

---

## 🚀 Quick Setup (15 minutes)

### Step 1: Get GHN Credentials (5 mins)
1. Đăng ký: https://sso.ghn.vn/register
2. Tạo shop trong dashboard
3. Lấy **Token** và **Shop ID**
4. **Save to notepad!**

### Step 2: Add to Project (5 mins)

**appsettings.Development.json:**
```json
{
  "Shipping": {
    "GHN": {
      "ApiUrl": "https://dev-online-gateway.ghn.vn/shiip/public-api",
      "Token": "PASTE_YOUR_TOKEN_HERE",
      "ShopId": "PASTE_YOUR_SHOP_ID_HERE"
    }
  }
}
```

### Step 3: Test Connection (5 mins)

Create test endpoint:

```csharp
// In any controller
[HttpGet("test-ghn")]
public async Task<IActionResult> TestGHN()
{
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("Token", Configuration["Shipping:GHN:Token"]);
    httpClient.DefaultRequestHeaders.Add("ShopId", Configuration["Shipping:GHN:ShopId"]);
    
    var response = await httpClient.PostAsJsonAsync(
        "https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee",
        new {
            service_type_id = 2,
            from_district_id = 1442, // Đống Đa
            to_district_id = 1542,   // Ba Đình
            weight = 1000
        }
    );
    
    var result = await response.Content.ReadAsStringAsync();
    return Ok(result);
}
```

Run và test: `GET https://localhost:5001/api/test-ghn`

Expected response:
```json
{
  "code": 200,
  "data": {
    "total": 25000,
    "service_fee": 20000
  }
}
```

✅ **Success!** You're connected to GHN.

---

## 📚 Full Documentation

- **Registration Guide:** [SHIPPING_API_REGISTRATION.md](./SHIPPING_API_REGISTRATION.md)
- **Implementation Plan:** [SHIPPING_INTEGRATION_PLAN.md](./SHIPPING_INTEGRATION_PLAN.md)

---

## 🎯 Next Steps

### Option A: Minimal (Chỉ GHN)
1. Follow [SHIPPING_API_REGISTRATION.md](./SHIPPING_API_REGISTRATION.md) - GHN section
2. Implement basic GHN service
3. Test with real orders

**Time:** 2-3 hours

### Option B: Complete (All 3 providers)
1. Register all providers (GHN, GHTK, ViettelPost)
2. Follow full [SHIPPING_INTEGRATION_PLAN.md](./SHIPPING_INTEGRATION_PLAN.md)
3. Implement factory pattern
4. Add webhooks

**Time:** 1-2 days

### Option C: Later
1. Keep this documentation
2. Focus on other features first
3. Come back when you need shipping

---

## ❓ Common Issues

### "Invalid Token"
- Double check token in appsettings.json
- Make sure no extra spaces
- Token should be ~40 characters

### "Shop not found"
- Verify Shop ID is a number
- Must create shop in GHN dashboard first

### "District not found"
- Use district_id (number), not name
- Get IDs from: `/master-data/district` endpoint

---

## 💡 Tips

1. **Start with GHN** - Easiest to setup and test
2. **Use Dev environment** - Free testing, no charges
3. **Save credentials securely** - Use User Secrets in development
4. **Test small** - Start with fee calculation before creating orders

---

**Ready to implement?** See [SHIPPING_INTEGRATION_PLAN.md](./SHIPPING_INTEGRATION_PLAN.md)
