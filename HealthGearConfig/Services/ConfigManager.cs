// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using HealthGearConfig.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HealthGearConfig.Services
{
    /// <summary>
    /// Classe per la gestione della configurazione dell'applicazione.
    /// Carica e salva le impostazioni da/verso un file JSON.
    /// </summary>
    public static class ConfigManager
    {
        /// <summary>
        /// Percorso del file di configurazione JSON.
        /// </summary>
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "healthgear.config.json");

        /// <summary>
        /// Opzioni di serializzazione JSON riutilizzabili per migliorare la performance.
        /// </summary>
        private static readonly JsonSerializerSettings JsonOptions = new()
        {
            Formatting = Formatting.Indented // Imposta il JSON con indentazione leggibile
        };

        /// <summary>
        /// Carica le impostazioni dal file JSON.
        /// Se il file non esiste, restituisce un oggetto con valori di default.
        /// </summary>
        /// <returns>Oggetto AppSettings contenente le impostazioni caricate.</returns>
        public static AppSettings LoadSettings()
        {
            // ✅ Se il file non esiste, lo creiamo con valori predefiniti
            if (!File.Exists(ConfigFilePath))
            {
                Console.WriteLine("⚠️ File di configurazione non trovato. Creazione di un nuovo file...");
                var defaultSettings = new AppSettings();
                SaveSettings(defaultSettings);
                return defaultSettings;
            }

            try
            {
                string json = File.ReadAllText(ConfigFilePath);
                var settings = JsonConvert.DeserializeObject<AppSettings>(json, JsonOptions) ?? new AppSettings();

                // 🔓 Decrittografiamo la password SMTP solo se è crittografata
                if (!string.IsNullOrEmpty(settings.SMTP.Password) && settings.SMTP.Password.StartsWith("ENC:"))
                {
                    try
                    {
                        settings.SMTP.Password = EncryptionHelper.Decrypt(settings.SMTP.Password.Substring(4));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Errore nella decrittazione della password SMTP: {ex.Message}");
                        settings.SMTP.Password = ""; // Preveniamo un valore errato
                    }
                }

                return settings;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ Errore durante il parsing del file JSON: {ex.Message}");
                return new AppSettings();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"❌ Errore di I/O durante la lettura del file JSON: {ex.Message}");
                return new AppSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore generico durante il caricamento delle impostazioni: {ex.Message}");
                return new AppSettings();
            }
        }



        /// <summary>
        /// Salva le impostazioni nel file JSON.
        /// Crittografa automaticamente la password SMTP prima di salvarla.
        /// Se il file non esiste, ne crea uno.
        /// </summary>
        /// <param name="settings">Oggetto AppSettings contenente le impostazioni da salvare.</param>
        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                // ✅ Se il file non esiste, lo creiamo con valori predefiniti
                if (!File.Exists(ConfigFilePath))
                {
                    Console.WriteLine("⚠️ Il file di configurazione non esiste. Creazione di uno nuovo...");
                    File.WriteAllText(ConfigFilePath, "{}"); // Creiamo un file JSON vuoto
                }

                // 🔐 Crittografiamo la password SMTP solo se non è già crittografata
                if (!string.IsNullOrEmpty(settings.SMTP.Password) && !settings.SMTP.Password.StartsWith("ENC:"))
                {
                    settings.SMTP.Password = "ENC:" + EncryptionHelper.Encrypt(settings.SMTP.Password);
                }

                string json = JsonConvert.SerializeObject(settings, JsonOptions);
                File.WriteAllText(ConfigFilePath, json);
                Console.WriteLine("✅ Configurazione salvata con successo.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("❌ Errore: Accesso negato. Prova a eseguire l'app come amministratore.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"❌ Errore di I/O durante il salvataggio delle impostazioni: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore generico nel salvataggio delle impostazioni: {ex.Message}");
            }
        }



        /// <summary>
        /// Esporta le impostazioni in un file JSON specificato dall'utente.
        /// </summary>
        public static bool ExportSettings(string filePath, AppSettings settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore durante l'esportazione delle impostazioni: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Importa le impostazioni da un file JSON selezionato dall'utente.
        /// </summary>
        public static AppSettings? ImportSettings(string filePath)

        {
            try
            {
                if (!File.Exists(filePath)) return null;

                string json = File.ReadAllText(filePath);

                if (!IsJsonValid(json))
                {
                    throw new Exception("Il file JSON non contiene tutti i parametri richiesti.");
                }

                var importedSettings = JsonConvert.DeserializeObject<AppSettings>(json) ?? throw new Exception("Errore nella deserializzazione delle impostazioni.");

                // 🔓 Decrittografiamo la password SMTP solo se è crittografata
                if (!string.IsNullOrEmpty(importedSettings.SMTP.Password) && importedSettings.SMTP.Password.StartsWith("ENC:"))
                {
                    try
                    {
                        importedSettings.SMTP.Password = EncryptionHelper.Decrypt(importedSettings.SMTP.Password.Substring(4));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Errore nella decrittazione della password SMTP: {ex.Message}");
                        importedSettings.SMTP.Password = "";
                    }
                }

                return importedSettings;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ Errore nel parsing del JSON: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore durante l'importazione delle impostazioni: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Verifica manualmente se il file JSON contiene tutte le sezioni richieste.
        /// </summary>
        private static bool IsJsonValid(string json)
        {
            try
            {
                JObject parsedJson = JObject.Parse(json);

                return parsedJson.ContainsKey("Server") &&
                       parsedJson.ContainsKey("SMTP") &&
                       parsedJson.ContainsKey("Logging");
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Classe che rappresenta la configurazione dell'applicazione.
    /// Contiene tutte le impostazioni per server, email e logging.
    /// </summary>
    public class AppSettings
    {
        public ServerSettings Server { get; set; } = new ServerSettings();
        public SMTPSettings SMTP { get; set; } = new SMTPSettings();
        public LoggingSettings Logging { get; set; } = new LoggingSettings();
    }
}
