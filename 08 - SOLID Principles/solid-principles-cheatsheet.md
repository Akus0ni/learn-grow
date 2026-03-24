# SOLID Principles — Interview Cheat Sheet

> Five design principles that make software **maintainable, extensible, and testable**.
> Coined by Robert C. Martin (Uncle Bob).

---

## The 5 Principles at a Glance

| Letter | Principle | One-Line Summary |
|--------|-----------|-----------------|
| **S** | Single Responsibility | One class, one reason to change |
| **O** | Open/Closed | Open to extend, closed to modify |
| **L** | Liskov Substitution | Subclass must fully replace its parent |
| **I** | Interface Segregation | Don't force clients to implement what they don't need |
| **D** | Dependency Inversion | Depend on abstractions, not concretes |

---

## S — Single Responsibility Principle (SRP)

> **"A class should have only one reason to change."**

### Memory Hook
> Think of a **Swiss Army Knife vs. a Chef's Knife**.
> A chef's knife does ONE thing perfectly — cuts food.
> A Swiss Army knife tries to do everything — and none of it well.

### Bad Example
```csharp
// This class does TOO MUCH — report generation AND email sending
public class ReportManager
{
    public string GenerateReport(Order order) { ... }  // responsibility 1
    public void SendEmail(string report) { ... }        // responsibility 2
    public void SaveToDatabase(string report) { ... }   // responsibility 3
}
```
**Problem:** If email logic changes, you touch the report class. If DB schema changes, same class.

### Good Example
```csharp
public class ReportGenerator
{
    public string Generate(Order order) { ... }  // only knows about reports
}

public class EmailService
{
    public void Send(string content) { ... }     // only knows about emails
}

public class ReportRepository
{
    public void Save(string report) { ... }      // only knows about persistence
}
```

### Interview Answer
> *"SRP means a class should only have one job. If my `UserService` is also sending emails and writing logs, that's three reasons to change it — three teams touching one file. I split them so each class evolves independently."*

---

## O — Open/Closed Principle (OCP)

> **"Software entities should be open for extension, but closed for modification."**

### Memory Hook
> Think of a **plugin system** — you add new plugins without rewriting the browser.
> Or a **phone case** — you swap cases without modifying the phone.

### Bad Example
```csharp
// Every time a new shape is added, you MODIFY this class
public class AreaCalculator
{
    public double Calculate(object shape)
    {
        if (shape is Circle c) return Math.PI * c.Radius * c.Radius;
        if (shape is Rectangle r) return r.Width * r.Height;
        // Adding Triangle? You MUST edit this class. OCP violation!
    }
}
```

### Good Example
```csharp
// Define the contract
public interface IShape
{
    double Area();
}

// Each shape knows its own area — no modification to existing code
public class Circle : IShape
{
    public double Radius { get; set; }
    public double Area() => Math.PI * Radius * Radius;
}

public class Rectangle : IShape
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double Area() => Width * Height;
}

// Adding Triangle? Just add a NEW class — existing code untouched
public class Triangle : IShape
{
    public double Base { get; set; }
    public double Height { get; set; }
    public double Area() => 0.5 * Base * Height;
}

public class AreaCalculator
{
    public double Calculate(IShape shape) => shape.Area(); // never changes
}
```

### Interview Answer
> *"OCP means I design classes so new behavior is added by creating new code, not editing old code. With interfaces or abstract classes, I can add a new payment method — PayPal, Stripe — without touching the existing checkout logic. Old code stays stable, new code is isolated."*

---

## L — Liskov Substitution Principle (LSP)

> **"Subtypes must be substitutable for their base types without breaking the program."**

### Memory Hook
> **The Duck Test gone wrong:**
> If it looks like a duck and quacks like a duck but needs batteries — it's NOT a duck.
> A `Square` looks like a `Rectangle` — but it isn't one (behaviorally).

### Bad Example — The Classic Square/Rectangle Trap
```csharp
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
    public int Area() => Width * Height;
}

public class Square : Rectangle
{
    // Square forces both sides equal — BREAKS the contract!
    public override int Width  { set { base.Width = base.Height = value; } }
    public override int Height { set { base.Width = base.Height = value; } }
}

// This breaks with Square!
void Resize(Rectangle r)
{
    r.Width = 5;
    r.Height = 10;
    Console.WriteLine(r.Area()); // Expected: 50, Got: 100 (Square broke it)
}
```

### Good Example
```csharp
// Don't force the inheritance — model what they truly share
public interface IShape { int Area(); }

public class Rectangle : IShape
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Area() => Width * Height;
}

public class Square : IShape
{
    public int Side { get; set; }
    public int Area() => Side * Side;
}
```

### Interview Answer
> *"LSP means a child class must honour the contract of its parent — same inputs, same expected behaviour. If I have a `Bird` base class with a `Fly()` method and I create a `Penguin`, calling `Fly()` on a Penguin breaks expectations. The fix is to rethink the hierarchy — maybe `FlyingBird` and `NonFlyingBird`."*

---

## I — Interface Segregation Principle (ISP)

> **"Clients should not be forced to depend on interfaces they do not use."**

### Memory Hook
> Think of a **TV remote with 50 buttons** — you only use 5.
> A better design: separate remote for TV power/volume, one for streaming apps.
> **Fat interfaces are like fat remotes** — confusing and wasteful.

