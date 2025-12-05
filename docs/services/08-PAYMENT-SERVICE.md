# Payment Service (Go)

## Overview
Payment processing service built with Go. Handles payment processing, payment gateway integration, refunds, and payment lifecycle management with high security and PCI compliance considerations.

## Technology Stack
- **Language**: Go 1.21+
- **Framework**: Fiber / Echo
- **Database**: PostgreSQL
- **ORM**: GORM / Ent (Code-First)
- **Migration**: Atlas / go-migrate
- **Cache**: Redis
- **Messaging**: Kafka (Consumer & Producer)
- **API**: REST + gRPC
- **Payment Gateways**: Stripe OSS libraries, PayPal SDK (OSS)
- **Testing**: Go testing, testify

## Core Responsibilities

### 1. **Payment Processing**
- Process credit/debit card payments
- Handle alternative payment methods (PayPal, bank transfers)
- Payment authorization
- Payment capture (two-phase commit)
- Automatic payment retry logic

### 2. **Payment Gateway Integration**
- Integrate with multiple payment gateways (Stripe, PayPal)
- Gateway selection based on region/currency
- Gateway failover handling
- Webhook handling from gateways

### 3. **Refund Management**
- Full refunds
- Partial refunds
- Refund status tracking
- Automatic refund processing
- Refund to original payment method

### 4. **Payment Security**
- PCI DSS compliance
- Tokenization (no raw card storage)
- Encryption at rest and in transit
- Fraud detection integration
- 3D Secure (SCA) support

### 5. **Multi-Currency Support**
- Currency conversion
- Regional payment methods
- Settlement in merchant currency
- FX rate management

### 6. **Payment Analytics**
- Transaction success/failure rates
- Payment method analytics
- Revenue tracking
- Chargeback monitoring

### 7. **Event Handling**
- Listen to order events (payment required)
- Publish payment success/failure events
- Handle payment gateway webhooks
- Idempotency guarantees

### 8. **Compliance & Audit**
- Transaction logging
- Audit trail
- Regulatory compliance (PSD2, etc.)
- Data retention policies

## Clean Architecture Structure

```
payment-service/
├── cmd/
│   └── main.go
├── internal/
│   ├── domain/
│   │   ├── entity/
│   │   │   ├── payment.go
│   │   │   ├── transaction.go
│   │   │   ├── refund.go
│   │   │   └── payment_method.go
│   │   ├── valueobject/
│   │   │   ├── money.go
│   │   │   ├── card_token.go
│   │   │   └── transaction_id.go
│   │   ├── enum/
│   │   │   ├── payment_status.go
│   │   │   ├── payment_method_type.go
│   │   │   └── gateway_type.go
│   │   ├── event/
│   │   │   ├── payment_successful.go
│   │   │   ├── payment_failed.go
│   │   │   └── refund_processed.go
│   │   └── repository/
│   │       ├── payment_repository.go
│   │       └── transaction_repository.go
│   │
│   ├── usecase/
│   │   ├── dto/
│   │   │   ├── payment_dto.go
│   │   │   └── refund_dto.go
│   │   ├── payment_usecase.go
│   │   ├── refund_usecase.go
│   │   └── webhook_usecase.go
│   │
│   ├── adapter/
│   │   ├── http/
│   │   │   ├── handler/
│   │   │   │   ├── payment_handler.go
│   │   │   │   ├── refund_handler.go
│   │   │   │   ├── webhook_handler.go
│   │   │   │   └── health_handler.go
│   │   │   ├── middleware/
│   │   │   │   ├── auth_middleware.go
│   │   │   │   ├── signature_verification.go
│   │   │   │   └── idempotency_middleware.go
│   │   │   └── router.go
│   │   └── grpc/
│   │       ├── proto/
│   │       │   └── payment.proto
│   │       └── server/
│   │           └── payment_server.go
│   │
│   └── infrastructure/
│       ├── db/
│       │   ├── postgres.go
│       │   ├── migration/
│       │   │   ├── 001_initial_schema.go
│       │   │   └── 002_add_refunds.go
│       │   └── repository/
│       │       ├── payment_repository_impl.go
│       │       └── transaction_repository_impl.go
│       ├── cache/
│       │   └── redis_cache.go
│       ├── gateway/
│       │   ├── stripe_gateway.go
│       │   ├── paypal_gateway.go
│       │   └── gateway_factory.go
│       ├── messaging/
│       │   ├── kafka_producer.go
│       │   ├── kafka_consumer.go
│       │   └── event_handler/
│       │       └── order_event_handler.go
│       ├── encryption/
│       │   └── encryption_service.go
│       └── config/
│           └── config.go
│
├── pkg/
│   ├── logger/
│   ├── validator/
│   └── security/
│       └── pci_compliance.go
│
├── go.mod
├── Dockerfile
└── README.md
```

