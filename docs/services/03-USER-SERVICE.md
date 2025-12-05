# User Service (.NET 10)

## Overview
User profile and account management service built with .NET 10. Handles user profiles, preferences, addresses, and user-related business logic. Implements Clean Architecture with DDD principles.

## Technology Stack
- **Language**: .NET 10 (C#)
- **Framework**: ASP.NET Core Web API (Controller-based)
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core 10 (Code-First)
- **Cache**: Redis
- **Messaging**: Kafka (Consumer & Producer)
- **API**: REST + gRPC
- **Testing**: xUnit, Moq, FluentAssertions

## Core Responsibilities

### 1. **User Profile Management**
- Create and update user profiles
- Manage user personal information
- Handle profile pictures and avatars
- User preferences (language, timezone, currency)
- Privacy settings
- Account status management (active, suspended, deleted)

### 2. **Address Management**
- Shipping addresses (CRUD)
- Billing addresses (CRUD)
- Default address selection
- Address validation
- Multi-country address formats

### 3. **User Preferences**
- Communication preferences
- Notification settings
- Display preferences (theme, language)
- Currency and timezone preferences
- Marketing consent management

### 4. **User Statistics & Analytics**
- Order history summary
- Wishlist management
- Recently viewed products
- User activity tracking
- Loyalty points (if applicable)

### 5. **Integration with Auth Service**
- Sync user data with Keycloak
- Handle user lifecycle events
- User role and permission caching
- Token validation and user context

### 6. **Event Publishing**
- Publish user created events
- Publish user updated events
- Publish user deleted events
- Publish preference changed events

### 7. **Multi-Region Support**
- User data replication across regions
- Handle regional compliance (GDPR, CCPA)
- Data sovereignty requirements
- Geo-specific user preferences

## Clean Architecture Structure

```
UserService/
├── src/
│   ├── Domain/                          # Enterprise Business Rules
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Address.cs
│   │   │   ├── UserPreference.cs
│   │   │   └── UserStatistics.cs
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs
│   │   │   ├── PhoneNumber.cs
│   │   │   ├── Money.cs
│   │   │   └── Address.cs
│   │   ├── Enums/
│   │   │   ├── UserStatus.cs
│   │   │   ├── AddressType.cs
│   │   │   └── PreferenceType.cs
│   │   ├── Events/
│   │   │   ├── UserCreatedEvent.cs
│   │   │   ├── UserUpdatedEvent.cs
│   │   │   └── AddressAddedEvent.cs
│   │   └── Repositories/               # Interface only
│   │       └── IUserRepository.cs
│   │
│   ├── Application/                     # Application Business Rules
│   │   ├── DTOs/
│   │   │   ├── Requests/
│   │   │   │   ├── CreateUserRequest.cs
│   │   │   │   ├── UpdateUserRequest.cs
│   │   │   │   └── AddAddressRequest.cs
│   │   │   └── Responses/
│   │   │       ├── UserResponse.cs
│   │   │       ├── AddressResponse.cs
│   │   │       └── UserPreferenceResponse.cs
│   │   ├── UseCases/
│   │   │   ├── CreateUserUseCase.cs
│   │   │   ├── GetUserUseCase.cs
│   │   │   ├── UpdateUserUseCase.cs
│   │   │   ├── DeleteUserUseCase.cs
│   │   │   ├── AddAddressUseCase.cs
│   │   │   ├── UpdatePreferenceUseCase.cs
│   │   │   └── GetUserStatisticsUseCase.cs
│   │   ├── Interfaces/
│   │   │   ├── IUserService.cs
│   │   │   ├── IAddressService.cs
│   │   │   └── IPreferenceService.cs
│   │   ├── Validators/
│   │   │   ├── CreateUserValidator.cs
│   │   │   ├── UpdateUserValidator.cs
│   │   │   └── AddressValidator.cs
│   │   └── Mappings/
│   │       └── AutoMapperProfile.cs
│   │
│   ├── Infrastructure/                   # Frameworks & Drivers
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   ├── AddressConfiguration.cs
│   │   │   │   └── UserPreferenceConfiguration.cs
│   │   │   └── Repositories/
│   │   │       └── UserRepository.cs
│   │   ├── Cache/
│   │   │   ├── RedisCache.cs
│   │   │   └── CacheKeys.cs
│   │   ├── Messaging/
│   │   │   ├── KafkaProducer.cs
│   │   │   ├── KafkaConsumer.cs
│   │   │   └── EventHandlers/
│   │   │       └── UserEventHandler.cs
│   │   ├── gRPC/
│   │   │   ├── Protos/
│   │   │   │   └── user.proto
│   │   │   └── Services/
│   │   │       └── UserGrpcService.cs
│   │   └── ExternalServices/
│   │       └── KeycloakClient.cs
│   │
│   └── Api/                              # Interface Adapters
│       ├── Controllers/
│       │   ├── UsersController.cs
│       │   ├── AddressesController.cs
│       │   ├── PreferencesController.cs
│       │   └── HealthController.cs
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   ├── RequestLoggingMiddleware.cs
│       │   └── AuthenticationMiddleware.cs
│       ├── Filters/
│       │   └── ValidationFilter.cs
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   ├── Domain.Tests/
│   ├── Application.Tests/
│   ├── Infrastructure.Tests/
│   └── Api.Tests/
│
├── Migrations/                          # EF Core Migrations
│   ├── 20250101000001_InitialCreate.cs
│   └── 20250101000002_AddUserPreferences.cs
│
├── UserService.csproj
├── UserService.sln
└── Dockerfile
```

## Domain Models (Code-First Entities)

### User Entity
```csharp
public class User : BaseEntity
{
    public Guid UserId { get; set; }
    public string KeycloakId { get; set; }  // Link to Keycloak
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string ProfilePictureUrl { get; set; }
    public UserStatus Status { get; set; }

    // Navigation properties
    public ICollection<Address> Addresses { get; set; }
    public UserPreference Preference { get; set; }
    public UserStatistics Statistics { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

### Address Entity
```csharp
public class Address : BaseEntity
{
    public Guid AddressId { get; set; }
    public Guid UserId { get; set; }
    public AddressType Type { get; set; }  // Shipping, Billing
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public bool IsDefault { get; set; }

    // Navigation
    public User User { get; set; }
}
```

### UserPreference Entity
```csharp
public class UserPreference : BaseEntity
{
    public Guid PreferenceId { get; set; }
    public Guid UserId { get; set; }
    public string Language { get; set; }
    public string Timezone { get; set; }
    public string Currency { get; set; }
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool MarketingConsent { get; set; }

    // Navigation
    public User User { get; set; }
}
```

### UserStatistics Entity
```csharp
public class UserStatistics : BaseEntity
{
    public Guid StatisticsId { get; set; }
    public Guid UserId { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public int WishlistCount { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public DateTime? LastLoginDate { get; set; }

    // Navigation
    public User User { get; set; }
}
```

## API Endpoints

### REST API

#### Users
```
GET    /api/v1/users                    # List users (admin)
GET    /api/v1/users/{id}               # Get user by ID
GET    /api/v1/users/me                 # Get current user
POST   /api/v1/users                    # Create user
PUT    /api/v1/users/{id}               # Update user
PATCH  /api/v1/users/{id}               # Partial update
DELETE /api/v1/users/{id}               # Soft delete user
GET    /api/v1/users/{id}/statistics    # Get user statistics
```

#### Addresses
```
GET    /api/v1/users/{userId}/addresses            # List addresses
GET    /api/v1/users/{userId}/addresses/{id}       # Get address
POST   /api/v1/users/{userId}/addresses            # Add address
PUT    /api/v1/users/{userId}/addresses/{id}       # Update address
DELETE /api/v1/users/{userId}/addresses/{id}       # Delete address
POST   /api/v1/users/{userId}/addresses/{id}/default  # Set as default
```

#### Preferences
```
GET    /api/v1/users/{userId}/preferences          # Get preferences
PUT    /api/v1/users/{userId}/preferences          # Update preferences
PATCH  /api/v1/users/{userId}/preferences          # Partial update
```

#### Health & Metrics
```
GET    /health                          # Health check
GET    /health/ready                    # Readiness probe
GET    /health/live                     # Liveness probe
GET    /metrics                         # Prometheus metrics
```

### gRPC API

```protobuf
service UserService {
  rpc GetUser(GetUserRequest) returns (UserResponse);
  rpc CreateUser(CreateUserRequest) returns (UserResponse);
  rpc UpdateUser(UpdateUserRequest) returns (UserResponse);
  rpc DeleteUser(DeleteUserRequest) returns (Empty);
  rpc GetUserAddresses(GetUserAddressesRequest) returns (AddressListResponse);
  rpc ValidateUser(ValidateUserRequest) returns (ValidationResponse);
}
```

## Database Schema (Code-First)

### EF Core DbContext
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }
    public DbSet<UserStatistics> UserStatistics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new UserPreferenceConfiguration());
        modelBuilder.ApplyConfiguration(new UserStatisticsConfiguration());
    }
}
```

### Entity Configuration Example
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(255);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.KeycloakId).IsUnique();

        builder.HasMany(u => u.Addresses)
               .WithOne(a => a.User)
               .HasForeignKey(a => a.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Preference)
               .WithOne(p => p.User)
               .HasForeignKey<UserPreference>(p => p.UserId);
    }
}
```

## Kafka Events

### Produced Events
```csharp
// user.created
{
  "eventId": "uuid",
  "eventType": "user.created",
  "timestamp": "2025-01-01T00:00:00Z",
  "userId": "uuid",
  "email": "user@example.com",
  "region": "us-west"
}

