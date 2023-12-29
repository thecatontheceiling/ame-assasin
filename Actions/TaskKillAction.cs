// Decompiled with JetBrains decompiler
// Type: ame_assassin.TaskKillAction
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using TrustedUninstaller.Shared.Tasks;

#nullable enable
namespace ame_assassin
{
  internal class TaskKillAction : ITaskAction
  {
    private readonly 
    #nullable disable
    string[] RegexNoKill = new string[8]
    {
      "lsass",
      "csrss",
      "winlogon",
      "TrustedUninstaller\\.CLI",
      "dwm",
      "conhost",
      "ame.?wizard",
      "ame.?assassin"
    };
    private readonly string[] RegexNotCritical = new string[4]
    {
      "SecurityHealthService",
      "wscsvc",
      "MsMpEng",
      "SgrmBroker"
    };

    public void RunTaskOnMainThread() => throw new NotImplementedException();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(
      TaskKillAction.ProcessAccessFlags dwDesiredAccess,
      bool bInheritHandle,
      int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    public 
    #nullable enable
    string? ProcessName { get; set; }

    public string? PathContains { get; set; }

    public int ProgressWeight { get; set; } = 2;

    public int GetProgressWeight() => this.ProgressWeight;

    private bool InProgress { get; set; }

    public void ResetProgress() => this.InProgress = false;

    public int? ProcessID { get; set; }

    public 
    #nullable disable
    string ErrorString()
    {
      string str1 = "TaskKillAction failed to kill processes matching '" + this.ProcessName + "'.";
      try
      {
        List<string> list = this.GetProcess().Select<System.Diagnostics.Process, string>((Func<System.Diagnostics.Process, string>) (process => process.ProcessName)).Distinct<string>().ToList<string>();
        if (list.Count > 1)
        {
          str1 = "TaskKillAction failed to kill processes:";
          foreach (string str2 in list)
            str1 = str1 + "|NEWLINE|" + str2;
        }
        else if (list.Count == 1)
          str1 = "TaskKillAction failed to kill process " + list[0] + ".";
      }
      catch (Exception ex)
      {
      }
      return str1;
    }

    public UninstallTaskStatus GetStatus()
    {
      if (this.InProgress)
        return UninstallTaskStatus.InProgress;
      List<System.Diagnostics.Process> source = new List<System.Diagnostics.Process>();
      if (this.ProcessID.HasValue)
      {
        try
        {
          source.Add(System.Diagnostics.Process.GetProcessById(this.ProcessID.Value));
        }
        catch (Exception ex)
        {
        }
      }
      else
        source = this.GetProcess().ToList<System.Diagnostics.Process>();
      return !source.Any<System.Diagnostics.Process>() ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo;
    }

    private List<System.Diagnostics.Process> GetProcess()
    {
      if (this.ProcessID.HasValue)
      {
        List<System.Diagnostics.Process> process = new List<System.Diagnostics.Process>();
        try
        {
          System.Diagnostics.Process processById = System.Diagnostics.Process.GetProcessById(this.ProcessID.Value);
          if (this.ProcessName != null && !processById.ProcessName.Equals(this.ProcessName, StringComparison.OrdinalIgnoreCase))
            return process;
          process.Add(processById);
        }
        catch (Exception ex)
        {
          return process;
        }
      }
      if (this.ProcessName == null)
        return new List<System.Diagnostics.Process>();
      if (this.ProcessName.EndsWith("*") && this.ProcessName.StartsWith("*"))
        return ((IEnumerable<System.Diagnostics.Process>) System.Diagnostics.Process.GetProcesses()).ToList<System.Diagnostics.Process>().Where<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (process => process.ProcessName.IndexOf(this.ProcessName.Trim('*'), StringComparison.CurrentCultureIgnoreCase) >= 0)).ToList<System.Diagnostics.Process>();
      if (this.ProcessName.EndsWith("*"))
        return ((IEnumerable<System.Diagnostics.Process>) System.Diagnostics.Process.GetProcesses()).Where<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (process => process.ProcessName.StartsWith(this.ProcessName.TrimEnd('*'), StringComparison.CurrentCultureIgnoreCase))).ToList<System.Diagnostics.Process>();
      return this.ProcessName.StartsWith("*") ? ((IEnumerable<System.Diagnostics.Process>) System.Diagnostics.Process.GetProcesses()).Where<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (process => process.ProcessName.EndsWith(this.ProcessName.TrimStart('*'), StringComparison.CurrentCultureIgnoreCase))).ToList<System.Diagnostics.Process>() : ((IEnumerable<System.Diagnostics.Process>) System.Diagnostics.Process.GetProcessesByName(this.ProcessName)).ToList<System.Diagnostics.Process>();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool IsProcessCritical(IntPtr hProcess, ref bool Critical);

