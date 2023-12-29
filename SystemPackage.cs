// Decompiled with JetBrains decompiler
// Type: ame_assassin.SystemPackage
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using TrustedUninstaller.Shared.Tasks;

#nullable disable
namespace ame_assassin
{
  public static class SystemPackage
  {
    private static List<SystemPackage.AssemblyIdentity> dependentsRemoved = new List<SystemPackage.AssemblyIdentity>();
    private static List<string> ExcludeDependentsList = new List<string>();
    private static List<string> ExcludeList = new List<string>();
    private static List<string> IncludeList = new List<string>();
    public static SystemPackage.AssemblyIdentity InputIdentity = new SystemPackage.AssemblyIdentity();
    public static string HelperDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public static List<string> RemovedRegistryKeys { get; set; } = new List<string>();

    internal static List<RegistryValue> RemovedRegistryValues { get; set; } = new List<RegistryValue>();

    public static List<string> RemovedFiles { get; set; } = new List<string>();

    public static List<string> RemovedDirectories { get; set; } = new List<string>();

    public static List<string> RemovedEventProviders { get; set; } = new List<string>();

    public static List<string> RemovedEventChannels { get; set; } = new List<string>();

    public static List<string> RemovedScheduledTasks { get; set; } = new List<string>();

    public static List<string> RemovedServices { get; set; } = new List<string>();

    public static List<string> RemovedDevices { get; set; } = new List<string>();

    public static List<string> RemovedCounters { get; set; } = new List<string>();

    private static void FetchWCP()
    {
      Console.WriteLine("Fetching required dll...");
      List<string> wcpFiles = new List<string>();
      Directory.EnumerateDirectories(Environment.ExpandEnvironmentVariables("%WINDIR%\\WinSxS"), "amd64_*servicingstack*").ToList<string>().ForEach((Action<string>) (x => wcpFiles.AddRange((IEnumerable<string>) Directory.GetFiles(x, "wcp.dll", SearchOption.AllDirectories))));
      string str = wcpFiles.OrderByDescending<string, DateTime>((Func<string, DateTime>) (x => new FileInfo(x).LastWriteTime)).FirstOrDefault<string>();
      if (str == null)
        throw new FileNotFoundException("Could not locate any wcp.dll file within WinSxS.", "wcp.dll");
      File.Copy(str, SystemPackage.HelperDir + "\\" + Path.GetFileName(str), true);
    }

