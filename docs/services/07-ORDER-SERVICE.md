# Order Service (Java Spring Boot 3)

## Overview
Order management service built with Java Spring Boot 3. Handles order placement, order lifecycle, order history, and implements transactional outbox pattern for reliable event publishing.

## Technology Stack
- **Language**: Java 17+
- **Framework**: Spring Boot 3.x
- **Database**: PostgreSQL
- **ORM**: JPA/Hibernate (Code-First)
- **Migration**: Flyway OSS
- **Messaging**: Kafka (Producer & Consumer) with Outbox Pattern
- **API**: REST + gRPC
- **Testing**: JUnit 5, Mockito, TestContainers

## Core Responsibilities

### 1. **Order Creation & Management**
- Create orders from cart
- Order validation
- Order status tracking
- Order modification (before fulfillment)
- Order cancellation
- Order history

### 2. **Order Lifecycle Management**
- Order state machine
- Status transitions (Pending → Confirmed → Processing → Shipped → Delivered)
- Payment confirmation integration
- Inventory reservation coordination
- Fulfillment coordination

### 3. **Order Calculations**
- Order total calculation
- Tax calculation per region
- Shipping cost calculation
- Discount application
- Multi-currency support

### 4. **Payment Integration**
- Coordinate with Payment Service
- Handle payment callbacks
- Payment retry logic
- Refund processing

### 5. **Fulfillment Coordination**
- Coordinate with Inventory Service
- Split shipments handling
- Shipping provider integration
- Tracking number management

### 6. **Order Analytics**
- Order metrics and reporting
- Customer order history
- Sales analytics
- Performance tracking

### 7. **Event Publishing (Outbox Pattern)**
- Order created events
- Order status changed events
- Order cancelled events
- Payment required events
- Reliable event delivery guarantee

### 8. **Multi-Region Support**
- Regional order processing
- Cross-border orders
- Regional tax and compliance
- Multi-currency orders

## Clean Architecture Structure

