# C# Logical Thinking & Algorithmic Coding

> These test your problem-solving approach, not memorisation. Talk out loud!

---

## How to Approach Live Coding in C#

```
1. Understand   → Restate the problem in your own words
2. Clarify      → Ask about edge cases, nulls, empty inputs, performance
3. Plan         → "I'll use a Dictionary for O(1) lookup because..."
4. Code         → Write readable code with meaningful names
5. Test         → Walk through with an example
6. Optimise     → "Could be improved by..."
```

---

## String Manipulation Problems

### Q1: "Reverse a string without using Reverse()."

```csharp
// Approach: two pointers
public static string ReverseString(string input)
{
    if (string.IsNullOrEmpty(input)) return input;
    
    char[] chars = input.ToCharArray();
    int left = 0, right = chars.Length - 1;
    
    while (left < right)
    {
        (chars[left], chars[right]) = (chars[right], chars[left]); // C# tuple swap
        left++;
        right--;
    }
    
    return new string(chars);
}

// Tests:
// ReverseString("hello") → "olleh"
// ReverseString("") → ""
// ReverseString(null) → null
// ReverseString("a") → "a"
```

**Talk through**: "O(n) time, O(n) space for the char array. If strings were mutable we'd do it in-place. In C# strings are immutable so we must use a char array."

---

### Q2: "Check if a string is a palindrome."

```csharp
// Approach: two pointers, ignore case and non-alphanumeric
public static bool IsPalindrome(string s)
{
    if (string.IsNullOrEmpty(s)) return true;
    
    int left = 0, right = s.Length - 1;
    
    while (left < right)
    {
        // Skip non-alphanumeric characters
        while (left < right && !char.IsLetterOrDigit(s[left])) left++;
        while (left < right && !char.IsLetterOrDigit(s[right])) right--;
        
        if (char.ToLower(s[left]) != char.ToLower(s[right]))
            return false;
        
        left++;
        right--;
    }
    
    return true;
}

// Tests:
// IsPalindrome("racecar") → true
// IsPalindrome("A man, a plan, a canal: Panama") → true
// IsPalindrome("hello") → false
```

---

### Q3: "Find all duplicate characters in a string."

```csharp
public static Dictionary<char, int> FindDuplicates(string input)
{
    if (string.IsNullOrEmpty(input)) 
        return new Dictionary<char, int>();
    
    // Count frequency using Dictionary
    var freq = new Dictionary<char, int>();
    foreach (char c in input)
        freq[c] = freq.GetValueOrDefault(c, 0) + 1;
    
    // Return only duplicates (count > 1)
    return freq.Where(kvp => kvp.Value > 1)
               .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}

// LINQ version (concise, good to know):
public static IEnumerable<char> FindDuplicatesLinq(string input)
    => input?.GroupBy(c => c)
             .Where(g => g.Count() > 1)
             .Select(g => g.Key)
       ?? Enumerable.Empty<char>();
```

---

## Collection / Array Problems

### Q4: "Find the two numbers in an array that sum to a target."

```csharp
// Naive O(n²) — mention it first, then optimise
public static (int, int)? TwoSumNaive(int[] nums, int target)
{
    for (int i = 0; i < nums.Length; i++)
        for (int j = i + 1; j < nums.Length; j++)
            if (nums[i] + nums[j] == target)
                return (nums[i], nums[j]);
    return null;
}

// Optimised O(n) using HashSet
public static (int, int)? TwoSum(int[] nums, int target)
{
    if (nums == null || nums.Length < 2) return null;
    
    var seen = new HashSet<int>();
    foreach (int num in nums)
    {
        int complement = target - num;
        if (seen.Contains(complement))
            return (complement, num);
        seen.Add(num);
    }
    return null;
}

// Tests:
// TwoSum([2, 7, 11, 15], 9) → (2, 7)
// TwoSum([3, 2, 4], 6) → (2, 4)
```

**Talk through**: "First approach is O(n²) — works but slow. Better: use a HashSet to store what we've seen. For each number, check if its complement exists in the set. O(n) time, O(n) space."

---

### Q5: "Find the most frequent element in a list."

