using System.Text.Json;

namespace SQLGenerator;

/// <summary>
/// Utility class for SQL generation and Power BI table parsing
/// </summary>
public static class SQLGeneratorUtils
{
    /// <summary>
    /// Maps Power BI data types to SQL Server data types
    /// </summary>
    public static string MapPowerBIToSQLType(string powerBIType)
    {
        return powerBIType.ToLower() switch
        {
            "string" or "text" => "NVARCHAR(MAX)",
            "int64" or "integer" => "BIGINT",
            "int32" => "INT",
            "decimal" or "number" => "DECIMAL(18,2)",
            "double" => "FLOAT",
            "datetime" or "date" => "DATETIME2",
            "boolean" or "bool" => "BIT",
            "binary" => "VARBINARY(MAX)",
            _ => "NVARCHAR(MAX)"
        };
    }

    /// <summary>
    /// Checks if two SQL data types are compatible (basic type matching)
    /// </summary>
    public static bool AreTypesCompatible(string sqlServerType, string targetType)
    {
        // Normalize both types to lowercase for comparison
        var sqlType = sqlServerType.ToLower();
        var target = targetType.ToLower();
        
        // Direct match
        if (sqlType == target)
            return true;
            
        // Check base type compatibility
        // NVARCHAR variations
        if ((sqlType.Contains("nvarchar") || sqlType == "ntext") && target.Contains("nvarchar"))
            return true;
            
        // INT variations
        if ((sqlType == "int" || sqlType == "integer") && target == "int")
            return true;
            
        // BIGINT variations
        if ((sqlType == "bigint") && target == "bigint")
            return true;
            
        // DECIMAL/NUMERIC variations
        if ((sqlType.Contains("decimal") || sqlType.Contains("numeric") || sqlType == "money") && 
            target.Contains("decimal"))
            return true;
            
        // FLOAT/REAL variations
        if ((sqlType == "float" || sqlType == "real") && target == "float")
            return true;
            
        // DATE/DATETIME variations
        if ((sqlType.Contains("datetime") || sqlType == "date" || sqlType == "smalldatetime") && 
            target.Contains("datetime"))
            return true;
            
        // BIT/BOOLEAN
        if (sqlType == "bit" && target == "bit")
            return true;
            
        // BINARY variations
        if ((sqlType.Contains("varbinary") || sqlType == "binary") && target.Contains("varbinary"))
            return true;
            
        return false;
    }

    /// <summary>
    /// Validates that a connection string includes encryption
    /// </summary>
    public static bool ValidateConnectionStringSecurity(string connString)
    {
        if (string.IsNullOrWhiteSpace(connString))
            return false;
            
        return connString.Contains("Encrypt=True", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses Power BI table definition from JSON
    /// </summary>
    public static List<PowerBIColumn>? ParsePowerBITable(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var powerBITable = JsonSerializer.Deserialize<PowerBITableDefinition>(json, options);
            return powerBITable?.Columns;
        }
        catch (JsonException)
        {
            // Try parsing as just an array of columns
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<PowerBIColumn>>(json, options);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Generates a generic SQL query based on Power BI columns
    /// </summary>
    public static string GenerateGenericSQL(List<PowerBIColumn> powerBIColumns)
    {
        var sql = new System.Text.StringBuilder();
        
        sql.AppendLine("-- Generic SQL Query to match Power BI table structure");
        sql.AppendLine("-- Please replace [YourTableName] with the actual table name");
        sql.AppendLine("SELECT");
        
        var selectColumns = new List<string>();
        
        foreach (var pbCol in powerBIColumns)
        {
            string sqlType = MapPowerBIToSQLType(pbCol.DataType ?? "String");
            selectColumns.Add($"    CAST(NULL AS {sqlType}) AS [{pbCol.Name}]");
        }
        
        sql.AppendLine(string.Join(",\r\n", selectColumns));
        sql.AppendLine("FROM [YourTableName];");
        
        return sql.ToString();
    }
}
