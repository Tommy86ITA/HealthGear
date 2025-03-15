// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ServiceProcess;

namespace HealthGearConfig.Services
{
    /// <summary>
    /// Classe statica per la gestione del servizio HealthGear.
    /// Permette di avviare, arrestare e verificare lo stato del servizio.
    /// </summary>
    public static class ServiceManager
    {
        /// <summary>
        /// Nome del servizio di HealthGear.
        /// </summary>
        private const string ServiceName = "HealthGearService";

        /// <summary>
        /// Avvia il servizio HealthGear se non è già in esecuzione.
        /// </summary>
        /// <returns>True se il servizio è stato avviato correttamente, False in caso di errore.</returns>
        public static bool StartService()
        {
            try
            {
                using var service = new ServiceController(ServiceName);
                if (service.Status == ServiceControllerStatus.Stopped || service.Status == ServiceControllerStatus.Paused)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'avvio del servizio: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Arresta il servizio HealthGear se è attualmente in esecuzione.
        /// </summary>
        /// <returns>True se il servizio è stato arrestato correttamente, False in caso di errore.</returns>
        public static bool StopService()
        {
            try
            {
                using var service = new ServiceController(ServiceName);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'arresto del servizio: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Verifica se il servizio HealthGear è installato.
        /// </summary>
        /// <returns>True se il servizio è installato, False altrimenti.</returns>
        public static bool IsServiceInstalled()
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (var service in services)
                {
                    if (service.ServiceName.Equals(ServiceName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore durante la verifica del servizio: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Controlla se il servizio è in esecuzione.
        /// </summary>
        /// <returns>True se il servizio è in esecuzione, False se è fermo, e genera un'eccezione se il servizio non è installato.</returns>
        public static bool IsServiceRunning()
        {
            try
            {
                using var service = new System.ServiceProcess.ServiceController("HealthGearService");
                return service.Status == System.ServiceProcess.ServiceControllerStatus.Running;
            }
            catch
            {
                throw new InvalidOperationException("Il servizio non è installato.");
            }
        }

    }
}
