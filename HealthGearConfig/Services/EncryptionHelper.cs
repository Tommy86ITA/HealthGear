// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace HealthGearConfig.Services
{
    public static class EncryptionHelper
    {
        private static readonly string EncryptionKey = LoadEncryptionKey();

        private static string LoadEncryptionKey()
        {
            string configPath = "encryption_config.json";
            string environment = Environment.GetEnvironmentVariable("HEALTHGEAR_ENV") ?? "development";

            if (environment == "production")
            {
                // 🚀 In produzione, leggiamo solo da variabile d’ambiente
                string? envKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
                if (!string.IsNullOrEmpty(envKey))
                {
                    return envKey;
                }
                throw new Exception("❌ Nessuna chiave di crittografia trovata in produzione!");
            }
            else
            {
                // 🛠️ In sviluppo, proviamo a caricare il file
                if (File.Exists(configPath))
                {
                    try
                    {
                        var json = File.ReadAllText(configPath);
                        var config = JsonConvert.DeserializeObject<EncryptionConfig>(json);

                        if (config == null || string.IsNullOrEmpty(config.EncryptionKey))
                        {
                            throw new Exception("❌ La chiave di crittografia nel file è null o non valida.");
                        }

                        return config.EncryptionKey;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Errore nel caricamento della chiave dal file: {ex.Message}");
                    }
                }

                throw new Exception("❌ Nessuna chiave di crittografia trovata!");
            }
        }

        /// <summary>
        /// Crittografa un testo con AES.
        /// </summary>
        public static string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16]; // IV statico (per semplicità)

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cs))
            {
                writer.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Decrittografa un testo AES.
        /// </summary>
        public static string Decrypt(string encryptedText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16]; // IV statico

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(encryptedText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }
    }

    /// <summary>
    /// Configurazione per la chiave crittografica.
    /// </summary>
    public class EncryptionConfig
    {
        public string EncryptionKey { get; set; } = string.Empty;
    }

}
