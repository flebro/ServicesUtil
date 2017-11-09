// Decompiled with JetBrains decompiler
// Type: ServiceManager.Client.Program
// Assembly: ServiceManager.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 831F4F52-5CCA-44F4-AF96-E5AC5BB97E74
// Assembly location: C:\Users\flebro\source\repos\ServicesUtil\Docs\ServiceManager.Client.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;

namespace ServiceManager.Client
{
  internal class Program
  {
    private const string COMMAND_LIST = "list";
    private const string COMMAND_LIST_RUNNING = "/running";
    private const string COMMAND_LIST_STOPPED = "/stopped";
    private const string COMMAND_LIST_PAUSED = "/paused";
    private const string COMMAND_LIST_STATUS = "/status";
    private const string COMMAND_SELECT = "select";
    private const string COMMAND_DETAILS = "details";
    private const string COMMAND_START = "start";
    private const string COMMAND_STOP = "stop";
    private const string COMMAND_PAUSE = "pause";
    private const string COMMAND_CONTINUE = "continue";
    private const string COMMAND_MACHINE = "machine";
    private const string COMMAND_EXIT = "exit";
    private static string _MachineName;
    private static string _SelectedService;

    private static IEnumerable<string> AvailableCommands
    {
      get
      {
        return (IEnumerable<string>) new string[9]
        {
          "list",
          "select",
          "details",
          "start",
          "stop",
          "pause",
          "continue",
          "machine",
          "exit"
        };
      }
    }

    private static void Main(string[] args)
    {
      Program.SelectMachineName("machine");
      bool flag = false;
      do
      {
        string prompt = Program.PromptMenu();
        Console.Clear();
        Console.WriteLine(prompt);
        Console.WriteLine();
        string str1 = prompt;
        if (str1 != null)
        {
          string str2 = str1;
          if (str2.StartsWith("list"))
            Program.ListService(prompt);
          else if (str2.StartsWith("machine"))
            Program.SelectMachineName(prompt);
          else if (str2.StartsWith("select"))
            Program.SelectService(prompt);
          else if (str2.StartsWith("details") && !string.IsNullOrWhiteSpace(Program._SelectedService))
            Program.ShowDetails();
          else if (str2.StartsWith("start") && !string.IsNullOrWhiteSpace(Program._SelectedService))
            Program.Start(prompt);
          else if (str2.StartsWith("stop") && !string.IsNullOrWhiteSpace(Program._SelectedService))
            Program.Stop();
          else if (str2.StartsWith("pause") && !string.IsNullOrWhiteSpace(Program._SelectedService))
            Program.Pause();
          else if (!str2.StartsWith("continue") || string.IsNullOrWhiteSpace(Program._SelectedService))
          {
            if (str2 != null && str2 == "exit")
              flag = true;
          }
          else
            Program.Continue();
        }
        Console.WriteLine("Appuyez sur une touche pour continuer...");
        Console.ReadKey();
      }
      while (!flag);
    }

    private static string PromptMenu()
    {
      string prompt = (string) null;
      do
      {
        Console.Clear();
        Console.WriteLine("Machine sélectionnée : " + Program._MachineName);
        if (!string.IsNullOrWhiteSpace(Program._SelectedService))
          Console.WriteLine("Service sélectionné : " + Program._SelectedService);
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
        prompt = Console.ReadLine().ToLower().Trim();
      }
      while (!Program.AvailableCommands.Any<string>((Func<string, bool>) (c => prompt.StartsWith(c))));
      return prompt;
    }

    private static void SelectMachineName(string prompt)
    {
      if (string.IsNullOrWhiteSpace(prompt.Substring("machine".Length).TrimStart((char[]) Array.Empty<char>())))
      {
        Console.WriteLine("Entrer le nom ou l'adresse IP de la machine cible ou vide pour cette machine :");
        Program._MachineName = Console.ReadLine();
        Program._MachineName = string.IsNullOrWhiteSpace(Program._MachineName) ? "localhost" : Program._MachineName;
      }
      else
        Program._MachineName = prompt.Substring("machine".Length).TrimStart((char[]) Array.Empty<char>());
    }

