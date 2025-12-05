# FX Rate Service (Go)

## Overview
Foreign exchange rate service built with Go. Provides real-time and historical currency conversion rates for multi-currency support across the e-commerce platform.

## Technology Stack
- **Language**: Go 1.21+
- **Framework**: Fiber / Echo
- **Database**: PostgreSQL (Code-First GORM)
- **Cache**: Redis (heavily cached)
- **External APIs**: Open Exchange Rates API, Fixer.io (OSS alternatives)
- **Scheduler**: Cron jobs for rate updates
- **API**: REST + gRPC

## Core Responsibilities

### 1. **Exchange Rate Management**
- Fetch current exchange rates from external providers
- Store historical rates
- Calculate currency conversions
- Support multiple base currencies
- Rate change tracking

### 2. **Multi-Currency Pricing**
- Convert product prices to user's currency
- Calculate order totals in different currencies
- Apply regional pricing rules
- Handle currency rounding rules

### 3. **Rate Caching**
- Aggressive Redis caching (rates change infrequently)
- Fallback to database if cache miss
- Background rate refresh
- Cache invalidation strategy

### 4. **Historical Rates**
- Store historical exchange rates
- Provide rates for specific dates
- Rate trend analysis
- Audit trail for conversions

### 5. **Rate Provider Management**
- Multiple provider support
- Provider failover
- Rate aggregation from multiple sources
- Provider priority configuration

### 6. **Currency Configuration**
- Supported currencies list
- Currency symbols and formatting
- Decimal places per currency
- Currency active/inactive status

### 7. **Conversion API**
- Simple conversion (amount, from, to)
- Bulk conversion
- Multi-currency conversion
- Rate lookup

### 8. **Rate Alerts**
- Alert on significant rate changes
- Rate threshold monitoring
- Integration with notification service

## Architecture

```
fx-rate-service/
├── cmd/
│   └── main.go
├── internal/
│   ├── domain/
│   │   ├── entity/
│   │   │   ├── exchange_rate.go
│   │   │   ├── currency.go
│   │   │   └── conversion_log.go
│   │   ├── valueobject/
│   │   │   └── money.go
│   │   └── repository/
│   │       ├── rate_repository.go
│   │       └── currency_repository.go
│   │
│   ├── usecase/
│   │   ├── dto/
│   │   │   ├── conversion_dto.go
│   │   │   └── rate_dto.go
│   │   ├── conversion_usecase.go
│   │   ├── rate_sync_usecase.go
│   │   └── currency_usecase.go
│   │
│   ├── adapter/
│   │   ├── http/
│   │   │   ├── handler/
│   │   │   │   ├── conversion_handler.go
│   │   │   │   ├── rate_handler.go
│   │   │   │   └── currency_handler.go
│   │   │   └── router.go
│   │   └── grpc/
│   │       ├── proto/
│   │       │   └── fx_rate.proto
│   │       └── server/
│   │           └── fx_rate_server.go
│   │
│   └── infrastructure/
│       ├── db/
│       │   ├── postgres.go
│       │   ├── migration/
│       │   │   └── 001_initial_schema.go
│       │   └── repository/
│       │       ├── rate_repository_impl.go
│       │       └── currency_repository_impl.go
│       ├── cache/
│       │   └── redis_cache.go
│       ├── provider/
│       │   ├── provider_interface.go
│       │   ├── openexchangerates_provider.go
│       │   ├── fixer_provider.go
│       │   └── provider_factory.go
│       ├── scheduler/
│       │   └── rate_sync_job.go
│       └── config/
│           └── config.go
│
├── pkg/
│   ├── converter/
│   │   └── currency_converter.go
│   └── formatter/
│       └── currency_formatter.go
│
├── go.mod
├── Dockerfile
└── README.md
```

## Domain Models (GORM - Code-First)

