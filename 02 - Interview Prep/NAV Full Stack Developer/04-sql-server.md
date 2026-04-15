# SQL Server — Interview Q&A

---

## Core SQL Concepts

**Q: What are the different types of JOINs?**

```sql
-- INNER JOIN: Only matching rows from both tables
SELECT o.Id, c.Name
FROM Orders o
INNER JOIN Clients c ON o.ClientId = c.Id

-- LEFT JOIN: All rows from left, matching from right (NULL if no match)
SELECT o.Id, c.Name
FROM Orders o
LEFT JOIN Clients c ON o.ClientId = c.Id

-- RIGHT JOIN: All rows from right, matching from left
-- FULL OUTER JOIN: All rows from both, NULL where no match
-- CROSS JOIN: Cartesian product (every combination)
-- SELF JOIN: Table joined to itself (e.g., employee-manager hierarchy)
```

> **Interview tip:** Be ready to draw a Venn diagram mentally.

---

**Q: What is the difference between WHERE and HAVING?**

A:
- `WHERE` — Filters rows **before** grouping. Cannot use aggregate functions.
- `HAVING` — Filters groups **after** `GROUP BY`. Can use aggregate functions.

```sql
SELECT ClientId, SUM(Amount) as Total
FROM Orders
WHERE Status = 'Completed'        -- filters rows first
GROUP BY ClientId
HAVING SUM(Amount) > 10000        -- filters groups after aggregation
```

---

**Q: What is the difference between DELETE, TRUNCATE, and DROP?**

| | DELETE | TRUNCATE | DROP |
|---|---|---|---|
| Removes | Specific rows | All rows | Entire table |
| WHERE clause | ✅ | ❌ | ❌ |
| Rollback | ✅ (logged) | ✅ (minimal logging) | ❌ |
| Triggers | ✅ | ❌ | ❌ |
| Resets identity | ❌ | ✅ | N/A |
| DDL/DML | DML | DDL | DDL |

---

**Q: What is an index? What are the types?**

A: An index speeds up data retrieval by creating a data structure (B-tree) that allows fast lookups.

**Types:**
- **Clustered Index** — Physically sorts table data by indexed column. One per table (usually Primary Key).
- **Non-Clustered Index** — Separate structure with pointers to the data rows. Multiple allowed per table.
- **Unique Index** — Enforces uniqueness.
- **Composite Index** — Multiple columns. Column order matters.
- **Covering Index** — Includes all columns a query needs, avoiding table lookup.

```sql
CREATE NONCLUSTERED INDEX IX_Orders_ClientId
ON Orders(ClientId)
INCLUDE (Amount, Status)  -- covering index
```

**When NOT to index:** Small tables, frequently updated columns, low cardinality columns (e.g., boolean).

---

**Q: What is a stored procedure vs a function?**

| | Stored Procedure | Function |
|---|---|---|
| Return value | Optional (output params) | Must return a value |
| DML (INSERT/UPDATE) | ✅ | ❌ (scalar functions) |
| Call from SELECT | ❌ | ✅ |
| Transactions | ✅ | ❌ |
| Error handling (TRY/CATCH) | ✅ | ❌ |

```sql
-- Stored Procedure
CREATE PROCEDURE GetClientOrders
    @ClientId INT,
    @StartDate DATE
AS
BEGIN
    SELECT * FROM Orders
    WHERE ClientId = @ClientId AND OrderDate >= @StartDate
END

EXEC GetClientOrders @ClientId = 1, @StartDate = '2025-01-01'
```

---

**Q: What are transactions and what are the ACID properties?**

A:
- **Atomicity** — All operations succeed or all fail (no partial commit).
- **Consistency** — Database goes from one valid state to another.
- **Isolation** — Concurrent transactions don't interfere with each other.
- **Durability** — Committed data survives system failure.

```sql
BEGIN TRANSACTION
    UPDATE Accounts SET Balance -= 1000 WHERE Id = 1  -- debit
    UPDATE Accounts SET Balance += 1000 WHERE Id = 2  -- credit

    IF @@ERROR != 0
        ROLLBACK TRANSACTION
    ELSE
        COMMIT TRANSACTION
```