## Domain Models (GORM - Code-First)

### Payment Entity
```go
type Payment struct {
    PaymentID   uuid.UUID `gorm:"type:uuid;primaryKey;default:gen_random_uuid()"`
    OrderID     uuid.UUID `gorm:"type:uuid;not null;uniqueIndex"`
    UserID      uuid.UUID `gorm:"type:uuid;not null;index"`

    Amount      float64 `gorm:"type:decimal(19,4);not null"`
    Currency    string  `gorm:"type:varchar(3);not null"`
    Status      PaymentStatus `gorm:"type:varchar(20);not null"`

    PaymentMethod     PaymentMethodType `gorm:"type:varchar(30);not null"`
    PaymentMethodID   *uuid.UUID        `gorm:"type:uuid"`

    Gateway           GatewayType `gorm:"type:varchar(20);not null"`
    GatewayPaymentID  string      `gorm:"type:varchar(255);uniqueIndex"`
    GatewayCustomerID string      `gorm:"type:varchar(255)"`

    Description string `gorm:"type:varchar(500)"`

    // Security
    IdempotencyKey string `gorm:"type:varchar(100);uniqueIndex"`

    // Audit
    AuthorizedAt *time.Time
    CapturedAt   *time.Time
    FailedAt     *time.Time
    RefundedAt   *time.Time

    FailureReason string `gorm:"type:varchar(500)"`
    RetryCount    int    `gorm:"default:0"`

    CreatedAt time.Time `gorm:"not null;default:now()"`
    UpdatedAt time.Time `gorm:"not null;default:now()"`
}

type PaymentStatus string

const (
    PaymentPending    PaymentStatus = "PENDING"
    PaymentAuthorized PaymentStatus = "AUTHORIZED"
    PaymentCaptured   PaymentStatus = "CAPTURED"
    PaymentFailed     PaymentStatus = "FAILED"
    PaymentRefunded   PaymentStatus = "REFUNDED"
    PaymentCancelled  PaymentStatus = "CANCELLED"
)

type PaymentMethodType string

const (
    CreditCard     PaymentMethodType = "CREDIT_CARD"
    DebitCard      PaymentMethodType = "DEBIT_CARD"
    PayPal         PaymentMethodType = "PAYPAL"
    BankTransfer   PaymentMethodType = "BANK_TRANSFER"
    Cryptocurrency PaymentMethodType = "CRYPTO"
)

type GatewayType string

const (
    Stripe GatewayType = "STRIPE"
    PayPal GatewayType = "PAYPAL"
)
```

### Transaction Entity (Payment History)
```go
type Transaction struct {
    TransactionID uuid.UUID `gorm:"type:uuid;primaryKey;default:gen_random_uuid()"`
    PaymentID     uuid.UUID `gorm:"type:uuid;not null;index"`

    Type          TransactionType `gorm:"type:varchar(20);not null"`
    Amount        float64         `gorm:"type:decimal(19,4);not null"`
    Currency      string          `gorm:"type:varchar(3);not null"`

    Status        string `gorm:"type:varchar(20);not null"`
    GatewayTxnID  string `gorm:"type:varchar(255)"`

    Metadata      string `gorm:"type:jsonb"`

    CreatedAt time.Time `gorm:"not null;default:now()"`
}

type TransactionType string

const (
    TxnAuthorization TransactionType = "AUTHORIZATION"
    TxnCapture       TransactionType = "CAPTURE"
    TxnRefund        TransactionType = "REFUND"
    TxnVoid          TransactionType = "VOID"
)
```

