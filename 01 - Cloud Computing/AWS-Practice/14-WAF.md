# AWS WAF — Web Application Firewall

## 1. Core Concepts

### What is AWS WAF?
AWS WAF is a **managed Web Application Firewall** that filters and monitors HTTP/HTTPS requests before they reach your application. It operates at Layer 7 (application layer) and protects against common web exploits like SQL injection, XSS, and bot attacks.

### Deployment Targets
AWS WAF can be attached to:
| Resource | Scope |
|----------|-------|
| **CloudFront** | Global (WAF Web ACL must be in `us-east-1`) |
| **ALB** | Regional (WAF Web ACL must be in same region) |
| **API Gateway** (REST) | Regional |
| **AppSync** | Regional |
| **Cognito User Pools** | Regional |
| **App Runner** | Regional |

### Core Components
```
Web ACL
  └── Rules (evaluated in priority order)
        ├── Rule Group (reusable set of rules)
        │     ├── AWS Managed Rule Group
        │     └── Custom Rule Group
        └── Individual Rule
              ├── Statement (match condition)
              └── Action (Allow / Block / Count / CAPTCHA / Challenge)
```

### Rule Statements (Match Conditions)
| Statement | Matches On |
|-----------|-----------|
| **IP Set** | Source IP or IP range (IPv4/IPv6) |
| **Geo Match** | Country of origin |
| **String Match** | URI, headers, body, query string — exact, prefix, suffix, regex |
| **Regex Pattern Set** | Pattern library applied to request components |
| **Size Constraint** | Request body/header/URI size |
| **SQL Injection Match** | SQLi patterns in any request component |
| **XSS Match** | Cross-site scripting patterns |
| **Rate-Based Rule** | Requests per IP per 5-min window |
| **Label Match** | Labels set by other rules (for chaining logic) |

### Rule Actions
| Action | Effect |
|--------|--------|
| **Allow** | Forward request; optionally insert headers |
| **Block** | Return HTTP 403 (or custom response) |
| **Count** | Log the match without blocking; useful for testing |
| **CAPTCHA** | Present CAPTCHA challenge; block bots |
| **Challenge** | Transparent browser challenge (JavaScript puzzle); no user interaction |

### AWS Managed Rule Groups
| Group | Protects Against |
|-------|-----------------|
| **AWSManagedRulesCommonRuleSet** | OWASP Top 10 (SQLi, XSS, path traversal, etc.) |
| **AWSManagedRulesKnownBadInputsRuleSet** | Log4Shell, Spring4Shell, SSRF |
| **AWSManagedRulesAmazonIpReputationList** | Known malicious IPs, botnets |
| **AWSManagedRulesAnonymousIpList** | Tor exit nodes, hosting providers used for anonymization |
| **AWSManagedRulesBotControlRuleSet** | Common bots, scrapers (Targeted tier adds ML-based detection) |
| **AWSManagedRulesATPRuleSet** | Account Takeover Prevention (credential stuffing) |
| **AWSManagedRulesACFPRuleSet** | Account Creation Fraud Prevention |
| **AWSManagedRulesSQLiRuleSet** | Targeted SQL injection patterns |

### Rate-Based Rules
Rate-based rules automatically count requests from an IP over a **rolling 5-minute window** and block when the threshold is exceeded. Aggregation keys can be:
- **IP address** (default)
- **IP + URI** — rate limit per URL per IP
- **Forwarded IP** — use `X-Forwarded-For` header (for proxied traffic)
- **Custom aggregation key** — combination of headers, query params, labels

### WCUs (WAF Capacity Units)
Each rule consumes capacity units. A Web ACL has a maximum of **5,000 WCUs**. Managed rule groups consume a fixed WCU amount (e.g., AWSManagedRulesCommonRuleSet = 700 WCUs).

### Logging & Monitoring
- Enable **WAF logging** to S3, CloudWatch Logs, or Kinesis Data Firehose
- Logs include: timestamp, client IP, URI, action taken, matched rule, labels
- Use **CloudWatch metrics** per Web ACL, rule group, and individual rule (`AllowedRequests`, `BlockedRequests`, `CountedRequests`, `PassedRequests`)
- **AWS WAF dashboards** in the console provide real-time traffic breakdown

### Pricing
- **Web ACL**: $5/month per ACL
- **Rule**: $1/month per rule
- **Requests**: $0.60 per million requests (CloudFront) or $0.60/million (regional)
- **Bot Control**: additional $10/month + $1/million requests
- **Intelligent Threat Mitigation (ATP, ACFP)**: additional $10/month per rule group

---

## 2. Interview Questions & Answers

### Q1. What is AWS WAF and what does it protect against?
AWS WAF is a Layer 7 (HTTP) firewall that filters web requests before they reach your application. It protects against:
- **SQL injection** and **XSS** (via managed and custom rules)
- **OWASP Top 10** vulnerabilities
- **Bot traffic** (scrapers, credential stuffing, DDoS HTTP floods)
- **Known bad IPs** (threat intelligence feeds)
- **Rate-based attacks** (brute force, DoS via volume)
It doesn't protect against Layer 3/4 DDoS (that's AWS Shield).

