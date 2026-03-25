# Hands-On Step-by-Step Guide

Follow each step exactly. Copy-paste the commands. By the end, you'll have a working microservices app with CI/CD.

---

## Prerequisites Check

Run these first to make sure you have everything:

```bash
dotnet --version        # Need 8.0+
git --version           # Need git
docker --version        # Need Docker Desktop
node --version          # Need 18+ (for Angular)
ng version              # Need Angular CLI (install: npm install -g @angular/cli)
gh --version            # GitHub CLI (optional but helpful)
```

If missing Angular CLI:
```bash
npm install -g @angular/cli
```

---

## PHASE 1: Project Setup (30 min)

### Step 1.1 — Create GitHub Repo

Go to GitHub → New Repository → Name: `dotnet-microservices-cicd-demo` → Public → Create

Then clone it:
```bash
cd ~/Documents/Dev
git clone https://github.com/YOUR_USERNAME/dotnet-microservices-cicd-demo.git
cd dotnet-microservices-cicd-demo
```

### Step 1.2 — Create Solution Structure

```bash
# Create the solution file
dotnet new sln -n MicroservicesDemo

# Create source and test directories
mkdir -p src tests

# Create the three microservices
dotnet new webapi -n ProductService -o src/ProductService --use-controllers
dotnet new webapi -n OrderService -o src/OrderService --use-controllers
dotnet new mvc -n ApiGateway -o src/ApiGateway

# Create test projects
dotnet new xunit -n ProductService.Tests -o tests/ProductService.Tests
dotnet new xunit -n OrderService.Tests -o tests/OrderService.Tests

# Add all projects to the solution
dotnet sln add src/ProductService/ProductService.csproj
dotnet sln add src/OrderService/OrderService.csproj
dotnet sln add src/ApiGateway/ApiGateway.csproj
dotnet sln add tests/ProductService.Tests/ProductService.Tests.csproj
dotnet sln add tests/OrderService.Tests/OrderService.Tests.csproj

# Add test project references
dotnet add tests/ProductService.Tests reference src/ProductService
dotnet add tests/OrderService.Tests reference src/OrderService

# Add EF Core InMemory to each service
dotnet add src/ProductService package Microsoft.EntityFrameworkCore.InMemory
dotnet add src/OrderService package Microsoft.EntityFrameworkCore.InMemory

# Add HttpClient support to gateway
dotnet add src/ApiGateway package Microsoft.Extensions.Http

# Verify everything builds
dotnet build
```

### Step 1.3 — Initial Commit

```bash
git add -A
git commit -m "Initial solution structure with 3 services and test projects"
git push origin main
```

---

## PHASE 2: Build Microservices (90 min)

### Step 2.1 — ProductService

#### Create the Model
Create file: `src/ProductService/Models/Product.cs`
```csharp
namespace ProductService.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### Create the DbContext
Create file: `src/ProductService/Data/ProductDbContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed data
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Description = "Gaming Laptop", Price = 999.99m, Category = "Electronics", StockQuantity = 50 },
            new Product { Id = 2, Name = "Mouse", Description = "Wireless Mouse", Price = 29.99m, Category = "Electronics", StockQuantity = 200 },
            new Product { Id = 3, Name = "Keyboard", Description = "Mechanical Keyboard", Price = 79.99m, Category = "Electronics", StockQuantity = 150 }
        );
    }
}
```

#### Replace the Controller
Replace file: `src/ProductService/Controllers/ProductsController.cs`
(Delete the auto-generated WeatherForecastController and WeatherForecast model files first)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        _logger.LogInformation("Getting all products");
        return await _context.Products.ToListAsync();
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
            return NotFound();
        return product;
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT: api/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Product product)
    {
        if (id != product.Id)
            return BadRequest();

        _context.Entry(product).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Products.AnyAsync(p => p.Id == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    // DELETE: api/products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
            return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
```

#### Update Program.cs
Replace `src/ProductService/Program.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using ProductService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add EF Core with InMemory database
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseInMemoryDatabase("ProductDb"));

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.EnsureCreated();
}

// Configure pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
```

#### Set the port
Add to `src/ProductService/Properties/launchSettings.json` — change the `applicationUrl` under the `http` profile to:
```
"applicationUrl": "http://localhost:5001"
```

