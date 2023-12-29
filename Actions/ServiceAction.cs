// Decompiled with JetBrains decompiler
// Type: ame_assassin.ServiceAction
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Threading;
using TrustedUninstaller.Shared.Tasks;

#nullable enable
namespace ame_assassin
{
  public class ServiceAction : ITaskAction
  {
    private readonly string[] RegexNoKill = new string[1]
    {
      "DcomLaunch"
    };

    public void RunTaskOnMainThread() => throw new NotImplementedException();

    public ServiceOperation Operation { get; set; } = ServiceOperation.Delete;

    public string ServiceName { get; set; }

    public int? Startup { get; set; }

    public bool DeleteStop { get; set; } = true;

    public bool RegistryDelete { get; set; }

    public bool Device { get; set; }

    public int ProgressWeight { get; set; } = 4;

    public int GetProgressWeight() => this.ProgressWeight;

    private bool InProgress { get; set; }

    public void ResetProgress() => this.InProgress = false;

    public string ErrorString()
    {
      return "ServiceAction failed to " + this.Operation.ToString().ToLower() + " service " + this.ServiceName + ".";
    }

    private ServiceController? GetService()
    {
      if (this.ServiceName.EndsWith("*") && this.ServiceName.StartsWith("*"))
        return ((IEnumerable<ServiceController>) ServiceController.GetServices()).FirstOrDefault<ServiceController>((Func<ServiceController, bool>) (service => service.ServiceName.IndexOf(this.ServiceName.Trim('*'), StringComparison.CurrentCultureIgnoreCase) >= 0));
      if (this.ServiceName.EndsWith("*"))
        return ((IEnumerable<ServiceController>) ServiceController.GetServices()).FirstOrDefault<ServiceController>((Func<ServiceController, bool>) (service => service.ServiceName.StartsWith(this.ServiceName.TrimEnd('*'), StringComparison.CurrentCultureIgnoreCase)));
      return this.ServiceName.StartsWith("*") ? ((IEnumerable<ServiceController>) ServiceController.GetServices()).FirstOrDefault<ServiceController>((Func<ServiceController, bool>) (service => service.ServiceName.EndsWith(this.ServiceName.TrimStart('*'), StringComparison.CurrentCultureIgnoreCase))) : ((IEnumerable<ServiceController>) ServiceController.GetServices()).FirstOrDefault<ServiceController>((Func<ServiceController, bool>) (service => service.ServiceName.Equals(this.ServiceName, StringComparison.CurrentCultureIgnoreCase)));
    }

    private ServiceController? GetDevice()
    {
      if (this.ServiceName.EndsWith("*") && this.ServiceName.StartsWith("*"))
        return ((IEnumerable<ServiceController>) ServiceController.GetDevices()).FirstOrDefault<ServiceController>((Func<ServiceController, bool>) (service => service.ServiceName.IndexOf(this.ServiceName.Trim('*'), StringComparison.CurrentCultureIgnoreCase) >= 0));
      if (this.ServiceName.EndsWith("*"))
        return ((IEnumerable<ServiceController>) ServiceController.GetDevices()).FirstOrDefault<ServiceController>((Func<ServiceController, bool>) (service => service.ServiceName.StartsWith(this.ServiceName.TrimEnd('*'), StringComparison.CurrentCultureIgnoreCase)));
      return this.ServiceName.StartsWith("*") ? ((IEnumerable<ServiceController>) ServiceController.GetDevices()).FirstOrDefault<ServiceController>((Func<ServiceController, bool>) (service => service.ServiceName.EndsWith(this.ServiceName.TrimStart('*'), StringComparison.CurrentCultureIgnoreCase))) : ((IEnumerable<ServiceController>) ServiceController.GetDevices()).FirstOrDefault<ServiceController>((Func<ServiceController, bool>) (service => service.ServiceName.Equals(this.ServiceName, StringComparison.CurrentCultureIgnoreCase)));
    }