### Refund Entity
```go
type Refund struct {
    RefundID      uuid.UUID `gorm:"type:uuid;primaryKey;default:gen_random_uuid()"`
    PaymentID     uuid.UUID `gorm:"type:uuid;not null;index"`
    TransactionID uuid.UUID `gorm:"type:uuid;not null"`

    Amount        float64 `gorm:"type:decimal(19,4);not null"`
    Currency      string  `gorm:"type:varchar(3);not null"`
    Reason        string  `gorm:"type:varchar(500)"`

    Status        RefundStatus `gorm:"type:varchar(20);not null"`
    GatewayRefundID string     `gorm:"type:varchar(255)"`

    RequestedBy   uuid.UUID  `gorm:"type:uuid;not null"`
    RequestedAt   time.Time  `gorm:"not null"`
    ProcessedAt   *time.Time

    CreatedAt time.Time `gorm:"not null;default:now()"`
}

type RefundStatus string

const (
    RefundPending   RefundStatus = "PENDING"
    RefundProcessed RefundStatus = "PROCESSED"
    RefundFailed    RefundStatus = "FAILED"
)
```

### PaymentMethod Entity (Stored payment methods)
```go
type PaymentMethod struct {
    PaymentMethodID uuid.UUID `gorm:"type:uuid;primaryKey;default:gen_random_uuid()"`
    UserID          uuid.UUID `gorm:"type:uuid;not null;index"`

    Type            PaymentMethodType `gorm:"type:varchar(30);not null"`

    // Tokenized card info (PCI compliant)
    CardToken       string `gorm:"type:varchar(255)"` // Gateway token
    CardLast4       string `gorm:"type:varchar(4)"`
    CardBrand       string `gorm:"type:varchar(20)"`  // Visa, Mastercard
    CardExpiryMonth int
    CardExpiryYear  int

    // PayPal
    PayPalEmail string `gorm:"type:varchar(255)"`

    IsDefault bool `gorm:"default:false"`

    CreatedAt time.Time `gorm:"not null;default:now()"`
    UpdatedAt time.Time
    DeletedAt gorm.DeletedAt `gorm:"index"`
}
```

## API Endpoints

### REST API

#### Payments
```
POST   /api/v1/payments                      # Create payment
GET    /api/v1/payments/:id                  # Get payment details
POST   /api/v1/payments/:id/authorize        # Authorize payment
POST   /api/v1/payments/:id/capture          # Capture payment
POST   /api/v1/payments/:id/cancel           # Cancel payment
GET    /api/v1/payments/:id/status           # Get payment status
```

#### Refunds
```
POST   /api/v1/payments/:id/refund           # Create refund
GET    /api/v1/refunds/:id                   # Get refund details
GET    /api/v1/payments/:id/refunds          # List refunds for payment
```

#### Payment Methods
```
GET    /api/v1/payment-methods               # List saved payment methods
POST   /api/v1/payment-methods               # Add payment method
DELETE /api/v1/payment-methods/:id           # Remove payment method
POST   /api/v1/payment-methods/:id/default   # Set as default
```

#### Webhooks
```
POST   /api/v1/webhooks/stripe               # Stripe webhook
POST   /api/v1/webhooks/paypal               # PayPal webhook
```

#### Health
```
GET    /health                               # Health check
GET    /metrics                              # Prometheus metrics
```

### gRPC API

```protobuf
service PaymentService {
  rpc CreatePayment(CreatePaymentRequest) returns (PaymentResponse);
  rpc AuthorizePayment(AuthorizePaymentRequest) returns (PaymentResponse);
  rpc CapturePayment(CapturePaymentRequest) returns (PaymentResponse);
  rpc GetPayment(GetPaymentRequest) returns (PaymentResponse);
  rpc RefundPayment(RefundPaymentRequest) returns (RefundResponse);
  rpc GetPaymentStatus(GetPaymentStatusRequest) returns (PaymentStatusResponse);
}
```

