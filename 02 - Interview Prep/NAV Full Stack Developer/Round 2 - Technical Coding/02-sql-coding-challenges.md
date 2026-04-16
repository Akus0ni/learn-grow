# SQL Server — Coding Challenges & Logical Thinking

> Focus: Write correct SQL from scratch. Think performance. Explain your reasoning.

---

## Setup: Sample Schema (Fund Administration Domain)

```sql
-- Use this mental model for all queries below
CREATE TABLE Funds (
    FundId      INT PRIMARY KEY,
    FundName    VARCHAR(100),
    FundType    VARCHAR(50),   -- 'Equity', 'Fixed Income', 'Mixed'
    Currency    CHAR(3),
    LaunchDate  DATE,
    ManagerId   INT
);

CREATE TABLE NAVHistory (
    NavId       INT PRIMARY KEY,
    FundId      INT FOREIGN KEY REFERENCES Funds(FundId),
    NavDate     DATE,
    NavPerUnit  DECIMAL(18,6),
    TotalAssets DECIMAL(18,2),
    TotalUnits  DECIMAL(18,4)
);

CREATE TABLE Investors (
    InvestorId  INT PRIMARY KEY,
    FullName    VARCHAR(100),
    Email       VARCHAR(200),
    Country     VARCHAR(50),
    JoinDate    DATE
);

CREATE TABLE Holdings (
    HoldingId   INT PRIMARY KEY,
    InvestorId  INT FOREIGN KEY REFERENCES Investors(InvestorId),
    FundId      INT FOREIGN KEY REFERENCES Funds(FundId),
    Units       DECIMAL(18,4),
    BuyDate     DATE,
    BuyNavPrice DECIMAL(18,6)
);

CREATE TABLE Transactions (
    TxId        INT PRIMARY KEY,
    InvestorId  INT,
    FundId      INT,
    TxType      VARCHAR(20),  -- 'BUY', 'SELL', 'DIVIDEND'
    Amount      DECIMAL(18,2),
    Units       DECIMAL(18,4),
    TxDate      DATE,
    Status      VARCHAR(20)   -- 'COMPLETED', 'PENDING', 'FAILED'
);
```

---

## Challenge 1: Basic JOINs with Aggregation

### Q: "Find all investors who have holdings in more than 2 funds, along with their total invested value."

**Think out loud**: Investors → Holdings JOIN → GROUP BY investor → HAVING count > 2 → JOIN back for names

```sql
SELECT 
    i.InvestorId,
    i.FullName,
    i.Country,
    COUNT(DISTINCT h.FundId)      AS FundsCount,
    SUM(h.Units * h.BuyNavPrice)  AS TotalInvestedValue
FROM Investors i
INNER JOIN Holdings h ON i.InvestorId = h.InvestorId
GROUP BY i.InvestorId, i.FullName, i.Country
HAVING COUNT(DISTINCT h.FundId) > 2
ORDER BY TotalInvestedValue DESC;
```

**Performance note**: "I'd want an index on `Holdings(InvestorId)` and `Holdings(FundId)` for this query."

---

## Challenge 2: Window Functions

### Q: "For each fund, show the NAV per unit for today, yesterday, and the percentage change."

**Think out loud**: Need LAG window function. Partition by fund, order by date.

```sql
SELECT 
    f.FundName,
    n.NavDate,
    n.NavPerUnit                          AS CurrentNAV,
    LAG(n.NavPerUnit) OVER (
        PARTITION BY n.FundId 
        ORDER BY n.NavDate
    )                                      AS PreviousNAV,
    ROUND(
        (n.NavPerUnit - LAG(n.NavPerUnit) OVER (
            PARTITION BY n.FundId 
            ORDER BY n.NavDate
        )) 
        / NULLIF(LAG(n.NavPerUnit) OVER (
            PARTITION BY n.FundId 
            ORDER BY n.NavDate
        ), 0) * 100,
        2
    )                                      AS PercentChange
FROM NAVHistory n
INNER JOIN Funds f ON n.FundId = f.FundId
WHERE n.NavDate >= DATEADD(DAY, -30, GETDATE())
ORDER BY f.FundName, n.NavDate DESC;
```