```csharp
public static T MostFrequent<T>(IEnumerable<T> items)
{
    if (items == null || !items.Any())
        throw new ArgumentException("Collection is null or empty");
    
    return items.GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
}

// Without LINQ (shows you understand the mechanics):
public static T MostFrequentManual<T>(IList<T> items)
{
    var freq = new Dictionary<T, int>();
    foreach (var item in items)
        freq[item] = freq.GetValueOrDefault(item, 0) + 1;
    
    T maxItem = items[0];
    int maxCount = 0;
    foreach (var kvp in freq)
    {
        if (kvp.Value > maxCount)
        {
            maxCount = kvp.Value;
            maxItem = kvp.Key;
        }
    }
    return maxItem;
}
```

---

### Q6: "Remove duplicates from a list while preserving order."

```csharp
// HashSet preserves insertion order awareness, List preserves order
public static List<T> RemoveDuplicates<T>(IEnumerable<T> items)
{
    var seen = new HashSet<T>();
    var result = new List<T>();
    
    foreach (var item in items)
    {
        if (seen.Add(item)) // Add returns false if already present
            result.Add(item);
    }
    return result;
}

// LINQ version:
public static IEnumerable<T> RemoveDuplicatesLinq<T>(IEnumerable<T> items)
    => items.Distinct(); // preserves order in most implementations

// Tests:
// [1, 2, 3, 2, 4, 1] → [1, 2, 3, 4]
```

---

## OOP / Class Design Problems

### Q7: "Design a simple Stack class using a generic list."

```csharp
public class Stack<T>
{
    private readonly List<T> _data = new List<T>();
    
    public void Push(T item) => _data.Add(item);
    
    public T Pop()
    {
        if (IsEmpty) throw new InvalidOperationException("Stack is empty");
        var item = _data[^1]; // C# 8 index from end
        _data.RemoveAt(_data.Count - 1);
        return item;
    }
    
    public T Peek()
    {
        if (IsEmpty) throw new InvalidOperationException("Stack is empty");
        return _data[^1];
    }
    
    public bool IsEmpty => _data.Count == 0;
    public int Count => _data.Count;
    
    public override string ToString()
        => string.Join(", ", _data);
}

// Usage:
// var stack = new Stack<int>();
// stack.Push(1); stack.Push(2); stack.Push(3);
// stack.Pop() → 3
// stack.Peek() → 2
```

---

### Q8: "Implement a generic Pair / Tuple class."

```csharp
public class Pair<TFirst, TSecond>
{
    public TFirst First { get; }
    public TSecond Second { get; }
    
    public Pair(TFirst first, TSecond second)
    {
        First = first;
        Second = second;
    }
    
    public void Deconstruct(out TFirst first, out TSecond second)
    {
        first = First;
        second = Second;
    }
    
    public Pair<TSecond, TFirst> Swap() => new Pair<TSecond, TFirst>(Second, First);
    
    public override string ToString() => $"({First}, {Second})";
    
    public override bool Equals(object obj)
        => obj is Pair<TFirst, TSecond> other
           && EqualityComparer<TFirst>.Default.Equals(First, other.First)
           && EqualityComparer<TSecond>.Default.Equals(Second, other.Second);
    
    public override int GetHashCode() => HashCode.Combine(First, Second);
}
```

> **Say**: "I implemented `Deconstruct` to support C# pattern matching syntax `var (a, b) = pair;`, `Equals` and `GetHashCode` together so it works correctly in collections."

---

## LINQ Coding Challenges

### Q9: "Given a list of transactions, find the top 5 investors by total buy amount in the last 30 days."

```csharp
public class Transaction
{
    public int InvestorId { get; set; }
    public string InvestorName { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } // "BUY" or "SELL"
    public DateTime Date { get; set; }
}

public static IEnumerable<(string Name, decimal TotalBuy)> 
    TopInvestorsByBuyVolume(IEnumerable<Transaction> transactions, int topN = 5)
{
    var cutoff = DateTime.Today.AddDays(-30);
    
    return transactions
        .Where(t => t.Type == "BUY" && t.Date >= cutoff)
        .GroupBy(t => new { t.InvestorId, t.InvestorName })
        .Select(g => (
            Name: g.Key.InvestorName,
            TotalBuy: g.Sum(t => t.Amount)
        ))
        .OrderByDescending(x => x.TotalBuy)
        .Take(topN);
}
```

---

### Q10: "Flatten a nested list and find distinct values."

