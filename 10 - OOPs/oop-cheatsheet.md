# Object-Oriented Programming — Interview Cheat Sheet

> The four pillars and key concepts that form the foundation of modern software design.
> Mastery of OOP is expected in every .NET / C# interview.

---

## The 4 Pillars at a Glance

| Pillar | One-Line Summary |
|--------|-----------------|
| **Encapsulation** | Bundle data + behaviour, hide internal state |
| **Abstraction** | Expose only what's necessary, hide complexity |
| **Inheritance** | Child class reuses and extends parent behaviour |
| **Polymorphism** | One interface, many forms — same method, different behaviour |

---

## Encapsulation

> **"Bundle data and the methods that operate on it. Hide internal state from the outside world."**

### Memory Hook
> Think of a **capsule pill** — the ingredients are sealed inside.
> You swallow it, but you never touch the chemicals directly.
> That's encapsulation — the outside world interacts through a controlled surface.

### Bad Example
```csharp
// All fields public — anyone can corrupt internal state
public class BankAccount
{
    public decimal Balance;  // ❌ Anyone can set this to -1 million
    public string Owner;
}

// Outside code breaking invariants freely:
var account = new BankAccount();
account.Balance = -99999;  // ❌ No validation, state is broken
```

### Good Example
```csharp
public class BankAccount
{
    private decimal _balance;          // private field — hidden
    private readonly string _owner;
    private readonly List<string> _transactionLog = new();

    public BankAccount(string owner, decimal initialDeposit)
    {
        _owner = owner;
        _balance = initialDeposit;
    }

    // Read-only property — consumers can SEE balance but not SET it
    public decimal Balance => _balance;
    public string Owner => _owner;

    // Controlled mutation — rules enforced inside the class
    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Deposit must be positive");
        _balance += amount;
        _transactionLog.Add($"Deposited: {amount:C}");
    }

    public bool Withdraw(decimal amount)
    {
        if (amount <= 0 || amount > _balance) return false;
        _balance -= amount;
        _transactionLog.Add($"Withdrew: {amount:C}");
        return true;
    }

    public IReadOnlyList<string> GetTransactionHistory() => _transactionLog;
}

// Usage — state changes only through defined contract
var account = new BankAccount("Alice", 1000);
account.Deposit(500);
bool success = account.Withdraw(200);
Console.WriteLine(account.Balance); // ✅ 1300
```

### Properties vs Fields — C# Nuance
```csharp
public class Person
{
    // Auto-property (shorthand) — compiler generates backing field
    public string Name { get; set; }

    // Read-only auto-property — set only in constructor
    public DateTime DateOfBirth { get; init; }

    // Computed property — no backing field
    public int Age => DateTime.Today.Year - DateOfBirth.Year;

    // Fully controlled property with validation
    private int _age;
    public int AgeValidated
    {
        get => _age;
        set
        {
            if (value < 0 || value > 150) throw new ArgumentOutOfRangeException();
            _age = value;
        }
    }
}
```

### Interview Answer
> *"Encapsulation means keeping internal state private and only allowing changes through well-defined methods. In my `BankAccount` class, the balance field is private — you can't directly set it to a negative value. Deposit and Withdraw enforce business rules. This prevents invalid state and localises changes — if validation logic changes, I update one place."*

---

## Abstraction

> **"Expose only what's necessary. Hide the implementation complexity behind a simple interface."**

### Memory Hook
> Think of **driving a car**.
> You use the steering wheel, pedals, and gear stick.
> You don't care about pistons, fuel injection, or the transmission internals.
> The car **abstracts** all that complexity behind a simple interface.

### Bad Example
```csharp
// Caller has to know ALL the low-level steps — no abstraction
public class EmailSender
{
    public SmtpClient SmtpClient = new SmtpClient();       // exposed internals
    public MailMessage Message = new MailMessage();         // exposed internals
    public string SmtpServer = "smtp.example.com";
    public int Port = 587;
    // Caller now needs to configure all this manually — tightly coupled!
}
```

