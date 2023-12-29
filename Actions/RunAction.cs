// Decompiled with JetBrains decompiler
// Type: ame_assassin.RunAction
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TrustedUninstaller.Shared.Tasks;

#nullable enable
namespace ame_assassin
{
  public class RunAction : ITaskAction
  {
    public void RunTaskOnMainThread() => this.RunAsProcess(this.Exe);

    public 
    #nullable disable
    string Exe { get; set; }

    public 
    #nullable enable
    string? Arguments { get; set; }

    public bool CreateWindow { get; set; }

    public int? Timeout { get; set; }

    public bool Wait { get; set; } = true;

    public int ProgressWeight { get; set; } = 5;

    public int GetProgressWeight() => this.ProgressWeight;

    private bool InProgress { get; set; }

    public void ResetProgress() => this.InProgress = false;

    private bool HasExited { get; set; }

    public string? Output { get; private set; }

    private string? StandardError { get; set; }

    public 
    #nullable disable
    string ErrorString()
    {
      if (string.IsNullOrEmpty(this.Arguments))
        return "RunAction failed to execute '" + this.Exe + "'.";
      return "RunAction failed to execute '" + this.Exe + "' with arguments '" + this.Arguments + "'.";
    }

    public static bool ExistsInPath(string fileName)
    {
      if (File.Exists(fileName))
        return true;
      string environmentVariable = Environment.GetEnvironmentVariable("PATH");
      char[] chArray = new char[1]{ Path.PathSeparator };
      foreach (string path1 in environmentVariable.Split(chArray))
      {
        string path = Path.Combine(path1, fileName);
        if (File.Exists(path) || !fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && File.Exists(path + ".exe"))
          return true;
      }
      return false;
    }

    public UninstallTaskStatus GetStatus()
    {
      if (this.InProgress)
        return UninstallTaskStatus.InProgress;
      return !this.HasExited && this.Wait ? UninstallTaskStatus.ToDo : UninstallTaskStatus.Completed;
    }

    public void RunTask() => throw new NotImplementedException();

    private void RunAsProcess(string file)
    {
      ProcessStartInfo processStartInfo = new ProcessStartInfo()
      {
        CreateNoWindow = !this.CreateWindow,
        UseShellExecute = false,
        WindowStyle = ProcessWindowStyle.Normal,
        RedirectStandardError = false,
        RedirectStandardOutput = false,
        FileName = file
      };
      if (this.Arguments != null)
        processStartInfo.Arguments = Environment.ExpandEnvironmentVariables(this.Arguments);
      if (!this.Wait)
      {
        processStartInfo.RedirectStandardError = false;
        processStartInfo.RedirectStandardOutput = false;
        processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        processStartInfo.UseShellExecute = true;
      }
      System.Diagnostics.Process process1 = new System.Diagnostics.Process()
      {
        StartInfo = processStartInfo,
        EnableRaisingEvents = true
      };
      process1.Start();
      if (!this.Wait)
      {
        process1.Dispose();
      }
      else
      {
        int? timeout = this.Timeout;
        if (timeout.HasValue)
        {
          System.Diagnostics.Process process2 = process1;
          timeout = this.Timeout;
          int milliseconds = timeout.Value;
          if (!process2.WaitForExit(milliseconds))
          {
            process1.Kill();
            throw new TimeoutException("Executable run timeout exceeded.");
          }
        }
        else
        {
          bool flag = process1.WaitForExit(30000);
          while (!flag && RunAction.ExeRunning(process1.ProcessName, process1.Id))
            flag = process1.WaitForExit(30000);
        }
        this.HasExited = true;
        process1.Dispose();
      }
    }

    private static bool ExeRunning(string name, int id)
    {
      try
      {
        return ((IEnumerable<System.Diagnostics.Process>) System.Diagnostics.Process.GetProcessesByName(name)).Any<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (x => x.Id == id));
      }
      catch (Exception ex)
      {
        return false;
      }
    }
  }
}