// user.updated
{
  "eventId": "uuid",
  "eventType": "user.updated",
  "timestamp": "2025-01-01T00:00:00Z",
  "userId": "uuid",
  "changes": ["firstName", "lastName"]
}

// address.added
{
  "eventId": "uuid",
  "eventType": "address.added",
  "userId": "uuid",
  "addressId": "uuid",
  "addressType": "shipping"
}
```

### Consumed Events
```csharp
// order.completed (from Order Service)
// → Update user statistics

// payment.successful (from Payment Service)
// → Update total spent
```

## Environment Variables
```
# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Database
DATABASE_HOST=postgres
DATABASE_PORT=5432
DATABASE_NAME=user_service
DATABASE_USER=user_service
DATABASE_PASSWORD=***
DATABASE_POOL_SIZE=100

# Redis
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_PASSWORD=***
REDIS_DATABASE=0
REDIS_TTL_SECONDS=3600

# Kafka
KAFKA_BOOTSTRAP_SERVERS=kafka:9092
KAFKA_GROUP_ID=user-service
KAFKA_TOPIC_USER_EVENTS=user.events
KAFKA_TOPIC_ORDER_EVENTS=order.events

# Keycloak
KEYCLOAK_URL=http://keycloak:8080
KEYCLOAK_REALM=ecommerce
KEYCLOAK_CLIENT_ID=user-service
KEYCLOAK_CLIENT_SECRET=***

