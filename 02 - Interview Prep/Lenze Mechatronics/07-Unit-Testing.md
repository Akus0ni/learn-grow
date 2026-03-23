# Unit Testing & Test Frameworks - Interview Q&A

> Sr. Software Engineer (6+ YoE) | C#/.NET Focus

---

### Q1: What is the fundamental difference between Unit Testing and Integration Testing? Why do we need both?

**Answer:** Unit testing and Integration testing serve different purposes but are complementary in achieving software quality.

- **Unit Testing** focuses on validating the smallest piece of testable code (typically a method or class) **in complete isolation**. It verifies that the internal logic, algorithms, and state changes behave correctly. To achieve isolation, all external dependencies (databases, external APIs, filesystem, message queues) must be replaced with Test Doubles (mocks/stubs). Unit tests are deterministic, run in milliseconds, and are intended to run constantly on developer machines (often during TDD cycles).

- **Integration Testing** verifies that different modules or services **work together correctly**. The goal is to test the "glue" or integration points. It often involves real dependencies or lightweight alternatives (like in-memory databases, Testcontainers running Docker images, or WireMock). These tests are slower, can be flaky if data state isn't managed perfectly, and usually execute in a CI/CD pipeline rather than on every local keystroke.

**Why Both?** A system can have 100% unit test coverage and still fail entirely if the units are wired together incorrectly (e.g., mismatched dependency injection registrations or mismatched database schema expectations). Unit tests ensure the logic is right; integration tests ensure the architecture is wired correctly.

---

### Q2: Compare the primary .NET Testing Frameworks: MSTest, NUnit, and xUnit. Which would you choose for a new .NET project and why?

**Answer:** 
All three frameworks are fully functional, but they differ in design philosophy and history.

| Feature | MSTest (v2/v3) | NUnit (v3) | xUnit (v2/v3) |
|---------|----------------|------------|---------------|
| **Attributes** | `[TestClass]`, `[TestMethod]` | `[TestFixture]`, `[Test]` | `[Fact]`, `[Theory]` |
| **Parameterized**| `[DataRow]` | `[TestCase]`, `[TestCaseSource]` | `[InlineData]`, `[MemberData]` |
| **Setup/Teardown**| `[TestInitialize]`, `[TestCleanup]` | `[SetUp]`, `[TearDown]` | Constructors, `IDisposable` |
| **Isolation** | Single class instance per execution run | Single class instance per execution run | **New instance** per test method |
| **Execution** | Parallel by class | Parallel by class or method | Parallel by collection (class group) |

**Recommendation for New Projects:** **xUnit** is generally the preferred choice for modern .NET applications (and is used internally by Microsoft for .NET Core). 

**Why xUnit?**
1. **Enforced Isolation:** By instantiating the test class anew for *every single test method*, xUnit dramatically reduces the risk of shared state bugs ("Test A passes if run alone, but fails if run after Test B").
2. **Modern Setup/Teardown:** It avoids proprietary `[SetUp]` attributes, relying instead on standard object-oriented C# concepts: the constructor for initialization and `IDisposable.Dispose()` for cleanup.
3. **Clean Extensibility:** Its use of `[Theory]` and `[ClassData]`/`[MemberData]` makes large-scale data-driven testing extremely powerful and expressive.

---

### Q3: Explain the Arrange-Act-Assert (AAA) pattern. What are the benefits of organizing tests this way?

**Answer:** The Arrange-Act-Assert (AAA) pattern is the industry standard for structuring unit tests. It provides clarity and uniformity.

- **Arrange:** Sets up the precise preconditions and inputs needed for the test. This includes object instantiation, defining variables, and configuring mock behaviors (e.g., `_mockRepo.Setup(...)`).
- **Act:** Executes the single specific method or functionality under test. Ideally, this should be a single line of code returning a result or modifying a state.
- **Assert:** Verifies that the action produced the expected result. This includes checking return values, verifying property changes, evaluating exceptions, or verifying that a mock was called accurately (`_mockRepo.Verify(...)`).

**Code Example:**
```csharp
[Fact]
public void CalculateDiscount_WithPremiumCustomer_ReturnsTwentyPercentOff()
{
    // Arrange
    var calculator = new DiscountCalculator();
    var customer = new Customer { IsPremium = true };
    decimal originalPrice = 100m;

    // Act
    decimal discountedPrice = calculator.CalculateDiscount(customer, originalPrice);

    // Assert
    Assert.Equal(80m, discountedPrice);
}
```

**Benefits:**
- **Readability:** Developers instantly understand the structure of the test and can distinguish setup boilerplate from the actual action being tested.
- **Focused Scope:** If you find your "Act" phase spans multiple method calls, it indicates the test is doing too much or the code lacks cohesion.

