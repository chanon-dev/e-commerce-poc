# E-Commerce Platform - Service Overview

## Introduction

This document provides a comprehensive overview of all microservices in the E-Commerce Platform. The platform follows a microservices architecture with Clean Architecture, Domain-Driven Design (DDD), and Event-Driven patterns.

## Architecture Principles

- **Clean Architecture**: Separation of concerns with clear boundaries
- **Domain-Driven Design (DDD)**: Business logic at the core
- **Event-Driven**: Asynchronous communication via Kafka
- **Code-First Database**: All services use code-first approach for database schema
- **Multi-Region**: Active-active deployment across multiple regions
- **Open Source Only**: All technologies are OSS/Free

## Technology Stack Summary

| Component | Technology |
|-----------|-----------|
| **Backend Languages** | .NET 10, Java 17+ (Spring Boot 3), Go 1.21+ |
| **Databases** | PostgreSQL (Code-First) |
| **Cache** | Redis OSS |
| **Search** | OpenSearch / Elasticsearch OSS |
| **Messaging** | Apache Kafka OSS |
| **API Gateway** | Kong Gateway OSS |
| **Identity** | Keycloak OSS |
| **Service Discovery** | Consul OSS |
| **Secrets Management** | Vault OSS |
| **Monitoring** | Prometheus, Grafana, Loki, Jaeger |

## Service Catalog

### 1. API Gateway Service
- **Documentation**: [01-API-GATEWAY-SERVICE.md](01-API-GATEWAY-SERVICE.md)
- **Technology**: Kong Gateway OSS
- **Purpose**: Single entry point for all client requests
- **Key Features**:
  - Request routing and load balancing
  - Geo-routing for multi-region
  - Authentication & rate limiting
  - Circuit breaking
- **Port**: 8000 (HTTP), 8443 (HTTPS), 8001 (Admin)

### 2. Identity & Auth Service
- **Documentation**: [02-IDENTITY-AUTH-SERVICE.md](02-IDENTITY-AUTH-SERVICE.md)
- **Technology**: Keycloak OSS
- **Purpose**: Centralized authentication and authorization
- **Key Features**:
  - OAuth 2.0 / OpenID Connect (OIDC)
  - Multi-factor authentication (MFA)
  - Social login integration
  - Role-based access control (RBAC)
  - Multi-region session management
- **Port**: 8080

### 3. User Service
- **Documentation**: [03-USER-SERVICE.md](03-USER-SERVICE.md)
- **Technology**: .NET 10 (ASP.NET Core)
- **Database**: PostgreSQL (EF Core Code-First)
- **Purpose**: User profile and account management
- **Key Features**:
  - User profile management
  - Address management
  - User preferences
  - Multi-region user data
- **APIs**: REST + gRPC
- **Port**: 8080 (HTTP), 9090 (gRPC)

### 4. Product Service
- **Documentation**: [04-PRODUCT-SERVICE.md](04-PRODUCT-SERVICE.md)
- **Technology**: Java Spring Boot 3
- **Database**: PostgreSQL (JPA Code-First + Flyway)
- **Search**: OpenSearch
- **Purpose**: Product catalog management
- **Key Features**:
  - Product CRUD operations
  - Category management
  - Product variants
  - Multi-currency pricing
  - Review and ratings
  - Search indexing
- **APIs**: REST + gRPC
- **Port**: 8080 (HTTP), 9090 (gRPC)

### 5. Inventory Service
- **Documentation**: [05-INVENTORY-SERVICE.md](05-INVENTORY-SERVICE.md)
- **Technology**: Go (Fiber/Echo)
- **Database**: PostgreSQL (GORM Code-First)
- **Purpose**: Stock and inventory management
- **Key Features**:
  - Real-time stock tracking
  - Stock reservation (TTL-based)
  - Multi-warehouse management
  - Inventory allocation
  - Low stock alerts
- **APIs**: REST + gRPC
- **Port**: 8080 (HTTP), 9090 (gRPC)

### 6. Cart Service
- **Documentation**: [06-CART-SERVICE.md](06-CART-SERVICE.md)
- **Technology**: .NET 10 (ASP.NET Core)
- **Primary Storage**: Redis (with PostgreSQL backup)
- **Purpose**: Shopping cart management
- **Key Features**:
  - Real-time cart operations
  - Cart persistence (7 days authenticated, 24h anonymous)
  - Cart calculations (pricing, tax, shipping)
  - Cart merge (anonymous → authenticated)
  - Save for later