```csharp
// Scenario: Each fund has a list of investor IDs (some investors in multiple funds)
var fundInvestors = new List<List<int>>
{
    new List<int> { 1, 2, 3, 4 },
    new List<int> { 3, 4, 5, 6 },
    new List<int> { 5, 6, 7, 8 }
};

// All unique investors across all funds:
var uniqueInvestors = fundInvestors
    .SelectMany(list => list)   // flatten
    .Distinct()                  // deduplicate
    .OrderBy(x => x)             // sort
    .ToList();

// Result: [1, 2, 3, 4, 5, 6, 7, 8]

// Investors in ALL funds (intersection of all lists):
var investorsInAllFunds = fundInvestors
    .Aggregate((a, b) => a.Intersect(b).ToList());
// Result: [] (no investor in all 3)
```

---

## Async / Await Coding

### Q11: "Write an async method that fetches data from multiple sources in parallel."

```csharp
public class FundDataService
{
    private readonly HttpClient _httpClient;
    
    // Pattern 1: Sequential (slow)
    public async Task<(decimal nav, decimal benchmark)> GetDataSequentialAsync(int fundId)
    {
        var nav = await GetNavAsync(fundId);       // waits for this...
        var benchmark = await GetBenchmarkAsync(fundId); // ...then this
        return (nav, benchmark);
    }
    
    // Pattern 2: Parallel with Task.WhenAll (fast)
    public async Task<(decimal nav, decimal benchmark)> GetDataParallelAsync(int fundId)
    {
        var navTask = GetNavAsync(fundId);           // both start
        var benchmarkTask = GetBenchmarkAsync(fundId); // immediately
        
        await Task.WhenAll(navTask, benchmarkTask);  // wait for both
        
        return (await navTask, await benchmarkTask);
    }
    
    // Pattern 3: With cancellation token (production quality)
    public async Task<decimal> GetNavAsync(int fundId, CancellationToken ct = default)
    {
        // Pass ct to every awaitable operation
        var response = await _httpClient.GetAsync($"/api/nav/{fundId}", ct);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(ct);
        return decimal.Parse(content);
    }
    
    private Task<decimal> GetBenchmarkAsync(int fundId) 
        => Task.FromResult(100.0m); // stub
}
```

> **Say**: "Task.WhenAll is the key pattern for parallel async operations — both tasks start without waiting for each other. Always pass CancellationToken through for proper timeout and cancellation support."

---

### Q12: "What's wrong with this async code?"

```csharp
// BUGGY CODE (they may show you this):
public class ReportController : Controller
{
    public ActionResult GetReport(int id)
    {
        var report = GetReportAsync(id).Result; // DEADLOCK!
        return View(report);
    }
    
    private async Task<Report> GetReportAsync(int id)
    {
        await Task.Delay(100); // simulate async work
        return new Report { Id = id };
    }
}
```

**Explain the bug**:
> "`.Result` blocks the thread synchronously. In ASP.NET Classic, the continuation needs the same synchronization context thread that `.Result` is blocking. This causes a deadlock. Fix 1: Make `GetReport` async and `await` the task. Fix 2: Use `ConfigureAwait(false)` in the library code. Fix 3: Never block on async code with `.Result` or `.Wait()`."

```csharp
// FIXED:
public async Task<ActionResult> GetReport(int id)
{
    var report = await GetReportAsync(id); // no deadlock
    return View(report);
}
```

---

## Exception Handling Patterns

### Q13: "Implement a retry mechanism for transient failures."

```csharp
public static async Task<T> RetryAsync<T>(
    Func<Task<T>> operation,
    int maxRetries = 3,
    TimeSpan? delay = null,
    Func<Exception, bool> shouldRetry = null)
{
    delay ??= TimeSpan.FromSeconds(1);
    shouldRetry ??= ex => ex is HttpRequestException or TimeoutException;
    
    var exceptions = new List<Exception>();
    
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (shouldRetry(ex))
        {
            exceptions.Add(ex);
            
            if (attempt == maxRetries)
                break;
            
            Console.WriteLine($"Attempt {attempt} failed: {ex.Message}. Retrying in {delay}...");
            await Task.Delay(delay.Value * attempt); // exponential backoff
        }
    }
    
    throw new AggregateException($"Operation failed after {maxRetries} attempts", exceptions);
}

// Usage:
// var result = await RetryAsync(() => httpClient.GetStringAsync("/api/nav/1"));
```

> **Say**: "The `when` clause in catch is a C# filter — the exception is only caught if the condition is true. Exponential backoff multiplies the delay by the attempt number, avoiding hammering a struggling service."

