# Product Service (Java Spring Boot 3)

## Overview
Product catalog and management service built with Java Spring Boot 3. Handles product information, categories, pricing, variants, and search indexing. Implements Clean Architecture with DDD principles.

## Technology Stack
- **Language**: Java 17+
- **Framework**: Spring Boot 3.x
- **Database**: PostgreSQL
- **ORM**: JPA/Hibernate (Code-First)
- **Migration**: Flyway OSS / Liquibase
- **Search**: OpenSearch/Elasticsearch
- **Cache**: Redis
- **Messaging**: Kafka (Producer)
- **API**: REST + gRPC
- **Testing**: JUnit 5, Mockito, TestContainers

## Core Responsibilities

### 1. **Product Catalog Management**
- Create, read, update, delete products
- Product variants (size, color, etc.)
- Product images and media
- Product descriptions (multi-language)
- SKU management
- Product status (draft, active, inactive, discontinued)

### 2. **Category Management**
- Hierarchical category structure
- Category attributes and filters
- Category-product associations
- Category SEO metadata

### 3. **Pricing Management**
- Base pricing
- Multi-currency support
- Regional pricing
- Discount and promotion rules
- Price history tracking
- Bulk pricing

### 4. **Inventory Coordination**
- Check product availability (via Inventory Service)
- Stock level display
- Pre-order and backorder support
- Product availability by region

### 5. **Search & Filtering**
- Full-text search
- Faceted search (filters)
- Auto-suggestions
- Search analytics
- Ranking and relevance tuning

### 6. **Product Reviews & Ratings**
- Customer reviews
- Rating aggregation
- Review moderation
- Verified purchase badges

### 7. **Event Publishing**
- Product created/updated/deleted events
- Price changed events
- Product availability events
- Index update events

### 8. **Multi-Region Support**
- Regional product catalogs
- Region-specific pricing
- Multi-language product information
- Geo-specific availability

## Clean Architecture Structure

