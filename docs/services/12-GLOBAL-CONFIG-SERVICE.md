# Global Config Service (Consul + Vault)

## Overview
Centralized configuration and secrets management using Consul for service discovery and configuration, and Vault for secrets management. This is not a custom service but a configuration of these OSS tools.

## Technology Stack
- **Configuration**: HashiCorp Consul OSS
- **Secrets**: HashiCorp Vault OSS
- **No Database**: In-memory + persistent storage in Consul/Vault
- **API**: Consul HTTP API, Vault HTTP API

## Core Responsibilities

### 1. **Service Discovery (Consul)**
- Service registration
- Service health checking
- DNS-based service discovery
- Load balancing via Consul DNS
- Service mesh capabilities (Consul Connect)

### 2. **Configuration Management (Consul)**
- Centralized configuration storage
- Key-Value store for application configs
- Configuration versioning
- Dynamic configuration updates
- Multi-datacenter configuration replication

### 3. **Secrets Management (Vault)**
- Database credentials management
- API keys storage
- Encryption keys
- TLS certificates
- Dynamic secret generation
- Secret rotation

### 4. **Access Control**
- ACL (Access Control Lists) in Consul
- Policy-based access in Vault
- Token-based authentication
- Role-based access control

### 5. **Multi-Region Support**
- Cross-datacenter replication (Consul)
- Vault replication for HA
- Regional configuration isolation
- Failover management

## Consul Configuration

### Service Registration

Each microservice registers itself with Consul:

```json
{
  "service": {
    "id": "user-service-1",
    "name": "user-service",
    "tags": ["dotnet", "v1"],
    "address": "user-service-1.local",
    "port": 8080,
    "meta": {
      "version": "1.0.0",
      "region": "us-west"
    },
    "checks": [
      {
        "http": "http://user-service-1.local:8080/health",
        "interval": "10s",
        "timeout": "2s"
      }
    ]
  }
}
```

### Service Discovery

Services discover each other via Consul DNS or HTTP API:

```bash
# DNS Query
dig @consul.service.consul user-service.service.consul

# HTTP API
curl http://consul:8500/v1/catalog/service/user-service
```

### Key-Value Storage Structure

```
/config/
  /global/
    /database/
      pool_size: "100"
      timeout: "30s"
    /kafka/
      bootstrap_servers: "kafka:9092"
      group_id_prefix: "ecommerce"
    /redis/
      host: "redis"
      port: "6379"
      ttl_default: "3600"

  /user-service/
    /production/
      log_level: "info"
      api_rate_limit: "1000"
      cache_ttl: "600"
    /staging/
      log_level: "debug"
      api_rate_limit: "100"

  /product-service/
    /production/
      index_name: "products"
      search_size: "100"

  /regions/
    /us-west/
      timezone: "America/Los_Angeles"
      currency: "USD"
      tax_rate: "0.10"
    /eu-west/
      timezone: "Europe/London"
      currency: "EUR"
      tax_rate: "0.20"
```

### Health Checks Configuration

```json
{
  "checks": [
    {
      "id": "user-service-http",
      "name": "User Service HTTP Health Check",
      "http": "http://user-service:8080/health",
      "method": "GET",
      "interval": "10s",
      "timeout": "2s"
    },
    {
      "id": "user-service-db",
      "name": "User Service Database Connection",
      "tcp": "postgres:5432",
      "interval": "30s",
      "timeout": "5s"
    }
  ]
}
```

## Vault Configuration

### Secret Engines

#### Database Secrets Engine
```bash
# Enable database secrets engine
vault secrets enable database

# Configure PostgreSQL connection
vault write database/config/user-service-db \
  plugin_name=postgresql-database-plugin \
  allowed_roles="user-service-role" \
  connection_url="postgresql://{{username}}:{{password}}@postgres:5432/user_service" \
  username="vault" \
  password="vault-password"

# Create role with dynamic credentials
vault write database/roles/user-service-role \
  db_name=user-service-db \
  creation_statements="CREATE ROLE \"{{name}}\" WITH LOGIN PASSWORD '{{password}}' VALID UNTIL '{{expiration}}'; \
    GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO \"{{name}}\";" \
  default_ttl="1h" \
  max_ttl="24h"
```

