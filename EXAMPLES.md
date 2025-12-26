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

## Example 2: Connecting to Azure Fabric

**Connection String Format:**
```
Server=your-workspace.datawarehouse.fabric.microsoft.com;Database=YourDatabase;User ID=YourUser;Password=YourPassword;Encrypt=True;TrustServerCertificate=False;
```

**Alternative with Azure AD Authentication:**
```
Server=your-workspace.datawarehouse.fabric.microsoft.com;Database=YourDatabase;Authentication=Active Directory Integrated;Encrypt=True;
```

## Example 3: Matching Specific Table

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

## Example 4: Complex Power BI Table

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

## Example 5: Simplified Column Array Format

If you don't want to use the full object format, you can use just an array:

```json
[
  { "name": "ProductID", "dataType": "Int64" },
  { "name": "ProductName", "dataType": "String" },
  { "name": "Price", "dataType": "Decimal" },
  { "name": "InStock", "dataType": "Boolean" }
]
```

## Tips and Best Practices

1. **Test Connection First**: Always test your connection before generating SQL
2. **Select Appropriate Tables**: If you know which table to query, select it from the list for better matching
3. **Review Generated SQL**: Always review the generated SQL before using it in production
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
