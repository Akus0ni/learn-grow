# AWS CloudFront — Content Delivery Network (CDN)

## 1. Core Concepts

### What is CloudFront?
CloudFront is AWS's **globally distributed CDN**. It caches content at **edge locations** (400+ worldwide) so users receive responses from a nearby Point of Presence (PoP) instead of the origin server. This reduces latency, offloads origin traffic, and improves availability.

### Key Terminology
| Term | Meaning |
|------|---------|
| **Distribution** | A CloudFront deployment (has a `*.cloudfront.net` domain) |
| **Origin** | The source of truth — S3 bucket, ALB, EC2, API Gateway, or any HTTP server |
| **Edge Location** | AWS data center that caches content closest to users |
| **Regional Edge Cache** | Larger caches between edge locations and origins — reduces origin requests |
| **Cache Behavior** | Rules matching URL patterns that control TTL, headers, cookies, compression |
| **OAC / OAI** | Origin Access Control / Identity — locks S3 so only CloudFront can read it |
| **Invalidation** | Force-evict objects from cache before TTL expires |
| **Lambda@Edge / CloudFront Functions** | Run code at the edge (request/response manipulation) |

### Cache Behavior Rules
```
Viewer Request → Cache Behavior Match → Cache Hit? → Yes → Return cached object
                                                  ↓ No
                                          Forward to Origin (with cache key)
                                                  ↓
                                          Cache Response → Return to viewer
```

Cache key is built from: URL path + selected headers + cookies + query strings (you control which are included).

### TTL and Cache Control
| Setting | Default | Override |
|---------|---------|---------|
| Default TTL | 86,400 s (24 hr) | `Cache-Control: max-age=N` header from origin |
| Minimum TTL | 0 | Forces CloudFront to honor `no-cache` |
| Maximum TTL | 31,536,000 s (1 yr) | Caps origin's `max-age` |

### Origin Types
| Origin | Notes |
|--------|-------|
| **S3 Bucket** | Use OAC for security; block public access on bucket |
| **S3 Website Endpoint** | Use for single-page apps; supports custom error pages |
| **ALB / EC2** | Dynamic content; restrict origin with security group allowing only CloudFront IPs |
| **API Gateway** | Throttling + caching for REST APIs |
| **Custom HTTP** | Any HTTP/HTTPS endpoint |

### Security
- **HTTPS**: Enforce viewer → CloudFront → origin with TLS; use ACM certificates (must be in `us-east-1`)
- **AWS WAF integration**: Attach a Web ACL to a distribution for OWASP protection
- **Geo-restriction**: Allow or deny countries at the distribution level
- **Field-Level Encryption**: Encrypt specific POST fields (e.g., credit card numbers) with a public key at the edge so only specific backend services can decrypt

### Lambda@Edge vs CloudFront Functions
| | CloudFront Functions | Lambda@Edge |
|--|---------------------|------------|
| Trigger | Viewer req/res only | Viewer + Origin req/res |
| Runtime | JS (ECMAScript 5.1) | Node.js, Python |
| Max duration | 1 ms | 5 s (viewer), 30 s (origin) |
| Max memory | 2 MB | 128 MB – 10 GB |
| Use case | Header manipulation, URL rewrites, auth token validation | Complex logic, A/B testing, dynamic auth |
| Cost | ~6× cheaper | Per invocation + duration |

### Pricing
- **Data transfer out** (per GB, tiered by region — cheapest in US/EU)
- **HTTP requests** (per 10,000)
- **Invalidations**: first 1,000 paths/month free; then $0.005 per path
- **No charge** for data transfer from S3 / ALB to CloudFront

---

## 2. Interview Questions & Answers

### Q1. What is the difference between an edge location and a Regional Edge Cache?
**Edge locations** are small PoPs distributed globally (400+) that serve cached content to users. They have limited storage.  
**Regional Edge Caches** (REC) are a mid-tier layer between edge locations and origins. They're larger caches (12 globally) that reduce origin requests when the object has expired at the edge. If the edge location misses, it checks the REC before going all the way to the origin.

---

### Q2. How does CloudFront handle cache invalidations? What are the trade-offs?
You call `CreateInvalidation` with wildcard paths (e.g., `/*` or `/images/*`). CloudFront propagates the invalidation to all edge locations — typically within seconds to minutes.  
**Trade-offs**: Invalidations cost money (after 1,000 paths/month free), cause a cache miss storm on the next request, and add latency. Prefer **versioned file names** (e.g., `app.v2.js`) to avoid invalidations entirely — new filenames bypass the cache automatically.

