// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection;

namespace HealthGearConfig.Services
{
    public static class AboutManager
    {
        /// <summary>
        /// Ottiene la versione corrente del software.
        /// </summary>
        public static string GetAppVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Versione non disponibile";
        }

        /// <summary>
        /// Ottiene la versione del software HealthGear.
        /// Se HealthGear non è installato, restituisce un messaggio di errore.
        /// </summary>
        public static string GetHealthGearVersion()
        {
            string[] possiblePaths =
            [
        Path.Combine(Application.StartupPath, "HealthGear", "HealthGear.exe"), // ✅ Sottocartella
        Path.Combine(Application.StartupPath, "..", "HealthGear", "HealthGear.exe"), // ✅ Cartella superiore
        @"C:\Program Files\HealthGear\HealthGear.exe", // ✅ Percorso tipico di installazione
        @"C:\Program Files (x86)\HealthGear\HealthGear.exe" // ✅ Percorso su sistemi a 64 bit
    ];

            foreach (string exePath in possiblePaths)
            {
                if (File.Exists(exePath))
                {
                    try
                    {
                        return FileVersionInfo.GetVersionInfo(exePath).FileVersion ?? "Versione non disponibile";
                    }
                    catch
                    {
                        return "Errore nel recupero versione";
                    }
                }
            }

            return "HealthGear non trovato";
        }

        /// <summary>
        /// Inizializza il tab "About" con informazioni su versione e licenza.
        /// </summary>
        public static void InitializeAboutTab(Label softwareName, Label versionLabel, Label healthGearVersionLabel, RichTextBox licenseBox)
        {
            softwareName.Text = "HealthGearConfig";
            versionLabel.Text = $"Versione Config: {GetAppVersion()}";
            healthGearVersionLabel.Text = $"Versione HealthGear: {GetHealthGearVersion()}";

            licenseBox.Text = "HealthGearConfig è distribuito sotto licenza proprietaria.\n\n"
                 + "© 2025 Thomas Amaranto\n\n"
                 + "*Proprietà*\n"
                 + "Il software HealthGearConfig è di proprietà esclusiva di Thomas Amaranto.\n\n"
                 + "*Licenza d'uso per Studio C.R.E.A.*\n"
                 + "È concessa allo Studio C.R.E.A. una licenza d’uso non esclusiva e non trasferibile, "
                 + "limitata all’uso interno presso la propria struttura.\n\n"
                 + "*Restrizioni*\n"
                 + "Qualsiasi modifica, distribuzione, concessione a terzi o utilizzo al di fuori della "
                 + "struttura è espressamente vietato senza il consenso scritto dell’Autore.\n\n"
                 + "*Esclusione di garanzia*\n"
                 + "Il software è fornito \"così com’è\", senza alcuna garanzia espressa o implicita, incluse, "
                 + "a titolo esemplificativo, le garanzie di commerciabilità, idoneità per un fine particolare "
                 + "e non violazione di diritti altrui.\n\n"
                 + "*Limitazione di responsabilità*\n"
                 + "In nessun caso l’autore potrà essere ritenuto responsabile per eventuali danni derivanti "
                 + "dall’uso del software.\n\n"
                 + "*Tutti i diritti sono riservati.*";
            licenseBox.ReadOnly = true;
            licenseBox.BackColor = SystemColors.Window;
        }
    }
}

