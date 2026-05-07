# Likely Interview Questions & Answers — Qentelli .NET + AWS

Read these out loud. Hearing yourself answer builds confidence.

---

## .NET / C# Questions

**Q: What is the difference between `IEnumerable`, `ICollection`, and `IList`?**
> `IEnumerable` — read-only forward iteration only, deferred execution (LINQ). `ICollection` — adds Count, Add, Remove. `IList` — adds indexed access. Use the most restrictive type in method signatures — if you only iterate, accept `IEnumerable`.

**Q: Explain dependency injection and why it matters.**
> DI is a design pattern where a class's dependencies are provided (injected) rather than created internally. In ASP.NET Core, the built-in IoC container resolves services registered in `Program.cs`. Benefits: loose coupling, easier unit testing (inject mocks), centralized lifetime management.

**Q: What's the difference between Scoped, Transient, and Singleton?**
> Singleton — one instance for the entire application lifetime. Scoped — one instance per HTTP request (dispose at end of request). Transient — new instance every time one is requested. Scoped is most common for services that hold request state like a DbContext.

**Q: How does `async/await` work under the hood?**
> `await` doesn't block the thread — it registers a continuation callback and returns the thread to the thread pool. When the awaited operation completes, a thread picks up execution from the continuation. This allows a server to handle thousands of concurrent requests without needing thousands of threads.

**Q: What is middleware in ASP.NET Core?**
> Middleware is software assembled into a pipeline to handle HTTP requests and responses. Each middleware component calls the next one or short-circuits the pipeline. Examples: authentication, logging, exception handling, routing. Order matters — auth must come before authorization.

**Q: How do you prevent N+1 queries in Entity Framework Core?**
> Use `.Include()` for eager loading related entities. For complex scenarios, use `.AsSplitQuery()` to split into multiple queries rather than one cartesian-product JOIN. Also use `AsNoTracking()` for read-only queries to improve performance.

**Q: What is CQRS and when would you use it?**
> CQRS separates reads (queries) from writes (commands). Queries return data without side effects; commands change state. Useful when read and write models differ significantly — e.g., the read side can be denormalized for speed. In .NET, commonly implemented with MediatR. Not needed for simple CRUD apps.

---

## AWS Questions

**Q: When would you use Lambda instead of EC2?**
> Lambda for short-lived, event-driven tasks (file processing, API calls, scheduled jobs) where you want zero server management and pay-per-use billing. EC2 when you need long-running processes, stateful workloads, full OS access, or execution longer than 15 minutes.

**Q: How do you connect a .NET application to AWS S3?**
> Install `AWSSDK.S3` NuGet package. Register `IAmazonS3` in DI using `builder.Services.AddAWSService<IAmazonS3>()`. The SDK picks up credentials from the IAM Role attached to the EC2 or Lambda — no hardcoded keys. Then use `PutObjectAsync` to upload, `GetObjectAsync` to download, or generate pre-signed URLs for temporary access.

**Q: What is a pre-signed URL in S3?**
> A time-limited URL that grants access to a private S3 object without requiring AWS credentials. You generate it server-side with an expiry (e.g., 1 hour), then share it with the client. Useful for allowing users to download private files or upload directly to S3 without going through your server.

**Q: How do you handle secrets/credentials in AWS?**
> Never hardcode them. Use IAM Roles for service-to-service auth (EC2/Lambda assume a role with only the permissions they need). For database passwords and API keys, use AWS Secrets Manager — the app fetches the secret at startup and can auto-rotate without redeployment.

**Q: What is the difference between SNS and SQS?**
> SNS is pub/sub: a message is pushed to all subscribers immediately (fan-out). SQS is a queue: consumers poll for messages, which are retained up to 14 days. Often combined: SNS publishes to multiple SQS queues, giving both fan-out and resilient buffering.

**Q: How do you make an application highly available on AWS?**
> Deploy across multiple Availability Zones. Use an Application Load Balancer with an Auto Scaling Group of EC2s. Use RDS Multi-AZ for database failover. Use ElastiCache for session/cache so any instance can serve any user. Use S3 for shared file storage (inherently highly available).

---

## Angular Questions

**Q: What is the difference between `Subject`, `BehaviorSubject`, and `ReplaySubject`?**
> `Subject` — no initial value, only emits to current subscribers. `BehaviorSubject` — requires an initial value, replays the last emitted value to new subscribers (great for state). `ReplaySubject(n)` — replays the last n values to new subscribers.

**Q: How do you prevent memory leaks in Angular?**
> Always unsubscribe from Observables in `ngOnDestroy`. Common patterns: use the `async` pipe (auto-unsubscribes), or the `takeUntil` pattern with a Subject that emits in `ngOnDestroy`. Avoid storing subscriptions manually unless using `Subscription.unsubscribe()`.

**Q: What is the difference between `switchMap`, `mergeMap`, and `concatMap`?**
> `switchMap` — cancels the previous inner observable when a new value arrives (use for search/autocomplete). `mergeMap` — runs all inner observables concurrently (use for parallel HTTP calls). `concatMap` — runs inner observables sequentially, waits for each to complete (use when order matters).

**Q: What is Angular's change detection and how does OnPush work?**
> By default, Angular checks every component on every event. `OnPush` restricts checks to: when an @Input reference changes, an async pipe emits, or you manually call `markForCheck()`. Makes components much more performant for large lists or complex trees.

---

## Cross-Cutting / Behavioral Questions

**Q: Tell me about a performance optimization you made.**
> STAR: Situation — API endpoint was taking 3+ seconds. Task — reduce to under 500ms. Action — used SQL profiler to find N+1 queries, added `.Include()` and an index on the FK column, added response caching with `IMemoryCache` for static data. Result — response time dropped to 120ms.

**Q: How do you approach debugging a production issue?**
> First reproduce it — check CloudWatch Logs or Application Insights for the exact error and stack trace. Correlate with recent deployments. If it's intermittent, add more structured logging around the suspect area. Use feature flags to roll back if needed. Fix in a branch, test, deploy to staging first.

**Q: How do you ensure code quality in a team?**
> Peer code reviews on all PRs, coding standards enforced via `.editorconfig` and Roslyn analyzers, unit tests required for business logic, CI pipeline that blocks merge on failing tests or coverage drops. Regular tech debt reviews.

**Q: Why Qentelli? Why this role?**
> Prepare a genuine 2-3 sentence answer: mention cloud-native .NET development interest, the opportunity to work on scalable AWS architecture, and what draws you to the company (consulting breadth, innovative projects, Hyderabad team).

---

## Questions to Ask Them
- What does the day-to-day development workflow look like for this team?
- What AWS services does the team use most heavily right now?
- How large is the engineering team and how are squads structured?
- What does the onboarding process look like for new developers?
- What does success look like in the first 90 days?
