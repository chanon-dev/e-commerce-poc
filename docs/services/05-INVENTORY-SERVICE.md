# Inventory Service (Go)

## Overview
Inventory management service built with Go. Handles stock levels, reservations, warehouse management, and stock movements. Implements Clean Architecture with high-performance requirements.

## Technology Stack
- **Language**: Go 1.21+
- **Framework**: Fiber / Echo
- **Database**: PostgreSQL
- **ORM**: GORM (Code-First)
- **Migration**: Atlas / go-migrate
- **Cache**: Redis
- **Messaging**: Kafka (Consumer & Producer)
- **API**: REST + gRPC
- **Testing**: Go testing, testify

## Core Responsibilities

### 1. **Stock Management**
- Track stock levels per product/variant
- Multi-warehouse inventory
- Real-time stock updates
- Stock level alerts (low stock, out of stock)
- Stock movement history
- Batch stock updates

### 2. **Stock Reservation**
- Reserve inventory during checkout
- Temporary holds (TTL-based)
- Release reservations on timeout
- Reservation rollback on payment failure
- Prevent overselling

### 3. **Warehouse Management**
- Multiple warehouse support
- Warehouse-specific stock levels
- Inter-warehouse transfers
- Warehouse priority rules
- Regional fulfillment

### 4. **Stock Allocation**
- Intelligent stock allocation
- Nearest warehouse selection
- Stock availability by region
- Split shipment support

### 5. **Restock Management**
- Purchase order tracking
- Restock alerts
- Automatic reorder points
- Supplier management

### 6. **Stock Adjustments**
- Manual adjustments (damage, loss, found)
- Adjustment reason tracking
- Audit trail for all changes
- Approval workflow for large adjustments

### 7. **Event Handling**
- Listen to order events (reserve, commit, cancel)
- Listen to product events (new products)
- Publish stock level change events
- Publish low stock alerts

### 8. **Multi-Region Support**
- Regional stock visibility
- Cross-region stock transfers
- Regional stock reports

## Clean Architecture Structure

```
inventory-service/
├── cmd/
│   └── main.go
├── internal/
│   ├── domain/                          # Domain Layer
│   │   ├── entity/
│   │   │   ├── inventory.go
│   │   │   ├── warehouse.go
│   │   │   ├── reservation.go
│   │   │   └── stock_movement.go
│   │   ├── valueobject/
│   │   │   ├── sku.go
│   │   │   └── quantity.go
│   │   ├── enum/
│   │   │   ├── movement_type.go
│   │   │   └── reservation_status.go
│   │   ├── event/
│   │   │   ├── stock_updated.go
│   │   │   ├── stock_reserved.go
│   │   │   └── low_stock_alert.go
│   │   └── repository/
│   │       ├── inventory_repository.go
│   │       └── warehouse_repository.go
│   │
│   ├── usecase/                         # Application Layer
│   │   ├── dto/
│   │   │   ├── inventory_dto.go
│   │   │   ├── reservation_dto.go
│   │   │   └── warehouse_dto.go
│   │   ├── inventory_usecase.go
│   │   ├── reservation_usecase.go
│   │   ├── warehouse_usecase.go
│   │   └── stock_adjustment_usecase.go
│   │
│   ├── adapter/                         # Interface Adapters
│   │   ├── http/
│   │   │   ├── handler/
│   │   │   │   ├── inventory_handler.go
│   │   │   │   ├── warehouse_handler.go
│   │   │   │   └── health_handler.go
│   │   │   ├── middleware/
│   │   │   │   ├── auth_middleware.go
│   │   │   │   └── logging_middleware.go
│   │   │   └── router.go
│   │   └── grpc/
│   │       ├── proto/
│   │       │   └── inventory.proto
│   │       └── server/
│   │           └── inventory_server.go
│   │
│   └── infrastructure/                  # Frameworks & Drivers
│       ├── db/
│       │   ├── postgres.go
│       │   ├── migration/
│       │   │   ├── 001_initial_schema.go
│       │   │   └── 002_add_reservations.go
│       │   └── repository/
│       │       ├── inventory_repository_impl.go
│       │       └── warehouse_repository_impl.go
│       ├── cache/
│       │   └── redis_cache.go
│       ├── messaging/
│       │   ├── kafka_producer.go
│       │   ├── kafka_consumer.go
│       │   └── event_handler/
│       │       ├── order_event_handler.go
│       │       └── product_event_handler.go
│       └── config/
│           └── config.go
│
├── pkg/
│   ├── logger/
│   │   └── logger.go
│   └── util/
│       └── validator.go
│
├── config/
│   └── config.yaml
│
├── go.mod
├── go.sum
├── Makefile
├── Dockerfile
└── README.md
```

