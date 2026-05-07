# NAV Interview - Technical Coding Questions

## Q1: Create a CustomException class in .NET C#

### Answer

```csharp
using System;

public class CustomException : Exception
{
    public int ErrorCode { get; }

    public CustomException(string message) 
        : base(message)
    {
    }

    public CustomException(string message, int errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public CustomException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public CustomException(string message, int errorCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

// Usage
public class OrderService
{
    public void ProcessOrder(int orderId)
    {
        if (orderId <= 0)
            throw new CustomException("Invalid order ID", errorCode: 1001);

        try
        {
            // some operation
        }
        catch (Exception ex)
        {
            throw new CustomException("Order processing failed", errorCode: 1002, innerException: ex);
        }
    }
}

// Catching it
try
{
    service.ProcessOrder(-1);
}
catch (CustomException ex)
{
    Console.WriteLine($"Error {ex.ErrorCode}: {ex.Message}");
}
```

### Key Points
- Always inherit from `Exception` (or a more specific subclass like `ArgumentException`, `InvalidOperationException`)
- Implement the 3 standard constructors: `()`, `(message)`, `(message, innerException)` — this is a best practice
- Add domain-specific properties like `ErrorCode` to carry extra context
- Use `innerException` to preserve the original exception stack trace

### How to Approach in a Virtual Interview
1. **Say it first**: "I'll inherit from `Exception` since that's the base class for all .NET exceptions."
2. **Mention the 3 constructors**: Shows you know the convention.
3. **Add a custom property**: Demonstrates you understand the *why* — carrying extra context.
4. **Show usage**: Write a try/catch to show how it's consumed.
5. **Bonus**: Mention you could inherit from `ApplicationException` but it's discouraged in modern .NET — shows depth.

---

## Q2: Create a CustomString class that implements/extends the string class. Can we inherit string?

### Short Answer

**No, you cannot inherit from `string` in C#.** The `string` class is `sealed`, which means it cannot be subclassed.

```csharp
// This will NOT compile:
public class CustomString : string { } // Error: cannot derive from sealed type 'string'
```

### Why is string sealed?
- **Immutability guarantee**: `string` is immutable by design. Subclassing could break that contract.
- **Security**: The CLR (Common Language Runtime) treats strings specially — interning, memory layout, etc. Allowing inheritance would undermine these guarantees.
- **Performance**: Sealing allows the JIT compiler to optimize string operations.

### What You CAN Do Instead

#### Option 1: Wrapping (Composition over Inheritance)
```csharp
public class CustomString
{
    private readonly string _value;

    public CustomString(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public int Length => _value.Length;

    public CustomString ToUpperCase() => new CustomString(_value.ToUpper());

    public CustomString Trim() => new CustomString(_value.Trim());

    public bool ContainsIgnoreCase(string other) =>
        _value.Contains(other, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => _value;

    // Implicit conversion so it works like a string in many contexts
    public static implicit operator string(CustomString cs) => cs._value;
    public static implicit operator CustomString(string s) => new CustomString(s);
}

// Usage
CustomString name = "  Hello World  ";
CustomString trimmed = name.Trim();
Console.WriteLine(trimmed.ContainsIgnoreCase("hello")); // true
string plain = trimmed; // implicit conversion works
```

#### Option 2: Extension Methods (most idiomatic in C#)
```csharp
public static class StringExtensions
{
    public static bool ContainsIgnoreCase(this string source, string value) =>
        source.Contains(value, StringComparison.OrdinalIgnoreCase);

    public static string TruncateAt(this string source, int maxLength) =>
        source.Length <= maxLength ? source : source[..maxLength] + "...";

    public static bool IsPalindrome(this string source)
    {
        var chars = source.ToLower().ToCharArray();
        return chars.SequenceEqual(chars.Reverse());
    }
}

// Usage — works on any string, no wrapping needed
string name = "racecar";
Console.WriteLine(name.IsPalindrome()); // true
Console.WriteLine("Hello World".TruncateAt(5)); // Hello...
```

### Q2b: Create a method similar to `IsNullOrEmpty`

`string.IsNullOrEmpty` is a **static method** — it takes a string as a parameter rather than being called on an instance, because you can't call instance methods on `null`.

```csharp
// How the real one works:
string.IsNullOrEmpty(null);  // true
string.IsNullOrEmpty("");    // true
string.IsNullOrEmpty("hi"); // false
```

#### In the wrapper class — as a static method
```csharp
public class CustomString
{
    private readonly string _value;

    public CustomString(string value)
    {
        _value = value;  // allow null here so IsNullOrEmpty makes sense
    }

    // Mirror the static pattern of string.IsNullOrEmpty
    public static bool IsNullOrEmpty(CustomString cs)
    {
        return cs == null || cs._value == null || cs._value.Length == 0;
    }

    // Bonus: IsNullOrWhiteSpace equivalent
    public static bool IsNullOrWhiteSpace(CustomString cs)
    {
        return cs == null || string.IsNullOrWhiteSpace(cs._value);
    }

    public override string ToString() => _value ?? string.Empty;
}

// Usage
CustomString name = null;
Console.WriteLine(CustomString.IsNullOrEmpty(name));   // true

CustomString empty = new CustomString("");
Console.WriteLine(CustomString.IsNullOrEmpty(empty));  // true

CustomString valid = new CustomString("hello");
Console.WriteLine(CustomString.IsNullOrEmpty(valid));  // false
```

#### Why static and not an instance method?
Because if the object itself is `null`, you can't call any instance method on it — you'd get a `NullReferenceException`. Static methods sidestep this by accepting the value as a parameter.

```csharp
CustomString s = null;
s.IsNullOrEmpty(); // NullReferenceException!
CustomString.IsNullOrEmpty(s); // works fine
```

#### Interview tip: show you understand this distinction
> "I'd make it `static` just like `string.IsNullOrEmpty`, because if the object reference itself is null, you can't call an instance method on it — that would throw a NullReferenceException before the method even runs."

---

### Which approach to pick?
| Approach | When to use |
|----------|-------------|
| **Composition (wrapper)** | Need a distinct type, operator overloading, or want to enforce constraints (e.g., non-null, trimmed) |
| **Extension methods** | Just adding utility methods to existing strings — most common, most idiomatic |

### How to Approach in a Virtual Interview
1. **Answer the can-we-inherit question directly first**: "No — `string` is `sealed` in C#, so we can't inherit it."
2. **Explain why briefly**: Immutability, security, CLR internals.
3. **Pivot to solutions**: "But we can achieve the goal two ways — wrapping with composition, or extension methods."
4. **Write one**: Start with extension methods (simpler), then offer the wrapper class as a follow-up.
5. **Name the pattern**: "This is Composition over Inheritance — a core OOP principle."
6. **Bonus**: Mention `implicit operator` if you add it to the wrapper — shows you're thinking about usability.
