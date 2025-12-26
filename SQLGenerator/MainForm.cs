using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace SQLGenerator;

public partial class MainForm : Form
{
    private string? connectionString;
    private Dictionary<string, List<ColumnInfo>> tableSchemas = new();
    private bool isConnecting = false;

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

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        tableSchemas.Clear();
        lstTables.Items.Clear();

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

        txtStatus.AppendText($"Loaded {tableSchemas.Count} tables.\r\n");
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
        // Find the best matching table
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

    private string? FindBestMatchingTable(List<PowerBIColumn> powerBIColumns)
    {
        int maxMatches = 0;
        string? bestTable = null;

        foreach (var table in tableSchemas)
        {
            int matches = 0;
            foreach (var pbCol in powerBIColumns)
            {
                if (table.Value.Any(c => c.ColumnName.Equals(pbCol.Name, StringComparison.OrdinalIgnoreCase)))
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
        sql.AppendLine("SELECT");
        
        var selectColumns = new List<string>();
        
        foreach (var pbCol in powerBIColumns)
        {
            var matchingColumn = schema.FirstOrDefault(c => 
                c.ColumnName.Equals(pbCol.Name, StringComparison.OrdinalIgnoreCase));
            
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
        sql.AppendLine($"FROM [{tableName}];");
        
        return sql.ToString();
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
                
                var columns = ParseCSVFile(openFileDialog.FileName);
                
                if (columns != null && columns.Count > 0)
                {
                    var tableDefinition = new PowerBITableDefinition
                    {
                        Name = Path.GetFileNameWithoutExtension(openFileDialog.FileName),
                        Columns = columns
                    };

                    var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    txtPowerBITable.Text = JsonSerializer.Serialize(tableDefinition, options);
                    
                    txtStatus.AppendText($"Successfully loaded {columns.Count} columns from CSV.\r\n");
                    MessageBox.Show($"CSV file loaded successfully!\r\nDetected {columns.Count} columns.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