### Good Example
```csharp
// Abstraction via abstract class
public abstract class Notification
{
    // Template method — defines the WHAT (algorithm), hides the HOW
    public void Send(string recipient, string message)
    {
        if (string.IsNullOrWhiteSpace(recipient)) throw new ArgumentException("Recipient required");
        Deliver(recipient, message);  // HOW is defined by subclass
        Log(recipient);               // Shared behaviour
    }

    protected abstract void Deliver(string recipient, string message);

    private void Log(string recipient)
    {
        Console.WriteLine($"[{DateTime.Now}] Notification sent to {recipient}");
    }
}

public class EmailNotification : Notification
{
    protected override void Deliver(string recipient, string message)
    {
        Console.WriteLine($"EMAIL → {recipient}: {message}");
        // Real SMTP complexity hidden here
    }
}

public class SmsNotification : Notification
{
    protected override void Deliver(string recipient, string message)
    {
        Console.WriteLine($"SMS → {recipient}: {message}");
        // Twilio API complexity hidden here
    }
}

// Caller only knows "Send" — no SMTP, no Twilio details
Notification notifier = new EmailNotification();
notifier.Send("alice@example.com", "Your order shipped!");
```

### Abstraction via Interface
```csharp
// Interface = pure abstraction (no implementation, just contract)
public interface IPaymentGateway
{
    bool Charge(string customerId, decimal amount);
    bool Refund(string transactionId);
}

public class StripeGateway : IPaymentGateway
{
    public bool Charge(string customerId, decimal amount)
    {
        // 100 lines of Stripe SDK calls — hidden from caller
        return true;
    }
    public bool Refund(string transactionId) { return true; }
}

// Checkout only knows the contract — not the provider
public class CheckoutService
{
    private readonly IPaymentGateway _gateway;
    public CheckoutService(IPaymentGateway gateway) => _gateway = gateway;

    public bool CompleteOrder(string customerId, decimal total)
    {
        return _gateway.Charge(customerId, total);
    }
}
```

### Interview Answer
> *"Abstraction means hiding complexity and showing only what the consumer needs. I use abstract classes for template logic — like a `Notification` base class where `Send()` handles logging and validation, but each subclass defines `Deliver()`. The caller just calls `Send()`. Interfaces are pure contracts — `IPaymentGateway` tells callers what they can do with a gateway, not how it works internally."*

---

## Inheritance

> **"A child class acquires properties and behaviour of its parent, and can extend or override them."**

### Memory Hook
> Think of **biological inheritance**.
> You inherit your parents' DNA (traits) but you also develop your own personality.
> A child class inherits the parent's fields and methods, then adds or overrides its own.

### Bad Example
```csharp
// Without inheritance — massive duplication
public class Car
{
    public string Brand { get; set; }
    public int Speed { get; set; }
    public void StartEngine() => Console.WriteLine("Car engine started");
    public void Accelerate() => Speed += 10;
}

public class Truck
{
    public string Brand { get; set; }
    public int Speed { get; set; }
    public int Payload { get; set; }
    public void StartEngine() => Console.WriteLine("Truck engine started"); // duplicated!
    public void Accelerate() => Speed += 5;
}
```

### Good Example
```csharp
// Base class — shared state and behaviour
public class Vehicle
{
    public string Brand { get; set; }
    public int Speed { get; protected set; }

    public Vehicle(string brand) => Brand = brand;

    public virtual void StartEngine()
    {
        Console.WriteLine($"{Brand} engine started.");
    }

    public virtual void Accelerate(int amount)
    {
        Speed += amount;
        Console.WriteLine($"{Brand} accelerating. Speed: {Speed} km/h");
    }
}

// Child inherits, extends, and can override
public class Car : Vehicle
{
    public int Doors { get; set; }

    public Car(string brand, int doors) : base(brand)
    {
        Doors = doors;
    }

    // Overriding parent behaviour
    public override void Accelerate(int amount)
    {
        Speed += amount * 2; // cars accelerate faster
        Console.WriteLine($"{Brand} (car) speed: {Speed} km/h");
    }
}

public class Truck : Vehicle
{
    public int PayloadTons { get; set; }

    public Truck(string brand, int payloadTons) : base(brand)
    {
        PayloadTons = payloadTons;
    }

    public override void StartEngine()
    {
        base.StartEngine(); // call parent first
        Console.WriteLine("Truck air brakes pressurised.");
    }
}

// Polymorphic usage
List<Vehicle> fleet = new()
{
    new Car("BMW", 4),
    new Truck("Volvo", 20)
};

foreach (var v in fleet)
{
    v.StartEngine(); // Calls overridden version for each type
}
```