#### Clean up generated files
```bash
rm src/ProductService/Controllers/WeatherForecastController.cs
rm src/ProductService/WeatherForecast.cs
```

---

### Step 2.2 — OrderService

#### Create Models
Create file: `src/OrderService/Models/Order.cs`
```csharp
namespace OrderService.Models;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
}
```

#### Create the DbContext
Create file: `src/OrderService/Data/OrderDbContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().HasData(
            new Order { Id = 1, CustomerName = "John Doe", CustomerEmail = "john@example.com", ProductId = 1, ProductName = "Laptop", Quantity = 1, TotalPrice = 999.99m, Status = "Completed" },
            new Order { Id = 2, CustomerName = "Jane Smith", CustomerEmail = "jane@example.com", ProductId = 2, ProductName = "Mouse", Quantity = 3, TotalPrice = 89.97m, Status = "Pending" }
        );
    }
}
```

#### Replace the Controller
Create file: `src/OrderService/Controllers/OrdersController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(OrderDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll()
    {
        _logger.LogInformation("Getting all orders");
        return await _context.Orders.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetById(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return NotFound();
        return order;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return NotFound();

        order.Status = status;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return NotFound();

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
```

#### Update Program.cs
Replace `src/OrderService/Program.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using OrderService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrderDb"));

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
```

#### Set the port and clean up
Set port to `http://localhost:5002` in `launchSettings.json`.
```bash
rm src/OrderService/Controllers/WeatherForecastController.cs
rm src/OrderService/WeatherForecast.cs
```

---

### Step 2.3 — API Gateway (MVC + Web API)

This is the key piece — it demonstrates both MVC (views) and Web API (JSON endpoints), and inter-service communication.

#### Create Gateway Models
Create file: `src/ApiGateway/Models/Product.cs`
```csharp
namespace ApiGateway.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}
```

Create file: `src/ApiGateway/Models/Order.cs`
```csharp
namespace ApiGateway.Models;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}
```

Create file: `src/ApiGateway/Models/DashboardViewModel.cs`
```csharp
namespace ApiGateway.Models;

public class DashboardViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}
```

#### Create Service Clients
Create file: `src/ApiGateway/Services/ProductServiceClient.cs`
```csharp
using ApiGateway.Models;

namespace ApiGateway.Services;

public class ProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductServiceClient> _logger;

    public ProductServiceClient(HttpClient httpClient, ILogger<ProductServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Product>>("api/products");
            return response ?? new List<Product>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products from ProductService");
            return new List<Product>();
        }
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Product>($"api/products/{id}");
    }
}
```

Create file: `src/ApiGateway/Services/OrderServiceClient.cs`
```csharp
using ApiGateway.Models;

namespace ApiGateway.Services;

public class OrderServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderServiceClient> _logger;

    public OrderServiceClient(HttpClient httpClient, ILogger<OrderServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Order>>("api/orders");
            return response ?? new List<Order>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders from OrderService");
            return new List<Order>();
        }
    }
}
```

#### Create API Controller (Web API — returns JSON)
Create file: `src/ApiGateway/Controllers/Api/GatewayApiController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using ApiGateway.Services;

namespace ApiGateway.Controllers.Api;

[ApiController]
[Route("api/gateway")]
public class GatewayApiController : ControllerBase
{
    private readonly ProductServiceClient _productClient;
    private readonly OrderServiceClient _orderClient;

    public GatewayApiController(ProductServiceClient productClient, OrderServiceClient orderClient)
    {
        _productClient = productClient;
        _orderClient = orderClient;
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productClient.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _orderClient.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardData()
    {
        // Aggregate data from multiple services — this is the Gateway pattern
        var productsTask = _productClient.GetAllAsync();
        var ordersTask = _orderClient.GetAllAsync();
        await Task.WhenAll(productsTask, ordersTask);

        return Ok(new
        {
            TotalProducts = productsTask.Result.Count,
            TotalOrders = ordersTask.Result.Count,
            TotalRevenue = ordersTask.Result.Sum(o => o.TotalPrice),
            Products = productsTask.Result,
            Orders = ordersTask.Result
        });
    }
}
```

