using System.Diagnostics;
using System.Security.Principal;

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
                    ProcessStartInfo startInfo = new()
                    {
                        FileName = Application.ExecutablePath,
                        Verb = "runas",
                        UseShellExecute = true // 🛠 Necessario per l'UAC
                    };

                    Process.Start(startInfo);
                }
                catch
                {
                    MessageBox.Show("Devi eseguire questa applicazione come amministratore.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // 🛑 Esce immediatamente dal processo corrente
                Environment.Exit(0);
                return;
            }

            // 🛠 Previeni esecuzioni multiple
            using Mutex mutex = new(true, "HealthGearConfigMutex", out bool createdNew);
            if (!createdNew)
            {
                MessageBox.Show("L'app è già in esecuzione.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
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
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
