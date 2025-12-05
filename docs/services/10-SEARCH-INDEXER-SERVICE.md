# Search Indexer Service (Go/Java)

## Overview
Search indexing service that maintains product search indices in OpenSearch/Elasticsearch. Consumes product events and keeps search index synchronized with product catalog in real-time.

## Technology Stack
- **Language**: Go 1.21+ or Java Spring Boot 3
- **Framework**: Fiber/Echo (Go) or Spring Boot (Java)
- **Search Engine**: OpenSearch OSS / Elasticsearch OSS
- **Cache**: Redis
- **Messaging**: Kafka (Consumer)
- **Database**: None (reads from OpenSearch)
- **API**: REST + gRPC

## Core Responsibilities

### 1. **Index Management**
- Create and manage search indices
- Index mapping configuration
- Index lifecycle management (ILM)
- Index versioning and migration
- Alias management

### 2. **Real-Time Indexing**
- Listen to product events (created, updated, deleted)
- Index new products
- Update existing product indices
- Delete removed products
- Bulk indexing for performance

### 3. **Search Query Processing**
- Full-text search
- Faceted search (filters)
- Auto-suggestions and autocomplete
- Fuzzy search and typo tolerance
- Search ranking and relevance tuning

### 4. **Search Analytics**
- Track search queries
- Monitor search performance
- Analyze search trends
- Zero-result queries tracking
- Click-through rate tracking

### 5. **Multi-Language Support**
- Language-specific analyzers
- Multi-language search
- Language detection
- Phonetic matching

### 6. **Performance Optimization**
- Query caching
- Index optimization
- Search result caching (Redis)
- Response time optimization

### 7. **Facets & Filters**
- Category filters
- Price range filters
- Brand filters
- Rating filters
- Availability filters
- Dynamic facet generation

### 8. **Search Suggestions**
- Query suggestions
- Product suggestions
- Popular searches
- Trending searches

## Architecture (Go Version)

```
search-indexer-service/
├── cmd/
│   └── main.go
├── internal/
│   ├── domain/
│   │   ├── model/
│   │   │   ├── product_document.go
│   │   │   ├── search_query.go
│   │   │   └── search_result.go
│   │   └── repository/
│   │       └── search_repository.go
│   │
│   ├── usecase/
│   │   ├── indexing_usecase.go
│   │   ├── search_usecase.go
│   │   └── suggestion_usecase.go
│   │
│   ├── adapter/
│   │   ├── http/
│   │   │   ├── handler/
│   │   │   │   ├── search_handler.go
│   │   │   │   └── suggestion_handler.go
│   │   │   └── router.go
│   │   └── grpc/
│   │       └── search_server.go
│   │
│   └── infrastructure/
│       ├── opensearch/
│       │   ├── client.go
│       │   ├── index_manager.go
│       │   ├── indexer.go
│       │   └── searcher.go
│       ├── cache/
│       │   └── redis_cache.go
│       ├── messaging/
│       │   ├── kafka_consumer.go
│       │   └── event_handler/
│       │       └── product_event_handler.go
│       └── config/
│           └── config.go
│
├── mappings/
│   └── product_mapping.json
│
├── go.mod
├── Dockerfile
└── README.md
```

## Product Document Schema (OpenSearch)