### ExchangeRate Entity
```go
type ExchangeRate struct {
    RateID      uuid.UUID `gorm:"type:uuid;primaryKey;default:gen_random_uuid()"`
    BaseCurrency string   `gorm:"type:varchar(3);not null;index:idx_rate_lookup"`
    TargetCurrency string `gorm:"type:varchar(3);not null;index:idx_rate_lookup"`
    Rate        float64   `gorm:"type:decimal(20,10);not null"`
    Provider    string    `gorm:"type:varchar(50);not null"`

    Date        time.Time `gorm:"type:date;not null;index:idx_rate_lookup"`

    CreatedAt   time.Time `gorm:"not null;default:now()"`
    UpdatedAt   time.Time `gorm:"not null;default:now()"`
}

// Composite index for fast lookups
// CREATE INDEX idx_rate_lookup ON exchange_rates(base_currency, target_currency, date DESC);
```

### Currency Entity
```go
type Currency struct {
    CurrencyCode string `gorm:"type:varchar(3);primaryKey"`
    Name         string `gorm:"type:varchar(100);not null"`
    Symbol       string `gorm:"type:varchar(10)"`
    DecimalPlaces int   `gorm:"not null;default:2"`
    IsActive     bool   `gorm:"not null;default:true"`

    CreatedAt time.Time `gorm:"not null;default:now()"`
    UpdatedAt time.Time `gorm:"not null;default:now()"`
}
```

### ConversionLog Entity (Optional - for auditing)
```go
type ConversionLog struct {
    LogID       uuid.UUID `gorm:"type:uuid;primaryKey;default:gen_random_uuid()"`
    FromCurrency string   `gorm:"type:varchar(3);not null"`
    ToCurrency   string   `gorm:"type:varchar(3);not null"`
    FromAmount   float64  `gorm:"type:decimal(19,4);not null"`
    ToAmount     float64  `gorm:"type:decimal(19,4);not null"`
    Rate         float64  `gorm:"type:decimal(20,10);not null"`
    RequestedBy  *uuid.UUID `gorm:"type:uuid"`

    CreatedAt time.Time `gorm:"not null;default:now()"`
}
```

## API Endpoints

### REST API

#### Conversion
```
POST   /api/v1/convert                    # Convert currency
POST   /api/v1/convert/bulk               # Bulk conversion
GET    /api/v1/convert?from=USD&to=EUR&amount=100
```

#### Rates
```
GET    /api/v1/rates                      # Get all current rates
GET    /api/v1/rates/:base                # Rates for base currency
GET    /api/v1/rates/:base/:target        # Specific rate
GET    /api/v1/rates/history              # Historical rates
```

#### Currencies
```
GET    /api/v1/currencies                 # List supported currencies
GET    /api/v1/currencies/:code           # Get currency details
POST   /api/v1/currencies                 # Add currency (admin)
PUT    /api/v1/currencies/:code           # Update currency (admin)
```

#### Admin
```
POST   /api/v1/admin/sync                 # Trigger rate sync
GET    /api/v1/admin/providers            # List providers
PUT    /api/v1/admin/providers/:name      # Update provider config
```

#### Health
```
GET    /health                            # Health check
GET    /metrics                           # Prometheus metrics
```

### gRPC API

```protobuf
service FxRateService {
  rpc Convert(ConvertRequest) returns (ConvertResponse);
  rpc GetRate(GetRateRequest) returns (RateResponse);
  rpc GetRates(GetRatesRequest) returns (RatesResponse);
  rpc GetCurrencies(Empty) returns (CurrenciesResponse);
}

message ConvertRequest {
  double amount = 1;
  string from_currency = 2;
  string to_currency = 3;
}

message ConvertResponse {
  double amount = 1;
  double converted_amount = 2;
  double rate = 3;
  string from_currency = 4;
  string to_currency = 5;
  string timestamp = 6;
}
```

## Rate Provider Interface

```go
type RateProvider interface {
    GetRates(ctx context.Context, baseCurrency string) (map[string]float64, error)
    GetRate(ctx context.Context, from, to string) (float64, error)
    GetProviderName() string
}
```

### OpenExchangeRates Provider
```go
type OpenExchangeRatesProvider struct {
    apiKey     string
    httpClient *http.Client
}

func (p *OpenExchangeRatesProvider) GetRates(ctx context.Context, baseCurrency string) (map[string]float64, error) {
    url := fmt.Sprintf("https://openexchangerates.org/api/latest.json?app_id=%s&base=%s",
        p.apiKey, baseCurrency)

    resp, err := p.httpClient.Get(url)
    if err != nil {
        return nil, err
    }
    defer resp.Body.Close()

    var result struct {
        Rates map[string]float64 `json:"rates"`
    }

    if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
        return nil, err
    }

    return result.Rates, nil
}
```