---

### Q2. What is the difference between AWS WAF and AWS Shield?
| | AWS WAF | AWS Shield |
|--|---------|-----------|
| Layer | 7 (HTTP/HTTPS) | 3/4 (network/transport) |
| Protects against | SQLi, XSS, bots, HTTP floods | SYN floods, UDP reflection, volumetric DDoS |
| Standard | No (add-on) | Yes — free for all AWS customers |
| Advanced | Paid ($5/mo + rules) | $3,000/month + 1-yr commit; includes WAF + DDoS response team |
| Managed rules | Yes (AWS Managed Rule Groups) | N/A |

WAF and Shield Advanced are complementary — you typically use both for full coverage.

---

### Q3. How do you test WAF rules before enforcing them?
1. Set rule action to **Count** instead of **Block** — requests are logged but not blocked
2. Monitor CloudWatch metrics (`CountedRequests`) and WAF logs to see what would have been blocked
3. Review the logs to identify false positives
4. Switch action from Count to Block only after confirming accuracy
5. For new AWS Managed Rule Groups, use **override action: Count** at the rule group level first

---

### Q4. What is a rate-based rule and how does it work?
A rate-based rule counts requests from a specific aggregation key (typically the client IP) over a **rolling 5-minute window**. If the count exceeds the threshold (minimum: 100), WAF automatically blocks the IP for the remainder of the window.  
The block expires as the window slides. Rate-based rules are effective against:
- Brute-force login attempts
- Credential stuffing
- HTTP DDoS floods targeting specific endpoints

---

### Q5. How would you prevent SQL injection attacks using AWS WAF?
1. Attach **AWSManagedRulesCommonRuleSet** (includes SQLi rules) to the Web ACL
2. Add **AWSManagedRulesSQLiRuleSet** for targeted SQL injection detection
3. Inspect all request components: URI, query string, body, and headers (especially `Content-Type: application/x-www-form-urlencoded`)
4. Use **URL decode** and **HTML entity decode** transformations before matching — attackers often encode payloads to evade detection
5. Monitor false positives with Count mode before blocking

---

### Q6. What are WAF labels and how are they used?
**Labels** are metadata tags that rules can add to a request (e.g., `awswaf:managed:aws:core-rule-set:CrossSiteScripting_Body`). Subsequent rules can match on these labels to implement **complex chaining logic** — for example, block a request only if it matches both an XSS label AND comes from a specific country.  
Labels enable AND/OR logic across rules without writing a single complex regex.

---

### Q7. How does WAF Bot Control work?
**Bot Control** is an AWS Managed Rule Group that uses **fingerprinting and ML** to categorize traffic as:
- **Common bots**: search engine crawlers (Google, Bing), monitoring services — by default allowed but labeled
- **Targeted bots**: headless browsers, scraping tools — Targeted tier uses ML-based behavioral analysis

You can allow legitimate bots (e.g., Googlebot) while blocking malicious ones. Bot Control uses **Challenge** and **CAPTCHA** actions to transparently verify browsers.

---

### Q8. How do you protect an ALB-backed application with WAF?
1. Create a WAF Web ACL in the same region as the ALB
2. Associate the Web ACL with the ALB
3. Add rules:
   - AWS Managed Common Rule Set
   - Rate-based rule (e.g., 500 req/5 min per IP on `/login`)
   - IP allow/block list as needed
4. Enable WAF logging to CloudWatch Logs or S3
5. Monitor via CloudWatch metrics (`BlockedRequests`, `AllowedRequests`)

---

### Q9. What is the difference between WAF on CloudFront vs WAF on ALB?
| | WAF on CloudFront | WAF on ALB |
|--|-------------------|-----------|
| Scope | Global (all edge PoPs) | Regional |
| Web ACL location | Must be `us-east-1` | Same region as ALB |
| Blocks at | Edge (~400 locations) | Region (reaches your VPC) |
| Best for | Static content, global apps | Regional APIs, internal apps |

Blocking at the edge (CloudFront) is preferred because malicious requests never reach your origin infrastructure.

---

### Q10. How do you block traffic from specific countries?
Create a **Geo Match Statement** in a WAF rule:
```
Statement: GeoMatchStatement → CountryCodes: [CN, RU, ...]
Action: Block
```
Note: geo-blocking is based on IP geolocation databases and may have ~1% inaccuracy. VPN/proxy users may bypass it. For stricter enforcement, combine with the **Anonymous IP List** managed rule group.

---