- **APIs**: REST + gRPC
- **Port**: 8080 (HTTP), 9090 (gRPC)

### 7. Order Service
- **Documentation**: [07-ORDER-SERVICE.md](07-ORDER-SERVICE.md)
- **Technology**: Java Spring Boot 3
- **Database**: PostgreSQL (JPA Code-First + Flyway)
- **Purpose**: Order lifecycle management
- **Key Features**:
  - Order creation and processing
  - Order state machine
  - Transactional outbox pattern
  - Multi-currency orders
  - Order history
  - Integration with payment and fulfillment
- **APIs**: REST + gRPC
- **Port**: 8080 (HTTP), 9090 (gRPC)

### 8. Payment Service
- **Documentation**: [08-PAYMENT-SERVICE.md](08-PAYMENT-SERVICE.md)
- **Technology**: Go (Fiber/Echo)
- **Database**: PostgreSQL (GORM Code-First)
- **Payment Gateways**: Stripe, PayPal
- **Purpose**: Payment processing and management
- **Key Features**:
  - Multi-gateway payment processing
  - Two-phase commit (authorize + capture)
  - Refund management
  - PCI DSS compliance
  - Idempotency guarantees
  - Webhook handling
- **APIs**: REST + gRPC
- **Port**: 8080 (HTTP), 9090 (gRPC)

### 9. Notification Service
- **Documentation**: [09-NOTIFICATION-SERVICE.md](09-NOTIFICATION-SERVICE.md)
- **Technology**: .NET 10 (ASP.NET Core)
- **Database**: PostgreSQL (EF Core Code-First)
- **Purpose**: Multi-channel notification delivery
- **Key Features**:
  - Email notifications (SMTP)
  - SMS notifications (Twilio)
  - Push notifications (FCM)
  - In-app notifications
  - Template management
  - User preferences
  - Delivery tracking
- **APIs**: REST
- **Port**: 8080

### 10. Search Indexer Service
- **Documentation**: [10-SEARCH-INDEXER-SERVICE.md](10-SEARCH-INDEXER-SERVICE.md)
- **Technology**: Go or Java Spring Boot
- **Search Engine**: OpenSearch
- **Purpose**: Product search and indexing
- **Key Features**:
  - Real-time product indexing
  - Full-text search
  - Faceted search (filters)
  - Auto-suggestions
  - Multi-language search
  - Search analytics
- **APIs**: REST + gRPC
- **Port**: 8080 (HTTP), 9090 (gRPC)

### 11. FX Rate Service
- **Documentation**: [11-FX-RATE-SERVICE.md](11-FX-RATE-SERVICE.md)
- **Technology**: Go (Fiber/Echo)
- **Database**: PostgreSQL (GORM Code-First)
- **Cache**: Redis (heavily cached)
- **Purpose**: Currency conversion and FX rates
- **Key Features**:
  - Real-time exchange rates
  - Multi-currency support
  - Historical rates
  - Rate caching (5 min TTL)
  - Multiple provider support
  - Currency formatting
- **APIs**: REST + gRPC
- **Port**: 8080 (HTTP), 9090 (gRPC)

### 12. Global Config Service
- **Documentation**: [12-GLOBAL-CONFIG-SERVICE.md](12-GLOBAL-CONFIG-SERVICE.md)
- **Technology**: Consul + Vault
- **Purpose**: Configuration and secrets management
- **Key Features**:
  - Service discovery (Consul)
  - Centralized configuration (Consul KV)
  - Secrets management (Vault)
  - Dynamic credentials
  - Health checking
  - Multi-region support
- **Ports**:
  - Consul: 8500 (HTTP), 8600 (DNS)
  - Vault: 8200 (HTTPS)

## Service Communication Patterns

### Synchronous Communication
- **REST APIs**: For client-facing operations
- **gRPC**: For service-to-service communication (high performance)

### Asynchronous Communication
- **Kafka Events**: For event-driven workflows and data synchronization

## Service Dependencies