```json
{
  "settings": {
    "number_of_shards": 5,
    "number_of_replicas": 2,
    "analysis": {
      "analyzer": {
        "product_analyzer": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": ["lowercase", "asciifolding", "product_synonym"]
        },
        "autocomplete_analyzer": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": ["lowercase", "edge_ngram"]
        }
      },
      "filter": {
        "edge_ngram": {
          "type": "edge_ngram",
          "min_gram": 2,
          "max_gram": 10
        },
        "product_synonym": {
          "type": "synonym",
          "synonyms": [
            "tv, television",
            "mobile, phone, smartphone"
          ]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "productId": {
        "type": "keyword"
      },
      "sku": {
        "type": "keyword"
      },
      "name": {
        "type": "text",
        "analyzer": "product_analyzer",
        "fields": {
          "autocomplete": {
            "type": "text",
            "analyzer": "autocomplete_analyzer"
          },
          "keyword": {
            "type": "keyword"
          }
        }
      },
      "description": {
        "type": "text",
        "analyzer": "product_analyzer"
      },
      "category": {
        "type": "object",
        "properties": {
          "id": {"type": "keyword"},
          "name": {"type": "text"},
          "path": {"type": "text"}
        }
      },
      "brand": {
        "type": "keyword"
      },
      "price": {
        "type": "double"
      },
      "currency": {
        "type": "keyword"
      },
      "rating": {
        "type": "float"
      },
      "reviewCount": {
        "type": "integer"
      },
      "status": {
        "type": "keyword"
      },
      "inStock": {
        "type": "boolean"
      },
      "images": {
        "type": "keyword"
      },
      "tags": {
        "type": "keyword"
      },
      "attributes": {
        "type": "nested",
        "properties": {
          "name": {"type": "keyword"},
          "value": {"type": "keyword"}
        }
      },
      "createdAt": {
        "type": "date"
      },
      "updatedAt": {
        "type": "date"
      }
    }
  }
}
```

## Product Document Model (Go)

```go
type ProductDocument struct {
    ProductID   string                 `json:"productId"`
    SKU         string                 `json:"sku"`
    Name        string                 `json:"name"`
    Description string                 `json:"description"`
    Category    CategoryInfo           `json:"category"`
    Brand       string                 `json:"brand"`
    Price       float64                `json:"price"`
    Currency    string                 `json:"currency"`
    Rating      float64                `json:"rating"`
    ReviewCount int                    `json:"reviewCount"`
    Status      string                 `json:"status"`
    InStock     bool                   `json:"inStock"`
    Images      []string               `json:"images"`
    Tags        []string               `json:"tags"`
    Attributes  []ProductAttribute     `json:"attributes"`
    CreatedAt   time.Time              `json:"createdAt"`
    UpdatedAt   time.Time              `json:"updatedAt"`
}

type CategoryInfo struct {
    ID   string `json:"id"`
    Name string `json:"name"`
    Path string `json:"path"`  // Electronics > Phones > Smartphones
}

type ProductAttribute struct {
    Name  string `json:"name"`
    Value string `json:"value"`
}
```

## Search Query Model

```go
type SearchRequest struct {
    Query       string            `json:"query"`
    Filters     map[string]interface{} `json:"filters"`
    Sort        []SortOption      `json:"sort"`
    From        int               `json:"from"`
    Size        int               `json:"size"`
    Facets      []string          `json:"facets"`
}

type SortOption struct {
    Field string `json:"field"`
    Order string `json:"order"`  // asc, desc
}

type SearchResponse struct {
    Total       int64             `json:"total"`
    Took        int               `json:"took"` // milliseconds
    Products    []ProductDocument `json:"products"`
    Facets      map[string]Facet  `json:"facets"`
    Suggestions []string          `json:"suggestions"`
}

type Facet struct {
    Name    string        `json:"name"`
    Buckets []FacetBucket `json:"buckets"`
}

type FacetBucket struct {
    Key      string `json:"key"`
    DocCount int64  `json:"docCount"`
}
```

## API Endpoints

### REST API

#### Search
```
GET    /api/v1/search                     # Search products
POST   /api/v1/search                     # Advanced search
GET    /api/v1/search/suggestions         # Auto-suggestions
GET    /api/v1/search/trending            # Trending searches
GET    /api/v1/search/facets              # Get available facets
```

#### Indexing (Admin)
```
POST   /api/v1/admin/index/create         # Create index
POST   /api/v1/admin/index/reindex        # Full reindex
DELETE /api/v1/admin/index/:name          # Delete index
GET    /api/v1/admin/index/stats          # Index statistics
```

#### Health
```
GET    /health                            # Health check
GET    /metrics                           # Prometheus metrics
```

### gRPC API

```protobuf
service SearchService {
  rpc Search(SearchRequest) returns (SearchResponse);
  rpc Suggest(SuggestRequest) returns (SuggestResponse);
  rpc GetFacets(GetFacetsRequest) returns (FacetsResponse);
}
```

## OpenSearch Query Examples

### Full-Text Search
```json
{
  "query": {
    "multi_match": {
      "query": "smartphone",
      "fields": ["name^3", "description", "brand^2", "tags"],
      "type": "best_fields",
      "fuzziness": "AUTO"
    }
  }
}
```

