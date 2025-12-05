# E-Commerce Platform - Service Documentation

## Overview

ยินดีต้อนรับสู่เอกสารแต่ละ Service ของระบบ E-Commerce Platform

เอกสารชุดนี้ออกแบบมาเพื่อให้นักพัฒนาสามารถเข้าใจและลงมือ implement แต่ละ service ได้อย่างครบถ้วน

## 📚 Document Structure

### [00-SERVICE-OVERVIEW.md](./00-SERVICE-OVERVIEW.md)
**เริ่มต้นที่นี่!** ภาพรวมทั้งหมดของระบบ ประกอบด้วย:
- สถาปัตยกรรมโดยรวม
- Technology stack summary
- Service catalog พร้อม dependencies
- Event flow diagrams
- Multi-region architecture
- Implementation checklist

---

## 🚀 Core Infrastructure Services

### [01-API-GATEWAY-SERVICE.md](./01-API-GATEWAY-SERVICE.md)
**Kong Gateway OSS**
- Request routing & load balancing
- Geo-routing for multi-region
- Authentication & rate limiting
- Circuit breaking & resilience

### [02-IDENTITY-AUTH-SERVICE.md](./02-IDENTITY-AUTH-SERVICE.md)
**Keycloak OSS**
- OAuth 2.0 / OpenID Connect (OIDC)
- Multi-factor authentication (MFA)
- SSO across all services
- Multi-region session management

### [12-GLOBAL-CONFIG-SERVICE.md](./12-GLOBAL-CONFIG-SERVICE.md)
**Consul + Vault**
- Service discovery (Consul)
- Configuration management (Consul KV)
- Secrets management (Vault)
- Dynamic credentials

---

## 💼 Business Services

### [03-USER-SERVICE.md](./03-USER-SERVICE.md)
**Technology**: .NET 10
**Database**: PostgreSQL (EF Core Code-First)
- User profile management
- Address management
- User preferences
- Multi-region user data

### [04-PRODUCT-SERVICE.md](./04-PRODUCT-SERVICE.md)
**Technology**: Java Spring Boot 3
**Database**: PostgreSQL (JPA + Flyway)
**Search**: OpenSearch
- Product catalog management
- Category hierarchy
- Product variants
- Multi-currency pricing
- Reviews & ratings

### [05-INVENTORY-SERVICE.md](./05-INVENTORY-SERVICE.md)
**Technology**: Go
**Database**: PostgreSQL (GORM Code-First)
- Real-time stock tracking
- Stock reservation (TTL-based)
- Multi-warehouse management
- Inventory allocation

### [06-CART-SERVICE.md](./06-CART-SERVICE.md)
**Technology**: .NET 10
**Storage**: Redis (primary) + PostgreSQL (backup)
- Shopping cart operations
- Cart persistence & expiration
- Cart calculations
- Cart merge (anonymous → authenticated)

### [07-ORDER-SERVICE.md](./07-ORDER-SERVICE.md)
**Technology**: Java Spring Boot 3
**Database**: PostgreSQL (JPA + Flyway)
**Pattern**: Transactional Outbox
- Order lifecycle management
- Order state machine
- Multi-currency orders
- Order history

### [08-PAYMENT-SERVICE.md](./08-PAYMENT-SERVICE.md)
**Technology**: Go
**Database**: PostgreSQL (GORM)
**Gateways**: Stripe, PayPal
- Payment processing
- Two-phase commit (authorize + capture)
- Refund management
- PCI DSS compliance

---

## 📡 Supporting Services

### [09-NOTIFICATION-SERVICE.md](./09-NOTIFICATION-SERVICE.md)
**Technology**: .NET 10
**Database**: PostgreSQL
- Multi-channel notifications (Email, SMS, Push, In-App)
- Template management
- User preferences
- Delivery tracking

### [10-SEARCH-INDEXER-SERVICE.md](./10-SEARCH-INDEXER-SERVICE.md)
**Technology**: Go or Java
**Search Engine**: OpenSearch
- Real-time product indexing
- Full-text search
- Faceted search
- Auto-suggestions

