// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace HealthGearConfig.Settings
{
    /// <summary>
    /// Classe che rappresenta le impostazioni del server.
    /// Contiene le configurazioni per la porta del server, il database e il percorso di upload dei file.
    /// </summary>
    public class ServerSettings
    {
        /// <summary>
        /// Porta su cui il server è in ascolto.
        /// </summary>
        public int Port { get; set; } = 5051;

        /// <summary>
        /// Percorso del file database SQLite.
        /// </summary>
        public string DatabasePath { get; set; } = @"C:\ProgramData\HealthGear\healthgear.db";

        /// <summary>
        /// Percorso della cartella in cui vengono salvati i file caricati dagli utenti.
        /// </summary>
        public string FileUploadPath { get; set; } = @"C:\ProgramData\HealthGear\Uploads";
    }
}
