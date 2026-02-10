# Third-Party Shipping Integration Plan

## Overview
Integrate multiple shipping providers (GHN, GHTK, ViettelPost) into BookStation using Strategy Pattern and abstraction layers.

## 📖 Prerequisites

**Before starting implementation, you MUST:**
1. Register accounts and get API credentials from providers
2. See detailed guide: **[SHIPPING_API_REGISTRATION.md](./SHIPPING_API_REGISTRATION.md)**

This document contains:
- ✅ Step-by-step registration for GHN, GHTK, ViettelPost
- ✅ How to get API keys and tokens
- ✅ Quick implementation examples
- ✅ Security best practices

---

## Architecture Design

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌────────────────────────────────────────────────────┐    │
│  │         IShippingService (Interface)                │    │
│  │  - CreateShipment()                                 │    │
│  │  - GetShippingFee()                                 │    │
│  │  - TrackShipment()                                  │    │
│  │  - CancelShipment()                                 │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                          ▲
                          │ implements
                          │
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐  │
│  │ GHNService   │  │ GHTKService  │  │ ViettelService  │  │
│  └──────────────┘  └──────────────┘  └─────────────────┘  │
│         │                  │                   │            │
│         └──────────────────┴───────────────────┘            │
│                            │                                │
│                   ┌────────────────────┐                    │
│                   │ ShippingFactory    │                    │
│                   │ (Get provider)     │                    │
│                   └────────────────────┘                    │
└─────────────────────────────────────────────────────────────┘
```

---

## Step-by-Step Implementation

### Phase 1: Core Abstractions & Domain Models ✅

#### 1.1 Create Shipping Value Objects
**Location:** `src/BookStation.Domain/ValueObjects/`

```csharp
// ShippingAddress.cs
public record ShippingAddress(
    string RecipientName,
    string Phone,
    string Street,
    string Ward,
    string District,
    string City,
    string Country = "Vietnam"
);

// ShippingPackage.cs
public record ShippingPackage(
    int Weight,      // grams
    int Length,      // cm
    int Width,       // cm
    int Height,      // cm
    decimal CodAmount,
    string Note
);
```

#### 1.2 Create Shipping Enums
**Location:** `src/BookStation.Domain/Enums/ShippingEnums.cs`

```csharp
public enum EShippingProvider
{
    GHN = 1,
    GHTK = 2,
    ViettelPost = 3
}

public enum EShippingServiceType
{
    Standard = 1,    // Thường
    Express = 2,     // Hỏa tốc
    SuperExpress = 3 // Siêu tốc
}
```

#### 1.3 Update Shipment Entity
**Location:** `src/BookStation.Domain/Entities/ShipmentAggregate/Shipment.cs`

Add properties:
```csharp
public EShippingProvider? Provider { get; private set; }
public decimal ShippingFee { get; private set; }
public string? ProviderOrderCode { get; private set; } // Mã đơn của GHN/GHTK

public void SetProvider(EShippingProvider provider, string providerOrderCode, decimal fee)
{
    Provider = provider;
    ProviderOrderCode = providerOrderCode;
    ShippingFee = fee;
    UpdatedAt = DateTime.UtcNow;
}
```

---

### Phase 2: Application Layer Interfaces 🔧

#### 2.1 Create DTOs
**Location:** `src/BookStation.Application/Shipping/DTOs/`

```csharp
// ShippingOrderRequest.cs
public record CreateShippingOrderRequest(
    long OrderId,
    EShippingProvider Provider,
    EShippingServiceType ServiceType,
    ShippingAddress FromAddress,
    ShippingAddress ToAddress,
    ShippingPackage Package,
    List<ShippingItem> Items
);

public record ShippingItem(
    string Name,
    int Quantity,
    decimal Price
);

// ShippingOrderResponse.cs
public record CreateShippingOrderResponse(
    string ProviderOrderCode,
    string TrackingCode,
    decimal Fee,
    DateTime ExpectedDeliveryDate,
    string Message
);

// ShippingFeeRequest.cs
public record CalculateShippingFeeRequest(
    EShippingProvider Provider,
    string FromDistrictId,
    string ToDistrictId,
    int Weight,
    EShippingServiceType ServiceType
);

// TrackingResponse.cs
public record TrackingResponse(
    string TrackingCode,
    EShipmentStatus CurrentStatus,
    string StatusDescription,
    List<TrackingHistory> History
);

