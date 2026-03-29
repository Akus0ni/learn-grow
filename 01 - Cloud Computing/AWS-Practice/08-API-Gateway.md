# AWS API Gateway — API Management

## 1. Core Concepts

### What is API Gateway?
Amazon API Gateway is a **fully managed service** for creating, deploying, securing, monitoring, and scaling APIs. It acts as the "front door" for applications to access backend services (Lambda, EC2, ECS, HTTP endpoints, AWS services).

### API Types

| Type | Protocol | Use Case |
|------|---------|---------|
| **REST API** | HTTP/HTTPS | Full-featured API management — throttling, caching, request/response transformation, API keys, usage plans |
| **HTTP API** | HTTP/HTTPS | Low latency, low cost (~70% cheaper) — simpler feature set; best for Lambda + OIDC JWT auth |
| **WebSocket API** | WebSocket | Bidirectional, persistent connections — chat, real-time notifications, gaming |

### REST API vs HTTP API
| Feature | REST API | HTTP API |
|---------|---------|---------|
| Cost | Higher | ~70% cheaper |
| Latency | Slightly higher | Lower |
| Caching | Yes | No |
| Request/response transform | Yes (mapping templates) | No |
| Usage plans & API keys | Yes | No |
| WAF integration | Yes | Yes |
| Private integrations | Yes | Yes |
| JWT authorizer | Yes | Yes (native, simpler) |
| Lambda authorizer | Yes | Yes |
| IAM auth | Yes | Yes |
| Cognito auth | Yes | Yes (native) |
| X-Ray | Yes | Yes |
| Use case | Full enterprise API gateway | Simple APIs, serverless backends |

### Integration Types
| Type | Target | Notes |
|------|--------|-------|
| **Lambda Proxy** | Lambda function | Full request passed to Lambda; Lambda returns structured response |
| **Lambda Non-Proxy** | Lambda function | API GW transforms request using mapping templates |
| **HTTP Proxy** | HTTP endpoint | Pass-through to backend HTTP URL |
| **HTTP** | HTTP endpoint | Transform request/response with mapping templates |
| **AWS Service** | Any AWS service | Direct integration (e.g., SQS, Kinesis, DynamoDB) without Lambda |
| **Mock** | API GW itself | Return pre-defined response — useful for testing |

### Deployment & Stages
- **Stage**: Deployment environment (dev, staging, prod). Each stage has its own URL.
- **Stage Variables**: Like environment variables for a stage (point to different Lambda aliases or backend URLs)
- **Canary Deployments**: Route X% of traffic to new deployment for testing

### Throttling & Quotas
- **Account-level**: 10,000 requests/second (RPS), 5,000 burst (default, can increase)
- **Stage/Method-level throttling**: Override per route
- **Usage Plans + API Keys**: Throttle and quota by customer/tier (free, pro, enterprise)
- When throttled: 429 Too Many Requests

### Caching
- Enable per stage: cache TTL 0–3600 seconds (default: 300s)
- Cache capacity: 0.5 GB – 237 GB
- Cache key: method, path, headers, query params (configurable)
- **Cache invalidation**: `Cache-Control: max-age=0` header (if client is authorized)
- Reduces backend calls — significant cost and latency reduction for repeated requests

### Authorization Mechanisms
| Type | How | Use Case |
|------|-----|---------|
| **IAM (AWS_IAM)** | SigV4 signed requests | Internal AWS services, machine-to-machine |
| **Cognito User Pools** | JWT from Cognito | Mobile/web apps with user accounts |
| **Lambda Authorizer (Token)** | Custom JWT/OAuth2 token | Third-party IdPs, custom auth logic |
| **Lambda Authorizer (Request)** | Header, query param, path, cookie | Multi-param custom auth |
| **API Key** | Static key in header | Simple partner API access + usage tracking |
| **Resource Policy** | IP allowlist, VPC, account | Restrict access by source IP, VPC endpoint |

### CORS (Cross-Origin Resource Sharing)
Enable CORS on API Gateway when browsers call the API from a different origin:
- REST API: Enable CORS in console → adds OPTIONS method with required headers
- HTTP API: Built-in CORS configuration
- Headers needed: `Access-Control-Allow-Origin`, `Access-Control-Allow-Methods`, `Access-Control-Allow-Headers`