## Conversion Use Case

```go
type ConversionUseCase struct {
    rateRepo  repository.RateRepository
    cache     cache.Cache
}

func (uc *ConversionUseCase) Convert(ctx context.Context, req ConvertRequest) (*ConvertResponse, error) {
    // Same currency, no conversion needed
    if req.FromCurrency == req.ToCurrency {
        return &ConvertResponse{
            Amount:           req.Amount,
            ConvertedAmount:  req.Amount,
            Rate:             1.0,
            FromCurrency:     req.FromCurrency,
            ToCurrency:       req.ToCurrency,
        }, nil
    }

    // Try cache first
    cacheKey := fmt.Sprintf("rate:%s:%s", req.FromCurrency, req.ToCurrency)
    cachedRate, err := uc.cache.Get(ctx, cacheKey)

    var rate float64
    if err == nil {
        rate, _ = strconv.ParseFloat(cachedRate, 64)
    } else {
        // Get from database
        rateEntity, err := uc.rateRepo.GetLatestRate(ctx, req.FromCurrency, req.ToCurrency)
        if err != nil {
            return nil, fmt.Errorf("rate not found: %w", err)
        }
        rate = rateEntity.Rate

        // Cache for 5 minutes
        uc.cache.Set(ctx, cacheKey, fmt.Sprintf("%.10f", rate), 5*time.Minute)
    }

    convertedAmount := req.Amount * rate

    // Round based on target currency decimal places
    currency, _ := uc.currencyRepo.GetByCurrencyCode(ctx, req.ToCurrency)
    convertedAmount = roundToCurrency(convertedAmount, currency.DecimalPlaces)

    return &ConvertResponse{
        Amount:          req.Amount,
        ConvertedAmount: convertedAmount,
        Rate:            rate,
        FromCurrency:    req.FromCurrency,
        ToCurrency:      req.ToCurrency,
        Timestamp:       time.Now(),
    }, nil
}

func roundToCurrency(amount float64, decimalPlaces int) float64 {
    multiplier := math.Pow(10, float64(decimalPlaces))
    return math.Round(amount*multiplier) / multiplier
}
```

## Rate Sync Job

```go
type RateSyncJob struct {
    providers []RateProvider
    rateRepo  repository.RateRepository
}

func (j *RateSyncJob) SyncRates(ctx context.Context) error {
    baseCurrencies := []string{"USD", "EUR", "GBP"}

    for _, baseCurrency := range baseCurrencies {
        for _, provider := range j.providers {
            rates, err := provider.GetRates(ctx, baseCurrency)
            if err != nil {
                log.Printf("Failed to fetch rates from %s: %v", provider.GetProviderName(), err)
                continue
            }

            // Save rates to database
            for targetCurrency, rate := range rates {
                exchangeRate := &ExchangeRate{
                    BaseCurrency:   baseCurrency,
                    TargetCurrency: targetCurrency,
                    Rate:           rate,
                    Provider:       provider.GetProviderName(),
                    Date:           time.Now().UTC().Truncate(24 * time.Hour),
                }

                if err := j.rateRepo.Upsert(ctx, exchangeRate); err != nil {
                    log.Printf("Failed to save rate: %v", err)
                }
            }

            log.Printf("Synced %d rates for %s from %s", len(rates), baseCurrency, provider.GetProviderName())
            break // Success, no need to try other providers
        }
    }

    return nil
}

// Schedule: Run every hour
func (j *RateSyncJob) Schedule() {
    ticker := time.NewTicker(1 * time.Hour)
    go func() {
        for range ticker.C {
            ctx := context.Background()
            if err := j.SyncRates(ctx); err != nil {
                log.Printf("Rate sync failed: %v", err)
            }
        }
    }()
}
```

## Currency Formatter

