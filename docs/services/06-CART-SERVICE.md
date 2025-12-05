# Cart Service (.NET 10)

## Overview
Shopping cart service built with .NET 10. Manages user shopping carts, cart items, pricing calculations, and cart persistence. Optimized for high performance with Redis caching.

## Technology Stack
- **Language**: .NET 10 (C#)
- **Framework**: ASP.NET Core Web API
- **Primary Storage**: Redis (in-memory)
- **Database**: PostgreSQL (backup/persistence)
- **ORM**: Entity Framework Core 10 (Code-First)
- **Cache**: Redis
- **Messaging**: Kafka (Producer)
- **API**: REST + gRPC
- **Testing**: xUnit, Moq

## Core Responsibilities

### 1. **Cart Management**
- Create/retrieve user carts
- Add items to cart
- Update item quantities
- Remove items from cart
- Clear cart
- Merge carts (anonymous → authenticated user)
- Cart expiration and cleanup

### 2. **Cart Calculations**
- Calculate item subtotals
- Calculate cart total
- Apply discounts and promotions
- Calculate taxes (per region)
- Multi-currency support
- Shipping cost estimation

### 3. **Cart Persistence**
- Redis for active carts (fast access)
- PostgreSQL for long-term persistence
- Auto-save cart state
- Cart recovery after expiration

### 4. **Inventory Integration**
- Check product availability
- Validate quantities against stock
- Real-time stock validation
- Handle out-of-stock items

### 5. **Pricing Integration**
- Fetch current product prices
- Apply regional pricing
- Currency conversion
- Promotional pricing

### 6. **Cart Sharing**
- Save for later functionality
- Share cart via link
- Cart templates for repeat orders

### 7. **Event Publishing**
- Cart created events
- Item added/removed events
- Cart abandoned events
- Checkout initiated events

### 8. **Multi-Region Support**
- Regional cart persistence
- Currency-specific pricing
- Regional tax calculations

## Clean Architecture Structure

```
CartService/
├── src/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── Cart.cs
│   │   │   ├── CartItem.cs
│   │   │   └── SavedCart.cs
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs
│   │   │   ├── Price.cs
│   │   │   └── Quantity.cs
│   │   ├── Enums/
│   │   │   ├── CartStatus.cs
│   │   │   └── ItemType.cs
│   │   ├── Events/
│   │   │   ├── CartCreatedEvent.cs
│   │   │   ├── ItemAddedEvent.cs
│   │   │   └── CheckoutInitiatedEvent.cs
│   │   └── Repositories/
│   │       ├── ICartRepository.cs
│   │       └── ICartCacheRepository.cs
│   │
│   ├── Application/
│   │   ├── DTOs/
│   │   │   ├── Requests/
│   │   │   │   ├── AddItemRequest.cs
│   │   │   │   ├── UpdateQuantityRequest.cs
│   │   │   │   └── ApplyCouponRequest.cs
│   │   │   └── Responses/
│   │   │       ├── CartResponse.cs
│   │   │       ├── CartSummaryResponse.cs
│   │   │       └── PriceBreakdownResponse.cs
│   │   ├── UseCases/
│   │   │   ├── GetCartUseCase.cs
│   │   │   ├── AddItemUseCase.cs
│   │   │   ├── UpdateQuantityUseCase.cs
│   │   │   ├── RemoveItemUseCase.cs
│   │   │   ├── CalculateTotalsUseCase.cs
│   │   │   └── MergeCartsUseCase.cs
│   │   ├── Interfaces/
│   │   │   ├── ICartService.cs
│   │   │   ├── IPricingService.cs
│   │   │   └── IInventoryClient.cs
│   │   └── Validators/
│   │       ├── AddItemValidator.cs
│   │       └── UpdateQuantityValidator.cs
│   │
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── CartDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── CartConfiguration.cs
│   │   │   │   └── CartItemConfiguration.cs
│   │   │   └── Repositories/
│   │   │       └── CartRepository.cs
│   │   ├── Cache/
│   │   │   ├── RedisCartRepository.cs
│   │   │   └── CacheKeys.cs
│   │   ├── ExternalServices/
│   │   │   ├── ProductServiceClient.cs
│   │   │   ├── InventoryServiceClient.cs
│   │   │   └── PricingServiceClient.cs
│   │   ├── Messaging/
│   │   │   └── KafkaProducer.cs
│   │   └── gRPC/
│   │       ├── Protos/
│   │       │   └── cart.proto
│   │       └── Services/
│   │           └── CartGrpcService.cs
│   │
│   └── Api/
│       ├── Controllers/
│       │   ├── CartController.cs
│       │   └── HealthController.cs
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── CartContextMiddleware.cs
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   ├── Domain.Tests/
│   ├── Application.Tests/
│   └── Api.Tests/
│
├── Migrations/
│   └── 20250101000001_InitialCreate.cs
│
├── CartService.csproj
└── Dockerfile
```

## Domain Models (Code-First)

### Cart Entity
```csharp
public class Cart : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid? UserId { get; set; }  // Null for anonymous carts
    public string SessionId { get; set; }  // For anonymous users

    public CartStatus Status { get; set; }
    public string Currency { get; set; }  // USD, EUR, THB, etc.
    public string Region { get; set; }

    // Navigation
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    // Pricing
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingEstimate { get; set; }
    public decimal Total { get; set; }

    public string CouponCode { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public enum CartStatus
{
    Active,
    Abandoned,
    CheckedOut,
    Expired
}
```

### CartItem Entity
```csharp
public class CartItem : BaseEntity
{
    public Guid CartItemId { get; set; }
    public Guid CartId { get; set; }

    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public string SKU { get; set; }

    // Product details (cached)
    public string ProductName { get; set; }
    public string ProductImage { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }

    public bool IsAvailable { get; set; }

    // Navigation
    public Cart Cart { get; set; }

    public DateTime AddedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### SavedCart Entity (Save for Later)
```csharp
public class SavedCart : BaseEntity
{
    public Guid SavedCartId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string CartSnapshot { get; set; }  // JSON serialized cart

    public DateTime SavedAt { get; set; }
}
```

## API Endpoints

### REST API

#### Cart Operations
```
GET    /api/v1/cart                    # Get current user's cart
POST   /api/v1/cart                    # Create new cart
DELETE /api/v1/cart                    # Clear cart

GET    /api/v1/cart/items              # Get cart items
POST   /api/v1/cart/items              # Add item to cart
PUT    /api/v1/cart/items/:id          # Update item quantity
DELETE /api/v1/cart/items/:id          # Remove item

POST   /api/v1/cart/merge              # Merge anonymous cart to user cart
POST   /api/v1/cart/validate           # Validate cart (stock, prices)
POST   /api/v1/cart/calculate          # Recalculate totals
```

#### Coupons & Discounts
```
POST   /api/v1/cart/coupon             # Apply coupon
DELETE /api/v1/cart/coupon             # Remove coupon
```

#### Saved Carts
```
GET    /api/v1/cart/saved              # List saved carts
POST   /api/v1/cart/saved              # Save current cart
POST   /api/v1/cart/saved/:id/restore  # Restore saved cart
DELETE /api/v1/cart/saved/:id          # Delete saved cart
```

#### Health
```
GET    /health                         # Health check
GET    /metrics                        # Prometheus metrics
```

### gRPC API

```protobuf
service CartService {
  rpc GetCart(GetCartRequest) returns (CartResponse);
  rpc AddItem(AddItemRequest) returns (CartResponse);
  rpc UpdateQuantity(UpdateQuantityRequest) returns (CartResponse);
  rpc RemoveItem(RemoveItemRequest) returns (CartResponse);
  rpc ValidateCart(ValidateCartRequest) returns (ValidationResponse);
  rpc CalculateTotals(CalculateTotalsRequest) returns (CartResponse);
}
```

## Redis Data Structure

### Cart Storage (JSON)
```json
// Key: cart:{userId} or cart:session:{sessionId}
{
  "cartId": "uuid",
  "userId": "uuid",
  "items": [
    {
      "cartItemId": "uuid",
      "productId": "uuid",
      "sku": "PROD-001",
      "productName": "Product Name",
      "quantity": 2,
      "unitPrice": 100.00,
      "subtotal": 200.00
    }
  ],
  "subtotal": 200.00,
  "discountAmount": 20.00,
  "taxAmount": 18.00,
  "total": 198.00,
  "currency": "USD",
  "expiresAt": "2025-01-01T00:00:00Z"
}

// TTL: 7 days for authenticated, 24 hours for anonymous
```

## Database Schema (Code-First)

### EF Core Configuration
```csharp
public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");
        builder.HasKey(c => c.CartId);

        builder.Property(c => c.Subtotal).HasPrecision(19, 4);
        builder.Property(c => c.Total).HasPrecision(19, 4);

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.SessionId);
        builder.HasIndex(c => c.ExpiresAt);

        builder.HasMany(c => c.Items)
               .WithOne(i => i.Cart)
               .HasForeignKey(i => i.CartId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");
        builder.HasKey(i => i.CartItemId);

        builder.Property(i => i.UnitPrice).HasPrecision(19, 4);
        builder.Property(i => i.Subtotal).HasPrecision(19, 4);

        builder.HasIndex(i => i.CartId);
        builder.HasIndex(i => i.ProductId);
    }
}
```

## Kafka Events

### Produced Events
```csharp
// cart.item_added
{
  "eventId": "uuid",
  "eventType": "cart.item_added",
  "cartId": "uuid",
  "userId": "uuid",
  "productId": "uuid",
  "quantity": 2,
  "timestamp": "2025-01-01T00:00:00Z"
}

