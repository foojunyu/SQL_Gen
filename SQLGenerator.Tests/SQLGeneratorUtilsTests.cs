using SQLGenerator;
using System.Text.Json;

namespace SQLGenerator.Tests;

public class SQLGeneratorUtilsTests
{
    [Fact]
    public void MapPowerBIToSQLType_StringType_ReturnsNVarcharMax()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.MapPowerBIToSQLType("string");

        // Assert
        Assert.Equal("NVARCHAR(MAX)", result);
    }

    [Fact]
    public void MapPowerBIToSQLType_TextType_ReturnsNVarcharMax()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.MapPowerBIToSQLType("text");

        // Assert
        Assert.Equal("NVARCHAR(MAX)", result);
    }

    [Fact]
    public void MapPowerBIToSQLType_Int64Type_ReturnsBigInt()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.MapPowerBIToSQLType("int64");

        // Assert
        Assert.Equal("BIGINT", result);
    }

    [Fact]
    public void MapPowerBIToSQLType_Int32Type_ReturnsInt()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.MapPowerBIToSQLType("Int32");

        // Assert
        Assert.Equal("INT", result);
    }

    [Fact]
    public void MapPowerBIToSQLType_DecimalType_ReturnsDecimal()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.MapPowerBIToSQLType("decimal");

        // Assert
        Assert.Equal("DECIMAL(18,2)", result);
    }

    [Fact]
    public void MapPowerBIToSQLType_DateTimeType_ReturnsDateTime2()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.MapPowerBIToSQLType("DateTime");

        // Assert
        Assert.Equal("DATETIME2", result);
    }

    [Fact]
    public void MapPowerBIToSQLType_BooleanType_ReturnsBit()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.MapPowerBIToSQLType("boolean");

        // Assert
        Assert.Equal("BIT", result);
    }

    [Fact]
    public void MapPowerBIToSQLType_UnknownType_ReturnsNVarcharMax()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.MapPowerBIToSQLType("unknowntype");

        // Assert
        Assert.Equal("NVARCHAR(MAX)", result);
    }

    [Fact]
    public void ValidateConnectionStringSecurity_WithEncrypt_ReturnsTrue()
    {
        // Arrange
        var connectionString = "Server=test.database.windows.net;Database=TestDB;Encrypt=True;";

        // Act
        var result = SQLGeneratorUtils.ValidateConnectionStringSecurity(connectionString);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateConnectionStringSecurity_WithoutEncrypt_ReturnsFalse()
    {
        // Arrange
        var connectionString = "Server=test.database.windows.net;Database=TestDB;";

        // Act
        var result = SQLGeneratorUtils.ValidateConnectionStringSecurity(connectionString);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateConnectionStringSecurity_EmptyString_ReturnsFalse()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.ValidateConnectionStringSecurity("");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateConnectionStringSecurity_NullString_ReturnsFalse()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.ValidateConnectionStringSecurity(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ParsePowerBITable_ValidFullFormat_ReturnsColumns()
    {
        // Arrange
        var json = @"{
            ""name"": ""TestTable"",
            ""columns"": [
                { ""name"": ""ID"", ""dataType"": ""Int64"" },
                { ""name"": ""Name"", ""dataType"": ""String"" }
            ]
        }";

        // Act
        var result = SQLGeneratorUtils.ParsePowerBITable(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("ID", result[0].Name);
        Assert.Equal("Int64", result[0].DataType);
        Assert.Equal("Name", result[1].Name);
        Assert.Equal("String", result[1].DataType);
    }

    [Fact]
    public void ParsePowerBITable_ValidArrayFormat_ReturnsColumns()
    {
        // Arrange
        var json = @"[
            { ""name"": ""OrderID"", ""dataType"": ""Int32"" },
            { ""name"": ""Amount"", ""dataType"": ""Decimal"" }
        ]";

        // Act
        var result = SQLGeneratorUtils.ParsePowerBITable(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("OrderID", result[0].Name);
        Assert.Equal("Int32", result[0].DataType);
    }

    [Fact]
    public void ParsePowerBITable_InvalidJson_ReturnsNull()
    {
        // Arrange
        var json = "{ invalid json }";

        // Act
        var result = SQLGeneratorUtils.ParsePowerBITable(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParsePowerBITable_EmptyString_ReturnsNull()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.ParsePowerBITable("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GenerateGenericSQL_WithColumns_GeneratesCorrectSQL()
    {
        // Arrange
        var columns = new List<PowerBIColumn>
        {
            new PowerBIColumn { Name = "ID", DataType = "Int64" },
            new PowerBIColumn { Name = "Name", DataType = "String" },
            new PowerBIColumn { Name = "IsActive", DataType = "Boolean" }
        };

        // Act
        var result = SQLGeneratorUtils.GenerateGenericSQL(columns);

        // Assert
        Assert.Contains("SELECT", result);
        Assert.Contains("CAST(NULL AS BIGINT) AS [ID]", result);
        Assert.Contains("CAST(NULL AS NVARCHAR(MAX)) AS [Name]", result);
        Assert.Contains("CAST(NULL AS BIT) AS [IsActive]", result);
        Assert.Contains("FROM [YourTableName]", result);
    }

    [Fact]
    public void GenerateGenericSQL_WithMixedDataTypes_GeneratesCorrectSQL()
    {
        // Arrange
        var columns = new List<PowerBIColumn>
        {
            new PowerBIColumn { Name = "Date", DataType = "DateTime" },
            new PowerBIColumn { Name = "Price", DataType = "Decimal" },
            new PowerBIColumn { Name = "Quantity", DataType = "Int32" }
        };

        // Act
        var result = SQLGeneratorUtils.GenerateGenericSQL(columns);

        // Assert
        Assert.Contains("CAST(NULL AS DATETIME2) AS [Date]", result);
        Assert.Contains("CAST(NULL AS DECIMAL(18,2)) AS [Price]", result);
        Assert.Contains("CAST(NULL AS INT) AS [Quantity]", result);
    }

    [Fact]
    public void GenerateGenericSQL_WithNullDataType_UsesNVarcharMax()
    {
        // Arrange
        var columns = new List<PowerBIColumn>
        {
            new PowerBIColumn { Name = "UnknownColumn", DataType = null }
        };

        // Act
        var result = SQLGeneratorUtils.GenerateGenericSQL(columns);

        // Assert
        Assert.Contains("CAST(NULL AS NVARCHAR(MAX)) AS [UnknownColumn]", result);
    }

    [Fact]
    public void MapPowerBIToSQLType_CaseInsensitive_WorksCorrectly()
    {
        // Arrange & Act
        var result1 = SQLGeneratorUtils.MapPowerBIToSQLType("STRING");
        var result2 = SQLGeneratorUtils.MapPowerBIToSQLType("String");
        var result3 = SQLGeneratorUtils.MapPowerBIToSQLType("string");

        // Assert
        Assert.Equal("NVARCHAR(MAX)", result1);
        Assert.Equal("NVARCHAR(MAX)", result2);
        Assert.Equal("NVARCHAR(MAX)", result3);
    }

    [Fact]
    public void AreTypesCompatible_SameTypes_ReturnsTrue()
    {
        // Arrange & Act
        var result = SQLGeneratorUtils.AreTypesCompatible("int", "INT");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AreTypesCompatible_NVarcharVariations_ReturnsTrue()
    {
        // Arrange & Act
        var result1 = SQLGeneratorUtils.AreTypesCompatible("nvarchar", "NVARCHAR(MAX)");
        var result2 = SQLGeneratorUtils.AreTypesCompatible("nvarchar(100)", "NVARCHAR(MAX)");
        var result3 = SQLGeneratorUtils.AreTypesCompatible("ntext", "NVARCHAR(MAX)");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
    }

    [Fact]
    public void AreTypesCompatible_IntegerTypes_ReturnsTrue()
    {
        // Arrange & Act
        var result1 = SQLGeneratorUtils.AreTypesCompatible("int", "INT");
        var result2 = SQLGeneratorUtils.AreTypesCompatible("bigint", "BIGINT");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
    }

    [Fact]
    public void AreTypesCompatible_DecimalVariations_ReturnsTrue()
    {
        // Arrange & Act
        var result1 = SQLGeneratorUtils.AreTypesCompatible("decimal", "DECIMAL(18,2)");
        var result2 = SQLGeneratorUtils.AreTypesCompatible("numeric", "DECIMAL(18,2)");
        var result3 = SQLGeneratorUtils.AreTypesCompatible("money", "DECIMAL(18,2)");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
    }

    [Fact]
    public void AreTypesCompatible_DateTimeVariations_ReturnsTrue()
    {
        // Arrange & Act
        var result1 = SQLGeneratorUtils.AreTypesCompatible("datetime", "DATETIME2");
        var result2 = SQLGeneratorUtils.AreTypesCompatible("datetime2", "DATETIME2");
        var result3 = SQLGeneratorUtils.AreTypesCompatible("date", "DATETIME2");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
    }

    [Fact]
    public void AreTypesCompatible_IncompatibleTypes_ReturnsFalse()
    {
        // Arrange & Act
        var result1 = SQLGeneratorUtils.AreTypesCompatible("int", "NVARCHAR(MAX)");
        var result2 = SQLGeneratorUtils.AreTypesCompatible("datetime", "INT");

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }
}