### [11-FX-RATE-SERVICE.md](./11-FX-RATE-SERVICE.md)
**Technology**: Go
**Database**: PostgreSQL (GORM)
**Cache**: Redis (heavily cached)
- Currency conversion
- Real-time exchange rates
- Historical rates
- Multi-currency support

---

## 🎯 Quick Navigation

### By Technology

**-.NET 10 Services:**
- User Service
- Cart Service
- Notification Service

**Java Spring Boot 3 Services:**
- Product Service
- Order Service

**Go Services:**
- Inventory Service
- Payment Service
- FX Rate Service
- Search Indexer Service (option)

**Infrastructure:**
- Kong Gateway (API Gateway)
- Keycloak (Identity & Auth)
- Consul + Vault (Config & Secrets)

---

## 📖 How to Use This Documentation

### For Project Managers
1. Start with [00-SERVICE-OVERVIEW.md](./00-SERVICE-OVERVIEW.md) for complete system understanding
2. Review service dependencies and event flows
3. Check implementation checklist and timeline

### For Architects
1. Review [00-SERVICE-OVERVIEW.md](./00-SERVICE-OVERVIEW.md) for architecture patterns
2. Study multi-region architecture
3. Review service communication patterns (REST, gRPC, Kafka)
4. Examine database per service strategy

### For Developers
1. **First-time setup**: Read [00-SERVICE-OVERVIEW.md](./00-SERVICE-OVERVIEW.md)
2. **Implementing a service**: Read the specific service documentation
3. Each service doc contains:
   - Technology stack
   - Core responsibilities
   - Clean Architecture structure
   - Domain models (Code-First)
   - API endpoints (REST + gRPC)
   - Database schema
   - Kafka events
   - Implementation tasks
   - Code examples
   - Testing requirements

### For DevOps Engineers
1. Review infrastructure requirements in each service doc
2. Check port allocations in [00-SERVICE-OVERVIEW.md](./00-SERVICE-OVERVIEW.md)
3. Review monitoring & observability sections
4. Study multi-region deployment strategy

---

## 🏗️ Implementation Order

แนะนำให้ implement ตามลำดับนี้:

### Phase 1: Foundation (Week 1-2)
1. Global Config Service (Consul + Vault)
2. Identity & Auth Service (Keycloak)
3. API Gateway Service (Kong)

### Phase 2: Core Services (Week 3-4)
4. User Service
5. FX Rate Service
6. Product Service

### Phase 3: E-Commerce Core (Week 5-6)
7. Inventory Service
8. Cart Service
9. Search Indexer Service

### Phase 4: Transaction Services (Week 7-8)
10. Order Service
11. Payment Service
12. Notification Service

---

## 🔑 Key Principles

### 1. Code-First Database
- **ห้าม** เขียน SQL script โดยตรง
- ต้องสร้าง schema จาก code เสมอ
- .NET → EF Core Migrations
- Java → JPA/Hibernate + Flyway
- Go → GORM/Ent + Atlas/go-migrate

### 2. Clean Architecture
```
Domain Layer (Core)
  ↓
Application Layer (Use Cases)
  ↓
Infrastructure Layer (Database, External APIs)
  ↓
API Layer (Controllers, gRPC)
```

### 3. Event-Driven
- ใช้ Kafka สำหรับ async communication
- Publish events หลัง transaction commit
- Idempotent event handlers
- Outbox pattern สำหรับ reliable delivery

### 4. Multi-Region
- Active-active deployment
- Data replication across regions
- Geo-routing for low latency
- Regional failover

---

## 📊 Service Comparison

| Service | Language | Database | Primary Use |
|---------|----------|----------|-------------|
| User | .NET 10 | PostgreSQL | User management |
| Product | Java | PostgreSQL + OpenSearch | Catalog |
| Inventory | Go | PostgreSQL | Stock mgmt |
| Cart | .NET 10 | Redis + PostgreSQL | Shopping cart |
| Order | Java | PostgreSQL | Order mgmt |
| Payment | Go | PostgreSQL | Payments |
| Notification | .NET 10 | PostgreSQL | Notifications |
| Search | Go/Java | OpenSearch | Search |
| FX Rate | Go | PostgreSQL + Redis | Currency |