public record TrackingHistory(
    DateTime Timestamp,
    string Status,
    string Location,
    string Description
);
```

#### 2.2 Create Service Interface
**Location:** `src/BookStation.Application/Shipping/IShippingService.cs`

```csharp
public interface IShippingService
{
    /// <summary>
    /// Calculate shipping fee
    /// </summary>
    Task<decimal> CalculateShippingFeeAsync(
        CalculateShippingFeeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create shipping order with provider
    /// </summary>
    Task<CreateShippingOrderResponse> CreateShippingOrderAsync(
        CreateShippingOrderRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Track shipment status
    /// </summary>
    Task<TrackingResponse> TrackShipmentAsync(
        string trackingCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel shipping order
    /// </summary>
    Task<bool> CancelShippingOrderAsync(
        string providerOrderCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available services for a route
    /// </summary>
    Task<List<ShippingServiceInfo>> GetAvailableServicesAsync(
        string fromDistrictId,
        string toDistrictId,
        CancellationToken cancellationToken = default);
}

public record ShippingServiceInfo(
    EShippingServiceType Type,
    string Name,
    decimal BaseFee,
    int ExpectedDays
);
```

---

### Phase 3: Infrastructure Implementation 🏗️

#### 3.1 GHN Service Implementation
**Location:** `src/BookStation.Infrastructure/ExternalServices/GHN/GHNService.cs`

```csharp
public class GHNService : IShippingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly string _apiUrl;
    private readonly string _token;
    private readonly int _shopId;

    public GHNService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
        _apiUrl = config["Shipping:GHN:ApiUrl"];
        _token = config["Shipping:GHN:Token"];
        _shopId = int.Parse(config["Shipping:GHN:ShopId"]);
    }

    public async Task<decimal> CalculateShippingFeeAsync(
        CalculateShippingFeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var ghnRequest = new
        {
            service_type_id = MapServiceType(request.ServiceType),
            from_district_id = int.Parse(request.FromDistrictId),
            to_district_id = int.Parse(request.ToDistrictId),
            weight = request.Weight,
            insurance_value = 0
        };

        var response = await PostAsync<GHNFeeResponse>(
            "/v2/shipping-order/fee",
            ghnRequest,
            cancellationToken);

        return response.Data.Total;
    }

    public async Task<CreateShippingOrderResponse> CreateShippingOrderAsync(
        CreateShippingOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var ghnRequest = new
        {
            payment_type_id = 2, // Người nhận thanh toán
            required_note = "KHONGCHOXEMHANG",
            service_type_id = MapServiceType(request.ServiceType),
            to_name = request.ToAddress.RecipientName,
            to_phone = request.ToAddress.Phone,
            to_address = request.ToAddress.Street,
            to_ward_code = request.ToAddress.Ward,
            to_district_id = int.Parse(request.ToAddress.District),
            cod_amount = (int)request.Package.CodAmount,
            weight = request.Package.Weight,
            length = request.Package.Length,
            width = request.Package.Width,
            height = request.Package.Height,
            items = request.Items.Select(i => new
            {
                name = i.Name,
                quantity = i.Quantity,
                price = (int)i.Price
            }).ToList()
        };

        var response = await PostAsync<GHNCreateOrderResponse>(
            "/v2/shipping-order/create",
            ghnRequest,
            cancellationToken);

        return new CreateShippingOrderResponse(
            response.Data.OrderCode,
            response.Data.OrderCode, // GHN uses same code for tracking
            response.Data.TotalFee,
            response.Data.ExpectedDeliveryTime,
            "Order created successfully"
        );
    }

    private async Task<T> PostAsync<T>(
        string endpoint,
        object data,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl + endpoint);
        request.Headers.Add("Token", _token);
        request.Headers.Add("ShopId", _shopId.ToString());
        request.Content = JsonContent.Create(data);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private int MapServiceType(EShippingServiceType type) => type switch
    {
        EShippingServiceType.Standard => 2,
        EShippingServiceType.Express => 1,
        _ => 2
    };
}
```

#### 3.2 Create Shipping Factory
**Location:** `src/BookStation.Infrastructure/ExternalServices/ShippingFactory.cs`

```csharp
public class ShippingFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ShippingFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IShippingService GetService(EShippingProvider provider)
    {
        return provider switch
        {
            EShippingProvider.GHN => _serviceProvider.GetRequiredService<GHNService>(),
            EShippingProvider.GHTK => _serviceProvider.GetRequiredService<GHTKService>(),
            EShippingProvider.ViettelPost => _serviceProvider.GetRequiredService<ViettelPostService>(),
            _ => throw new NotSupportedException($"Provider {provider} is not supported")
        };
    }
}
```

---

### Phase 4: Application Commands & Handlers 📝

#### 4.1 Create Commands
**Location:** `src/BookStation.Application/Shipping/Commands/ShippingCommands.cs`

```csharp
// Calculate shipping fee for multiple providers
public record CalculateShippingFeesCommand(
    long OrderId,
    string ToDistrictId,
    int Weight
) : IRequest<List<ShippingFeeOption>>;

public record ShippingFeeOption(
    EShippingProvider Provider,
    EShippingServiceType ServiceType,
    decimal Fee,
    int ExpectedDays
);

// Create shipment with selected provider
public record CreateShipmentCommand(
    long OrderId,
    EShippingProvider Provider,
    EShippingServiceType ServiceType
) : IRequest<CreateShipmentResponse>;

public record CreateShipmentResponse(
    long ShipmentId,
    string ProviderOrderCode,
    string TrackingCode,
    decimal Fee
);
```

#### 4.2 Create Handlers
**Location:** `src/BookStation.Application/Shipping/Commands/ShippingCommandHandlers.cs`

```csharp
public class CreateShipmentCommandHandler 
    : IRequestHandler<CreateShipmentCommand, CreateShipmentResponse>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IShipmentRepository _shipmentRepo;
    private readonly ShippingFactory _shippingFactory;