### Method Hiding vs Overriding
```csharp
public class Animal
{
    public virtual void Speak() => Console.WriteLine("...");
    public void Move() => Console.WriteLine("Animal moves");
}

public class Dog : Animal
{
    // override — replaces parent at RUNTIME (polymorphism works)
    public override void Speak() => Console.WriteLine("Woof!");

    // new — HIDES parent method, NOT polymorphic
    public new void Move() => Console.WriteLine("Dog runs");
}

Animal a = new Dog();
a.Speak(); // "Woof!"   ← override works polymorphically
a.Move();  // "Animal moves" ← new doesn't — Animal.Move() is called
```

### Sealed Classes and Methods
```csharp
// sealed class — cannot be inherited
public sealed class Singleton { }
// public class MySingleton : Singleton { } // ❌ Compile error

// sealed method — prevents further overriding in deep hierarchies
public class Shape { public virtual double Area() => 0; }
public class Circle : Shape { public sealed override double Area() => Math.PI * 5 * 5; }
// public class SpecialCircle : Circle { public override double Area() => ...; } // ❌ Compile error
```

### Interview Answer
> *"Inheritance lets child classes reuse parent code and override specific behaviours. I use `virtual` + `override` for proper polymorphism — a `Truck` can call `base.StartEngine()` before adding its own logic. I avoid deep inheritance chains (more than 2-3 levels) because they create tight coupling and make debugging difficult. Prefer composition over inheritance when the relationship isn't a true IS-A."*

---

## Polymorphism

> **"One interface, many forms. The same method call behaves differently depending on the actual object type."**

### Memory Hook
> Think of a **universal TV remote**.
> The `Power` button works on a Samsung, Sony, or LG TV.
> Same button press — different internal response.

### Two Types

| Type | Happens At | Mechanism |
|------|-----------|-----------|
| **Compile-time** (Static) | Compile time | Method Overloading |
| **Runtime** (Dynamic) | Runtime | Method Overriding + virtual/override |

### Compile-Time Polymorphism — Method Overloading
```csharp
public class Calculator
{
    // Same method name, different signatures — resolved at compile time
    public int Add(int a, int b) => a + b;
    public double Add(double a, double b) => a + b;
    public int Add(int a, int b, int c) => a + b + c;
    public string Add(string a, string b) => a + b;  // string concatenation
}

var calc = new Calculator();
Console.WriteLine(calc.Add(1, 2));        // int version
Console.WriteLine(calc.Add(1.5, 2.5));    // double version
Console.WriteLine(calc.Add("Hello ", "World")); // string version
```

### Runtime Polymorphism — Method Overriding
```csharp
public abstract class Shape
{
    public abstract double Area();  // must be overridden

    // Template: concrete method calling abstract method
    public void PrintArea()
    {
        Console.WriteLine($"{GetType().Name} area: {Area():F2}");
    }
}

public class Circle : Shape
{
    public double Radius { get; set; }
    public override double Area() => Math.PI * Radius * Radius;
}

public class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }
    public override double Area() => Width * Height;
}

public class Triangle : Shape
{
    public double Base { get; set; }
    public double Height { get; set; }
    public override double Area() => 0.5 * Base * Height;
}

// Runtime polymorphism — actual type determines which Area() runs
List<Shape> shapes = new()
{
    new Circle { Radius = 5 },
    new Rectangle { Width = 4, Height = 6 },
    new Triangle { Base = 3, Height = 8 }
};

foreach (var shape in shapes)
{
    shape.PrintArea(); // Each calls its own Area() — decided at RUNTIME
}
// Circle area: 78.54
// Rectangle area: 24.00
// Triangle area: 12.00
```