```
order-service/
├── src/
│   ├── main/
│   │   ├── java/
│   │   │   └── com/ecommerce/order/
│   │   │       ├── domain/
│   │   │       │   ├── model/
│   │   │       │   │   ├── Order.java
│   │   │       │   │   ├── OrderItem.java
│   │   │       │   │   ├── OrderStatus.java
│   │   │       │   │   ├── ShippingAddress.java
│   │   │       │   │   └── PaymentInfo.java
│   │   │       │   ├── valueobject/
│   │   │       │   │   ├── Money.java
│   │   │       │   │   ├── OrderNumber.java
│   │   │       │   │   └── TrackingNumber.java
│   │   │       │   ├── enums/
│   │   │       │   │   ├── OrderStatus.java
│   │   │       │   │   ├── PaymentStatus.java
│   │   │       │   │   └── FulfillmentStatus.java
│   │   │       │   ├── event/
│   │   │       │   │   ├── OrderCreatedEvent.java
│   │   │       │   │   ├── OrderConfirmedEvent.java
│   │   │       │   │   └── OrderCancelledEvent.java
│   │   │       │   └── repository/
│   │   │       │       ├── OrderRepository.java
│   │   │       │       └── OutboxRepository.java
│   │   │       │
│   │   │       ├── application/
│   │   │       │   ├── dto/
│   │   │       │   │   ├── request/
│   │   │       │   │   │   ├── CreateOrderRequest.java
│   │   │       │   │   │   ├── UpdateOrderRequest.java
│   │   │       │   │   │   └── CancelOrderRequest.java
│   │   │       │   │   └── response/
│   │   │       │   │       ├── OrderResponse.java
│   │   │       │   │       ├── OrderSummaryResponse.java
│   │   │       │   │       └── OrderHistoryResponse.java
│   │   │       │   ├── usecase/
│   │   │       │   │   ├── CreateOrderUseCase.java
│   │   │       │   │   ├── ConfirmOrderUseCase.java
│   │   │       │   │   ├── CancelOrderUseCase.java
│   │   │       │   │   ├── UpdateOrderStatusUseCase.java
│   │   │       │   │   └── GetOrderHistoryUseCase.java
│   │   │       │   ├── service/
│   │   │       │   │   ├── OrderService.java
│   │   │       │   │   ├── OrderCalculationService.java
│   │   │       │   │   └── OrderStateMachine.java
│   │   │       │   └── saga/
│   │   │       │       └── OrderCreationSaga.java
│   │   │       │
│   │   │       ├── infrastructure/
│   │   │       │   ├── persistence/
│   │   │       │   │   ├── entity/
│   │   │       │   │   │   ├── OrderEntity.java
│   │   │       │   │   │   ├── OrderItemEntity.java
│   │   │       │   │   │   └── OutboxEntity.java
│   │   │       │   │   ├── jpa/
│   │   │       │   │   │   ├── OrderJpaRepository.java
│   │   │       │   │   │   └── OutboxJpaRepository.java
│   │   │       │   │   └── repository/
│   │   │       │   │       └── OrderRepositoryImpl.java
│   │   │       │   ├── messaging/
│   │   │       │   │   ├── KafkaProducer.java
│   │   │       │   │   ├── KafkaConsumer.java
│   │   │       │   │   ├── OutboxPublisher.java
│   │   │       │   │   └── eventhandler/
│   │   │       │   │       ├── PaymentEventHandler.java
│   │   │       │   │       └── InventoryEventHandler.java
│   │   │       │   ├── external/
│   │   │       │   │   ├── PaymentServiceClient.java
│   │   │       │   │   ├── InventoryServiceClient.java
│   │   │       │   │   └── UserServiceClient.java
│   │   │       │   └── grpc/
│   │   │       │       ├── proto/
│   │   │       │       │   └── order.proto
│   │   │       │       └── OrderGrpcService.java
│   │   │       │
│   │   │       └── api/
│   │   │           ├── controller/
│   │   │           │   ├── OrderController.java
│   │   │           │   ├── OrderHistoryController.java
│   │   │           │   └── HealthController.java
│   │   │           ├── exception/
│   │   │           │   └── GlobalExceptionHandler.java
│   │   │           ├── config/
│   │   │           │   ├── JpaConfig.java
│   │   │           │   ├── KafkaConfig.java
│   │   │           │   └── SecurityConfig.java
│   │   │           └── OrderServiceApplication.java
│   │   │
│   │   └── resources/
│   │       ├── application.yml
│   │       └── db/migration/
│   │           ├── V1__initial_schema.sql
│   │           └── V2__add_outbox_table.sql
│   │
│   └── test/
│
├── pom.xml
├── Dockerfile
└── README.md
```

## Domain Models (JPA Entities - Code-First)