---

### Q3. How do you secure an S3 bucket so only CloudFront can access it?
Use **Origin Access Control (OAC)** (preferred over legacy OAI):
1. Create an OAC in CloudFront and attach it to the S3 origin
2. Block all public access on the S3 bucket
3. Update the bucket policy to allow `s3:GetObject` only from the CloudFront service principal with a condition on the distribution ARN
This prevents users from bypassing the CDN and hitting S3 directly.

---

### Q4. What is the difference between CloudFront Functions and Lambda@Edge?
**CloudFront Functions** run at every edge location (~1 ms budget), are very cheap, and support simple operations like header manipulation, URL rewrites, and token validation on viewer requests/responses only.  
**Lambda@Edge** runs at Regional Edge Caches, supports more complex logic (up to 30 s for origin events), and can trigger on all four event types (viewer request, origin request, origin response, viewer response). Use Lambda@Edge for A/B testing, dynamic content personalization, or auth flows that call external services.

---

### Q5. How do you configure CloudFront to serve a React single-page application?
1. Upload built assets to S3 with an S3 Static Website origin (or use OAC with a bucket origin)
2. Create a CloudFront distribution pointing to the S3 origin
3. Set the default root object to `index.html`
4. Create a **custom error response**: HTTP 403/404 → return `/index.html` with HTTP 200 — this handles client-side routing where the browser navigates to a deep URL that doesn't exist in S3
5. Use versioned file names (e.g., `main.abc123.js`) with a long `max-age` for assets; serve `index.html` with `Cache-Control: no-cache`

---

### Q6. How does CloudFront integrate with AWS WAF?
Attach a **WAF Web ACL** (in `us-east-1`) to the CloudFront distribution. WAF rules execute at every edge location before the request is forwarded to the origin. Common rules:
- AWS Managed Rules (OWASP Top 10, bot control, IP reputation)
- Rate-based rules: block IPs making >1,000 requests per 5-min window
- Geo-match rules: block traffic from specific countries
WAF is evaluated before the CloudFront cache, so even cached responses can be blocked for bad actors.

---

### Q7. What is Field-Level Encryption and when would you use it?
Field-Level Encryption lets you encrypt specific fields in an HTTPS POST request (e.g., credit card number, SSN) at the edge using a **public key**. The encrypted data travels through CloudFront and your entire backend stack, and only the specific application that holds the **private key** can decrypt it.  
Use case: PCI-DSS compliance where even backend developers should not have access to raw card numbers.

---

### Q8. How do you enforce HTTPS with CloudFront?
1. Request an ACM certificate in `us-east-1` for your custom domain
2. In the distribution, set **Viewer Protocol Policy** to "Redirect HTTP to HTTPS" or "HTTPS Only"
3. Set **Origin Protocol Policy** to HTTPS to encrypt the CloudFront → origin leg
4. Use **Security Policy** TLSv1.2_2021 (minimum) to disable old TLS versions

---

### Q9. How does CloudFront pricing work compared to serving directly from S3?
Data transfer from **S3 to CloudFront is free**. You only pay for:
- Data transfer **out from CloudFront to viewers** (cheaper than S3 direct egress in most regions)
- HTTP request counts
- Optional: Lambda@Edge invocations, WAF requests, invalidations  
At scale, CloudFront is almost always cheaper than S3 direct egress and dramatically reduces origin load.

---

### Q10. How do you do A/B testing or canary releases with CloudFront?
Use **Lambda@Edge** on the Origin Request event:
1. Read a cookie from the viewer request (or set one randomly)
2. Route 90% of requests to the `v1` origin and 10% to the `v2` origin based on the cookie value
3. Set the cookie in the Viewer Response for stickiness
This requires ensuring the cookie is part of the **cache key** so users consistently see the same version.

---

### Q11. What is the difference between a CloudFront Origin Group and a single origin?
An **Origin Group** enables origin failover. You configure a primary origin and a fallback origin. If the primary returns specific HTTP status codes (e.g., 500, 502, 503, 504), CloudFront automatically retries the request against the fallback origin.  
Use case: Primary is an ALB in `us-east-1`; failback is an ALB in `us-west-2` for disaster recovery.