## Domain Models (GORM - Code-First)

### Inventory Entity
```go
type Inventory struct {
    InventoryID  uuid.UUID `gorm:"type:uuid;primaryKey;default:gen_random_uuid()"`
    ProductID    uuid.UUID `gorm:"type:uuid;not null;index"`
    VariantID    *uuid.UUID `gorm:"type:uuid;index"`
    WarehouseID  uuid.UUID `gorm:"type:uuid;not null;index"`
    SKU          string    `gorm:"type:varchar(50);not null;index"`

    QuantityAvailable int `gorm:"not null;default:0"`
    QuantityReserved  int `gorm:"not null;default:0"`
    QuantityTotal     int `gorm:"not null;default:0"`

    ReorderPoint      int `gorm:"not null;default:10"`
    ReorderQuantity   int `gorm:"not null;default:50"`

    CreatedAt time.Time `gorm:"not null;default:now()"`
    UpdatedAt time.Time `gorm:"not null;default:now()"`
    DeletedAt gorm.DeletedAt `gorm:"index"`
}
```

### Warehouse Entity
```go
type Warehouse struct {
    WarehouseID uuid.UUID `gorm:"type:uuid;primaryKey;default:gen_random_uuid()"`
    Code        string    `gorm:"type:varchar(20);unique;not null"`
    Name        string    `gorm:"type:varchar(255);not null"`

    Address     string    `gorm:"type:varchar(500)"`
    City        string    `gorm:"type:varchar(100)"`
    State       string    `gorm:"type:varchar(100)"`
    Country     string    `gorm:"type:varchar(100)"`
    PostalCode  string    `gorm:"type:varchar(20)"`

    Region      string    `gorm:"type:varchar(50);index"`
    IsActive    bool      `gorm:"not null;default:true"`
    Priority    int       `gorm:"not null;default:1"`

    Latitude    float64
    Longitude   float64

    CreatedAt time.Time `gorm:"not null;default:now()"`
    UpdatedAt time.Time `gorm:"not null;default:now()"`
}
```

### Reservation Entity
```go
type Reservation struct {
    ReservationID uuid.UUID `gorm:"type:uuid;primaryKey;default:gen_random_uuid()"`
    OrderID       uuid.UUID `gorm:"type:uuid;not null;index"`
    InventoryID   uuid.UUID `gorm:"type:uuid;not null;index"`

    Quantity      int       `gorm:"not null"`
    Status        ReservationStatus `gorm:"type:varchar(20);not null"`

    ReservedAt    time.Time `gorm:"not null;default:now()"`
    ExpiresAt     time.Time `gorm:"not null;index"`
    CommittedAt   *time.Time
    CancelledAt   *time.Time

    CreatedAt time.Time `gorm:"not null;default:now()"`
    UpdatedAt time.Time `gorm:"not null;default:now()"`
}

type ReservationStatus string

const (
    ReservationPending   ReservationStatus = "PENDING"
    ReservationCommitted ReservationStatus = "COMMITTED"
    ReservationCancelled ReservationStatus = "CANCELLED"
    ReservationExpired   ReservationStatus = "EXPIRED"
)
```

### StockMovement Entity
```go
type StockMovement struct {
    MovementID  uuid.UUID `gorm:"type:uuid;primaryKey;default:gen_random_uuid()"`
    InventoryID uuid.UUID `gorm:"type:uuid;not null;index"`

    Type        MovementType `gorm:"type:varchar(20);not null"`
    Quantity    int          `gorm:"not null"`

    Reason      string `gorm:"type:varchar(255)"`
    Reference   string `gorm:"type:varchar(100)"` // Order ID, PO ID, etc.

    PerformedBy uuid.UUID `gorm:"type:uuid"`

    CreatedAt time.Time `gorm:"not null;default:now()"`
}

type MovementType string

const (
    MovementInbound     MovementType = "INBOUND"      // Restock
    MovementOutbound    MovementType = "OUTBOUND"     // Sold
    MovementAdjustment  MovementType = "ADJUSTMENT"   // Manual fix
    MovementTransfer    MovementType = "TRANSFER"     // Between warehouses
    MovementReturn      MovementType = "RETURN"       // Customer return
    MovementDamage      MovementType = "DAMAGE"       // Damaged goods
)
```