### Bad Example
```csharp
// One fat interface — forces ALL machines to implement ALL methods
public interface IMachine
{
    void Print(Document d);
    void Scan(Document d);
    void Fax(Document d);
    void Staple(Document d);
}

// Old basic printer CAN'T fax or staple — forced to implement garbage!
public class OldPrinter : IMachine
{
    public void Print(Document d) { ... }   // OK
    public void Scan(Document d) { ... }    // OK
    public void Fax(Document d) => throw new NotImplementedException();   // PROBLEM
    public void Staple(Document d) => throw new NotImplementedException(); // PROBLEM
}
```

### Good Example
```csharp
// Split into focused interfaces
public interface IPrinter  { void Print(Document d); }
public interface IScanner  { void Scan(Document d); }
public interface IFax      { void Fax(Document d); }

// Old printer only implements what it actually supports
public class OldPrinter : IPrinter, IScanner
{
    public void Print(Document d) { ... }
    public void Scan(Document d) { ... }
}

// Modern all-in-one implements all
public class ModernPrinter : IPrinter, IScanner, IFax
{
    public void Print(Document d) { ... }
    public void Scan(Document d) { ... }
    public void Fax(Document d) { ... }
}
```

### Interview Answer
> *"ISP says split big interfaces into small, role-specific ones. In my work, I avoid 'God interfaces' with 20 methods where a class only needs 3. This means implementing classes stay lean, and changes to an unused method don't ripple across unrelated classes."*

---

## D — Dependency Inversion Principle (DIP)

> **"High-level modules should not depend on low-level modules. Both should depend on abstractions."**

### Memory Hook
> Think of a **power outlet standard**.
> Your laptop (high-level) doesn't depend on a specific power plant (low-level).
> Both depend on the **outlet standard (abstraction)** — 230V, Type-C, etc.
> Swap the power plant, laptop still works.

### Bad Example
```csharp
// High-level OrderService is TIGHTLY COUPLED to a specific low-level class
public class OrderService
{
    private SqlOrderRepository _repo = new SqlOrderRepository(); // hardcoded!

    public void PlaceOrder(Order order)
    {
        _repo.Save(order); // If DB changes to MongoDB — this whole class breaks
    }
}
```

### Good Example
```csharp
// Define the abstraction
public interface IOrderRepository
{
    void Save(Order order);
}

// Low-level detail implements the abstraction
public class SqlOrderRepository : IOrderRepository
{
    public void Save(Order order) { /* SQL logic */ }
}

public class MongoOrderRepository : IOrderRepository
{
    public void Save(Order order) { /* Mongo logic */ }
}

// High-level module depends on the INTERFACE, not the concrete class
public class OrderService
{
    private readonly IOrderRepository _repo;

    // Injected from outside — this is Dependency Injection in action
    public OrderService(IOrderRepository repo)
    {
        _repo = repo;
    }

    public void PlaceOrder(Order order) => _repo.Save(order);
}

// In startup / DI container:
// builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
```

### Interview Answer
> *"DIP means high-level business logic shouldn't care about low-level implementation details. I use constructor injection with interfaces — my `OrderService` depends on `IOrderRepository`, not `SqlOrderRepository`. Tomorrow if we switch to MongoDB, I swap one line in the DI container — zero changes in business logic."*

---

## Putting It All Together — Quick Recall Table

| Principle | Violation Symptom | Fix |
|-----------|-------------------|-----|
| **SRP** | Class has methods from 3 different concerns | Split into 3 classes |
| **OCP** | Every new feature requires editing existing class | Use interfaces/abstract + new classes |
| **LSP** | Subclass throws `NotImplementedException` or returns unexpected values | Rethink hierarchy or split interfaces |
| **ISP** | Class implements methods with `throw new NotImplementedException()` | Split fat interface into smaller ones |
| **DIP** | `new ConcreteClass()` inside a business class | Inject via interface (constructor injection) |

---

## Common Interview Questions & Crisp Answers

**Q: What's the difference between OCP and DIP?**
> OCP is about *extending behaviour without modifying existing code* (use inheritance/composition).
> DIP is about *who you depend on* — depend on abstractions, not concrete implementations. They often work together.

**Q: Can a class violate SRP but still be SOLID in other ways?**
> Yes — principles are independent. A class can follow OCP perfectly but still have multiple responsibilities. All five work together for the best design.

**Q: Is Dependency Injection the same as Dependency Inversion?**
> No. DIP is the **principle** — depend on abstractions. DI (Dependency Injection) is a **pattern/technique** used to achieve it — passing dependencies from outside instead of creating them inside.

**Q: Real-world example of LSP violation?**
> The classic: `Square` extending `Rectangle`. In .NET, `IEnumerable` vs `ICollection` — not all `IEnumerable` are `ICollection`, so you can't substitute freely. Always check if the child class honours the full contract of the parent.

---

## 30-Second Elevator Pitch for Any SOLID Question

> *"SOLID is a set of five design principles that keep code maintainable as it grows.
> S — one class, one job.
> O — extend with new code, don't edit old code.
> L — child classes must fully honour their parent's contract.
> I — small focused interfaces over one fat interface.
> D — depend on abstractions so you can swap implementations easily.
> In practice, I apply these to avoid the common traps: God classes, rigid code that breaks when extended, and tight coupling that makes unit testing a nightmare."*

---

*Last updated: 2026-03-24*