#### Create MVC Controller (returns Views — HTML)
Replace `src/ApiGateway/Controllers/HomeController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;
using ApiGateway.Models;
using ApiGateway.Services;

namespace ApiGateway.Controllers;

public class HomeController : Controller
{
    private readonly ProductServiceClient _productClient;
    private readonly OrderServiceClient _orderClient;

    public HomeController(ProductServiceClient productClient, OrderServiceClient orderClient)
    {
        _productClient = productClient;
        _orderClient = orderClient;
    }

    public async Task<IActionResult> Index()
    {
        var productsTask = _productClient.GetAllAsync();
        var ordersTask = _orderClient.GetAllAsync();
        await Task.WhenAll(productsTask, ordersTask);

        var model = new DashboardViewModel
        {
            Products = productsTask.Result,
            Orders = ordersTask.Result,
            TotalProducts = productsTask.Result.Count,
            TotalOrders = ordersTask.Result.Count,
            TotalRevenue = ordersTask.Result.Sum(o => o.TotalPrice)
        };

        return View(model);
    }
}
```

#### Create the Dashboard View
Replace `src/ApiGateway/Views/Home/Index.cshtml`:
```html
@model ApiGateway.Models.DashboardViewModel
@{
    ViewData["Title"] = "Microservices Dashboard";
}

<div class="container mt-4">
    <h1>Microservices Dashboard</h1>
    <p class="text-muted">API Gateway aggregating data from ProductService and OrderService</p>

    <div class="row mb-4">
        <div class="col-md-4">
            <div class="card text-white bg-primary">
                <div class="card-body">
                    <h5 class="card-title">Total Products</h5>
                    <h2>@Model.TotalProducts</h2>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card text-white bg-success">
                <div class="card-body">
                    <h5 class="card-title">Total Orders</h5>
                    <h2>@Model.TotalOrders</h2>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card text-white bg-info">
                <div class="card-body">
                    <h5 class="card-title">Total Revenue</h5>
                    <h2>$@Model.TotalRevenue.ToString("N2")</h2>
                </div>
            </div>
        </div>
    </div>

    <div class="row">
        <div class="col-md-6">
            <h3>Products <small class="text-muted">(from ProductService)</small></h3>
            <table class="table table-striped">
                <thead><tr><th>Name</th><th>Price</th><th>Stock</th></tr></thead>
                <tbody>
                @foreach (var p in Model.Products)
                {
                    <tr><td>@p.Name</td><td>$@p.Price</td><td>@p.StockQuantity</td></tr>
                }
                </tbody>
            </table>
        </div>
        <div class="col-md-6">
            <h3>Orders <small class="text-muted">(from OrderService)</small></h3>
            <table class="table table-striped">
                <thead><tr><th>Customer</th><th>Product</th><th>Total</th><th>Status</th></tr></thead>
                <tbody>
                @foreach (var o in Model.Orders)
                {
                    <tr><td>@o.CustomerName</td><td>@o.ProductName</td><td>$@o.TotalPrice</td><td>@o.Status</td></tr>
                }
                </tbody>
            </table>
        </div>
    </div>
</div>
```

#### Update Gateway Program.cs
Replace `src/ApiGateway/Program.cs`:
```csharp
using ApiGateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC (Views + API controllers)
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register typed HttpClients for each microservice
builder.Services.AddHttpClient<ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductService"]
        ?? "http://localhost:5001");
});

builder.Services.AddHttpClient<OrderServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:OrderService"]
        ?? "http://localhost:5002");
});

// CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHealthChecks("/health");

app.Run();
```

#### Add service URLs to appsettings
Add to `src/ApiGateway/appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ServiceUrls": {
    "ProductService": "http://localhost:5001",
    "OrderService": "http://localhost:5002"
  }
}
```

Set Gateway port to `http://localhost:5000` in `launchSettings.json`.

### Step 2.4 — Verify Everything Works

Open 3 terminals:
```bash
# Terminal 1
cd src/ProductService && dotnet run

# Terminal 2
cd src/OrderService && dotnet run

# Terminal 3
cd src/ApiGateway && dotnet run
```

Test these URLs:
- http://localhost:5001/swagger — ProductService API docs
- http://localhost:5002/swagger — OrderService API docs
- http://localhost:5000 — Gateway Dashboard (MVC view)
- http://localhost:5000/api/gateway/products — Gateway API (JSON)
- http://localhost:5000/api/gateway/dashboard — Aggregated data
- http://localhost:5001/health — Health check

