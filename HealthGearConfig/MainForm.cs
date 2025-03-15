// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using HealthGearConfig.Services;
using HealthGearConfig.Settings;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace HealthGearConfig
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Variabile di stato per verificare se le impostazioni sono state modificate ma non salvate.
        /// </summary>
        private bool settingsModified = false;

        /// <summary>
        /// Variabile di stato per verificare se l'utente ha confermato l'uscita dall'applicazione.
        /// </summary>
        private bool exitConfirmed = false;


        /// <summary>
        /// Costruttore della finestra principale.
        /// Inizializza i componenti e aggiorna lo stato del servizio all'avvio.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // Rileva modifiche nelle impostazioni per mostrare un avviso se non salvate
            numericUpDownServerPort.ValueChanged += SettingsChanged;
            textBoxDatabasePath.TextChanged += SettingsChanged;
            textBoxFileUploadPath.TextChanged += SettingsChanged;
            textBoxSMTPHost.TextChanged += SettingsChanged;
            numericUpDownSMTPPort.ValueChanged += SettingsChanged;
            textBoxSMTPUsername.TextChanged += SettingsChanged;
            textBoxSMTPPassword.TextChanged += SettingsChanged;
            checkBoxSSL.CheckedChanged += SettingsChanged;
            checkBoxSMTPAuth.CheckedChanged += SettingsChanged;
            comboBoxLogLevel.SelectedIndexChanged += SettingsChanged;
            textBoxLogPath.TextChanged += SettingsChanged;
            this.FormClosing += Form1_OnFormClosing;

            LoadSettings(); // ✅ Carica le impostazioni all'avvio
            UpdateServiceStatus(); // ✅ Controlla lo stato del servizio all'avvio
            AboutManager.InitializeAboutTab(labelSoftwareName, labelSoftwareVersion, labelHealthGearVersion, richTextBoxLicense); // ✅ Inizializza il tab "About"
            settingsModified = false; // 🚫 Resetta il flag delle impostazioni modificate
        }


        ///--------------------------------------------------------------------------------
        /// Metodi per la gestione dell'avvio e arresto del servizio
        ///--------------------------------------------------------------------------------

        /// <summary>
        /// Gestisce il click sul pulsante "Avvia Servizio".
        /// Avvia il servizio e aggiorna la UI in base al nuovo stato.
        /// </summary>
        private void buttonStartService_Click(object sender, EventArgs e)
        {
            if (ServiceManager.StartService())
            {
                UpdateServiceStatus(); // ✅ Aggiorna UI dopo l'avvio
            }
            else
            {
                _ = MessageBox.Show("Errore durante l'avvio del servizio.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Ferma Servizio".
        /// Arresta il servizio e aggiorna la UI in base al nuovo stato.
        /// </summary>
        private void buttonStopService_Click(object sender, EventArgs e)
        {
            if (ServiceManager.StopService())
            {
                UpdateServiceStatus(); // ✅ Aggiorna UI dopo lo stop
            }
            else
            {
                _ = MessageBox.Show("Errore durante l'arresto del servizio.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        ///--------------------------------------------------------------------------------
        /// Gestione dei pulsanti per la selezione delle cartelle
        ///--------------------------------------------------------------------------------

        /// <summary>
        /// Gestione logica dei folder browser per la selezione del percorso del database, cartella di upload e log.
        /// <summay/>
        private void buttonBrowseDBFolderPath_Click(object sender, EventArgs e)
        {
            BrowseDatabasePath_Click(sender, e);
        }

        private void buttonBrowseUploadPath_Click(object sender, EventArgs e)
        {
            BrowseFileUploadPath_Click(sender, e);
        }

        private void buttonBrowseLoggingPath_Click(object sender, EventArgs e)
        {
            BrowseLogPath_Click(sender, e);
        }

        ///--------------------------------------------------------------------------------
        /// Metodi helper per la gestione dei folder browser
        ///--------------------------------------------------------------------------------

        /// <summary>
        /// Apre una finestra di dialogo per selezionare la cartella in cui verrà salvato il database.
        /// </summary>
        private void BrowseDatabasePath_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new();
            dialog.Description = "Seleziona la cartella dove verrà salvato il database";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxDatabasePath.Text = Path.Combine(dialog.SelectedPath, "HealthGear.db");
            }
        }

        /// <summary>
        /// Apre una finestra di dialogo per selezionare la cartella per il caricamento dei file.
        /// </summary>
        private void BrowseFileUploadPath_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new();
            dialog.Description = "Seleziona la cartella per il salvataggio dei file caricati";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFileUploadPath.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// Apre una finestra di dialogo per selezionare la cartella di logging.
        /// </summary>
        private void BrowseLogPath_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new();
            dialog.Description = "Seleziona la cartella di logging";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxLogPath.Text = dialog.SelectedPath;
            }
        }

        ///--------------------------------------------------------------------------------
        /// Gestione dei pulsanti per l'esportazione, importazione e reset delle impostazioni
        ///--------------------------------------------------------------------------------
        private void buttonExportSettings_Click(object sender, EventArgs e)
        {
            ExportSettings(); // ✅ Esporta le impostazioni
        }

        private void buttonImportSettings_Click(object sender, EventArgs e)
        {
            ImportSettings();
        }

        private void buttonApplyImportedSettings_Click(object sender, EventArgs e)
        {
            ApplyImportedSettings();
        }

        private void buttonDiscardImportedSettings_Click(object sender, EventArgs e)
        {
            DiscardImportedSettings();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveSettings(); // ✅ Salva le impostazioni
        }

        private void buttonResetSettings_Click(object sender, EventArgs e)
        {
            ResetSettings(); // ✅ Ripristina le impostazioni predefinite
        }


        ///--------------------------------------------------------------------------------
        /// Metodi helper per la gestione dell'interfaccia grafica
        ///--------------------------------------------------------------------------------

        /// <summary>
        /// Aggiorna l'interfaccia utente in base allo stato attuale del servizio.
        /// - Cambia il colore del pannello di stato.
        /// - Aggiorna il testo dello stato del servizio.
        /// - Abilita/disabilita i pulsanti e i controlli in base allo stato del servizio.
        /// </summary>
        private void UpdateServiceStatus()
        {
            try
            {
                bool isRunning = ServiceManager.IsServiceRunning();

                // Stato del servizio
                if (isRunning)
                {
                    labelServiceStatus.Text = "Servizo in esecuzione";
                    panelServiceStatus.BackColor = Color.Green;
                }
                else
                {
                    labelServiceStatus.Text = "Servizio arrestato";
                    panelServiceStatus.BackColor = Color.Red;
                }

                // Abilita/disabilita i pulsanti in base allo stato del servizio
                buttonStartService.Enabled = !isRunning;
                // Disabilita il pulsante di arresto se il servizio è già fermo
                buttonStopService.Enabled = isRunning;

                // Disabilita la tab delle impostazioni del server se il servizio è attivo
                tabServerSettings.Enabled = !isRunning;

                // Disabilita i controlli di importazione se il servizio è attivo
                groupBoxImportSettings.Enabled = !isRunning;

                // Disabilita il pulsante di reset delle impostazioni se il servizio è attivo
                buttonResetSettings.Enabled = !isRunning;
            }
            catch (Exception)
            {
                // Se il servizio non è installato, gestiamo lo stato specifico
                labelServiceStatus.Text = "Servizio non installato";
                panelServiceStatus.BackColor = Color.Gray;

                // Disabilitiamo i pulsanti di avvio e arresto
                buttonStartService.Enabled = false;
                buttonStopService.Enabled = false;

                // Abilitiamo il reset delle impostazioni
                buttonResetSettings.Enabled = true;

                // Mostriamo l'avviso solo se non è già stato mostrato
                _ = MessageBox.Show(
                    "Il servizio HealthGear non è installato su questo sistema.\n" +
                    "Contatta l'Amministratore di sistema.",
                    "Servizio non installato",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        ///--------------------------------------------------------------------------------
        /// Metodi per la gestione delle impostazioni (caricamento, salvataggio, 
        /// esportazione, importazione e reset)
        ///--------------------------------------------------------------------------------

        /// <summary>
        /// Richiamato quando le impostazioni vengono modificate
        /// <!-- Aggiunge un flag per segnalare che le impostazioni sono state modificate -->
        /// </summary>
        private void SettingsChanged(object sender, EventArgs e)
        {
            settingsModified = true;
        }

        /// <summary>
        /// Carica le impostazioni dal file JSON e aggiorna l'interfaccia utente.
        /// </summary>
        private void LoadSettings()
        {
            var settings = ConfigManager.LoadSettings();

            // Impostazioni Server
            numericUpDownServerPort.Value = settings.Server.Port;
            textBoxDatabasePath.Text = settings.Server.DatabasePath;
            textBoxFileUploadPath.Text = settings.Server.FileUploadPath;

            // Impostazioni SMTP
            textBoxSMTPHost.Text = settings.SMTP.Host;
            numericUpDownSMTPPort.Value = settings.SMTP.Port;
            textBoxSMTPUsername.Text = settings.SMTP.Username;
            textBoxSMTPPassword.Text = settings.SMTP.Password;
            checkBoxSSL.Checked = settings.SMTP.UseSSL;
            checkBoxSMTPAuth.Checked = settings.SMTP.RequiresAuthentication;

            // Impostazioni Logging
            if (comboBoxLogLevel.Items.Contains(settings.Logging.LogLevel))
            {
                comboBoxLogLevel.SelectedItem = settings.Logging.LogLevel;
            }
            else
            {
                comboBoxLogLevel.SelectedIndex = 0; // Imposta "Verbose" come default
            }
            textBoxLogPath.Text = settings.Logging.LogPath;
        }

        /// <summary>
        /// Salva le impostazioni attuali nel file JSON.
        /// </summary>
        private void SaveSettings()
        {
            var settings = new AppSettings
            {
                Server = new ServerSettings
                {
                    Port = (int)numericUpDownServerPort.Value,
                    DatabasePath = textBoxDatabasePath.Text,
                    FileUploadPath = textBoxFileUploadPath.Text
                },
                SMTP = new SMTPSettings
                {
                    Host = textBoxSMTPHost.Text,
                    Port = (int)numericUpDownSMTPPort.Value,
                    Username = textBoxSMTPUsername.Text,
                    Password = textBoxSMTPPassword.Text, // 🔐 Verrà crittografata in ConfigManager
                    UseSSL = checkBoxSSL.Checked,
                    RequiresAuthentication = checkBoxSMTPAuth.Checked
                },
                Logging = new LoggingSettings
                {
                    LogLevel = comboBoxLogLevel.SelectedItem.ToString(),
                    LogPath = textBoxLogPath.Text
                }
            };

            ConfigManager.SaveSettings(settings);
            settingsModified = false; // 🚫 Resetta il flag delle impostazioni modificate
            _ = MessageBox.Show("Impostazioni salvate con successo!", "Salvataggio", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Esporta le impostazioni attuali in un file JSON selezionato dall'utente.
        /// </summary>
        private void ExportSettings()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "File JSON (*.json)|*.json";
                saveFileDialog.Title = "Esporta Configurazione";
                saveFileDialog.FileName = "HealthGearConfig_Export.json";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var settings = ConfigManager.LoadSettings(); // 📌 Carica le impostazioni attuali
                    bool success = ConfigManager.ExportSettings(saveFileDialog.FileName, settings);

                    if (success)
                    {
                        _ = MessageBox.Show("Esportazione completata con successo!", "Esportazione", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        _ = MessageBox.Show("Errore durante l'esportazione delle impostazioni.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Importa le impostazioni da un file JSON selezionato dall'utente.
        /// </summary>
        private void ImportSettings()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "File JSON (*.json)|*.json";
                openFileDialog.Title = "Importa Configurazione";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var importedSettings = ConfigManager.ImportSettings(openFileDialog.FileName);

                        if (importedSettings == null)
                        {
                            throw new Exception("Il file selezionato non contiene una configurazione valida.");
                        }

                        // 📌 Se il JSON è valido, aggiorniamo l'anteprima e abilitiamo i pulsanti
                        textBoxPreview.Text = File.ReadAllText(openFileDialog.FileName);
                        buttonApplyImportedSettings.Enabled = true;
                        buttonDiscardImportedSettings.Enabled = true;

                        _ = MessageBox.Show("Impostazioni importate con successo! Controlla l'anteprima prima di applicarle.",
                            "Importazione completata", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show($"Errore: Il file selezionato non è valido per HealthGear.\n\nDettagli: {ex.Message}",
                            "Errore di Importazione", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        // 📌 Disabilitiamo i pulsanti perché il file non è valido
                        buttonApplyImportedSettings.Enabled = false;
                        buttonDiscardImportedSettings.Enabled = false;
                        textBoxPreview.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Applica le impostazioni importate e le salva nel file di configurazione.
        /// </summary>
        private void ApplyImportedSettings()
        {
            var importedSettings = ConfigManager.ImportSettings(textBoxPreview.Text);

            if (importedSettings != null)
            {
                // Aggiorniamo l'interfaccia con le nuove impostazioni
                numericUpDownServerPort.Value = importedSettings.Server.Port;
                textBoxDatabasePath.Text = importedSettings.Server.DatabasePath;
                textBoxFileUploadPath.Text = importedSettings.Server.FileUploadPath;

                textBoxSMTPHost.Text = importedSettings.SMTP.Host;
                numericUpDownSMTPPort.Value = importedSettings.SMTP.Port;
                textBoxSMTPUsername.Text = importedSettings.SMTP.Username;
                textBoxSMTPPassword.Text = importedSettings.SMTP.Password;
                checkBoxSSL.Checked = importedSettings.SMTP.UseSSL;
                checkBoxSMTPAuth.Checked = importedSettings.SMTP.RequiresAuthentication;

                comboBoxLogLevel.SelectedItem = importedSettings.Logging.LogLevel;
                textBoxLogPath.Text = importedSettings.Logging.LogPath;

                // Salviamo le nuove impostazioni
                ConfigManager.SaveSettings(importedSettings);

                _ = MessageBox.Show("Le impostazioni importate sono state applicate con successo!",
                    "Applicazione completata", MessageBoxButtons.OK, MessageBoxIcon.Information);

                buttonApplyImportedSettings.Enabled = false; // Disabilita il pulsante dopo l'applicazione
            }
            else
            {
                _ = MessageBox.Show("Errore durante l'applicazione delle impostazioni importate.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Scarta le impostazioni importate e svuota l'anteprima.
        /// </summary>
        private void DiscardImportedSettings()
        {
            textBoxPreview.Clear();
            buttonApplyImportedSettings.Enabled = false;
            buttonDiscardImportedSettings.Enabled = false;

            _ = MessageBox.Show("Le impostazioni importate sono state scartate.",
                "Impostazioni scartate", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Ripristina tutte le impostazioni ai valori predefiniti.
        /// </summary>

        private void ResetSettings()
        {
            if (MessageBox.Show("Sei sicuro di voler ripristinare le impostazioni predefinite?",
                "Conferma Ripristino", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var defaultSettings = new AppSettings(); // Crea una nuova istanza con valori di default

                ConfigManager.SaveSettings(defaultSettings); // Salva le impostazioni predefinite

                LoadSettings(); // Ricarica le impostazioni per aggiornare la UI

                _ = MessageBox.Show("Impostazioni ripristinate ai valori predefiniti.",
                    "Ripristino completato", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        ///--------------------------------------------------------------------------------
        /// Gestione degli eventi di chiusura dell'applicazione
        ///--------------------------------------------------------------------------------
        private void buttonExit_Click(object sender, EventArgs e)
        {
            ExitApplication(); // ✅ Gestisce l'uscita dall'applicazione
        }

        /// <summary>
        /// Gestisce la chiusura dell'applicazione quando l'utente clicca sulla "X" o preme ALT+F4.
        /// </summary>
        private void Form1_OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!exitConfirmed) // ✅ Se non è stato già confermato, chiedi conferma
            {
                ExitApplication();
                e.Cancel = !exitConfirmed; // ❌ Annulla la chiusura se l'utente ha scelto di restare
            }
        }

        /// <summary>
        /// Gestisce l'uscita dall'applicazione, mostrando avvisi se necessario.
        /// </summary>
        private void ExitApplication()
        {
            // Se la conferma è già stata data, esci direttamente
            if (exitConfirmed)
            {
                Application.Exit();
                return;
            }

            if (ExitManager.ConfirmExit(settingsModified))
            {
                exitConfirmed = true; // ✅ Imposta il flag per evitare doppia richiesta
                Application.Exit();
            }
        }

        private void buttonSMTPTest_Click(object sender, EventArgs e)
        {
            // 🔍 Recupera i parametri attuali dalla UI e li passa a SmtpTester
            SmtpTester.TestSMTP(
                textBoxSMTPHost.Text,
                (int)numericUpDownSMTPPort.Value,
                textBoxSMTPUsername.Text,
                textBoxSMTPPassword.Text,
                checkBoxSSL.Checked
            );
        }
    }
}
