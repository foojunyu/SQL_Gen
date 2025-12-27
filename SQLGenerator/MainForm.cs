using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace SQLGenerator;

public partial class MainForm : Form
{
    private string? connectionString;
    private Dictionary<string, List<ColumnInfo>> tableSchemas = new();
    private Dictionary<string, List<ForeignKeyInfo>> foreignKeys = new(); // Store foreign key relationships
    private bool isConnecting = false;
    private List<Dictionary<string, string>>? csvData = null; // Store CSV data rows
    private string? csvFilePath = null; // Store the CSV file path

    public MainForm()
    {
        InitializeComponent();
    }

    private async void btnConnect_Click(object sender, EventArgs e)
    {
        // Prevent multiple concurrent connection attempts
        if (isConnecting)
        {
            MessageBox.Show("A connection attempt is already in progress.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            isConnecting = true;
            btnConnect.Enabled = false;
            
            txtStatus.AppendText("Connecting to Azure Fabric Data Warehouse...\r\n");
            connectionString = txtConnectionString.Text.Trim();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("Please enter a connection string.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate connection string security
            if (!SQLGeneratorUtils.ValidateConnectionStringSecurity(connectionString))
            {
                MessageBox.Show("Connection string must include 'Encrypt=True' for secure connections.", "Security Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStatus.AppendText("Warning: Connection string should use encryption.\r\n");
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            txtStatus.AppendText("Connected successfully!\r\n");
            
            // Load available tables
            await LoadTablesAsync(connection);
            
            btnGenerateSQL.Enabled = true;
            MessageBox.Show("Connected successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (SqlException sqlEx)
        {
            txtStatus.AppendText($"SQL Connection failed: {sqlEx.Message}\r\n");
            MessageBox.Show($"SQL Connection failed: {sqlEx.Message}\r\nPlease verify your connection string and credentials.", "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            txtStatus.AppendText($"Connection failed: {ex.Message}\r\n");
            MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            isConnecting = false;
            btnConnect.Enabled = true;
        }
    }

    private async Task LoadTablesAsync(SqlConnection connection)
    {
        txtStatus.AppendText("Loading available tables...\r\n");
        
        string query = @"
            SELECT 
                SCHEMA_NAME(t.schema_id) AS SchemaName,
                t.name AS TableName,
                c.name AS ColumnName,
                TYPE_NAME(c.user_type_id) AS DataType,
                c.max_length AS MaxLength,
                c.precision AS Precision,
                c.scale AS Scale,
                c.is_nullable AS IsNullable
            FROM sys.tables t
            INNER JOIN sys.columns c ON t.object_id = c.object_id
            ORDER BY t.name, c.column_id";

        tableSchemas.Clear();
        lstTables.Items.Clear();

        // Scope the DataReader to ensure it's disposed before calling LoadForeignKeysAsync
        using (var command = new SqlCommand(query, connection))
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                string schemaName = reader.GetString(0);
                string tableName = reader.GetString(1);
                string fullTableName = $"{schemaName}.{tableName}";

                if (!tableSchemas.ContainsKey(fullTableName))
                {
                    tableSchemas[fullTableName] = new List<ColumnInfo>();
                    lstTables.Items.Add(fullTableName);
                }

                tableSchemas[fullTableName].Add(new ColumnInfo
                {
                    ColumnName = reader.GetString(2),
                    DataType = reader.GetString(3),
                    MaxLength = reader.GetInt16(4),
                    Precision = reader.GetByte(5),
                    Scale = reader.GetByte(6),
                    IsNullable = reader.GetBoolean(7)
                });
            }
        } // DataReader is properly disposed here

        txtStatus.AppendText($"Loaded {tableSchemas.Count} tables.\r\n");
        
        // Load foreign key relationships - DataReader from above is now closed
        await LoadForeignKeysAsync(connection);
    }

    private async Task LoadForeignKeysAsync(SqlConnection connection)
    {
        txtStatus.AppendText("Loading foreign key relationships...\r\n");
        
        string query = @"
            SELECT 
                SCHEMA_NAME(fk.schema_id) + '.' + OBJECT_NAME(fk.parent_object_id) AS ParentTable,
                COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ParentColumn,
                SCHEMA_NAME(ref_obj.schema_id) + '.' + OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
                COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn,
                fk.name AS ConstraintName
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            INNER JOIN sys.objects ref_obj ON fk.referenced_object_id = ref_obj.object_id
            ORDER BY ParentTable, ReferencedTable";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        foreignKeys.Clear();

        while (await reader.ReadAsync())
        {
            string parentTable = reader.GetString(0);
            string parentColumn = reader.GetString(1);
            string referencedTable = reader.GetString(2);
            string referencedColumn = reader.GetString(3);
            string constraintName = reader.GetString(4);

            if (!foreignKeys.ContainsKey(parentTable))
            {
                foreignKeys[parentTable] = new List<ForeignKeyInfo>();
            }

            foreignKeys[parentTable].Add(new ForeignKeyInfo
            {
                ParentTable = parentTable,
                ParentColumn = parentColumn,
                ReferencedTable = referencedTable,
                ReferencedColumn = referencedColumn,
                ConstraintName = constraintName
            });
        }

        txtStatus.AppendText($"Loaded {foreignKeys.Values.Sum(list => list.Count)} foreign key relationships.\r\n");
    }

    private void btnGenerateSQL_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtPowerBITable.Text))
            {
                MessageBox.Show("Please enter Power BI table definition (JSON format).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Parse Power BI table definition
            var powerBIColumns = SQLGeneratorUtils.ParsePowerBITable(txtPowerBITable.Text);
            
            if (powerBIColumns == null || powerBIColumns.Count == 0)
            {
                MessageBox.Show("Invalid Power BI table definition. Please check the JSON format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Generate SQL based on Power BI table structure
            string sql = GenerateSQLForPowerBI(powerBIColumns);
            
            txtGeneratedSQL.Text = sql;
            txtStatus.AppendText("SQL generated successfully!\r\n");
        }
        catch (JsonException jsonEx)
        {
            txtStatus.AppendText($"JSON parsing error: {jsonEx.Message}\r\n");
            MessageBox.Show($"Invalid JSON format: {jsonEx.Message}", "JSON Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            txtStatus.AppendText($"Error generating SQL: {ex.Message}\r\n");
            MessageBox.Show($"Error generating SQL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string GenerateSQLForPowerBI(List<PowerBIColumn> powerBIColumns)
    {
        // Try to find columns across multiple tables (multi-table scenario)
        var tableColumnMappings = FindColumnsAcrossTables(powerBIColumns);
        
        if (tableColumnMappings.Count > 0)
        {
            var uniqueTables = tableColumnMappings.Select(m => m.TableName).Distinct().ToList();
            
            if (uniqueTables.Count > 1)
            {
                // Multi-table scenario - generate JOIN query
                txtStatus.AppendText($"Detected columns from {uniqueTables.Count} tables. Generating JOIN query...\r\n");
                return GenerateMultiTableSQL(powerBIColumns, tableColumnMappings);
            }
            else if (uniqueTables.Count == 1)
            {
                // Single table - use existing logic
                return GenerateSQLForSpecificTable(uniqueTables[0], powerBIColumns);
            }
        }
        
        // Fallback to original logic
        string? selectedTable = lstTables.SelectedItem?.ToString();
        
        if (!string.IsNullOrEmpty(selectedTable) && tableSchemas.ContainsKey(selectedTable))
        {
            return GenerateSQLForSpecificTable(selectedTable, powerBIColumns);
        }
        
        // If no table selected, try to find best match
        string? bestMatch = FindBestMatchingTable(powerBIColumns);
        
        if (bestMatch != null)
        {
            txtStatus.AppendText($"Auto-matched to table: {bestMatch}\r\n");
            return GenerateSQLForSpecificTable(bestMatch, powerBIColumns);
        }
        
        // Generate generic SQL
        return SQLGeneratorUtils.GenerateGenericSQL(powerBIColumns);
    }

    private List<TableColumnMapping> FindColumnsAcrossTables(List<PowerBIColumn> powerBIColumns)
    {
        var mappings = new List<TableColumnMapping>();
        
        foreach (var pbCol in powerBIColumns)
        {
            // Try to find this column in any table
            foreach (var table in tableSchemas)
            {
                var matchingColumn = FindMatchingColumn(table.Value, pbCol.Name);
                if (matchingColumn != null)
                {
                    mappings.Add(new TableColumnMapping
                    {
                        TableName = table.Key,
                        ColumnName = matchingColumn.ColumnName,
                        CSVColumnName = pbCol.Name
                    });
                    break; // Found a match, move to next CSV column
                }
            }
        }
        
        return mappings;
    }

    private string GenerateMultiTableSQL(List<PowerBIColumn> powerBIColumns, List<TableColumnMapping> mappings)
    {
        var sql = new System.Text.StringBuilder();
        var uniqueTables = mappings.Select(m => m.TableName).Distinct().ToList();
        
        if (uniqueTables.Count == 0) return string.Empty;
        
        // Find join path between tables
        var joinPath = FindJoinPath(uniqueTables);
        
        sql.AppendLine($"-- Multi-table SQL Query matching CSV structure");
        sql.AppendLine($"-- Tables involved: {string.Join(", ", uniqueTables)}");
        sql.AppendLine();
        
        // Add data validation if CSV data exists
        if (csvData != null && csvData.Count > 0 && !string.IsNullOrEmpty(connectionString))
        {
            sql.AppendLine("-- Data Validation: Check if CSV values exist in database");
            foreach (var mapping in mappings)
            {
                var schema = tableSchemas[mapping.TableName];
                sql.AppendLine(GenerateDataValidationSQLForColumn(mapping.TableName, mapping.CSVColumnName, schema));
            }
            sql.AppendLine();
        }
        
        // Generate SELECT clause
        sql.AppendLine("-- Main SELECT query with JOINs");
        sql.AppendLine("SELECT");
        
        var selectColumns = new List<string>();
        foreach (var pbCol in powerBIColumns)
        {
            var mapping = mappings.FirstOrDefault(m => m.CSVColumnName == pbCol.Name);
            if (mapping != null)
            {
                string tableAlias = GetTableAlias(mapping.TableName);
                selectColumns.Add($"    {tableAlias}.[{mapping.ColumnName}] AS [{pbCol.Name}]");
            }
            else
            {
                selectColumns.Add($"    NULL AS [{pbCol.Name}] -- Column not found");
            }
        }
        
        sql.AppendLine(string.Join(",\r\n", selectColumns));
        
        // Generate FROM and JOIN clauses
        if (joinPath.Count > 0)
        {
            string firstTable = joinPath[0].Item1;
            sql.AppendLine($"FROM [{firstTable}] {GetTableAlias(firstTable)}");
            
            for (int i = 0; i < joinPath.Count; i++)
            {
                var (fromTable, toTable, fromCol, toCol) = joinPath[i];
                string fromAlias = GetTableAlias(fromTable);
                string toAlias = GetTableAlias(toTable);
                sql.AppendLine($"INNER JOIN [{toTable}] {toAlias} ON {fromAlias}.[{fromCol}] = {toAlias}.[{toCol}]");
            }
        }
        else if (uniqueTables.Count == 1)
        {
            sql.AppendLine($"FROM [{uniqueTables[0]}] {GetTableAlias(uniqueTables[0])}");
        }
        
        // Add WHERE clause if CSV data exists
        if (csvData != null && csvData.Count > 0)
        {
            sql.AppendLine(GenerateWhereClauseForMultiTable(mappings));
        }
        else
        {
            sql.AppendLine(";");
        }
        
        // Add sample data query
        sql.AppendLine();
        sql.AppendLine("-- Sample data from main table");
        sql.AppendLine($"SELECT TOP 5 * FROM [{uniqueTables[0]}];");
        
        return sql.ToString();
    }

    private List<(string, string, string, string)> FindJoinPath(List<string> tables)
    {
        // Find the optimal join path between tables using foreign keys
        var joinPath = new List<(string, string, string, string)>();
        
        if (tables.Count <= 1) return joinPath;
        
        var connected = new HashSet<string> { tables[0] };
        var remaining = new HashSet<string>(tables.Skip(1));
        
        while (remaining.Count > 0)
        {
            bool foundJoin = false;
            
            foreach (var connectedTable in connected.ToList())
            {
                // Check foreign keys from connected table
                if (foreignKeys.ContainsKey(connectedTable))
                {
                    foreach (var fk in foreignKeys[connectedTable])
                    {
                        if (remaining.Contains(fk.ReferencedTable))
                        {
                            joinPath.Add((connectedTable, fk.ReferencedTable, fk.ParentColumn, fk.ReferencedColumn));
                            connected.Add(fk.ReferencedTable);
                            remaining.Remove(fk.ReferencedTable);
                            foundJoin = true;
                            break;
                        }
                    }
                }
                
                // Check foreign keys TO connected table
                foreach (var kvp in foreignKeys)
                {
                    if (remaining.Contains(kvp.Key))
                    {
                        foreach (var fk in kvp.Value)
                        {
                            if (fk.ReferencedTable == connectedTable)
                            {
                                joinPath.Add((connectedTable, kvp.Key, fk.ReferencedColumn, fk.ParentColumn));
                                connected.Add(kvp.Key);
                                remaining.Remove(kvp.Key);
                                foundJoin = true;
                                break;
                            }
                        }
                        if (foundJoin) break;
                    }
                }
                
                if (foundJoin) break;
            }
            
            if (!foundJoin)
            {
                // No direct relationship found, add remaining tables without join
                txtStatus.AppendText($"Warning: Could not find join path for all tables. Some tables may not be properly connected.\r\n");
                break;
            }
        }
        
        return joinPath;
    }

    private string GetTableAlias(string fullTableName)
    {
        // Generate simple alias from table name (e.g., "dbo.Customers" -> "c")
        var parts = fullTableName.Split('.');
        string tableName = parts.Length > 1 ? parts[1] : parts[0];
        return tableName.Substring(0, 1).ToLower();
    }

    private string GenerateDataValidationSQLForColumn(string tableName, string csvColumnName, List<ColumnInfo> schema)
    {
        if (csvData == null || csvData.Count == 0) return string.Empty;
        
        var mapping = schema.FirstOrDefault(c => c.ColumnName.Equals(csvColumnName, StringComparison.OrdinalIgnoreCase));
        if (mapping == null) return string.Empty;
        
        var uniqueValues = csvData
            .Select(row => row.ContainsKey(csvColumnName) ? row[csvColumnName] : "")
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct()
            .Take(10)
            .ToList();
        
        if (uniqueValues.Count > 0)
        {
            var valueList = uniqueValues.Select(v => $"'{v.Replace("'", "''")}'");
            return $"SELECT DISTINCT [{mapping.ColumnName}] FROM [{tableName}] WHERE [{mapping.ColumnName}] IN ({string.Join(", ", valueList)});";
        }
        
        return string.Empty;
    }

    private string GenerateWhereClauseForMultiTable(List<TableColumnMapping> mappings)
    {
        if (csvData == null || csvData.Count == 0) return ";";
        
        var whereConditions = new List<string>();
        
        foreach (var mapping in mappings)
        {
            var uniqueValues = csvData
                .Select(row => row.ContainsKey(mapping.CSVColumnName) ? row[mapping.CSVColumnName] : "")
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .Take(20)
                .ToList();
            
            if (uniqueValues.Count > 0 && uniqueValues.Count <= 20)
            {
                string tableAlias = GetTableAlias(mapping.TableName);
                var valueList = uniqueValues.Select(v => $"'{v.Replace("'", "''")}'");
                whereConditions.Add($"    {tableAlias}.[{mapping.ColumnName}] IN ({string.Join(", ", valueList)})");
            }
        }
        
        if (whereConditions.Count > 0)
        {
            return "WHERE\r\n" + string.Join(" OR\r\n", whereConditions) + ";";
        }
        
        return ";";
    }

    private string? FindBestMatchingTable(List<PowerBIColumn> powerBIColumns)
    {
        int maxMatches = 0;
        string? bestTable = null;

        foreach (var table in tableSchemas)
        {
            int matches = 0;
            foreach (var pbCol in powerBIColumns)
            {
                // Use the improved matching logic
                if (FindMatchingColumn(table.Value, pbCol.Name) != null)
                {
                    matches++;
                }
            }

            if (matches > maxMatches)
            {
                maxMatches = matches;
                bestTable = table.Key;
            }
        }

        return maxMatches > 0 ? bestTable : null;
    }

    private string GenerateSQLForSpecificTable(string tableName, List<PowerBIColumn> powerBIColumns)
    {
        var schema = tableSchemas[tableName];
        var sql = new System.Text.StringBuilder();
        
        sql.AppendLine($"-- SQL Query to match Power BI table structure from {tableName}");
        
        // If we have CSV data, add data validation query first
        if (csvData != null && csvData.Count > 0 && !string.IsNullOrEmpty(connectionString))
        {
            sql.AppendLine("-- Data Validation: Check if CSV values exist in database");
            sql.AppendLine(GenerateDataValidationSQL(tableName, powerBIColumns, schema));
            sql.AppendLine();
        }
        
        sql.AppendLine("-- Main SELECT query");
        sql.AppendLine("SELECT");
        
        var selectColumns = new List<string>();
        
        foreach (var pbCol in powerBIColumns)
        {
            // Try multiple matching strategies
            var matchingColumn = FindMatchingColumn(schema, pbCol.Name);
            
            if (matchingColumn != null)
            {
                string columnExpr = $"    [{matchingColumn.ColumnName}]";
                
                // Add type conversion if needed
                if (!string.IsNullOrEmpty(pbCol.DataType))
                {
                    string sqlType = SQLGeneratorUtils.MapPowerBIToSQLType(pbCol.DataType);
                    if (!SQLGeneratorUtils.AreTypesCompatible(matchingColumn.DataType, sqlType))
                    {
                        columnExpr = $"    CAST([{matchingColumn.ColumnName}] AS {sqlType})";
                    }
                }
                
                columnExpr += $" AS [{pbCol.Name}]";
                selectColumns.Add(columnExpr);
            }
            else
            {
                // Column not found, add as NULL
                selectColumns.Add($"    NULL AS [{pbCol.Name}] -- Column not found in source table");
            }
        }
        
        sql.AppendLine(string.Join(",\r\n", selectColumns));
        sql.AppendLine($"FROM [{tableName}]");
        
        // Add WHERE clause if we have CSV data
        if (csvData != null && csvData.Count > 0)
        {
            sql.AppendLine(GenerateWhereClause(powerBIColumns, schema));
        }
        else
        {
            sql.AppendLine(";");
        }
        
        // Add sample data query
        sql.AppendLine();
        sql.AppendLine("-- Sample data from database (top 5 rows)");
        sql.AppendLine($"SELECT TOP 5 * FROM [{tableName}];");
        
        return sql.ToString();
    }

    private string GenerateDataValidationSQL(string tableName, List<PowerBIColumn> powerBIColumns, List<ColumnInfo> schema)
    {
        var sql = new System.Text.StringBuilder();
        
        if (csvData == null || csvData.Count == 0) return string.Empty;
        
        // For each column in CSV, check if values exist in database
        foreach (var pbCol in powerBIColumns)
        {
            var matchingColumn = FindMatchingColumn(schema, pbCol.Name);
            if (matchingColumn == null) continue;
            
            // Get unique values from CSV for this column
            var uniqueValues = csvData
                .Select(row => row.ContainsKey(pbCol.Name) ? row[pbCol.Name] : "")
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .Take(10) // Limit to first 10 unique values
                .ToList();
            
            if (uniqueValues.Count > 0)
            {
                sql.AppendLine($"-- Check if values for '{pbCol.Name}' exist:");
                sql.Append($"SELECT DISTINCT [{matchingColumn.ColumnName}] FROM [{tableName}] WHERE [{matchingColumn.ColumnName}] IN (");
                
                var valueList = uniqueValues.Select(v => $"'{v.Replace("'", "''")}'");
                sql.Append(string.Join(", ", valueList));
                sql.AppendLine(");");
            }
        }
        
        return sql.ToString();
    }

    private string GenerateWhereClause(List<PowerBIColumn> powerBIColumns, List<ColumnInfo> schema)
    {
        if (csvData == null || csvData.Count == 0) return ";";
        
        var whereConditions = new List<string>();
        
        // Build WHERE clause based on CSV data
        foreach (var pbCol in powerBIColumns)
        {
            var matchingColumn = FindMatchingColumn(schema, pbCol.Name);
            if (matchingColumn == null) continue;
            
            // Get unique values from CSV (limit to reasonable number)
            var uniqueValues = csvData
                .Select(row => row.ContainsKey(pbCol.Name) ? row[pbCol.Name] : "")
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .Take(100) // Limit to first 100 unique values
                .ToList();
            
            if (uniqueValues.Count > 0 && uniqueValues.Count <= 20) // Only add if reasonable number
            {
                var valueList = uniqueValues.Select(v => $"'{v.Replace("'", "''")}'");
                whereConditions.Add($"    [{matchingColumn.ColumnName}] IN ({string.Join(", ", valueList)})");
            }
        }
        
        if (whereConditions.Count > 0)
        {
            return "WHERE\r\n" + string.Join(" OR\r\n", whereConditions) + ";";
        }
        
        return ";";
    }

    private ColumnInfo? FindMatchingColumn(List<ColumnInfo> schema, string searchName)
    {
        // Strategy 1: Exact case-insensitive match
        var match = schema.FirstOrDefault(c => 
            c.ColumnName.Equals(searchName, StringComparison.OrdinalIgnoreCase));
        if (match != null) return match;

        // Strategy 2: Match after removing spaces and underscores
        var normalizedSearch = NormalizeColumnName(searchName);
        match = schema.FirstOrDefault(c => 
            NormalizeColumnName(c.ColumnName).Equals(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        if (match != null) return match;

        // Strategy 3: Partial match (search name is contained in column name or vice versa)
        match = schema.FirstOrDefault(c => 
            c.ColumnName.Contains(searchName, StringComparison.OrdinalIgnoreCase) ||
            searchName.Contains(c.ColumnName, StringComparison.OrdinalIgnoreCase));
        if (match != null) return match;

        // Strategy 4: Match normalized partial strings
        match = schema.FirstOrDefault(c =>
        {
            var normalizedCol = NormalizeColumnName(c.ColumnName);
            return normalizedCol.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                   normalizedSearch.Contains(normalizedCol, StringComparison.OrdinalIgnoreCase);
        });

        return match;
    }

    private string NormalizeColumnName(string name)
    {
        // Remove spaces, underscores, hyphens, and other separators
        return name.Replace(" ", "")
                   .Replace("_", "")
                   .Replace("-", "")
                   .Replace(".", "");
    }

    private void btnCopy_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtGeneratedSQL.Text))
        {
            Clipboard.SetText(txtGeneratedSQL.Text);
            MessageBox.Show("SQL copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        txtGeneratedSQL.Clear();
        txtStatus.Clear();
    }

    private void lstTables_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lstTables.SelectedItem != null)
        {
            string tableName = lstTables.SelectedItem.ToString()!;
            if (tableSchemas.ContainsKey(tableName))
            {
                var columns = tableSchemas[tableName];
                txtStatus.AppendText($"\r\nTable: {tableName}\r\n");
                txtStatus.AppendText("Columns:\r\n");
                foreach (var col in columns)
                {
                    txtStatus.AppendText($"  - {col.ColumnName} ({col.DataType})\r\n");
                }
            }
        }
    }

    private void btnLoadExample_Click(object sender, EventArgs e)
    {
        var example = new PowerBITableDefinition
        {
            Name = "SalesData",
            Columns = new List<PowerBIColumn>
            {
                new PowerBIColumn { Name = "OrderID", DataType = "Int64" },
                new PowerBIColumn { Name = "CustomerName", DataType = "String" },
                new PowerBIColumn { Name = "OrderDate", DataType = "DateTime" },
                new PowerBIColumn { Name = "TotalAmount", DataType = "Decimal" },
                new PowerBIColumn { Name = "IsActive", DataType = "Boolean" }
            }
        };

        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        txtPowerBITable.Text = JsonSerializer.Serialize(example, options);
    }

    private void btnLoadCSV_Click(object sender, EventArgs e)
    {
        try
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Select CSV File",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtStatus.AppendText($"Loading CSV file: {Path.GetFileName(openFileDialog.FileName)}...\r\n");
                
                csvFilePath = openFileDialog.FileName;
                var (columns, data) = ParseCSVFileWithData(csvFilePath);
                
                if (columns != null && columns.Count > 0)
                {
                    csvData = data; // Store the CSV data
                    
                    var tableDefinition = new PowerBITableDefinition
                    {
                        Name = Path.GetFileNameWithoutExtension(openFileDialog.FileName),
                        Columns = columns
                    };

                    var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    txtPowerBITable.Text = JsonSerializer.Serialize(tableDefinition, options);
                    
                    txtStatus.AppendText($"Successfully loaded {columns.Count} columns and {data?.Count ?? 0} data rows from CSV.\r\n");
                    MessageBox.Show($"CSV file loaded successfully!\r\nDetected {columns.Count} columns and {data?.Count ?? 0} data rows.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtStatus.AppendText("Failed to parse CSV file or no columns detected.\r\n");
                    MessageBox.Show("Failed to parse CSV file. Please ensure it has a header row.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            txtStatus.AppendText($"Error loading CSV: {ex.Message}\r\n");
            MessageBox.Show($"Error loading CSV file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private List<PowerBIColumn>? ParseCSVFile(string filePath)
    {
        try
        {
            var columns = new List<PowerBIColumn>();
            
            using var reader = new StreamReader(filePath);
            
            // Read header line
            var headerLine = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return null;
            }

            var headers = ParseCSVLine(headerLine);
            
            // Read first data row to infer types
            var firstDataLine = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(firstDataLine))
            {
                // No data rows, just use headers with default String type
                foreach (var header in headers)
                {
                    columns.Add(new PowerBIColumn { Name = header.Trim(), DataType = "String" });
                }
            }
            else
            {
                var values = ParseCSVLine(firstDataLine);
                
                for (int i = 0; i < headers.Length; i++)
                {
                    var columnName = headers[i].Trim();
                    var dataType = "String"; // Default
                    
                    if (i < values.Length)
                    {
                        dataType = InferDataType(values[i]);
                    }
                    
                    columns.Add(new PowerBIColumn { Name = columnName, DataType = dataType });
                }
            }
            
            return columns;
        }
        catch
        {
            return null;
        }
    }

    private (List<PowerBIColumn>?, List<Dictionary<string, string>>?) ParseCSVFileWithData(string filePath)
    {
        try
        {
            var columns = new List<PowerBIColumn>();
            var dataRows = new List<Dictionary<string, string>>();
            
            using var reader = new StreamReader(filePath);
            
            // Read header line
            var headerLine = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return (null, null);
            }

            var headers = ParseCSVLine(headerLine);
            
            // Read all data rows
            string? line;
            bool firstRow = true;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                var values = ParseCSVLine(line);
                var row = new Dictionary<string, string>();
                
                for (int i = 0; i < headers.Length; i++)
                {
                    var columnName = headers[i].Trim();
                    var value = i < values.Length ? values[i].Trim() : string.Empty;
                    row[columnName] = value;
                    
                    // Infer data type from first row
                    if (firstRow)
                    {
                        var dataType = InferDataType(value);
                        columns.Add(new PowerBIColumn { Name = columnName, DataType = dataType });
                    }
                }
                
                dataRows.Add(row);
                firstRow = false;
            }
            
            // If no data rows, use headers with default String type
            if (columns.Count == 0)
            {
                foreach (var header in headers)
                {
                    columns.Add(new PowerBIColumn { Name = header.Trim(), DataType = "String" });
                }
            }
            
            return (columns, dataRows);
        }
        catch
        {
            return (null, null);
        }
    }

    private string[] ParseCSVLine(string line)
    {
        var values = new List<string>();
        var currentValue = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        values.Add(currentValue.ToString());
        return values.ToArray();
    }

    private string InferDataType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "String";

        value = value.Trim();

        // Try integer
        if (int.TryParse(value, out _))
            return "Int32";

        // Try long
        if (long.TryParse(value, out _))
            return "Int64";

        // Try decimal
        if (decimal.TryParse(value, out _))
            return "Decimal";

        // Try boolean
        if (bool.TryParse(value, out _))
            return "Boolean";

        // Try datetime
        if (DateTime.TryParse(value, out _))
            return "DateTime";

        // Default to string
        return "String";
    }
}

public class ColumnInfo
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public short MaxLength { get; set; }
    public byte Precision { get; set; }
    public byte Scale { get; set; }
    public bool IsNullable { get; set; }
}

public class PowerBITableDefinition
{
    public string Name { get; set; } = string.Empty;
    public List<PowerBIColumn> Columns { get; set; } = new();
}

public class PowerBIColumn
{
    public string Name { get; set; } = string.Empty;
    public string? DataType { get; set; }
}

public class ForeignKeyInfo
{
    public string ParentTable { get; set; } = string.Empty;
    public string ParentColumn { get; set; } = string.Empty;
    public string ReferencedTable { get; set; } = string.Empty;
    public string ReferencedColumn { get; set; } = string.Empty;
    public string ConstraintName { get; set; } = string.Empty;
}

public class TableColumnMapping
{
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string CSVColumnName { get; set; } = string.Empty;
}