#### KV Secrets Engine (v2)
```bash
# Enable KV v2 secrets engine
vault secrets enable -version=2 -path=secret kv

# Store service secrets
vault kv put secret/user-service \
  database_password="***" \
  jwt_secret="***" \
  keycloak_client_secret="***"

vault kv put secret/payment-service \
  stripe_api_key="***" \
  stripe_webhook_secret="***" \
  paypal_client_id="***" \
  paypal_client_secret="***"

vault kv put secret/notification-service \
  smtp_password="***" \
  twilio_auth_token="***" \
  fcm_server_key="***"
```

### Secrets Structure

```
secret/
  /global/
    /encryption_key
    /master_api_key

  /user-service/
    /database_password
    /jwt_secret
    /keycloak_client_secret

  /product-service/
    /database_password
    /opensearch_password

  /order-service/
    /database_password

  /payment-service/
    /stripe_api_key
    /stripe_webhook_secret
    /paypal_client_id
    /paypal_client_secret
    /encryption_key

  /notification-service/
    /smtp_password
    /twilio_account_sid
    /twilio_auth_token
    /fcm_server_key

  /api-gateway/
    /admin_token
    /rate_limit_redis_password
```

### Vault Policies

#### User Service Policy
```hcl
# user-service-policy.hcl
path "secret/data/user-service/*" {
  capabilities = ["read"]
}

path "secret/data/global/*" {
  capabilities = ["read"]
}

path "database/creds/user-service-role" {
  capabilities = ["read"]
}
```

#### Payment Service Policy
```hcl
# payment-service-policy.hcl
path "secret/data/payment-service/*" {
  capabilities = ["read"]
}

path "secret/data/global/encryption_key" {
  capabilities = ["read"]
}

path "transit/encrypt/payment" {
  capabilities = ["update"]
}

path "transit/decrypt/payment" {
  capabilities = ["update"]
}
```

### Dynamic Secret Generation

Services request short-lived credentials from Vault:

```go
// Example: Getting database credentials from Vault
func getDbCredentials(vaultClient *api.Client) (string, string, error) {
    secret, err := vaultClient.Logical().Read("database/creds/user-service-role")
    if err != nil {
        return "", "", err
    }

    username := secret.Data["username"].(string)
    password := secret.Data["password"].(string)

    return username, password, nil
}
```

## Service Integration

### Consul Integration Example (.NET)

```csharp
// Register service with Consul
public class ConsulServiceRegistration : IHostedService
{
    private readonly IConsulClient _consulClient;
    private string _registrationId;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _registrationId = $"user-service-{Guid.NewGuid()}";

        var registration = new AgentServiceRegistration
        {
            ID = _registrationId,
            Name = "user-service",
            Address = "user-service",
            Port = 8080,
            Tags = new[] { "dotnet", "v1" },
            Check = new AgentServiceCheck
            {
                HTTP = "http://user-service:8080/health",
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(2)
            }
        };

        await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
    }
}

// Get configuration from Consul KV
public class ConsulConfigurationProvider
{
    public async Task<string> GetConfigAsync(string key)
    {
        var result = await _consulClient.KV.Get($"config/user-service/{key}");
        return Encoding.UTF8.GetString(result.Response.Value);
    }
}
```

### Vault Integration Example (Go)

```go
// Get secrets from Vault
func getSecrets(vaultAddr, token string) (map[string]string, error) {
    config := &api.Config{
        Address: vaultAddr,
    }
    client, err := api.NewClient(config)
    if err != nil {
        return nil, err
    }

    client.SetToken(token)

    secret, err := client.Logical().Read("secret/data/payment-service")
    if err != nil {
        return nil, err
    }

    data := secret.Data["data"].(map[string]interface{})
    secrets := make(map[string]string)
    for k, v := range data {
        secrets[k] = v.(string)
    }

    return secrets, nil
}
```

## Deployment Architecture

### Multi-Datacenter Consul Setup

```
US-WEST Datacenter
├── Consul Server 1 (Leader)
├── Consul Server 2
├── Consul Server 3
└── Consul Clients (on each service node)

EU-WEST Datacenter
├── Consul Server 1 (Leader)
├── Consul Server 2
├── Consul Server 3
└── Consul Clients (on each service node)

WAN Federation between datacenters
```

### Vault HA Setup

```
Vault Cluster (US-WEST)
├── Vault Node 1 (Active)
├── Vault Node 2 (Standby)
└── Vault Node 3 (Standby)

Consul as Storage Backend
```