```go
type CurrencyFormatter struct {
    currencyRepo repository.CurrencyRepository
}

func (f *CurrencyFormatter) Format(amount float64, currencyCode string) string {
    currency, err := f.currencyRepo.GetByCurrencyCode(context.Background(), currencyCode)
    if err != nil {
        return fmt.Sprintf("%.2f %s", amount, currencyCode)
    }

    format := fmt.Sprintf("%%.%df %%s", currency.DecimalPlaces)
    return fmt.Sprintf(format, amount, currency.Symbol)
}

// Examples:
// Format(100.5, "USD") → "100.50 $"
// Format(100.5, "EUR") → "100.50 €"
// Format(100.5, "JPY") → "101 ¥"  (no decimals for JPY)
```

## Environment Variables
```bash
# Application
APP_PORT=8080
GRPC_PORT=9090

# Database
DB_HOST=postgres
DB_PORT=5432
DB_NAME=fx_rate_service
DB_USER=fx_rate_service
DB_PASSWORD=***

# Redis
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_TTL=300  # 5 minutes

# Rate Providers
OPENEXCHANGERATES_API_KEY=***
FIXER_API_KEY=***
PRIMARY_PROVIDER=openexchangerates

# Sync Job
RATE_SYNC_INTERVAL=1h
BASE_CURRENCIES=USD,EUR,GBP

# Supported Currencies
SUPPORTED_CURRENCIES=USD,EUR,GBP,JPY,THB,SGD,AUD,CAD,CNY,INR

LOG_LEVEL=info
```

## Implementation Tasks

### Phase 1: Setup
- [ ] Initialize Go project
- [ ] Set up GORM with PostgreSQL
- [ ] Configure Redis
- [ ] Create database schema

### Phase 2: Domain Layer
- [ ] Define entities (ExchangeRate, Currency)
- [ ] Create repository interfaces
- [ ] Implement value objects

### Phase 3: Use Case Layer
- [ ] Implement conversion logic
- [ ] Create rate sync use case
- [ ] Add caching layer
- [ ] Implement rounding rules

### Phase 4: Infrastructure Layer
- [ ] Implement repositories
- [ ] Integrate rate providers (OpenExchangeRates, Fixer)
- [ ] Set up rate sync job
- [ ] Configure Redis caching

### Phase 5: API Layer
- [ ] Implement REST handlers
- [ ] Create gRPC service
- [ ] Add health checks
- [ ] Implement metrics

### Phase 6: Testing
- [ ] Unit tests (conversion logic)
- [ ] Integration tests (provider integration)
- [ ] Cache testing
- [ ] Performance testing

## Supported Currencies (Initial)

```go
var DefaultCurrencies = []Currency{
    {CurrencyCode: "USD", Name: "US Dollar", Symbol: "$", DecimalPlaces: 2},
    {CurrencyCode: "EUR", Name: "Euro", Symbol: "€", DecimalPlaces: 2},
    {CurrencyCode: "GBP", Name: "British Pound", Symbol: "£", DecimalPlaces: 2},
    {CurrencyCode: "JPY", Name: "Japanese Yen", Symbol: "¥", DecimalPlaces: 0},
    {CurrencyCode: "THB", Name: "Thai Baht", Symbol: "฿", DecimalPlaces: 2},
    {CurrencyCode: "SGD", Name: "Singapore Dollar", Symbol: "S$", DecimalPlaces: 2},
    {CurrencyCode: "AUD", Name: "Australian Dollar", Symbol: "A$", DecimalPlaces: 2},
    {CurrencyCode: "CAD", Name: "Canadian Dollar", Symbol: "C$", DecimalPlaces: 2},
    {CurrencyCode: "CNY", Name: "Chinese Yuan", Symbol: "¥", DecimalPlaces: 2},
    {CurrencyCode: "INR", Name: "Indian Rupee", Symbol: "₹", DecimalPlaces: 2},
}
```

## Testing Requirements

### Unit Tests
- [ ] Conversion calculations
- [ ] Rounding logic
- [ ] Currency formatting
- [ ] Cache key generation

### Integration Tests
- [ ] Database operations
- [ ] Provider integration (mocked)
- [ ] Redis caching
- [ ] Rate sync job

### Performance Tests
- [ ] Concurrent conversions
- [ ] Cache performance
- [ ] API response time

## Success Criteria
- 99.99% uptime
- Conversion API < 10ms (p95) with cache
- Rate freshness < 1 hour
- Support 50K+ conversions/second
- Cache hit rate > 95%
- Zero conversion errors