    public UninstallTaskStatus GetStatus()
    {
      if (this.InProgress)
        return UninstallTaskStatus.InProgress;
      if (this.Operation == ServiceOperation.Change && this.Startup.HasValue)
      {
        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\" + this.ServiceName);
        return registryKey == null || (int) registryKey.GetValue("Start") == this.Startup.Value ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo;
      }
      ServiceController serviceController = !this.Device ? this.GetService() : this.GetDevice();
      if (this.Operation == ServiceOperation.Delete && this.RegistryDelete)
        return Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\" + this.ServiceName) != null ? UninstallTaskStatus.ToDo : UninstallTaskStatus.Completed;
      switch (this.Operation)
      {
        case ServiceOperation.Stop:
          return serviceController == null || serviceController != null && serviceController.Status == ServiceControllerStatus.Stopped || serviceController != null && serviceController.Status == ServiceControllerStatus.StopPending ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo;
        case ServiceOperation.Continue:
          return serviceController == null || serviceController != null && serviceController.Status == ServiceControllerStatus.Running || serviceController != null && serviceController.Status == ServiceControllerStatus.ContinuePending ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo;
        case ServiceOperation.Start:
          return serviceController != null && serviceController.Status == ServiceControllerStatus.StartPending || serviceController != null && serviceController.Status == ServiceControllerStatus.Running ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo;
        case ServiceOperation.Pause:
          return serviceController == null || serviceController != null && serviceController.Status == ServiceControllerStatus.Paused || serviceController != null && serviceController.Status == ServiceControllerStatus.PausePending ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo;
        case ServiceOperation.Delete:
          return serviceController == null || ame_assassin.Win32.ServiceEx.IsPendingDeleteOrDeleted(serviceController.ServiceName) ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo;
        default:
          throw new ArgumentOutOfRangeException("Argument out of Range", (Exception) new ArgumentOutOfRangeException());
      }
    }