```
┌─────────────────┐
│  API Gateway    │ (Entry Point)
└────────┬────────┘
         │
    ┌────┴────────────────────────────┐
    │                                 │
┌───▼───────┐                   ┌────▼─────┐
│ Keycloak  │                   │ Services │
│ (Auth)    │                   │          │
└───────────┘                   └────┬─────┘
                                     │
         ┌───────────────────────────┼────────────────────────┐
         │                           │                        │
    ┌────▼─────┐              ┌─────▼──────┐         ┌──────▼───────┐
    │   User   │              │  Product   │         │  Inventory   │
    │ Service  │              │  Service   │         │   Service    │
    └────┬─────┘              └─────┬──────┘         └──────┬───────┘
         │                          │                       │
         │                    ┌─────▼──────┐               │
         │                    │   Search   │               │
         │                    │  Indexer   │               │
         │                    └────────────┘               │
         │                                                 │
    ┌────▼─────────────────────────────────────────────┬──▼─────┐
    │                  Cart Service                     │        │
    └────┬──────────────────────────────────────────────┘        │
         │                                                        │
    ┌────▼─────┐                                                 │
    │  Order   │                                                 │
    │ Service  │                                                 │
    └────┬─────┘                                                 │
         │                                                       │
    ┌────▼─────────┐                                            │
    │   Payment    │◄───────────────────────────────────────────┘
    │   Service    │
    └────┬─────────┘
         │
    ┌────▼──────────┐
    │ Notification  │
    │   Service     │
    └───────────────┘

Cross-Cutting:
- FX Rate Service (used by all services for currency conversion)
- Consul (service discovery for all services)
- Vault (secrets for all services)
- Kafka (event bus for all services)
```

## Event Flow Examples

### 1. Order Creation Flow

```
User → API Gateway → Cart Service (get cart)
                  ↓
            Order Service (create order)
                  ↓
              Kafka: order.created event
                  ↓
    ┌─────────────┴──────────────┐
    ↓                            ↓
Inventory Service          Notification Service
(reserve stock)           (send confirmation email)
    ↓
Kafka: inventory.reserved
    ↓
Payment Service
(process payment)
    ↓
Kafka: payment.successful
    ↓
    ├─→ Order Service (update status)
    ├─→ Inventory Service (commit reservation)
    └─→ Notification Service (payment confirmation)
```

### 2. Product Update Flow

```
Admin → API Gateway → Product Service (update product)
                           ↓
                   Kafka: product.updated event
                           ↓
              ┌────────────┴───────────┐
              ↓                        ↓
      Search Indexer              Cart Service
      (update index)             (update cached prices)
```

## Kafka Topics

| Topic | Producers | Consumers | Purpose |
|-------|-----------|-----------|---------|
| `user.events` | User Service | Notification Service | User lifecycle events |
| `product.events` | Product Service | Search Indexer, Cart Service | Product changes |
| `inventory.events` | Inventory Service | Product Service, Order Service | Stock updates |
| `cart.events` | Cart Service | Analytics Services | Cart activities |
| `order.events` | Order Service | Payment, Inventory, Notification | Order lifecycle |
| `payment.events` | Payment Service | Order, Notification | Payment status |

## Database Schema Strategy

All services follow **Code-First** approach:

- **.NET Services**: Entity Framework Core Migrations
- **Java Services**: JPA/Hibernate + Flyway/Liquibase
- **Go Services**: GORM/Ent + Atlas/go-migrate

### Database Per Service Pattern

Each service has its own PostgreSQL database:

```
- user_service_db
- product_service_db
- inventory_service_db
- cart_service_db (+ Redis primary)
- order_service_db
- payment_service_db
- notification_service_db
- fx_rate_service_db
```

## Multi-Region Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Global DNS (GeoDNS)                 │
└───────────────────┬─────────────────┬───────────────────┘
                    │                 │
        ┌───────────▼─────┐   ┌───────▼──────────┐
        │   US-WEST        │   │   EU-WEST        │
        │   Region         │   │   Region         │
        ├──────────────────┤   ├──────────────────┤
        │ - All Services   │   │ - All Services   │
        │ - PostgreSQL     │   │ - PostgreSQL     │
        │ - Redis          │   │ - Redis          │
        │ - Kafka          │   │ - Kafka          │
        │ - OpenSearch     │   │ - OpenSearch     │
        └──────────────────┘   └──────────────────┘
                    │                 │
        ┌───────────▼─────────────────▼───────────┐
        │     Cross-Region Replication            │
        │  - Keycloak Session Replication         │
        │  - Consul WAN Federation                │
        │  - Database Replication (read replicas) │
        └─────────────────────────────────────────┘
