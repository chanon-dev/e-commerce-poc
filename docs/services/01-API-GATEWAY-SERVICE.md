# API Gateway Service

## Overview
Kong Gateway OSS-based API Gateway that serves as the single entry point for all client requests. Provides routing, load balancing, authentication, rate limiting, and geo-routing capabilities.

## Technology Stack
- **Gateway**: Kong Gateway OSS
- **Authentication**: Keycloak Integration
- **Database**: PostgreSQL (for Kong configuration)
- **Cache**: Redis (for rate limiting, caching)
- **Service Discovery**: Consul

## Core Responsibilities

### 1. **Request Routing & Load Balancing**
- Route incoming requests to appropriate microservices
- Implement intelligent load balancing across service instances
- Support for both REST and gRPC protocols
- Path-based and header-based routing

### 2. **Geo-Routing**
- Detect user's geographic location (via IP geolocation)
- Route requests to nearest regional deployment
- Minimize network latency for global users
- Support multi-region active-active architecture

### 3. **Authentication & Authorization**
- Integrate with Keycloak for OAuth2/OIDC
- Validate JWT tokens from Keycloak
- Extract user context and forward to downstream services
- Support API key authentication for service-to-service calls

### 4. **Rate Limiting & Throttling**
- Per-user rate limiting
- Per-API endpoint rate limiting
- Distributed rate limiting using Redis
- Protect backend services from overload

### 5. **Security**
- CORS handling
- Request/Response transformation
- IP whitelisting/blacklisting
- DDoS protection
- Request size limits

### 6. **Observability**
- Request/Response logging
- Metrics collection (latency, throughput, error rates)
- Distributed tracing (Jaeger integration)
- Health check endpoints

### 7. **API Versioning**
- Support multiple API versions
- Backward compatibility
- Version-based routing

### 8. **Circuit Breaking & Resilience**
- Circuit breaker pattern for failing services
- Automatic retry with exponential backoff
- Timeout management
- Fallback responses

## API Endpoints

### Gateway Management
```
GET    /health                    # Health check
GET    /metrics                   # Prometheus metrics
GET    /admin/routes              # List all routes (admin only)
POST   /admin/routes              # Add new route (admin only)
DELETE /admin/routes/:id          # Remove route (admin only)
```

### Proxied Services
```
# All service endpoints are proxied through:
/api/v1/users/*          → User Service
/api/v1/products/*       → Product Service
/api/v1/inventory/*      → Inventory Service
/api/v1/cart/*           → Cart Service
/api/v1/orders/*         → Order Service
/api/v1/payments/*       → Payment Service
/api/v1/search/*         → Search Indexer Service
/api/v1/fx-rates/*       → FX Rate Service
```

## Configuration Structure

### Kong Configuration Files
```
/config
  /kong.yml              # Main Kong configuration
  /plugins
    /rate-limiting.yml   # Rate limiting plugin config
    /cors.yml            # CORS plugin config
    /jwt.yml             # JWT authentication config
    /geo-routing.yml     # Custom geo-routing plugin
  /routes
    /user-service.yml    # User service routes
    /product-service.yml # Product service routes
    /order-service.yml   # Order service routes
    ...
  /upstreams
    /regions.yml         # Regional upstream definitions
```

### Environment Variables
```
KONG_DATABASE=postgres
KONG_PG_HOST=postgres
KONG_PG_PORT=5432
KONG_PG_DATABASE=kong
KONG_PG_USER=kong
KONG_PG_PASSWORD=***

KONG_PROXY_LISTEN=0.0.0.0:8000, 0.0.0.0:8443 ssl
KONG_ADMIN_LISTEN=0.0.0.0:8001

REDIS_HOST=redis
REDIS_PORT=6379

KEYCLOAK_URL=http://keycloak:8080
KEYCLOAK_REALM=ecommerce

CONSUL_HOST=consul
CONSUL_PORT=8500

# Geo-routing
GEO_REGIONS=us-west,us-east,eu-west,ap-southeast
DEFAULT_REGION=us-west
```

## Implementation Tasks

### Phase 1: Basic Setup
- [ ] Set up Kong Gateway with PostgreSQL
- [ ] Configure basic routing to downstream services
- [ ] Implement health check endpoints
- [ ] Set up Docker containerization

### Phase 2: Authentication & Security
- [ ] Integrate Keycloak JWT validation
- [ ] Configure CORS policies
- [ ] Implement rate limiting with Redis
- [ ] Add request/response size limits
- [ ] Set up IP filtering

### Phase 3: Geo-Routing
- [ ] Implement IP geolocation detection
- [ ] Configure regional upstreams
- [ ] Implement geo-aware routing logic
- [ ] Add region failover mechanisms

### Phase 4: Resilience
- [ ] Configure circuit breakers
- [ ] Implement retry logic
- [ ] Set up timeout policies
- [ ] Add fallback responses

### Phase 5: Observability
- [ ] Integrate Prometheus metrics exporter
- [ ] Configure Jaeger tracing
- [ ] Set up centralized logging (Loki)
- [ ] Create Grafana dashboards

### Phase 6: Advanced Features
- [ ] Implement request transformation
- [ ] Add response caching
- [ ] Configure API versioning
- [ ] Implement service discovery with Consul

## Custom Plugins (Lua)

### Geo-Routing Plugin
```lua
-- /plugins/geo-routing/handler.lua
-- Detects user location and routes to nearest region
```

### Multi-Currency Plugin
```lua
-- /plugins/multi-currency/handler.lua
-- Detects and validates currency headers
```

## Deployment Architecture

### Multi-Region Setup
```
                    Global DNS (GeoDNS)
                           |
        +------------------+------------------+
        |                  |                  |
   US Region          EU Region         APAC Region
   Kong GW            Kong GW            Kong GW
   (Active)           (Active)           (Active)
```

### High Availability
- Minimum 3 Kong instances per region
- PostgreSQL with replication
- Redis cluster for rate limiting
- Load balancer in front of Kong instances

## Monitoring & Alerts

### Key Metrics
- Request rate (req/sec)
- Response time (p50, p95, p99)
- Error rate (4xx, 5xx)
- Upstream service health
- Rate limit hits

### Alerts
- High error rate (> 5%)
- High latency (p95 > 500ms)
- Service unavailability
- Rate limit threshold exceeded

## Testing Requirements

### Unit Tests
- Plugin logic testing
- Configuration validation

### Integration Tests
- End-to-end request routing
- Authentication flow
- Rate limiting behavior
- Circuit breaker activation

### Load Tests
- Concurrent request handling
- Rate limiting under load
- Failover scenarios

## Documentation Deliverables
- [ ] Kong configuration guide
- [ ] Plugin development guide
- [ ] Routing rules documentation
- [ ] Security policies documentation
- [ ] Monitoring playbook
- [ ] Troubleshooting guide

## Dependencies
- Keycloak (Identity Service)
- Consul (Service Discovery)
- PostgreSQL (Kong DB)
- Redis (Rate Limiting)
- Prometheus/Grafana (Monitoring)

## Success Criteria
- 99.99% uptime
- p95 latency < 50ms (gateway overhead)
- Handle 100K+ requests/second
- Automatic failover < 5 seconds
- Zero-downtime deployments