### Operator Overloading (Compile-Time)
```csharp
public class Vector
{
    public double X { get; set; }
    public double Y { get; set; }

    public Vector(double x, double y) { X = x; Y = y; }

    // + operator overloaded
    public static Vector operator +(Vector a, Vector b)
        => new Vector(a.X + b.X, a.Y + b.Y);

    public override string ToString() => $"({X}, {Y})";
}

var v1 = new Vector(1, 2);
var v2 = new Vector(3, 4);
Console.WriteLine(v1 + v2); // (4, 6) — + is polymorphic here
```

### Interface Polymorphism (most common in real-world .NET)
```csharp
public interface IExporter
{
    void Export(IEnumerable<string> data);
}

public class CsvExporter : IExporter
{
    public void Export(IEnumerable<string> data)
    {
        Console.WriteLine("Exporting to CSV: " + string.Join(",", data));
    }
}

public class JsonExporter : IExporter
{
    public void Export(IEnumerable<string> data)
    {
        Console.WriteLine("Exporting to JSON: [" + string.Join(",", data) + "]");
    }
}

// Plugin-style: swap behaviour at runtime via DI or config
public class ReportService
{
    private readonly IExporter _exporter;
    public ReportService(IExporter exporter) => _exporter = exporter;

    public void GenerateReport(IEnumerable<string> data) => _exporter.Export(data);
}
```

### Interview Answer
> *"Polymorphism has two forms. Compile-time: method overloading — same name, different parameters, resolved by the compiler. Runtime: method overriding — a base class reference calls the overridden method on the actual object, resolved at runtime via the virtual dispatch table (vtable). In production .NET, I use interface polymorphism constantly — a `ReportService` depends on `IExporter`, and the concrete exporter (CSV, JSON, Excel) is swapped via DI with zero code changes."*

---

## Key Supporting Concepts

### Constructors — All Variants
```csharp
public class Order
{
    public int Id { get; set; }
    public string Customer { get; set; }
    public decimal Total { get; set; }

    // 1. Default constructor
    public Order() { }

    // 2. Parameterised constructor
    public Order(int id, string customer, decimal total)
    {
        Id = id;
        Customer = customer;
        Total = total;
    }

    // 3. Constructor chaining with :this()
    public Order(int id, string customer) : this(id, customer, 0m) { }

    // 4. Static constructor — runs once before first use of class
    static Order()
    {
        Console.WriteLine("Order type initialised.");
    }

    // 5. Copy constructor pattern
    public Order(Order other) : this(other.Id, other.Customer, other.Total) { }
}

// C# 9+ Record — immutable, auto-generates constructor, Equals, ToString
public record ProductRecord(int Id, string Name, decimal Price);
var p1 = new ProductRecord(1, "Laptop", 999.99m);
var p2 = p1 with { Price = 899.99m }; // non-destructive mutation
```

---

### Access Modifiers
```csharp
public class AccessDemo
{
    public int PublicField;          // accessible everywhere
    internal int InternalField;      // accessible within same assembly
    protected int ProtectedField;    // accessible in this class + subclasses
    protected internal int ProtectedInternal; // protected OR internal
    private protected int PrivateProtected;   // protected AND internal (C# 7.2+)
    private int PrivateField;        // accessible only in this class
}
```

