คุณคือ Software Architect ระดับ Enterprise  
ช่วยออกแบบระบบ E-Commerce Platform ขนาดใหญ่ (Global-Scale)  
รองรับผู้ใช้งานทั่วโลกแบบ Multi-Region, Multi-Timezone, Multi-Currency  
ระบบต้อง Highly Available, Low Latency, Fault Tolerant และรองรับ Global Scalability  
โดยใช้สถาปัตยกรรม Microservices + Clean Architecture + DDD + Event-Driven  
และต้องใช้แนวทาง Code-First สำหรับทุก Service ใน Layer Database

เงื่อนไขสำคัญ:

- ทุก Technology ต้องเป็น Open Source / Free (OSS-only)
- Backend services ต้องใช้ .NET 10, Java Spring Boot 3+, และ Go เท่านั้น (ตาม service)
- รองรับ API 2 แบบ: RESTful & gRPC
- รองรับ Global Usage (Multi-Region, Multi-Currency, Multi-Timezone, Geo-routing)
- Database ต้องใช้ **Code-First Approach** ทุก service
  - .NET → Entity Framework Core 10 (Code First + Migrations)
  - Java → JPA/Hibernate (Code First + Liquibase/Flyway OSS)
  - Go → GORM หรือ Ent (Code First + Migration tool เช่น Atlas/Go-Migrate)
- ไม่มีแบบ DB-First, ไม่มี SQL script ที่เขียนเอง ต้อง generate schema ผ่าน code เท่านั้น

=====================================
GLOBAL TECH STACK (OSS Only)
=====================================

Backend:

- .NET 10 (ASP.NET Core Web API, Controller-based) — Code First EF Core
- Java 17+ (Spring Boot 3+ + Hibernate/JPA, Flyway/Liquibase) — Code First
- Go (Fiber/Echo + GORM/Ent + Atlas/Go-Migrate) — Code First

Databases & Data:

- PostgreSQL (DB-per-service) + Multi-region read replicas
- Redis OSS
- OpenSearch/ElasticSearch
- Kafka OSS
- Consul / Vault
- Prometheus / Grafana / Loki / Jaeger

=====================================
SERVICE ARCHITECTURE (WITH TECH STACK PER SERVICE + CODE-FIRST)
=====================================

ให้คุณออกแบบ Microservices พร้อม Tech Stack และ Code-First DB tool ดังนี้:

1) API Gateway Service
   - Kong Gateway OSS
   - Keycloak integration
   - Geo-routing

2) Identity & Auth Service
   - Keycloak OSS
   - PostgreSQL Code-First schema from Keycloak
   - Multi-region sync

3) User Service
   - Language: .NET 10
   - DB: PostgreSQL (Code-First EF Core)
   - Cache: Redis
   - REST + gRPC

4) Product / Catalog Service
   - Language: Java Spring Boot
   - DB: PostgreSQL (Code-First JPA + Flyway/Liquibase)
   - Search Index: OpenSearch
   - Kafka producer

5) Inventory Service
   - Language: Go (Fiber/Echo)
   - DB: PostgreSQL (Code-First using GORM or Ent)
   - Migrations: Atlas / Go-Migrate
   - Kafka consumer + gRPC API

6) Cart Service
   - Language: .NET 10
   - Cache: Redis
   - DB: PostgreSQL (Code-First EF Core)

7) Order Service
   - Language: Java Spring Boot
   - DB: PostgreSQL (Code-First JPA/Hibernate)
   - Flyway migration auto-generated
   - Kafka Outbox pattern

8) Payment Service
   - Language: Go
   - DB: PostgreSQL (Code-First via GORM/Ent)
   - Migrations: Atlas
   - Kafka consumer/producer

9) Notification Service
   - Language: .NET 10
   - Code-First EF Core
   - Kafka consumer

10) Search Indexer Service
    - Go หรือ Java
    - NoSQL schema (OpenSearch)

11) FX Rate Service
    - Language: Go
    - Code-First schema for FX cache table (PostgreSQL)
    - Redis caching

12) Global Config Service
    - Consul + Vault (No DB)

=====================================
WHAT YOU MUST DESIGN
=====================================

1. Global Architecture (multi-region, active-active)
2. Domain & Bounded Contexts
3. Clean Architecture per Service
4. REST + gRPC API Design
5. Kafka Event Topology (multi-region)
6. Data Architecture (PostgreSQL code-first)
7. Performance & scalability
8. Security & Keycloak federation
9. DevOps (Jenkins + ArgoCD)

=====================================
10. เขียนโค้ดจริงแบบ Production-ready (ห้าม skeleton)
=====================================

❗ **ทุกโค้ดต้อง Build/Compile ได้จริง และสอดคล้องกับ Code-First**

---------------------------------------

10.1 .NET 10 (ASP.NET Core Web API)
---------------------------------------

ต้องเขียนโค้ดจริงดังนี้:

- Program.cs + dependency injection + EF Core PostgreSQL config
- appsettings.json (ตัวอย่าง)
- Entity + DbContext (Code-First)
- EF Core Migration class (จริง)
- Repository implementation (async)
- Use Case service
- Controller (REST) พร้อม attribute routing
- gRPC client (ถ้ามี)

---------------------------------------

10.2 Java Spring Boot 3 (REST + Kafka)
---------------------------------------

ต้องเขียนโค้ดจริงดังนี้:

- pom.xml หรือ Gradle config
- Application class
- Entity + Repository (JPA)
- Code-First schema migration via Flyway/Liquibase
- Service layer
- REST Controller
- Kafka Producer config + Producer code

---------------------------------------

10.3 Go (gRPC handler + Kafka consumer)
---------------------------------------

ต้องเขียนโค้ดจริงดังนี้:

- main.go (start gRPC server)
- .proto file + generated code
- GORM/Ent entity definitions (Code-First)
- Migration file via Atlas / go-migrate
- Repository implementation
- Use case logic
- Kafka consumer implementation

---------------------------------------

10.4 โครงสร้างโฟลเดอร์จริง (Clean Architecture)
---------------------------------------

ต้องแสดงโฟลเดอร์จริง เช่น:

.NET
/src/Domain/Entities  
/src/Application/UseCases  
/src/Infrastructure/EFCore  
/src/Api/Controllers  

Java
/src/main/java/.../domain  
/src/main/java/.../application  
/src/main/java/.../infrastructure  
/src/main/java/.../api  

Go
/internal/domain  
/internal/usecase  
/internal/adapter  
/internal/infrastructure  

---------------------------------------

โปรดตอบอย่างละเอียด step-by-step  
พร้อม diagram ASCII, ตารางสรุป และ global design rationale  
และตรวจสอบอีกครั้งว่า **ทุกเทคโนโลยีเป็น OSS / Free เท่านั้น**  
และทุกส่วนที่ต้องเขียนโค้ด ให้เขียนโค้ดจริงที่รันได้ (not pseudo-code).
