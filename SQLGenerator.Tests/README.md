# SQL Generator Tests

This project contains unit tests for the SQL Generator application utilities.

## Running Tests

### On Windows

```bash
dotnet test
```

### On Linux/Mac (CI/CD)

The tests require the Windows Desktop runtime which is not available on Linux/Mac. The tests are designed to run on Windows environments. The build will succeed on all platforms, but test execution requires Windows.

For CI/CD on Linux, you can:
1. Skip test execution
2. Use a Windows-based test runner
3. Extract non-UI logic into a separate .NET Standard library for cross-platform testing

## Test Coverage

The test suite covers:
- **Data Type Mapping**: Tests for converting Power BI data types to SQL Server types
- **Connection String Validation**: Tests for security validation of connection strings
- **JSON Parsing**: Tests for parsing Power BI table definitions
- **SQL Generation**: Tests for generating SQL queries from Power BI table structures

## Test Cases

### SQLGeneratorUtilsTests
- Type mapping for all supported Power BI data types
- Connection string security validation
- JSON parsing (both full and array formats)
- SQL generation for generic tables
- Case-insensitive type handling
- Error handling for invalid inputs

Total test cases: 20+
