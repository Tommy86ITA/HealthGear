// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace HealthGearConfig
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // 🔍 Controlla se l'app è in esecuzione come amministratore
            if (!IsRunningAsAdministrator())
            {
                try
                {
                    // 🔄 Riavvia l'app con privilegi di amministratore
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = Application.ExecutablePath,
                        Verb = "runas" // 🔥 Questo forza l'elevazione dei privilegi
                    };

                    Process.Start(startInfo);
                    return; // ❌ Esce dall'istanza corrente
                }
                catch
                {
                    MessageBox.Show("Devi eseguire questa applicazione come amministratore.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// Controlla se il programma è in esecuzione con privilegi di amministratore.
        /// </summary>
        private static bool IsRunningAsAdministrator()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}