    public void RunTask()
    {
      this.InProgress = true;
      int? processId;
      if (string.IsNullOrEmpty(this.ProcessName))
      {
        processId = this.ProcessID;
        if (processId.HasValue)
        {
          processId = this.ProcessID;
          Console.WriteLine(string.Format("Killing process with PID '{0}'...", (object) processId.Value));
          goto label_6;
        }
      }
      if (this.ProcessName != null && ((IEnumerable<string>) this.RegexNoKill).Any<string>((Func<string, bool>) (regex => Regex.Match(this.ProcessName, regex, RegexOptions.IgnoreCase).Success)))
      {
        Console.WriteLine("Skipping " + this.ProcessName + "...");
        return;
      }
      Console.WriteLine("Killing processes matching '" + this.ProcessName + "'...");
label_6:
      CmdAction cmdAction = new CmdAction();
      if (this.ProcessName != null)
      {
        if (this.ProcessName.Equals("svchost", StringComparison.OrdinalIgnoreCase))
        {
          try
          {
            processId = this.ProcessID;
            if (processId.HasValue)
            {
              processId = this.ProcessID;
              foreach (string str in Win32.ServiceEx.GetServicesFromProcessId(processId.Value))
              {
                try
                {
                  new ServiceAction()
                  {
                    ServiceName = str,
                    Operation = ServiceOperation.Stop
                  }.RunTask();
                }
                catch (Exception ex)
                {
                  Console.WriteLine("Could not kill service " + str + ": " + ex.Message);
                  Console.WriteLine(ex.Message);
                }
              }
            }
            else
            {
              foreach (System.Diagnostics.Process process in this.GetProcess())
              {
                foreach (string str in Win32.ServiceEx.GetServicesFromProcessId(process.Id))
                {
                  try
                  {
                    new ServiceAction()
                    {
                      ServiceName = str,
                      Operation = ServiceOperation.Stop
                    }.RunTask();
                  }
                  catch (Exception ex)
                  {
                    Console.WriteLine("Could not kill service " + str + ": " + ex.Message);
                    Console.WriteLine(ex.Message);
                  }
                }
              }
            }
          }
          catch (NullReferenceException ex)
          {
            Console.WriteLine(string.Format("A service with PID: {0} could not be found.", (object) this.ProcessID.Value));
            Console.WriteLine(ex.Message);
          }
          int num;
          for (num = 0; num <= 6 && this.GetProcess().Any<System.Diagnostics.Process>(); ++num)
            Thread.Sleep(100 * num);
          if (num < 6)
          {
            this.InProgress = false;
            return;
          }
        }
        if (this.PathContains != null)
        {
          processId = this.ProcessID;
          if (!processId.HasValue)
          {
            List<System.Diagnostics.Process> process1 = this.GetProcess();
            if (process1.Count > 0)
              Console.WriteLine("Processes:");
            foreach (System.Diagnostics.Process process2 in process1.Where<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (x =>
            {
              try
              {
                return x.MainModule.FileName.Contains(this.PathContains);
              }
              catch (Exception ex)
              {
                return false;
              }
            })))
            {
              System.Diagnostics.Process process = process2;
              Console.WriteLine(process.ProcessName + " - " + process.Id.ToString());
              if (!((IEnumerable<string>) this.RegexNotCritical).Any<string>((Func<string, bool>) (x => Regex.Match(process.ProcessName, x, RegexOptions.IgnoreCase).Success)))
              {
                bool Critical = false;
                IntPtr num = TaskKillAction.OpenProcess(TaskKillAction.ProcessAccessFlags.QueryLimitedInformation, false, process.Id);
                TaskKillAction.IsProcessCritical(num, ref Critical);
                TaskKillAction.CloseHandle(num);
                if (Critical)
                {
                  Console.WriteLine(process.ProcessName + " is a critical process, skipping...");
                  continue;
                }
              }
              try
              {
                if (!TaskKillAction.TerminateProcess(process.Handle, 1U))
                  Console.WriteLine("TerminateProcess failed with error code: " + Marshal.GetLastWin32Error().ToString());
              }
              catch (Exception ex)
              {
                Console.WriteLine("Could not open process handle: " + ex.Message);
              }
              try
              {
                process.WaitForExit(1000);
              }
              catch (Exception ex)
              {
                Console.WriteLine("Error waiting for process exit: " + ex.Message);
              }
              if (!(process.ProcessName == "explorer"))
              {
                cmdAction.Command = Program.ProcessHacker + string.Format(" -s -elevate -c -ctype process -cobject {0} -caction terminate", (object) process.Id);
                if (Program.UseKernelDriver && process.ProcessName != "explorer")
                  cmdAction.RunTaskOnMainThread();
                int num;
                for (num = 0; num <= 3; ++num)
                {
                  if (this.GetProcess().Any<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (x => x.Id == process.Id && x.ProcessName == process.ProcessName)))
                  {
                    try
                    {
                      try
                      {
                        if (Program.UseKernelDriver)
                          cmdAction.RunTaskOnMainThread();
                        else
                          TaskKillAction.TerminateProcess(process.Handle, 1U);
                      }
                      catch (Exception ex)
                      {
                      }
                      process.WaitForExit(500);
                    }
                    catch (Exception ex)
                    {
                    }
                    Thread.Sleep(100);
                  }
                  else
                    break;
                }
                if (num >= 3)
                  Console.WriteLine("Task kill timeout exceeded.");
              }
            }
            this.InProgress = false;
            return;
          }
        }
      }
      processId = this.ProcessID;
      if (processId.HasValue)
      {
        processId = this.ProcessID;
        System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(processId.Value);
        if (this.ProcessName != null)
        {
          if (this.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase))
          {
            try
            {
              if (!TaskKillAction.TerminateProcess(process.Handle, 1U))
                Console.WriteLine("TerminateProcess failed with error code: " + Marshal.GetLastWin32Error().ToString());
              try
              {
                process.WaitForExit(1000);
                goto label_97;
              }
              catch (Exception ex)
              {
                Console.WriteLine("Error waiting for process exit: " + ex.Message);
                goto label_97;
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine("Could not open process handle: " + ex.Message);
              goto label_97;
            }
          }
        }
        if (!((IEnumerable<string>) this.RegexNotCritical).Any<string>((Func<string, bool>) (x => Regex.Match(process.ProcessName, x, RegexOptions.IgnoreCase).Success)))
        {
          bool Critical = false;
          try
          {
            IntPtr num = TaskKillAction.OpenProcess(TaskKillAction.ProcessAccessFlags.QueryLimitedInformation, false, process.Id);
            TaskKillAction.IsProcessCritical(num, ref Critical);
            TaskKillAction.CloseHandle(num);
          }
          catch (InvalidOperationException ex)
          {
            Console.WriteLine("Could not check if process is critical.");
            return;
          }
          if (Critical)
          {
            Console.WriteLine(process.ProcessName + " is a critical process, skipping...");
            return;
          }
        }
        try
        {
          if (!TaskKillAction.TerminateProcess(process.Handle, 1U))
            Console.WriteLine("TerminateProcess failed with error code: " + Marshal.GetLastWin32Error().ToString());
        }
        catch (Exception ex)
        {
          Console.WriteLine("Could not open process handle: " + ex.Message);
        }
        try
        {
          process.WaitForExit(1000);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error waiting for process exit: " + ex.Message);
        }
        cmdAction.Command = Program.ProcessHacker + string.Format(" -s -elevate -c -ctype process -cobject {0} -caction terminate", (object) this.ProcessID.Value);
        if (Program.UseKernelDriver)
          cmdAction.RunTaskOnMainThread();
label_97:
        int num1;
        for (num1 = 0; num1 <= 3; ++num1)
        {
          if (this.GetProcess().Any<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (x => x.Id == process.Id && x.ProcessName == process.ProcessName)))
          {
            try
            {
              try
              {
                if (Program.UseKernelDriver)
                  cmdAction.RunTaskOnMainThread();
                else
                  TaskKillAction.TerminateProcess(process.Handle, 1U);
              }
              catch (Exception ex)
              {
              }
              process.WaitForExit(500);
            }
            catch (Exception ex)
            {
            }
            Thread.Sleep(100);
          }
          else
            break;
        }
        if (num1 >= 3)
          Console.WriteLine("Task kill timeout exceeded.");
      }
      else
      {
        List<System.Diagnostics.Process> process3 = this.GetProcess();
        if (process3.Count > 0)
          Console.WriteLine("Processes:");
        foreach (System.Diagnostics.Process process4 in process3)
        {
          System.Diagnostics.Process process = process4;
          Console.WriteLine(process.ProcessName + " - " + process.Id.ToString());
          if (!((IEnumerable<string>) this.RegexNotCritical).Any<string>((Func<string, bool>) (x => Regex.Match(process.ProcessName, x, RegexOptions.IgnoreCase).Success)))
          {
            bool Critical = false;
            try
            {
              IntPtr num = TaskKillAction.OpenProcess(TaskKillAction.ProcessAccessFlags.QueryLimitedInformation, false, process.Id);
              TaskKillAction.IsProcessCritical(num, ref Critical);
              TaskKillAction.CloseHandle(num);
            }
            catch (InvalidOperationException ex)
            {
              Console.WriteLine("Could not check if process is critical.");
              continue;
            }
            if (Critical)
            {
              Console.WriteLine(process.ProcessName + " is a critical process, skipping...");
              continue;
            }
          }
          try
          {
            if (!TaskKillAction.TerminateProcess(process.Handle, 1U))
              Console.WriteLine("TerminateProcess failed with error code: " + Marshal.GetLastWin32Error().ToString());
          }
          catch (Exception ex)
          {
            Console.WriteLine("Could not open process handle: " + ex.Message);
          }
          try
          {
            process.WaitForExit(1000);
          }
          catch (Exception ex)
          {
            Console.WriteLine("Error waiting for process exit: " + ex.Message);
          }
          if (!(process.ProcessName == "explorer"))
          {
            cmdAction.Command = Program.ProcessHacker + string.Format(" -s -elevate -c -ctype process -cobject {0} -caction terminate", (object) process.Id);
            if (Program.UseKernelDriver && process.ProcessName != "explorer")
              cmdAction.RunTaskOnMainThread();
            int num;
            for (num = 0; num <= 3; ++num)
            {
              if (this.GetProcess().Any<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (x => x.Id == process.Id && x.ProcessName == process.ProcessName)))
              {
                try
                {
                  try
                  {
                    if (Program.UseKernelDriver)
                      cmdAction.RunTaskOnMainThread();
                    else
                      TaskKillAction.TerminateProcess(process.Handle, 1U);
                  }
                  catch (Exception ex)
                  {
                  }
                  process.WaitForExit(500);
                }
                catch (Exception ex)
                {
                }
                Thread.Sleep(100);
              }
              else
                break;
            }
            if (num >= 3)
              Console.WriteLine("Task kill timeout exceeded.");
          }
        }
      }
      this.InProgress = false;
    }

    public enum ProcessAccessFlags : uint
    {
      QueryLimitedInformation = 4096, // 0x00001000
    }
  }
}
