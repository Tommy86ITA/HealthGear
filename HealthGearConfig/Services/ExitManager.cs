// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using HealthGearConfig.Services;
using System.Windows.Forms;

namespace HealthGearConfig
{
    public static class ExitManager
    {
        /// <summary>
        /// Gestisce l'uscita dall'applicazione, mostrando avvisi se necessario.
        /// </summary>
        public static bool ConfirmExit(bool settingsModified)
        {
            if (settingsModified)
            {
                DialogResult result = MessageBox.Show(
                    "Ci sono modifiche non salvate. Vuoi salvare prima di uscire?",
                    "Attenzione",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Cancel)
                {
                    return false; // ❌ L'utente ha scelto di rimanere nell'app
                }
                else if (result == DialogResult.Yes)
                {
                    ConfigManager.SaveSettings(ConfigManager.LoadSettings()); // ✅ Salva prima di uscire
                }
            }

            // Se il servizio è installato ma non in esecuzione, avvisiamo
            if (ServiceManager.IsServiceInstalled() && !ServiceManager.IsServiceRunning())
            {
                DialogResult serviceResult = MessageBox.Show(
                    "Stai per chiudere l'applicazione, ma il servizio non è in esecuzione.\n\nVuoi avviare il servizio prima di uscire?",
                    "Servizio non attivo",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (serviceResult == DialogResult.Cancel)
                {
                    return false;
                }
                else if (serviceResult == DialogResult.Yes)
                {
                    ServiceManager.StartService(); // ✅ Avvia il servizio prima di chiudere
                }
            }

            return true; // ✅ L'utente ha confermato l'uscita
        }
    }
}
