namespace SQLGenerator;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.grpConnection = new System.Windows.Forms.GroupBox();
        this.btnConnect = new System.Windows.Forms.Button();
        this.txtConnectionString = new System.Windows.Forms.TextBox();
        this.lblConnectionString = new System.Windows.Forms.Label();
        
        this.grpPowerBI = new System.Windows.Forms.GroupBox();
        this.btnLoadExample = new System.Windows.Forms.Button();
        this.btnLoadCSV = new System.Windows.Forms.Button();
        this.txtPowerBITable = new System.Windows.Forms.TextBox();
        this.lblPowerBITable = new System.Windows.Forms.Label();
        
        this.grpTables = new System.Windows.Forms.GroupBox();
        this.lstTables = new System.Windows.Forms.ListBox();
        
        this.grpSQL = new System.Windows.Forms.GroupBox();
        this.btnCopy = new System.Windows.Forms.Button();
        this.btnGenerateSQL = new System.Windows.Forms.Button();
        this.txtGeneratedSQL = new System.Windows.Forms.TextBox();
        
        this.grpStatus = new System.Windows.Forms.GroupBox();
        this.btnClear = new System.Windows.Forms.Button();
        this.txtStatus = new System.Windows.Forms.TextBox();
        
        this.grpConnection.SuspendLayout();
        this.grpPowerBI.SuspendLayout();
        this.grpTables.SuspendLayout();
        this.grpSQL.SuspendLayout();
        this.grpStatus.SuspendLayout();
        this.SuspendLayout();
        
        // 
        // grpConnection
        // 
        this.grpConnection.Controls.Add(this.btnConnect);
        this.grpConnection.Controls.Add(this.txtConnectionString);
        this.grpConnection.Controls.Add(this.lblConnectionString);
        this.grpConnection.Location = new System.Drawing.Point(12, 12);
        this.grpConnection.Name = "grpConnection";
        this.grpConnection.Size = new System.Drawing.Size(760, 90);
        this.grpConnection.TabIndex = 0;
        this.grpConnection.TabStop = false;
        this.grpConnection.Text = "Azure Fabric Connection";
        
        // 
        // lblConnectionString
        // 
        this.lblConnectionString.AutoSize = true;
        this.lblConnectionString.Location = new System.Drawing.Point(6, 25);
        this.lblConnectionString.Name = "lblConnectionString";
        this.lblConnectionString.Size = new System.Drawing.Size(110, 15);
        this.lblConnectionString.TabIndex = 0;
        this.lblConnectionString.Text = "Connection String:";
        
        // 
        // txtConnectionString
        // 
        this.txtConnectionString.Location = new System.Drawing.Point(6, 43);
        this.txtConnectionString.Name = "txtConnectionString";
        this.txtConnectionString.Size = new System.Drawing.Size(640, 23);
        this.txtConnectionString.TabIndex = 1;
        this.txtConnectionString.Text = "Data Source=kcrbz3buyjle3j3lpx5z53qmiy-jfvyjn4f25ee5h2ktlg2ym4qxq.datawarehouse.fabric.microsoft.com;Initial Catalog=LH_BE_PPC;User ID=Jun-Yu.Foo@ams-osram.com;Pooling=False;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Authentication=ActiveDirectoryInteractive;Application Name=vscode-mssql;Application Intent=ReadWrite;Command Timeout=30";
        
        // 
        // btnConnect
        // 
        this.btnConnect.Location = new System.Drawing.Point(652, 42);
        this.btnConnect.Name = "btnConnect";
        this.btnConnect.Size = new System.Drawing.Size(100, 25);
        this.btnConnect.TabIndex = 2;
        this.btnConnect.Text = "Connect";
        this.btnConnect.UseVisualStyleBackColor = true;
        this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
        
        // 
        // grpTables
        // 
        this.grpTables.Controls.Add(this.lstTables);
        this.grpTables.Location = new System.Drawing.Point(12, 108);
        this.grpTables.Name = "grpTables";
        this.grpTables.Size = new System.Drawing.Size(200, 380);
        this.grpTables.TabIndex = 1;
        this.grpTables.TabStop = false;
        this.grpTables.Text = "Available Tables";
        
        // 
        // lstTables
        // 
        this.lstTables.FormattingEnabled = true;
        this.lstTables.ItemHeight = 15;
        this.lstTables.Location = new System.Drawing.Point(6, 22);
        this.lstTables.Name = "lstTables";
        this.lstTables.Size = new System.Drawing.Size(188, 349);
        this.lstTables.TabIndex = 0;
        this.lstTables.SelectedIndexChanged += new System.EventHandler(this.lstTables_SelectedIndexChanged);
        
        // 
        // grpPowerBI
        // 
        this.grpPowerBI.Controls.Add(this.btnLoadCSV);
        this.grpPowerBI.Controls.Add(this.btnLoadExample);
        this.grpPowerBI.Controls.Add(this.txtPowerBITable);
        this.grpPowerBI.Controls.Add(this.lblPowerBITable);
        this.grpPowerBI.Location = new System.Drawing.Point(218, 108);
        this.grpPowerBI.Name = "grpPowerBI";
        this.grpPowerBI.Size = new System.Drawing.Size(554, 180);
        this.grpPowerBI.TabIndex = 2;
        this.grpPowerBI.TabStop = false;
        this.grpPowerBI.Text = "Power BI Table / CSV Data";
        
        // 
        // lblPowerBITable
        // 
        this.lblPowerBITable.AutoSize = true;
        this.lblPowerBITable.Location = new System.Drawing.Point(6, 19);
        this.lblPowerBITable.Name = "lblPowerBITable";
        this.lblPowerBITable.Size = new System.Drawing.Size(380, 15);
        this.lblPowerBITable.TabIndex = 0;
        this.lblPowerBITable.Text = "Enter Power BI JSON or load CSV file to auto-detect structure:";
        
        // 
        // txtPowerBITable
        // 
        this.txtPowerBITable.Location = new System.Drawing.Point(6, 37);
        this.txtPowerBITable.Multiline = true;
        this.txtPowerBITable.Name = "txtPowerBITable";
        this.txtPowerBITable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtPowerBITable.Size = new System.Drawing.Size(542, 105);
        this.txtPowerBITable.TabIndex = 1;
        
        // 
        // btnLoadExample
        // 
        this.btnLoadExample.Location = new System.Drawing.Point(6, 148);
        this.btnLoadExample.Name = "btnLoadExample";
        this.btnLoadExample.Size = new System.Drawing.Size(120, 25);
        this.btnLoadExample.TabIndex = 2;
        this.btnLoadExample.Text = "Load Example";
        this.btnLoadExample.UseVisualStyleBackColor = true;
        this.btnLoadExample.Click += new System.EventHandler(this.btnLoadExample_Click);
        
        // 
        // btnLoadCSV
        // 
        this.btnLoadCSV.Location = new System.Drawing.Point(132, 148);
        this.btnLoadCSV.Name = "btnLoadCSV";
        this.btnLoadCSV.Size = new System.Drawing.Size(120, 25);
        this.btnLoadCSV.TabIndex = 3;
        this.btnLoadCSV.Text = "Load from CSV";
        this.btnLoadCSV.UseVisualStyleBackColor = true;
        this.btnLoadCSV.Click += new System.EventHandler(this.btnLoadCSV_Click);
        
        // 
        // grpSQL
        // 
        this.grpSQL.Controls.Add(this.btnCopy);
        this.grpSQL.Controls.Add(this.btnGenerateSQL);
        this.grpSQL.Controls.Add(this.txtGeneratedSQL);
        this.grpSQL.Location = new System.Drawing.Point(218, 294);
        this.grpSQL.Name = "grpSQL";
        this.grpSQL.Size = new System.Drawing.Size(554, 194);
        this.grpSQL.TabIndex = 3;
        this.grpSQL.TabStop = false;
        this.grpSQL.Text = "Generated SQL";
        
        // 
        // txtGeneratedSQL
        // 
        this.txtGeneratedSQL.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtGeneratedSQL.Location = new System.Drawing.Point(6, 22);
        this.txtGeneratedSQL.Multiline = true;
        this.txtGeneratedSQL.Name = "txtGeneratedSQL";
        this.txtGeneratedSQL.ReadOnly = true;
        this.txtGeneratedSQL.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        this.txtGeneratedSQL.Size = new System.Drawing.Size(542, 130);
        this.txtGeneratedSQL.TabIndex = 0;
        this.txtGeneratedSQL.WordWrap = false;
        
        // 
        // btnGenerateSQL
        // 
        this.btnGenerateSQL.Enabled = false;
        this.btnGenerateSQL.Location = new System.Drawing.Point(6, 158);
        this.btnGenerateSQL.Name = "btnGenerateSQL";
        this.btnGenerateSQL.Size = new System.Drawing.Size(120, 30);
        this.btnGenerateSQL.TabIndex = 1;
        this.btnGenerateSQL.Text = "Generate SQL";
        this.btnGenerateSQL.UseVisualStyleBackColor = true;
        this.btnGenerateSQL.Click += new System.EventHandler(this.btnGenerateSQL_Click);
        
        // 
        // btnCopy
        // 
        this.btnCopy.Location = new System.Drawing.Point(132, 158);
        this.btnCopy.Name = "btnCopy";
        this.btnCopy.Size = new System.Drawing.Size(120, 30);
        this.btnCopy.TabIndex = 2;
        this.btnCopy.Text = "Copy to Clipboard";
        this.btnCopy.UseVisualStyleBackColor = true;
        this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
        
        // 
        // grpStatus
        // 
        this.grpStatus.Controls.Add(this.btnClear);
        this.grpStatus.Controls.Add(this.txtStatus);
        this.grpStatus.Location = new System.Drawing.Point(12, 494);
        this.grpStatus.Name = "grpStatus";
        this.grpStatus.Size = new System.Drawing.Size(760, 145);
        this.grpStatus.TabIndex = 4;
        this.grpStatus.TabStop = false;
        this.grpStatus.Text = "Status / Log";
        
        // 
        // txtStatus
        // 
        this.txtStatus.Location = new System.Drawing.Point(6, 22);
        this.txtStatus.Multiline = true;
        this.txtStatus.Name = "txtStatus";
        this.txtStatus.ReadOnly = true;
        this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtStatus.Size = new System.Drawing.Size(640, 110);
        this.txtStatus.TabIndex = 0;
        
        // 
        // btnClear
        // 
        this.btnClear.Location = new System.Drawing.Point(652, 22);
        this.btnClear.Name = "btnClear";
        this.btnClear.Size = new System.Drawing.Size(100, 30);
        this.btnClear.TabIndex = 1;
        this.btnClear.Text = "Clear";
        this.btnClear.UseVisualStyleBackColor = true;
        this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
        
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(784, 651);
        this.Controls.Add(this.grpStatus);
        this.Controls.Add(this.grpSQL);
        this.Controls.Add(this.grpPowerBI);
        this.Controls.Add(this.grpTables);
        this.Controls.Add(this.grpConnection);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "SQL Generator for Azure Fabric & Power BI";
        this.grpConnection.ResumeLayout(false);
        this.grpConnection.PerformLayout();
        this.grpPowerBI.ResumeLayout(false);
        this.grpPowerBI.PerformLayout();
        this.grpTables.ResumeLayout(false);
        this.grpSQL.ResumeLayout(false);
        this.grpSQL.PerformLayout();
        this.grpStatus.ResumeLayout(false);
        this.grpStatus.PerformLayout();
        this.ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.GroupBox grpConnection;
    private System.Windows.Forms.Label lblConnectionString;
    private System.Windows.Forms.TextBox txtConnectionString;
    private System.Windows.Forms.Button btnConnect;
    
    private System.Windows.Forms.GroupBox grpTables;
    private System.Windows.Forms.ListBox lstTables;
    
    private System.Windows.Forms.GroupBox grpPowerBI;
    private System.Windows.Forms.Label lblPowerBITable;
    private System.Windows.Forms.TextBox txtPowerBITable;
    private System.Windows.Forms.Button btnLoadExample;
    private System.Windows.Forms.Button btnLoadCSV;
    
    private System.Windows.Forms.GroupBox grpSQL;
    private System.Windows.Forms.TextBox txtGeneratedSQL;
    private System.Windows.Forms.Button btnGenerateSQL;
    private System.Windows.Forms.Button btnCopy;
    
    private System.Windows.Forms.GroupBox grpStatus;
    private System.Windows.Forms.TextBox txtStatus;
    private System.Windows.Forms.Button btnClear;
}
