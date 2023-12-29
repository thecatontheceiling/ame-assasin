// Decompiled with JetBrains decompiler
// Type: ame_assassin.CmdAction
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TrustedUninstaller.Shared.Tasks;

#nullable enable
namespace ame_assassin
{
  public class CmdAction : ITaskAction
  {
    public void RunTaskOnMainThread()
    {
      this.ExitCode = new int?();
      this.RunAsProcess();
    }

    public 
    #nullable disable
    string Command { get; set; }

    public int? Timeout { get; set; }

    public bool Wait { get; set; } = true;

    public bool ExeDir { get; set; }

    public int ProgressWeight { get; set; } = 1;

    public int GetProgressWeight() => this.ProgressWeight;

    private bool InProgress { get; set; }

    public void ResetProgress() => this.InProgress = false;

    private int? ExitCode { get; set; }

    public 
    #nullable enable
    string? StandardError { get; set; }

    public 
    #nullable disable
    string StandardOutput { get; set; }

    public string ErrorString() => "CmdAction failed to run command '" + this.Command + "'.";

    public UninstallTaskStatus GetStatus()
    {
      if (this.InProgress)
        return UninstallTaskStatus.InProgress;
      return this.ExitCode.HasValue ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo;
    }

    public void RunTask()
    {
    }

    private void RunAsProcess()
    {
      System.Diagnostics.Process process1 = new System.Diagnostics.Process();
      ProcessStartInfo processStartInfo = new ProcessStartInfo()
      {
        WindowStyle = ProcessWindowStyle.Normal,
        FileName = "cmd.exe",
        Arguments = "/C \"" + this.Command + "\"",
        UseShellExecute = false,
        RedirectStandardError = true,
        RedirectStandardOutput = true,
        CreateNoWindow = true
      };
      if (this.ExeDir)
        processStartInfo.WorkingDirectory = Directory.GetCurrentDirectory() + "\\Executables";
      if (!this.Wait)
      {
        processStartInfo.RedirectStandardError = false;
        processStartInfo.RedirectStandardOutput = false;
        processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        processStartInfo.UseShellExecute = true;
      }
      process1.StartInfo = processStartInfo;
      process1.Start();
      if (!this.Wait)
      {
        process1.Dispose();
      }
      else
      {
        StringBuilder error = new StringBuilder();
        process1.OutputDataReceived += new DataReceivedEventHandler(this.ProcOutputHandler);
        process1.ErrorDataReceived += (DataReceivedEventHandler) ((sender, args) =>
        {
          if (!string.IsNullOrEmpty(args.Data))
            error.AppendLine(args.Data);
          else
            error.AppendLine();
        });
        process1.BeginOutputReadLine();
        process1.BeginErrorReadLine();
        int? timeout = this.Timeout;
        if (timeout.HasValue)
        {
          System.Diagnostics.Process process2 = process1;
          timeout = this.Timeout;
          int milliseconds = timeout.Value;
          if (!process2.WaitForExit(milliseconds))
          {
            process1.Kill();
            throw new TimeoutException("Command '" + this.Command + "' timeout exceeded.");
          }
        }
        else
        {
          bool flag = process1.WaitForExit(30000);
          while (!flag && CmdAction.CmdRunning(process1.Id))
            flag = process1.WaitForExit(30000);
        }
        process1.CancelOutputRead();
        process1.CancelErrorRead();
        process1.Dispose();
      }
    }

    private static bool CmdRunning(int id)
    {
      try
      {
        return ((IEnumerable<System.Diagnostics.Process>) System.Diagnostics.Process.GetProcessesByName("cmd")).Any<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (x => x.Id == id));
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    private void ProcOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
      string str = outLine.Data;
      if (!string.IsNullOrEmpty(outLine.Data))
      {
        if (str.Contains("\\AME"))
          str = str.Substring(str.IndexOf('>') + 1);
        Console.WriteLine(str);
      }
      else
        Console.WriteLine();
    }
  }
}
