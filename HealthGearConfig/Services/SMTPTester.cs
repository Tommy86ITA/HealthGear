// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace HealthGearConfig.Services
{
    /// <summary>
    /// Classe per il test della configurazione SMTP.
    /// Verifica se i parametri SMTP inseriti permettono di connettersi al server.
    /// </summary>
    public static class SmtpTester
    {
        /// <summary>
        /// Esegue un test di connessione con il server SMTP specificato.
        /// </summary>
        /// <param name="host">Hostname del server SMTP</param>
        /// <param name="port">Porta SMTP</param>
        /// <param name="username">Username per l'autenticazione</param>
        /// <param name="password">Password per l'autenticazione</param>
        /// <param name="useSSL">Specifica se usare SSL</param>
        public static void TestSMTP(string host, int port, string username, string password, bool useSSL)
        {
            try
            {
                // 📧 Configura il client SMTP
                using (SmtpClient client = new SmtpClient(host, port))
                {
                    client.Credentials = new NetworkCredential(username, password);
                    client.EnableSsl = useSSL;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 5000; // ⏳ Timeout di 5 secondi

                    // 🔄 Tentativo di connessione al server SMTP
                    client.Send(new MailMessage(username, username, "Test SMTP", "Questo è un test di connessione SMTP."));
                }

                // ✅ Connessione riuscita
                MessageBox.Show("Connessione SMTP riuscita! Le impostazioni sono valide.",
                    "Test SMTP", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // ❌ Connessione fallita
                MessageBox.Show($"Errore nella connessione SMTP:\n{ex.Message}",
                    "Errore SMTP", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