**Bonus — cleaner with CTE:**
```sql
WITH NavWithLag AS (
    SELECT 
        n.FundId,
        f.FundName,
        n.NavDate,
        n.NavPerUnit,
        LAG(n.NavPerUnit) OVER (PARTITION BY n.FundId ORDER BY n.NavDate) AS PrevNAV
    FROM NAVHistory n
    INNER JOIN Funds f ON n.FundId = f.FundId
)
SELECT 
    FundName,
    NavDate,
    NavPerUnit,
    PrevNAV,
    ROUND((NavPerUnit - PrevNAV) / NULLIF(PrevNAV, 0) * 100, 2) AS PctChange
FROM NavWithLag
WHERE NavDate >= DATEADD(DAY, -7, GETDATE())
ORDER BY FundName, NavDate DESC;
```

> **Say**: "I used NULLIF to avoid division by zero — defensive coding. The CTE version is more readable and maintainable."

---

## Challenge 3: Ranking

### Q: "Rank the top 3 investors by total portfolio value within each country."

```sql
WITH InvestorPortfolioValue AS (
    SELECT 
        i.InvestorId,
        i.FullName,
        i.Country,
        SUM(h.Units * n.NavPerUnit) AS CurrentPortfolioValue
    FROM Investors i
    INNER JOIN Holdings h ON i.InvestorId = h.InvestorId
    INNER JOIN (
        -- Get latest NAV for each fund
        SELECT FundId, NavPerUnit
        FROM NAVHistory
        WHERE NavDate = (
            SELECT MAX(NavDate) FROM NAVHistory n2 
            WHERE n2.FundId = NAVHistory.FundId
        )
    ) n ON h.FundId = n.FundId
    GROUP BY i.InvestorId, i.FullName, i.Country
),
RankedInvestors AS (
    SELECT 
        *,
        RANK() OVER (PARTITION BY Country ORDER BY CurrentPortfolioValue DESC) AS CountryRank
    FROM InvestorPortfolioValue
)
SELECT *
FROM RankedInvestors
WHERE CountryRank <= 3
ORDER BY Country, CountryRank;
```

**Follow-up: "What's the difference between RANK, DENSE_RANK, and ROW_NUMBER?"**

```sql
-- Given values: 100, 90, 90, 80
-- ROW_NUMBER:  1, 2, 3, 4   (always unique, no gaps)
-- RANK:        1, 2, 2, 4   (ties share rank, gap after)
-- DENSE_RANK:  1, 2, 2, 3   (ties share rank, NO gap)

SELECT 
    InvestorId,
    CurrentPortfolioValue,
    ROW_NUMBER()  OVER (ORDER BY CurrentPortfolioValue DESC) AS RowNum,
    RANK()        OVER (ORDER BY CurrentPortfolioValue DESC) AS Rnk,
    DENSE_RANK()  OVER (ORDER BY CurrentPortfolioValue DESC) AS DenseRnk
FROM InvestorPortfolioValue;
```

---

## Challenge 4: Recursive CTE

### Q: "Write a query to find all transactions in the last N months per fund, using a recursive date series."

**Or the classic: "Generate a number/date series"**

```sql
-- Recursive CTE: Generate a date series for the last 12 months
WITH DateSeries AS (
    -- Anchor
    SELECT CAST(DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)) AS DATE) AS MonthStart
    UNION ALL
    -- Recursive member
    SELECT CAST(DATEADD(MONTH, 1, MonthStart) AS DATE)
    FROM DateSeries
    WHERE MonthStart < DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
)
SELECT 
    ds.MonthStart,
    f.FundName,
    COUNT(t.TxId)     AS TransactionCount,
    SUM(t.Amount)     AS TotalVolume,
    ISNULL(SUM(CASE WHEN t.TxType = 'BUY'  THEN t.Amount END), 0) AS BuyVolume,
    ISNULL(SUM(CASE WHEN t.TxType = 'SELL' THEN t.Amount END), 0) AS SellVolume
FROM DateSeries ds
CROSS JOIN Funds f
LEFT JOIN Transactions t 
    ON t.FundId = f.FundId
    AND t.TxDate >= ds.MonthStart 
    AND t.TxDate < DATEADD(MONTH, 1, ds.MonthStart)
    AND t.Status = 'COMPLETED'
GROUP BY ds.MonthStart, f.FundId, f.FundName
ORDER BY f.FundName, ds.MonthStart
OPTION (MAXRECURSION 13); -- safety limit
```

