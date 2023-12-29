// Decompiled with JetBrains decompiler
// Type: ame_assassin.RegistryManager
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

#nullable disable
namespace ame_assassin
{
  public class RegistryManager
  {
    private static bool HivesHooked;
    private static int HivesLoaded;
    private static bool ComponentsHiveHooked;
    private static bool DriversHiveHooked;

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegLoadKey(IntPtr hKey, string lpSubKey, string lpFile);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegSaveKey(IntPtr hKey, string lpFile, uint securityAttrPtr = 0);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegUnLoadKey(IntPtr hKey, string lpSubKey);

    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern IntPtr RtlAdjustPrivilege(
      int Privilege,
      bool bEnablePrivilege,
      bool IsThreadPrivilege,
      out bool PreviousValue);

    [DllImport("advapi32.dll")]
    private static extern bool LookupPrivilegeValue(
      string lpSystemName,
      string lpName,
      ref ulong lpLuid);

    [DllImport("advapi32.dll")]
    private static extern bool LookupPrivilegeValue(
      IntPtr lpSystemName,
      string lpName,
      ref ulong lpLuid);

    public static void LoadFromFile(string path, bool classHive = false)
    {
      RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
      string lpSubKey = !path.Contains("Users\\Default\\") ? (classHive ? "AME_UserHive_" + RegistryManager.HivesLoaded.ToString() + "_Classes" : "AME_UserHive_" + (RegistryManager.HivesLoaded + 1).ToString()) : (classHive ? "AME_UserHive_Default_Classes" : "AME_UserHive_Default");
      RegistryManager.RegLoadKey(registryKey.Handle.DangerousGetHandle(), lpSubKey, path);
      if (path.Contains("Users\\Default\\"))
        return;
      ++RegistryManager.HivesLoaded;
    }

    public static void LoadFromFile(string path, string name)
    {
      RegistryManager.RegLoadKey(RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default).Handle.DangerousGetHandle(), name, path);
      ++RegistryManager.HivesLoaded;
    }

    private static void AcquirePrivileges()
    {
      ulong lpLuid = 0;
      RegistryManager.LookupPrivilegeValue(IntPtr.Zero, "SeRestorePrivilege", ref lpLuid);
      bool PreviousValue;
      RegistryManager.RtlAdjustPrivilege((int) lpLuid, true, false, out PreviousValue);
      RegistryManager.LookupPrivilegeValue(IntPtr.Zero, "SeBackupPrivilege", ref lpLuid);
      RegistryManager.RtlAdjustPrivilege((int) lpLuid, true, false, out PreviousValue);
    }

    private static void ReturnPrivileges()
    {
      ulong lpLuid = 0;
      RegistryManager.LookupPrivilegeValue(IntPtr.Zero, "SeRestorePrivilege", ref lpLuid);
      bool PreviousValue;
      RegistryManager.RtlAdjustPrivilege((int) lpLuid, false, false, out PreviousValue);
      RegistryManager.LookupPrivilegeValue(IntPtr.Zero, "SeBackupPrivilege", ref lpLuid);
      RegistryManager.RtlAdjustPrivilege((int) lpLuid, false, false, out PreviousValue);
    }

    public static void HookComponentsHive()
    {
      if (RegistryManager.ComponentsHiveHooked || ((IEnumerable<string>) RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default).GetSubKeyNames()).Any<string>((Func<string, bool>) (x => x.StartsWith("AME_ComponentsHive"))))
        return;
      RegistryManager.ComponentsHiveHooked = true;
      try
      {
        if (!File.Exists(Environment.ExpandEnvironmentVariables("%WINDIR%\\System32\\config\\COMPONENTS")))
        {
          Console.WriteLine("\r\nError: Error attempting to load components registry hive.\r\nException: COMPONENTS file not found in config foler.");
        }
        else
        {
          RegistryManager.AcquirePrivileges();
          RegistryManager.LoadFromFile(Environment.ExpandEnvironmentVariables("%WINDIR%\\System32\\config\\COMPONENTS"), "AME_ComponentsHive");
          RegistryManager.ReturnPrivileges();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Critical error while attempting to mount components hive.\r\nException: " + ex.Message);
      }
    }

    public static void UnhookComponentsHive()
    {
      if (!RegistryManager.ComponentsHiveHooked)
        return;
      try
      {
        RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
        string lpSubKey = ((IEnumerable<string>) registryKey.GetSubKeyNames()).Where<string>((Func<string, bool>) (x => x.Equals("AME_ComponentsHive"))).FirstOrDefault<string>();
        if (lpSubKey != null)
        {
          RegistryManager.AcquirePrivileges();
          RegistryManager.RegUnLoadKey(registryKey.Handle.DangerousGetHandle(), lpSubKey);
          RegistryManager.ReturnPrivileges();
        }
        registryKey.Close();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Critical error while attempting to unmount components hive.\r\nException: " + ex.Message);
      }
    }