### Step 2.5 — Commit

```bash
git add -A
git commit -m "Add ProductService, OrderService, and API Gateway with MVC dashboard"
git push origin main
```

---

## PHASE 3: Angular Frontend (30 min)

### Step 3.1 — Scaffold Angular App

```bash
cd src
ng new angular-frontend --routing --style=css --skip-tests --standalone
cd angular-frontend
```

### Step 3.2 — Create Product Model

Create file: `src/angular-frontend/src/app/models/product.model.ts`
```typescript
export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  category: string;
  stockQuantity: number;
}
```

Create file: `src/angular-frontend/src/app/models/order.model.ts`
```typescript
export interface Order {
  id: number;
  customerName: string;
  customerEmail: string;
  productId: number;
  productName: string;
  quantity: number;
  totalPrice: number;
  status: string;
  orderDate: string;
}
```

### Step 3.3 — Create Services

Create file: `src/angular-frontend/src/app/services/api.service.ts`
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from '../models/product.model';
import { Order } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = 'http://localhost:5000/api/gateway';

  constructor(private http: HttpClient) {}

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/products`);
  }

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.baseUrl}/orders`);
  }

  getDashboard(): Observable<any> {
    return this.http.get(`${this.baseUrl}/dashboard`);
  }
}
```

### Step 3.4 — Create Components

```bash
ng generate component components/product-list --standalone
ng generate component components/order-list --standalone
ng generate component components/dashboard --standalone
```

Replace `src/angular-frontend/src/app/components/product-list/product-list.component.ts`:
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { Product } from '../../models/product.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <h2>Products</h2>
      <table class="table">
        <thead>
          <tr><th>Name</th><th>Description</th><th>Price</th><th>Category</th><th>Stock</th></tr>
        </thead>
        <tbody>
          <tr *ngFor="let p of products">
            <td>{{ p.name }}</td>
            <td>{{ p.description }}</td>
            <td>{{ p.price | currency }}</td>
            <td>{{ p.category }}</td>
            <td>{{ p.stockQuantity }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  `
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.api.getProducts().subscribe(data => this.products = data);
  }
}
```

Replace `src/angular-frontend/src/app/components/order-list/order-list.component.ts`:
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { Order } from '../../models/order.model';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <h2>Orders</h2>
      <table class="table">
        <thead>
          <tr><th>Customer</th><th>Product</th><th>Qty</th><th>Total</th><th>Status</th></tr>
        </thead>
        <tbody>
          <tr *ngFor="let o of orders">
            <td>{{ o.customerName }}</td>
            <td>{{ o.productName }}</td>
            <td>{{ o.quantity }}</td>
            <td>{{ o.totalPrice | currency }}</td>
            <td><span [class]="'badge ' + (o.status === 'Completed' ? 'bg-success' : 'bg-warning')">{{ o.status }}</span></td>
          </tr>
        </tbody>
      </table>
    </div>
  `
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.api.getOrders().subscribe(data => this.orders = data);
  }
}
```

### Step 3.5 — Setup Routing

Replace `src/angular-frontend/src/app/app.routes.ts`:
```typescript
import { Routes } from '@angular/router';
import { ProductListComponent } from './components/product-list/product-list.component';
import { OrderListComponent } from './components/order-list/order-list.component';