### Private APIs
Use **VPC Endpoint (Interface)** for API Gateway to create a private API accessible only from within your VPC. No internet traffic.

### WebSocket API Concepts
- **Connection ID**: Unique per WebSocket connection
- **$connect**: Route called on client connection (auth here)
- **$disconnect**: Route called on disconnect
- **$default**: Catch-all route
- **Custom routes**: Based on message content (e.g., `action: "sendMessage"`)
- **PostToConnection API**: Server pushes messages to specific connection IDs
- Connections stored in DynamoDB (track active users)

---

## 2. Interview Questions & Answers

### Q1. What is the difference between REST API and HTTP API in API Gateway?
**REST API**: Full-featured — supports caching, request/response transformation (Velocity templates), usage plans, API keys, AWS X-Ray, WAF, resource policies. More expensive and slightly higher latency.

**HTTP API**: Simplified, lower cost (~70% cheaper), lower latency. Supports JWT authorizers natively, Cognito, Lambda proxy. Missing: caching, transformation templates, usage plans, API keys.

**When to use which**: Use HTTP API for new serverless backends with JWT/Cognito auth where you don't need caching or request transformation. Use REST API when you need API keys for partner management, response caching, or complex request/response mapping.

---

### Q2. What are the different ways to authenticate API Gateway requests?
1. **IAM Auth**: Callers sign requests with AWS SigV4. Best for machine-to-machine within AWS.
2. **Cognito User Pools**: Issue JWTs to authenticated users. API Gateway validates JWTs natively.
3. **Lambda Authorizer (Token-based)**: API GW calls a Lambda with the token; Lambda returns an IAM policy (allow/deny). Use for third-party OAuth2, custom tokens.
4. **Lambda Authorizer (Request-based)**: Lambda receives full request context (headers, params, etc.) for complex auth logic.
5. **API Keys + Usage Plans**: Simple key-based access tracking, throttling per customer tier.

---

### Q3. What is a Lambda Authorizer (Custom Authorizer)?
A Lambda function invoked by API Gateway before the request reaches the backend. It:
1. Receives a token (or full request for request-based)
2. Validates the token (JWT signature, expiry, scope)
3. Returns an **IAM policy** (Allow or Deny on `execute-api:Invoke`)
4. Optionally returns a **context** object with user data (user ID, email) passed to the backend
5. Optionally enables **caching** of the policy for TTL seconds (avoid Lambda cold starts on every request)

---

### Q4. How does API Gateway handle throttling?
API Gateway throttles requests at:
- **Account level**: 10,000 RPS, 5,000 burst (token bucket algorithm)
- **Stage/method level**: Override for specific routes
- **Usage plan**: Per-key throttle and monthly quota

When throttled: **429 Too Many Requests** response. Clients should implement exponential backoff and retry.

---

### Q5. What is an API Gateway Stage and how are stage variables used?
A **Stage** represents a snapshot deployment of the API (e.g., `dev`, `staging`, `prod`). Each has its own URL: `https://{id}.execute-api.{region}.amazonaws.com/{stage}/`.

**Stage Variables**: String key-value pairs configured per stage. Reference in integration URIs with `${stageVariables.varName}`:
- Stage `prod`: `lambdaAlias = prod` → invokes `my-function:prod`
- Stage `dev`: `lambdaAlias = dev` → invokes `my-function:dev`

Same API definition, different backends per stage — no code changes needed.

---

### Q6. Explain API Gateway Canary Deployments.
A canary deployment allows you to route a configurable percentage of traffic to a new stage deployment:
1. Deploy new integration (e.g., new Lambda version) as a "canary"
2. Route e.g. 5% of production traffic to canary, 95% to original
3. Monitor error rate, latency in CloudWatch
4. Promote canary to 100% (or roll back)

This is API-layer canary — different from the CodeDeploy-level ECS canary.

---

