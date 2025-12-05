# Identity & Auth Service (Keycloak)

## Overview
Centralized identity and authentication service using Keycloak OSS. Manages user authentication, authorization, single sign-on (SSO), and user federation across multiple regions.

## Technology Stack
- **IAM Platform**: Keycloak OSS
- **Database**: PostgreSQL (Code-First via Keycloak JPA)
- **Cache**: Infinispan (embedded in Keycloak)
- **Protocol**: OAuth 2.0, OpenID Connect (OIDC), SAML 2.0
- **Multi-Region**: Cross-datacenter replication

## Core Responsibilities

### 1. **User Authentication**
- Username/password authentication
- Multi-factor authentication (MFA/2FA)
- Social login (Google, Facebook, GitHub)
- Passwordless authentication (WebAuthn, OTP)
- Session management
- Remember me functionality

### 2. **Authorization & Access Control**
- Role-Based Access Control (RBAC)
- Attribute-Based Access Control (ABAC)
- Client-level permissions
- Fine-grained authorization policies
- Group-based permissions
- Scope management

### 3. **Token Management**
- JWT token generation and validation
- Access token and refresh token lifecycle
- Token introspection
- Token revocation
- Token exchange
- Offline tokens for mobile apps

### 4. **User Management**
- User registration and profile management
- Email verification
- Password reset/recovery
- Account lockout policies
- User session tracking
- User federation from external systems

### 5. **Multi-Tenancy & Realms**
- Separate realms for different environments (dev, staging, prod)
- Realm-level configuration isolation
- Cross-realm trust (if needed)
- Realm import/export

### 6. **Single Sign-On (SSO)**
- SSO across all microservices
- Single logout (SLO)
- Session timeout management
- Cross-domain SSO

### 7. **Multi-Region Synchronization**
- User data replication across regions
- Session failover between regions
- Eventual consistency handling
- Conflict resolution

### 8. **Security**
- Brute force detection and protection
- Account lockout after failed attempts
- Security event logging
- Compliance with GDPR, CCPA
- Consent management

### 9. **Client Management**
- OAuth2/OIDC client registration
- Client authentication (client credentials)
- Redirect URI validation
- CORS configuration per client
- Client secret rotation

## Keycloak Configuration

### Realm Structure
```
ecommerce (Main Realm)
├── Clients
│   ├── web-app (SPA)
│   ├── mobile-app
│   ├── api-gateway
│   ├── user-service
│   ├── product-service
│   ├── order-service
│   └── ... (other services)
├── Roles
│   ├── admin
│   ├── customer
│   ├── merchant
│   ├── support
│   └── system
├── Groups
│   ├── Premium Customers
│   ├── VIP Customers
│   └── Staff
├── Identity Providers
│   ├── Google
│   ├── Facebook
│   └── GitHub
└── Authentication Flows
    ├── Browser Flow (with MFA)
    ├── Direct Grant Flow
    ├── Registration Flow
    └── Reset Credentials Flow
```

### Client Configurations

#### Web App Client
```json
{
  "clientId": "web-app",
  "enabled": true,
  "protocol": "openid-connect",
  "publicClient": true,
  "redirectUris": [
    "https://app.example.com/*",
    "http://localhost:3000/*"
  ],
  "webOrigins": ["+"],
  "standardFlowEnabled": true,
  "implicitFlowEnabled": false,
  "directAccessGrantsEnabled": false
}
```

#### API Gateway Client
```json
{
  "clientId": "api-gateway",
  "enabled": true,
  "protocol": "openid-connect",
  "publicClient": false,
  "bearerOnly": true,
  "standardFlowEnabled": false,
  "directAccessGrantsEnabled": false,
  "serviceAccountsEnabled": true
}
```

### Role Mappings
```
Realm Roles:
- admin          → Full system access
- customer       → Basic customer access
- premium        → Premium features access
- merchant       → Seller/vendor access
- support        → Customer support access
- system         → Service-to-service communication

Client Roles (per service):
user-service:
  - user.read
  - user.write
  - user.delete

product-service:
  - product.read
  - product.write
  - product.manage

order-service:
  - order.read
  - order.create
  - order.manage
  - order.refund
```

