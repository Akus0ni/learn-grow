# How to Crack a Coding Interview

> Based on Gayle Laakmann McDowell's guide — [YouTube Video](https://youtu.be/cfjvEy-GK8o?si=VT0dlJNKYXt_pQU8)
> Author: Founder & CEO of CareerCup.com | Ex-Google, Microsoft, Apple engineer

---

## Overview

Cracking a coding interview isn't about memorizing solutions — it's about developing a **repeatable problem-solving process** that demonstrates how you think. This guide covers the 7-step approach, optimization techniques, essential knowledge, and preparation strategy.

---

## The 7-Step Problem-Solving Framework

Use this exact sequence for every coding problem in an interview.

### Step 1 — Listen

- Absorb **every detail** in the problem statement. Nothing is mentioned by accident.
- Note special conditions: Is the array sorted? Can there be duplicates? What's the input size?
- Ask clarifying questions before diving in.
- Do **not** start coding until you fully understand the problem (and ideally until the interviewer asks you to code).

> Key insight: The details given often hint at the optimal solution. A sorted array hints at binary search. Duplicates hint at hash tables.

---

### Step 2 — Draw an Example

- Create a **large, specific, and non-trivial** example.
- Avoid examples that are too small (easy to miss edge cases) or too special-case (e.g., all same values).
- Use the example to manually trace through what the solution should do.

**Bad example for "Intersection of Two Sorted Arrays":**
```
A = [1, 2]
B = [2, 3]
```

**Good example:**
```
A = [1, 3, 5, 7, 9, 11]
B = [2, 3, 6, 7, 10, 11, 13]
Expected Output: [3, 7, 11]
```

---

### Step 3 — State the Brute Force

- Always state a **naive solution first**, no matter how slow or ugly.
- You don't need to code it — just explain it out loud.
- This proves you can solve the problem and gives you a baseline to optimize from.

> "Stupid and terrible is okay at this stage." — Gayle McDowell

- Mention the **time and space complexity** of your brute force.

---

### Step 4 — Optimize

Apply the **BUD technique** (see section below) plus other strategies to find a better approach.

Optimization strategies:
- Look for **BUD** (Bottlenecks, Unnecessary work, Duplicated work)
- Think about **space/time trade-offs** — can you use extra memory to save time?
- Try **precomputing** or caching results
- Consider **other data structures** — hash tables, heaps, trees, tries
- Think about the **best conceivable runtime** (BCR) — what's the theoretical lower bound?

---

### Step 5 — Walk Through Your Solution

- Before writing a single line of code, walk through the full algorithm in your head or on the whiteboard.
- Identify all variables and when they change.
- Make sure you understand every step of your approach.

> This prevents getting stuck mid-code and helps catch logical errors early.

---

### Step 6 — Write Beautiful Code

Write clean, production-quality code:

| Practice | Why It Matters |
|---|---|
| Modularize your code | Shows good engineering instincts |
| Use meaningful variable names | Makes intent clear |
| Handle edge cases explicitly | Shows thoroughness |
| Write helper functions | Keeps logic readable |
| Avoid magic numbers | Use constants or named variables |

```
// Bad
for (int i = 0; i < a.length - 1; i++) { ... }

// Better
int lastIndex = array.length - 1;
for (int i = 0; i < lastIndex; i++) { ... }
```

---

### Step 7 — Test

Never declare "I'm done" without testing. Use this sequence:

1. **Conceptual check** — Walk through your code line by line. Does the logic make sense?
2. **Weird edge cases** — Empty input, single element, null, negative numbers, very large input
3. **Hot spots** — Off-by-one errors, null pointer dereferences, integer overflow
4. **Small test cases** — Manually run a 2–3 element example through the code
5. **Special/stress cases** — All duplicates, already sorted, reversed input

> Fix bugs by re-evaluating the logic, not by patching with if-statements.

---

## BUD Optimization Technique

The most powerful framework for improving a brute-force solution.