---

### Q4: How do xUnit and NUnit handle Test Fixture setup differently?

**Answer:** They have fundamentally different philosophies regarding object lifetimes.

**NUnit Approach (Attribute-based):**
NUnit relies heavily on attributes for setup and teardown.
```csharp
[TestFixture]
public class MathTests
{
    private Calculator _calc; // Shared instance during run

    [SetUp] // Runs BEFORE EACH test
    public void Setup() => _calc = new Calculator();

    [TearDown] // Runs AFTER EACH test
    public void Cleanup() { /* release resources */ }

    [Test]
    public void AddTest() { ... }
}
```
*Risk:* If you forget to reset a mutable property in `[SetUp]`, state can leak between tests if the `_calc` was modified.

**xUnit Approach (Constructor-based):**
xUnit avoids attributes. The test runner creates a **brand-new instance** of the test class for every test method executed.
```csharp
public class MathTests : IDisposable
{
    private readonly Calculator _calc; // Fresh instance for every test

    // Acts as [SetUp]
    public MathTests() 
    {
        _calc = new Calculator();
    }

    // Acts as [TearDown]
    public void Dispose() 
    {
        // release resources
    }

    [Fact]
    public void AddTest() { ... }
}
```
If you absolutely must share state (e.g., an expensive DB connection) across multiple tests in xUnit, you implement `IClassFixture<T>` and inject it via the constructor. This enforces explicit awareness of shared test context.

---

### Q5: What is Test-Driven Development (TDD) and Behavior-Driven Development (BDD)?