## API Endpoints

### Authentication Endpoints
```
POST   /realms/{realm}/protocol/openid-connect/token
       # Login, get access/refresh tokens

POST   /realms/{realm}/protocol/openid-connect/logout
       # Logout, invalidate session

POST   /realms/{realm}/protocol/openid-connect/token/introspect
       # Validate token

POST   /realms/{realm}/protocol/openid-connect/token/revoke
       # Revoke token

GET    /realms/{realm}/protocol/openid-connect/certs
       # Get public keys for JWT validation

GET    /realms/{realm}/.well-known/openid-configuration
       # OIDC discovery endpoint
```

### User Management (Admin API)
```
GET    /admin/realms/{realm}/users
       # List users

POST   /admin/realms/{realm}/users
       # Create user

GET    /admin/realms/{realm}/users/{id}
       # Get user details

PUT    /admin/realms/{realm}/users/{id}
       # Update user

DELETE /admin/realms/{realm}/users/{id}
       # Delete user

PUT    /admin/realms/{realm}/users/{id}/reset-password
       # Reset user password

GET    /admin/realms/{realm}/users/{id}/sessions
       # Get user sessions

DELETE /admin/realms/{realm}/users/{id}/sessions
       # Logout user sessions
```

### Account Management (User API)
```
GET    /realms/{realm}/account
       # Get account details

POST   /realms/{realm}/account
       # Update account

POST   /realms/{realm}/account/credentials/password
       # Change password

GET    /realms/{realm}/account/sessions
       # List active sessions

DELETE /realms/{realm}/account/sessions/{id}
       # Delete specific session
```

## Database Schema (PostgreSQL - Code-First via Keycloak JPA)

### Core Tables (Auto-generated by Keycloak)
```sql
-- User management
USER_ENTITY
USER_ATTRIBUTE
USER_ROLE_MAPPING
USER_GROUP_MEMBERSHIP
CREDENTIAL

-- Client management
CLIENT
CLIENT_SCOPE
CLIENT_SCOPE_CLIENT
CLIENT_ATTRIBUTES

-- Role management
KEYCLOAK_ROLE
ROLE_ATTRIBUTE

-- Session management
USER_SESSION
AUTH_SESSION
CLIENT_SESSION

-- Realm configuration
REALM
REALM_ATTRIBUTE
IDENTITY_PROVIDER
AUTHENTICATION_FLOW
```

## Environment Variables
```
# Database
DB_VENDOR=postgres
DB_ADDR=postgres:5432
DB_DATABASE=keycloak
DB_USER=keycloak
DB_PASSWORD=***
DB_SCHEMA=public

# Keycloak Admin
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=***

# Hostname
KC_HOSTNAME=auth.example.com
KC_HOSTNAME_STRICT=true
KC_HOSTNAME_STRICT_HTTPS=true

# Multi-region clustering
KC_CACHE=ispn
KC_CACHE_STACK=kubernetes
JGROUPS_DISCOVERY_PROTOCOL=DNS_PING

# Logging
KC_LOG_LEVEL=info
KC_LOG_CONSOLE_COLOR=true

# Features
KC_FEATURES=token-exchange,admin-fine-grained-authz
KC_HEALTH_ENABLED=true
KC_METRICS_ENABLED=true

# Security
KC_HTTPS_CERTIFICATE_FILE=/opt/keycloak/certs/tls.crt
KC_HTTPS_CERTIFICATE_KEY_FILE=/opt/keycloak/certs/tls.key
```

## Implementation Tasks

### Phase 1: Basic Setup
- [ ] Deploy Keycloak with PostgreSQL
- [ ] Create main realm (ecommerce)
- [ ] Configure admin user
- [ ] Set up database persistence
- [ ] Configure logging