    private static void ListService(string prompt)
    {
      ServiceController[] services = ServiceController.GetServices(string.IsNullOrWhiteSpace(Program._MachineName) ? "localhost" : Program._MachineName);
      bool runningFilter = prompt.Contains("/running");
      bool stoppedFilter = prompt.Contains("/stopped");
      bool pausedFilter = prompt.Contains("/paused");
      string search = prompt.Substring("list".Length).Replace("/status", "").Replace("/running", "").Replace("/stopped", "").Replace("/paused", "").Trim();
      if (!runningFilter && !stoppedFilter && !pausedFilter)
      {
        runningFilter = true;
        stoppedFilter = true;
        pausedFilter = true;
      }
      foreach (ServiceController serviceController in ((IEnumerable<ServiceController>) services).OrderBy<ServiceController, string>((Func<ServiceController, string>) (sc => sc.DisplayName)).Where<ServiceController>((Func<ServiceController, bool>) (sc =>
      {
        if (!string.IsNullOrWhiteSpace(search) && !sc.DisplayName.ToLower().Contains(search) && !sc.ServiceName.ToLower().Contains(search))
          return false;
        if (runningFilter && sc.Status == ServiceControllerStatus.Running || stoppedFilter && sc.Status == ServiceControllerStatus.Stopped)
          return true;
        if (pausedFilter)
          return sc.Status == ServiceControllerStatus.Paused;
        return false;
      })))
      {
        Console.WriteLine(string.Format("[{0}]\t{1}", (object) serviceController.ServiceName, (object) serviceController.DisplayName));
        if (prompt.Contains("/status"))
          Console.WriteLine("\t" + (object) serviceController.Status);
        Console.WriteLine();
      }
      foreach (Component component in services)
        component.Dispose();
    }

    private static void SelectService(string prompt)
    {
      Program._SelectedService = prompt.Substring("select".Length).TrimStart((char[]) Array.Empty<char>());
      Program.ShowDetails();
    }

    private static void ShowDetails()
    {
      try
      {
        using (ServiceController serviceController = new ServiceController(Program._SelectedService, Program._MachineName))
        {
          Console.WriteLine("Machine = " + serviceController.MachineName);
          Console.WriteLine("Name = " + serviceController.ServiceName);
          Console.WriteLine("Display name = " + serviceController.DisplayName);
          Console.WriteLine("Status = " + (object) serviceController.Status);
          string str1 = "Can Pause and Continue = ";
          bool flag = serviceController.CanPauseAndContinue;
          string str2 = flag.ToString();
          Console.WriteLine(str1 + str2);
          string str3 = "Can ShutDown = ";
          flag = serviceController.CanShutdown;
          string str4 = flag.ToString();
          Console.WriteLine(str3 + str4);
          string str5 = "Can Stop = ";
          flag = serviceController.CanStop;
          string str6 = flag.ToString();
          Console.WriteLine(str5 + str6);
          Console.WriteLine("Service type = " + (object) serviceController.ServiceType);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(string.Format("Le service <{0}> n'existe pas ou la machine <{1}> n'est pas joignable.", (object) Program._SelectedService, (object) Program._MachineName));
        Program._SelectedService = (string) null;
      }
    }

    private static void Start(string prompt)
    {
      string str = prompt.Substring("start".Length);
      string[] args = (string[]) null;
      if (!string.IsNullOrWhiteSpace(str))
        args = str.Split(' ');
      using (ServiceController serviceController = new ServiceController(Program._SelectedService, Program._MachineName))
      {
        try
        {
          Console.WriteLine("Tentative de démarrage du service " + serviceController.DisplayName);
        }
        catch (Exception ex)
        {
          Console.WriteLine(string.Format("Le service <{0}> n'existe pas ou la machine <{1}> n'est pas joignable.", (object) Program._SelectedService, (object) Program._MachineName));
          Program._SelectedService = (string) null;
        }
        try
        {
          if (serviceController.Status == ServiceControllerStatus.Stopped)
          {
            if (args != null)
            {
              Console.WriteLine("Arguments : " + str);
              serviceController.Start(args);
            }
            else
              serviceController.Start();
            do
            {
              serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5.0));
            }
            while (serviceController.Status == ServiceControllerStatus.StartPending);
            if (serviceController.Status != ServiceControllerStatus.Running)
              Console.WriteLine("Impossible de démarrer le service.");
            else
              Console.WriteLine("Le service est en cours d'exécution.");
          }
          else
            Console.WriteLine("Le service n'est pas à l'arrêt. État : " + (object) serviceController.Status);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Erreur : " + ex.Message);
        }
      }
    }