## Kafka Events

### Consumed Events
```go
// order.confirmed → Initiate payment authorization
type OrderConfirmedEvent struct {
    EventID   string    `json:"eventId"`
    OrderID   string    `json:"orderId"`
    UserID    string    `json:"userId"`
    Total     float64   `json:"total"`
    Currency  string    `json:"currency"`
    Timestamp time.Time `json:"timestamp"`
}
```

### Produced Events
```go
// payment.successful
type PaymentSuccessfulEvent struct {
    EventID         string    `json:"eventId"`
    PaymentID       string    `json:"paymentId"`
    OrderID         string    `json:"orderId"`
    Amount          float64   `json:"amount"`
    Currency        string    `json:"currency"`
    PaymentMethod   string    `json:"paymentMethod"`
    TransactionID   string    `json:"transactionId"`
    Timestamp       time.Time `json:"timestamp"`
}

// payment.failed
type PaymentFailedEvent struct {
    EventID         string    `json:"eventId"`
    PaymentID       string    `json:"paymentId"`
    OrderID         string    `json:"orderId"`
    Reason          string    `json:"reason"`
    RetryAttempt    int       `json:"retryAttempt"`
    Timestamp       time.Time `json:"timestamp"`
}

// payment.refunded
type PaymentRefundedEvent struct {
    EventID   string    `json:"eventId"`
    PaymentID string    `json:"paymentId"`
    RefundID  string    `json:"refundId"`
    Amount    float64   `json:"amount"`
    Timestamp time.Time `json:"timestamp"`
}
```

## Gateway Integration Example

### Stripe Gateway
```go
type StripeGateway struct {
    client *stripe.Client
}

func (g *StripeGateway) AuthorizePayment(ctx context.Context, req PaymentRequest) (*PaymentResponse, error) {
    params := &stripe.PaymentIntentParams{
        Amount:   stripe.Int64(int64(req.Amount * 100)), // Convert to cents
        Currency: stripe.String(strings.ToLower(req.Currency)),
        CaptureMethod: stripe.String("manual"), // Two-phase commit
        Customer: stripe.String(req.CustomerID),
        PaymentMethod: stripe.String(req.PaymentMethodToken),
        Confirm: stripe.Bool(true),
    }

    pi, err := paymentintent.New(params)
    if err != nil {
        return nil, fmt.Errorf("stripe authorization failed: %w", err)
    }

    return &PaymentResponse{
        GatewayPaymentID: pi.ID,
        Status: mapStripeStatus(pi.Status),
        AuthorizedAt: time.Now(),
    }, nil
}

func (g *StripeGateway) CapturePayment(ctx context.Context, gatewayPaymentID string) error {
    params := &stripe.PaymentIntentCaptureParams{}
    _, err := paymentintent.Capture(gatewayPaymentID, params)
    return err
}

func (g *StripeGateway) RefundPayment(ctx context.Context, gatewayPaymentID string, amount float64) (*RefundResponse, error) {
    params := &stripe.RefundParams{
        PaymentIntent: stripe.String(gatewayPaymentID),
        Amount: stripe.Int64(int64(amount * 100)),
    }

    refund, err := refund.New(params)
    if err != nil {
        return nil, err
    }

    return &RefundResponse{
        GatewayRefundID: refund.ID,
        Status: RefundProcessed,
    }, nil
}
```

## Webhook Handler Example