## API Endpoints

### REST API

#### Inventory
```
GET    /api/v1/inventory                        # List inventory
GET    /api/v1/inventory/:id                    # Get inventory details
GET    /api/v1/inventory/product/:productId     # Get by product
GET    /api/v1/inventory/sku/:sku               # Get by SKU
POST   /api/v1/inventory                        # Create inventory entry
PUT    /api/v1/inventory/:id                    # Update inventory
POST   /api/v1/inventory/:id/adjust             # Adjust stock
GET    /api/v1/inventory/:id/movements          # Stock movement history
```

#### Reservations
```
POST   /api/v1/reservations                     # Create reservation
GET    /api/v1/reservations/:id                 # Get reservation
POST   /api/v1/reservations/:id/commit          # Commit reservation
POST   /api/v1/reservations/:id/cancel          # Cancel reservation
GET    /api/v1/reservations/order/:orderId      # Get by order
```

#### Warehouses
```
GET    /api/v1/warehouses                       # List warehouses
GET    /api/v1/warehouses/:id                   # Get warehouse
POST   /api/v1/warehouses                       # Create warehouse
PUT    /api/v1/warehouses/:id                   # Update warehouse
GET    /api/v1/warehouses/:id/inventory         # Warehouse inventory
```

#### Availability
```
POST   /api/v1/availability/check               # Check stock availability
POST   /api/v1/availability/bulk                # Bulk availability check
```

#### Health
```
GET    /health                                  # Health check
GET    /metrics                                 # Prometheus metrics
```

### gRPC API

```protobuf
service InventoryService {
  rpc GetInventory(GetInventoryRequest) returns (InventoryResponse);
  rpc CheckAvailability(CheckAvailabilityRequest) returns (AvailabilityResponse);
  rpc ReserveStock(ReserveStockRequest) returns (ReservationResponse);
  rpc CommitReservation(CommitReservationRequest) returns (Empty);
  rpc CancelReservation(CancelReservationRequest) returns (Empty);
  rpc UpdateStock(UpdateStockRequest) returns (InventoryResponse);
}
```

## Database Schema (GORM Auto-Migration)

```go
// migration/001_initial_schema.go
func up(db *gorm.DB) error {
    return db.AutoMigrate(
        &Warehouse{},
        &Inventory{},
        &Reservation{},
        &StockMovement{},
    )
}
```

## Kafka Events

### Consumed Events
```go
// order.created → Reserve stock
type OrderCreatedEvent struct {
    EventID   string    `json:"eventId"`
    OrderID   string    `json:"orderId"`
    Items     []OrderItem `json:"items"`
    Timestamp time.Time `json:"timestamp"`
}

// payment.successful → Commit reservation
// payment.failed → Cancel reservation
// order.cancelled → Cancel reservation and restock
```

### Produced Events
```go
// inventory.stock_updated
type StockUpdatedEvent struct {
    EventID     string    `json:"eventId"`
    InventoryID string    `json:"inventoryId"`
    ProductID   string    `json:"productId"`
    SKU         string    `json:"sku"`
    OldStock    int       `json:"oldStock"`
    NewStock    int       `json:"newStock"`
    Timestamp   time.Time `json:"timestamp"`
}

// inventory.low_stock
type LowStockAlert struct {
    EventID     string `json:"eventId"`
    ProductID   string `json:"productId"`
    SKU         string `json:"sku"`
    CurrentStock int   `json:"currentStock"`
    ReorderPoint int   `json:"reorderPoint"`
}

// inventory.reserved
// inventory.committed
// inventory.released
```