    public static void HookDriversHive()
    {
      if (RegistryManager.DriversHiveHooked || ((IEnumerable<string>) RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default).GetSubKeyNames()).Any<string>((Func<string, bool>) (x => x.StartsWith("AME_DriversHive"))))
        return;
      RegistryManager.DriversHiveHooked = true;
      try
      {
        if (!File.Exists(Environment.ExpandEnvironmentVariables("%WINDIR%\\System32\\config\\DRIVERS")))
        {
          Console.WriteLine("\r\nError: Error attempting to load drivers registry hive.\r\nException: DRIVERS file not found in config foler.");
        }
        else
        {
          RegistryManager.AcquirePrivileges();
          RegistryManager.LoadFromFile(Environment.ExpandEnvironmentVariables("%WINDIR%\\System32\\config\\DRIVERS"), "AME_DriversHive");
          RegistryManager.ReturnPrivileges();
          Console.ReadLine();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Critical error while attempting to mount drivers hive.\r\nException: " + ex.Message);
      }
    }

    public static void UnhookDriversHive()
    {
      if (!RegistryManager.DriversHiveHooked)
        return;
      try
      {
        RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
        string lpSubKey = ((IEnumerable<string>) registryKey.GetSubKeyNames()).Where<string>((Func<string, bool>) (x => x.Equals("AME_DriversHive"))).FirstOrDefault<string>();
        if (lpSubKey != null)
        {
          RegistryManager.AcquirePrivileges();
          RegistryManager.RegUnLoadKey(registryKey.Handle.DangerousGetHandle(), lpSubKey);
          RegistryManager.ReturnPrivileges();
        }
        registryKey.Close();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Critical error while attempting to unmount drivers hive.\r\nException: " + ex.Message);
      }
    }

    public static void HookUserHives()
    {
      try
      {
        if (RegistryManager.HivesHooked || ((IEnumerable<string>) RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default).GetSubKeyNames()).Any<string>((Func<string, bool>) (x => x.StartsWith("AME_UserHive_"))))
          return;
        RegistryManager.HivesHooked = true;
        string path = Environment.GetEnvironmentVariable("SYSTEMDRIVE") + "\\Users";
        List<string> ignoreList = new List<string>()
        {
          "Default User",
          "Public",
          "All Users"
        };
        List<string> list = ((IEnumerable<string>) Directory.GetDirectories(path)).Where<string>((Func<string, bool>) (x => !ignoreList.Contains(((IEnumerable<string>) x.Split('\\')).Last<string>()))).ToList<string>();
        if (list.Any<string>())
          RegistryManager.AcquirePrivileges();
        foreach (string str in list)
        {
          if (!File.Exists(str + "\\NTUSER.DAT"))
          {
            Console.WriteLine("\r\nError: Error attempting to load user registry hive.\r\nException: NTUSER.DAT file not found in user folder '" + str + "'.");
          }
          else
          {
            RegistryManager.LoadFromFile(str + "\\NTUSER.DAT");
            if (!str.EndsWith("\\Default"))
            {
              if (!File.Exists(str + "\\AppData\\Local\\Microsoft\\Windows\\UsrClass.dat"))
                Console.WriteLine("\r\nError: Error attempting to load user classes registry hive.\r\nUsrClass.dat file not found in user appdata folder '" + str + "\\AppData\\Local\\Microsoft\\Windows'.");
              else
                RegistryManager.LoadFromFile(str + "\\AppData\\Local\\Microsoft\\Windows\\UsrClass.dat", true);
            }
          }
        }
        if (!list.Any<string>())
          return;
        RegistryManager.ReturnPrivileges();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Critical error while attempting to mount user hives.\r\nException: " + ex.Message);
      }
    }

    public static void UnhookUserHives()
    {
      try
      {
        if (!RegistryManager.HivesHooked)
          return;
        RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
        List<string> list = ((IEnumerable<string>) registryKey.GetSubKeyNames()).Where<string>((Func<string, bool>) (x => x.StartsWith("AME_UserHive_"))).ToList<string>();
        if (list.Any<string>())
          RegistryManager.AcquirePrivileges();
        foreach (string lpSubKey in list)
          RegistryManager.RegUnLoadKey(registryKey.Handle.DangerousGetHandle(), lpSubKey);
        if (list.Any<string>())
          RegistryManager.ReturnPrivileges();
        registryKey.Close();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Critical error while attempting to unmount user hives.\r\nException: " + ex.Message);
      }
    }
  }
}