export const routes: Routes = [
  { path: '', redirectTo: '/products', pathMatch: 'full' },
  { path: 'products', component: ProductListComponent },
  { path: 'orders', component: OrderListComponent }
];
```

Update `src/angular-frontend/src/app/app.component.ts`:
```typescript
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule],
  template: `
    <nav class="navbar navbar-expand navbar-dark bg-dark">
      <div class="container">
        <a class="navbar-brand" routerLink="/">Microservices Demo</a>
        <ul class="navbar-nav">
          <li class="nav-item"><a class="nav-link" routerLink="/products">Products</a></li>
          <li class="nav-item"><a class="nav-link" routerLink="/orders">Orders</a></li>
        </ul>
      </div>
    </nav>
    <router-outlet></router-outlet>
  `
})
export class AppComponent {}
```

Update `src/angular-frontend/src/app/app.config.ts`:
```typescript
import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient()
  ]
};
```

### Step 3.6 — Add Bootstrap CSS (quick styling)

Add this line inside `<head>` in `src/angular-frontend/src/index.html`:
```html
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
```

### Step 3.7 — Test Angular

```bash
cd src/angular-frontend
ng serve
```
Open http://localhost:4200 — should show products and orders from the gateway.

### Step 3.8 — Commit

```bash
git add -A
git commit -m "Add Angular frontend with product and order views"
git push origin main
```

---

## PHASE 4: CI/CD with GitHub Actions (60 min)

### Step 4.1 — Create CI Workflow

Create file: `.github/workflows/ci.yml`
```yaml
name: CI Pipeline

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: nuget-

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Run tests
        run: dotnet test --no-build --configuration Release --verbosity normal --logger "trx;LogFileName=test-results.trx"

      - name: Upload test results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: test-results
          path: '**/test-results.trx'

  build-angular:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: src/angular-frontend/package-lock.json

      - name: Install dependencies
        run: npm ci
        working-directory: src/angular-frontend

      - name: Build Angular
        run: npx ng build --configuration production
        working-directory: src/angular-frontend

      - name: Upload build artifact
        uses: actions/upload-artifact@v4
        with:
          name: angular-dist
          path: src/angular-frontend/dist/
```

### Step 4.2 — Create CD Workflow

Create file: `.github/workflows/cd.yml`
```yaml
name: CD Pipeline

on:
  workflow_run:
    workflows: ["CI Pipeline"]
    branches: [main]
    types: [completed]

jobs:
  docker-build:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}

    strategy:
      matrix:
        service: [ProductService, OrderService, ApiGateway]

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Build Docker image
        run: |
          docker build -t ${{ matrix.service | lowercase }}:${{ github.sha }} \
            -f src/${{ matrix.service }}/Dockerfile \
            src/${{ matrix.service }}/

      - name: Tag as latest
        run: |
          docker tag ${{ matrix.service | lowercase }}:${{ github.sha }} \
            ${{ matrix.service | lowercase }}:latest

      # In a real project, you would push to a container registry:
      # - name: Push to registry
      #   run: docker push myregistry/${{ matrix.service }}:${{ github.sha }}

      - name: Display image info
        run: docker images ${{ matrix.service | lowercase }}

  deploy-staging:
    runs-on: ubuntu-latest
    needs: docker-build
    environment: staging

    steps:
      - name: Deploy to Staging
        run: |
          echo "🚀 Deploying to STAGING environment..."
          echo "   Commit: ${{ github.sha }}"
          echo "   Branch: main"
          echo "   In production, this would run:"
          echo "   - kubectl apply -f k8s/ (Kubernetes)"
          echo "   - OR az webapp deploy (Azure App Service)"
          echo "   - OR aws ecs update-service (AWS ECS)"
          echo "✅ Staging deployment simulated successfully!"

  deploy-production:
    runs-on: ubuntu-latest
    needs: deploy-staging
    environment: production    # Requires manual approval in GitHub settings

    steps:
      - name: Deploy to Production
        run: |
          echo "🚀 Deploying to PRODUCTION environment..."
          echo "   Commit: ${{ github.sha }}"
          echo "✅ Production deployment simulated successfully!"
```

### Step 4.3 — Add Unit Tests

Replace `tests/ProductService.Tests/UnitTest1.cs` with a new file
`tests/ProductService.Tests/ProductsControllerTests.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Controllers;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Tests;