    public void RunTask()
    {
      int? nullable;
      if (this.Operation == ServiceOperation.Change)
      {
        nullable = this.Startup;
        if (!nullable.HasValue)
          throw new ArgumentException("Startup property must be specified with the change operation.");
      }
      if (this.Operation == ServiceOperation.Change)
      {
        nullable = this.Startup;
        nullable = nullable.Value <= 4 ? this.Startup : throw new ArgumentException("Startup property must be between 1 and 4.");
        if (nullable.Value >= 0)
          goto label_7;
      }
label_7:
      Console.WriteLine(this.Operation.ToString().Replace("Stop", "Stopp").TrimEnd('e') + "ing services matching '" + this.ServiceName + "'...");
      if (this.Operation == ServiceOperation.Change)
      {
        RegistryValueAction registryValueAction = new RegistryValueAction();
        registryValueAction.KeyName = "HKLM\\SYSTEM\\CurrentControlSet\\Services\\" + this.ServiceName;
        registryValueAction.Value = "Start";
        nullable = this.Startup;
        registryValueAction.Data = (object) nullable.Value;
        registryValueAction.Type = RegistryValueType.REG_DWORD;
        registryValueAction.Operation = RegistryValueOperation.Set;
        registryValueAction.RunTask();
        this.InProgress = false;
      }
      else
      {
        ServiceController serviceController1 = !this.Device ? this.GetService() : this.GetDevice();
        if (serviceController1 == null)
        {
          Console.WriteLine("No services found matching '" + this.ServiceName + "'.");
          if (this.Operation == ServiceOperation.Start)
            throw new ArgumentException("Service " + this.ServiceName + " not found.");
        }
        else
        {
          this.InProgress = true;
          CmdAction cmdAction = new CmdAction();
          if (this.Operation == ServiceOperation.Delete && this.DeleteStop || this.Operation == ServiceOperation.Stop)
          {
            if (((IEnumerable<string>) this.RegexNoKill).Any<string>((Func<string, bool>) (regex => Regex.Match(this.ServiceName, regex, RegexOptions.IgnoreCase).Success)))
              Console.WriteLine("Skipping " + this.ServiceName + "...");
            try
            {
              foreach (ServiceController serviceController2 in ((IEnumerable<ServiceController>) serviceController1.DependentServices).Where<ServiceController>((Func<ServiceController, bool>) (x => x.Status != ServiceControllerStatus.Stopped)))
              {
                Console.WriteLine("Killing dependent service " + serviceController2.ServiceName + "...");
                if (serviceController2.Status != ServiceControllerStatus.StopPending)
                {
                  if (serviceController2.Status != ServiceControllerStatus.Stopped)
                  {
                    try
                    {
                      serviceController2.Stop();
                    }
                    catch (Exception ex)
                    {
                      serviceController2.Refresh();
                      if (serviceController2.Status != ServiceControllerStatus.Stopped)
                      {
                        if (serviceController2.Status != ServiceControllerStatus.StopPending)
                          Console.WriteLine("Dependent service stop failed: " + ex.Message);
                      }
                    }
                    cmdAction.Command = Program.ProcessHacker + " -s -elevate -c -ctype service -cobject " + serviceController2.ServiceName + " -caction stop";
                    if (Program.UseKernelDriver)
                      cmdAction.RunTaskOnMainThread();
                  }
                }
                Console.WriteLine("Waiting for the dependent service to stop...");
                try
                {
                  serviceController2.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(5000.0));
                }
                catch (Exception ex)
                {
                  serviceController2.Refresh();
                  if (serviceController1.Status != ServiceControllerStatus.Stopped)
                    Console.WriteLine("Dependent service stop timeout exceeded.");
                }
                try
                {
                  new TaskKillAction()
                  {
                    ProcessID = new int?(ame_assassin.Win32.ServiceEx.GetServiceProcessId(serviceController2.ServiceName))
                  }.RunTask();
                }
                catch (Exception ex)
                {
                  serviceController2.Refresh();
                  if (serviceController2.Status != ServiceControllerStatus.Stopped)
                    Console.WriteLine("Could not kill dependent service " + serviceController2.ServiceName + ".");
                }
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine("Error killing dependent services: " + ex.Message);
            }
          }
          if (this.Operation == ServiceOperation.Delete)
          {
            if (this.DeleteStop && serviceController1.Status != ServiceControllerStatus.StopPending)
            {
              if (serviceController1.Status != ServiceControllerStatus.Stopped)
              {
                try
                {
                  serviceController1.Stop();
                }
                catch (Exception ex)
                {
                  serviceController1.Refresh();
                  if (serviceController1.Status != ServiceControllerStatus.Stopped)
                  {
                    if (serviceController1.Status != ServiceControllerStatus.StopPending)
                      Console.WriteLine("Service stop failed: " + ex.Message);
                  }
                }
                cmdAction.Command = Program.ProcessHacker + " -s -elevate -c -ctype service -cobject " + serviceController1.ServiceName + " -caction stop";
                if (Program.UseKernelDriver)
                  cmdAction.RunTaskOnMainThread();
                Console.WriteLine("Waiting for the service to stop...");
                try
                {
                  serviceController1.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(5000.0));
                }
                catch (Exception ex)
                {
                  serviceController1.Refresh();
                  if (serviceController1.Status != ServiceControllerStatus.Stopped)
                    Console.WriteLine("Service stop timeout exceeded.");
                }
                try
                {
                  new TaskKillAction()
                  {
                    ProcessID = new int?(ame_assassin.Win32.ServiceEx.GetServiceProcessId(serviceController1.ServiceName))
                  }.RunTask();
                }
                catch (Exception ex)
                {
                  serviceController1.Refresh();
                  if (serviceController1.Status != ServiceControllerStatus.Stopped)
                    Console.WriteLine("Could not kill service " + serviceController1.ServiceName + ".");
                }
              }
            }
            if (this.RegistryDelete)
            {
              new RegistryKeyAction()
              {
                KeyName = ("HKLM\\SYSTEM\\CurrentControlSet\\Services\\" + this.ServiceName),
                Operation = RegistryKeyOperation.Delete
              }.RunTask();
            }
            else
            {
              try
              {
                ServiceInstaller serviceInstaller = new ServiceInstaller();
                serviceInstaller.Context = new InstallContext();
                serviceInstaller.ServiceName = serviceController1.ServiceName;
                serviceInstaller.Uninstall((IDictionary) null);
              }
              catch (Exception ex)
              {
                Console.WriteLine("Service uninstall failed: " + ex.Message);
              }
              cmdAction.Command = Program.ProcessHacker + " -s -elevate -c -ctype service -cobject " + serviceController1.ServiceName + " -caction delete";
              if (Program.UseKernelDriver)
                cmdAction.RunTaskOnMainThread();
            }
          }
          else if (this.Operation == ServiceOperation.Start)
          {
            try
            {
              serviceController1.Start();
            }
            catch (Exception ex)
            {
              serviceController1.Refresh();
              if (serviceController1.Status != ServiceControllerStatus.Running)
                Console.WriteLine("Service start failed: " + ex.Message);
            }
            cmdAction.Command = Program.ProcessHacker + " -s -elevate -c -ctype service -cobject " + serviceController1.ServiceName + " -caction start";
            if (Program.UseKernelDriver)
              cmdAction.RunTaskOnMainThread();
            try
            {
              serviceController1.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(5000.0));
            }
            catch (Exception ex)
            {
              serviceController1.Refresh();
              if (serviceController1.Status != ServiceControllerStatus.Running)
                Console.WriteLine("Service start timeout exceeded.");
            }
          }
          else if (this.Operation == ServiceOperation.Stop)
          {
            try
            {
              serviceController1.Stop();
            }
            catch (Exception ex)
            {
              serviceController1.Refresh();
              if (serviceController1.Status != ServiceControllerStatus.Stopped)
              {
                if (serviceController1.Status != ServiceControllerStatus.StopPending)
                  Console.WriteLine("Service stop failed: " + ex.Message);
              }
            }
            cmdAction.Command = Program.ProcessHacker + " -s -elevate -c -ctype service -cobject " + serviceController1.ServiceName + " -caction stop";
            if (Program.UseKernelDriver)
              cmdAction.RunTaskOnMainThread();
            Console.WriteLine("Waiting for the service to stop...");
            try
            {
              serviceController1.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(5000.0));
            }
            catch (Exception ex)
            {
              serviceController1.Refresh();
              if (serviceController1.Status != ServiceControllerStatus.Stopped)
                Console.WriteLine("Service stop timeout exceeded.");
            }
            try
            {
              new TaskKillAction()
              {
                ProcessID = new int?(ame_assassin.Win32.ServiceEx.GetServiceProcessId(serviceController1.ServiceName))
              }.RunTask();
            }
            catch (Exception ex)
            {
              serviceController1.Refresh();
              if (serviceController1.Status != ServiceControllerStatus.Stopped)
                Console.WriteLine("Could not kill dependent service " + serviceController1.ServiceName + ".");
            }
          }
          else if (this.Operation == ServiceOperation.Pause)
          {
            try
            {
              serviceController1.Pause();
            }
            catch (Exception ex)
            {
              serviceController1.Refresh();
              if (serviceController1.Status != ServiceControllerStatus.Paused)
                Console.WriteLine("Service pause failed: " + ex.Message);
            }
            cmdAction.Command = Program.ProcessHacker + " -s -elevate -c -ctype service -cobject " + serviceController1.ServiceName + " -caction pause";
            if (Program.UseKernelDriver)
              cmdAction.RunTaskOnMainThread();
            try
            {
              serviceController1.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromMilliseconds(5000.0));
            }
            catch (Exception ex)
            {
              serviceController1.Refresh();
              if (serviceController1.Status != ServiceControllerStatus.Paused)
                Console.WriteLine("Service pause timeout exceeded.");
            }
          }
          else if (this.Operation == ServiceOperation.Continue)
          {
            try
            {
              serviceController1.Pause();
            }
            catch (Exception ex)
            {
              serviceController1.Refresh();
              if (serviceController1.Status != ServiceControllerStatus.Running)
                Console.WriteLine("Service continue failed: " + ex.Message);
            }
            cmdAction.Command = Program.ProcessHacker + " -s -elevate -c -ctype service -cobject " + serviceController1.ServiceName + " -caction continue";
            if (Program.UseKernelDriver)
              cmdAction.RunTaskOnMainThread();
            try
            {
              serviceController1.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(5000.0));
            }
            catch (Exception ex)
            {
              serviceController1.Refresh();
              if (serviceController1.Status != ServiceControllerStatus.Running)
                Console.WriteLine("Service continue timeout exceeded.");
            }
          }
          serviceController1?.Dispose();
          Thread.Sleep(100);
          this.InProgress = false;
        }
      }
    }
  }
}