```
product-service/
├── src/
│   ├── main/
│   │   ├── java/
│   │   │   └── com/ecommerce/product/
│   │   │       ├── domain/                    # Domain Layer
│   │   │       │   ├── model/
│   │   │       │   │   ├── Product.java
│   │   │       │   │   ├── ProductVariant.java
│   │   │       │   │   ├── Category.java
│   │   │       │   │   ├── Price.java
│   │   │       │   │   └── Review.java
│   │   │       │   ├── valueobject/
│   │   │       │   │   ├── Money.java
│   │   │       │   │   ├── SKU.java
│   │   │       │   │   └── Rating.java
│   │   │       │   ├── enums/
│   │   │       │   │   ├── ProductStatus.java
│   │   │       │   │   ├── VariantType.java
│   │   │       │   │   └── Currency.java
│   │   │       │   ├── event/
│   │   │       │   │   ├── ProductCreatedEvent.java
│   │   │       │   │   ├── ProductUpdatedEvent.java
│   │   │       │   │   └── PriceChangedEvent.java
│   │   │       │   └── repository/           # Interfaces
│   │   │       │       ├── ProductRepository.java
│   │   │       │       └── CategoryRepository.java
│   │   │       │
│   │   │       ├── application/               # Application Layer
│   │   │       │   ├── dto/
│   │   │       │   │   ├── request/
│   │   │       │   │   │   ├── CreateProductRequest.java
│   │   │       │   │   │   ├── UpdateProductRequest.java
│   │   │       │   │   │   └── ProductSearchRequest.java
│   │   │       │   │   └── response/
│   │   │       │   │       ├── ProductResponse.java
│   │   │       │   │       ├── CategoryResponse.java
│   │   │       │   │       └── ProductDetailResponse.java
│   │   │       │   ├── usecase/
│   │   │       │   │   ├── CreateProductUseCase.java
│   │   │       │   │   ├── UpdateProductUseCase.java
│   │   │       │   │   ├── SearchProductUseCase.java
│   │   │       │   │   └── GetProductDetailsUseCase.java
│   │   │       │   ├── service/
│   │   │       │   │   ├── ProductService.java
│   │   │       │   │   ├── CategoryService.java
│   │   │       │   │   ├── PricingService.java
│   │   │       │   │   └── ReviewService.java
│   │   │       │   └── mapper/
│   │   │       │       └── ProductMapper.java
│   │   │       │
│   │   │       ├── infrastructure/            # Infrastructure Layer
│   │   │       │   ├── persistence/
│   │   │       │   │   ├── entity/
│   │   │       │   │   │   ├── ProductEntity.java
│   │   │       │   │   │   ├── CategoryEntity.java
│   │   │       │   │   │   └── PriceEntity.java
│   │   │       │   │   ├── jpa/
│   │   │       │   │   │   ├── ProductJpaRepository.java
│   │   │       │   │   │   └── CategoryJpaRepository.java
│   │   │       │   │   └── repository/
│   │   │       │   │       └── ProductRepositoryImpl.java
│   │   │       │   ├── cache/
│   │   │       │   │   └── RedisCacheService.java
│   │   │       │   ├── search/
│   │   │       │   │   └── OpenSearchService.java
│   │   │       │   ├── messaging/
│   │   │       │   │   ├── KafkaProducer.java
│   │   │       │   │   └── ProductEventPublisher.java
│   │   │       │   └── grpc/
│   │   │       │       ├── proto/
│   │   │       │       │   └── product.proto
│   │   │       │       └── ProductGrpcService.java
│   │   │       │
│   │   │       └── api/                       # API Layer
│   │   │           ├── controller/
│   │   │           │   ├── ProductController.java
│   │   │           │   ├── CategoryController.java
│   │   │           │   ├── SearchController.java
│   │   │           │   └── HealthController.java
│   │   │           ├── exception/
│   │   │           │   ├── GlobalExceptionHandler.java
│   │   │           │   └── ProductNotFoundException.java
│   │   │           ├── config/
│   │   │           │   ├── SecurityConfig.java
│   │   │           │   ├── JpaConfig.java
│   │   │           │   ├── KafkaConfig.java
│   │   │           │   └── RedisConfig.java
│   │   │           └── ProductServiceApplication.java
│   │   │
│   │   └── resources/
│   │       ├── application.yml
│   │       ├── application-prod.yml
│   │       └── db/migration/                  # Flyway migrations
│   │           ├── V1__initial_schema.sql
│   │           └── V2__add_variants.sql
│   │
│   └── test/
│       └── java/
│           └── com/ecommerce/product/
│               ├── domain/
│               ├── application/
│               ├── infrastructure/
│               └── api/
│
├── pom.xml
├── Dockerfile
└── README.md
```

## Domain Models (JPA Entities - Code-First)

### Product Entity
```java
@Entity
@Table(name = "products")
public class Product extends BaseEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID productId;

    @Column(nullable = false, length = 255)
    private String name;

    @Column(columnDefinition = "TEXT")
    private String description;

    @Column(unique = true, nullable = false, length = 50)
    private String sku;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false)
    private ProductStatus status;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "category_id")
    private Category category;

    @OneToMany(mappedBy = "product", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<ProductVariant> variants = new ArrayList<>();

    @OneToMany(mappedBy = "product", cascade = CascadeType.ALL)
    private List<Price> prices = new ArrayList<>();

    @OneToMany(mappedBy = "product", cascade = CascadeType.ALL)
    private List<Review> reviews = new ArrayList<>();

    @ElementCollection
    @CollectionTable(name = "product_images", joinColumns = @JoinColumn(name = "product_id"))
    @Column(name = "image_url")
    private List<String> imageUrls = new ArrayList<>();

    @Column(name = "average_rating")
    private Double averageRating;

    @Column(name = "review_count")
    private Integer reviewCount;

    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;

    @Column(name = "updated_at")
    private LocalDateTime updatedAt;

    @Column(name = "deleted_at")
    private LocalDateTime deletedAt;
}
```

### Category Entity
```java
@Entity
@Table(name = "categories")
public class Category extends BaseEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID categoryId;

    @Column(nullable = false, length = 255)
    private String name;

    @Column(length = 255)
    private String slug;

    @Column(columnDefinition = "TEXT")
    private String description;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "parent_id")
    private Category parent;

    @OneToMany(mappedBy = "parent")
    private List<Category> children = new ArrayList<>();

    @OneToMany(mappedBy = "category")
    private List<Product> products = new ArrayList<>();

    @Column(name = "display_order")
    private Integer displayOrder;

    @Column(name = "is_active")
    private Boolean isActive;
}
```

