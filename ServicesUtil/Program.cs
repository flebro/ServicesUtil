using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServicesUtil
{
    class Program
    {
        #region Constantes

        #region Commandes

        private const string COMMAND_LIST = "list";
        private const string COMMAND_SELECT = "select";
        private const string COMMAND_DETAILS = "details";
        private const string COMMAND_STOP = "stop";
        private const string COMMAND_START = "start";
        private const string COMMAND_CONTINUE = "continue";
        private const string COMMAND_MACHINE = "machine";
        private const string COMMAND_PAUSE = "pause";
        private const string COMMAND_EXIT = "exit";

        #endregion

        #region Arguments

        private const string ARG_SHOW_STATUS = "/status";
        private const string ARG_RUNNING = "/running";
        private const string ARG_PAUSED = "/paused";
        private const string ARG_STOPPED = "/stopped";

        #endregion

        private const string LOCALHOST = "localhost";

        #endregion

        #region Attributes

        private string _currentMachine;
        private string _currentService;

        #endregion

        #region Methods

        #region Point d'entrée

        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.StartUp();
        }

        #endregion

        #region Interface

        public void StartUp()
        {
            // Init
            _currentMachine = LOCALHOST;

            // Boucle d'éxécution
            bool exited = false;
            do
            {
                Console.Clear();
                ShowMenu();

                // On lit la commande
                string[] tags = Console.ReadLine().ToLower().Trim().Split(' ');

                if (tags.Length > 0)
                {
                    // On parse
                    string command = tags[0];
                    IEnumerable<string> commandArgs = tags.Skip(1);

                    // On traite
                    if (command.StartsWith(COMMAND_EXIT)) break;
                    else if (command.StartsWith(COMMAND_LIST)) List(commandArgs);
                    else if (command.StartsWith(COMMAND_MACHINE)) Machine(commandArgs);
                    else if (command.StartsWith(COMMAND_SELECT)) Select(commandArgs);
                    else if (_currentService != null)
                    {
                        if (command.StartsWith(COMMAND_DETAILS)) Details();
                        else if (command.StartsWith(COMMAND_START)) Start(commandArgs);
                        else if (command.StartsWith(COMMAND_STOP)) Stop();
                        else if (command.StartsWith(COMMAND_PAUSE)) Pause();
                        else if (command.StartsWith(COMMAND_CONTINUE)) Continue();
                    }
                    Console.ReadKey();
                }
            } while (true);

        }

        /// <summary>
        /// Affiche le menu principal
        /// </summary>
        public void ShowMenu()
        {
            Console.WriteLine($"Machine sélectionnée : {_currentMachine}");
            if (_currentService != null)
            {
                Console.WriteLine($"Service sélectionné : {_currentService}");
            }

            Console.WriteLine();

            Console.WriteLine("Lister les services :");
            Console.WriteLine("\tlist <Recherche>");
            Console.WriteLine();

            Console.WriteLine("\tArguments :");
            Console.WriteLine("\t\t/running :\tlister les services démarrés");
            Console.WriteLine("\t\t/stopped :\tlister les services arrêtés");
            Console.WriteLine("\t\t/paused :\tlister les services en pause");
            Console.WriteLine("\t\t/status :\tafficher le status des services");
            Console.WriteLine();

            if (_currentService != null)
            {
                Console.WriteLine("Afficher les détails du service sélectionné :");
                Console.WriteLine("\tdetails");
                Console.WriteLine();
                Console.WriteLine("Gérer le service :");
                Console.WriteLine("\tstart");
                Console.WriteLine("\tstop");
                Console.WriteLine("\tpause");
                Console.WriteLine("\tcontinue");
                Console.WriteLine();
            }

            Console.WriteLine("Sélectionner un service :");
            Console.WriteLine("\tselect <nom système du service>");
            Console.WriteLine();
            Console.WriteLine("Changer de machine :");
            Console.WriteLine("\tmachine <nom ou adresse IP de la machine>");
            Console.WriteLine();
            Console.WriteLine("Quitter:");
            Console.WriteLine("\texit");
            Console.WriteLine();
        }

        #endregion

        #region Commandes

        public void List(IEnumerable<string> args)
        {
            List<ServiceControllerStatus> status = new List<ServiceControllerStatus>();
            bool showStatus = false;
            string search = null;

            // D'abord on traite les arguments
            foreach (string arg in args)
            {
                if (arg.Equals(ARG_RUNNING)) status.Add(ServiceControllerStatus.Running);
                else if (arg.Equals(ARG_PAUSED)) status.Add(ServiceControllerStatus.Paused);
                else if (arg.Equals(ARG_STOPPED)) status.Add(ServiceControllerStatus.Stopped);
                else if (arg.Equals(ARG_SHOW_STATUS)) showStatus = true;
                else search = arg;
            }
            // On gère le cas particulier où il n'y a pas de filtre
            if (status.Count == 0) status.AddRange(new ServiceControllerStatus[] { ServiceControllerStatus.Running, ServiceControllerStatus.Paused, ServiceControllerStatus.Stopped });

            // On définit le layout
            string layout = "[{0}]\t{1}";
            if (showStatus) layout += "\t{2}";

            // On fait la recherche
            ServiceController.GetServices(_currentMachine).ToList().ForEach(sc =>
            {
                if (status.Contains(sc.Status) && (search == null || sc.ServiceName.ToLower().Contains(search)))
                {
                    Console.WriteLine(layout, sc.ServiceName, sc.DisplayName, sc.Status);
                }
                sc.Dispose();
            });

            Console.WriteLine();
            Console.ReadKey();
        }

        public void Machine(IEnumerable<string> args)
        {
            string newMachine = null;
            if (args.Any())
            {
                newMachine = args.First();
            }
            else
            {
                Console.WriteLine("Entrer le nom ou l'adresse IP de la machine cible ou vide pour cette machine :");
                _currentMachine = Console.ReadLine();
            }

            if (string.IsNullOrWhiteSpace(newMachine)) newMachine = LOCALHOST;
            
            if (newMachine != _currentMachine)
            {
                _currentMachine = newMachine;
                _currentService = null;
            }
        }

        public void Select(IEnumerable<string> args)
        {
            if (args.Any())
            {
                _currentService = args.First();
                try
                {
                    Details();
                } catch
                {
                    OnError();
                }
            }
        }

        public void Details()
        {
            using (ServiceController sc = new ServiceController(_currentService, _currentMachine))
            {
                Console.WriteLine();
                Console.WriteLine($"Machine = {sc.MachineName}");
                Console.WriteLine($"Name = {sc.ServiceName}");
                Console.WriteLine($"Display name = {sc.DisplayName}");
                Console.WriteLine($"Status = {(object)sc.Status}");
                Console.WriteLine($"Can Pause and Continue = {sc.CanPauseAndContinue}");
                Console.WriteLine($"Can ShutDown = {sc.CanShutdown}");
                Console.WriteLine($"Can Stop = {sc.CanStop}");
                Console.WriteLine($"Service type = {sc.ServiceType}");
            }
        }

        public void Start(IEnumerable<string> args)
        {
            try
            {
                using (ServiceController sc = new ServiceController(_currentService, _currentMachine))
                {
                    Console.WriteLine($"Tentative de démarrage du service {sc.DisplayName}");
                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        Console.WriteLine($"Le service n'est pas à l'arrêt. État : {sc.Status}");
                    }
                    else
                    {
                        try
                        {
                            sc.Start(args.ToArray());
                            sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                            Console.WriteLine("Le service est en cours d'exécution.");
                        }
                        catch
                        {
                            Console.WriteLine("Impossible de démarrer le service.");
                        }
                    }
                }
            }
            catch
            {
                OnError();
            }
        }

        public void Stop()
        {
            try
            {
                using (ServiceController sc = new ServiceController(_currentService, _currentMachine))
                {
                    Console.WriteLine($"Tentative d'arrêt du service {sc.DisplayName}");
                    if (!sc.CanStop)
                    {
                        Console.WriteLine($"Le service ne peut pas être arrêté. État : {sc.Status}");
                    }
                    else
                    {
                        try
                        {
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
                            Console.WriteLine("Le service est arrêté.");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Impossible d'arrêter le service. {GetReadble(e)}");
                        }
                    }
                }
            }
            catch
            {
                OnError();
            }
        }

        public void Pause()
        {
            try
            {
                using (ServiceController sc = new ServiceController(_currentService, _currentMachine))
                {
                    Console.WriteLine($"Tentative de mise en pause du service {sc.DisplayName}");
                    if (!sc.CanPauseAndContinue || sc.Status != ServiceControllerStatus.Running)
                    {
                        Console.WriteLine($"Le service ne peut pas être mis en pause. État : {sc.Status} Can Pause and Continue = {sc.CanPauseAndContinue}");
                    }
                    else
                    {
                        try
                        {
                            sc.Pause();
                            sc.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromSeconds(60));
                            Console.WriteLine("Le service est en pause.");
                        }
                        catch
                        {
                            Console.WriteLine("Impossible de mettre en pause le service.");
                        }
                    }
                }
            }
            catch
            {
                OnError();
            }
        }

        public void Continue()
        {
            try
            {
                using (ServiceController sc = new ServiceController(_currentService, _currentMachine))
                {
                    Console.WriteLine($"Tentative de reprise du service {sc.DisplayName}");
                    if (!sc.CanPauseAndContinue || sc.Status != ServiceControllerStatus.Paused)
                    {
                        Console.WriteLine($"Le service ne peut pas être repris. État : {sc.Status} Can Pause and Continue = {sc.CanPauseAndContinue}");
                    }
                    else
                    {
                        try
                        {
                            sc.Pause();
                            sc.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromSeconds(60));
                            Console.WriteLine("Le service est en cours d'exécution.");
                        }
                        catch
                        {
                            Console.WriteLine("Impossible de reprendre le service.");
                        }
                    }
                }
            }
            catch
            {
                OnError();
            }
        }

        #endregion

        #region Utils

        private void OnError()
        {
            Console.WriteLine($"Le service <{_currentService}> n'existe pas ou la machine <{_currentMachine}> n'est pas joignable.");
            _currentService = null;
        }

        private string GetReadble(Exception e)
        {
            string errMsg = "";
            while (e != null)
            {
                errMsg += e.Message;
                e = e.InnerException;
            }
            return errMsg;
        }

        #endregion

        #endregion







    }


}
