// Decompiled with JetBrains decompiler
// Type: ame_assassin.RegistryKeyAction
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TrustedUninstaller.Shared.Tasks;

#nullable enable
namespace ame_assassin
{
  internal class RegistryKeyAction : ITaskAction
  {
    private static Dictionary<RegistryHive, UIntPtr> HiveKeys = new Dictionary<RegistryHive, UIntPtr>()
    {
      {
        RegistryHive.ClassesRoot,
        new UIntPtr(2147483648U)
      },
      {
        RegistryHive.CurrentConfig,
        new UIntPtr(2147483653U)
      },
      {
        RegistryHive.CurrentUser,
        new UIntPtr(2147483649U)
      },
      {
        RegistryHive.DynData,
        new UIntPtr(2147483654U)
      },
      {
        RegistryHive.LocalMachine,
        new UIntPtr(2147483650U)
      },
      {
        RegistryHive.PerformanceData,
        new UIntPtr(2147483652U)
      },
      {
        RegistryHive.Users,
        new UIntPtr(2147483651U)
      }
    };

    public void RunTaskOnMainThread() => throw new NotImplementedException();

    public string KeyName { get; set; }

    public Scope Scope { get; set; }

    public bool OnlyIfEmpty { get; set; }

    public RegistryKeyOperation Operation { get; set; }

    public int ProgressWeight { get; set; } = 1;

    public int GetProgressWeight() => this.ProgressWeight;

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegOpenKeyEx(
      UIntPtr hKey,
      string subKey,
      int ulOptions,
      int samDesired,
      out UIntPtr hkResult);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegDeleteKeyEx(
      UIntPtr hKey,
      string lpSubKey,
      uint samDesired,
      uint Reserved);

    private static void DeleteKeyTreeWin32(string key, RegistryHive hive)
    {
      RegistryKey registryKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default).OpenSubKey(key);
      if (registryKey == null)
        return;
      ((IEnumerable<string>) registryKey.GetSubKeyNames()).ToList<string>().ForEach((Action<string>) (subKey => RegistryKeyAction.DeleteKeyTreeWin32(key + "\\" + subKey, hive)));
      registryKey.Close();
      RegistryKeyAction.RegDeleteKeyEx(RegistryKeyAction.HiveKeys[hive], key, 256U, 0U);
    }

    private bool InProgress { get; set; }

    public void ResetProgress() => this.InProgress = false;

    public string ErrorString()
    {
      return "RegistryKeyAction failed to " + this.Operation.ToString().ToLower() + " key '" + this.KeyName + "'.";
    }

    private List<RegistryKey> GetRoots()
    {
      string upper = this.KeyName.Split('\\').GetValue(0).ToString().ToUpper();
      List<RegistryKey> list = new List<RegistryKey>();
      RegistryKey usersKey;
      if (upper.Equals("HKCU") || upper.Equals("HKEY_CURRENT_USER"))
      {
        switch (this.Scope)
        {
          case Scope.AllUsers:
            RegistryManager.HookUserHives();
            usersKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
            List<string> list1 = ((IEnumerable<string>) usersKey.GetSubKeyNames()).Where<string>((Func<string, bool>) (x => x.StartsWith("S-") && ((IEnumerable<string>) usersKey.OpenSubKey(x).GetSubKeyNames()).Any<string>((Func<string, bool>) (y => y.Equals("Volatile Environment"))))).ToList<string>();
            list1.AddRange((IEnumerable<string>) ((IEnumerable<string>) usersKey.GetSubKeyNames()).Where<string>((Func<string, bool>) (x => x.StartsWith("AME_UserHive_") && !x.EndsWith("_Classes"))).ToList<string>());
            list1.ForEach((Action<string>) (x => list.Add(usersKey.OpenSubKey(x, true))));
            return list;
          case Scope.ActiveUsers:
            usersKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
            ((IEnumerable<string>) usersKey.GetSubKeyNames()).Where<string>((Func<string, bool>) (x => x.StartsWith("S-") && ((IEnumerable<string>) usersKey.OpenSubKey(x).GetSubKeyNames()).Any<string>((Func<string, bool>) (y => y.Equals("Volatile Environment"))))).ToList<string>().ForEach((Action<string>) (x => list.Add(usersKey.OpenSubKey(x, true))));
            return list;
          case Scope.DefaultUser:
            usersKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
            ((IEnumerable<string>) usersKey.GetSubKeyNames()).Where<string>((Func<string, bool>) (x => x.Equals("AME_UserHive_Default") && !x.EndsWith("_Classes"))).ToList<string>().ForEach((Action<string>) (x => list.Add(usersKey.OpenSubKey(x, true))));
            return list;
        }
      }
      List<RegistryKey> registryKeyList = list;
      if (upper != null)
      {
        RegistryKey registryKey;
        switch (upper.Length)
        {
          case 3:
            if (upper == "HKU")
            {
              registryKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
              break;
            }
            goto label_25;
          case 4:
            switch (upper[3])
            {
              case 'M':
                if (upper == "HKLM")
                {
                  registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
                  break;
                }
                goto label_25;
              case 'R':
                if (upper == "HKCR")
                {
                  registryKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default);
                  break;
                }
                goto label_25;
              case 'U':
                if (upper == "HKCU")
                {
                  registryKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
                  break;
                }
                goto label_25;
              default:
                goto label_25;
            }
            break;
          case 10:
            if (upper == "HKEY_USERS")
            {
              registryKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
              break;
            }
            goto label_25;
          case 17:
            switch (upper[6])
            {
              case 'L':
                if (upper == "HKEY_CLASSES_ROOT")
                {
                  registryKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default);
                  break;
                }
                goto label_25;
              case 'U':
                if (upper == "HKEY_CURRENT_USER")
                {
                  registryKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
                  break;
                }
                goto label_25;
              default:
                goto label_25;
            }
            break;
          case 18:
            if (upper == "HKEY_LOCAL_MACHINE")
            {
              registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
              break;
            }
            goto label_25;
          default:
            goto label_25;
        }
        registryKeyList.Add(registryKey);
        return list;
      }