### Q7. What is the difference between API Gateway and an Application Load Balancer?
| | API Gateway | ALB |
|--|------------|-----|
| Protocol | HTTP/HTTPS/WebSocket | HTTP/HTTPS/gRPC/WebSocket |
| Auth | IAM, Cognito, Lambda auth, API keys | Cognito (OIDC), basic rule-based |
| Throttling | Built-in per route | No |
| Caching | Yes (REST API) | No |
| Request transform | Yes (REST API) | No |
| Backend | Lambda, HTTP, AWS services | EC2, ECS, Lambda, IP, ALB |
| Cost | Per request + data | Per LCU-hour + data |
| Use case | Public/partner APIs, serverless | Web app load distribution, microservices |

Use API Gateway when you need API management features (auth, throttling, caching, API keys). Use ALB for HTTP traffic distribution to compute backends.

---

### Q8. How does caching work in API Gateway?
Enable caching on a REST API stage with configurable TTL (default 300s, max 3600s) and cache size (0.5–237 GB). Cache key includes the method + path + configured headers/query params.

When a cached response exists, API Gateway returns it directly without calling the backend — significant cost and latency reduction.

**Invalidation**: Clients can send `Cache-Control: max-age=0` header to bypass cache (if the method is configured to allow it). Or you invalidate all cache entries via the console/CLI.

---

### Q9. What is the maximum request/response size in API Gateway?
- **Maximum payload (request + response)**: **10 MB** for REST and HTTP APIs
- **Maximum WebSocket frame size**: **128 KB**
- **Maximum timeout**: **29 seconds** for REST/HTTP API (integration timeout)

These are hard limits. For large file uploads, use pre-signed S3 URLs instead of routing through API Gateway.

---

### Q10. How do you secure API Gateway with WAF?
Associate an **AWS WAF Web ACL** with the API Gateway stage (REST or HTTP). WAF rules:
- **Managed rule groups**: AWS Managed Rules (OWASP Top 10, SQL injection, XSS, known bad inputs)
- **Rate-based rules**: Throttle by IP (limit burst from single IPs)
- **IP set rules**: Allowlist/blocklist specific IPs or CIDRs
- **Geo match rules**: Block requests from specific countries
- **Custom rules**: Pattern matching on headers, query strings, request body

WAF operates before API Gateway processes the request — blocked requests return 403.

---

### Q11. What is an API Gateway Resource Policy?
A resource policy (JSON) attached to a REST API controls which principals can invoke it:
- Restrict to specific AWS accounts, VPCs, or VPC endpoints
- Block specific IP addresses or CIDRs
- Allow cross-account access

Common use case: Private API accessible only from a specific VPC endpoint:
```json
{
  "Effect": "Deny",
  "Principal": "*",
  "Action": "execute-api:Invoke",
  "Resource": "arn:aws:execute-api:...",
  "Condition": {
    "StringNotEquals": {
      "aws:SourceVpce": "vpce-1234abcd"
    }
  }
}
```

---

### Q12. How do you handle CORS in API Gateway?
**CORS** allows browsers to make cross-origin API requests. When a browser sends a cross-origin request, it first sends a preflight `OPTIONS` request.

**REST API**: Enable CORS in the console for each resource → API GW adds `OPTIONS` method with required response headers (`Access-Control-Allow-Origin`, etc.).

**HTTP API**: Configure CORS settings at the API level (specifying allowed origins, methods, headers).

**Important**: Your Lambda backend must NOT add CORS headers — API Gateway handles it. Common mistake is double-setting headers causing browser errors.

---

### Q13. What is a usage plan and API key?
**API Key**: A string identifier (`x-api-key` header) that clients include with requests. Identifies the caller (partner, customer) for tracking and throttling — NOT a security mechanism.

**Usage Plan**: Associates API keys with throttle limits and quotas:
- Throttle: 100 RPS burst, 50 RPS steady
- Quota: 10,000 requests/month

Common pattern: Free tier (100 req/day), Pro tier (10,000 req/day), Enterprise (unlimited).

---

### Q14. Explain the integration between API Gateway and SQS without Lambda.
API Gateway can integrate directly with SQS (AWS Service integration without Lambda):
1. Configure REST API resource with `POST /orders`
2. Integration type: AWS
3. Action: `sqs:SendMessage`
4. Mapping template: Transform HTTP body into SQS message format
5. Response: Return 200 with message ID

**Benefit**: No Lambda needed — one less hop, lower cost, API Gateway → SQS is synchronous acknowledgment. The actual processing happens asynchronously via SQS consumer.