---

## Generics & Interfaces

### Q14: "Write a generic Result/Either type for error handling without exceptions."

```csharp
// Functional error handling pattern (used in modern .NET APIs)
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    
    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }
    
    private Result(string error)
    {
        IsSuccess = false;
        Error = error;
    }
    
    public static Result<T> Success(T value) => new Result<T>(value);
    public static Result<T> Failure(string error) => new Result<T>(error);
    
    // Railway-oriented programming style
    public Result<TNext> Map<TNext>(Func<T, TNext> transform)
        => IsSuccess 
           ? Result<TNext>.Success(transform(Value))
           : Result<TNext>.Failure(Error);
    
    public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> transform)
        => IsSuccess ? transform(Value) : Result<TNext>.Failure(Error);
    
    public override string ToString()
        => IsSuccess ? $"Success({Value})" : $"Failure({Error})";
}

// Usage:
public Result<Trade> ValidateAndProcess(TradeRequest request)
{
    if (request == null) 
        return Result<Trade>.Failure("Request cannot be null");
    if (request.Amount <= 0) 
        return Result<Trade>.Failure("Amount must be positive");
    
    var trade = new Trade { Amount = request.Amount };
    return Result<Trade>.Success(trade);
}

// Chaining:
var result = ValidateAndProcess(request)
    .Map(trade => new TradeDto { Id = trade.Id, Amount = trade.Amount });
```

---

## Debugging Challenge: Common C# Gotchas

### Bug 1: Value type in foreach (can't modify)
```csharp
struct Point { public int X; public int Y; }

var points = new List<Point> { new Point { X = 1, Y = 2 } };
// foreach (var p in points) p.X = 10; // compile error — p is a copy
// Fix: use index
for (int i = 0; i < points.Count; i++)
    points[i] = new Point { X = 10, Y = points[i].Y };
```

### Bug 2: Closure captures variable by reference
```csharp
var actions = new List<Action>();
for (int i = 0; i < 3; i++)
    actions.Add(() => Console.WriteLine(i)); // captures reference to i!

foreach (var a in actions) a(); // Prints 3, 3, 3 NOT 0, 1, 2

// Fix: capture a local copy
for (int i = 0; i < 3; i++)
{
    int localI = i;
    actions.Add(() => Console.WriteLine(localI)); // each closure gets its own copy
}
```

### Bug 3: String comparison
```csharp
string a = "Hello";
string b = "Hello";
Console.WriteLine(a == b);         // true (value equality for literals)
Console.WriteLine(a.Equals(b));    // true

string c = new string("Hello".ToCharArray());
Console.WriteLine(object.ReferenceEquals(a, c)); // false (different references)
Console.WriteLine(a == c);         // true (string overloads ==)

// Safer comparison for user input:
string input = "HELLO";
bool match = string.Equals(input, "hello", StringComparison.OrdinalIgnoreCase);
```

### Bug 4: null reference in LINQ
```csharp
List<string> items = null;
// items.Where(x => x.Length > 3) // NullReferenceException!

// Safe pattern:
var result = items?.Where(x => x?.Length > 3) ?? Enumerable.Empty<string>();
```

---

## Big-O Quick Reference

| Operation | Data Structure | Complexity |
|-----------|---------------|------------|
| Add/Remove end | List<T> | O(1) amortized |
| Insert middle | List<T> | O(n) |
| Lookup by key | Dictionary<K,V> | O(1) average |
| Contains | HashSet<T> | O(1) average |
| Binary search | sorted array/list | O(log n) |
| Sort | Array.Sort / LINQ OrderBy | O(n log n) |
| LINQ Where | any collection | O(n) |
| LINQ GroupBy | any collection | O(n) |

**Talk about this**: "I choose Dictionary/HashSet when I need O(1) lookups. I use List when I need ordered access. If I need both fast lookup and ordering, a SortedDictionary gives O(log n) for both."

---

## Interview Pattern: How to Handle "I Don't Know"

If stuck:
1. **Explain what you DO know**: "I know this involves string manipulation..."
2. **Talk through your approach**: "I'd start with a dictionary to count frequencies..."
3. **Write pseudocode first**: "Something like: for each char, increment count..."
4. **Ask for a hint gracefully**: "Could you point me in the right direction on the data structure?"

They care MORE about your thinking process than getting the perfect answer.