> **Say**: "Recursive CTEs need a base case (anchor) and recursive member, with a termination condition in the WHERE clause. MAXRECURSION prevents infinite loops."

---

## Challenge 5: Complex Aggregation with PIVOT-like Logic

### Q: "Show total buy and sell volume per fund per month (horizontal pivoting)."

```sql
SELECT 
    f.FundName,
    FORMAT(t.TxDate, 'yyyy-MM') AS Month,
    SUM(CASE WHEN t.TxType = 'BUY'  THEN t.Amount ELSE 0 END) AS BuyVolume,
    SUM(CASE WHEN t.TxType = 'SELL' THEN t.Amount ELSE 0 END) AS SellVolume,
    SUM(CASE WHEN t.TxType = 'BUY'  THEN t.Amount ELSE 0 END) 
  - SUM(CASE WHEN t.TxType = 'SELL' THEN t.Amount ELSE 0 END) AS NetFlow
FROM Transactions t
INNER JOIN Funds f ON t.FundId = f.FundId
WHERE t.Status = 'COMPLETED'
  AND t.TxDate >= DATEADD(YEAR, -1, GETDATE())
GROUP BY f.FundName, FORMAT(t.TxDate, 'yyyy-MM')
ORDER BY f.FundName, Month;
```

---

## Challenge 6: Finding Duplicates

### Q: "Find duplicate transactions — same investor, same fund, same amount, same day."

```sql
-- Method 1: GROUP BY + HAVING (most common interview answer)
SELECT 
    InvestorId,
    FundId,
    Amount,
    TxDate,
    COUNT(*) AS DuplicateCount
FROM Transactions
GROUP BY InvestorId, FundId, Amount, TxDate
HAVING COUNT(*) > 1;

-- Method 2: Window function (more powerful — shows all duplicate rows)
WITH DuplicateFlagged AS (
    SELECT 
        TxId,
        InvestorId,
        FundId,
        Amount,
        TxDate,
        ROW_NUMBER() OVER (
            PARTITION BY InvestorId, FundId, Amount, TxDate
            ORDER BY TxId
        ) AS RowNum
    FROM Transactions
)
SELECT * FROM DuplicateFlagged WHERE RowNum > 1;
-- RowNum = 1 are originals, > 1 are duplicates

-- Method 3: Delete duplicates (keep one)
WITH CTE AS (
    SELECT TxId,
           ROW_NUMBER() OVER (
               PARTITION BY InvestorId, FundId, Amount, TxDate
               ORDER BY TxId
           ) AS RowNum
    FROM Transactions
)
DELETE FROM CTE WHERE RowNum > 1;
```

> **Say**: "Method 2 is my preferred approach — GROUP BY + HAVING only shows you the count, but the window function approach lets you see and act on the actual duplicate rows. Method 3 shows I can delete them safely."

---

## Challenge 7: Self-JOIN

### Q: "Find investors who joined after another investor in the same country (mentor-mentee style)."

```sql
SELECT 
    i1.FullName AS EarlierInvestor,
    i2.FullName AS LaterInvestor,
    i1.Country,
    i1.JoinDate AS EarlierDate,
    i2.JoinDate AS LaterDate,
    DATEDIFF(DAY, i1.JoinDate, i2.JoinDate) AS DaysBetween
FROM Investors i1
INNER JOIN Investors i2 
    ON i1.Country = i2.Country
    AND i1.JoinDate < i2.JoinDate
    AND i1.InvestorId != i2.InvestorId
ORDER BY i1.Country, i1.JoinDate;
```

---

## Challenge 8: Stored Procedure

### Q: "Write a stored procedure to calculate and store NAV for a fund."

