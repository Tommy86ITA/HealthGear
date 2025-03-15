// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace HealthGearConfig
{
    partial class MainForm
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxServiceStatus = new System.Windows.Forms.GroupBox();
            this.panelServiceStatus = new System.Windows.Forms.Panel();
            this.labelServiceStatus = new System.Windows.Forms.Label();
            this.buttonStopService = new System.Windows.Forms.Button();
            this.buttonStartService = new System.Windows.Forms.Button();
            this.tabServerSettings = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.buttonBrowseUploadPath = new System.Windows.Forms.Button();
            this.textBoxFileUploadPath = new System.Windows.Forms.TextBox();
            this.labelUploadFolderPath = new System.Windows.Forms.Label();
            this.buttonBrowseDBFolderPath = new System.Windows.Forms.Button();
            this.textBoxDatabasePath = new System.Windows.Forms.TextBox();
            this.numericUpDownServerPort = new System.Windows.Forms.NumericUpDown();
            this.labelDBFilepath = new System.Windows.Forms.Label();
            this.labelServerPort = new System.Windows.Forms.Label();
            this.tabPageSMTP = new System.Windows.Forms.TabPage();
            this.buttonSMTPTest = new System.Windows.Forms.Button();
            this.checkBoxSMTPAuth = new System.Windows.Forms.CheckBox();
            this.checkBoxSSL = new System.Windows.Forms.CheckBox();
            this.textBoxSMTPPassword = new System.Windows.Forms.TextBox();
            this.labelSMTPPassword = new System.Windows.Forms.Label();
            this.textBoxSMTPUsername = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxSMTPHost = new System.Windows.Forms.TextBox();
            this.labelSMTPHost = new System.Windows.Forms.Label();
            this.numericUpDownSMTPPort = new System.Windows.Forms.NumericUpDown();
            this.labelHostPort = new System.Windows.Forms.Label();
            this.tabPageLogging = new System.Windows.Forms.TabPage();
            this.buttonBrowseLoggingPath = new System.Windows.Forms.Button();
            this.textBoxLogPath = new System.Windows.Forms.TextBox();
            this.labelLoggingPath = new System.Windows.Forms.Label();
            this.comboBoxLogLevel = new System.Windows.Forms.ComboBox();
            this.labelLoggingLevel = new System.Windows.Forms.Label();
            this.tabPageManageSettings = new System.Windows.Forms.TabPage();
            this.groupBoxReset = new System.Windows.Forms.GroupBox();
            this.labelResetSettings = new System.Windows.Forms.Label();
            this.buttonResetSettings = new System.Windows.Forms.Button();
            this.groupBoxImportSettings = new System.Windows.Forms.GroupBox();
            this.buttonDiscardImportedSettings = new System.Windows.Forms.Button();
            this.buttonApplyImportedSettings = new System.Windows.Forms.Button();
            this.labelSettingsPreview = new System.Windows.Forms.Label();
            this.textBoxPreview = new System.Windows.Forms.TextBox();
            this.labelImportSettings = new System.Windows.Forms.Label();
            this.buttonImportSettings = new System.Windows.Forms.Button();
            this.groupBoxExportSettings = new System.Windows.Forms.GroupBox();
            this.labelExportSettings = new System.Windows.Forms.Label();
            this.buttonExportSettings = new System.Windows.Forms.Button();
            this.tabPageAbout = new System.Windows.Forms.TabPage();
            this.buttonExit = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.richTextBoxLicense = new System.Windows.Forms.RichTextBox();
            this.groupBoxAbout = new System.Windows.Forms.GroupBox();
            this.labelHealthGearVersion = new System.Windows.Forms.Label();
            this.labelSoftwareVersion = new System.Windows.Forms.Label();
            this.labelSoftwareName = new System.Windows.Forms.Label();
            this.groupBoxServiceStatus.SuspendLayout();
            this.tabServerSettings.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownServerPort)).BeginInit();
            this.tabPageSMTP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSMTPPort)).BeginInit();
            this.tabPageLogging.SuspendLayout();
            this.tabPageManageSettings.SuspendLayout();
            this.groupBoxReset.SuspendLayout();
            this.groupBoxImportSettings.SuspendLayout();
            this.groupBoxExportSettings.SuspendLayout();
            this.tabPageAbout.SuspendLayout();
            this.groupBoxAbout.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxServiceStatus
            // 
            this.groupBoxServiceStatus.Controls.Add(this.panelServiceStatus);
            this.groupBoxServiceStatus.Controls.Add(this.labelServiceStatus);
            this.groupBoxServiceStatus.Controls.Add(this.buttonStopService);
            this.groupBoxServiceStatus.Controls.Add(this.buttonStartService);
            this.groupBoxServiceStatus.Location = new System.Drawing.Point(14, 10);
            this.groupBoxServiceStatus.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxServiceStatus.Name = "groupBoxServiceStatus";
            this.groupBoxServiceStatus.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxServiceStatus.Size = new System.Drawing.Size(635, 96);
            this.groupBoxServiceStatus.TabIndex = 0;
            this.groupBoxServiceStatus.TabStop = false;
            this.groupBoxServiceStatus.Text = "Stato servizio";
            // 
            // panelServiceStatus
            // 
            this.panelServiceStatus.BackColor = System.Drawing.Color.Red;
            this.panelServiceStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelServiceStatus.Location = new System.Drawing.Point(345, 36);
            this.panelServiceStatus.Margin = new System.Windows.Forms.Padding(2);
            this.panelServiceStatus.Name = "panelServiceStatus";
            this.panelServiceStatus.Size = new System.Drawing.Size(40, 40);
            this.panelServiceStatus.TabIndex = 3;
            // 
            // labelServiceStatus
            // 
            this.labelServiceStatus.AutoSize = true;
            this.labelServiceStatus.Location = new System.Drawing.Point(403, 48);
            this.labelServiceStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelServiceStatus.Name = "labelServiceStatus";
            this.labelServiceStatus.Size = new System.Drawing.Size(104, 20);
            this.labelServiceStatus.TabIndex = 2;
            this.labelServiceStatus.Text = "Stato servizio";
            // 
            // buttonStopService
            // 
            this.buttonStopService.Location = new System.Drawing.Point(177, 35);
            this.buttonStopService.Margin = new System.Windows.Forms.Padding(2);
            this.buttonStopService.Name = "buttonStopService";
            this.buttonStopService.Size = new System.Drawing.Size(155, 45);
            this.buttonStopService.TabIndex = 1;
            this.buttonStopService.Text = "Arresta servizio";
            this.buttonStopService.UseVisualStyleBackColor = true;
            this.buttonStopService.Click += new System.EventHandler(this.buttonStopService_Click);
            // 
            // buttonStartService
            // 
            this.buttonStartService.Location = new System.Drawing.Point(16, 35);
            this.buttonStartService.Margin = new System.Windows.Forms.Padding(2);
            this.buttonStartService.Name = "buttonStartService";
            this.buttonStartService.Size = new System.Drawing.Size(155, 45);
            this.buttonStartService.TabIndex = 0;
            this.buttonStartService.Text = "Avvia servizio";
            this.buttonStartService.UseVisualStyleBackColor = true;
            this.buttonStartService.Click += new System.EventHandler(this.buttonStartService_Click);
            // 
            // tabServerSettings
            // 
            this.tabServerSettings.Controls.Add(this.tabPage1);
            this.tabServerSettings.Controls.Add(this.tabPageSMTP);
            this.tabServerSettings.Controls.Add(this.tabPageLogging);
            this.tabServerSettings.Controls.Add(this.tabPageManageSettings);
            this.tabServerSettings.Controls.Add(this.tabPageAbout);
            this.tabServerSettings.Location = new System.Drawing.Point(11, 111);
            this.tabServerSettings.Margin = new System.Windows.Forms.Padding(2);
            this.tabServerSettings.Multiline = true;
            this.tabServerSettings.Name = "tabServerSettings";
            this.tabServerSettings.SelectedIndex = 0;
            this.tabServerSettings.Size = new System.Drawing.Size(634, 766);
            this.tabServerSettings.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.buttonBrowseUploadPath);
            this.tabPage1.Controls.Add(this.textBoxFileUploadPath);
            this.tabPage1.Controls.Add(this.labelUploadFolderPath);
            this.tabPage1.Controls.Add(this.buttonBrowseDBFolderPath);
            this.tabPage1.Controls.Add(this.textBoxDatabasePath);
            this.tabPage1.Controls.Add(this.numericUpDownServerPort);
            this.tabPage1.Controls.Add(this.labelDBFilepath);
            this.tabPage1.Controls.Add(this.labelServerPort);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(626, 733);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Server";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // buttonBrowseUploadPath
            // 
            this.buttonBrowseUploadPath.Location = new System.Drawing.Point(466, 271);
            this.buttonBrowseUploadPath.Margin = new System.Windows.Forms.Padding(2);
            this.buttonBrowseUploadPath.Name = "buttonBrowseUploadPath";
            this.buttonBrowseUploadPath.Size = new System.Drawing.Size(98, 30);
            this.buttonBrowseUploadPath.TabIndex = 8;
            this.buttonBrowseUploadPath.Text = "Sfoglia...";
            this.buttonBrowseUploadPath.UseVisualStyleBackColor = true;
            this.buttonBrowseUploadPath.Click += new System.EventHandler(this.buttonBrowseUploadPath_Click);
            // 
            // textBoxFileUploadPath
            // 
            this.textBoxFileUploadPath.Location = new System.Drawing.Point(70, 273);
            this.textBoxFileUploadPath.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxFileUploadPath.Name = "textBoxFileUploadPath";
            this.textBoxFileUploadPath.Size = new System.Drawing.Size(366, 26);
            this.textBoxFileUploadPath.TabIndex = 7;
            // 
            // labelUploadFolderPath
            // 
            this.labelUploadFolderPath.AutoSize = true;
            this.labelUploadFolderPath.Location = new System.Drawing.Point(65, 244);
            this.labelUploadFolderPath.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelUploadFolderPath.Name = "labelUploadFolderPath";
            this.labelUploadFolderPath.Size = new System.Drawing.Size(175, 20);
            this.labelUploadFolderPath.TabIndex = 6;
            this.labelUploadFolderPath.Text = "Percorso File Database";
            // 
            // buttonBrowseDBFolderPath
            // 
            this.buttonBrowseDBFolderPath.Location = new System.Drawing.Point(466, 147);
            this.buttonBrowseDBFolderPath.Margin = new System.Windows.Forms.Padding(2);
            this.buttonBrowseDBFolderPath.Name = "buttonBrowseDBFolderPath";
            this.buttonBrowseDBFolderPath.Size = new System.Drawing.Size(98, 30);
            this.buttonBrowseDBFolderPath.TabIndex = 5;
            this.buttonBrowseDBFolderPath.Text = "Sfoglia...";
            this.buttonBrowseDBFolderPath.UseVisualStyleBackColor = true;
            this.buttonBrowseDBFolderPath.Click += new System.EventHandler(this.buttonBrowseDBFolderPath_Click);
            // 
            // textBoxDatabasePath
            // 
            this.textBoxDatabasePath.Location = new System.Drawing.Point(70, 149);
            this.textBoxDatabasePath.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxDatabasePath.Name = "textBoxDatabasePath";
            this.textBoxDatabasePath.Size = new System.Drawing.Size(366, 26);
            this.textBoxDatabasePath.TabIndex = 4;
            // 
            // numericUpDownServerPort
            // 
            this.numericUpDownServerPort.Location = new System.Drawing.Point(466, 41);
            this.numericUpDownServerPort.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDownServerPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDownServerPort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numericUpDownServerPort.Name = "numericUpDownServerPort";
            this.numericUpDownServerPort.Size = new System.Drawing.Size(98, 26);
            this.numericUpDownServerPort.TabIndex = 3;
            this.numericUpDownServerPort.Value = new decimal(new int[] {
            5051,
            0,
            0,
            0});
            // 
            // labelDBFilepath
            // 
            this.labelDBFilepath.AutoSize = true;
            this.labelDBFilepath.Location = new System.Drawing.Point(65, 126);
            this.labelDBFilepath.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDBFilepath.Name = "labelDBFilepath";
            this.labelDBFilepath.Size = new System.Drawing.Size(175, 20);
            this.labelDBFilepath.TabIndex = 1;
            this.labelDBFilepath.Text = "Percorso File Database";
            // 
            // labelServerPort
            // 
            this.labelServerPort.AutoSize = true;
            this.labelServerPort.Location = new System.Drawing.Point(65, 41);
            this.labelServerPort.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelServerPort.Name = "labelServerPort";
            this.labelServerPort.Size = new System.Drawing.Size(97, 20);
            this.labelServerPort.TabIndex = 0;
            this.labelServerPort.Text = "Porta Server";
            // 
            // tabPageSMTP
            // 
            this.tabPageSMTP.Controls.Add(this.buttonSMTPTest);
            this.tabPageSMTP.Controls.Add(this.checkBoxSMTPAuth);
            this.tabPageSMTP.Controls.Add(this.checkBoxSSL);
            this.tabPageSMTP.Controls.Add(this.textBoxSMTPPassword);
            this.tabPageSMTP.Controls.Add(this.labelSMTPPassword);
            this.tabPageSMTP.Controls.Add(this.textBoxSMTPUsername);
            this.tabPageSMTP.Controls.Add(this.label3);
            this.tabPageSMTP.Controls.Add(this.textBoxSMTPHost);
            this.tabPageSMTP.Controls.Add(this.labelSMTPHost);
            this.tabPageSMTP.Controls.Add(this.numericUpDownSMTPPort);
            this.tabPageSMTP.Controls.Add(this.labelHostPort);
            this.tabPageSMTP.Location = new System.Drawing.Point(4, 29);
            this.tabPageSMTP.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageSMTP.Name = "tabPageSMTP";
            this.tabPageSMTP.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageSMTP.Size = new System.Drawing.Size(626, 733);
            this.tabPageSMTP.TabIndex = 1;
            this.tabPageSMTP.Text = "SMTP";
            this.tabPageSMTP.UseVisualStyleBackColor = true;
            // 
            // buttonSMTPTest
            // 
            this.buttonSMTPTest.Location = new System.Drawing.Point(229, 432);
            this.buttonSMTPTest.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSMTPTest.Name = "buttonSMTPTest";
            this.buttonSMTPTest.Size = new System.Drawing.Size(155, 45);
            this.buttonSMTPTest.TabIndex = 14;
            this.buttonSMTPTest.Text = "Verifica";
            this.buttonSMTPTest.UseVisualStyleBackColor = true;
            this.buttonSMTPTest.Click += new System.EventHandler(this.buttonSMTPTest_Click);
            // 
            // checkBoxSMTPAuth
            // 
            this.checkBoxSMTPAuth.AutoSize = true;
            this.checkBoxSMTPAuth.Checked = true;
            this.checkBoxSMTPAuth.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSMTPAuth.Location = new System.Drawing.Point(357, 364);
            this.checkBoxSMTPAuth.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxSMTPAuth.Name = "checkBoxSMTPAuth";
            this.checkBoxSMTPAuth.Size = new System.Drawing.Size(207, 24);
            this.checkBoxSMTPAuth.TabIndex = 13;
            this.checkBoxSMTPAuth.Text = "Richiede Autenticazione";
            this.checkBoxSMTPAuth.UseVisualStyleBackColor = true;
            // 
            // checkBoxSSL
            // 
            this.checkBoxSSL.AutoSize = true;
            this.checkBoxSSL.Checked = true;
            this.checkBoxSSL.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSSL.Location = new System.Drawing.Point(69, 364);
            this.checkBoxSSL.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxSSL.Name = "checkBoxSSL";
            this.checkBoxSSL.Size = new System.Drawing.Size(165, 24);
            this.checkBoxSSL.TabIndex = 12;
            this.checkBoxSSL.Text = "Richiede TLS/SSL";
            this.checkBoxSSL.UseVisualStyleBackColor = true;
            // 
            // textBoxSMTPPassword
            // 
            this.textBoxSMTPPassword.Location = new System.Drawing.Point(69, 284);
            this.textBoxSMTPPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxSMTPPassword.Name = "textBoxSMTPPassword";
            this.textBoxSMTPPassword.Size = new System.Drawing.Size(491, 26);
            this.textBoxSMTPPassword.TabIndex = 11;
            this.textBoxSMTPPassword.UseSystemPasswordChar = true;
            // 
            // labelSMTPPassword
            // 
            this.labelSMTPPassword.AutoSize = true;
            this.labelSMTPPassword.Location = new System.Drawing.Point(65, 261);
            this.labelSMTPPassword.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSMTPPassword.Name = "labelSMTPPassword";
            this.labelSMTPPassword.Size = new System.Drawing.Size(125, 20);
            this.labelSMTPPassword.TabIndex = 10;
            this.labelSMTPPassword.Text = "SMTP Password";
            // 
            // textBoxSMTPUsername
            // 
            this.textBoxSMTPUsername.Location = new System.Drawing.Point(69, 201);
            this.textBoxSMTPUsername.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxSMTPUsername.Name = "textBoxSMTPUsername";
            this.textBoxSMTPUsername.Size = new System.Drawing.Size(491, 26);
            this.textBoxSMTPUsername.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(65, 178);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 20);
            this.label3.TabIndex = 8;
            this.label3.Text = "SMTP Username";
            // 
            // textBoxSMTPHost
            // 
            this.textBoxSMTPHost.Location = new System.Drawing.Point(69, 71);
            this.textBoxSMTPHost.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxSMTPHost.Name = "textBoxSMTPHost";
            this.textBoxSMTPHost.Size = new System.Drawing.Size(491, 26);
            this.textBoxSMTPHost.TabIndex = 7;
            // 
            // labelSMTPHost
            // 
            this.labelSMTPHost.AutoSize = true;
            this.labelSMTPHost.Location = new System.Drawing.Point(65, 48);
            this.labelSMTPHost.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSMTPHost.Name = "labelSMTPHost";
            this.labelSMTPHost.Size = new System.Drawing.Size(90, 20);
            this.labelSMTPHost.TabIndex = 6;
            this.labelSMTPHost.Text = "SMTP Host";
            // 
            // numericUpDownSMTPPort
            // 
            this.numericUpDownSMTPPort.Location = new System.Drawing.Point(461, 125);
            this.numericUpDownSMTPPort.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDownSMTPPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDownSMTPPort.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDownSMTPPort.Name = "numericUpDownSMTPPort";
            this.numericUpDownSMTPPort.Size = new System.Drawing.Size(98, 26);
            this.numericUpDownSMTPPort.TabIndex = 5;
            this.numericUpDownSMTPPort.Value = new decimal(new int[] {
            587,
            0,
            0,
            0});
            // 
            // labelHostPort
            // 
            this.labelHostPort.AutoSize = true;
            this.labelHostPort.Location = new System.Drawing.Point(65, 127);
            this.labelHostPort.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelHostPort.Name = "labelHostPort";
            this.labelHostPort.Size = new System.Drawing.Size(135, 20);
            this.labelHostPort.TabIndex = 4;
            this.labelHostPort.Text = "Porta Server Host";
            // 
            // tabPageLogging
            // 
            this.tabPageLogging.Controls.Add(this.buttonBrowseLoggingPath);
            this.tabPageLogging.Controls.Add(this.textBoxLogPath);
            this.tabPageLogging.Controls.Add(this.labelLoggingPath);
            this.tabPageLogging.Controls.Add(this.comboBoxLogLevel);
            this.tabPageLogging.Controls.Add(this.labelLoggingLevel);
            this.tabPageLogging.Location = new System.Drawing.Point(4, 29);
            this.tabPageLogging.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageLogging.Name = "tabPageLogging";
            this.tabPageLogging.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageLogging.Size = new System.Drawing.Size(626, 733);
            this.tabPageLogging.TabIndex = 2;
            this.tabPageLogging.Text = "Logging";
            this.tabPageLogging.UseVisualStyleBackColor = true;
            // 
            // buttonBrowseLoggingPath
            // 
            this.buttonBrowseLoggingPath.Location = new System.Drawing.Point(469, 156);
            this.buttonBrowseLoggingPath.Margin = new System.Windows.Forms.Padding(2);
            this.buttonBrowseLoggingPath.Name = "buttonBrowseLoggingPath";
            this.buttonBrowseLoggingPath.Size = new System.Drawing.Size(98, 30);
            this.buttonBrowseLoggingPath.TabIndex = 8;
            this.buttonBrowseLoggingPath.Text = "Sfoglia...";
            this.buttonBrowseLoggingPath.UseVisualStyleBackColor = true;
            this.buttonBrowseLoggingPath.Click += new System.EventHandler(this.buttonBrowseLoggingPath_Click);
            // 
            // textBoxLogPath
            // 
            this.textBoxLogPath.Location = new System.Drawing.Point(73, 158);
            this.textBoxLogPath.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxLogPath.Name = "textBoxLogPath";
            this.textBoxLogPath.Size = new System.Drawing.Size(366, 26);
            this.textBoxLogPath.TabIndex = 7;
            // 
            // labelLoggingPath
            // 
            this.labelLoggingPath.AutoSize = true;
            this.labelLoggingPath.Location = new System.Drawing.Point(69, 135);
            this.labelLoggingPath.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelLoggingPath.Name = "labelLoggingPath";
            this.labelLoggingPath.Size = new System.Drawing.Size(148, 20);
            this.labelLoggingPath.TabIndex = 6;
            this.labelLoggingPath.Text = "Percorso File di Log";
            // 
            // comboBoxLogLevel
            // 
            this.comboBoxLogLevel.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLogLevel.FormattingEnabled = true;
            this.comboBoxLogLevel.Items.AddRange(new object[] {
            "Verbose",
            "Warning",
            "Error"});
            this.comboBoxLogLevel.Location = new System.Drawing.Point(358, 53);
            this.comboBoxLogLevel.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxLogLevel.Name = "comboBoxLogLevel";
            this.comboBoxLogLevel.Size = new System.Drawing.Size(210, 28);
            this.comboBoxLogLevel.TabIndex = 1;
            // 
            // labelLoggingLevel
            // 
            this.labelLoggingLevel.AutoSize = true;
            this.labelLoggingLevel.Location = new System.Drawing.Point(69, 53);
            this.labelLoggingLevel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelLoggingLevel.Name = "labelLoggingLevel";
            this.labelLoggingLevel.Size = new System.Drawing.Size(113, 20);
            this.labelLoggingLevel.TabIndex = 0;
            this.labelLoggingLevel.Text = "Livello Logging";
            // 
            // tabPageManageSettings
            // 
            this.tabPageManageSettings.Controls.Add(this.groupBoxReset);
            this.tabPageManageSettings.Controls.Add(this.groupBoxImportSettings);
            this.tabPageManageSettings.Controls.Add(this.groupBoxExportSettings);
            this.tabPageManageSettings.Location = new System.Drawing.Point(4, 29);
            this.tabPageManageSettings.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageManageSettings.Name = "tabPageManageSettings";
            this.tabPageManageSettings.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageManageSettings.Size = new System.Drawing.Size(626, 733);
            this.tabPageManageSettings.TabIndex = 3;
            this.tabPageManageSettings.Text = "Gestione Impostazioni";
            this.tabPageManageSettings.UseVisualStyleBackColor = true;
            // 
            // groupBoxReset
            // 
            this.groupBoxReset.Controls.Add(this.labelResetSettings);
            this.groupBoxReset.Controls.Add(this.buttonResetSettings);
            this.groupBoxReset.Location = new System.Drawing.Point(16, 621);
            this.groupBoxReset.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxReset.Name = "groupBoxReset";
            this.groupBoxReset.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxReset.Size = new System.Drawing.Size(594, 108);
            this.groupBoxReset.TabIndex = 2;
            this.groupBoxReset.TabStop = false;
            this.groupBoxReset.Text = "Reset";
            // 
            // labelResetSettings
            // 
            this.labelResetSettings.AutoSize = true;
            this.labelResetSettings.Location = new System.Drawing.Point(5, 49);
            this.labelResetSettings.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelResetSettings.Name = "labelResetSettings";
            this.labelResetSettings.Size = new System.Drawing.Size(252, 20);
            this.labelResetSettings.TabIndex = 2;
            this.labelResetSettings.Text = "Ripristina le impostazioni di default";
            // 
            // buttonResetSettings
            // 
            this.buttonResetSettings.BackColor = System.Drawing.Color.Red;
            this.buttonResetSettings.ForeColor = System.Drawing.Color.White;
            this.buttonResetSettings.Location = new System.Drawing.Point(434, 37);
            this.buttonResetSettings.Margin = new System.Windows.Forms.Padding(2);
            this.buttonResetSettings.Name = "buttonResetSettings";
            this.buttonResetSettings.Size = new System.Drawing.Size(155, 45);
            this.buttonResetSettings.TabIndex = 1;
            this.buttonResetSettings.Text = "Ripristina";
            this.buttonResetSettings.UseVisualStyleBackColor = false;
            this.buttonResetSettings.Click += new System.EventHandler(this.buttonResetSettings_Click);
            // 
            // groupBoxImportSettings
            // 
            this.groupBoxImportSettings.Controls.Add(this.buttonDiscardImportedSettings);
            this.groupBoxImportSettings.Controls.Add(this.buttonApplyImportedSettings);
            this.groupBoxImportSettings.Controls.Add(this.labelSettingsPreview);
            this.groupBoxImportSettings.Controls.Add(this.textBoxPreview);
            this.groupBoxImportSettings.Controls.Add(this.labelImportSettings);
            this.groupBoxImportSettings.Controls.Add(this.buttonImportSettings);
            this.groupBoxImportSettings.Location = new System.Drawing.Point(16, 138);
            this.groupBoxImportSettings.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxImportSettings.Name = "groupBoxImportSettings";
            this.groupBoxImportSettings.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxImportSettings.Size = new System.Drawing.Size(594, 479);
            this.groupBoxImportSettings.TabIndex = 1;
            this.groupBoxImportSettings.TabStop = false;
            this.groupBoxImportSettings.Text = "Importa";
            // 
            // buttonDiscardImportedSettings
            // 
            this.buttonDiscardImportedSettings.Enabled = false;
            this.buttonDiscardImportedSettings.Location = new System.Drawing.Point(316, 421);
            this.buttonDiscardImportedSettings.Margin = new System.Windows.Forms.Padding(2);
            this.buttonDiscardImportedSettings.Name = "buttonDiscardImportedSettings";
            this.buttonDiscardImportedSettings.Size = new System.Drawing.Size(155, 45);
            this.buttonDiscardImportedSettings.TabIndex = 6;
            this.buttonDiscardImportedSettings.Text = "Scarta";
            this.buttonDiscardImportedSettings.UseVisualStyleBackColor = true;
            this.buttonDiscardImportedSettings.Click += new System.EventHandler(this.buttonDiscardImportedSettings_Click);
            // 
            // buttonApplyImportedSettings
            // 
            this.buttonApplyImportedSettings.Enabled = false;
            this.buttonApplyImportedSettings.Location = new System.Drawing.Point(122, 421);
            this.buttonApplyImportedSettings.Margin = new System.Windows.Forms.Padding(2);
            this.buttonApplyImportedSettings.Name = "buttonApplyImportedSettings";
            this.buttonApplyImportedSettings.Size = new System.Drawing.Size(155, 45);
            this.buttonApplyImportedSettings.TabIndex = 5;
            this.buttonApplyImportedSettings.Text = "Applica";
            this.buttonApplyImportedSettings.UseVisualStyleBackColor = true;
            this.buttonApplyImportedSettings.Click += new System.EventHandler(this.buttonApplyImportedSettings_Click);
            // 
            // labelSettingsPreview
            // 
            this.labelSettingsPreview.AutoSize = true;
            this.labelSettingsPreview.Location = new System.Drawing.Point(5, 102);
            this.labelSettingsPreview.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSettingsPreview.Name = "labelSettingsPreview";
            this.labelSettingsPreview.Size = new System.Drawing.Size(174, 20);
            this.labelSettingsPreview.TabIndex = 4;
            this.labelSettingsPreview.Text = "Anteprima impostazioni";
            // 
            // textBoxPreview
            // 
            this.textBoxPreview.Location = new System.Drawing.Point(9, 138);
            this.textBoxPreview.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxPreview.Multiline = true;
            this.textBoxPreview.Name = "textBoxPreview";
            this.textBoxPreview.ReadOnly = true;
            this.textBoxPreview.Size = new System.Drawing.Size(581, 279);
            this.textBoxPreview.TabIndex = 3;
            // 
            // labelImportSettings
            // 
            this.labelImportSettings.AutoSize = true;
            this.labelImportSettings.Location = new System.Drawing.Point(5, 49);
            this.labelImportSettings.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelImportSettings.Name = "labelImportSettings";
            this.labelImportSettings.Size = new System.Drawing.Size(286, 20);
            this.labelImportSettings.TabIndex = 2;
            this.labelImportSettings.Text = "Importa le impostazioni da un file JSON";
            // 
            // buttonImportSettings
            // 
            this.buttonImportSettings.Location = new System.Drawing.Point(434, 37);
            this.buttonImportSettings.Margin = new System.Windows.Forms.Padding(2);
            this.buttonImportSettings.Name = "buttonImportSettings";
            this.buttonImportSettings.Size = new System.Drawing.Size(155, 45);
            this.buttonImportSettings.TabIndex = 1;
            this.buttonImportSettings.Text = "Importa";
            this.buttonImportSettings.UseVisualStyleBackColor = true;
            this.buttonImportSettings.Click += new System.EventHandler(this.buttonImportSettings_Click);
            // 
            // groupBoxExportSettings
            // 
            this.groupBoxExportSettings.Controls.Add(this.labelExportSettings);
            this.groupBoxExportSettings.Controls.Add(this.buttonExportSettings);
            this.groupBoxExportSettings.Location = new System.Drawing.Point(16, 26);
            this.groupBoxExportSettings.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxExportSettings.Name = "groupBoxExportSettings";
            this.groupBoxExportSettings.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxExportSettings.Size = new System.Drawing.Size(594, 108);
            this.groupBoxExportSettings.TabIndex = 0;
            this.groupBoxExportSettings.TabStop = false;
            this.groupBoxExportSettings.Text = "Esporta";
            // 
            // labelExportSettings
            // 
            this.labelExportSettings.AutoSize = true;
            this.labelExportSettings.Location = new System.Drawing.Point(5, 49);
            this.labelExportSettings.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelExportSettings.Name = "labelExportSettings";
            this.labelExportSettings.Size = new System.Drawing.Size(328, 20);
            this.labelExportSettings.TabIndex = 2;
            this.labelExportSettings.Text = "Esporte le impostazioni attuali in un file JSON";
            // 
            // buttonExportSettings
            // 
            this.buttonExportSettings.Location = new System.Drawing.Point(434, 37);
            this.buttonExportSettings.Margin = new System.Windows.Forms.Padding(2);
            this.buttonExportSettings.Name = "buttonExportSettings";
            this.buttonExportSettings.Size = new System.Drawing.Size(155, 45);
            this.buttonExportSettings.TabIndex = 1;
            this.buttonExportSettings.Text = "Esporta";
            this.buttonExportSettings.UseVisualStyleBackColor = true;
            this.buttonExportSettings.Click += new System.EventHandler(this.buttonExportSettings_Click);
            // 
            // tabPageAbout
            // 
            this.tabPageAbout.Controls.Add(this.groupBoxAbout);
            this.tabPageAbout.Controls.Add(this.richTextBoxLicense);
            this.tabPageAbout.Location = new System.Drawing.Point(4, 29);
            this.tabPageAbout.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageAbout.Name = "tabPageAbout";
            this.tabPageAbout.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageAbout.Size = new System.Drawing.Size(626, 733);
            this.tabPageAbout.TabIndex = 4;
            this.tabPageAbout.Text = "About";
            this.tabPageAbout.UseVisualStyleBackColor = true;
            // 
            // buttonExit
            // 
            this.buttonExit.Location = new System.Drawing.Point(347, 881);
            this.buttonExit.Margin = new System.Windows.Forms.Padding(2);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(155, 45);
            this.buttonExit.TabIndex = 3;
            this.buttonExit.Text = "Esci";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(153, 881);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(155, 45);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = "Salva";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // richTextBoxLicense
            // 
            this.richTextBoxLicense.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.richTextBoxLicense.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxLicense.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.richTextBoxLicense.Location = new System.Drawing.Point(15, 167);
            this.richTextBoxLicense.Name = "richTextBoxLicense";
            this.richTextBoxLicense.ReadOnly = true;
            this.richTextBoxLicense.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBoxLicense.Size = new System.Drawing.Size(593, 539);
            this.richTextBoxLicense.TabIndex = 2;
            this.richTextBoxLicense.Text = "";
            // 
            // groupBoxAbout
            // 
            this.groupBoxAbout.Controls.Add(this.labelHealthGearVersion);
            this.groupBoxAbout.Controls.Add(this.labelSoftwareVersion);
            this.groupBoxAbout.Controls.Add(this.labelSoftwareName);
            this.groupBoxAbout.Location = new System.Drawing.Point(15, 0);
            this.groupBoxAbout.Name = "groupBoxAbout";
            this.groupBoxAbout.Size = new System.Drawing.Size(593, 161);
            this.groupBoxAbout.TabIndex = 3;
            this.groupBoxAbout.TabStop = false;
            // 
            // labelHealthGearVersion
            // 
            this.labelHealthGearVersion.AutoSize = true;
            this.labelHealthGearVersion.Location = new System.Drawing.Point(9, 108);
            this.labelHealthGearVersion.Name = "labelHealthGearVersion";
            this.labelHealthGearVersion.Size = new System.Drawing.Size(146, 20);
            this.labelHealthGearVersion.TabIndex = 6;
            this.labelHealthGearVersion.Text = "HealthGearVersion";
            this.labelHealthGearVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelSoftwareVersion
            // 
            this.labelSoftwareVersion.AutoSize = true;
            this.labelSoftwareVersion.Location = new System.Drawing.Point(9, 78);
            this.labelSoftwareVersion.Name = "labelSoftwareVersion";
            this.labelSoftwareVersion.Size = new System.Drawing.Size(127, 20);
            this.labelSoftwareVersion.TabIndex = 5;
            this.labelSoftwareVersion.Text = "SoftwareVersion";
            this.labelSoftwareVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelSoftwareName
            // 
            this.labelSoftwareName.AutoSize = true;
            this.labelSoftwareName.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSoftwareName.ForeColor = System.Drawing.Color.DodgerBlue;
            this.labelSoftwareName.Location = new System.Drawing.Point(6, 22);
            this.labelSoftwareName.Name = "labelSoftwareName";
            this.labelSoftwareName.Size = new System.Drawing.Size(332, 40);
            this.labelSoftwareName.TabIndex = 4;
            this.labelSoftwareName.Text = "HealthGear Config";
            this.labelSoftwareName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(655, 937);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.tabServerSettings);
            this.Controls.Add(this.groupBoxServiceStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.groupBoxServiceStatus.ResumeLayout(false);
            this.groupBoxServiceStatus.PerformLayout();
            this.tabServerSettings.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownServerPort)).EndInit();
            this.tabPageSMTP.ResumeLayout(false);
            this.tabPageSMTP.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSMTPPort)).EndInit();
            this.tabPageLogging.ResumeLayout(false);
            this.tabPageLogging.PerformLayout();
            this.tabPageManageSettings.ResumeLayout(false);
            this.groupBoxReset.ResumeLayout(false);
            this.groupBoxReset.PerformLayout();
            this.groupBoxImportSettings.ResumeLayout(false);
            this.groupBoxImportSettings.PerformLayout();
            this.groupBoxExportSettings.ResumeLayout(false);
            this.groupBoxExportSettings.PerformLayout();
            this.tabPageAbout.ResumeLayout(false);
            this.groupBoxAbout.ResumeLayout(false);
            this.groupBoxAbout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxServiceStatus;
        private System.Windows.Forms.Label labelServiceStatus;
        private System.Windows.Forms.Button buttonStopService;
        private System.Windows.Forms.Button buttonStartService;
        private System.Windows.Forms.Panel panelServiceStatus;
        private System.Windows.Forms.TabControl tabServerSettings;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPageSMTP;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.NumericUpDown numericUpDownServerPort;
        private System.Windows.Forms.Label labelDBFilepath;
        private System.Windows.Forms.Label labelServerPort;
        private System.Windows.Forms.Button buttonBrowseUploadPath;
        private System.Windows.Forms.TextBox textBoxFileUploadPath;
        private System.Windows.Forms.Label labelUploadFolderPath;
        private System.Windows.Forms.Button buttonBrowseDBFolderPath;
        private System.Windows.Forms.TextBox textBoxDatabasePath;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TabPage tabPageLogging;
        private System.Windows.Forms.TabPage tabPageManageSettings;
        private System.Windows.Forms.TabPage tabPageAbout;
        private System.Windows.Forms.NumericUpDown numericUpDownSMTPPort;
        private System.Windows.Forms.Label labelHostPort;
        private System.Windows.Forms.TextBox textBoxSMTPHost;
        private System.Windows.Forms.Label labelSMTPHost;
        private System.Windows.Forms.TextBox textBoxSMTPUsername;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxSMTPPassword;
        private System.Windows.Forms.Label labelSMTPPassword;
        private System.Windows.Forms.CheckBox checkBoxSMTPAuth;
        private System.Windows.Forms.CheckBox checkBoxSSL;
        private System.Windows.Forms.ComboBox comboBoxLogLevel;
        private System.Windows.Forms.Label labelLoggingLevel;
        private System.Windows.Forms.Button buttonBrowseLoggingPath;
        private System.Windows.Forms.TextBox textBoxLogPath;
        private System.Windows.Forms.Label labelLoggingPath;
        private System.Windows.Forms.GroupBox groupBoxExportSettings;
        private System.Windows.Forms.Button buttonExportSettings;
        private System.Windows.Forms.GroupBox groupBoxImportSettings;
        private System.Windows.Forms.TextBox textBoxPreview;
        private System.Windows.Forms.Label labelImportSettings;
        private System.Windows.Forms.Label labelExportSettings;
        private System.Windows.Forms.GroupBox groupBoxReset;
        private System.Windows.Forms.Label labelResetSettings;
        private System.Windows.Forms.Button buttonResetSettings;
        private System.Windows.Forms.Label labelSettingsPreview;
        private System.Windows.Forms.Button buttonSMTPTest;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Button buttonImportSettings;
        private System.Windows.Forms.Button buttonDiscardImportedSettings;
        private System.Windows.Forms.Button buttonApplyImportedSettings;
        private System.Windows.Forms.GroupBox groupBoxAbout;
        private System.Windows.Forms.Label labelHealthGearVersion;
        private System.Windows.Forms.Label labelSoftwareVersion;
        private System.Windows.Forms.Label labelSoftwareName;
        private System.Windows.Forms.RichTextBox richTextBoxLicense;
    }
}