## Environment Variables
```bash
# Application
APP_ENV=production
APP_PORT=8080
GRPC_PORT=9090

# Database
DB_HOST=postgres
DB_PORT=5432
DB_NAME=inventory_service
DB_USER=inventory_service
DB_PASSWORD=***
DB_MAX_OPEN_CONNS=100
DB_MAX_IDLE_CONNS=10

# Redis
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_PASSWORD=***
REDIS_DB=0

# Kafka
KAFKA_BROKERS=kafka:9092
KAFKA_GROUP_ID=inventory-service
KAFKA_TOPIC_INVENTORY_EVENTS=inventory.events
KAFKA_TOPIC_ORDER_EVENTS=order.events

# Observability
JAEGER_ENDPOINT=http://jaeger:14268/api/traces
PROMETHEUS_PORT=9091
LOG_LEVEL=info
```

## Implementation Tasks

### Phase 1: Project Setup
- [ ] Initialize Go project with modules
- [ ] Set up project structure (Clean Architecture)
- [ ] Configure GORM with PostgreSQL
- [ ] Set up migrations (Atlas/go-migrate)
- [ ] Configure logging

### Phase 2: Domain Layer
- [ ] Define domain entities
- [ ] Create value objects
- [ ] Define repository interfaces
- [ ] Implement domain events

### Phase 3: Use Case Layer
- [ ] Implement inventory use cases
- [ ] Implement reservation use cases
- [ ] Implement stock adjustment use cases
- [ ] Create DTOs and validators

### Phase 4: Infrastructure Layer
- [ ] Implement GORM repositories
- [ ] Set up Redis caching
- [ ] Configure Kafka producer/consumer
- [ ] Implement event handlers

### Phase 5: API Layer
- [ ] Implement REST handlers (Fiber/Echo)
- [ ] Create middleware (auth, logging)
- [ ] Implement gRPC server
- [ ] Add health check endpoints

### Phase 6: Business Logic
- [ ] Implement stock reservation with TTL
- [ ] Create automatic expiration cleanup job
- [ ] Implement intelligent allocation logic
- [ ] Add low stock alerting

### Phase 7: Testing
- [ ] Write unit tests
- [ ] Write integration tests
- [ ] Test event handling
- [ ] Performance testing

## Key Implementation Details

### Stock Reservation with TTL
```go
func (uc *ReservationUseCase) ReserveStock(ctx context.Context, req ReserveStockRequest) (*Reservation, error) {
    // Check availability
    inventory, err := uc.inventoryRepo.GetBySKU(ctx, req.SKU)
    if err != nil {
        return nil, err
    }

    if inventory.QuantityAvailable < req.Quantity {
        return nil, ErrInsufficientStock
    }

    // Create reservation with TTL
    reservation := &Reservation{
        OrderID:     req.OrderID,
        InventoryID: inventory.InventoryID,
        Quantity:    req.Quantity,
        Status:      ReservationPending,
        ReservedAt:  time.Now(),
        ExpiresAt:   time.Now().Add(15 * time.Minute), // 15 min TTL
    }

    // Update inventory
    inventory.QuantityAvailable -= req.Quantity
    inventory.QuantityReserved += req.Quantity

    // Save in transaction
    err = uc.db.Transaction(func(tx *gorm.DB) error {
        if err := tx.Create(reservation).Error; err != nil {
            return err
        }
        if err := tx.Save(inventory).Error; err != nil {
            return err
        }
        return nil
    })

    // Publish event
    uc.publisher.Publish("inventory.reserved", reservation)

    return reservation, nil
}
```

### Expiration Cleanup Job
```go
func (uc *ReservationUseCase) CleanupExpiredReservations(ctx context.Context) error {
    expiredReservations, err := uc.reservationRepo.FindExpired(ctx)

    for _, res := range expiredReservations {
        // Release stock and mark as expired
        uc.CancelReservation(ctx, res.ReservationID)
    }

    return nil
}
```

## Testing Requirements

### Unit Tests
- [ ] Domain entity validation
- [ ] Use case business logic
- [ ] Stock calculation accuracy

### Integration Tests
- [ ] Database operations
- [ ] Kafka event handling
- [ ] gRPC service calls
- [ ] Reservation TTL behavior

### Performance Tests
- [ ] Concurrent stock updates
- [ ] High-volume reservations
- [ ] Query performance

## Success Criteria
- 99.99% uptime
- API response time < 50ms (p95)
- Support 50K+ concurrent reservations
- Zero overselling incidents
- Accurate stock tracking (100%)
- Event processing latency < 100ms