## Configuration Files

### Consul Server Configuration
```hcl
# consul-server.hcl
datacenter = "us-west"
data_dir = "/opt/consul"
log_level = "INFO"

server = true
bootstrap_expect = 3

bind_addr = "0.0.0.0"
client_addr = "0.0.0.0"

ui_config {
  enabled = true
}

ports {
  grpc = 8502
  http = 8500
  dns = 8600
}

connect {
  enabled = true
}

acl {
  enabled = true
  default_policy = "deny"
  enable_token_persistence = true
}
```

### Vault Server Configuration
```hcl
# vault-server.hcl
storage "consul" {
  address = "consul:8500"
  path = "vault/"
}

listener "tcp" {
  address = "0.0.0.0:8200"
  tls_disable = false
  tls_cert_file = "/vault/certs/tls.crt"
  tls_key_file = "/vault/certs/tls.key"
}

api_addr = "https://vault:8200"
cluster_addr = "https://vault:8201"

ui = true

log_level = "Info"
```

## Access Patterns

### Service Startup Sequence

1. **Service starts up**
2. **Authenticate with Vault** (using AppRole or Kubernetes auth)
3. **Retrieve secrets** from Vault
4. **Get dynamic DB credentials** (if needed)
5. **Read configuration** from Consul KV
6. **Register service** with Consul
7. **Start serving requests**

### Configuration Refresh

Services can watch Consul KV for configuration changes:

```go
func watchConfig(consulClient *api.Client, key string) {
    params := &api.QueryOptions{WaitIndex: 0}

    for {
        result, meta, err := consulClient.KV().Get(key, params)
        if err != nil {
            log.Printf("Error watching config: %v", err)
            time.Sleep(5 * time.Second)
            continue
        }

        if meta.LastIndex > params.WaitIndex {
            // Configuration changed
            newValue := string(result.Value)
            log.Printf("Config updated: %s = %s", key, newValue)

            // Apply new configuration
            applyConfig(newValue)
        }

        params.WaitIndex = meta.LastIndex
    }
}
```

## Environment Variables

### Consul
```bash
CONSUL_HTTP_ADDR=http://consul:8500
CONSUL_HTTP_TOKEN=***
CONSUL_DATACENTER=us-west
```

### Vault
```bash
VAULT_ADDR=https://vault:8200
VAULT_TOKEN=***
VAULT_SKIP_VERIFY=false  # Set to true for dev only
```

## Implementation Tasks

### Phase 1: Consul Setup
- [ ] Deploy Consul servers (3+ nodes)
- [ ] Configure ACLs
- [ ] Set up service registration
- [ ] Configure health checks
- [ ] Set up WAN federation (multi-region)

### Phase 2: Vault Setup
- [ ] Deploy Vault servers (3 nodes for HA)
- [ ] Initialize and unseal Vault
- [ ] Configure Consul storage backend
- [ ] Enable secret engines (KV, Database, Transit)
- [ ] Create policies and roles

### Phase 3: Service Integration
- [ ] Integrate each service with Consul
- [ ] Implement Vault secret retrieval
- [ ] Configure dynamic secret rotation
- [ ] Set up configuration watching

### Phase 4: Security
- [ ] Enable TLS for Consul
- [ ] Enable TLS for Vault
- [ ] Configure ACLs and policies
- [ ] Set up audit logging

### Phase 5: Monitoring
- [ ] Set up Consul metrics export
- [ ] Set up Vault audit logs
- [ ] Create Grafana dashboards
- [ ] Configure alerts

## Backup & Recovery

### Consul Backup
```bash
# Backup Consul snapshot
consul snapshot save backup.snap

# Restore Consul snapshot
consul snapshot restore backup.snap
```

### Vault Backup
```bash
# Backup Vault data (via Consul backend)
consul snapshot save vault-backup.snap

# Backup Vault recovery keys (manual, secure storage)
```

## Monitoring

### Consul Metrics
- Service health status
- Service registration count
- KV store operations
- DNS query rate

### Vault Metrics
- Secret read/write operations
- Token usage
- Seal/unseal status
- Backend performance

## Success Criteria
- 99.99% uptime
- Service discovery < 10ms
- Secret retrieval < 50ms
- Zero secret exposure
- Automatic failover < 5 seconds
- Support 10K+ services
