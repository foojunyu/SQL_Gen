# SQL Generator - Usage Examples

## Example 1: Basic Usage with Example Data

1. Start the application
2. Click "Load Example" button to load a sample Power BI table definition:

```json
{
  "name": "SalesData",
  "columns": [
    { "name": "OrderID", "dataType": "Int64" },
    { "name": "CustomerName", "dataType": "String" },
    { "name": "OrderDate", "dataType": "DateTime" },
    { "name": "TotalAmount", "dataType": "Decimal" },
    { "name": "IsActive", "dataType": "Boolean" }
  ]
}
```

3. Click "Generate SQL" to create a SQL query

**Generated SQL (without table selection):**
```sql
-- Generic SQL Query to match Power BI table structure
-- Please replace [YourTableName] with the actual table name
SELECT
    CAST(NULL AS BIGINT) AS [OrderID],
    CAST(NULL AS NVARCHAR(MAX)) AS [CustomerName],
    CAST(NULL AS DATETIME2) AS [OrderDate],
    CAST(NULL AS DECIMAL(18,2)) AS [TotalAmount],
    CAST(NULL AS BIT) AS [IsActive]
FROM [YourTableName];
```

## Example 2: Loading Data Structure from CSV File

1. Start the application and connect to Azure Fabric
2. Click "Load from CSV" button
3. Select a CSV file, for example `customer_data.csv`:

```csv
CustomerID,CustomerName,Email,RegistrationDate,TotalPurchases,IsActive
1001,John Smith,john@example.com,2023-01-15,1250.50,true
1002,Jane Doe,jane@example.com,2023-02-20,3420.75,true
1003,Bob Johnson,bob@example.com,2023-03-10,890.25,false
```

4. The application automatically detects the structure and displays:

```json
{
  "name": "customer_data",
  "columns": [
    { "name": "CustomerID", "dataType": "Int32" },
    { "name": "CustomerName", "dataType": "String" },
    { "name": "Email", "dataType": "String" },
    { "name": "RegistrationDate", "dataType": "DateTime" },
    { "name": "TotalPurchases", "dataType": "Decimal" },
    { "name": "IsActive", "dataType": "Boolean" }
  ]
}
```

5. Click "Generate SQL" to create a matching query
6. The application will find the best matching table in Azure Fabric or generate a template

**Generated SQL with Enhanced Features:**
```sql
-- SQL Query to match Power BI table structure from dbo.Customers
-- Data Validation: Check if CSV values exist in database
-- Check if values for 'CustomerID' exist:
SELECT DISTINCT [CUSTOMER_ID] FROM [dbo.Customers] WHERE [CUSTOMER_ID] IN ('1001', '1002', '1003');
-- Check if values for 'CustomerName' exist:
SELECT DISTINCT [CUSTOMER_NAME] FROM [dbo.Customers] WHERE [CUSTOMER_NAME] IN ('John Smith', 'Jane Doe', 'Bob Johnson');

-- Main SELECT query
SELECT
    [CUSTOMER_ID] AS [CustomerID],
    [CUSTOMER_NAME] AS [CustomerName],
    [EMAIL] AS [Email],
    CAST([REGISTRATION_DATE] AS DATETIME2) AS [RegistrationDate],
    [TOTAL_PURCHASES] AS [TotalPurchases],
    [IS_ACTIVE] AS [IsActive]
FROM [dbo.Customers]
WHERE
    [CUSTOMER_ID] IN ('1001', '1002', '1003') OR
    [CUSTOMER_NAME] IN ('John Smith', 'Jane Doe', 'Bob Johnson');

-- Sample data from database (top 5 rows)
SELECT TOP 5 * FROM [dbo.Customers];
```

**Benefits:**
- No manual JSON creation needed
- Automatic data type inference
- Data validation queries to check if CSV values exist in database
- WHERE clauses to filter based on CSV data
- Sample data queries for verification
- Quick structure detection from existing data files

## Example 3: Connecting to Azure Fabric

**Connection String Format:**
```
Server=your-workspace.datawarehouse.fabric.microsoft.com;Database=YourDatabase;User ID=YourUser;Password=YourPassword;Encrypt=True;TrustServerCertificate=False;
```