```

## Port Allocation

| Service | HTTP | gRPC | Admin |
|---------|------|------|-------|
| API Gateway | 8000/8443 | - | 8001 |
| Keycloak | 8080 | - | - |
| User Service | 8080 | 9090 | - |
| Product Service | 8080 | 9090 | - |
| Inventory Service | 8080 | 9090 | - |
| Cart Service | 8080 | 9090 | - |
| Order Service | 8080 | 9090 | - |
| Payment Service | 8080 | 9090 | - |
| Notification Service | 8080 | - | - |
| Search Indexer | 8080 | 9090 | - |
| FX Rate Service | 8080 | 9090 | - |
| Consul | 8500 | 8502 | - |
| Vault | 8200 | - | - |

## Implementation Checklist

### Infrastructure Setup
- [ ] Set up Consul cluster (3+ nodes per region)
- [ ] Set up Vault cluster (3 nodes for HA)
- [ ] Deploy Keycloak with PostgreSQL
- [ ] Configure Kong API Gateway
- [ ] Set up PostgreSQL clusters (per service)
- [ ] Deploy Redis clusters
- [ ] Set up Kafka clusters
- [ ] Deploy OpenSearch cluster
- [ ] Configure Prometheus/Grafana

### Service Implementation Order

**Phase 1: Foundation**
1. [ ] Global Config Service (Consul + Vault)
2. [ ] Identity & Auth Service (Keycloak)
3. [ ] API Gateway Service (Kong)

**Phase 2: Core Services**
4. [ ] User Service
5. [ ] FX Rate Service
6. [ ] Product Service

**Phase 3: E-Commerce Core**
7. [ ] Inventory Service
8. [ ] Cart Service
9. [ ] Search Indexer Service

**Phase 4: Transaction Services**
10. [ ] Order Service
11. [ ] Payment Service
12. [ ] Notification Service

### Testing Strategy
- [ ] Unit tests for each service
- [ ] Integration tests (API + Database)
- [ ] Event-driven workflow tests
- [ ] Performance/load tests
- [ ] Security tests
- [ ] Multi-region failover tests

### Monitoring & Observability
- [ ] Service health checks
- [ ] Prometheus metrics collection
- [ ] Grafana dashboards per service
- [ ] Distributed tracing (Jaeger)
- [ ] Centralized logging (Loki)
- [ ] Alert rules configuration

## Development Guidelines

### Code-First Database Approach
- Always define entities/models in code
- Generate migrations from code
- Never write SQL scripts directly
- Version control all migrations
- Test migrations in dev/staging before production

### API Design
- RESTful principles for client-facing APIs
- gRPC for service-to-service communication
- Consistent error responses
- API versioning (v1, v2, etc.)
- OpenAPI/Swagger documentation

### Event-Driven Design
- Publish events after transaction commit
- Use Outbox pattern for reliable event publishing
- Idempotent event handlers
- Event versioning and schema evolution
- Dead letter queues for failed events

### Security Best Practices
- All secrets in Vault (never in code/env files)
- JWT tokens for authentication
- mTLS for service-to-service communication
- Rate limiting on all public APIs
- Input validation on all endpoints
- SQL injection prevention (use ORMs)

## Deployment Strategy

### Docker Compose (Development)
- Each service has its own Dockerfile
- docker-compose.yml for local development
- Includes all dependencies (PostgreSQL, Redis, Kafka, etc.)

### Kubernetes (Production)
- Helm charts for each service
- Horizontal Pod Autoscaling (HPA)
- Persistent volumes for stateful services
- ConfigMaps and Secrets management
- Ingress for external access

### CI/CD Pipeline
- Jenkins for build automation
- ArgoCD for GitOps deployment
- Automated testing in pipeline
- Blue-green or canary deployments
- Automated rollback on failure

## Success Metrics

| Metric | Target |
|--------|--------|
| Overall Uptime | 99.99% |
| API Response Time (p95) | < 200ms |
| Order Completion Rate | > 95% |
| Payment Success Rate | > 98% |
| Search Latency | < 50ms |
| Event Processing Latency | < 100ms |
| Zero Data Loss | 100% |

## Next Steps

1. **Read individual service documentation** for detailed implementation guides
2. **Set up infrastructure** (Consul, Vault, Keycloak, databases)
3. **Implement services** following the recommended order
4. **Set up monitoring** and observability
5. **Test end-to-end workflows**
6. **Deploy to staging** for integration testing
7. **Load test** and optimize
8. **Deploy to production** with gradual rollout

## Additional Resources

- [Clean Architecture Principles](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Microservices Patterns](https://microservices.io/patterns/index.html)
- [Twelve-Factor App](https://12factor.net/)
- [Consul Documentation](https://www.consul.io/docs)
- [Vault Documentation](https://www.vaultproject.io/docs)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Kong Gateway Documentation](https://docs.konghq.com/)

---

**Document Version**: 1.0
**Last Updated**: 2025-01-05
**Maintained By**: Platform Architecture Team