```go
func (h *WebhookHandler) HandleStripeWebhook(c *fiber.Ctx) error {
    payload := c.Body()
    signature := c.Get("Stripe-Signature")

    event, err := webhook.ConstructEvent(payload, signature, h.config.StripeWebhookSecret)
    if err != nil {
        return c.Status(400).JSON(fiber.Map{"error": "Invalid signature"})
    }

    switch event.Type {
    case "payment_intent.succeeded":
        var pi stripe.PaymentIntent
        json.Unmarshal(event.Data.Raw, &pi)
        h.handlePaymentSuccess(pi.ID)

    case "payment_intent.payment_failed":
        var pi stripe.PaymentIntent
        json.Unmarshal(event.Data.Raw, &pi)
        h.handlePaymentFailure(pi.ID, pi.LastPaymentError.Message)

    case "charge.refunded":
        var charge stripe.Charge
        json.Unmarshal(event.Data.Raw, &charge)
        h.handleRefund(charge.PaymentIntent)
    }

    return c.SendStatus(200)
}
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
DB_NAME=payment_service
DB_USER=payment_service
DB_PASSWORD=***

# Redis
REDIS_HOST=redis
REDIS_PORT=6379

# Kafka
KAFKA_BROKERS=kafka:9092
KAFKA_GROUP_ID=payment-service
KAFKA_TOPIC_PAYMENT_EVENTS=payment.events
KAFKA_TOPIC_ORDER_EVENTS=order.events

# Stripe
STRIPE_API_KEY=***
STRIPE_WEBHOOK_SECRET=***
STRIPE_PUBLISHABLE_KEY=***

# PayPal
PAYPAL_CLIENT_ID=***
PAYPAL_CLIENT_SECRET=***
PAYPAL_MODE=live  # sandbox or live

# Security
ENCRYPTION_KEY=***
PCI_COMPLIANCE_MODE=true

# Observability
JAEGER_ENDPOINT=http://jaeger:14268/api/traces
LOG_LEVEL=info
```

## Implementation Tasks

### Phase 1: Setup
- [ ] Initialize Go project
- [ ] Set up Clean Architecture
- [ ] Configure GORM with PostgreSQL
- [ ] Set up migrations

### Phase 2: Domain Layer
- [ ] Define payment entities
- [ ] Create value objects
- [ ] Define domain events
- [ ] Create repository interfaces

### Phase 3: Use Case Layer
- [ ] Implement payment processing logic
- [ ] Add refund logic
- [ ] Implement retry mechanism
- [ ] Add validation

### Phase 4: Infrastructure Layer
- [ ] Implement repositories
- [ ] Integrate Stripe SDK
- [ ] Integrate PayPal SDK
- [ ] Set up Kafka

### Phase 5: API Layer
- [ ] Implement REST handlers
- [ ] Add webhook handlers
- [ ] Implement gRPC server
- [ ] Add security middleware

### Phase 6: Security
- [ ] Implement idempotency
- [ ] Add signature verification
- [ ] Encrypt sensitive data
- [ ] Add fraud detection hooks

### Phase 7: Testing
- [ ] Unit tests
- [ ] Integration tests with mocked gateways
- [ ] Webhook testing
- [ ] Security testing

## Security Considerations

### PCI DSS Compliance
- Never store raw card numbers
- Use gateway tokenization
- Encrypt data at rest
- Secure transmission (TLS 1.3)
- Regular security audits

### Idempotency
```go
func (uc *PaymentUseCase) ProcessPayment(ctx context.Context, req PaymentRequest) (*Payment, error) {
    // Check idempotency key
    if req.IdempotencyKey != "" {
        existing, err := uc.repo.FindByIdempotencyKey(ctx, req.IdempotencyKey)
        if err == nil {
            return existing, nil // Return existing payment
        }
    }

    // Process new payment
    payment := &Payment{
        IdempotencyKey: req.IdempotencyKey,
        // ... other fields
    }

    // Save and process
    // ...
}
```

## Testing Requirements

### Unit Tests
- [ ] Payment processing logic
- [ ] Refund calculations
- [ ] Status transitions
- [ ] Validation rules

### Integration Tests
- [ ] Database operations
- [ ] Kafka event handling
- [ ] Gateway integration (mocked)
- [ ] Webhook handling

### Security Tests
- [ ] Idempotency enforcement
- [ ] Signature verification
- [ ] Encryption/decryption
- [ ] PCI compliance checks

## Success Criteria
- 99.99% uptime
- Payment processing < 3 seconds (p95)
- Zero data breaches
- 100% idempotency guarantee
- PCI DSS Level 1 compliant
- Automatic retry success > 80%