### B — Bottleneck

Find the step in your algorithm with the **worst time complexity** and focus optimization there.

**Example:** If your algorithm is O(n log n) for sorting + O(n²) for the search phase, focus on the search.

---

### U — Unnecessary Work

Eliminate work your algorithm does that **doesn't contribute** to the final result.

**Example:** In a nested loop searching for a pair, if you've already found the answer, break early. If you're searching both (a,b) and (b,a), do half the work.

---

### D — Duplicated Work

Spot places where you're **computing the same thing multiple times** and cache it.

**Example:**
```
// Duplicated: recomputing sum(arr[0..i]) on every iteration
// Fix: Precompute prefix sums once in O(n), then answer queries in O(1)
```

---

## Essential Knowledge

Master these before your interview. Everything else builds on top.

### Data Structures

| Structure | When to Use | Key Operations |
|---|---|---|
| **Array / ArrayList** | Random access, iteration | O(1) access, O(n) insert |
| **Hash Table** | Fast lookup, deduplication | O(1) avg get/put |
| **Linked List** | Dynamic size, frequent insert/delete | O(n) access |
| **Stack** | LIFO, backtracking, expression parsing | O(1) push/pop |
| **Queue** | FIFO, BFS | O(1) enqueue/dequeue |
| **Tree / BST** | Hierarchical data, ordered data | O(log n) avg operations |
| **Trie** | Prefix search, autocomplete | O(m) per operation |
| **Graph** | Networks, relationships | Varies |
| **Heap / Priority Queue** | Top-K problems, scheduling | O(log n) insert/extract |

---

### Algorithms

| Algorithm | Time Complexity | Key Use Case |
|---|---|---|
| **Binary Search** | O(log n) | Sorted arrays |
| **BFS** | O(V + E) | Shortest path (unweighted) |
| **DFS** | O(V + E) | Tree traversal, cycle detection |
| **Merge Sort** | O(n log n) | Stable sort |
| **Quick Sort** | O(n log n) avg | In-place sort |
| **Dynamic Programming** | Varies | Overlapping subproblems |
| **Two Pointers** | O(n) | Sorted arrays, pairs |
| **Sliding Window** | O(n) | Subarrays, substrings |
| **Backtracking** | Exponential | Combinations, permutations |

---

### Must-Know Concepts

- **Big O notation** — time and space complexity for every solution you write
- **Recursion** — understand call stack, base cases, and recurrence relations
- **Memoization / Dynamic Programming** — top-down vs bottom-up
- **Bit manipulation** — AND, OR, XOR, shifts, checking/setting bits

---

## Five Algorithm Approaches (When You're Stuck)

Use these when you don't immediately see a solution:

### 1. Exemplify
Run through more examples. The pattern often becomes obvious.

### 2. Pattern Matching
What problems does this **resemble**? Similar problems often use similar techniques.

### 3. Simplify and Generalize
Solve a simplified/constrained version first, then generalize.

### 4. Base Case and Build
Solve for n=1, then n=2, build up. Often leads to recursive or DP solutions.

### 5. Data Structure Brainstorm
Run through each data structure and ask: "Could a hash table help? A heap? A tree?"

---

## The Interview Mindset

### What Interviewers Actually Evaluate

| Dimension | What They Look For |
|---|---|
| **Analytical skill** | How do you break down problems? Do you think through trade-offs? |
| **Coding skill** | Is your code clean, correct, and organized? |
| **Communication** | Do you explain your thinking clearly as you go? |
| **Knowledge** | Do you know CS fundamentals? |
| **Culture fit** | Are you someone they'd enjoy working with? |

### Communication Tips

- **Think out loud** — narrate your thought process continuously
- **State your assumptions** — don't silently assume, say it
- **Check in with the interviewer** — "Does this approach make sense?" before coding
- **Don't freeze** — if stuck, say so and try a different approach out loud
- **Be honest** — saying "I'm not sure, let me think" is better than guessing silently