**Alternative with Azure AD Authentication:**
```
Server=your-workspace.datawarehouse.fabric.microsoft.com;Database=YourDatabase;Authentication=Active Directory Integrated;Encrypt=True;
```

## Example 4: Matching Specific Table

1. Connect to your Azure Fabric warehouse
2. Select a table from the "Available Tables" list (e.g., "dbo.Sales")
3. Enter your Power BI table definition
4. Click "Generate SQL"

**Generated SQL (with table selection):**
```sql
-- SQL Query to match Power BI table structure from dbo.Sales
SELECT
    [SalesOrderID] AS [OrderID],
    [CustomerName] AS [CustomerName],
    CAST([OrderDate] AS DATETIME2) AS [OrderDate],
    [TotalDue] AS [TotalAmount],
    NULL AS [IsActive] -- Column not found in source table
FROM [dbo.Sales];
```

## Example 5: Complex Power BI Table

```json
{
  "name": "CustomerAnalytics",
  "columns": [
    { "name": "CustomerID", "dataType": "Int32" },
    { "name": "FullName", "dataType": "String" },
    { "name": "Email", "dataType": "String" },
    { "name": "RegistrationDate", "dataType": "DateTime" },
    { "name": "TotalPurchases", "dataType": "Decimal" },
    { "name": "LastPurchaseDate", "dataType": "DateTime" },
    { "name": "IsActive", "dataType": "Boolean" },
    { "name": "LoyaltyPoints", "dataType": "Int32" },
    { "name": "PreferredCategory", "dataType": "String" }
  ]
}
```

## Example 6: Simplified Column Array Format

If you don't want to use the full object format, you can use just an array:

```json
[
  { "name": "ProductID", "dataType": "Int64" },
  { "name": "ProductName", "dataType": "String" },
  { "name": "Price", "dataType": "Decimal" },
  { "name": "InStock", "dataType": "Boolean" }
]
```

## Example 7: Multi-Table JOIN with Foreign Keys

When CSV columns come from multiple related tables, the application automatically generates JOIN queries:

1. Load a CSV with columns from different tables:
```csv
CustomerName,OrderDate,ProductName,Quantity,Price
John Smith,2024-01-15,Widget A,5,29.99
Jane Doe,2024-01-20,Widget B,3,49.99
```

2. Application detects columns from multiple tables:
   - CustomerName → dbo.Customers
   - OrderDate → dbo.Orders  
   - ProductName, Price → dbo.Products
   - Quantity → dbo.OrderDetails

3. **Generated Multi-Table SQL with JOINs:**
```sql
-- Multi-table SQL Query matching CSV structure
-- Tables involved: dbo.Customers, dbo.Orders, dbo.OrderDetails, dbo.Products

-- Main SELECT query with JOINs
SELECT
    COALESCE(c.[CustomerName], '#') AS [CustomerName],
    o.[OrderDate] AS [OrderDate],
    COALESCE(p.[ProductName], '#') AS [ProductName],
    d.[Quantity] AS [Quantity],
    p.[Price] AS [Price]
FROM [dbo.Customers] c
LEFT JOIN [dbo.Orders] o ON c.[CustomerID] = o.[CustomerID]
LEFT JOIN [dbo.OrderDetails] d ON o.[OrderID] = d.[OrderID]
LEFT JOIN [dbo.Products] p ON d.[ProductID] = p.[ProductID]
WHERE
    c.[CustomerName] IN ('John Smith', 'Jane Doe')
;
```

**How it works:**
- Application queries database metadata to discover foreign key relationships
- Finds optimal join path between tables using BFS algorithm (handles indirect paths)
- Generates proper LEFT JOIN statements with ON conditions to preserve all rows
- Uses COALESCE for nullable columns with '#' as default value
- Maps CSV columns to correct tables and columns
- Includes WHERE clauses with CSV data for filtering

**Pattern matches user reference SQL:**
```sql
-- Similar to this pattern:
FROM dbo.MainTable AS m
LEFT JOIN dbo.RelatedTable1 AS r1 ON m.Key1 = r1.Key1
LEFT JOIN dbo.RelatedTable2 AS r2 ON m.Key2 = r2.Key2
```

## Example 8: Production Data with Multiple JOINs (User Reference)

