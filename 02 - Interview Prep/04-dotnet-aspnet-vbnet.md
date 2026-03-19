# .NET, ASP.NET & VB.NET — Interview Prep

---

## Your .NET Position

Your resume shows: **C#, .NET across eGain (2021–2025) and Energy Exemplar (2025–2026)**.

The JD asks for: **ASP.NET, VB.NET**

**The bridge:** ASP.NET is part of the .NET ecosystem you already work in. VB.NET is a legacy language on the same CLR — the JD mentions it because this role likely involves maintaining or migrating older VB.NET codebases.

---

## ASP.NET Variants (Know These)

| Framework | What It Is | Status |
|---|---|---|
| **ASP.NET Web Forms** | Event-driven, drag-drop UI (2002) | Legacy |
| **ASP.NET MVC** | MVC pattern on .NET Framework | Legacy/maintained |
| **ASP.NET Web API** | RESTful API framework | Your territory (eGain) |
| **ASP.NET Core** | Cross-platform, modern .NET 5/6/7/8 | Current standard |

**Your experience maps to:** ASP.NET Web API — you built RESTful APIs at eGain ("Designed and implemented efficient RESTful APIs"). This is ASP.NET development.

---

## VB.NET — What You Need to Know

VB.NET and C# compile to the same IL (Intermediate Language) and run on the same CLR. The syntax is different but the capabilities are identical.

### Key Syntax Differences (C# → VB.NET)

| Concept | C# | VB.NET |
|---|---|---|
| Class declaration | `class Foo {}` | `Class Foo ... End Class` |
| Method | `void Bar() {}` | `Sub Bar() ... End Sub` |
| Function (return value) | `string Baz()` | `Function Baz() As String` |
| Variable | `var x = 5;` | `Dim x As Integer = 5` |
| If statement | `if (x > 0) {}` | `If x > 0 Then ... End If` |
| Loops | `for (int i=0; i<10; i++)` | `For i As Integer = 0 To 9 ... Next` |
| String interpolation | `$"Hello {name}"` | `$"Hello {name}"` (same) |
| Null check | `x ?? defaultVal` | `If(x, defaultVal)` |
| Comments | `// comment` | `' comment` |

### VB.NET in the Real World
- Heavily used in enterprise .NET apps built pre-2010
- Common in banking, insurance, healthcare, legacy government systems
- **This JD likely involves legacy VB.NET apps being modernized to AWS** — that's the whole point of App2Container + CAST

**Your honest answer if asked about VB.NET:**
> "My primary .NET experience is in C#, which shares the same runtime and libraries as VB.NET. The syntax differences are straightforward to navigate — I've reviewed VB.NET code before and can work with it. More importantly, I understand the modernization story: these are often older apps that need to be containerized or migrated to AWS, which is exactly what App2Container and Amazon Q code transformation address."

---

## ASP.NET Web API — Your Core Strength

You built RESTful APIs at eGain. Here's how to articulate it technically:

### What Makes a Good REST API (You Should Know Cold)
- **HTTP verbs:** GET (read), POST (create), PUT/PATCH (update), DELETE
- **Status codes:** 200 OK, 201 Created, 400 Bad Request, 401 Unauthorized, 404 Not Found, 500 Internal Server Error
- **Stateless:** Each request carries all needed info (no server-side session)
- **JSON serialization:** `System.Text.Json` (modern) or `Newtonsoft.Json`

### ASP.NET Core API Example (Know the Pattern)
```csharp
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReport(int id)
    {
        var report = await _service.GetByIdAsync(id);
        if (report == null) return NotFound();
        return Ok(report);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] ReportDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetReport), new { id = result.Id }, result);
    }
}
```

### Middleware Pipeline (ASP.NET Core)
```csharp
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRouting();
app.MapControllers();
```
Middleware runs in order — authentication before authorization, always.

---

## React vs Vue.js (Bridging the Gap)

JD asks for React. Your resume shows Vue.js. These are both component-based SPA frameworks.

### Component Model Comparison

**Vue.js (your experience):**
```vue
<template>
  <div>{{ message }}</div>
</template>
<script>
export default {
  data() { return { message: 'Hello' } }
}
</script>
```

**React (equivalent):**
```jsx
function MyComponent() {
  const [message, setMessage] = React.useState('Hello');
  return <div>{message}</div>;
}
```

### Key React Concepts
- **JSX** — HTML-like syntax in JavaScript
- **useState** — local component state
- **useEffect** — side effects (API calls, subscriptions)
- **Props** — parent-to-child data passing
- **Component lifecycle** — mount, update, unmount

**Your honest answer:**
> "My frontend framework experience is primarily with Vue.js, which I've used in production at both eGain and Energy Exemplar. React and Vue.js share the same component-based mental model — virtual DOM, props, state management, lifecycle hooks. The ecosystem differs but the concepts transfer directly. I'm confident I can get productive with React quickly."

---

## Interview Questions

**Q: What's the difference between .NET Framework and .NET Core/.NET 5+?**
> .NET Framework: Windows-only, full-featured legacy platform. .NET Core/.NET 5+: cross-platform (Windows/Linux/macOS), open-source, containerization-friendly, better performance. Modern development targets .NET 8+.

**Q: How do you handle authentication in ASP.NET?**
> JWT bearer tokens with ASP.NET Identity or IdentityServer. Middleware validates the token on each request. At IKS Health, I implemented JWT-based auth for the WFM platform.

**Q: What is middleware in ASP.NET Core?**
> Components in the request pipeline. Each middleware can short-circuit or pass to the next. Common: authentication, logging, exception handling, CORS, routing.

**Q: How do you deploy ASP.NET apps to AWS?**
> Options: Elastic Beanstalk (PaaS, easiest), ECS/EKS (containers), EC2 (manual control). For containerized .NET: build with multi-stage Dockerfile, push to ECR, deploy via ECS task definition. App2Container automates this for existing IIS apps.