### ProductVariant Entity
```java
@Entity
@Table(name = "product_variants")
public class ProductVariant extends BaseEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID variantId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "product_id", nullable = false)
    private Product product;

    @Column(unique = true, nullable = false)
    private String sku;

    @Column(nullable = false)
    private String name;

    @ElementCollection
    @CollectionTable(name = "variant_attributes", joinColumns = @JoinColumn(name = "variant_id"))
    @MapKeyColumn(name = "attribute_name")
    @Column(name = "attribute_value")
    private Map<String, String> attributes = new HashMap<>();

    @Column(name = "additional_price")
    private BigDecimal additionalPrice;

    @Column(name = "is_available")
    private Boolean isAvailable;
}
```

### Price Entity
```java
@Entity
@Table(name = "prices")
public class Price extends BaseEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID priceId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "product_id", nullable = false)
    private Product product;

    @Column(nullable = false, precision = 19, scale = 4)
    private BigDecimal amount;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false, length = 3)
    private Currency currency;

    @Column(length = 50)
    private String region;

    @Column(name = "effective_from")
    private LocalDateTime effectiveFrom;

    @Column(name = "effective_to")
    private LocalDateTime effectiveTo;
}
```

### Review Entity
```java
@Entity
@Table(name = "reviews")
public class Review extends BaseEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID reviewId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "product_id", nullable = false)
    private Product product;

    @Column(name = "user_id", nullable = false)
    private UUID userId;

    @Column(nullable = false)
    private Integer rating; // 1-5

    @Column(columnDefinition = "TEXT")
    private String comment;

    @Column(name = "is_verified_purchase")
    private Boolean isVerifiedPurchase;

    @Column(name = "is_approved")
    private Boolean isApproved;

    @Column(name = "created_at", nullable = false)
    private LocalDateTime createdAt;
}
```

## API Endpoints

### REST API

#### Products
```
GET    /api/v1/products                 # List products (paginated)
GET    /api/v1/products/{id}            # Get product details
POST   /api/v1/products                 # Create product (admin)
PUT    /api/v1/products/{id}            # Update product (admin)
PATCH  /api/v1/products/{id}            # Partial update (admin)
DELETE /api/v1/products/{id}            # Delete product (admin)
GET    /api/v1/products/{id}/variants   # Get product variants
POST   /api/v1/products/{id}/variants   # Add variant (admin)
```

#### Categories
```
GET    /api/v1/categories               # List all categories
GET    /api/v1/categories/{id}          # Get category details
GET    /api/v1/categories/{id}/products # Get products in category
POST   /api/v1/categories               # Create category (admin)
PUT    /api/v1/categories/{id}          # Update category (admin)
DELETE /api/v1/categories/{id}          # Delete category (admin)
```

#### Search
```
GET    /api/v1/search                   # Search products
POST   /api/v1/search/advanced          # Advanced search with filters
GET    /api/v1/search/suggestions       # Auto-suggestions
```

#### Reviews
```
GET    /api/v1/products/{id}/reviews    # Get product reviews
POST   /api/v1/products/{id}/reviews    # Create review (authenticated)
PUT    /api/v1/reviews/{id}             # Update review (owner only)
DELETE /api/v1/reviews/{id}             # Delete review (owner/admin)
```

#### Health & Metrics
```
GET    /actuator/health                 # Health check
GET    /actuator/metrics                # Metrics endpoint
GET    /actuator/info                   # Application info
```

### gRPC API

```protobuf
service ProductService {
  rpc GetProduct(GetProductRequest) returns (ProductResponse);
  rpc GetProducts(GetProductsRequest) returns (ProductListResponse);
  rpc CreateProduct(CreateProductRequest) returns (ProductResponse);
  rpc UpdateProduct(UpdateProductRequest) returns (ProductResponse);
  rpc DeleteProduct(DeleteProductRequest) returns (Empty);
  rpc SearchProducts(SearchRequest) returns (ProductListResponse);
  rpc CheckAvailability(CheckAvailabilityRequest) returns (AvailabilityResponse);
}
```