    private static void RemoveItemsFromManifest(SystemPackage.ParsedXML parsedXml, bool isDependent = false)
    {
      foreach (SystemPackage.AssemblyIdentity assemblyIdentity in parsedXml.Dependents.Where<SystemPackage.AssemblyIdentity>((Func<SystemPackage.AssemblyIdentity, bool>) (x => !SystemPackage.ExcludeDependentsList.Contains(x.Name))))
      {
        SystemPackage.AssemblyIdentity dependent = assemblyIdentity;
        if (!SystemPackage.dependentsRemoved.Any<SystemPackage.AssemblyIdentity>((Func<SystemPackage.AssemblyIdentity, bool>) (x => x.IdentityMatches(dependent))))
        {
          Console.WriteLine("\r\nDependent package " + dependent.Name + " found...");
          List<SystemPackage.Manifest.ManifestData> manifestsFromIdentity = SystemPackage.Manifest.FindManifestsFromIdentity(dependent);
          if (manifestsFromIdentity.Count == 0)
            Console.WriteLine("\r\nError: No package found that matches package dependent '" + dependent.Name + "'.");
          foreach (SystemPackage.Manifest.ManifestData manifest in manifestsFromIdentity)
          {
            try
            {
              SystemPackage.RemoveItemsFromManifest(SystemPackage.Manifest.ParseManifest(manifest), true);
              Console.WriteLine("Removing dependent package " + dependent.Name + " data...");
              SystemPackage.RemoveManifestLinks(manifest);
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not remove dependent package " + dependent.Name + ".\r\nException: " + ex.Message);
            }
          }
          SystemPackage.dependentsRemoved.Add(dependent);
        }
      }
      if (isDependent)
        Console.WriteLine("\r\n--- Removing dependent package " + parsedXml.Identity.Name + "...");
      else
        Console.WriteLine("\r\n--- Removing package " + parsedXml.Identity.Name + "...");
      List<string> list1 = parsedXml.Services.Except<string>((IEnumerable<string>) SystemPackage.RemovedServices).ToList<string>();
      foreach (string str in list1)
      {
        if (parsedXml.Identity.Arch != SystemPackage.Architecture.amd64)
        {
          if (parsedXml.Identity.Arch != SystemPackage.Architecture.msil)
            break;
        }
        try
        {
          ServiceAction serviceAction = new ServiceAction();
          serviceAction.ServiceName = str;
          serviceAction.Operation = ServiceOperation.Delete;
          serviceAction.RunTask();
          if (serviceAction.GetStatus() != UninstallTaskStatus.Completed)
            throw new Exception();
        }
        catch (Exception ex1)
        {
          try
          {
            new ServiceAction()
            {
              ServiceName = str,
              Operation = ServiceOperation.Delete,
              RegistryDelete = true
            }.RunTask();
          }
          catch (Exception ex2)
          {
            Console.WriteLine("\r\nError: Could not delete service '" + str + "'.\r\nException: " + ex2.Message);
          }
        }
      }
      SystemPackage.RemovedServices.AddRange((IEnumerable<string>) list1);
      List<string> list2 = parsedXml.Devices.Except<string>((IEnumerable<string>) SystemPackage.RemovedDevices).ToList<string>();
      foreach (string str in list2)
      {
        if (parsedXml.Identity.Arch != SystemPackage.Architecture.amd64)
        {
          if (parsedXml.Identity.Arch != SystemPackage.Architecture.msil)
            break;
        }
        try
        {
          new ServiceAction()
          {
            ServiceName = str,
            Operation = ServiceOperation.Delete,
            Device = true
          }.RunTask();
        }
        catch (Exception ex3)
        {
          try
          {
            new ServiceAction()
            {
              ServiceName = str,
              Operation = ServiceOperation.Delete,
              RegistryDelete = true,
              Device = true
            }.RunTask();
          }
          catch (Exception ex4)
          {
            Console.WriteLine("\r\nError: Could not delete device '" + str + "'.\r\nException: " + ex4.Message);
          }
        }
      }
      SystemPackage.RemovedDevices.AddRange((IEnumerable<string>) list2);
      List<string> list3 = parsedXml.Files.Except<string>((IEnumerable<string>) SystemPackage.RemovedFiles).Where<string>((Func<string, bool>) (x => !parsedXml.Directories.Any<string>((Func<string, bool>) (y => x.StartsWith(y, StringComparison.OrdinalIgnoreCase))))).OrderByDescending<string, int>((Func<string, int>) (x => x.Length)).ToList<string>();
      foreach (string str in list3)
      {
        string file = str;
        try
        {
          if (SystemPackage.ExcludeList.Any<string>((Func<string, bool>) (x => Regex.Match(file, x, RegexOptions.IgnoreCase).Success)))
          {
            if (!SystemPackage.IncludeList.Any<string>((Func<string, bool>) (x => Regex.Match(file, x, RegexOptions.IgnoreCase).Success)))
              continue;
          }
          if (file.ContainsIC(Environment.ExpandEnvironmentVariables("%SYSTEMDRIVE%\\Users\\Default")))
          {
            string path = Environment.GetEnvironmentVariable("SYSTEMDRIVE") + "\\Users";
            List<string> ignoreList = new List<string>()
            {
              "Default User",
              "Public",
              "All Users"
            };
            foreach (string replacement in ((IEnumerable<string>) Directory.GetDirectories(path)).Where<string>((Func<string, bool>) (x => !ignoreList.Contains(((IEnumerable<string>) x.Split('\\')).Last<string>()))).ToList<string>())
            {
              string userFile = file.ReplaceIC(Environment.ExpandEnvironmentVariables("%SYSTEMDRIVE%\\Users\\Default"), replacement);
              if (!SystemPackage.ExcludeList.Any<string>((Func<string, bool>) (x => Regex.Match(userFile, x, RegexOptions.IgnoreCase).Success)) || SystemPackage.IncludeList.Any<string>((Func<string, bool>) (x => Regex.Match(userFile, x, RegexOptions.IgnoreCase).Success)))
              {
                new FileAction() { RawPath = userFile }.RunTask();
                if (!Directory.EnumerateFileSystemEntries(Path.GetDirectoryName(userFile)).Any<string>())
                  new FileAction().RawPath = Path.GetDirectoryName(userFile);
              }
            }
          }
          else
          {
            new FileAction() { RawPath = file }.RunTask();
            if (!Directory.EnumerateFileSystemEntries(Path.GetDirectoryName(file)).Any<string>())
              new FileAction()
              {
                RawPath = Path.GetDirectoryName(file)
              }.RunTask();
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not delete file '" + file + "'.\r\nException: " + ex.Message);
        }
      }
      SystemPackage.RemovedFiles.AddRange((IEnumerable<string>) list3);
      List<string> list4 = parsedXml.Directories.Except<string>((IEnumerable<string>) SystemPackage.RemovedDirectories).OrderByDescending<string, int>((Func<string, int>) (x => x.Length)).ToList<string>();
      foreach (string str in list4)
      {
        string directory = str;
        try
        {
          if (SystemPackage.ExcludeList.Any<string>((Func<string, bool>) (x => Regex.Match(directory, x, RegexOptions.IgnoreCase).Success)))
          {
            if (!SystemPackage.IncludeList.Any<string>((Func<string, bool>) (x => Regex.Match(directory, x, RegexOptions.IgnoreCase).Success)))
              continue;
          }
          string fullName = Directory.GetParent(directory).FullName;
          bool flag = false;
          foreach (string enumerateFile in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
          {
            string file = enumerateFile;
            if (SystemPackage.ExcludeList.Any<string>((Func<string, bool>) (x => Regex.Match(file, x, RegexOptions.IgnoreCase).Success)) && !SystemPackage.IncludeList.Any<string>((Func<string, bool>) (x => Regex.Match(file, x, RegexOptions.IgnoreCase).Success)))
              flag = true;
            else
              new FileAction() { RawPath = file }.RunTask();
          }
          if (!flag)
          {
            new FileAction() { RawPath = directory }.RunTask();
            if (!Directory.EnumerateFileSystemEntries(fullName).Any<string>())
              new FileAction() { RawPath = fullName }.RunTask();
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not delete directory '" + directory + "'.\r\nException: " + ex.Message);
        }
      }
      SystemPackage.RemovedDirectories.AddRange((IEnumerable<string>) list4);
      List<RegistryValue> list5 = parsedXml.RegistryValues.Except<RegistryValue>((IEnumerable<RegistryValue>) SystemPackage.RemovedRegistryValues).Where<RegistryValue>((Func<RegistryValue, bool>) (x => !parsedXml.RegistryKeys.Any<string>((Func<string, bool>) (y => x.Key.StartsWith(y, StringComparison.OrdinalIgnoreCase))))).OrderByDescending<RegistryValue, int>((Func<RegistryValue, int>) (x => x.Key.Length)).ToList<RegistryValue>();
      foreach (RegistryValue registryValue in list5)
      {
        try
        {
          new RegistryValueAction()
          {
            KeyName = registryValue.Key,
            Value = registryValue.Value,
            Operation = RegistryValueOperation.Delete
          }.RunTask();
          new RegistryKeyAction()
          {
            KeyName = registryValue.Key,
            OnlyIfEmpty = true
          }.RunTask();
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not delete registry value '" + registryValue.Key + "\\" + registryValue.Value + "'.\r\nException: " + ex.Message);
        }
      }
      SystemPackage.RemovedRegistryValues.AddRange((IEnumerable<RegistryValue>) list5);
      List<string> list6 = parsedXml.RegistryKeys.Except<string>((IEnumerable<string>) SystemPackage.RemovedRegistryKeys).OrderByDescending<string, int>((Func<string, int>) (x => x.Length)).ToList<string>();
      foreach (string str in list6)
      {
        try
        {
          new RegistryKeyAction()
          {
            KeyName = str,
            Operation = RegistryKeyOperation.Delete
          }.RunTask();
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not delete registry key '" + str + "'.\r\nException: " + ex.Message);
        }
      }
      SystemPackage.RemovedRegistryKeys.AddRange((IEnumerable<string>) list6);
      List<string> list7 = parsedXml.ScheduledTasks.Except<string>((IEnumerable<string>) SystemPackage.RemovedScheduledTasks).ToList<string>();
      foreach (string scheduledTask in parsedXml.ScheduledTasks)
      {
        if (parsedXml.Identity.Arch != SystemPackage.Architecture.amd64)
        {
          if (parsedXml.Identity.Arch != SystemPackage.Architecture.msil)
            break;
        }
        try
        {
          new ScheduledTaskAction()
          {
            Path = scheduledTask,
            Operation = ScheduledTaskOperation.Delete
          }.RunTask();
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not delete scheduled task '" + scheduledTask + "'.\r\nException: " + ex.Message);
        }
      }
      SystemPackage.RemovedScheduledTasks.AddRange((IEnumerable<string>) list7);
      List<string> list8 = parsedXml.EventProviders.Except<string>((IEnumerable<string>) SystemPackage.RemovedEventProviders).ToList<string>();
      foreach (string str in list8)
      {
        try
        {
          RegistryKeyAction registryKeyAction = new RegistryKeyAction()
          {
            KeyName = "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\WINEVT\\Publishers\\" + str,
            Operation = RegistryKeyOperation.Delete
          };
          if (parsedXml.Identity.Arch != SystemPackage.Architecture.amd64 && parsedXml.Identity.Arch != SystemPackage.Architecture.msil)
            registryKeyAction.KeyName = "HKLM\\Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\WINEVT\\Publishers\\" + str;
          registryKeyAction.RunTask();
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not delete event provider with GUID '" + str + "'.\r\nException: " + ex.Message);
        }
      }
      SystemPackage.RemovedEventProviders.AddRange((IEnumerable<string>) list8);
      List<string> list9 = parsedXml.EventChannels.Except<string>((IEnumerable<string>) SystemPackage.RemovedEventChannels).ToList<string>();
      foreach (string str in list9)
      {
        try
        {
          RegistryKeyAction registryKeyAction = new RegistryKeyAction()
          {
            KeyName = "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\WINEVT\\Channels\\" + str,
            Operation = RegistryKeyOperation.Delete
          };
          if (parsedXml.Identity.Arch != SystemPackage.Architecture.amd64 && parsedXml.Identity.Arch != SystemPackage.Architecture.msil)
            registryKeyAction.KeyName = "HKLM\\Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\WINEVT\\Channels\\" + str;
          registryKeyAction.RunTask();
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not delete event channel '" + str + "'.\r\nException: " + ex.Message);
        }
      }
      SystemPackage.RemovedEventChannels.AddRange((IEnumerable<string>) list9);
      List<string> list10 = parsedXml.Counters.Except<string>((IEnumerable<string>) SystemPackage.RemovedCounters).ToList<string>();
      foreach (string str in list10)
      {
        try
        {
          RegistryKeyAction registryKeyAction = new RegistryKeyAction()
          {
            KeyName = "HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Perflib\\_V2Providers\\" + str,
            Operation = RegistryKeyOperation.Delete
          };
          if (parsedXml.Identity.Arch != SystemPackage.Architecture.amd64 && parsedXml.Identity.Arch != SystemPackage.Architecture.msil)
            registryKeyAction.KeyName = "HKLM\\Software\\Wow6432Node\\Microsoft\\Windows NT\\CurrentVersion\\Perflib\\_V2Providers\\" + str;
          registryKeyAction.RunTask();
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not delete performance counter with GUID '" + str + "'.\r\nException: " + ex.Message);
        }
      }
      SystemPackage.RemovedCounters.AddRange((IEnumerable<string>) list10);
    }

    private static void RemoveManifestLinks(SystemPackage.Manifest.ManifestData manifest)
    {
      string manifestWithoutExt = Path.GetFileNameWithoutExtension(manifest.ParsedName.RawPath);
      try
      {
        if (SystemPackage.ExcludeList.Any<string>((Func<string, bool>) (x => Regex.Match("%WINDIR%\\WinSxS\\Backup\\" + manifestWithoutExt, x, RegexOptions.IgnoreCase).Success)))
        {
          if (!SystemPackage.IncludeList.Any<string>((Func<string, bool>) (x => Regex.Match("%WINDIR%\\WinSxS\\Backup\\" + manifestWithoutExt, x, RegexOptions.IgnoreCase).Success)))
            goto label_5;
        }
        new FileAction()
        {
          RawPath = Environment.ExpandEnvironmentVariables("%WINDIR%\\WinSxS\\Backup\\" + manifestWithoutExt + "*")
        }.RunTask();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message ?? "");
      }
label_5:
      try
      {
        if (SystemPackage.ExcludeList.Any<string>((Func<string, bool>) (x => Regex.Match(manifest.ParsedName.RawPath, x, RegexOptions.IgnoreCase).Success)))
        {
          if (!SystemPackage.IncludeList.Any<string>((Func<string, bool>) (x => Regex.Match(manifest.ParsedName.RawPath, x, RegexOptions.IgnoreCase).Success)))
            goto label_10;
        }
        new FileAction()
        {
          RawPath = manifest.ParsedName.RawPath
        }.RunTask();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message ?? "");
      }
label_10:
      try
      {
        if (SystemPackage.ExcludeList.Any<string>())
        {
          foreach (string enumerateFile in Directory.EnumerateFiles(Environment.ExpandEnvironmentVariables("%WINDIR%\\WinSxS\\") + manifestWithoutExt, "*", SearchOption.AllDirectories))
          {
            string file = enumerateFile;
            if (!SystemPackage.ExcludeList.Any<string>((Func<string, bool>) (x => Regex.Match(file, x, RegexOptions.IgnoreCase).Success)) || SystemPackage.IncludeList.Any<string>((Func<string, bool>) (x => Regex.Match(file, x, RegexOptions.IgnoreCase).Success)))
              new FileAction() { RawPath = file }.RunTask();
          }
        }
        else
          new FileAction()
          {
            RawPath = (Environment.ExpandEnvironmentVariables("%WINDIR%\\WinSxS\\") + manifestWithoutExt)
          }.RunTask();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message ?? "");
      }
      string path = Environment.ExpandEnvironmentVariables("%WINDIR%\\WinSxS\\migration.xml");
      if (File.Exists(path))
      {
        try
        {
          string contents = File.ReadAllText(path).Replace("<file>" + manifest.ParsedName.RawName + "</file>", "");
          File.WriteAllText(path, contents);
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not modify migration.xml'.\r\nException: " + ex.Message);
        }
      }
      if (!manifest.Identity.IsDriver)
        return;
      try
      {
        string file = Environment.ExpandEnvironmentVariables("%WINDIR%\\System32\\DriverStore\\FileRepository\\" + manifest.Identity.Name.Replace("dual_", "") + "_" + manifest.ParsedName.Arch.ToString() + "*");
        if (SystemPackage.ExcludeList.Any<string>((Func<string, bool>) (x => Regex.Match(file, x, RegexOptions.IgnoreCase).Success)))
        {
          if (!SystemPackage.IncludeList.Any<string>((Func<string, bool>) (x => Regex.Match(file, x, RegexOptions.IgnoreCase).Success)))
            goto label_30;
        }
        new FileAction() { RawPath = file }.RunTask();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message ?? "");
      }
label_30:
      try
      {
        string file = Environment.ExpandEnvironmentVariables("%WINDIR%\\System32\\DriverStore\\en-US\\" + manifest.Identity.Name.Replace("dual_", "") + "_loc");
        if (SystemPackage.ExcludeList.Any<string>((Func<string, bool>) (x => Regex.Match(file, x, RegexOptions.IgnoreCase).Success)))
        {
          if (!SystemPackage.IncludeList.Any<string>((Func<string, bool>) (x => Regex.Match(file, x, RegexOptions.IgnoreCase).Success)))
            goto label_35;
        }
        new FileAction() { RawPath = file }.RunTask();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message ?? "");
      }
label_35:
      try
      {
        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\DriverDatabase\\DriverFiles", true);
        foreach (string valueName in registryKey.GetValueNames())
        {
          if ((string) registryKey.GetValue(valueName) == manifest.Identity.Name.Replace("dual_", ""))
            new RegistryValueAction()
            {
              KeyName = "HKLM\\SYSTEM\\DriverDatabase\\DriverFiles",
              Value = valueName
            }.RunTask();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message ?? "");
      }
      try
      {
        foreach (string str in ((IEnumerable<string>) Registry.LocalMachine.OpenSubKey("SYSTEM\\DriverDatabase\\DriverInfFiles", true).GetValueNames()).Where<string>(new Func<string, bool>(manifest.Identity.Name.Replace("dual_", "").Equals)))
          new RegistryValueAction()
          {
            KeyName = "HKLM\\SYSTEM\\DriverDatabase\\DriverInfFiles",
            Value = str
          }.RunTask();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message ?? "");
      }
      try
      {
        foreach (string str in ((IEnumerable<string>) Registry.LocalMachine.OpenSubKey("SYSTEM\\DriverDatabase\\DriverPackages").GetValueNames()).Where<string>(new Func<string, bool>((manifest.Identity.Name.Replace("dual_", "") + "_" + manifest.ParsedName.Arch.ToString()).Equals)))
          new RegistryValueAction()
          {
            KeyName = "HKLM\\SYSTEM\\DriverDatabase\\DriverPackages",
            Value = str
          }.RunTask();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message ?? "");
      }
    }

    internal static void Start(string[] args)
    {
      try
      {
        SystemPackage.InputIdentity.Name = args[1];
        for (int index = 0; index <= args.Length - 1; ++index)
        {
          try
          {
            if (args[index].EqualsIC("-Arch"))
              SystemPackage.InputIdentity.Arch = (SystemPackage.Architecture) Enum.Parse(typeof (SystemPackage.Architecture), args[index + 1]);
          }
          catch (ArgumentException ex)
          {
            Console.WriteLine("\r\nArgument -Arch must be supplied with amd64, x86, wow64, msil, or All.");
            Environment.Exit(1);
          }
          try
          {
            if (args[index].EqualsIC("-Language"))
              SystemPackage.InputIdentity.Language = args[index + 1];
          }
          catch (ArgumentException ex)
          {
            Console.WriteLine("\r\nArgument -Language must be supplied with a string.");
            Environment.Exit(1);
          }
          try
          {
            if (args[index].EqualsIC("-HelperDir"))
              SystemPackage.HelperDir = args[index + 1];
          }
          catch (ArgumentException ex)
          {
            Console.WriteLine("\r\nArgument -HelperDir must be supplied with a string.");
            Environment.Exit(1);
          }
          if (args[index].EqualsIC("-xdependent"))
            SystemPackage.ExcludeDependentsList.Add(args[index + 1]);
          if (args[index].EqualsIC("-xf"))
            SystemPackage.ExcludeList.Add(args[index + 1]);
          if (args[index].EqualsIC("-if"))
            SystemPackage.IncludeList.Add(args[index + 1]);
        }
      }
      catch (IndexOutOfRangeException ex)
      {
        Console.WriteLine("\r\nArgument -SystemPackage, -Arch, -xdependent, -xf, or -if must have a supplied string.");
        Environment.Exit(1);
      }
      if (!File.Exists(SystemPackage.HelperDir + "\\wcp.dll"))
        SystemPackage.FetchWCP();
      List<SystemPackage.Manifest.ManifestData> manifestsFromIdentity = SystemPackage.Manifest.FindManifestsFromIdentity(SystemPackage.InputIdentity);
      if (manifestsFromIdentity.Count == 0)
      {
        Console.WriteLine("\r\nNo package found that matches '" + SystemPackage.InputIdentity.Name + "' with the specified properties.");
        Environment.Exit(0);
      }
      foreach (SystemPackage.Manifest.ManifestData manifest in manifestsFromIdentity)
      {
        try
        {
          SystemPackage.RemoveItemsFromManifest(SystemPackage.Manifest.ParseManifest(manifest));
          Console.WriteLine("Removing package " + manifest.Identity.Name + " data...");
          SystemPackage.RemoveManifestLinks(manifest);
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not remove package " + SystemPackage.InputIdentity.Name + ".\r\nException: " + ex.Message);
        }
      }
      if (FileLock.HasKilledExplorer)
      {
        try
        {
          new RunAction()
          {
            Exe = "explorer.exe",
            Wait = false
          }.RunTaskOnMainThread();
        }
        catch (Exception ex)
        {
        }
      }
      Console.WriteLine("\r\nComplete!");
    }

    private static class Manifest
    {
      public static SystemPackage.Manifest.ParsedName ParseName(string name)
      {
        SystemPackage.Manifest.ParsedName name1 = new SystemPackage.Manifest.ParsedName();
        name1.RawName = name;
        string[] source = name.Split('_');
        name1.Arch = (SystemPackage.Architecture) Enum.Parse(typeof (SystemPackage.Architecture), ((IEnumerable<string>) source).First<string>());
        name1.ShortName = string.Join("_", ((IEnumerable<string>) source).Take<string>(source.Length - 4).Skip<string>(1));
        name1.PublicKey = source[source.Length - 4];
        name1.Version = source[source.Length - 3];
        name1.Language = source[source.Length - 2];
        name1.Randomizer = source[source.Length - 1];
        return name1;
      }

      [DllImport("assassin-helper.dll")]
      private static extern StringBuilder ParseFile(string path);

      internal static string FetchXML(string manifest, bool fullPath = false)
      {
        string str1 = SystemPackage.Manifest.ParseFile(!fullPath ? Environment.ExpandEnvironmentVariables("%WINDIR%\\WinSxS\\manifests\\") + manifest : manifest).ToString();
        string str2 = str1.Substring(str1.IndexOf("<?xml", StringComparison.OrdinalIgnoreCase));
        return str2.Substring(0, str2.IndexOf("\n</assembly>", StringComparison.OrdinalIgnoreCase) + 12);
      }

      public static void ReplaceIC(ref StringBuilder builder, string value, string replacement)
      {
        string str = Regex.Replace(builder.ToString(), Regex.Escape(value), Environment.ExpandEnvironmentVariables(replacement), RegexOptions.IgnoreCase);
        builder = new StringBuilder(str);
      }

      private static string TranslateVariables(string text, SystemPackage.Architecture arch)
      {
        StringBuilder builder = new StringBuilder(text);
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.windows)", "%WINDIR%");
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.help)", "%WINDIR%\\Help");
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.programData)", "%PROGRAMDATA%");
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.bootdrive)", "%SYSTEMDRIVE%");
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.Public)", "%SYSTEMDRIVE%\\Users\\Public");
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.apppatch)", "%WINDIR%\\apppatch");
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.inf)", "%WINDIR%\\INF");
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.systemRoot)", "%SYSTEMROOT%");
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.windir)", "%WINDIR%");
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.fonts)", "%WINDIR%\\Fonts");
        if (arch == SystemPackage.Architecture.amd64)
        {
          SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.system32)", "%WINDIR%\\System32");
          SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.drivers)", "%WINDIR%\\System32\\drivers");
          SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.wbem)", "%WINDIR%\\System32\\wbem");
          SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.programFiles)", "%PROGRAMFILES%");
          SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.commonFiles)", "%PROGRAMFILES%\\Common Files");
        }
        else
        {
          SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.system32)", "%WINDIR%\\SysWOW64");
          SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.drivers)", "%WINDIR%\\SysWOW64\\drivers");
          SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.wbem)", "%WINDIR%\\SysWOW64\\wbem");
          SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.programFiles)", "%PROGRAMFILES(x86)%");
          SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.commonFiles)", "%PROGRAMFILES(x86)%\\Common Files");
        }
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.userProfile)", "%SYSTEMDRIVE%\\Users\\Default");
        SystemPackage.Manifest.ReplaceIC(ref builder, "$(runtime.startMenu)", "%PROGRAMDATA%\\Microsoft\\Windows\\Start Menu");
        return builder.ToString();
      }

      internal static List<SystemPackage.Manifest.ManifestData> FindManifestsFromIdentity(
        SystemPackage.AssemblyIdentity identity)
      {
        Console.WriteLine("Searching for manifest of '" + identity.Name + "'...");
        string str1 = identity.Arch.ToString() + "_" + identity.Name.Substring(0, Math.Min(identity.Name.Length, 19)).ToLower();
        List<SystemPackage.Manifest.ManifestData> manifestsFromIdentity = new List<SystemPackage.Manifest.ManifestData>();
        foreach (string str2 in Directory.EnumerateFiles(Environment.ExpandEnvironmentVariables("%WINDIR%\\WinSxS\\manifests"), str1 + "*").Where<string>((Func<string, bool>) (x => SystemPackage.Manifest.ParseName(Path.GetFileName(x)).IdentityMatches(identity))))
        {
          try
          {
            XmlDocument xmlDocument = new XmlDocument();
            string xml = SystemPackage.Manifest.FetchXML(str2, true);
            xmlDocument.LoadXml(xml);
            SystemPackage.AssemblyIdentity identity1 = SystemPackage.Manifest.ParseIdentity((XmlElement) xmlDocument.DocumentElement.FirstChild);
            if (identity1.IdentityMatches(identity))
            {
              string fileName = Path.GetFileName(str2);
              manifestsFromIdentity.Add(new SystemPackage.Manifest.ManifestData()
              {
                xml = xml,
                ParsedName = SystemPackage.Manifest.ParseName(fileName),
                Identity = identity1
              });
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not identify manifest file '" + str2 + "'.\r\nException: " + ex.Message);
          }
        }
        return manifestsFromIdentity;
      }

      public static SystemPackage.AssemblyIdentity ParseIdentity(XmlElement identityNode)
      {
        SystemPackage.AssemblyIdentity identity = new SystemPackage.AssemblyIdentity();
        try
        {
          identity.Name = identityNode.Attributes["name"].InnerText;
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not parse identity property.\r\nException: " + ex.Message);
        }
        try
        {
          identity.Arch = (SystemPackage.Architecture) Enum.Parse(typeof (SystemPackage.Architecture), identityNode.Attributes["processorArchitecture"].InnerText);
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not parse identity property.\r\nException: " + ex.Message);
        }
        try
        {
          identity.Language = identityNode.Attributes["language"].InnerText;
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not parse identity property.\r\nException: " + ex.Message);
        }
        try
        {
          identity.PublicKey = identityNode.Attributes["publicKeyToken"].InnerText;
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not parse identity property.\r\nException: " + ex.Message);
        }
        return identity;
      }

      public static SystemPackage.ParsedXML ParseManifest(
        SystemPackage.Manifest.ManifestData manifest)
      {
        Console.WriteLine("Parsing manifest...");
        SystemPackage.ParsedXML manifest1 = new SystemPackage.ParsedXML();
        XmlElement documentElement;
        try
        {
          XmlDocument xmlDocument = new XmlDocument();
          xmlDocument.LoadXml(manifest.xml);
          documentElement = xmlDocument.DocumentElement;
          if (documentElement == null)
          {
            Console.WriteLine("\r\nError: No primary xml node found in xml.");
            return (SystemPackage.ParsedXML) null;
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not parse xml.\r\nException: " + ex.Message);
          return (SystemPackage.ParsedXML) null;
        }
        manifest1.Identity = manifest.Identity;
        if (manifest.Identity.Arch == SystemPackage.Architecture.msil)
          manifest1.Directories.Add(Environment.ExpandEnvironmentVariables("%WINDIR%\\Microsoft.NET\\assembly\\GAC_MSIL\\" + manifest.Identity.Name));
        foreach (XmlElement xmlElement in documentElement.GetElementsByTagName("dependency"))
        {
          try
          {
            if (xmlElement.ParentNode.Name == "assembly")
            {
              foreach (XmlElement identityNode in xmlElement.GetElementsByTagName("assemblyIdentity"))
                manifest1.Dependents.Add(SystemPackage.Manifest.ParseIdentity(identityNode));
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not parse a dependent XML element.\r\nException: " + ex.Message);
          }
        }
        foreach (XmlElement xmlElement in documentElement.GetElementsByTagName("registryKey"))
        {
          string text = xmlElement.Attributes["keyName"].InnerText.TrimEnd('\\');
          if (manifest1.Identity.Arch != SystemPackage.Architecture.amd64 && manifest1.Identity.Arch != SystemPackage.Architecture.msil)
          {
            if (text.ContainsIC("HKEY_LOCAL_MACHINE\\SOFTWARE") || text.ContainsIC("HKLM\\SOFTWARE"))
              text = text.ReplaceIC("HKEY_LOCAL_MACHINE\\SOFTWARE", "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node").ReplaceIC("HKLM\\SOFTWARE", "HKLM\\SOFTWARE\\Wow6432Node");
            else
              continue;
          }
          if (xmlElement.ParentNode.Name.Equals("registryKeys") && !xmlElement.ChildNodes.Cast<XmlElement>().Any<XmlElement>((Func<XmlElement, bool>) (x => x.Name == "registryValue")))
            manifest1.RegistryKeys.Add(text);
          else if (xmlElement.ParentNode.Name.Equals("registryKeys") && xmlElement.HasChildNodes)
          {
            bool flag = false;
            foreach (XmlElement childNode in xmlElement.ChildNodes)
            {
              if (!(childNode.Name != "registryValue"))
              {
                flag = true;
                if (childNode.Attributes["name"].InnerText == "FileName" && (text.StartsWith("HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Control\\WMI\\Autologger\\", StringComparison.OrdinalIgnoreCase) || text.StartsWith("HKLM\\System\\CurrentControlSet\\Control\\WMI\\Autologger\\", StringComparison.OrdinalIgnoreCase)))
                  manifest1.Files.Add(Environment.ExpandEnvironmentVariables(childNode.Attributes["name"].InnerText) + "*");
                RegistryValue registryValue = new RegistryValue()
                {
                  Key = text,
                  Value = childNode.Attributes["name"].InnerText
                };
                manifest1.RegistryValues.Add(registryValue);
              }
            }
            if (!flag)
              manifest1.RegistryKeys.Add(text);
          }
        }
        foreach (XmlElement xmlElement in documentElement.GetElementsByTagName("file"))
        {
          try
          {
            if (manifest1.Identity.Arch != SystemPackage.Architecture.amd64 && manifest1.Identity.Arch != SystemPackage.Architecture.msil)
            {
              string text = SystemPackage.Manifest.TranslateVariables(xmlElement.Attributes["destinationPath"].InnerText, manifest.Identity.Arch);
              if (!text.ContainsIC("\\Program Files (x86)\\"))
              {
                if (!text.ContainsIC("Windows\\SysWOW64"))
                  continue;
              }
            }
            if (xmlElement.ParentNode.Name == "assembly")
            {
              if (xmlElement.GetElementsByTagName("infFile").Cast<XmlElement>().Any<XmlElement>() && xmlElement.Attributes["destinationPath"] == null)
                manifest1.Files.Add(SystemPackage.Manifest.TranslateVariables("$(runtime.inf)\\" + xmlElement.Attributes["name"].InnerText, manifest.Identity.Arch));
              else if (xmlElement.Attributes["destinationPath"] != null)
              {
                string str = SystemPackage.Manifest.TranslateVariables(xmlElement.Attributes["destinationPath"].InnerText, manifest.Identity.Arch) + xmlElement.Attributes["name"].InnerText;
                manifest1.Files.Add(str);
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not parse a file XML element.\r\nException: " + ex.Message);
          }
        }
        foreach (XmlElement xmlElement in documentElement.GetElementsByTagName("file"))
        {
          try
          {
            if (manifest1.Identity.Arch != SystemPackage.Architecture.amd64 && manifest1.Identity.Arch != SystemPackage.Architecture.msil)
            {
              string text = SystemPackage.Manifest.TranslateVariables(xmlElement.Attributes["destinationPath"].InnerText, manifest.Identity.Arch);
              if (!text.ContainsIC("\\Program Files (x86)\\"))
              {
                if (!text.ContainsIC("Windows\\SysWOW64"))
                  continue;
              }
            }
            if (xmlElement.ParentNode.Name == "directories")
            {
              string str = SystemPackage.Manifest.TranslateVariables(xmlElement.Attributes["destinationPath"].InnerText, manifest.Identity.Arch).TrimEnd('\\');
              manifest1.Directories.Add(str);
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not parse a directory XML element.\r\nException: " + ex.Message);
          }
        }
        foreach (XmlElement xmlElement1 in documentElement.GetElementsByTagName("events"))
        {
          try
          {
            if (xmlElement1.ParentNode.Name == "instrumentation")
            {
              foreach (XmlElement xmlElement2 in xmlElement1.GetElementsByTagName("provider"))
              {
                manifest1.EventProviders.Add(xmlElement2.Attributes["guid"].InnerText);
                foreach (XmlElement xmlElement3 in xmlElement1.GetElementsByTagName("channel"))
                  manifest1.EventChannels.Add(xmlElement3.Attributes["name"].InnerText);
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not parse an event XML element.\r\nException: " + ex.Message);
          }
        }
        foreach (XmlElement xmlElement4 in documentElement.GetElementsByTagName("counters"))
        {
          try
          {
            if (xmlElement4.ParentNode.Name == "instrumentation")
            {
              foreach (XmlElement xmlElement5 in xmlElement4.GetElementsByTagName("provider"))
                manifest1.Counters.Add(xmlElement5.Attributes["providerGuid"].InnerText);
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not parse a counter XML element.\r\nException: " + ex.Message);
          }
        }
        foreach (XmlElement xmlElement6 in documentElement.GetElementsByTagName("Task"))
        {
          try
          {
            if (xmlElement6.ParentNode.Name == "taskScheduler")
            {
              foreach (XmlElement xmlElement7 in xmlElement6.GetElementsByTagName("URI"))
                manifest1.ScheduledTasks.Add(xmlElement7.InnerText);
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not parse a scheduled task XML element.\r\nException: " + ex.Message);
          }
        }
        foreach (XmlElement xmlElement in documentElement.GetElementsByTagName("serviceData"))
        {
          try
          {
            if (xmlElement.ParentNode.Name == "categoryInstance")
            {
              if (xmlElement.Attributes["type"].InnerText.EqualsIC("kernelDriver") || xmlElement.Attributes["type"].InnerText.EqualsIC("fileSystemDriver"))
                manifest1.Devices.Add(xmlElement.Attributes["name"].InnerText);
              else
                manifest1.Services.Add(xmlElement.Attributes["name"].InnerText);
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not parse a service XML element.\r\nException: " + ex.Message);
          }
        }
        foreach (XmlElement xmlElement in documentElement.GetElementsByTagName("shortCut"))
        {
          try
          {
            if (xmlElement.ParentNode.Name == "categoryInstance")
            {
              if (xmlElement.Attributes["destinationPath"] != null)
              {
                string str = SystemPackage.Manifest.TranslateVariables(xmlElement.Attributes["destinationPath"].InnerText, manifest.Identity.Arch) + "\\" + xmlElement.Attributes["destinationName"].InnerText;
                manifest1.Files.Add(str);
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not parse a shortcut XML element.\r\nException: " + ex.Message);
          }
        }
        try
        {
          foreach (XmlElement xmlElement8 in documentElement.GetElementsByTagName("id").Cast<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>) (x => x.ParentNode.Name == "categoryMembership" && x.Attributes["name"].InnerText.EqualsIC("Microsoft.Windows.Categories") && x.Attributes["typeName"].InnerText.EqualsIC("SvcHost"))))
          {
            try
            {
              foreach (XmlElement xmlElement9 in xmlElement8.ParentNode.ChildNodes.Cast<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>) (x => x.Name == "categoryInstance")))
              {
                string innerText = xmlElement9.Attributes["subcategory"].InnerText;
                foreach (XmlElement xmlElement10 in xmlElement9.GetElementsByTagName("serviceGroup"))
                  manifest1.RegistryValues.Add(new RegistryValue()
                  {
                    Key = manifest1.Identity.Arch == SystemPackage.Architecture.amd64 || manifest1.Identity.Arch == SystemPackage.Architecture.msil ? "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Svchost\\" + innerText : "HKLM\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows NT\\CurrentVersion\\Svchost\\" + innerText,
                    Value = xmlElement10.Attributes["serviceName"].InnerText
                  });
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not parse a svc XML element (1).\r\nException: " + ex.Message);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not parse a svc XML element (2).\r\nException: " + ex.Message);
        }
        return manifest1;
      }

      public class ParsedName
      {
        public string ShortName { get; set; }

        public SystemPackage.Architecture Arch { get; set; }

        public string Language { get; set; }

        public string PublicKey { get; set; }

        public string Version { get; set; }

        public string Randomizer { get; set; }

        public string RawName { get; set; }

        public string RawPath
        {
          get
          {
            return Environment.ExpandEnvironmentVariables("%WINDIR%\\WinSxS\\manifests\\") + this.RawName;
          }
        }

        public bool IdentityMatches(SystemPackage.AssemblyIdentity identity)
        {
          if (this.ShortName.Contains(".."))
          {
            string[] source = this.ShortName.Split(new string[1]
            {
              ".."
            }, StringSplitOptions.None);
            if (!identity.Name.ToLower().EndsWith(((IEnumerable<string>) source).Last<string>()) || !identity.Name.ToLower().StartsWith(((IEnumerable<string>) source).First<string>()))
              return false;
          }
          else if (!this.ShortName.Equals(identity.Name.ToLower()))
            return false;
          return (identity.Arch == SystemPackage.Architecture.All || identity.Arch == this.Arch) && (!(identity.Language != "*") || !(identity.Language.ToLower() != this.Language.ToLower()) || identity.Language.ToLower().Equals("neutral") && this.Language.ToLower() == "none") && (!(identity.PublicKey != "*") || !(identity.PublicKey.ToLower() != this.PublicKey.ToLower()));
        }
      }

      public class ManifestData
      {
        public string xml { get; set; }

        public SystemPackage.AssemblyIdentity Identity { get; set; }

        public SystemPackage.Manifest.ParsedName ParsedName { get; set; }
      }
    }

    private class ParsedXML
    {
      public List<SystemPackage.AssemblyIdentity> Dependents { get; set; } = new List<SystemPackage.AssemblyIdentity>();

      public SystemPackage.AssemblyIdentity Identity { get; set; }

      public List<string> RegistryKeys { get; set; } = new List<string>();

      public List<RegistryValue> RegistryValues { get; set; } = new List<RegistryValue>();

      public List<string> Files { get; set; } = new List<string>();

      public List<string> Directories { get; set; } = new List<string>();

      public List<string> EventProviders { get; set; } = new List<string>();

      public List<string> EventChannels { get; set; } = new List<string>();

      public List<string> ScheduledTasks { get; set; } = new List<string>();

      public List<string> Services { get; set; } = new List<string>();

      public List<string> Devices { get; set; } = new List<string>();

      public List<string> Counters { get; set; } = new List<string>();
    }

    internal enum Architecture
    {
      All,
      amd64,
      wow64,
      x86,
      msil,
    }

    public class AssemblyIdentity
    {
      public string Name { get; set; }

      internal SystemPackage.Architecture Arch { get; set; }

      public string PublicKey { get; set; } = "*";

      public string Language { get; set; } = "*";

      public bool IsDriver => this.Name.EndsWith(".inf");

      public bool IdentityMatches(SystemPackage.AssemblyIdentity other)
      {
        return (!(other.Name != "*") || !(this.Name != "*") || !(other.Name.ToLower() != this.Name.ToLower())) && (other.Arch == SystemPackage.Architecture.All || this.Arch == SystemPackage.Architecture.All || other.Arch == this.Arch) && (!(other.Language != "*") || !(this.Language != "*") || !(other.Language.ToLower() != this.Language.ToLower())) && (!(other.PublicKey != "*") || !(this.PublicKey != "*") || !(other.PublicKey.ToLower() != this.PublicKey.ToLower()));
      }
    }
  }
}