### Order Entity
```java
@Entity
@Table(name = "orders")
public class Order extends BaseEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID orderId;

    @Column(unique = true, nullable = false, length = 20)
    private String orderNumber;

    @Column(name = "user_id", nullable = false)
    private UUID userId;

    @Column(name = "cart_id")
    private UUID cartId;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false, length = 30)
    private OrderStatus status;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false, length = 30)
    private PaymentStatus paymentStatus;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false, length = 30)
    private FulfillmentStatus fulfillmentStatus;

    @OneToMany(mappedBy = "order", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<OrderItem> items = new ArrayList<>();

    @Embedded
    private ShippingAddress shippingAddress;

    @Embedded
    private BillingAddress billingAddress;

    // Pricing
    @Column(nullable = false, precision = 19, scale = 4)
    private BigDecimal subtotal;

    @Column(nullable = false, precision = 19, scale = 4)
    private BigDecimal discountAmount;

    @Column(nullable = false, precision = 19, scale = 4)
    private BigDecimal taxAmount;

    @Column(nullable = false, precision = 19, scale = 4)
    private BigDecimal shippingCost;

    @Column(nullable = false, precision = 19, scale = 4)
    private BigDecimal total;

    @Column(nullable = false, length = 3)
    private String currency;

    @Column(length = 50)
    private String couponCode;

    // Payment
    @Column(name = "payment_method", length = 50)
    private String paymentMethod;

    @Column(name = "payment_id")
    private UUID paymentId;

    // Shipping
    @Column(name = "tracking_number", length = 100)
    private String trackingNumber;

    @Column(name = "shipping_carrier", length = 100)
    private String shippingCarrier;

    @Column(name = "estimated_delivery")
    private LocalDate estimatedDelivery;

    // Audit
    @Column(name = "created_at", nullable = false)
    private LocalDateTime createdAt;

    @Column(name = "confirmed_at")
    private LocalDateTime confirmedAt;

    @Column(name = "shipped_at")
    private LocalDateTime shippedAt;

    @Column(name = "delivered_at")
    private LocalDateTime deliveredAt;

    @Column(name = "cancelled_at")
    private LocalDateTime cancelledAt;

    @Column(name = "updated_at")
    private LocalDateTime updatedAt;
}

public enum OrderStatus {
    PENDING,
    CONFIRMED,
    PROCESSING,
    SHIPPED,
    DELIVERED,
    CANCELLED,
    REFUNDED
}

public enum PaymentStatus {
    PENDING,
    AUTHORIZED,
    CAPTURED,
    FAILED,
    REFUNDED
}

public enum FulfillmentStatus {
    PENDING,
    RESERVED,
    PICKING,
    PACKED,
    SHIPPED,
    DELIVERED,
    RETURNED
}
```

### OrderItem Entity
```java
@Entity
@Table(name = "order_items")
public class OrderItem extends BaseEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID orderItemId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "order_id", nullable = false)
    private Order order;

    @Column(name = "product_id", nullable = false)
    private UUID productId;

    @Column(name = "variant_id")
    private UUID variantId;

    @Column(nullable = false, length = 50)
    private String sku;

    @Column(nullable = false, length = 255)
    private String productName;

    @Column(name = "product_image")
    private String productImage;

    @Column(nullable = false)
    private Integer quantity;

    @Column(nullable = false, precision = 19, scale = 4)
    private BigDecimal unitPrice;

    @Column(nullable = false, precision = 19, scale = 4)
    private BigDecimal subtotal;

    @Column(precision = 19, scale = 4)
    private BigDecimal discountAmount;

    @Column(nullable = false, precision = 19, scale = 4)
    private BigDecimal finalPrice;

    @Column(name = "warehouse_id")
    private UUID warehouseId;
}
```

### Outbox Entity (Transactional Outbox Pattern)
```java
@Entity
@Table(name = "outbox_events")
public class OutboxEvent extends BaseEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID eventId;

    @Column(nullable = false, length = 100)
    private String aggregateType;  // "Order"

    @Column(nullable = false)
    private UUID aggregateId;      // OrderId

    @Column(nullable = false, length = 100)
    private String eventType;      // "OrderCreated", "OrderConfirmed"

    @Column(nullable = false, columnDefinition = "TEXT")
    private String payload;        // JSON event data

    @Column(nullable = false)
    private LocalDateTime occurredAt;

    @Column(nullable = false)
    private Boolean processed;

    @Column
    private LocalDateTime processedAt;

    @Column
    private Integer retryCount;
}
```

## API Endpoints

### REST API

#### Orders
```
POST   /api/v1/orders                   # Create order from cart
GET    /api/v1/orders                   # List user's orders
GET    /api/v1/orders/{id}              # Get order details
PUT    /api/v1/orders/{id}              # Update order (limited fields)
POST   /api/v1/orders/{id}/cancel       # Cancel order
POST   /api/v1/orders/{id}/confirm      # Confirm order (after payment)
```