    private static void Stop()
    {
      using (ServiceController serviceController = new ServiceController(Program._SelectedService, Program._MachineName))
      {
        try
        {
          Console.WriteLine("Tentative d'arrêt du service " + serviceController.DisplayName);
        }
        catch (Exception ex)
        {
          Console.WriteLine(string.Format("Le service <{0}> n'existe pas ou la machine <{1}> n'est pas joignable.", (object) Program._SelectedService, (object) Program._MachineName));
          Program._SelectedService = (string) null;
        }
        try
        {
          if (serviceController.CanStop)
          {
            serviceController.Stop();
            do
            {
              serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5.0));
            }
            while (serviceController.Status == ServiceControllerStatus.StopPending);
            if (serviceController.Status != ServiceControllerStatus.Stopped)
              Console.WriteLine("Impossible d'arrêter le service.");
            else
              Console.WriteLine("Le service est arrêté.");
          }
          else
            Console.WriteLine("Le service ne peut pas être arrêté. État : " + (object) serviceController.Status);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Erreur : " + ex.Message);
        }
      }
    }

    private static void Pause()
    {
      using (ServiceController serviceController = new ServiceController(Program._SelectedService, Program._MachineName))
      {
        try
        {
          Console.WriteLine("Tentative de mise en pause du service " + serviceController.DisplayName);
        }
        catch (Exception ex)
        {
          Console.WriteLine(string.Format("Le service <{0}> n'existe pas ou la machine <{1}> n'est pas joignable.", (object) Program._SelectedService, (object) Program._MachineName));
          Program._SelectedService = (string) null;
        }
        try
        {
          if (serviceController.CanPauseAndContinue && serviceController.Status == ServiceControllerStatus.Running)
          {
            serviceController.Pause();
            do
            {
              serviceController.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromSeconds(5.0));
            }
            while (serviceController.Status == ServiceControllerStatus.PausePending);
            if (serviceController.Status != ServiceControllerStatus.Stopped)
              Console.WriteLine("Impossible de mettre en pause le service.");
            else
              Console.WriteLine("Le service est en pause.");
          }
          else
            Console.WriteLine("Le service ne peut pas être mis en pause. État : " + (object) serviceController.Status);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Erreur : " + ex.Message);
        }
      }
    }

    private static void Continue()
    {
      using (ServiceController serviceController = new ServiceController(Program._SelectedService, Program._MachineName))
      {
        try
        {
          Console.WriteLine("Tentative de reprise du service " + serviceController.DisplayName);
        }
        catch (Exception ex)
        {
          Console.WriteLine(string.Format("Le service <{0}> n'existe pas ou la machine <{1}> n'est pas joignable.", (object) Program._SelectedService, (object) Program._MachineName));
          Program._SelectedService = (string) null;
        }
        try
        {
          if (serviceController.CanPauseAndContinue && serviceController.Status == ServiceControllerStatus.Running)
          {
            serviceController.Continue();
            do
            {
              serviceController.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromSeconds(5.0));
            }
            while (serviceController.Status == ServiceControllerStatus.PausePending);
            if (serviceController.Status != ServiceControllerStatus.Stopped)
              Console.WriteLine("Impossible de reprendre le service.");
            else
              Console.WriteLine("Le service est en cours d'exécution.");
          }
          else
            Console.WriteLine("Le service ne peut pas être repris. État : " + (object) serviceController.Status);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Erreur : " + ex.Message);
        }
      }
    }
  }
}