```sql
CREATE PROCEDURE usp_CalculateAndStoreNAV
    @FundId     INT,
    @NavDate    DATE,
    @TotalAssets DECIMAL(18,2),
    @TotalUnits  DECIMAL(18,4),
    @NewNavId   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validation
    IF NOT EXISTS (SELECT 1 FROM Funds WHERE FundId = @FundId)
    BEGIN
        RAISERROR('Fund %d does not exist', 16, 1, @FundId);
        RETURN -1;
    END
    
    IF @TotalUnits = 0
    BEGIN
        RAISERROR('Total units cannot be zero', 16, 1);
        RETURN -2;
    END
    
    -- Check for duplicate
    IF EXISTS (SELECT 1 FROM NAVHistory WHERE FundId = @FundId AND NavDate = @NavDate)
    BEGIN
        RAISERROR('NAV already exists for Fund %d on %s', 16, 1, @FundId, @NavDate);
        RETURN -3;
    END
    
    DECLARE @NavPerUnit DECIMAL(18,6) = @TotalAssets / @TotalUnits;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        INSERT INTO NAVHistory (FundId, NavDate, NavPerUnit, TotalAssets, TotalUnits)
        VALUES (@FundId, @NavDate, @NavPerUnit, @TotalAssets, @TotalUnits);
        
        SET @NewNavId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        RETURN -99;
    END CATCH
    
    RETURN 0;
END;
GO

-- Usage:
DECLARE @OutputNavId INT;
EXEC usp_CalculateAndStoreNAV 
    @FundId = 1, 
    @NavDate = '2026-04-15',
    @TotalAssets = 5000000.00,
    @TotalUnits = 50000.0000,
    @NewNavId = @OutputNavId OUTPUT;
    
SELECT @OutputNavId AS NewNavId;
```

> **Say**: "I always include: input validation, duplicate checks, TRY/CATCH with proper rollback, SCOPE_IDENTITY (not @@IDENTITY which can be affected by triggers), SET NOCOUNT ON for performance, and meaningful error messages."

---

## Challenge 9: Query Optimisation

### Q: "This query is slow — how would you fix it?"

```sql
-- SLOW query (given to you in interview):
SELECT *
FROM Transactions
WHERE YEAR(TxDate) = 2025
  AND MONTH(TxDate) = 3;
```

**Spot the problem**: Function on indexed column prevents index usage (non-sargable).

```sql
-- FAST version: range predicate — index can be used
SELECT TxId, InvestorId, FundId, TxType, Amount, TxDate, Status
FROM Transactions
WHERE TxDate >= '2025-03-01' 
  AND TxDate < '2025-04-01'  -- half-open interval

-- Also fix: SELECT * → SELECT only needed columns
-- Then: CREATE NONCLUSTERED INDEX IX_Transactions_TxDate 
--       ON Transactions(TxDate) INCLUDE (InvestorId, FundId, Amount, Status);
```

**Other optimisation patterns to mention**:

```sql
-- Avoid: subquery in SELECT (runs once per row)
SELECT FundId, (SELECT COUNT(*) FROM Transactions t WHERE t.FundId = f.FundId) AS TxCount
FROM Funds f;

-- Better: JOIN with aggregation
SELECT f.FundId, COUNT(t.TxId) AS TxCount
FROM Funds f
LEFT JOIN Transactions t ON f.FundId = t.FundId
GROUP BY f.FundId;

-- Avoid: NOT IN with NULLs (unexpected behavior!)
SELECT * FROM Investors 
WHERE InvestorId NOT IN (SELECT InvestorId FROM Transactions WHERE InvestorId IS NULL);
-- If subquery returns any NULL, the whole NOT IN returns 0 rows!

-- Better: NOT EXISTS
SELECT * FROM Investors i
WHERE NOT EXISTS (SELECT 1 FROM Transactions t WHERE t.InvestorId = i.InvestorId);
```

---

## Challenge 10: Transaction & Concurrency

### Q: "Write a transaction to transfer units between two investors safely."

