# Common Interview Questions & Answers

Practice answering these out loud before the interview.

---

## CI/CD Questions

### Q: What is CI/CD and why is it important?
**A:** CI is automatically building and testing code on every push to catch bugs early. CD is automatically deploying that code to staging or production. It's important because it reduces manual errors, gives fast feedback, and ensures code is always in a deployable state. In my project, I used GitHub Actions — every push triggers build, test, and Docker image creation.

### Q: Describe your CI/CD pipeline.
**A:** On every push or PR to main, GitHub Actions:
1. Checks out the code
2. Sets up .NET SDK
3. Restores NuGet packages (cached for speed)
4. Builds all microservices
5. Runs unit and integration tests
6. Builds Docker images using multi-stage builds
7. For the CD part, it deploys to staging automatically and requires manual approval for production.

### Q: What's the difference between Continuous Delivery and Continuous Deployment?
**A:** Continuous Delivery means code is always *ready* to deploy, but there's a manual approval gate before production. Continuous Deployment means every passing change goes straight to production automatically. Most enterprises use Continuous Delivery for the safety of manual approval.

### Q: How do you handle secrets in CI/CD?
**A:** GitHub Actions has encrypted secrets — you add them in repo settings, reference them as `${{ secrets.NAME }}` in workflows. They're never logged, even if you try to echo them. For more complex setups, tools like Azure Key Vault or AWS Secrets Manager are used.

---

## Microservices Questions

### Q: What are microservices? Why use them over a monolith?
**A:** Microservices is an architecture where the application is split into small, independently deployable services, each owning a specific business domain. Compared to a monolith:
- **Independent scaling** — scale only the services that need it
- **Independent deployment** — deploy one service without affecting others
- **Technology flexibility** — each service can use the best-fit tech
- **Team autonomy** — different teams own different services
- The trade-off is increased operational complexity — you need CI/CD, containers, service discovery, and distributed tracing.

### Q: How do microservices communicate?
**A:** Two main patterns:
- **Synchronous** (HTTP REST or gRPC) — for when you need an immediate response, like fetching product details. I used `IHttpClientFactory` in .NET Core.
- **Asynchronous** (message queues like RabbitMQ, Azure Service Bus) — for fire-and-forget scenarios like sending notifications after an order is placed.
I also used the API Gateway pattern where clients only talk to the gateway, which routes to the appropriate service.

### Q: What is the Circuit Breaker pattern?
**A:** It prevents cascading failures. If Service A calls Service B and B is failing, the circuit breaker "opens" after N failures and immediately returns an error without calling B. After a timeout, it enters "half-open" state and tries one request. If it succeeds, the circuit closes again. In .NET, I'd use the Polly library.

### Q: How do you handle distributed transactions?
**A:** Since each microservice has its own database, you can't use traditional ACID transactions. Instead, I'd use the **Saga pattern** — a sequence of local transactions where each service does its work and publishes an event. If one step fails, compensating transactions are triggered to undo previous steps. For example: CreateOrder → ReserveInventory → ProcessPayment. If payment fails → release inventory → cancel order.

### Q: What is an API Gateway?
**A:** A single entry point for all client requests. It handles cross-cutting concerns like authentication, rate limiting, logging, and request routing. It can also aggregate responses from multiple services into one response for the client. In .NET, tools like Ocelot or YARP can be used. In the cloud, Azure API Management or AWS API Gateway.

---

## .NET Core / Web API Questions

### Q: What is the difference between .NET Framework and .NET Core?
**A:** .NET Core (now just ".NET" from version 5+) is cross-platform (Windows, Linux, Mac), open-source, and has better performance. .NET Framework is Windows-only and in maintenance mode. New projects should always use .NET 6/7/8.

### Q: Explain the middleware pipeline in ASP.NET Core.
**A:** Requests flow through a series of middleware components in order. Each middleware can process the request, pass it to the next middleware, or short-circuit. The order matters:
1. Exception handling (first, so it catches everything)
2. HTTPS redirection
3. CORS
4. Authentication (who are you?)
5. Authorization (are you allowed?)
6. Routing / Controllers

### Q: What is `[ApiController]` attribute?
**A:** It enables API-specific behaviors:
- Automatic model validation (returns 400 if model state is invalid)
- Binding source inference (`[FromBody]`, `[FromRoute]` inferred automatically)
- Problem Details responses for errors (RFC 7807 standard format)