**Answer:** 
**Test-Driven Development (TDD)** is an evolutionary software development process where tests are written *before* the application code. It follows a strict micro-cycle known as **Red-Green-Refactor**:
1. **Red:** Write a small test defining the desired improvement or capability. Run it. It must fail (proving it's testing something new).
2. **Green:** Write the simplest, ugliest code necessary to make the test pass.
3. **Refactor:** Clean up the code, apply design patterns, and remove duplication, while the passing test acts as a safety harness.

*Senior perspective:* TDD is fundamentally a **design technique**, not just a testing technique. It forces you to think about the public API of your class from the caller's perspective before worrying about implementation details, naturally producing loosely coupled code.

**Behavior-Driven Development (BDD)** is an extension of TDD focusing on the *business behavior*. It aims to bridge the communication gap between business stakeholders and technical developers using ubiquitous language.
- Tests are written in a strictly formatted natural language, often Gherkin (e.g., `Given` some context, `When` some action occurs, `Then` verify this consequence).
- In .NET, tools like **SpecFlow** bind these plain-text English sentences to underlying C# test methods.
- BDD shifts the focus from "testing technical units" to "verifying business scenarios."

---

### Q6: What are Test Doubles? Differentiate between Dummy, Stub, Spy, Mock, and Fake.

**Answer:** A "Test Double" is a generic term (coined by Martin Fowler/Gerard Meszaros) for any object replacing a real component for testing purposes. They serve different intents:

1. **Dummy:** Objects passed around merely to satisfy the compiler or method signatures, but they are never actually used or executed inside the test.
   - *Example:* Passing an empty string or `.ItIsAny<Logger>()` simply because the constructor requires it.

2. **Stub:** Provide canned answers to calls made during the test. They are purely state-based and do not fail the test directly.
   - *Example:* An `IUserRepository` stub configured to firmly return `new User { Id = 1 }` whenever `GetUser` is called.

3. **Spy:** Similar to a stub, but it secretly records information about how it was called (e.g., number of times called, arguments passed). You verify this state later.

4. **Mock:** Objects pre-programmed with expectations. You use a mock to verify behavior (interaction testing). If the specific expected method isn't called, or is called with the wrong parameters, the mock itself fails the test.
   - *Example:* `_mockMailService.Verify(m => m.Send("welcome@test.com"), Times.Once);`

5. **Fake:** Objects that actually have working implementations, but usually take shortcuts making them unsuitable for production.
   - *Example:* An In-Memory database repository (using `Dictionary<int, Entity>`) instead of connecting to SQL Server.

---

### Q7: Explain Data-Driven Testing (Parameterized Tests) using xUnit.

**Answer:** Data-driven testing allows you to run the exact same test logic multiple times using different inputs. It heavily reduces test duplication. In xUnit, this is achieved using the `[Theory]` attribute.

**1. InlineData (For constants/primitives):**
```csharp
[Theory]
[InlineData(2, 2, 4)]
[InlineData(5, -2, 3)]
[InlineData(0, 0, 0)]
public void Add_ReturnsCorrectSum(int a, int b, int expected)
{
    var calc = new Calculator();
    Assert.Equal(expected, calc.Add(a, b));
}
```

**2. MemberData (For complex objects or dynamic data):**
Uses a static property or method returning an `IEnumerable<object[]>`.
```csharp
public static IEnumerable<object[]> GetDiscountData()
{
    yield return new object[] { new Customer(isPremium: true), 100m, 80m };
    yield return new object[] { new Customer(isPremium: false), 100m, 100m };
}

[Theory]
[MemberData(nameof(GetDiscountData))]
public void CalculateDiscount_Theory(Customer cust, decimal price, decimal expected)
{
    var calc = new DiscountCalculator();
    Assert.Equal(expected, calc.Calculate(cust, price));
}
```

**3. ClassData:**
Moves the data generation logic entirely into a dedicated class implementing `IEnumerable<object[]>`, allowing data to be shared across multiple test classes cleanly.

---

### Q8: How does the Moq framework work in C#? Show an example setting up a mock and verifying behavior.

**Answer:** **Moq** is the most popular mocking library for .NET. It leverages LINQ expression trees and Lambda expressions to intercept interface or virtual method calls and dictate behavior or verify interactions.

**Key concepts in Moq:**
- **Setup():** Dictates what the mock should do when a method is called (e.g., return a value, throw an exception).
- **Verify():** Asserts that an interaction actually took place.
- **It.IsAny<T>():** Argument matching; accepts any value of type T.

**Code Example:**
```csharp
public class OrderProcessorTests
{
    [Fact]
    public void Process_ValidOrder_ChargesPaymentAndSaves()
    {
        // Arrange
        var mockPayment = new Mock<IPaymentGateway>();
        var mockRepo = new Mock<IOrderRepository>();
        
        // Setup mock to return true regardless of the amount passed
        mockPayment.Setup(p => p.Charge(It.IsAny<decimal>())).Returns(true);

        var processor = new OrderProcessor(mockPayment.Object, mockRepo.Object);
        var order = new Order { TotalAmount = 100m };

        // Act
        processor.ProcessOrder(order);

        // Assert/Verify Behavior
        // Ensure Charge was called exactly once with 100m
        mockPayment.Verify(p => p.Charge(100m), Times.Once);
        
        // Ensure Save was called exactly once with our order object
        mockRepo.Verify(r => r.SaveOrder(order), Times.Once);
    }
}
```

---

### Q9: How do you handle testing Private or Internal methods?

**Answer:** 
**Private Methods:** 
As a best practice, **you should not test private methods directly.** Private methods are implementation details. If you unit test the `public` method that calls the private method, the private logic is implicitly covered. 
*Architectural hint:* If a private method contains logic so complex that it desperately demands its own isolated tests, it is almost always a violation of the Single Responsibility Principle (SRP). You should extract that private logic into a new class with a public interface, inject that new class as a dependency, and test it there.

**Internal Methods:**
`internal` classes and methods are visible only within the assembly they are declared. To test them without making them public, you use the `[assembly: InternalsVisibleTo("Your.Test.Project.Name")]` attribute in your application assembly. E.g., in modern .NET (`.csproj`):
```xml
<ItemGroup>
  <InternalsVisibleTo Include="MyApplication.UnitTests" />
  <InternalsVisibleTo Include="DynamicProxyGenAssembly2" /> <!-- Required for Moq to mock internals! -->
</ItemGroup>
```

---

### Q10: How do you properly test Asynchronous code (`async`/`await`) in C#?

**Answer:** To test asynchronous methods, the unit test method itself must be marked as `async Task`.

```csharp
[Fact]
public async Task GetDataAsync_ReturnsExpectedResult()
{
    // Arrange
    var service = new RemoteService();

    // Act
    var result = await service.GetDataAsync();

    // Assert
    Assert.NotNull(result);
}
```

**Crucial Pitfall:** **Never use `async void` for unit tests.** 
If a test is `async void`, the test runner (xUnit/NUnit) is unable to `await` the completion of the test method. 
- If the test passes synchronously initially but fails on a background thread, the assertion exception is thrown unhandled on the ThreadPool, crashing the entire test runner process without reporting a clean test failure. 
- The test may be marked as "Pass" prematurely because the runner finishes before the `await` completes.

---

### Q11: What is Code Coverage, and is targeting 100% Code Coverage a pragmatic goal?

**Answer:** 
Code Coverage is an automated metric detailing the percentage of source code lines (or branches/blocks) executed during the test suite run.

**Targeting 100% is almost universally considered an anti-pattern.** 
Why?
1. **Law of Diminishing Returns:** Getting from 0% to 80% covers all critical business logic. Getting from 80% to 100% usually forces developers to write brittle tests for auto-generated properties, guard clauses that cannot reasonably fail, framework wiring, and simple DTOs.
2. **Coverage does not equal Quality:** A test can execute a line of code without asserting anything meaningful about its state. You can have 100% coverage with zero `Assert` statements.
3. **Fragility:** Tests written just to hit coverage metrics are heavily tied to implementation details and will break upon minor refactoring.

**A pragmatic approach:** Target 70-85% overall coverage, but mandate near 100% coverage on core domain models, pricing engines, or critical algorithmic services. Exclude UI layers and simple DTOs using `[ExcludeFromCodeCoverage]`.

---

### Q12: How do you test code that relies heavily on `DateTime.Now` or Random numbers?

**Answer:** Time and Randomness are inherently non-deterministic, making them the enemy of unit testing. If a method does `if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)`, it will pass only on Fridays.

**Solution: Abstract the dependency.**
You should never use `DateTime.Now` directly in business logic. Inject a time provider interface.

```csharp
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

// Production Implementation
public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

// In the code being tested:
public class ReportGenerator
{
    private readonly IDateTimeProvider _timeProvider;
    public ReportGenerator(IDateTimeProvider timeProvider) => _timeProvider = timeProvider;
    
    public bool IsEligible() => _timeProvider.UtcNow.DayOfWeek == DayOfWeek.Friday;
}
```

In the test, you mock `IDateTimeProvider` to return a specific, hardcoded Friday.
*(Note: .NET 8 introduced a built-in `TimeProvider` abstract class specifically for this purpose to standardize time abstractions!)*

---

### Q13: Explain Structural Equality vs. Referential Equality in Assertions. How do C# 9 `records` simplify testing?

**Answer:** 
- **Referential Equality:** Asserts that two variables point to the exact same object instance in memory (`object.ReferenceEquals`).
- **Structural Equality:** Asserts that two different instances contain the exact same data values.

Historically, testing returned objects was tedious in C#:
```csharp
// Act
var user = service.GetUser();

// Assert with standard class (Classes default to Referential equality)
// Assert.Equal(new User { Name = "John" }, user); // THIS FAILS! Different memory addresses.

// To fix, you had to assert every property manually:
Assert.Equal("John", user.Name);
Assert.Equal(25, user.Age);
```

**C# 9+ Records:**
`record` types inherently provide deeply compiled **Value-Based (Structural) Equality**. If your DTOs or domain models are `records`, the assertion becomes elegantly simple:
```csharp
public record UserRecord(string Name, int Age);

// Act
var user = service.GetUserRecord();

// Assert
// THIS PASSES! xUnit calls .Equals(), which the record overrides to check properties automatically.
Assert.Equal(new UserRecord("John", 25), user);
```

---

### Q14: Discuss common Anti-Patterns or Code Smells in Unit Tests, and how to avoid them.

**Answer:** Senior engineers should identify not just bad code, but bad tests.

1. **The Fragile Test:** A test that breaks easily due to minor changes in implementation details rather than changes in behavior. *Fix:* Avoid testing private state. Test the public API contract (behavior).
2. **The Obscure Test (Over-mocking):** A test where the Arrange phase has 50 lines of complex Mock setups, making it impossible to read. *Fix:* The class under test is too coupled (violates SRP). Break the class down into smaller components.
3. **The Assertion Roulette:** A single test with 15 different Assert statements verifying entirely different things. If the first fails, the test aborts, hiding the other failures. *Fix:* Split into multiple focused tests (One logical assertion per test).
4. **The False Positive:** Tests that always pass because they either lack an assertion, or the mock is set up to return the exact expected value directly into the assertion, completely bypassing the actual logic.
5. **Slow Tests:** Including File I/O, `Thread.Sleep()`, or Database calls in unit tests. *Fix:* Isolate these dependencies. Unit tests must be lightning fast.

---

### Q15: What is Mutation Testing, and how does it relate to Unit Testing?

**Answer:** While Code Coverage tells you *what* code was executed, Mutation Testing proves *how good* your tests actually are.

**How it works:** A tool (like **Stryker.NET** for C#) automatically modifies ("mutates") your production code in small ways:
- Changes `a + b` to `a - b`
- Changes `if (x > 0)` to `if (x >= 0)`
- Changes a boolean return to its opposite.

After applying a mutation, it runs your unit tests. If your tests **fail**, the mutant is "killed" (this is good; your tests caught the bug). If your tests **pass**, the mutant "survived" (this is bad; your tests executed the code but didn't actually assert the logic correctly).

*Senior Insight:* It is the ultimate defense against false-positive tests and a highly sophisticated practice reserved for mature engineering teams looking to bullet-proof critical domain logic.
