# JOIN Path Finding - Test Scenarios

This document describes test scenarios for the improved JOIN path finding algorithm.

## Test Scenario 1: Direct Foreign Key Relationships

**Setup:**
- Table A has columns: ID, Name
- Table B has columns: ID, A_ID (FK to A.ID), Description
- Table C has columns: ID, B_ID (FK to B.ID), Value

**CSV Input:**
- Columns: Name (from A), Description (from B), Value (from C)

**Expected Result:**
- ✅ Direct path found: A → B → C
- ✅ Generated SQL includes proper JOINs
- ✅ No warning messages

**Generated SQL Pattern:**
```sql
SELECT
    a.[Name] AS [Name],
    b.[Description] AS [Description],
    c.[Value] AS [Value]
FROM [TableA] a
LEFT JOIN [TableB] b ON a.[ID] = b.[A_ID]
LEFT JOIN [TableC] c ON b.[ID] = c.[B_ID];
```

## Test Scenario 2: Indirect Relationships Through Intermediate Tables

**Setup:**
- Table Customers has: CustomerID, Name
- Table Orders has: OrderID, CustomerID (FK to Customers), OrderDate
- Table OrderDetails has: DetailID, OrderID (FK to Orders), ProductID (FK to Products)
- Table Products has: ProductID, ProductName, Price

**CSV Input:**
- Columns: Name (from Customers), ProductName (from Products), Price (from Products)
- Note: Orders and OrderDetails are intermediate tables not in CSV

**Expected Result:**
- ✅ Indirect path found through intermediate tables
- ✅ Status log message: "Found indirect join path from Customers to Products through intermediate tables"
- ✅ Generated SQL includes all necessary intermediate JOINs

**Generated SQL Pattern:**
```sql
SELECT
    c.[Name] AS [Name],
    p.[ProductName] AS [ProductName],
    p.[Price] AS [Price]
FROM [Customers] c
LEFT JOIN [Orders] o ON c.[CustomerID] = o.[CustomerID]
LEFT JOIN [OrderDetails] d ON o.[OrderID] = d.[OrderID]
LEFT JOIN [Products] p ON d.[ProductID] = p.[ProductID];
```

## Test Scenario 3: No Relationship Between Tables

**Setup:**
- Table A has: ID, Name
- Table X has: ID, Value
- No foreign keys between A and X

**CSV Input:**
- Columns: Name (from A), Value (from X)

**Expected Result:**
- ⚠️ Warning: "Could not find join path to connect tables: [TableX]"
- ⚠️ Warning: "These tables may need to be queried separately or joined manually"
- ✅ Generated SQL includes commented placeholders

**Generated SQL Pattern:**
```sql
-- WARNING: No join path found between tables. Please add appropriate JOIN conditions.
SELECT
    a.[Name] AS [Name],
    x.[Value] AS [Value]
FROM [TableA] a
-- TODO: Add JOIN condition for [TableX] x
-- CROSS JOIN [TableX] x -- Uncomment and add proper JOIN condition
;
```

## Test Scenario 4: Partial Connectivity

**Setup:**
- Table A has: ID, Name
- Table B has: ID, A_ID (FK to A), Description
- Table C has: ID, Value
- A connects to B, but C is isolated

**CSV Input:**
- Columns: Name (from A), Description (from B), Value (from C)

**Expected Result:**
- ✅ A and B are properly joined
- ⚠️ Warning about C being unconnected
- ✅ SQL includes proper JOIN for A-B, placeholder for C

**Generated SQL Pattern:**
```sql
-- WARNING: No join path found between tables. Please add appropriate JOIN conditions.
SELECT
    a.[Name] AS [Name],
    b.[Description] AS [Description],
    c.[Value] AS [Value]
FROM [TableA] a
LEFT JOIN [TableB] b ON a.[ID] = b.[A_ID]
-- TODO: Add JOIN condition for [TableC] c
-- CROSS JOIN [TableC] c -- Uncomment and add proper JOIN condition
;
```

## Test Scenario 5: Multiple Path Options (Choose Shortest)

**Setup:**
- Table A connects to Table B
- Table B connects to Table C
- Table A also connects directly to Table C (alternative path)

**CSV Input:**
- Columns from A and C only

**Expected Result:**
- ✅ Algorithm chooses the shorter direct path (A → C)
- ✅ Does not include unnecessary intermediate table B
- ✅ Generates optimal SQL

## Algorithm Correctness Tests

### BFS Path Finding
- ✅ Finds shortest path between two tables
- ✅ Avoids cycles (doesn't revisit tables)
- ✅ Handles bidirectional relationships (A→B and B→A)
- ✅ Returns null when no path exists

### Connected Set Management
- ✅ Tracks connected tables correctly
- ✅ Updates connected set as new tables are added
- ✅ Removes processed tables from remaining set

### Error Handling
- ✅ Gracefully handles missing foreign keys
- ✅ Provides informative error messages
- ✅ Generates valid SQL even in failure cases

## Manual Testing Procedure

To manually test these scenarios:

1. **Setup Test Database**
   - Create tables with specified relationships
   - Add sample data

2. **Test Each Scenario**
   - Connect application to test database
   - Load CSV or JSON with specified columns
   - Click "Generate SQL"
   - Verify generated SQL matches expected pattern
   - Check status log for appropriate messages

3. **Execute Generated SQL**
   - Run the generated SQL in SQL Server Management Studio
   - Verify it executes without errors (or with expected comments)
   - Check result set matches expectations

## Success Criteria

✅ All direct relationships are found and joined correctly
✅ Indirect relationships through intermediate tables are discovered
✅ No false positives (doesn't claim connection when none exists)
✅ Generated SQL is syntactically correct
✅ Error messages are helpful and specific
✅ Performance is acceptable (< 1 second for typical databases with 100 tables)
