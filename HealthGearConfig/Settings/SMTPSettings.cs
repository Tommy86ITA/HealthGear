// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace HealthGearConfig.Settings
{
    /// <summary>
    /// Classe che rappresenta le impostazioni SMTP per l'invio delle email.
    /// </summary>
    public class SMTPSettings
    {
        /// <summary>
        /// Indirizzo del server SMTP.
        /// </summary>
        public string Host { get; set; } = "smtp.example.com";

        /// <summary>
        /// Porta del server SMTP (es. 587 per STARTTLS, 465 per SSL).
        /// </summary>
        public int Port { get; set; } = 587;

        /// <summary>
        /// Nome utente per l'autenticazione SMTP.
        /// </summary>
        public string Username { get; set; } = "admin@example.com";

        /// <summary>
        /// Password crittografata per l'autenticazione SMTP.
        /// </summary>
        public string Password { get; set; } = "";

        /// <summary>
        /// Indica se la connessione deve utilizzare SSL/TLS.
        /// </summary>
        public bool UseSSL { get; set; } = true;

        /// <summary>
        /// Indica se il server SMTP richiede autenticazione.
        /// </summary>
        public bool RequiresAuthentication { get; set; } = true;
    }
}