public class ProductsControllerTests
{
    private ProductDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ProductDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private ProductsController CreateController(ProductDbContext context)
    {
        var logger = new Mock<ILogger<ProductsController>>();
        return new ProductsController(context, logger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsSeededProducts()
    {
        // Arrange
        using var context = CreateContext();
        var controller = CreateController(context);

        // Act
        var result = await controller.GetAll();

        // Assert
        var products = Assert.IsAssignableFrom<IEnumerable<Product>>(result.Value);
        Assert.Equal(3, products.Count()); // 3 seeded products
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsProduct()
    {
        using var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.GetById(1);

        Assert.NotNull(result.Value);
        Assert.Equal("Laptop", result.Value!.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        using var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidProduct_ReturnsCreatedAtAction()
    {
        using var context = CreateContext();
        var controller = CreateController(context);

        var newProduct = new Product
        {
            Name = "Headphones",
            Description = "Wireless",
            Price = 49.99m,
            Category = "Electronics",
            StockQuantity = 100
        };

        var result = await controller.Create(newProduct);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var product = Assert.IsType<Product>(createdResult.Value);
        Assert.Equal("Headphones", product.Name);
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        using var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await context.Products.FindAsync(1));
    }

    [Fact]
    public async Task Delete_NonExistingId_ReturnsNotFound()
    {
        using var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
```

Add Moq to the test project:
```bash
dotnet add tests/ProductService.Tests package Moq
dotnet add tests/ProductService.Tests package Microsoft.EntityFrameworkCore.InMemory
```

Remove the old test file:
```bash
rm tests/ProductService.Tests/UnitTest1.cs
rm tests/OrderService.Tests/UnitTest1.cs
```

### Step 4.4 — Run Tests Locally

```bash
dotnet test --verbosity normal
```
All 6 tests should pass.

### Step 4.5 — Commit and Watch Pipeline

```bash
git add -A
git commit -m "Add CI/CD pipelines and unit tests"
git push origin main
```

Go to GitHub → your repo → Actions tab → watch the CI pipeline run!

### Step 4.6 — Add Build Badge to README

Create `README.md` in the repo root:
```markdown
# Microservices CI/CD Demo

![CI Pipeline](https://github.com/YOUR_USERNAME/dotnet-microservices-cicd-demo/actions/workflows/ci.yml/badge.svg)

A microservices demo using .NET Core, MVC, Web API, and Angular with GitHub Actions CI/CD.

## Architecture
- **ProductService** (Web API) — port 5001
- **OrderService** (Web API) — port 5002
- **API Gateway** (MVC + Web API) — port 5000
- **Angular Frontend** — port 4200

## Run Locally
```bash
# Start all services
dotnet run --project src/ProductService
dotnet run --project src/OrderService
dotnet run --project src/ApiGateway

# Start Angular
cd src/angular-frontend && ng serve
```

## Run Tests
```bash
dotnet test
```
```

---

## PHASE 5: Docker (30 min)

### Step 5.1 — Add Dockerfiles

Create file: `src/ProductService/Dockerfile`
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ProductService.dll"]
```

Create file: `src/OrderService/Dockerfile`
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "OrderService.dll"]
```

Create file: `src/ApiGateway/Dockerfile`
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
```

### Step 5.2 — Add Docker Compose

Create file: `docker-compose.yml` (in repo root)
```yaml
services:
  product-service:
    build:
      context: ./src/ProductService
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 10s
      timeout: 5s
      retries: 3

  order-service:
    build:
      context: ./src/OrderService
    ports:
      - "5002:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 10s
      timeout: 5s
      retries: 3

  api-gateway:
    build:
      context: ./src/ApiGateway
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ServiceUrls__ProductService=http://product-service:8080
      - ServiceUrls__OrderService=http://order-service:8080
    depends_on:
      product-service:
        condition: service_started
      order-service:
        condition: service_started
```

### Step 5.3 — Test with Docker Compose

```bash
docker-compose up --build
```
Test: http://localhost:5000 — should show dashboard with data from all services.

```bash
docker-compose down
```

### Step 5.4 — Final Commit

```bash
git add -A
git commit -m "Add Dockerfiles and docker-compose for local multi-service setup"
git push origin main
```

---

## PHASE 6: Verify & Practice (30 min)

### Checklist
- [ ] All services build: `dotnet build`
- [ ] All tests pass: `dotnet test`
- [ ] Docker compose runs: `docker-compose up --build`
- [ ] CI pipeline passes on GitHub (check Actions tab)
- [ ] Can explain the architecture diagram
- [ ] Can explain CI vs CD
- [ ] Can explain why microservices over monolith
- [ ] Reviewed `01-cheatsheet.md`
- [ ] Reviewed `03-interview-questions.md`
- [ ] Practiced the "Quick Architecture Pitch" out loud

### Key Talking Points for Interview
1. "I built this demo to practice microservices with CI/CD"
2. "Each service has its own in-memory database — demonstrating database-per-service"
3. "The API Gateway aggregates data from multiple services — this is the BFF pattern"
4. "GitHub Actions runs build and tests on every push automatically"
5. "Docker enables consistent environments from dev to production"
6. "The multi-stage Dockerfile reduces image size from ~700MB to ~100MB"

You're ready. Good luck tomorrow! 🎯