## Database Schema (Flyway Migration)

### Initial Migration (V1__initial_schema.sql)
```sql
-- Auto-generated from JPA entities
-- This is generated by Flyway based on the entity models

CREATE TABLE categories (
    category_id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(255),
    description TEXT,
    parent_id UUID REFERENCES categories(category_id),
    display_order INTEGER,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP
);

CREATE TABLE products (
    product_id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    sku VARCHAR(50) UNIQUE NOT NULL,
    status VARCHAR(50) NOT NULL,
    category_id UUID REFERENCES categories(category_id),
    average_rating DECIMAL(3,2),
    review_count INTEGER DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    deleted_at TIMESTAMP
);

CREATE TABLE product_variants (
    variant_id UUID PRIMARY KEY,
    product_id UUID NOT NULL REFERENCES products(product_id) ON DELETE CASCADE,
    sku VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    additional_price DECIMAL(19,4),
    is_available BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE prices (
    price_id UUID PRIMARY KEY,
    product_id UUID NOT NULL REFERENCES products(product_id) ON DELETE CASCADE,
    amount DECIMAL(19,4) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    region VARCHAR(50),
    effective_from TIMESTAMP,
    effective_to TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE reviews (
    review_id UUID PRIMARY KEY,
    product_id UUID NOT NULL REFERENCES products(product_id) ON DELETE CASCADE,
    user_id UUID NOT NULL,
    rating INTEGER NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment TEXT,
    is_verified_purchase BOOLEAN DEFAULT FALSE,
    is_approved BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Indexes
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_sku ON products(sku);
CREATE INDEX idx_products_status ON products(status);
CREATE INDEX idx_variants_product ON product_variants(product_id);
CREATE INDEX idx_prices_product ON prices(product_id);
CREATE INDEX idx_reviews_product ON reviews(product_id);
CREATE INDEX idx_reviews_user ON reviews(user_id);
```

## Kafka Events

### Produced Events
```java
// product.created
{
  "eventId": "uuid",
  "eventType": "product.created",
  "timestamp": "2025-01-01T00:00:00Z",
  "productId": "uuid",
  "sku": "PROD-001",
  "name": "Product Name",
  "categoryId": "uuid",
  "status": "ACTIVE"
}

// product.updated
{
  "eventId": "uuid",
  "eventType": "product.updated",
  "timestamp": "2025-01-01T00:00:00Z",
  "productId": "uuid",
  "changes": ["price", "description"]
}

// price.changed
{
  "eventId": "uuid",
  "eventType": "price.changed",
  "productId": "uuid",
  "oldPrice": 100.00,
  "newPrice": 90.00,
  "currency": "USD"
}

// product.indexed
{
  "eventId": "uuid",
  "eventType": "product.indexed",
  "productId": "uuid",
  "action": "CREATE|UPDATE|DELETE"
}
```

## OpenSearch Integration

### Product Index Schema
```json
{
  "mappings": {
    "properties": {
      "productId": {"type": "keyword"},
      "name": {"type": "text", "analyzer": "standard"},
      "description": {"type": "text"},
      "sku": {"type": "keyword"},
      "categoryId": {"type": "keyword"},
      "categoryName": {"type": "text"},
      "price": {"type": "double"},
      "currency": {"type": "keyword"},
      "rating": {"type": "float"},
      "reviewCount": {"type": "integer"},
      "status": {"type": "keyword"},
      "tags": {"type": "keyword"},
      "createdAt": {"type": "date"}
    }
  }
}
```