    public async Task<CreateShipmentResponse> Handle(
        CreateShipmentCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get order
        var order = await _orderRepo.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException("Order not found");

        // 2. Create shipment entity
        var shipment = Shipment.Create(request.OrderId, DateTime.UtcNow.AddDays(3));
        await _shipmentRepo.AddAsync(shipment, cancellationToken);

        // 3. Get shipping service
        var shippingService = _shippingFactory.GetService(request.Provider);

        // 4. Create shipping order with provider
        var shippingRequest = new CreateShippingOrderRequest(
            order.Id,
            request.Provider,
            request.ServiceType,
            FromAddress: GetSellerAddress(order.SellerId),
            ToAddress: order.ShippingAddress,
            Package: new ShippingPackage(
                Weight: CalculateWeight(order),
                Length: 30, Width: 20, Height: 10,
                CodAmount: order.TotalAmount,
                Note: order.Note
            ),
            Items: order.Items.Select(i => new ShippingItem(
                i.ProductName, i.Quantity, i.Price
            )).ToList()
        );

        var providerResponse = await shippingService.CreateShippingOrderAsync(
            shippingRequest,
            cancellationToken);

        // 5. Update shipment with provider info
        shipment.SetProvider(
            request.Provider,
            providerResponse.ProviderOrderCode,
            providerResponse.Fee);
        shipment.SetTrackingCode(providerResponse.TrackingCode);

        return new CreateShipmentResponse(
            shipment.Id,
            providerResponse.ProviderOrderCode,
            providerResponse.TrackingCode,
            providerResponse.Fee
        );
    }
}
```

---

### Phase 5: Webhook Handlers 🔔

#### 5.1 Create Webhook Controller
**Location:** `src/BookStation.PublicApi/Controllers/WebhooksController.cs`

```csharp
[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost("ghn")]
    public async Task<IActionResult> GHNWebhook([FromBody] GHNWebhookPayload payload)
    {
        // Verify signature
        // Process status update
        var command = new UpdateShipmentStatusCommand(
            payload.OrderCode,
            MapGHNStatus(payload.Status)
        );
        await _mediator.Send(command);
        return Ok();
    }
}
```

---

### Phase 6: Configuration ⚙️

#### 6.1 appsettings.json
```json
{
  "Shipping": {
    "GHN": {
      "ApiUrl": "https://dev-online-gateway.ghn.vn/shiip/public-api",
      "Token": "your-ghn-token",
      "ShopId": "your-shop-id"
    },
    "GHTK": {
      "ApiUrl": "https://services.giaohangtietkiem.vn",
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

#### 6.2 DI Registration
**Location:** `src/BookStation.PublicApi/Program.cs`

```csharp
// Register shipping services
builder.Services.AddHttpClient<GHNService>();
builder.Services.AddHttpClient<GHTKService>();
builder.Services.AddHttpClient<ViettelPostService>();
builder.Services.AddScoped<ShippingFactory>();
```

---

## Testing Strategy

### Unit Tests
- Test each provider service with mocked HttpClient
- Test shipping factory provider selection
- Test command handlers

### Integration Tests
- Test with GHN sandbox API
- Test webhook processing

---

## Deployment Checklist

- [ ] Register accounts with GHN, GHTK, ViettelPost
- [ ] Get API credentials
- [ ] Configure webhook URLs
- [ ] Test in sandbox/dev environment
- [ ] Setup monitoring & alerts
- [ ] Document API usage & limits

---

**Next Steps:** Would you like me to implement this step-by-step?
