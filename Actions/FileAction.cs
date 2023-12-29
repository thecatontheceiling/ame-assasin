// Decompiled with JetBrains decompiler
// Type: ame_assassin.FileAction
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TrustedUninstaller.Shared.Tasks;

#nullable disable
namespace ame_assassin
{
  public class FileAction : ITaskAction
  {
    public void RunTaskOnMainThread() => throw new NotImplementedException();

    public string RawPath { get; set; }

    public bool ExeFirst { get; set; }

    public int ProgressWeight { get; set; } = 2;

    public bool TrustedInstaller { get; set; }

    public int GetProgressWeight() => this.ProgressWeight;

    private bool InProgress { get; set; }

    public void ResetProgress() => this.InProgress = false;

    public string ErrorString()
    {
      return "FileAction failed to remove file or directory '" + Environment.ExpandEnvironmentVariables(this.RawPath) + "'.";
    }

    private string GetRealPath() => Environment.ExpandEnvironmentVariables(this.RawPath);

    private string GetRealPath(string path) => Environment.ExpandEnvironmentVariables(path);

    public UninstallTaskStatus GetStatus()
    {
      if (this.InProgress)
        return UninstallTaskStatus.InProgress;
      string realPath = this.GetRealPath();
      if (realPath.Contains("*"))
      {
        int startIndex = realPath.LastIndexOf("\\");
        string path = realPath.Remove(startIndex).TrimEnd('\\');
        if (path.Contains("*"))
          return UninstallTaskStatus.Completed;
        string searchPattern = realPath.Substring(startIndex + 1);
        return Directory.Exists(path) && (((IEnumerable<string>) Directory.GetFiles(path, searchPattern)).Any<string>() || ((IEnumerable<string>) Directory.GetDirectories(path, searchPattern)).Any<string>()) ? UninstallTaskStatus.ToDo : UninstallTaskStatus.Completed;
      }
      return !(File.Exists(realPath) | Directory.Exists(realPath)) ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo;
    }