## Environment Variables (application.yml)
```yaml
spring:
  application:
    name: product-service
  datasource:
    url: jdbc:postgresql://${DB_HOST:localhost}:${DB_PORT:5432}/${DB_NAME:product_service}
    username: ${DB_USER:product_service}
    password: ${DB_PASSWORD:}
    driver-class-name: org.postgresql.Driver
    hikari:
      maximum-pool-size: 100
      minimum-idle: 10
  jpa:
    hibernate:
      ddl-auto: validate
    properties:
      hibernate:
        dialect: org.hibernate.dialect.PostgreSQLDialect
        format_sql: true
    show-sql: false
  flyway:
    enabled: true
    locations: classpath:db/migration
  data:
    redis:
      host: ${REDIS_HOST:localhost}
      port: ${REDIS_PORT:6379}
      password: ${REDIS_PASSWORD:}

kafka:
  bootstrap-servers: ${KAFKA_BOOTSTRAP_SERVERS:localhost:9092}
  producer:
    key-serializer: org.apache.kafka.common.serialization.StringSerializer
    value-serializer: org.springframework.kafka.support.serializer.JsonSerializer
  topics:
    product-events: product.events

opensearch:
  host: ${OPENSEARCH_HOST:localhost}
  port: ${OPENSEARCH_PORT:9200}
  username: ${OPENSEARCH_USER:admin}
  password: ${OPENSEARCH_PASSWORD:}

grpc:
  server:
    port: 9090
```

## Implementation Tasks

### Phase 1: Project Setup
- [ ] Create Spring Boot 3 project
- [ ] Configure Maven/Gradle dependencies
- [ ] Set up Clean Architecture structure
- [ ] Configure PostgreSQL connection
- [ ] Set up Flyway migrations

### Phase 2: Domain Layer
- [ ] Define JPA entities (Product, Category, etc.)
- [ ] Create value objects (Money, SKU)
- [ ] Define domain events
- [ ] Create repository interfaces

### Phase 3: Application Layer
- [ ] Implement DTOs
- [ ] Create use cases
- [ ] Implement service layer
- [ ] Add Bean Validation
- [ ] Create mapper utilities

### Phase 4: Infrastructure Layer
- [ ] Implement JPA repositories
- [ ] Configure Redis caching
- [ ] Set up OpenSearch client
- [ ] Implement Kafka producer
- [ ] Create gRPC services

### Phase 5: API Layer
- [ ] Implement REST controllers
- [ ] Add global exception handling
- [ ] Configure Spring Security
- [ ] Set up Swagger/OpenAPI
- [ ] Add actuator endpoints

### Phase 6: Search Integration
- [ ] Configure OpenSearch indices
- [ ] Implement indexing service
- [ ] Create search endpoints
- [ ] Add faceted search
- [ ] Implement auto-suggestions

### Phase 7: Testing
- [ ] Write unit tests (JUnit 5)
- [ ] Write integration tests (TestContainers)
- [ ] Test Kafka event publishing
- [ ] Performance testing

## Dependencies (pom.xml key dependencies)
```xml
<dependencies>
    <!-- Spring Boot -->
    <dependency>
        <groupId>org.springframework.boot</groupId>
        <artifactId>spring-boot-starter-web</artifactId>
    </dependency>
    <dependency>
        <groupId>org.springframework.boot</groupId>
        <artifactId>spring-boot-starter-data-jpa</artifactId>
    </dependency>
    <dependency>
        <groupId>org.springframework.boot</groupId>
        <artifactId>spring-boot-starter-data-redis</artifactId>
    </dependency>

    <!-- Database -->
    <dependency>
        <groupId>org.postgresql</groupId>
        <artifactId>postgresql</artifactId>
    </dependency>
    <dependency>
        <groupId>org.flywaydb</groupId>
        <artifactId>flyway-core</artifactId>
    </dependency>

    <!-- Kafka -->
    <dependency>
        <groupId>org.springframework.kafka</groupId>
        <artifactId>spring-kafka</artifactId>
    </dependency>

    <!-- gRPC -->
    <dependency>
        <groupId>net.devh</groupId>
        <artifactId>grpc-server-spring-boot-starter</artifactId>
    </dependency>

    <!-- OpenSearch -->
    <dependency>
        <groupId>org.opensearch.client</groupId>
        <artifactId>spring-data-opensearch</artifactId>
    </dependency>
</dependencies>
```

## Success Criteria
- 99.9% uptime
- API response time < 100ms (p95)
- Search latency < 50ms
- Support 1M+ products in catalog
- Handle 10K+ concurrent requests
- Cache hit rate > 80%