---

**Q: What are transaction isolation levels?**

| Level | Dirty Read | Non-Repeatable Read | Phantom Read |
|---|---|---|---|
| Read Uncommitted | ✅ possible | ✅ possible | ✅ possible |
| Read Committed (default) | ❌ | ✅ possible | ✅ possible |
| Repeatable Read | ❌ | ❌ | ✅ possible |
| Serializable | ❌ | ❌ | ❌ |
| Snapshot | ❌ | ❌ | ❌ (uses versioning) |

---

**Q: What is a CTE (Common Table Expression)?**

A: A named temporary result set used within a single query. Improves readability and enables recursive queries.

```sql
WITH ActiveClients AS (
    SELECT Id, Name FROM Clients WHERE Status = 'Active'
),
RecentOrders AS (
    SELECT ClientId, SUM(Amount) AS Total
    FROM Orders
    WHERE OrderDate >= DATEADD(MONTH, -3, GETDATE())
    GROUP BY ClientId
)
SELECT ac.Name, ro.Total
FROM ActiveClients ac
JOIN RecentOrders ro ON ac.Id = ro.ClientId
```

---

**Q: What are window functions?**

A: Perform calculations across a set of rows related to the current row, without collapsing them like GROUP BY.

```sql
SELECT
    OrderId,
    ClientId,
    Amount,
    ROW_NUMBER() OVER (PARTITION BY ClientId ORDER BY Amount DESC) AS Rank,
    SUM(Amount) OVER (PARTITION BY ClientId) AS ClientTotal,
    LAG(Amount) OVER (ORDER BY OrderDate) AS PreviousAmount
FROM Orders
```

Common: `ROW_NUMBER()`, `RANK()`, `DENSE_RANK()`, `SUM()`, `AVG()`, `LAG()`, `LEAD()`, `FIRST_VALUE()`.

---

**Q: How do you optimize a slow query?**

A:
1. Check **execution plan** (`CTRL+M` in SSMS or `SET STATISTICS IO ON`)
2. Look for **table scans** → add appropriate indexes
3. Avoid `SELECT *` → select only needed columns
4. Avoid functions on indexed columns in WHERE (`WHERE YEAR(OrderDate) = 2025` → `WHERE OrderDate BETWEEN '2025-01-01' AND '2025-12-31'`)
5. Use **covering indexes** for frequently-used queries
6. Check for **N+1 queries** and replace with JOINs
7. Avoid cursors → use set-based operations
8. Use `EXISTS` instead of `IN` for large subqueries

---

**Q: What is the difference between UNION and UNION ALL?**

- `UNION` — Combines result sets, **removes duplicates** (slower).
- `UNION ALL` — Combines result sets, **keeps duplicates** (faster).

Use `UNION ALL` when you know there are no duplicates or don't care.

---

**Q: What are the SQL command categories?**

| Category | Commands |
|---|---|
| DDL (Data Definition) | CREATE, ALTER, DROP, TRUNCATE |
| DML (Data Manipulation) | SELECT, INSERT, UPDATE, DELETE |
| DCL (Data Control) | GRANT, REVOKE |
| TCL (Transaction Control) | BEGIN, COMMIT, ROLLBACK, SAVEPOINT |

---

## Quick-Fire

- **Primary Key** — Unique, not null, one per table. Clustered by default.
- **Foreign Key** — References PK of another table. Enforces referential integrity.
- **Unique Constraint** — Allows one NULL, enforces uniqueness.
- **CHECK Constraint** — Validates column values against a condition.
- **View** — Virtual table based on a SELECT query. Does not store data.
- **Materialized View** (indexed view in SQL Server) — Stores query result physically.
- **Normalization** — 1NF (atomic values), 2NF (no partial dependency), 3NF (no transitive dependency).
- **Deadlock** — Two transactions waiting on each other's locks. SQL Server detects and kills one.