```sql
CREATE PROCEDURE usp_TransferUnits
    @FromInvestorId INT,
    @ToInvestorId   INT,
    @FundId         INT,
    @UnitsToTransfer DECIMAL(18,4)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Lock the source row first (avoid deadlocks by always locking in same order)
        DECLARE @AvailableUnits DECIMAL(18,4);
        
        SELECT @AvailableUnits = Units
        FROM Holdings WITH (UPDLOCK, ROWLOCK)  -- row-level lock
        WHERE InvestorId = @FromInvestorId AND FundId = @FundId;
        
        IF @AvailableUnits IS NULL OR @AvailableUnits < @UnitsToTransfer
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Insufficient units for transfer', 16, 1);
            RETURN;
        END
        
        -- Deduct from source
        UPDATE Holdings
        SET Units = Units - @UnitsToTransfer
        WHERE InvestorId = @FromInvestorId AND FundId = @FundId;
        
        -- Add to destination (INSERT if not exists, UPDATE if exists)
        IF EXISTS (SELECT 1 FROM Holdings WHERE InvestorId = @ToInvestorId AND FundId = @FundId)
            UPDATE Holdings
            SET Units = Units + @UnitsToTransfer
            WHERE InvestorId = @ToInvestorId AND FundId = @FundId;
        ELSE
            INSERT INTO Holdings (InvestorId, FundId, Units, BuyDate, BuyNavPrice)
            SELECT @ToInvestorId, @FundId, @UnitsToTransfer, GETDATE(), 
                   (SELECT TOP 1 NavPerUnit FROM NAVHistory 
                    WHERE FundId = @FundId ORDER BY NavDate DESC);
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        THROW; -- re-raise the error
    END CATCH
END;
```

> **Say**: "UPDLOCK + ROWLOCK hints prevent dirty reads and serialise access to that row during the transaction. I always check @@TRANCOUNT before rollback in case the transaction was already rolled back by a nested error. THROW (SQL Server 2012+) is cleaner than RAISERROR for re-raising."

---

## SQL Window Functions Quick Reference

```sql
-- ROW_NUMBER — unique sequential number
ROW_NUMBER() OVER (PARTITION BY FundId ORDER BY NavDate DESC)

-- RANK — gaps after ties
RANK() OVER (PARTITION BY Country ORDER BY PortfolioValue DESC)

-- DENSE_RANK — no gaps after ties  
DENSE_RANK() OVER (ORDER BY Amount DESC)

-- LAG / LEAD — previous/next row value
LAG(NavPerUnit, 1, 0) OVER (PARTITION BY FundId ORDER BY NavDate)
LEAD(NavPerUnit, 1, 0) OVER (PARTITION BY FundId ORDER BY NavDate)

-- Running total
SUM(Amount) OVER (PARTITION BY FundId ORDER BY TxDate ROWS UNBOUNDED PRECEDING)

-- Moving average (7-day)
AVG(NavPerUnit) OVER (
    PARTITION BY FundId 
    ORDER BY NavDate 
    ROWS BETWEEN 6 PRECEDING AND CURRENT ROW
)

-- NTILE — divide into buckets
NTILE(4) OVER (ORDER BY PortfolioValue DESC) AS Quartile

-- FIRST_VALUE / LAST_VALUE
FIRST_VALUE(NavPerUnit) OVER (PARTITION BY FundId ORDER BY NavDate) AS FirstNAV
LAST_VALUE(NavPerUnit)  OVER (PARTITION BY FundId ORDER BY NavDate 
                               ROWS BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) AS LastNAV
```

---

## Index Strategy Quick Reference

```sql
-- When to create which index:
-- Clustered: PK column (auto-created), or the column most used in range scans
-- Non-clustered: frequently filtered columns (WHERE clause)
-- Covering: include all columns needed by a query to avoid key lookups

-- Example covering index for the transaction query above:
CREATE NONCLUSTERED INDEX IX_Transactions_FundDate
ON Transactions (FundId, TxDate)
INCLUDE (TxType, Amount, Status, InvestorId);
-- Covers: WHERE FundId = x AND TxDate BETWEEN ..., SELECT TxType, Amount, Status

-- Composite index order matters:
-- Put equality columns first, range columns last
-- WHERE FundId = 1 AND TxDate >= '2025-01-01' → index on (FundId, TxDate)
```

---

## Interview Answer Framework for SQL Questions

When asked any SQL optimisation question, say:

1. **"First I'd look at the execution plan"** — check for table scans, key lookups
2. **"Check if the columns in WHERE/JOIN have indexes"**
3. **"Look for non-sargable predicates"** — functions on columns, implicit conversions
4. **"Check query statistics"** — `SET STATISTICS IO ON`
5. **"Consider covering indexes"** to eliminate key lookups
6. **"For large tables, consider partitioning or archiving old data"**
