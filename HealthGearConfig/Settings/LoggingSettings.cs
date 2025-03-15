// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace HealthGearConfig.Settings
{
    /// <summary>
    /// Classe che rappresenta le impostazioni di logging.
    /// </summary>
    public class LoggingSettings
    {
        /// <summary>
        /// Livello di log (Verbose, Warning, Error).
        /// </summary>
        public string LogLevel { get; set; } = "Information";

        /// <summary>
        /// Percorso della cartella in cui vengono salvati i file di log.
        /// </summary>
        public string LogPath { get; set; } = @"C:\ProgramData\HealthGear\Logs";
    }
}
