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

        private const string COMMAND_LIST       = "list";
        private const string COMMAND_SELECT     = "select";
        private const string COMMAND_DETAILS    = "details";
        private const string COMMAND_STOP       = "stop";
        private const string COMMAND_START      = "start";
        private const string COMMAND_CONTINUE   = "continue";
        private const string COMMAND_MACHINE    = "machine";
        private const string COMMAND_PAUSE      = "pause";
        private const string COMMAND_EXIT       = "exit";

        #endregion

        #region Arguments

        private const string ARG_STATUS = "/status";
        private const string ARG_RUNNING = "/running";
        private const string ARG_PAUSED = "/paused";
        private const string ARG_STOPPED = "/stopped";

        #endregion

        private const string LOCALHOST = "localhost";

        #region Attributes

        private string _currentMachine;
        private ServiceController _currentService;

        #endregion

        #endregion

        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.Start();
        }

        public void Start()
        {
            // Boucle d'éxécution
            bool exited = false;
            do
            {
                // Init
                _currentMachine = LOCALHOST;
                ShowMenu();

                // On lit la commande
                string command = Console.ReadLine().ToLower().Trim();

                // On traite
                if (command.StartsWith(COMMAND_LIST)) ShowMenu();
                else if (command.StartsWith(COMMAND_SELECT)) ShowMenu();
                else if (command.StartsWith(COMMAND_DETAILS)) ShowMenu();
                else if (command.StartsWith(COMMAND_STOP)) ShowMenu();
                else if (command.StartsWith(COMMAND_START)) ShowMenu();
                else if (command.StartsWith(COMMAND_CONTINUE)) ShowMenu();
                else if (command.StartsWith(COMMAND_MACHINE)) ShowMenu();
                else if (command.StartsWith(COMMAND_PAUSE)) ShowMenu();
                else if (command.StartsWith(COMMAND_EXIT)) exited = true;

            } while (!exited);

        }

        public void ShowMenu()
        {
            Console.WriteLine($"Machine sélectionnée : {_currentMachine}");
            if (_currentService != null)
            {
                Console.WriteLine($"Service sélectionné : {}" + Program._SelectedService);
            }
                
            Console.WriteLine();
            Console.WriteLine("Lister les services :");
            Console.WriteLine("\tlist <Recherche>");
            Console.WriteLine();
            Console.WriteLine("    Arguments :");
            Console.WriteLine("\t/running :\tlister les services démarrés");
            Console.WriteLine("\t/stopped :\tlister les services arrêtés");
            Console.WriteLine("\t/paused :\tlister les services en pause");
            Console.WriteLine("\t/status :\tafficher le status des services");
            if (!string.IsNullOrWhiteSpace(Program._SelectedService))
            {
                Console.WriteLine();
                Console.WriteLine("Afficher les détails du service sélectionné :");
                Console.WriteLine("\tdetails");
                Console.WriteLine();
                Console.WriteLine("Gérer le service :");
                Console.WriteLine("\tstart");
                Console.WriteLine("\tstop");
                Console.WriteLine("\tpause");
                Console.WriteLine("\tcontinue");
            }
            Console.WriteLine();
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

    }

    
}