label_25:
      throw new ArgumentException("Key '" + this.KeyName + "' does not specify a valid registry hive.");
    }

    public string GetSubKey() => this.KeyName.Substring(this.KeyName.IndexOf('\\') + 1);

    private RegistryKey? OpenSubKey(RegistryKey root)
    {
      return root.OpenSubKey(this.GetSubKey() ?? throw new ArgumentException("Key '" + this.KeyName + "' is invalid."), true);
    }

    public UninstallTaskStatus GetStatus()
    {
      try
      {
        foreach (RegistryKey root in this.GetRoots())
        {
          RegistryKey registryKey1 = root;
          string str = this.GetSubKey();
          if (registryKey1.Name.Contains("AME_UserHive_") && str.StartsWith("SOFTWARE\\Classes", StringComparison.CurrentCultureIgnoreCase))
          {
            registryKey1 = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default).OpenSubKey(registryKey1.Name.Substring(11) + "_Classes", true);
            str = Regex.Replace(str, "^SOFTWARE\\\\*Classes\\\\*", "", RegexOptions.IgnoreCase);
            if (registryKey1 == null)
              continue;
          }
          RegistryKey registryKey2 = registryKey1.OpenSubKey(str);
          if (this.Operation == RegistryKeyOperation.Delete && registryKey2 != null || this.Operation == RegistryKeyOperation.Add && registryKey2 == null)
            return UninstallTaskStatus.ToDo;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return UninstallTaskStatus.ToDo;
      }
      return UninstallTaskStatus.Completed;
    }

    public void RunTask()
    {
      Console.WriteLine(this.Operation.ToString().TrimEnd('e') + "ing registry key '" + this.KeyName + "'...");
      foreach (RegistryKey root in this.GetRoots())
      {
        RegistryKey registryKey1 = root;
        string str = this.GetSubKey();
        try
        {
          if (registryKey1.Name.Contains("AME_UserHive_") && str.StartsWith("SOFTWARE\\Classes", StringComparison.CurrentCultureIgnoreCase))
          {
            registryKey1 = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default).OpenSubKey(registryKey1.Name.Substring(11) + "_Classes", true);
            str = Regex.Replace(str, "^SOFTWARE\\\\*Classes\\\\*", "", RegexOptions.IgnoreCase);
            if (registryKey1 == null)
            {
              Console.WriteLine("User classes hive not found for hive " + root.Name + ".");
              continue;
            }
          }
          if (this.Operation == RegistryKeyOperation.Add && registryKey1.OpenSubKey(str) == null)
            registryKey1.CreateSubKey(str);
          if (this.Operation == RegistryKeyOperation.Delete)
          {
            if (this.OnlyIfEmpty)
            {
              RegistryKey registryKey2 = registryKey1.OpenSubKey(str);
              if (registryKey2 != null && (((IEnumerable<string>) registryKey2.GetValueNames()).Any<string>() || ((IEnumerable<string>) registryKey2.GetSubKeyNames()).Any<string>()))
                break;
            }
            try
            {
              registryKey1.DeleteSubKeyTree(str, false);
            }
            catch (Exception ex)
            {
              Console.WriteLine(ex.GetType()?.ToString() + ": " + ex.Message);
              RegistryHive registryHive1;
              switch (((IEnumerable<string>) registryKey1.Name.Split('\\')).First<string>())
              {
                case "HKEY_CURRENT_USER":
                  registryHive1 = RegistryHive.CurrentUser;
                  break;
                case "HKEY_LOCAL_MACHINE":
                  registryHive1 = RegistryHive.LocalMachine;
                  break;
                case "HKEY_CLASSES_ROOT":
                  registryHive1 = RegistryHive.ClassesRoot;
                  break;
                case "HKEY_USERS":
                  registryHive1 = RegistryHive.Users;
                  break;
                default:
                  throw new ArgumentException("Unable to parse: " + ((IEnumerable<string>) registryKey1.Name.Split('\\')).First<string>());
              }
              RegistryHive registryHive2 = registryHive1;
              string key;
              if (!registryKey1.Name.StartsWith("HKEY_USERS"))
                key = str;
              else
                key = registryKey1.Name.Split('\\')[1] + "\\" + str;
              int hive = (int) registryHive2;
              RegistryKeyAction.DeleteKeyTreeWin32(key, (RegistryHive) hive);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
    }
  }
}