### What to Do When Stuck

1. Go back to your example — trace through it manually
2. Try a different data structure
3. Think about what information you're not using
4. Ask: "What's the best conceivable runtime?" — reverse-engineer from there
5. Look for BUD in your current approach

---

## Behavioral Questions

Technical skill alone won't get you hired. Prepare STAR-format stories.

### STAR Format

| Component | Description |
|---|---|
| **S**ituation | Brief context — the project, team, problem |
| **T**ask | Your specific responsibility |
| **A**ction | Exactly what YOU did (not "we") |
| **R**esult | Measurable outcome |

### Common Behavioral Topics

- A challenging technical problem you solved
- A conflict with a teammate
- A time you failed and what you learned
- A time you showed leadership
- A time you had to make a decision with incomplete information

---

## Preparation Strategy

### What to Actually Practice

1. **Implement core data structures from scratch** — linked list, hash table, BST, graph
2. **Implement core algorithms from scratch** — BFS/DFS, merge sort, binary search
3. **Master Big O** — be able to derive complexity on the fly for any code snippet
4. **Solve 50–100 problems** on LeetCode/HackerRank — focus on medium difficulty
5. **Practice on paper/whiteboard** — not just in an IDE with autocomplete
6. **Do mock interviews** — with peers or on platforms like Pramp, Interviewing.io

### Study Schedule

| Phase | Duration | Focus |
|---|---|---|
| **Foundation** | Weeks 1–2 | Data structures + algorithms (theory + implementation) |
| **Practice** | Weeks 3–6 | Easy + Medium LeetCode problems (2–3/day) |
| **Simulate** | Weeks 7–8 | Timed mock interviews, Hard problems |
| **Review** | Week 9+ | Weak areas, behavioral questions, company-specific prep |

### Recommended Problem Order

1. Arrays & Strings
2. Linked Lists
3. Stacks & Queues
4. Trees & Graphs
5. Recursion & Dynamic Programming
6. Sorting & Searching
7. System Design (for senior roles)

---

## Quick Reference Cheatsheet

```
INTERVIEW FLOW
══════════════
1. Listen carefully → ask clarifying questions
2. Draw a large, general example
3. State brute force (out loud, don't code)
4. Optimize using BUD + other techniques
5. Walk through full solution before coding
6. Write clean, modular code
7. Test with edge cases + trace through code

BUD OPTIMIZATION
════════════════
B - Bottleneck: worst complexity step
U - Unnecessary work: remove useless computation
D - Duplicated work: cache repeated results

WHEN STUCK
══════════
→ More examples
→ Pattern match to known problems
→ Simplify the problem
→ Base case and build
→ Brainstorm data structures
→ Think: "what's the BCR?"

DATA STRUCTURE CHEATSHEET
══════════════════════════
Hash Table  → O(1) lookup   → frequency, dedup
Heap        → O(log n)      → top-K, min/max
Tree/BST    → O(log n)      → sorted data
Trie        → O(m)          → prefix/string search
Graph       → BFS/DFS       → connectivity, path
Stack       → LIFO          → matching, undo
Queue       → FIFO          → BFS, scheduling
```

---

## Resources

| Resource | Link |
|---|---|
| Original Video Guide | [YouTube — cfjvEy-GK8o](https://youtu.be/cfjvEy-GK8o?si=VT0dlJNKYXt_pQU8) |
| Cracking the Coding Interview (Book) | [crackingthecodinginterview.com](https://www.crackingthecodinginterview.com/) |
| LeetCode Practice | [leetcode.com](https://leetcode.com) |
| Tech Interview Handbook | [techinterviewhandbook.org](https://www.techinterviewhandbook.org) |
| NeetCode (YouTube) | [youtube.com/@NeetCode](https://www.youtube.com/@NeetCode) |
| Pramp (Mock Interviews) | [pramp.com](https://www.pramp.com) |