### Phase 2: Client & Role Configuration
- [ ] Create clients for all services
- [ ] Define realm roles (admin, customer, merchant, etc.)
- [ ] Create client-specific roles
- [ ] Configure role mappings
- [ ] Set up default groups

### Phase 3: Authentication Flows
- [ ] Configure browser authentication flow
- [ ] Enable MFA/2FA
- [ ] Set up social login (Google, Facebook)
- [ ] Configure passwordless authentication
- [ ] Customize login/registration pages

### Phase 4: Security Policies
- [ ] Configure password policies
- [ ] Set up brute force detection
- [ ] Configure session timeouts
- [ ] Enable security event logging
- [ ] Set up GDPR compliance features

### Phase 5: Multi-Region Setup
- [ ] Configure cross-datacenter replication
- [ ] Set up session failover
- [ ] Test multi-region synchronization
- [ ] Configure conflict resolution
- [ ] Implement health checks per region

### Phase 6: Integration
- [ ] Integrate with API Gateway
- [ ] Configure service-to-service authentication
- [ ] Set up token validation in services
- [ ] Implement user context propagation
- [ ] Configure CORS policies

### Phase 7: Monitoring & Auditing
- [ ] Enable metrics export (Prometheus)
- [ ] Configure audit logging
- [ ] Set up security event monitoring
- [ ] Create Grafana dashboards
- [ ] Configure alerts for security events

## Custom Extensions (Optional)

### User Registration Validation
```java
// Custom provider for business-specific validation
public class CustomRegistrationValidation implements FormAction {
    @Override
    public void validate(ValidationContext context) {
        // Custom validation logic
    }
}
```

### Event Listener
```java
// Custom event listener for user events
public class CustomEventListener implements EventListenerProvider {
    @Override
    public void onEvent(Event event) {
        // Publish to Kafka, send notifications, etc.
    }
}
```

## Multi-Region Architecture

### Active-Active Setup
```
        Global DNS (GeoDNS)
               |
    +----------+----------+
    |          |          |
 US-WEST    EU-WEST   AP-SOUTHEAST
 Keycloak   Keycloak   Keycloak
 (Active)   (Active)   (Active)
    |          |          |
PostgreSQL PostgreSQL PostgreSQL
(Replicated across regions)
```

### Session Replication
- Use Infinispan distributed cache
- Configure cross-site replication
- Handle session stickiness at load balancer
- Implement session backup and recovery

## Monitoring & Alerts

### Key Metrics
- Login success/failure rate
- Token issuance rate
- Active sessions count
- MFA enrollment rate
- Password reset requests
- API response time

### Alerts
- Failed login spike (potential attack)
- Abnormal token issuance rate
- Session overflow
- Database connection issues
- High response time

## Testing Requirements

### Functional Tests
- [ ] User registration flow
- [ ] Login/logout flow
- [ ] Password reset flow
- [ ] MFA enrollment and verification
- [ ] Social login integration
- [ ] Token refresh flow

### Security Tests
- [ ] Brute force protection
- [ ] SQL injection attempts
- [ ] XSS prevention
- [ ] CSRF protection
- [ ] Token tampering detection
- [ ] Session hijacking prevention

### Performance Tests
- [ ] Concurrent login requests
- [ ] Token validation throughput
- [ ] Session creation/deletion
- [ ] Database query performance

## Documentation Deliverables
- [ ] Realm configuration guide
- [ ] Client integration guide
- [ ] Authentication flow diagrams
- [ ] Security policies documentation
- [ ] Multi-region setup guide
- [ ] Troubleshooting guide
- [ ] API integration examples

## Dependencies
- PostgreSQL (Database)
- API Gateway (Integration)
- All Microservices (Clients)
- Prometheus/Grafana (Monitoring)

## Success Criteria
- 99.99% uptime
- Login response time < 200ms
- Support 10K+ concurrent sessions
- Zero data loss during region failover
- SOC2/GDPR compliant
- Automatic security threat detection