### Faceted Search
```json
{
  "query": {
    "bool": {
      "must": [
        {"match": {"name": "laptop"}}
      ],
      "filter": [
        {"term": {"brand": "Apple"}},
        {"range": {"price": {"gte": 500, "lte": 2000}}},
        {"term": {"inStock": true}}
      ]
    }
  },
  "aggs": {
    "brands": {
      "terms": {"field": "brand", "size": 20}
    },
    "price_ranges": {
      "range": {
        "field": "price",
        "ranges": [
          {"to": 500},
          {"from": 500, "to": 1000},
          {"from": 1000, "to": 2000},
          {"from": 2000}
        ]
      }
    },
    "categories": {
      "terms": {"field": "category.name", "size": 10}
    }
  }
}
```

### Auto-Suggestions
```json
{
  "suggest": {
    "product_suggest": {
      "prefix": "iph",
      "completion": {
        "field": "name.autocomplete",
        "size": 10,
        "fuzzy": {
          "fuzziness": 1
        }
      }
    }
  }
}
```

## Kafka Event Handlers

### Consumed Events
```go
// product.created → Index new product
func (h *ProductEventHandler) HandleProductCreated(event ProductCreatedEvent) error {
    doc := mapToProductDocument(event)
    return h.indexer.IndexProduct(context.Background(), doc)
}

// product.updated → Update index
func (h *ProductEventHandler) HandleProductUpdated(event ProductUpdatedEvent) error {
    doc := mapToProductDocument(event)
    return h.indexer.UpdateProduct(context.Background(), doc.ProductID, doc)
}

// product.deleted → Remove from index
func (h *ProductEventHandler) HandleProductDeleted(event ProductDeletedEvent) error {
    return h.indexer.DeleteProduct(context.Background(), event.ProductID)
}

// inventory.stock_updated → Update stock status
func (h *ProductEventHandler) HandleStockUpdated(event StockUpdatedEvent) error {
    return h.indexer.UpdateProductField(
        context.Background(),
        event.ProductID,
        "inStock",
        event.NewStock > 0,
    )
}
```

## Indexer Implementation

```go
type OpenSearchIndexer struct {
    client *opensearch.Client
    index  string
}

func (i *OpenSearchIndexer) IndexProduct(ctx context.Context, doc ProductDocument) error {
    body, err := json.Marshal(doc)
    if err != nil {
        return err
    }

    req := opensearchapi.IndexRequest{
        Index:      i.index,
        DocumentID: doc.ProductID,
        Body:       bytes.NewReader(body),
        Refresh:    "true",
    }

    res, err := req.Do(ctx, i.client)
    if err != nil {
        return err
    }
    defer res.Body.Close()

    if res.IsError() {
        return fmt.Errorf("indexing failed: %s", res.String())
    }

    return nil
}

func (i *OpenSearchIndexer) BulkIndex(ctx context.Context, docs []ProductDocument) error {
    var buf bytes.Buffer

    for _, doc := range docs {
        // Action metadata
        meta := map[string]interface{}{
            "index": map[string]string{
                "_index": i.index,
                "_id":    doc.ProductID,
            },
        }
        metaJSON, _ := json.Marshal(meta)
        buf.Write(metaJSON)
        buf.WriteByte('\n')

        // Document
        docJSON, _ := json.Marshal(doc)
        buf.Write(docJSON)
        buf.WriteByte('\n')
    }

    req := opensearchapi.BulkRequest{
        Body:    &buf,
        Refresh: "true",
    }

    res, err := req.Do(ctx, i.client)
    if err != nil {
        return err
    }
    defer res.Body.Close()

    if res.IsError() {
        return fmt.Errorf("bulk indexing failed: %s", res.String())
    }

    return nil
}
```

## Search Implementation