### Q11. Can AWS WAF be bypassed? How do you mitigate bypass attempts?
Common bypass techniques:
- **Encoding**: URL encoding, double encoding, Unicode tricks → use WAF **text transformation** rules (URL decode, HTML decode, lowercase)
- **Fragmented payloads**: Splitting attack across multiple requests → WAF inspects per-request; multi-request attacks require application-layer defenses
- **VPN/Tor**: Geo-blocks bypassed → add **Anonymous IP List** managed rule group
- **Low-and-slow attacks**: Distributed below rate limits → combine WAF with Shield Advanced and behavior-based anomaly detection

---

### Q12. How do you use WAF to protect an API endpoint against credential stuffing?
1. Add **AWSManagedRulesATPRuleSet** (Account Takeover Prevention) to your Web ACL
2. Configure the login endpoint path (e.g., `/api/login`)
3. ATP inspects POST body for credential patterns and tracks failed login rates per IP
4. Enable **CAPTCHA/Challenge** for suspicious IPs
5. Combine with a **rate-based rule** on the login endpoint (e.g., block after 10 req/5 min per IP)

---

### Q13. What is WAF's request inspection limit?
By default, WAF inspects only the **first 8 KB** of the request body. For larger bodies (e.g., JSON APIs), you can increase the inspection limit to 16 KB, 32 KB, or 48 KB (at higher WCU cost). Beyond the limit, WAF applies the **oversized body handling** action (continue inspection or block).

---

### Q14. How do you handle WAF false positives in production?
1. Use **Count** mode for new rules — never roll them out as Block immediately
2. Review WAF logs filtered to `Action: COUNT` to identify legitimate traffic being flagged
3. Add **IP Set exclusions** for known good IPs (internal scanners, monitoring tools)
4. Add **rule-level overrides** for specific AWS Managed Rules that cause false positives — keep the rule group in Block mode but override individual rules to Count
5. Create custom rules with narrower scope (specific URI + method combination) instead of broad body inspection

---

### Q15. How would you design WAF protection for a multi-tier e-commerce application?
```
Architecture:
Users → Route 53 → CloudFront + WAF (Global Web ACL)
                 → API Gateway + WAF (Regional Web ACL)
                 → ALB + WAF (Regional Web ACL)

Global Web ACL (CloudFront):
- AWSManagedRulesCommonRuleSet (Block)
- AWSManagedRulesAmazonIpReputationList (Block)
- AWSManagedRulesBotControlRuleSet (Count then Block)
- Rate: 1,000 req/5 min per IP → Block

Regional Web ACL (API Gateway):
- AWSManagedRulesCommonRuleSet (Block)
- AWSManagedRulesATPRuleSet on /auth/login (Block)
- Rate: 50 req/5 min per IP on /checkout → Block
- Rate: 200 req/5 min per IP on /products → Allow
```

---

## 3. Real-World Use Case: E-Commerce Platform WAF Protection

### Scenario
ShopWave processes 50,000 orders/day and has experienced:
- SQLi attempts on the product search API
- Credential stuffing on the login endpoint (1,000 attempts/hour from distributed IPs)
- Price-scraping bots hitting the product catalog 10× normal rate

### Architecture

```
[Users & Bots] → [Route 53]
                      │
          ┌───────────▼────────────┐
          │  CloudFront + WAF      │  ← Global Web ACL (us-east-1)
          │  Rules:                │    • Common Rule Set (Block)
          │  • IP Reputation       │    • IP Reputation List (Block)
          │  • Bot Control         │    • Bot Control (Challenge/Block)
          │  • Rate: 500/5min      │    • Rate limit: 500 req/5min/IP
          └───────────┬────────────┘
                      │
          ┌───────────▼────────────┐
          │  API Gateway + WAF     │  ← Regional Web ACL
          │  Rules:                │    • Common Rule Set
          │  • ATP on /auth/login  │    • ATP (login endpoint)
          │  • Rate: 20/5min/IP    │    • Rate limit: 20/5min/IP on /auth
          │    on /auth/login      │
          └───────────┬────────────┘
                      │
          [Lambda] → [DynamoDB / RDS]
```

### Key Design Decisions
| Decision | Why |
|----------|-----|
| WAF at CloudFront | Blocks bots before they reach origin — reduces origin load and cost |
| Bot Control on scraping | Challenge action fingerprints headless browsers; legitimate users unaffected |
| ATP on /auth/login | Tracks per-IP failed logins; blocks credential stuffing without a custom rule |
| Count before Block | All new rule groups ran in Count mode for 1 week before switching to Block |
| Geo-block high-fraud countries | Combined with Anonymous IP list to reduce fraud without false positives |

### Interview Narration (Whiteboard Script)
> "I'd layer WAF at two points — on CloudFront globally and on API Gateway regionally. The global WAF handles OWASP rules and bot detection at the edge so malicious traffic never reaches our infrastructure. The regional WAF adds ATP on the login endpoint to stop credential stuffing. I'd run everything in Count mode first, review logs for false positives, then switch to Block. Rate-based rules handle DDoS HTTP floods at both layers. This defense-in-depth approach means an attacker would need to evade multiple independent rule sets."