# Observability
JAEGER_AGENT_HOST=jaeger
JAEGER_AGENT_PORT=6831
PROMETHEUS_PORT=9090
```

## Implementation Tasks

### Phase 1: Project Setup & Domain Layer
- [ ] Create .NET 10 Web API project
- [ ] Set up solution structure (Clean Architecture)
- [ ] Define domain entities (User, Address, UserPreference)
- [ ] Implement value objects (Email, PhoneNumber)
- [ ] Create domain events
- [ ] Define repository interfaces

### Phase 2: Application Layer
- [ ] Implement DTOs (Request/Response)
- [ ] Create use cases (CQRS pattern)
- [ ] Add FluentValidation validators
- [ ] Implement AutoMapper profiles
- [ ] Create service interfaces

### Phase 3: Infrastructure Layer
- [ ] Set up EF Core DbContext
- [ ] Configure entity configurations
- [ ] Create initial migration
- [ ] Implement repository pattern
- [ ] Set up Redis caching
- [ ] Configure Kafka producer/consumer

### Phase 4: API Layer
- [ ] Implement REST controllers
- [ ] Add authentication middleware
- [ ] Implement exception handling
- [ ] Add request/response logging
- [ ] Configure Swagger/OpenAPI
- [ ] Implement health checks

### Phase 5: gRPC Implementation
- [ ] Define .proto files
- [ ] Generate gRPC code
- [ ] Implement gRPC services
- [ ] Add gRPC health checks

### Phase 6: Integration
- [ ] Integrate with Keycloak
- [ ] Set up JWT validation
- [ ] Implement user sync logic
- [ ] Configure CORS
- [ ] Add rate limiting

### Phase 7: Observability
- [ ] Add Prometheus metrics
- [ ] Configure Jaeger tracing
- [ ] Set up structured logging (Serilog)
- [ ] Create health check endpoints

### Phase 8: Testing
- [ ] Write unit tests (Domain, Application)
- [ ] Write integration tests (API, Database)
- [ ] Write gRPC client tests
- [ ] Performance testing

## Code Examples

### Controller Example
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return user != null ? Ok(user) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), 201)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
    }
}
```

### Use Case Example
```csharp
public class CreateUserUseCase : ICreateUserUseCase
{
    private readonly IUserRepository _repository;
    private readonly IKafkaProducer _kafkaProducer;

    public async Task<UserResponse> ExecuteAsync(CreateUserRequest request)
    {
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Status = UserStatus.Active
        };

        await _repository.AddAsync(user);
        await _kafkaProducer.PublishAsync("user.created", new UserCreatedEvent(user));

        return MapToResponse(user);
    }
}
```

## Testing Requirements

### Unit Tests
- [ ] Domain entity validation
- [ ] Value object behavior
- [ ] Use case business logic
- [ ] Validator rules

### Integration Tests
- [ ] Database operations (CRUD)
- [ ] API endpoint testing
- [ ] Kafka event publishing
- [ ] Redis caching

### Performance Tests
- [ ] Concurrent user operations
- [ ] Database query performance
- [ ] Cache hit/miss ratios
- [ ] API response times

## Documentation Deliverables
- [ ] API documentation (Swagger)
- [ ] gRPC service documentation
- [ ] Database schema documentation
- [ ] Event schema documentation
- [ ] Deployment guide
- [ ] Troubleshooting guide

## Dependencies
- Keycloak (Authentication)
- PostgreSQL (Database)
- Redis (Cache)
- Kafka (Messaging)
- API Gateway (Routing)

## Success Criteria
- 99.9% uptime
- API response time < 100ms (p95)
- Support 10K+ concurrent users
- Cache hit rate > 80%
- Event publishing latency < 10ms
- Zero data loss