---

### Q15. What monitoring metrics does API Gateway expose?
Metrics in CloudWatch (per API/stage/resource/method):
- **Count**: Total API calls
- **Latency**: End-to-end request time (API GW + integration)
- **IntegrationLatency**: Backend-only latency
- **4XXError**: Client errors (4xx)
- **5XXError**: Server/integration errors (5xx)
- **CacheHitCount** / **CacheMissCount**: Caching effectiveness

Set alarms on `5XXError` (threshold > 1%) and `Latency` (threshold > 2000ms) tied to SNS for on-call alerts.

Enable **Access Logs**: Full request/response logging to CloudWatch or Kinesis Firehose → S3.

Enable **X-Ray**: Distributed tracing through API GW to Lambda — view end-to-end traces.

---

## 3. Real-World Use Case: Serverless REST API Architecture

### Scenario
A startup needs to build a ride-sharing platform API that:
- Handles 50,000 requests/minute during peak hours
- Authenticates users via mobile app (JWT from Cognito)
- Partners get metered API access with throttling per tier
- Must not expose internal architecture
- Sub-200ms P99 response time for read endpoints

### Architecture

```
Mobile App / Partner                     Web App
     │                                      │
     └──────────────┬───────────────────────┘
                    │
           [Route 53] (api.rideshare.com)
                    │
              [AWS WAF]
                    │
           [API Gateway REST API]
           ─────────────────────
           Stage: prod
           Custom domain: api.rideshare.com
           Caching: enabled (GET endpoints, 60s TTL)
                    │
         ┌──────────┼──────────┬──────────────┐
         │          │          │              │
  [Cognito     [Lambda     [Lambda       [Lambda
   Auth]        Authorizer   /rides        /drivers
   (mobile      (Partner     GET,POST]     GET]
    users)       JWT)]         │             │
                         ┌─────┴──────┐      │
                         │            │      │
                      [Lambda]    [Lambda] [Lambda]
                       Create     Get      Search
                       Ride       Ride     Drivers
                         │            │      │
                    [DynamoDB]  [ElastiCache] [OpenSearch]
                    (rides)     (active rides) (driver search)
                         │
                      [SQS]
                         │
                    [Lambda: Notify]
                         │
                      [SNS → Push notifications]

Usage Plans:
  Partner-Free: 100 req/min, 1,000/day (API key required)
  Partner-Pro: 1,000 req/min, 100,000/day
  Partner-Enterprise: 10,000 req/min, unlimited

Monitoring:
  CloudWatch Alarm: 5XXError > 0.5% → PagerDuty
  CloudWatch Alarm: Latency P99 > 500ms → Slack
  X-Ray: Trace from API GW through Lambda to DynamoDB
```

### Key Design Decisions

| Decision | Reason |
|----------|--------|
| REST API (not HTTP API) | Need caching for `GET /rides` (frequent, same response), usage plans for partner tiers |
| Cognito for mobile users | Native JWT issuance and validation — no custom Lambda authorizer needed |
| Lambda Authorizer for partners | Third-party OAuth2 partner tokens require custom validation logic; cache policy for 5 min reduces cold starts |
| Caching on GET /rides | Popular rides queried repeatedly; 60s cache reduces DynamoDB calls by ~90% |
| API GW → SQS direct (no Lambda) | Notifications are fire-and-forget; no Lambda needed to enqueue |
| WAF rate-based rules | Prevent 429 bursts from individual IPs even within usage plan limits |
| Custom domain + Route 53 | Decouple URL from API Gateway internal ID; easy blue/green at DNS level |

### Interview Narration (White-board Script)
> "For the ride-sharing API I'd use API Gateway REST API because I need caching on the frequent `GET /rides` endpoint and usage plans to meter partner access. Mobile users authenticate via Cognito — API Gateway validates JWTs natively without a Lambda call, which keeps latency low. Partners use a Lambda Authorizer with a 5-minute policy cache, so we only call the authorizer Lambda on the first request in each window. For notifications I integrate API Gateway directly with SQS — no Lambda needed in that path. CloudWatch alarms on 5XX error rate and P99 latency ensure we know immediately if something degrades."