    [DllImport("Unlocker.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool EzUnlockFileW(string path);

    private async Task DeleteFile(string file, bool log = false)
    {
      try
      {
        File.Delete(file);
      }
      catch (Exception ex)
      {
      }
      if (!File.Exists(file))
        return;
      try
      {
        FileAction.EzUnlockFileW(file);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error while unlocking file: " + ex.Message);
      }
      try
      {
        await Task.Run((Action) (() => File.Delete(file)));
      }
      catch (Exception ex)
      {
      }
    }

    private async Task RemoveDirectory(string dir, bool log = false)
    {
      try
      {
        Directory.Delete(dir, true);
      }
      catch
      {
      }
    }

    private async Task DeleteItemsInDirectory(string dir, string filter = "*")
    {
      string realPath = this.GetRealPath(dir);
      IEnumerable<string> source1 = Directory.EnumerateFiles(realPath, filter);
      IEnumerable<string> directories = Directory.EnumerateDirectories(realPath, filter);
      if (this.ExeFirst)
        source1 = (IEnumerable<string>) source1.ToList<string>().OrderByDescending<string, bool>((Func<string, bool>) (x => x.EndsWith(".exe")));
      List<string> lockedFilesList = new List<string>()
      {
        "MpOAV.dll",
        "MsMpLics.dll",
        "EppManifest.dll",
        "MpAsDesc.dll",
        "MpClient.dll",
        "MsMpEng.exe"
      };
      string file;
      foreach (string str1 in source1)
      {
        file = str1;
        Console.WriteLine("Deleting " + file + "...");
        GC.Collect();
        GC.WaitForPendingFinalizers();
        await this.DeleteFile(file);
        if (File.Exists(file))
        {
          TaskKillAction taskKillAction1 = new TaskKillAction();
          if (file.EndsWith(".sys"))
          {
            string withoutExtension = Path.GetFileNameWithoutExtension(file);
            try
            {
              CmdAction cmdAction = new CmdAction();
              Console.WriteLine("Removing driver service " + withoutExtension + "...");
              try
              {
                ServiceInstaller serviceInstaller = new ServiceInstaller();
                serviceInstaller.Context = new InstallContext();
                serviceInstaller.ServiceName = withoutExtension;
                serviceInstaller.Uninstall((IDictionary) null);
              }
              catch (Exception ex)
              {
                Console.WriteLine("Service uninstall failed: " + ex.Message);
              }
              cmdAction.Command = Program.ProcessHacker + " -s -elevate -c -ctype service -cobject " + withoutExtension + " -caction stop";
              if (Program.UseKernelDriver)
                cmdAction.RunTaskOnMainThread();
              cmdAction.Command = Program.ProcessHacker + " -s -elevate -c -ctype service -cobject " + withoutExtension + " -caction delete";
              if (Program.UseKernelDriver)
                cmdAction.RunTaskOnMainThread();
            }
            catch (Exception ex)
            {
              Console.WriteLine(ex.Message);
            }
          }
          if (lockedFilesList.Contains(Path.GetFileName(file)))
          {
            TaskKillAction taskKillAction2 = new TaskKillAction();
            taskKillAction2.ProcessName = "MsMpEng";
            taskKillAction2.RunTask();
            taskKillAction2.ProcessName = "NisSrv";
            taskKillAction2.RunTask();
            taskKillAction2.ProcessName = "SecurityHealthService";
            taskKillAction2.RunTask();
            taskKillAction2.ProcessName = "smartscreen";
            taskKillAction2.RunTask();
          }
          List<System.Diagnostics.Process> source2 = new List<System.Diagnostics.Process>();
          try
          {
            source2 = FileLock.WhoIsLocking(file);
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
          }
          int millisecondsTimeout = 0;
          int num = 0;
          foreach (System.Diagnostics.Process process in source2.Where<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (x => x.ProcessName.Equals("svchost"))))
          {
            try
            {
              foreach (string str2 in Win32.ServiceEx.GetServicesFromProcessId(process.Id))
              {
                string serviceName = str2;
                ++num;
                try
                {
                  ServiceController serviceController = ((IEnumerable<ServiceController>) ServiceController.GetServices()).FirstOrDefault<ServiceController>((Func<ServiceController, bool>) (x => x.ServiceName.Equals(serviceName)));
                  if (serviceController != null)
                    num += serviceController.DependentServices.Length;
                }
                catch (Exception ex)
                {
                  Console.WriteLine("\r\nError: Could not get amount of dependent services for " + serviceName + ".\r\nException: " + ex.Message);
                }
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not get amount of services locking file.\r\nException: " + ex.Message);
            }
          }
          for (; source2.Any<System.Diagnostics.Process>() && millisecondsTimeout <= 800; millisecondsTimeout += 100)
          {
            Console.WriteLine("Processes locking the file:");
            foreach (System.Diagnostics.Process process in source2)
              Console.WriteLine(process.ProcessName);
            if (num > 10)
            {
              Console.WriteLine("Amount of locking services exceeds 10, skipping...");
              break;
            }
            foreach (System.Diagnostics.Process process in source2)
            {
              try
              {
                if (process.ProcessName.Equals("TrustedUninstaller.CLI"))
                {
                  Console.WriteLine("Skipping TU.CLI...");
                  continue;
                }
                if (Regex.Match(process.ProcessName, "ame.?wizard", RegexOptions.IgnoreCase).Success)
                {
                  Console.WriteLine("Skipping AME Wizard...");
                  continue;
                }
                taskKillAction1.ProcessName = process.ProcessName;
                taskKillAction1.ProcessID = new int?(process.Id);
                Console.WriteLine(string.Format("Killing locking process {0} with PID {1}...", (object) process.ProcessName, (object) process.Id));
              }
              catch (InvalidOperationException ex)
              {
                continue;
              }
              try
              {
                taskKillAction1.RunTask();
              }
              catch (Exception ex)
              {
                Console.WriteLine(ex.Message);
              }
            }
            Thread.Sleep(millisecondsTimeout);
            try
            {
              source2 = FileLock.WhoIsLocking(file);
            }
            catch (Exception ex)
            {
              Console.WriteLine(ex.Message);
            }
          }
          if (millisecondsTimeout >= 800)
            Console.WriteLine("Could not kill locking processes for file '" + file + "'. Process termination loop exceeded max cycles (8).");
          if (Path.GetExtension(file).Equals(".exe", StringComparison.OrdinalIgnoreCase))
            new TaskKillAction()
            {
              ProcessName = Path.GetFileNameWithoutExtension(file)
            }.RunTask();
          await this.DeleteFile(file, true);
        }
        file = (string) null;
      }
      foreach (string str in directories)
      {
        file = str;
        await this.DeleteItemsInDirectory(file);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        await this.RemoveDirectory(file, true);
        if (Directory.Exists(file))
          Console.WriteLine("Could not remove directory '" + file + "'.");
        file = (string) null;
      }
      directories = (IEnumerable<string>) null;
      lockedFilesList = (List<string>) null;
    }

    public async void RunTask()
    {
      string realPath = this.GetRealPath();
      Console.WriteLine("Removing file or directory '" + realPath + "'...");
      if (realPath.Contains("*"))
      {
        int startIndex = realPath.LastIndexOf("\\");
        string dir = realPath.Remove(startIndex).TrimEnd('\\');
        if (dir.Contains("*"))
          throw new ArgumentException("Parent directories to a given file filter cannot contain wildcards.");
        string filter = realPath.Substring(startIndex + 1);
        await this.DeleteItemsInDirectory(dir, filter);
        this.InProgress = false;
        realPath = (string) null;
      }
      else
      {
        bool isFile = File.Exists(realPath);
        if (Directory.Exists(realPath))
        {
          GC.Collect();
          GC.WaitForPendingFinalizers();
          await this.RemoveDirectory(realPath);
          if (Directory.Exists(realPath))
          {
            CmdAction cmdAction = new CmdAction()
            {
              Command = "takeown /f \"" + realPath + "\" /r /d Y>NUL & icacls \"" + realPath + "\" /t /grant Administrators:F /c > NUL",
              Timeout = new int?(5000)
            };
            try
            {
              cmdAction.RunTaskOnMainThread();
            }
            catch (Exception ex)
            {
              Console.WriteLine(ex.Message);
            }
            try
            {
              if (realPath.Contains("Defender"))
              {
                TaskKillAction taskKillAction = new TaskKillAction();
                taskKillAction.ProcessName = "MsMpEng";
                taskKillAction.RunTask();
                taskKillAction.ProcessName = "NisSrv";
                taskKillAction.RunTask();
                taskKillAction.ProcessName = "SecurityHealthService";
                taskKillAction.RunTask();
                taskKillAction.ProcessName = "smartscreen";
                taskKillAction.RunTask();
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine(ex.Message);
            }
            await this.RemoveDirectory(realPath, true);
            if (Directory.Exists(realPath))
            {
              await this.DeleteItemsInDirectory(realPath);
              GC.Collect();
              GC.WaitForPendingFinalizers();
              await this.RemoveDirectory(realPath, true);
            }
          }
        }
        if (isFile)
        {
          try
          {
            List<string> lockedFilesList = new List<string>()
            {
              "MpOAV.dll",
              "MsMpLics.dll",
              "EppManifest.dll",
              "MpAsDesc.dll",
              "MpClient.dll",
              "MsMpEng.exe"
            };
            string fileName = ((IEnumerable<string>) realPath.Split('\\')).LastOrDefault<string>();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await this.DeleteFile(realPath);
            if (File.Exists(realPath))
            {
              CmdAction cmdAction1 = new CmdAction()
              {
                Command = "takeown /f \"" + realPath + "\" /r /d Y>NUL & icacls \"" + realPath + "\" /t /grant Administrators:F /c > NUL",
                Timeout = new int?(5000)
              };
              try
              {
                cmdAction1.RunTaskOnMainThread();
              }
              catch (Exception ex)
              {
                Console.WriteLine(ex.Message);
              }
              TaskKillAction taskKillAction1 = new TaskKillAction();
              if (realPath.EndsWith(".sys"))
              {
                string withoutExtension = Path.GetFileNameWithoutExtension(realPath);
                try
                {
                  CmdAction cmdAction2 = new CmdAction();
                  Console.WriteLine("Removing driver service " + withoutExtension + "...");
                  try
                  {
                    ServiceInstaller serviceInstaller = new ServiceInstaller();
                    serviceInstaller.Context = new InstallContext();
                    serviceInstaller.ServiceName = withoutExtension;
                    serviceInstaller.Uninstall((IDictionary) null);
                  }
                  catch (Exception ex)
                  {
                    Console.WriteLine("Service uninstall failed: " + ex.Message);
                  }
                  cmdAction2.Command = Program.ProcessHacker + " -s -elevate -c -ctype service -cobject " + withoutExtension + " -caction stop";
                  if (Program.UseKernelDriver)
                    cmdAction2.RunTaskOnMainThread();
                  cmdAction2.Command = Program.ProcessHacker + " -s -elevate -c -ctype service -cobject " + withoutExtension + " -caction delete";
                  if (Program.UseKernelDriver)
                    cmdAction2.RunTaskOnMainThread();
                }
                catch (Exception ex)
                {
                  Console.WriteLine(ex.Message);
                }
              }
              if (lockedFilesList.Contains(fileName))
              {
                TaskKillAction taskKillAction2 = new TaskKillAction();
                taskKillAction2.ProcessName = "MsMpEng";
                taskKillAction2.RunTask();
                taskKillAction2.ProcessName = "NisSrv";
                taskKillAction2.RunTask();
                taskKillAction2.ProcessName = "SecurityHealthService";
                taskKillAction2.RunTask();
                taskKillAction2.ProcessName = "smartscreen";
                taskKillAction2.RunTask();
              }
              List<System.Diagnostics.Process> source = new List<System.Diagnostics.Process>();
              try
              {
                source = FileLock.WhoIsLocking(realPath);
              }
              catch (Exception ex)
              {
                Console.WriteLine(ex.Message);
              }
              int millisecondsTimeout = 0;
              int num = 0;
              foreach (System.Diagnostics.Process process in source.Where<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (x => x.ProcessName.Equals("svchost"))))
              {
                try
                {
                  foreach (string str in Win32.ServiceEx.GetServicesFromProcessId(process.Id))
                  {
                    string serviceName = str;
                    ++num;
                    try
                    {
                      ServiceController serviceController = ((IEnumerable<ServiceController>) ServiceController.GetServices()).FirstOrDefault<ServiceController>((Func<ServiceController, bool>) (x => x.ServiceName.Equals(serviceName)));
                      if (serviceController != null)
                        num += serviceController.DependentServices.Length;
                    }
                    catch (Exception ex)
                    {
                      Console.WriteLine("\r\nError: Could not get amount of dependent services for " + serviceName + ".\r\nException: " + ex.Message);
                    }
                  }
                }
                catch (Exception ex)
                {
                  Console.WriteLine("\r\nError: Could not get amount of services locking file.\r\nException: " + ex.Message);
                }
              }
              if (num > 8)
                Console.WriteLine("Amount of locking services exceeds 8, skipping...");
              for (; source.Any<System.Diagnostics.Process>() && millisecondsTimeout <= 800 && num <= 8; millisecondsTimeout += 100)
              {
                Console.WriteLine("Processes locking the file:");
                foreach (System.Diagnostics.Process process in source)
                  Console.WriteLine(process.ProcessName);
                foreach (System.Diagnostics.Process process in source)
                {
                  try
                  {
                    if (process.ProcessName.Equals("TrustedUninstaller.CLI"))
                    {
                      Console.WriteLine("Skipping TU.CLI...");
                      continue;
                    }
                    if (Regex.Match(process.ProcessName, "ame.?wizard", RegexOptions.IgnoreCase).Success)
                    {
                      Console.WriteLine("Skipping AME Wizard...");
                      continue;
                    }
                    taskKillAction1.ProcessName = process.ProcessName;
                    taskKillAction1.ProcessID = new int?(process.Id);
                    Console.WriteLine(string.Format("Killing {0} with PID {1}... it is locking {2}", (object) process.ProcessName, (object) process.Id, (object) realPath));
                  }
                  catch (InvalidOperationException ex)
                  {
                    continue;
                  }
                  try
                  {
                    taskKillAction1.RunTask();
                  }
                  catch (Exception ex)
                  {
                    Console.WriteLine(ex.Message);
                  }
                }
                Thread.Sleep(millisecondsTimeout);
                try
                {
                  source = FileLock.WhoIsLocking(realPath);
                }
                catch (Exception ex)
                {
                  Console.WriteLine(ex.Message);
                }
              }
              if (millisecondsTimeout >= 800)
                Console.WriteLine("Could not kill locking processes for file '" + realPath + "'. Process termination loop exceeded max cycles (8).");
              if (Path.GetExtension(realPath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                new TaskKillAction()
                {
                  ProcessName = Path.GetFileNameWithoutExtension(realPath)
                }.RunTask();
              await this.DeleteFile(realPath, true);
            }
            lockedFilesList = (List<string>) null;
            fileName = (string) null;
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
          }
        }
        else
          Console.WriteLine("File or directory '" + realPath + "' not found.");
        this.InProgress = false;
        realPath = (string) null;
      }
    }
  }
}