| Modifier | Same Class | Subclass | Same Assembly | Other Assembly |
|----------|-----------|----------|---------------|----------------|
| `public` | ✅ | ✅ | ✅ | ✅ |
| `internal` | ✅ | ✅ | ✅ | ❌ |
| `protected` | ✅ | ✅ | ❌ | ❌ |
| `protected internal` | ✅ | ✅ | ✅ | ❌ |
| `private protected` | ✅ | ✅ (same assembly) | ❌ | ❌ |
| `private` | ✅ | ❌ | ❌ | ❌ |

---

### Abstract Class vs Interface — Decision Guide
```csharp
// USE ABSTRACT CLASS when:
// - IS-A relationship (Dog IS-A Animal)
// - Shared state (fields) or implementation
// - Want protected members accessible to subclasses

public abstract class Animal
{
    protected string Name { get; }         // shared state
    public Animal(string name) => Name = name;  // shared constructor

    public abstract string MakeSound();    // subclass must implement
    public void Sleep() => Console.WriteLine($"{Name} is sleeping"); // shared behaviour
}

// USE INTERFACE when:
// - CAN-DO relationship (Bird CAN-FLY, Plane CAN-FLY)
// - Unrelated types sharing a capability
// - Multiple "contracts" needed (a class can implement many interfaces)

public interface IFlyable  { void Fly(); }
public interface ISwimmable { void Swim(); }

public class Duck : Animal, IFlyable, ISwimmable  // inherits one, implements many
{
    public Duck() : base("Duck") { }
    public override string MakeSound() => "Quack!";
    public void Fly() => Console.WriteLine("Duck flying low");
    public void Swim() => Console.WriteLine("Duck paddling");
}
```

---

### Static vs Instance Members
```csharp
public class Counter
{
    // Static: shared across ALL instances, belongs to the class
    private static int _totalCount = 0;
    public static int TotalCount => _totalCount;

    // Instance: belongs to each individual object
    private int _instanceCount = 0;
    public int InstanceCount => _instanceCount;

    public void Increment()
    {
        _instanceCount++;    // only this object's count
        _totalCount++;       // shared counter for all instances
    }
}

var c1 = new Counter();
var c2 = new Counter();
c1.Increment(); c1.Increment();
c2.Increment();

Console.WriteLine(c1.InstanceCount);    // 2
Console.WriteLine(c2.InstanceCount);    // 1
Console.WriteLine(Counter.TotalCount);  // 3 — shared
```

---

### Object Class — Methods Every C# Developer Must Know
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    // Override Equals for value equality (default is reference equality)
    public override bool Equals(object? obj)
    {
        if (obj is not Product other) return false;
        return Id == other.Id;
    }

    // ALWAYS override GetHashCode when overriding Equals
    // Objects that are Equal must have the same hash code
    public override int GetHashCode() => Id.GetHashCode();

    // Human-readable string representation
    public override string ToString() => $"Product [{Id}] {Name} @ {Price:C}";
}

var p1 = new Product { Id = 1, Name = "Laptop", Price = 999m };
var p2 = new Product { Id = 1, Name = "Laptop", Price = 999m };

Console.WriteLine(p1.Equals(p2));  // true  (overridden Equals)
Console.WriteLine(p1 == p2);       // false (reference equality, == not overridden)
Console.WriteLine(p1);             // "Product [1] Laptop @ £999.00"
```

---

### Composition over Inheritance
```csharp
// ❌ Deep inheritance — fragile, rigid
public class Animal { }
public class Pet : Animal { }
public class TrainedPet : Pet { }
public class GuardDog : TrainedPet { }  // 4 levels deep — change Animal, breaks GuardDog

// ✅ Composition — flexible, testable
public class Logger
{
    public void Log(string msg) => Console.WriteLine($"[LOG] {msg}");
}

public class Authenticator
{
    public bool Authenticate(string token) => token.Length > 5;
}

public class OrderService
{
    private readonly Logger _logger;
    private readonly Authenticator _auth;

    // Composed from focused collaborators
    public OrderService(Logger logger, Authenticator auth)
    {
        _logger = logger;
        _auth = auth;
    }