For complex manufacturing or production data with multiple related tables:

**CSV Input:**
```csv
Month,Facility,Family,Owner,Sub Family,Week,Operation Name,Operation Code,IN
202401,FAB1,ProductA,John,SubA,2024-01,Assembly,OP001,1000
202401,FAB2,ProductB,Jane,SubB,2024-02,Testing,OP002,500
```

**Generated SQL (matches user reference pattern):**
```sql
-- Multi-table SQL Query matching CSV structure
-- Tables involved: dbo.TBL_BE_F_FG_YIELD, dbo.OperationFrom, dbo.Product, dbo.Facility

SELECT
    COALESCE(y.[Month], '#') AS [Month],
    COALESCE(f.[Facility], '#') AS [Facility],
    COALESCE(p.[Family], '#') AS [Family],
    COALESCE(y.[OWNER], '#') AS [Owner],
    COALESCE(p.[SubFamily], '#') AS [Sub Family],
    COALESCE(y.[WORKWEEK], '#') AS [Week],
    COALESCE(op.[OperName], '') AS [Operation Name],
    COALESCE(op.[OperNum], '') AS [Operation Code],
    y.[IN_QTY] AS [IN]
FROM [dbo.TBL_BE_F_FG_YIELD] y
LEFT JOIN [dbo.OperationFrom] op ON y.[LINKOPERFROM] = op.[linkoper]
LEFT JOIN [dbo.Product] p ON y.[LINKPRODUCT] = p.[linkproduct]
LEFT JOIN [dbo.Facility] f ON y.[FACILITY_FROM] = f.[Facility]
WHERE y.[LINKLOT] IS NOT NULL;
```

**Key Features:**
- Uses COALESCE with '#' for string nulls (manufacturing convention)
- Uses COALESCE with '' for empty strings (operation names)
- Preserves all rows from main yield table with LEFT JOINs
- Short table aliases (y, op, p, f) for readability
- Foreign key joins on link columns (LINKOPERFROM, LINKPRODUCT, etc.)



## Tips and Best Practices

1. **Test Connection First**: Always test your connection before generating SQL
2. **Use CSV Import**: For existing data files, use the "Load from CSV" feature for automatic structure detection
3. **Select Appropriate Tables**: If you know which table to query, select it from the list for better matching
4. **Review Generated SQL**: Always review the generated SQL before using it in production
5. **CSV Format**: Ensure CSV files have a header row for proper column name detection
6. **Type Inference**: The application infers types from the first data row in CSV files
4. **Type Conversions**: The tool automatically adds CAST statements when data types don't match
5. **Missing Columns**: Columns not found in the source table are generated as NULL with a comment
6. **Case Sensitivity**: Column matching is case-insensitive for better flexibility

## Common Data Type Mappings

| Power BI Type | SQL Type |
|--------------|----------|
| String / Text | NVARCHAR(MAX) |
| Int64 / Integer | BIGINT |
| Int32 | INT |
| Decimal / Number | DECIMAL(18,2) |
| Double | FLOAT |
| DateTime / Date | DATETIME2 |
| Boolean / Bool | BIT |
| Binary | VARBINARY(MAX) |

## Troubleshooting

### Connection Fails
- Verify connection string format
- Check firewall rules on Azure Fabric
- Ensure credentials are correct
- Verify database name

### No Tables Appear
- Connection may not have proper permissions
- Database may be empty
- Check status log for error messages

### SQL Generation Errors
- Verify JSON format is correct
- Ensure at least one column is defined
- Check status log for detailed error messages

### Column Matching Improvements

The application uses intelligent column matching with multiple strategies:

1. **Exact Match**: Direct case-insensitive comparison
2. **Normalized Match**: Removes spaces, underscores, hyphens (e.g., "Operation Code" matches "OPERATION_CODE")
3. **Partial Match**: Checks if one name contains the other
4. **Normalized Partial**: Combines normalization with partial matching

**Examples of successful matches:**
- "Area" → "AREA"
- "Operation Code" → "OPERATION_CODE", "OperationCode", "OPERATIONCODE"
- "Sub Family" → "SUB_FAMILY", "SUBFAMILY"
- "Month" → "MONTH", "MONTH_NAME"
