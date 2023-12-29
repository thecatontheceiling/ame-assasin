// Decompiled with JetBrains decompiler
// Type: ame_assassin.RegistryValueAction
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrustedUninstaller.Shared.Tasks;

#nullable enable
namespace ame_assassin
{
  internal class RegistryValueAction : ITaskAction
  {
    public void RunTaskOnMainThread() => throw new NotImplementedException();

    public string KeyName { get; set; }

    public string Value { get; set; } = "";

    public object? Data { get; set; }

    public RegistryValueType Type { get; set; }

    public Scope Scope { get; set; }

    public RegistryValueOperation Operation { get; set; } = RegistryValueOperation.Add;

    public int ProgressWeight { get; set; } = 1;

    public int GetProgressWeight() => this.ProgressWeight;

    private bool InProgress { get; set; }

    public void ResetProgress() => this.InProgress = false;

    public string ErrorString()
    {
      return "RegistryValueAction failed to " + this.Operation.ToString().ToLower() + " value '" + this.Value + "' in key '" + this.KeyName + "'";
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

    public object? GetCurrentValue(RegistryKey root)
    {
      string subKey = this.GetSubKey();
      return Registry.GetValue(root.Name + "\\" + subKey, this.Value, (object) null);
    }

    public static byte[] StringToByteArray(string hex)
    {
      return Enumerable.Range(0, hex.Length).Where<int>((Func<int, bool>) (x => x % 2 == 0)).Select<int, byte>((Func<int, byte>) (x => Convert.ToByte(hex.Substring(x, 2), 16))).ToArray<byte>();
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
          if (registryKey2 != null || this.Operation != RegistryValueOperation.Set && this.Operation != RegistryValueOperation.Delete)
          {
            if (registryKey2 == null)
              return UninstallTaskStatus.ToDo;
            object first = registryKey2.GetValue(this.Value);
            if (first == null)
            {
              if (this.Operation != RegistryValueOperation.Set && this.Operation != RegistryValueOperation.Delete)
                return UninstallTaskStatus.ToDo;
            }
            else
            {
              if (this.Operation == RegistryValueOperation.Delete)
                return UninstallTaskStatus.ToDo;
              if (this.Data == null)
                return UninstallTaskStatus.ToDo;
              bool flag1;
              try
              {
                bool flag2;
                switch (this.Type)
                {
                  case RegistryValueType.REG_NONE:
                    flag2 = ((IEnumerable<byte>) (byte[]) first).SequenceEqual<byte>((IEnumerable<byte>) new byte[0]);
                    break;
                  case RegistryValueType.REG_UNKNOWN:
                    flag2 = this.Data.ToString() == first.ToString();
                    break;
                  case RegistryValueType.REG_SZ:
                    flag2 = this.Data.ToString() == first.ToString();
                    break;
                  case RegistryValueType.REG_EXPAND_SZ:
                    flag2 = Environment.ExpandEnvironmentVariables(this.Data.ToString()) == first.ToString();
                    break;
                  case RegistryValueType.REG_BINARY:
                    flag2 = ((IEnumerable<byte>) (byte[]) first).SequenceEqual<byte>((IEnumerable<byte>) RegistryValueAction.StringToByteArray(this.Data.ToString()));
                    break;
                  case RegistryValueType.REG_DWORD:
                    flag2 = (int) Convert.ToUInt32(this.Data) == (int) first;
                    break;
                  case RegistryValueType.REG_MULTI_SZ:
                    int num;
                    if (!(this.Data.ToString() == ""))
                      num = ((IEnumerable<string>) (string[]) first).SequenceEqual<string>((IEnumerable<string>) this.Data.ToString().Split(new string[1]
                      {
                        "\\0"
                      }, StringSplitOptions.None)) ? 1 : 0;
                    else
                      num = ((IEnumerable<string>) (string[]) first).SequenceEqual<string>((IEnumerable<string>) new string[0]) ? 1 : 0;
                    flag2 = num != 0;
                    break;
                  case RegistryValueType.REG_QWORD:
                    flag2 = (long) Convert.ToUInt64(this.Data) == (long) (ulong) first;
                    break;
                  default:
                    throw new ArgumentException("Impossible.");
                }
                flag1 = flag2;
              }
              catch (InvalidCastException ex)
              {
                flag1 = false;
              }
              if (!flag1)
                return UninstallTaskStatus.ToDo;
            }
          }
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
      Console.WriteLine(this.Operation.ToString().TrimEnd('e') + "ing value '" + this.Value + "' in key '" + this.KeyName + "'...");
      foreach (RegistryKey root1 in this.GetRoots())
      {
        RegistryKey root2 = root1;
        string str = this.GetSubKey();
        try
        {
          if (root2.Name.Contains("AME_UserHive_") && str.StartsWith("SOFTWARE\\Classes", StringComparison.CurrentCultureIgnoreCase))
          {
            root2 = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default).OpenSubKey(root2.Name.Substring(11) + "_Classes", true);
            str = Regex.Replace(str, "^SOFTWARE\\\\*Classes\\\\*", "", RegexOptions.IgnoreCase);
            if (root2 == null)
            {
              Console.WriteLine("User classes hive not found for hive " + root1.Name + ".");
              continue;
            }
          }
          if (this.GetCurrentValue(root2) != this.Data)
          {
            if (root2.OpenSubKey(str) == null)
            {
              if (this.Operation == RegistryValueOperation.Set)
                continue;
            }
            if (root2.OpenSubKey(str) == null && this.Operation == RegistryValueOperation.Add)
              root2.CreateSubKey(str);
            if (this.Operation == RegistryValueOperation.Delete)
              root2.OpenSubKey(str, true)?.DeleteValue(this.Value);
            else if (this.Type == RegistryValueType.REG_BINARY)
            {
              byte[] byteArray = RegistryValueAction.StringToByteArray(this.Data.ToString());
              Registry.SetValue(root2.Name + "\\" + str, this.Value, (object) byteArray, (RegistryValueKind) this.Type);
            }
            else if (this.Type == RegistryValueType.REG_DWORD)
            {
              int uint32 = (int) Convert.ToUInt32(this.Data);
              Registry.SetValue(root2.Name + "\\" + str, this.Value, (object) uint32, (RegistryValueKind) this.Type);
            }
            else if (this.Type == RegistryValueType.REG_QWORD)
              Registry.SetValue(root2.Name + "\\" + str, this.Value, (object) Convert.ToUInt64(this.Data), (RegistryValueKind) this.Type);
            else if (this.Type == RegistryValueType.REG_NONE)
            {
              byte[] numArray = new byte[0];
              Registry.SetValue(root2.Name + "\\" + str, this.Value, (object) numArray, (RegistryValueKind) this.Type);
            }
            else if (this.Type == RegistryValueType.REG_MULTI_SZ)
            {
              string[] strArray;
              if (this.Data.ToString() == "")
                strArray = new string[0];
              else
                strArray = this.Data.ToString().Split(new string[1]
                {
                  "\\0"
                }, StringSplitOptions.None);
              Registry.SetValue(root2.Name + "\\" + str, this.Value, (object) strArray, (RegistryValueKind) this.Type);
            }
            else
              Registry.SetValue(root2.Name + "\\" + str, this.Value, this.Data, (RegistryValueKind) this.Type);
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