#### Order History
```
GET    /api/v1/orders/history           # Order history (paginated)
GET    /api/v1/orders/{id}/tracking     # Get tracking info
GET    /api/v1/orders/{id}/invoice      # Get invoice
```

#### Admin
```
GET    /api/v1/admin/orders             # List all orders
PUT    /api/v1/admin/orders/{id}/status # Update order status
POST   /api/v1/admin/orders/{id}/refund # Process refund
```

#### Health
```
GET    /actuator/health                 # Health check
GET    /actuator/metrics                # Metrics
```

### gRPC API

```protobuf
service OrderService {
  rpc CreateOrder(CreateOrderRequest) returns (OrderResponse);
  rpc GetOrder(GetOrderRequest) returns (OrderResponse);
  rpc GetOrderHistory(GetOrderHistoryRequest) returns (OrderHistoryResponse);
  rpc UpdateOrderStatus(UpdateStatusRequest) returns (OrderResponse);
  rpc CancelOrder(CancelOrderRequest) returns (OrderResponse);
}
```

## Database Schema (Flyway Migration)

### V1__initial_schema.sql
```sql
CREATE TABLE orders (
    order_id UUID PRIMARY KEY,
    order_number VARCHAR(20) UNIQUE NOT NULL,
    user_id UUID NOT NULL,
    cart_id UUID,

    status VARCHAR(30) NOT NULL,
    payment_status VARCHAR(30) NOT NULL,
    fulfillment_status VARCHAR(30) NOT NULL,

    subtotal DECIMAL(19,4) NOT NULL,
    discount_amount DECIMAL(19,4) NOT NULL DEFAULT 0,
    tax_amount DECIMAL(19,4) NOT NULL DEFAULT 0,
    shipping_cost DECIMAL(19,4) NOT NULL DEFAULT 0,
    total DECIMAL(19,4) NOT NULL,
    currency VARCHAR(3) NOT NULL,

    coupon_code VARCHAR(50),
    payment_method VARCHAR(50),
    payment_id UUID,

    -- Shipping address (embedded)
    shipping_address_line1 VARCHAR(255),
    shipping_address_line2 VARCHAR(255),
    shipping_city VARCHAR(100),
    shipping_state VARCHAR(100),
    shipping_country VARCHAR(100),
    shipping_postal_code VARCHAR(20),

    -- Billing address (embedded)
    billing_address_line1 VARCHAR(255),
    billing_city VARCHAR(100),
    billing_country VARCHAR(100),
    billing_postal_code VARCHAR(20),

    tracking_number VARCHAR(100),
    shipping_carrier VARCHAR(100),
    estimated_delivery DATE,

    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    confirmed_at TIMESTAMP,
    shipped_at TIMESTAMP,
    delivered_at TIMESTAMP,
    cancelled_at TIMESTAMP,
    updated_at TIMESTAMP
);

CREATE TABLE order_items (
    order_item_id UUID PRIMARY KEY,
    order_id UUID NOT NULL REFERENCES orders(order_id) ON DELETE CASCADE,
    product_id UUID NOT NULL,
    variant_id UUID,
    sku VARCHAR(50) NOT NULL,
    product_name VARCHAR(255) NOT NULL,
    product_image VARCHAR(500),
    quantity INTEGER NOT NULL,
    unit_price DECIMAL(19,4) NOT NULL,
    subtotal DECIMAL(19,4) NOT NULL,
    discount_amount DECIMAL(19,4) DEFAULT 0,
    final_price DECIMAL(19,4) NOT NULL,
    warehouse_id UUID
);

-- Indexes
CREATE INDEX idx_orders_user ON orders(user_id);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_created ON orders(created_at);
CREATE INDEX idx_order_items_order ON order_items(order_id);
CREATE INDEX idx_order_items_product ON order_items(product_id);
```