---

## 🧪 Testing Strategy

แต่ละ service ต้องมี:

### Unit Tests
- Domain entity validation
- Use case business logic
- Value object behavior

### Integration Tests
- API endpoint testing
- Database operations
- Event publishing/consuming
- External service integration

### Performance Tests
- Concurrent request handling
- Database query performance
- Cache hit/miss ratios
- API response times

---

## 📈 Monitoring

แต่ละ service ต้องมี:

### Health Checks
- `/health` - Overall health
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

### Metrics
- Request rate
- Response time (p50, p95, p99)
- Error rate
- Resource usage
- Business metrics

### Logging
- Structured logging
- Correlation IDs
- Log levels
- Centralized logging (Loki)

### Tracing
- Distributed tracing (Jaeger)
- Span tracking
- Service dependency mapping

---

## 🔐 Security Considerations

### Authentication
- JWT tokens from Keycloak
- Service-to-service mTLS
- API key for external APIs

### Authorization
- Role-based access control (RBAC)
- Resource-level permissions
- Scope validation

### Data Protection
- All secrets in Vault
- Encryption at rest (database)
- Encryption in transit (TLS 1.3)
- PCI DSS compliance (Payment Service)

### API Security
- Rate limiting
- Input validation
- SQL injection prevention (ORMs)
- XSS prevention
- CORS configuration

---

## 🚧 Common Patterns

### Repository Pattern
```
Domain Layer: IRepository interface
Infrastructure Layer: Repository implementation
```

### CQRS (Command Query Responsibility Segregation)
```
Commands: CreateUserCommand, UpdateUserCommand
Queries: GetUserQuery, ListUsersQuery
```

### Saga Pattern
```
Order Service orchestrates:
1. Create Order
2. Reserve Inventory
3. Process Payment
4. Send Notification
```

### Outbox Pattern
```
1. Save domain entity + outbox event (same transaction)
2. Background job publishes events to Kafka
3. Mark event as processed
```

---

## 🛠️ Development Tools

### Required
- Docker & Docker Compose
- Git
- IDE (VS Code, IntelliJ IDEA, Visual Studio)
- Postman or similar API client

### By Language
- **.NET**: .NET 10 SDK, Visual Studio / Rider
- **Java**: JDK 17+, Maven/Gradle, IntelliJ IDEA
- **Go**: Go 1.21+, VS Code with Go extension

### Infrastructure
- kubectl (Kubernetes CLI)
- helm (Kubernetes package manager)
- psql (PostgreSQL client)
- redis-cli (Redis client)

---

## 📞 Support & Contribution

### Getting Help
1. อ่านเอกสาร service ที่เกี่ยวข้อง
2. ตรวจสอบ implementation tasks checklist
3. ดู code examples ในเอกสาร
4. ถาม team architect/lead

### Contributing to Docs
- อัพเดทเอกสารเมื่อมีการเปลี่ยนแปลง architecture
- เพิ่ม code examples ที่เป็นประโยชน์
- ระบุ version และ date ในการแก้ไข
- Review โดย architect ก่อน merge

---

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-01-05 | Initial documentation |

---

## 🎓 Learning Resources

### Clean Architecture
- [The Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Clean Architecture in .NET](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)

### Domain-Driven Design
- [DDD by Martin Fowler](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Domain-Driven Design Reference](https://www.domainlanguage.com/ddd/reference/)

### Microservices
- [Microservices Patterns](https://microservices.io/patterns/index.html)
- [Building Microservices by Sam Newman](https://www.oreilly.com/library/view/building-microservices-2nd/9781492034018/)

### Event-Driven Architecture
- [Event-Driven Architecture](https://martinfowler.com/articles/201701-event-driven.html)
- [Kafka: The Definitive Guide](https://www.confluent.io/resources/kafka-the-definitive-guide/)

---

**Ready to implement? Start with [00-SERVICE-OVERVIEW.md](./00-SERVICE-OVERVIEW.md)!** 🚀