// cart.abandoned
{
  "eventId": "uuid",
  "eventType": "cart.abandoned",
  "cartId": "uuid",
  "userId": "uuid",
  "items": [...],
  "total": 198.00,
  "abandonedAt": "2025-01-01T00:00:00Z"
}

// cart.checkout_initiated
{
  "eventId": "uuid",
  "eventType": "cart.checkout_initiated",
  "cartId": "uuid",
  "userId": "uuid",
  "total": 198.00,
  "timestamp": "2025-01-01T00:00:00Z"
}
```

## Environment Variables
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Database (PostgreSQL - for persistence)
DATABASE_HOST=postgres
DATABASE_PORT=5432
DATABASE_NAME=cart_service
DATABASE_USER=cart_service
DATABASE_PASSWORD=***

# Redis (Primary storage)
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_PASSWORD=***
REDIS_TTL_AUTHENTICATED=604800  # 7 days
REDIS_TTL_ANONYMOUS=86400       # 24 hours

# Kafka
KAFKA_BOOTSTRAP_SERVERS=kafka:9092
KAFKA_TOPIC_CART_EVENTS=cart.events

# External Services
PRODUCT_SERVICE_URL=http://product-service:8080
INVENTORY_SERVICE_URL=http://inventory-service:8080
PRICING_SERVICE_URL=http://fx-rate-service:8080

# Cart Settings
CART_MAX_ITEMS=100
CART_EXPIRATION_DAYS_AUTH=7
CART_EXPIRATION_DAYS_ANON=1
```