### Q: How do you handle errors in Web API?
**A:** Multiple levels:
- **Model validation** — Data Annotations + `[ApiController]` auto-returns 400
- **Exception middleware** — `app.UseExceptionHandler()` catches unhandled exceptions, returns 500
- **Problem Details** — Standardized error response format
- **Custom exception filters** — For specific error handling logic
- **Result pattern** — Return `ActionResult<T>` with appropriate status codes

### Q: Explain Dependency Injection lifetimes.
**A:** Three lifetimes in .NET Core:
- **Transient**: New instance every time it's injected. For lightweight, stateless services.
- **Scoped**: One instance per HTTP request. Used for DbContext.
- **Singleton**: One instance for the entire application lifetime. For caches, configuration.
Never inject a Scoped service into a Singleton — it creates a "captive dependency" bug.

---

## MVC Questions

### Q: Explain the MVC pattern.
**A:** Model-View-Controller separates concerns:
- **Model**: Data and business logic
- **View**: UI presentation (Razor .cshtml files)
- **Controller**: Handles requests, coordinates Model and View
A request hits the controller, which gets data via the model, passes it to the view, and the view renders HTML.

### Q: MVC vs Web API — when do you use which?
**A:** MVC is for server-rendered HTML pages with Razor views. Web API is for returning data (JSON/XML) to be consumed by SPAs, mobile apps, or other services. In a microservices architecture, I use Web API for the services and MVC for the gateway's admin dashboard or server-rendered pages. You can also combine both in one project.

---

## Entity Framework Core Questions

### Q: Code First vs Database First?
**A:** Code First — I define C# model classes, EF generates the database schema via migrations. Good for greenfield projects. Database First — I scaffold C# classes from an existing database. Good when the database already exists or is managed by a DBA team.

### Q: What are EF Core migrations?
**A:** Migrations are version control for the database schema. When I change my model classes, I create a migration (`dotnet ef migrations add AddPriceColumn`) which generates a C# file describing the schema change. Then `dotnet ef database update` applies it. In CI/CD, migrations can be applied automatically during deployment.

### Q: How do you improve EF Core performance?
**A:**
- Use `AsNoTracking()` for read-only queries
- Use `Select()` projections to fetch only needed columns
- Avoid N+1 with `Include()` for eager loading
- Use compiled queries for frequently-executed queries
- Index frequently-queried columns
- Use raw SQL for complex queries when LINQ generates inefficient SQL

---

## Angular Questions

### Q: How does Angular communicate with the backend?
**A:** Angular uses `HttpClient` module which returns RxJS Observables. I create services (`@Injectable`) that encapsulate API calls. Components subscribe to these observables to get data. For example, `this.productService.getAll().subscribe(products => this.products = products)`.

### Q: What are Observables vs Promises?
**A:** Observables (RxJS) can emit multiple values over time and are lazy (don't execute until subscribed). Promises are eager and resolve once. Observables can be cancelled, retried, and composed with operators like `map`, `filter`, `switchMap`. Angular's `HttpClient` returns Observables but they behave like Promises for single HTTP calls.

---

## Scenario-Based Questions

### Q: A service is running slow in production. How do you debug?
**A:**
1. Check health endpoints and application logs (structured logging with Serilog)
2. Look at distributed tracing (Jaeger/Application Insights) to find which service/call is slow
3. Check metrics — CPU, memory, response times, error rates
4. Check if it's a database issue — slow queries, missing indexes, connection pool exhaustion
5. Check if it's a downstream service issue (circuit breaker should help)
6. Reproduce locally with load testing tools (k6, JMeter)

### Q: How would you migrate a monolith to microservices?
**A:** Use the **Strangler Fig pattern**:
1. Identify bounded contexts in the monolith
2. Extract one service at a time, starting with the least coupled
3. Add an API gateway that routes some requests to the new service, rest to monolith
4. Gradually move functionality until the monolith is empty
5. At each step, the system is fully functional — no big bang migration

### Q: How do you ensure microservices are reliable?
**A:**
- **Health checks** — each service exposes `/health` endpoint
- **Circuit breakers** — prevent cascading failures (Polly)
- **Retries with exponential backoff** — handle transient failures
- **Distributed tracing** — trace requests across services
- **Centralized logging** — aggregate logs from all services (ELK, Application Insights)
- **Monitoring and alerting** — Prometheus + Grafana, or Azure Monitor
- **CI/CD with automated tests** — catch issues before production