    public void PlaceOrder(string token, Order order)
    {
        if (!_auth.Authenticate(token)) throw new UnauthorizedAccessException();
        _logger.Log($"Order placed: {order.Id}");
    }
}
```

---

## Tricky Interview Questions

**Q: What is the difference between `==` and `Equals()` in C#?**
> `==` is a reference comparison by default for classes (are they the same object in memory?).
> `Equals()` can be overridden for value equality (do they contain the same data?).
> Strings override `==` to behave as value equality — a common source of confusion.

**Q: Can you call a virtual method in a constructor? Is it safe?**
```csharp
public class Base
{
    public Base() => Print(); // ❌ Dangerous!
    public virtual void Print() => Console.WriteLine("Base");
}

public class Derived : Base
{
    private string _name = "Derived";
    public override void Print() => Console.WriteLine(_name);
}

new Derived(); // prints null — _name not yet set when Base() calls Print()
// Avoid calling virtual methods from constructors — derived state not initialised yet.
```

**Q: What's the difference between method overloading and overriding?**
> **Overloading** (compile-time): same name, different parameters — different methods resolved at compile time.
> **Overriding** (runtime): same name, same signature — child replaces parent behaviour, resolved at runtime via vtable.

**Q: Can a struct implement an interface in C#?**
```csharp
public interface IArea { double Area(); }

public struct Rectangle : IArea  // ✅ Yes, structs can implement interfaces
{
    public double Width;
    public double Height;
    public double Area() => Width * Height;
}
// WARNING: boxing occurs when struct is treated as interface reference
IArea r = new Rectangle { Width = 5, Height = 3 }; // boxing!
```

**Q: What is the Liskov Substitution Principle and how does it relate to inheritance?**
> LSP says a subclass must be fully substitutable for its base class without breaking expectations.
> Classic violation: `Square` inheriting `Rectangle` — setting Width also sets Height, breaking the area assumption.
> Fix: don't force IS-A when it isn't a true substitution — use separate abstractions.

**Q: What's the difference between early binding and late binding?**
> **Early binding** (compile-time): method call resolved at compile time. Method overloading, `new` hiding.
> **Late binding** (runtime): actual type determines which method runs. `virtual` + `override`, interface dispatch.

**Q: Can an abstract class have a constructor? Can you call it?**
```csharp
public abstract class Animal
{
    protected string Name;
    // ✅ Yes — abstract class CAN have constructor
    protected Animal(string name) => Name = name;
}

public class Dog : Animal
{
    public Dog(string name) : base(name) { } // called via base()
}

// new Animal("x"); // ❌ Cannot instantiate abstract class directly
```

---

## Quick Recall Table

| Concept | Violation Symptom | Fix |
|---------|-------------------|-----|
| **Encapsulation** | Public fields mutated from anywhere | Make private, expose via controlled methods/properties |
| **Abstraction** | Caller knows internal implementation details | Use interfaces or abstract classes as the surface |
| **Inheritance** | Deep chains (4+ levels), fragile base class | Prefer composition; limit to 2-3 levels max |
| **Polymorphism** | Long `if/switch` chains checking object type | Replace with virtual methods or interface dispatch |
| **Overloading** | Methods differing only in name convention | Use overloads with different signatures |
| **Encapsulation** | Logic spread across calling classes | Move behaviour close to the data it operates on |

---

## 30-Second Elevator Pitch for OOP

> *"OOP organises code around objects that combine state and behaviour.
> Encapsulation protects internal state — changes are only possible through defined methods, preventing invalid state.
> Abstraction hides complexity — callers work with simple interfaces, not implementation details.
> Inheritance lets child classes reuse parent code and specialise behaviour through overriding.
> Polymorphism means one interface handles many types — a List of Shapes can call Area() on each without knowing if it's a Circle or Rectangle.
> In modern .NET, I favour interfaces over deep inheritance, and composition over inheritance when the relationship isn't a true IS-A. These four pillars, combined with SOLID principles, keep code maintainable, testable, and easy to extend."*

---

*Last updated: 2026-03-25*