```go
func (s *OpenSearchSearcher) Search(ctx context.Context, req SearchRequest) (*SearchResponse, error) {
    query := buildQuery(req)

    searchReq := opensearchapi.SearchRequest{
        Index: []string{s.index},
        Body:  strings.NewReader(query),
    }

    res, err := searchReq.Do(ctx, s.client)
    if err != nil {
        return nil, err
    }
    defer res.Body.Close()

    var result map[string]interface{}
    if err := json.NewDecoder(res.Body).Decode(&result); err != nil {
        return nil, err
    }

    return parseSearchResponse(result), nil
}

func buildQuery(req SearchRequest) string {
    query := map[string]interface{}{
        "query": map[string]interface{}{
            "bool": map[string]interface{}{
                "must": []interface{}{
                    map[string]interface{}{
                        "multi_match": map[string]interface{}{
                            "query":     req.Query,
                            "fields":    []string{"name^3", "description", "brand^2"},
                            "fuzziness": "AUTO",
                        },
                    },
                },
                "filter": buildFilters(req.Filters),
            },
        },
        "from": req.From,
        "size": req.Size,
        "aggs": buildAggregations(req.Facets),
    }

    if len(req.Sort) > 0 {
        query["sort"] = buildSort(req.Sort)
    }

    jsonQuery, _ := json.Marshal(query)
    return string(jsonQuery)
}
```

## Caching Strategy

```go
func (s *CachedSearcher) Search(ctx context.Context, req SearchRequest) (*SearchResponse, error) {
    // Generate cache key
    cacheKey := fmt.Sprintf("search:%s", hashRequest(req))

    // Check cache
    cached, err := s.cache.Get(ctx, cacheKey)
    if err == nil {
        var result SearchResponse
        json.Unmarshal([]byte(cached), &result)
        return &result, nil
    }

    // Search OpenSearch
    result, err := s.searcher.Search(ctx, req)
    if err != nil {
        return nil, err
    }

    // Cache result (5 minutes TTL)
    resultJSON, _ := json.Marshal(result)
    s.cache.Set(ctx, cacheKey, string(resultJSON), 5*time.Minute)

    return result, nil
}
```

## Environment Variables
```bash
# OpenSearch
OPENSEARCH_HOST=opensearch
OPENSEARCH_PORT=9200
OPENSEARCH_USERNAME=admin
OPENSEARCH_PASSWORD=***
OPENSEARCH_INDEX=products
OPENSEARCH_SHARDS=5
OPENSEARCH_REPLICAS=2

# Redis
REDIS_HOST=redis
REDIS_PORT=6379

# Kafka
KAFKA_BROKERS=kafka:9092
KAFKA_GROUP_ID=search-indexer
KAFKA_TOPIC_PRODUCT_EVENTS=product.events
KAFKA_TOPIC_INVENTORY_EVENTS=inventory.events

# Application
APP_PORT=8080
GRPC_PORT=9090
LOG_LEVEL=info
```

## Implementation Tasks

### Phase 1: Setup
- [ ] Initialize project (Go/Java)
- [ ] Set up OpenSearch client
- [ ] Configure index mappings
- [ ] Create initial index

### Phase 2: Indexing
- [ ] Implement product indexer
- [ ] Add bulk indexing
- [ ] Create event handlers
- [ ] Set up Kafka consumers

### Phase 3: Search
- [ ] Implement search query builder
- [ ] Add faceted search
- [ ] Implement auto-suggestions
- [ ] Add result ranking

### Phase 4: Optimization
- [ ] Add Redis caching
- [ ] Optimize queries
- [ ] Implement query monitoring
- [ ] Add performance metrics

### Phase 5: API Layer
- [ ] Implement REST endpoints
- [ ] Create gRPC service
- [ ] Add authentication

### Phase 6: Testing
- [ ] Unit tests
- [ ] Integration tests
- [ ] Load testing
- [ ] Search relevance testing

## Testing Requirements

### Unit Tests
- [ ] Query builder logic
- [ ] Document mapping
- [ ] Filter generation

### Integration Tests
- [ ] Indexing operations
- [ ] Search queries
- [ ] Faceted search
- [ ] Auto-suggestions

### Performance Tests
- [ ] Search latency
- [ ] Bulk indexing throughput
- [ ] Concurrent search requests

## Success Criteria
- 99.99% uptime
- Search latency < 50ms (p95)
- Support 10K+ searches/second
- Index update latency < 1 second
- Search relevance > 90%
- Zero data loss during reindexing
