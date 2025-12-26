# SQL Generator for Azure Fabric & Power BI

A Windows Forms application that connects to Azure Fabric Data Warehouse and generates SQL queries to match your designated Power BI data table structure or CSV file data.

## Features

- **Azure Fabric Connectivity**: Connect to Azure Fabric Data Warehouse using connection strings
- **Table Discovery**: Automatically loads and displays all available tables and their schemas
- **Power BI Table Matching**: Input your Power BI table structure in JSON format
- **CSV File Import**: Load CSV files to auto-detect column structure and generate matching SQL
- **Intelligent SQL Generation**: 
  - Smart column matching with multiple strategies (exact, normalized, partial)
  - Auto-matches columns even with different naming conventions (spaces, underscores, etc.)
  - Generates SELECT queries with proper column mappings
  - Includes type conversions when needed
  - Handles missing columns gracefully
- **User-Friendly Interface**: 
  - Easy connection management
  - Table browsing with column details
  - SQL preview and copy to clipboard
  - Status logging

## Prerequisites

- .NET 8.0 or later
- Windows operating system
- Access to Azure Fabric Data Warehouse

## Building the Application

```bash
cd SQLGenerator
dotnet restore
dotnet build
```

## Running Tests

Unit tests are included in the `SQLGenerator.Tests` project. Tests require Windows Desktop runtime and should be run on Windows:

```bash
dotnet test
```

Note: Tests will not run on Linux/Mac due to Windows Forms dependencies, but the application builds successfully on all platforms.

## Running the Application

```bash
cd SQLGenerator
dotnet run
```

Or use Visual Studio to open the solution and run.

## Usage

1. **Connect to Azure Fabric**:
   - Enter your Azure Fabric connection string in the format:
     ```
     Server=your-fabric-server.database.windows.net;Database=YourDatabase;User ID=YourUser;Password=YourPassword;Encrypt=True;
     ```
   - Click "Connect" button

2. **Browse Tables**:
   - After connecting, available tables will appear in the left panel
   - Select a table to view its columns in the status log

3. **Define Data Structure**:
   
   **Option A - Power BI JSON Format:**
   - Enter your Power BI table structure in JSON format, or click "Load Example"
   - Example format:
     ```json
     {
       "name": "SalesData",
       "columns": [
         { "name": "OrderID", "dataType": "Int64" },
         { "name": "CustomerName", "dataType": "String" },
         { "name": "OrderDate", "dataType": "DateTime" },
         { "name": "TotalAmount", "dataType": "Decimal" }
       ]
     }
     ```
   
   **Option B - Load from CSV:**
   - Click "Load from CSV" button
   - Select a CSV file with header row
   - The application will automatically detect column names and infer data types
   - Column structure will be displayed in JSON format

4. **Generate SQL**:
   - Optionally select a specific table from the list
   - Click "Generate SQL" button
   - The application will create a SQL query matching your Power BI table structure
   - Copy the generated SQL to clipboard using the "Copy to Clipboard" button

## Power BI Data Types Supported

- String / Text → NVARCHAR(MAX)
- Int64 / Integer → BIGINT
- Int32 → INT
- Decimal / Number → DECIMAL(18,2)
- Double → FLOAT
- DateTime / Date → DATETIME2
- Boolean / Bool → BIT
- Binary → VARBINARY(MAX)

## Dependencies

- Microsoft.Data.SqlClient (5.2.0) - For Azure SQL connectivity
- System.Text.Json (8.0.5) - For JSON parsing

## Security Notes

- Never commit connection strings with credentials to source control
- Use Azure Key Vault or secure configuration management for production deployments
- Always use encrypted connections (Encrypt=True in connection string)

## License

This project is open source.