## Implementation Tasks

### Phase 1: Setup
- [ ] Create .NET 10 Web API project
- [ ] Set up Clean Architecture structure
- [ ] Configure EF Core and PostgreSQL
- [ ] Configure Redis connection
- [ ] Set up migrations

### Phase 2: Domain Layer
- [ ] Define Cart and CartItem entities
- [ ] Create value objects (Money, Price)
- [ ] Define domain events
- [ ] Create repository interfaces

### Phase 3: Application Layer
- [ ] Implement DTOs
- [ ] Create use cases (Add/Remove/Update)
- [ ] Implement cart calculation logic
- [ ] Add validators

### Phase 4: Infrastructure Layer
- [ ] Implement Redis repository
- [ ] Implement PostgreSQL repository (backup)
- [ ] Create gRPC clients for external services
- [ ] Set up Kafka producer

### Phase 5: API Layer
- [ ] Implement REST controllers
- [ ] Add authentication middleware
- [ ] Implement gRPC service
- [ ] Add health checks

### Phase 6: Business Logic
- [ ] Implement cart merge logic
- [ ] Add cart validation
- [ ] Implement pricing calculations
- [ ] Create background job for cart cleanup

### Phase 7: Testing
- [ ] Unit tests
- [ ] Integration tests
- [ ] Performance tests (Redis)

## Key Implementation Examples

### Add Item Use Case
```csharp
public class AddItemUseCase : IAddItemUseCase
{
    private readonly ICartCacheRepository _cache;
    private readonly IInventoryClient _inventoryClient;
    private readonly IProductServiceClient _productClient;

    public async Task<CartResponse> ExecuteAsync(Guid userId, AddItemRequest request)
    {
        // Get or create cart
        var cart = await _cache.GetCartByUserIdAsync(userId)
                   ?? new Cart { UserId = userId, Currency = "USD" };

        // Validate stock
        var availability = await _inventoryClient.CheckAvailability(request.SKU);
        if (!availability.IsAvailable || availability.Stock < request.Quantity)
        {
            throw new OutOfStockException();
        }

        // Get product details
        var product = await _productClient.GetProductAsync(request.ProductId);

        // Check if item exists
        var existingItem = cart.Items.FirstOrDefault(i => i.SKU == request.SKU);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.ProductId,
                SKU = request.SKU,
                ProductName = product.Name,
                Quantity = request.Quantity,
                UnitPrice = product.Price
            });
        }

        // Recalculate totals
        await RecalculateTotals(cart);

        // Save to Redis
        await _cache.SaveCartAsync(cart);

        // Publish event
        await _eventPublisher.PublishAsync(new ItemAddedEvent(cart, request));

        return MapToResponse(cart);
    }
}
```

### Cart Cleanup Job
```csharp
public class CartCleanupJob : IHostedService
{
    public async Task ExecuteAsync()
    {
        // Find expired carts
        var expiredCarts = await _cartRepository.FindExpiredCartsAsync();

        foreach (var cart in expiredCarts)
        {
            // Publish abandoned cart event
            await _eventPublisher.PublishAsync(new CartAbandonedEvent(cart));

            // Mark as abandoned or delete
            cart.Status = CartStatus.Abandoned;
            await _cartRepository.UpdateAsync(cart);

            // Remove from Redis
            await _cache.DeleteCartAsync(cart.CartId);
        }
    }
}
```

## Testing Requirements

### Unit Tests
- [ ] Cart calculation logic
- [ ] Item quantity updates
- [ ] Coupon application
- [ ] Cart validation

### Integration Tests
- [ ] Redis operations
- [ ] Database persistence
- [ ] External service calls
- [ ] Event publishing

### Performance Tests
- [ ] Concurrent cart updates
- [ ] Redis read/write performance
- [ ] Large cart handling (100 items)

## Success Criteria
- 99.99% uptime
- API response time < 50ms (p95)
- Redis hit rate > 95%
- Support 100K+ active carts
- Zero cart data loss
- Event publishing < 10ms