### V2__add_outbox_table.sql
```sql
CREATE TABLE outbox_events (
    event_id UUID PRIMARY KEY,
    aggregate_type VARCHAR(100) NOT NULL,
    aggregate_id UUID NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    payload TEXT NOT NULL,
    occurred_at TIMESTAMP NOT NULL DEFAULT NOW(),
    processed BOOLEAN NOT NULL DEFAULT FALSE,
    processed_at TIMESTAMP,
    retry_count INTEGER DEFAULT 0
);

CREATE INDEX idx_outbox_processed ON outbox_events(processed, occurred_at);
CREATE INDEX idx_outbox_aggregate ON outbox_events(aggregate_type, aggregate_id);
```

## Kafka Events (Outbox Pattern)

### Produced Events
```java
// order.created
{
  "eventId": "uuid",
  "eventType": "order.created",
  "orderId": "uuid",
  "orderNumber": "ORD-2025-001",
  "userId": "uuid",
  "total": 198.00,
  "currency": "USD",
  "items": [...],
  "timestamp": "2025-01-01T00:00:00Z"
}

// order.confirmed
// order.cancelled
// order.shipped
```

### Consumed Events
```java
// payment.successful → Update order status
// payment.failed → Cancel order and release inventory
// inventory.reserved → Update fulfillment status
```

## Outbox Publisher Implementation

```java
@Service
public class OutboxPublisher {

    @Scheduled(fixedDelay = 5000) // Every 5 seconds
    @Transactional
    public void publishPendingEvents() {
        List<OutboxEvent> pendingEvents = outboxRepository
            .findByProcessedFalseOrderByOccurredAtAsc(PageRequest.of(0, 100));

        for (OutboxEvent event : pendingEvents) {
            try {
                kafkaTemplate.send(
                    "order.events",
                    event.getAggregateId().toString(),
                    event.getPayload()
                );

                event.setProcessed(true);
                event.setProcessedAt(LocalDateTime.now());
                outboxRepository.save(event);

            } catch (Exception e) {
                event.setRetryCount(event.getRetryCount() + 1);
                outboxRepository.save(event);
                log.error("Failed to publish event: {}", event.getEventId(), e);
            }
        }
    }
}
```

## Environment Variables
```yaml
spring:
  datasource:
    url: jdbc:postgresql://${DB_HOST}:${DB_PORT}/${DB_NAME}
    username: ${DB_USER}
    password: ${DB_PASSWORD}
  jpa:
    hibernate:
      ddl-auto: validate
  flyway:
    enabled: true

kafka:
  bootstrap-servers: ${KAFKA_BOOTSTRAP_SERVERS}
  topics:
    order-events: order.events
    payment-events: payment.events
```

## Implementation Tasks

### Phase 1: Setup
- [ ] Create Spring Boot project
- [ ] Configure PostgreSQL & Flyway
- [ ] Set up Clean Architecture structure
- [ ] Create JPA entities

### Phase 2: Domain Layer
- [ ] Define Order aggregate
- [ ] Implement order state machine
- [ ] Create domain events
- [ ] Define repository interfaces

### Phase 3: Application Layer
- [ ] Implement order creation saga
- [ ] Create use cases
- [ ] Add business validation
- [ ] Implement calculation logic

### Phase 4: Infrastructure Layer
- [ ] Implement repositories
- [ ] Set up Outbox pattern
- [ ] Configure Kafka
- [ ] Create gRPC clients

### Phase 5: API Layer
- [ ] Implement REST controllers
- [ ] Add exception handling
- [ ] Configure security
- [ ] Add actuator endpoints

### Phase 6: Event Processing
- [ ] Implement outbox publisher job
- [ ] Create event handlers
- [ ] Add retry logic
- [ ] Monitor event processing

### Phase 7: Testing
- [ ] Unit tests
- [ ] Integration tests (TestContainers)
- [ ] Saga testing
- [ ] Performance testing

## Success Criteria
- 99.9% uptime
- Order creation < 200ms (p95)
- Zero order data loss
- Exactly-once event delivery
- Support 10K+ orders/day