---

### Q12. How does geo-restriction work in CloudFront?
CloudFront can **allow** or **deny** specific countries based on the viewer's IP geolocation. This is done at the distribution level (all cache behaviors). For more granular control (per-URL, dynamic rules), use **Lambda@Edge** to call a custom geolocation API or check headers injected by CloudFront (`CloudFront-Viewer-Country`).

---

### Q13. What CloudFront headers are automatically injected into origin requests?
CloudFront can be configured to forward:
- `CloudFront-Viewer-Country` — ISO 3166 country code
- `CloudFront-Is-Mobile-Viewer`, `CloudFront-Is-Desktop-Viewer`, `CloudFront-Is-Tablet-Viewer`
- `CloudFront-Forwarded-Proto` — http or https
- `X-Forwarded-For` — original client IP  
These are **not** forwarded by default — you must enable them in the origin request policy.

---

### Q14. How do you debug a CloudFront cache miss?
Inspect the `X-Cache` response header:
- `Hit from cloudfront` → served from edge cache
- `Miss from cloudfront` → forwarded to origin
- `RefreshHit from cloudfront` → TTL expired but revalidated with origin (304)

Also check `X-Amz-Cf-Pop` to identify which PoP served the request. Enable **CloudFront access logs** (to S3) or **real-time logs** (to Kinesis Data Streams) for detailed analysis.

---

### Q15. How would you design a globally available static website with high availability and low latency?
1. Store assets in **S3** (versioned file names, block public access)
2. Create a **CloudFront distribution** with OAC, HTTPS-only, and a custom domain
3. Attach **ACM certificate** (us-east-1)
4. Attach **WAF Web ACL** for OWASP + rate limiting
5. Configure **custom error responses** for SPA routing (404 → index.html)
6. Use **Route 53** latency routing with health checks pointing to the distribution
7. Set long `max-age` (1 year) for versioned assets; `no-cache` for `index.html`
8. **CI/CD**: Build → upload to S3 with new hash → invalidate only `index.html`

---

## 3. Real-World Use Case: Global Static + API Delivery

### Scenario
ShopWave needs to serve a React storefront and product images to customers in the US, EU, and APAC with < 50 ms latency and 99.99% availability.

### Architecture

```
Users (Global)
     │
 [Route 53] ← Simple routing (single CloudFront domain)
     │
 [AWS WAF] ← Web ACL: OWASP, bot control, rate limit 1000 req/5min per IP
     │
 [CloudFront Distribution]
 ├── Cache Behavior: /static/* → S3 origin (OAC)
 │     TTL: max-age=31536000 (1 year, versioned assets)
 │
 ├── Cache Behavior: /api/* → API Gateway origin
 │     TTL: 0 for POST/PUT; 30s for GET /products/*
 │     Forward: Authorization header (not cached if present)
 │
 └── Default Behavior: /* → S3 origin (index.html)
       Custom error: 403/404 → /index.html (200)
       TTL: Cache-Control: no-cache

[S3 Bucket] (private, OAC)          [API Gateway] → Lambda → DynamoDB
  ├── index.html
  ├── main.abc123.js
  └── products/images/*.jpg

[Lambda@Edge] (Viewer Request)
  └── Validate JWT in Authorization header → reject 401 without hitting origin
```

### Key Design Decisions
| Decision | Why |
|----------|-----|
| Versioned filenames for JS/CSS | No invalidations needed — new deploy = new URL |
| `no-cache` on index.html | Ensure users always get the latest app entry point |
| Lambda@Edge JWT validation | Reject unauthenticated requests at the edge — saves Lambda invocations |
| Cache GET /products/* 30 s | High-read product catalog; 30 s stale is acceptable |
| WAF rate limit per IP | Protect against scraping and DDoS at the edge |

### Interview Narration (Whiteboard Script)
> "I'd serve the React app and static assets from S3 via CloudFront with Origin Access Control — the bucket is private and only CloudFront can read it. I'd version all filenames (hash in the filename) so I can set a year-long max-age and never need invalidations. index.html gets no-cache so users always get the latest shell. For API calls, I'd point CloudFront at API Gateway and cache GET requests on product data for 30 seconds to absorb read traffic. JWT validation happens at the edge via Lambda@Edge so we reject unauthenticated requests before they touch the backend. WAF is attached to the distribution for OWASP rules and rate limiting."
