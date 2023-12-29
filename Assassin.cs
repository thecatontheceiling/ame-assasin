// Decompiled with JetBrains decompiler
// Type: ame_assassin.Assassin
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

#nullable disable
namespace ame_assassin
{
  internal static class Assassin
  {
    public static List<string> PackageFilterList;
    public static List<string> PackageDirectoryList = new List<string>();
    public static List<string> AppSubNameList;
    private static List<string> AppFiles = new List<string>();
    public static List<string> ApplicationProgIDList;

    public static void MachineData(string key, string value, bool app = false)
    {
      if (key == "_PackageID")
        Assassin.MachineDependentsData(value);
      if (key == "_ApplicationID")
      {
        try
        {
          SqliteDataReader sqliteDataReader1 = Program.ActiveTransaction.NewCommand("SELECT \"ApplicationUserModelID\",\"Executable\" FROM \"Application\" WHERE \"" + key + "\" = \"" + value + "\"").ExecuteReader();
          while (sqliteDataReader1.Read())
          {
            Assassin.AppSubNameList.Add(((IEnumerable<string>) sqliteDataReader1.GetString(0).Split('!')).Last<string>());
            if (Program.TableList.Contains("ApplicationIdentity"))
            {
              SqliteDataReader sqliteDataReader2 = Program.ActiveTransaction.NewCommand("SELECT \"_ApplicationIdentityID\" FROM \"ApplicationIdentity\" WHERE \"ApplicationUserModelID\" = \"" + sqliteDataReader1.GetString(0) + "\"").ExecuteReader();
              while (sqliteDataReader2.Read())
              {
                Assassin.MachineData("_ApplicationIdentityID", sqliteDataReader2.GetString(0));
                if (Program.Verbose)
                  Console.WriteLine("\r\nRemoving _ApplicationIdentityID value " + sqliteDataReader2.GetString(0) + " from table ApplicationIdentity...");
                try
                {
                  Program.ActiveTransaction.NewCommand("DELETE FROM \"ApplicationIdentity\" WHERE \"_ApplicationIdentityID\" = \"" + sqliteDataReader2.GetString(0) + "\"").ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                  Console.WriteLine("\r\nError: Could not remove _ApplicationIdentityID value " + sqliteDataReader2.GetString(0) + " from table ApplicationIdentity.\r\nException: " + ex.Message);
                }
              }
            }
            try
            {
              Assassin.AppFiles.Add(sqliteDataReader1.GetString(1));
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not get app executable file location data.\r\nException: " + ex.Message);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not get detailed app information.\r\nException: " + ex.Message);
        }
      }
      bool flag = false;
      foreach (string table in Program.TableList)
      {
        SqliteDataReader sqliteDataReader3 = Program.ActiveTransaction.NewCommand("SELECT \"from\" FROM PRAGMA_foreign_key_list(\"" + table + "\") WHERE \"to\" = \"" + key + "\"").ExecuteReader();
        while (sqliteDataReader3.Read())
        {
          flag = true;
          string column = sqliteDataReader3.GetString(0);
          if (table == "BundlePackage")
          {
            Assassin.MachineBundleData(column, value);
            return;
          }
          SqliteDataReader sqliteDataReader4 = Program.ActiveTransaction.NewCommand("SELECT \"" + column + "\" FROM \"" + table + "\" WHERE \"" + column + "\" = \"" + value + "\"").ExecuteReader();
          while (sqliteDataReader4.Read())
          {
            if (table != null)
            {
              switch (table.Length)
              {
                case 7:
                  if (table == "Package")
                  {
                    SqliteDataReader sqliteDataReader5 = Program.ActiveTransaction.NewCommand("SELECT \"PackageFullName\" FROM \"" + table + "\" WHERE \"" + column + "\" = \"" + value + "\"").ExecuteReader();
                    while (sqliteDataReader5.Read())
                    {
                      string str1 = sqliteDataReader5.GetString(0);
                      string str2 = str1.Remove(((IEnumerable<string>) str1.Split('_')).First<string>().Length);
                      Assassin.PackageFilterList.Add("*" + str2 + "*");
                      Console.WriteLine("\r\nRemoving package " + str1 + "\nfrom machine database...");
                    }
                    goto label_59;
                  }
                  else
                    goto label_59;
                case 8:
                  if (table == "Protocol")
                    break;
                  goto label_59;
                case 11:
                  if (table == "Application")
                  {
                    SqliteDataReader sqliteDataReader6 = Program.ActiveTransaction.NewCommand("SELECT \"ApplicationUserModelID\" FROM \"" + table + "\" WHERE \"" + column + "\" = \"" + value + "\"").ExecuteReader();
                    while (sqliteDataReader6.Read())
                      Console.WriteLine("\r\nRemoving application " + sqliteDataReader6.GetString(0) + "\nfrom machine database...");
                    goto label_59;
                  }
                  else
                    goto label_59;
                case 13:
                  switch (table[0])
                  {
                    case 'A':
                      if (table == "AppUriHandler")
                        break;
                      goto label_59;
                    case 'P':
                      if (table == "PackageFamily")
                      {
                        SqliteDataReader sqliteDataReader7 = Program.ActiveTransaction.NewCommand("SELECT \"PackageFamilyName\" FROM \"" + table + "\" WHERE \"" + column + "\" = \"" + value + "\"").ExecuteReader();
                        while (sqliteDataReader7.Read())
                          Console.WriteLine("\r\nRemoving family " + sqliteDataReader7.GetString(0) + "\nfrom machine database...");
                        goto label_59;
                      }
                      else
                        goto label_59;
                    default:
                      goto label_59;
                  }
                  break;
                case 15:
                  if (table == "PackageLocation")
                  {
                    try
                    {
                      SqliteDataReader sqliteDataReader8 = Program.ActiveTransaction.NewCommand("SELECT \"InstalledLocation\" FROM \"" + table + "\" WHERE \"" + key + "\" = \"" + value + "\"").ExecuteReader();
                      while (sqliteDataReader8.Read())
                        Assassin.PackageDirectoryList.Add(sqliteDataReader8.GetString(0));
                      goto label_59;
                    }
                    catch (Exception ex)
                    {
                      goto label_59;
                    }
                  }
                  else
                    goto label_59;
                case 19:
                  if (table == "FileTypeAssociation")
                    break;
                  goto label_59;
                case 20:
                  if (table == "DynamicAppUriHandler")
                    break;
                  goto label_59;
                case 23:
                  if (table == "PackageExternalLocation")
                  {
                    try
                    {
                      SqliteDataReader sqliteDataReader9 = Program.ActiveTransaction.NewCommand("SELECT \"Path\" FROM \"" + table + "\" WHERE \"" + key + "\" = \"" + value + "\"").ExecuteReader();
                      while (sqliteDataReader9.Read())
                        Assassin.PackageDirectoryList.Add(sqliteDataReader9.GetString(0));
                      goto label_59;
                    }
                    catch (Exception ex)
                    {
                      goto label_59;
                    }
                  }
                  else
                    goto label_59;
                default:
                  goto label_59;
              }
              try
              {
                SqliteDataReader sqliteDataReader10 = Program.ActiveTransaction.NewCommand("SELECT \"ProgID\" FROM \"" + table + "\" WHERE \"" + column + "\" = \"" + value + "\"").ExecuteReader();
                while (sqliteDataReader10.Read())
                {
                  if (!Assassin.ApplicationProgIDList.Contains(sqliteDataReader10.GetString(0)))
                    Assassin.ApplicationProgIDList.Add(sqliteDataReader10.GetString(0));
                }
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not get ProgID value from table " + table + ".\r\nException: " + ex.Message);
              }
            }
label_59:
            SqliteDataReader sqliteDataReader11 = Program.ActiveTransaction.NewCommand("SELECT \"name\" FROM PRAGMA_table_info(\"" + table + "\") WHERE \"pk\" = 1").ExecuteReader();
            while (sqliteDataReader11.Read())
            {
              string key1 = sqliteDataReader11.GetString(0);
              SqliteDataReader sqliteDataReader12 = Program.ActiveTransaction.NewCommand("SELECT \"" + key1 + "\" FROM \"" + table + "\" WHERE \"" + column + "\" = \"" + value + "\"").ExecuteReader();
              while (sqliteDataReader12.Read())
              {
                string str = sqliteDataReader12.GetString(0);
                try
                {
                  Assassin.MachineData(key1, str);
                }
                catch (Exception ex)
                {
                  Console.WriteLine("\r\nError: " + ex.Message);
                }
              }
            }
            if (Program.Verbose)
              Console.WriteLine("\r\nRemoving " + column + " value " + value + " from table " + table + "...");
            try
            {
              Program.ActiveTransaction.NewCommand("DELETE FROM \"" + table + "\" WHERE \"" + column + "\" = \"" + value + "\"").ExecuteNonQuery();
            }
            catch (Exception ex)
            {
              throw new Exception("Could not remove " + column + " value " + value + " from table " + table + ".\r\nException: " + ex.Message);
            }
          }
        }
      }
      if (flag || !"_ApplicationID|_PackageID|_PackageFamilyID".Contains(key))
        return;
      Console.WriteLine("\r\nNo foreign keys detected within database\nswitching to static method...");
      try
      {
        switch (key)
        {
          case "_ApplicationID":
            Assassin.ManualData(key, value, "Application");
            break;
          case "_PackageID":
            Assassin.ManualData(key, value, "Package");
            break;
          case "_PackageFamilyID":
            Assassin.ManualData(key, value, "PackageFamily");
            break;
        }
      }
      catch (Exception ex)
      {
        throw new Exception("Could not remove requested data.\r\nException: " + ex.Message);
      }
    }

    public static void ManualData(string key, string value, string table, bool switchKeysOnly = false)
    {
      try
      {
        Program.ActiveTransaction.DisableForeignKeys();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Could not disable foreign keys.\r\nException: " + ex.Message);
      }
      string[] source;
      string[] strArray;
      if (table != null)
      {
        switch (table.Length)
        {
          case 6:
            if (table == "Bundle")
            {
              source = new string[1]{ "OptionalBundle" };
              strArray = new string[1]{ "MainBundle" };
              try
              {
                Assassin.MachineBundleData("Bundle", value);
                goto label_71;
              }
              catch (Exception ex)
              {
                throw new Exception("Could not remove bundle data from table " + table + ".\r\nException: " + ex.Message);
              }
            }
            else
              goto label_70;
          case 7:
            if (table == "Package")
            {
              source = new string[20]
              {
                "Bundle",
                "Resource",
                "TargetDeviceFamily",
                "PackageUser",
                "PackageLocation",
                "PackageExtension",
                "MrtPackage",
                "Dependency",
                "DependencyGraph",
                "Application",
                "AppxExtension",
                "CustomInstallWork",
                "MrtSharedPri",
                "MrtUserPri",
                "NamedDependency",
                "PackageExternalLocation",
                "PackagePolicy",
                "PackageProperty",
                "WowDependencyGraph",
                "XboxPackage"
              };
              strArray = new string[20]
              {
                "Package",
                "Package",
                "Package",
                "Package",
                "Package",
                "Package",
                "Package",
                "DependentPackage",
                "DependentPackage",
                "Package",
                "Package",
                "Package",
                "Package",
                "Package",
                "Package",
                "Package",
                "Package",
                "Package",
                "DependentPackage",
                "Package"
              };
              Assassin.MachineDependentsData(value);
              SqliteDataReader sqliteDataReader = Program.ActiveTransaction.NewCommand("SELECT \"PackageFullName\" FROM \"" + table + "\" WHERE \"" + key + "\" = \"" + value + "\"").ExecuteReader();
              while (sqliteDataReader.Read())
              {
                string str1 = sqliteDataReader.GetString(0);
                string str2 = str1.Remove(((IEnumerable<string>) str1.Split('_')).First<string>().Length);
                Assassin.PackageFilterList.Add("*" + str2 + "*");
                Console.WriteLine("\r\nRemoving package " + str1 + "\nfrom machine database...");
              }
              goto label_71;
            }
            else
              goto label_70;
          case 8:
            if (table == "Protocol")
              break;
            goto label_70;
          case 11:
            switch (table[0])
            {
              case 'A':
                if (table == "Application")
                {
                  source = new string[7]
                  {
                    "DefaultTile",
                    "MrtApplication",
                    "ApplicationExtension",
                    "PrimaryTile",
                    "ApplicationUser",
                    "ApplicationContentUriRule",
                    "ApplicationProperty"
                  };
                  strArray = new string[7]
                  {
                    "Application",
                    "Application",
                    "Application",
                    "Application",
                    "Application",
                    "Application",
                    "Application"
                  };
                  SqliteDataReader sqliteDataReader1 = Program.ActiveTransaction.NewCommand("SELECT \"ApplicationUserModelID\",\"Executable\" FROM \"" + table + "\" WHERE \"" + key + "\" = \"" + value + "\"").ExecuteReader();
                  while (sqliteDataReader1.Read())
                  {
                    string str = sqliteDataReader1.GetString(0);
                    Assassin.AppSubNameList.Add(((IEnumerable<string>) str.Split('!')).Last<string>());
                    Console.WriteLine("\r\nRemoving application " + str + "\nfrom machine database...");
                    if (Program.TableList.Contains("ApplicationIdentity"))
                    {
                      SqliteDataReader sqliteDataReader2 = Program.ActiveTransaction.NewCommand("SELECT \"_ApplicationIdentityID\" FROM \"ApplicationIdentity\" WHERE \"ApplicationUserModelID\" = \"" + str + "\"").ExecuteReader();
                      while (sqliteDataReader2.Read())
                        Assassin.ManualData("_ApplicationIdentityID", sqliteDataReader2.GetString(0), "ApplicationIdentity");
                    }
                    try
                    {
                      Assassin.AppFiles.Add(sqliteDataReader1.GetString(1));
                    }
                    catch (Exception ex)
                    {
                      Console.WriteLine("\r\nError: Could not get app executable file location data.\r\nException: " + ex.Message);
                    }
                  }
                  goto label_71;
                }
                else
                  goto label_70;
              case 'D':
                if (table == "DefaultTile")
                {
                  source = new string[1]{ "MrtDefaultTile" };
                  strArray = new string[1]{ "DefaultTile" };
                  goto label_71;
                }
                else
                  goto label_70;
              default:
                goto label_70;
            }
          case 12:
            if (table == "ContentGroup")
            {
              source = new string[1]{ "ContentGroupFile" };
              strArray = new string[1]{ "ContentGroup" };
              goto label_71;
            }
            else
              goto label_70;
          case 13:
            switch (table[0])
            {
              case 'A':
                if (table == "AppUriHandler")
                  break;
                goto label_70;
              case 'P':
                if (table == "PackageFamily")
                {
                  source = new string[12]
                  {
                    "PackageFamilyUser",
                    "Package",
                    "ConnectedSetPackageFamily",
                    "DynamicAppUriHandlerGoup",
                    "EndOfLifePackage",
                    "PackageDependency",
                    "PackageFamilyPolicy",
                    "PackageFamilyUser",
                    "PackageIdentity",
                    "ProvisionedPackageExclude",
                    "SRJournal",
                    "SRJournalArchive"
                  };
                  strArray = new string[12]
                  {
                    "PackageFamily",
                    "PackageFamily",
                    "PackageFamily",
                    "PackageFamily",
                    "PackageFamily",
                    "PackageFamily",
                    "PackageFamily",
                    "PackageFamily",
                    "PackageFamily",
                    "PackageFamily",
                    "PackageFamily",
                    "PackageFamily"
                  };
                  SqliteDataReader sqliteDataReader = Program.ActiveTransaction.NewCommand("SELECT \"PackageFamilyName\" FROM \"" + table + "\" WHERE \"" + key + "\" = \"" + value + "\"").ExecuteReader();
                  while (sqliteDataReader.Read())
                    Console.WriteLine("\r\nRemoving family " + sqliteDataReader.GetString(0) + "\nfrom machine database...");
                  goto label_71;
                }
                else
                  goto label_70;
              default:
                goto label_70;
            }
            break;
          case 15:
            switch (table[7])
            {
              case 'I':
                if (table == "PackageIdentity")
                {
                  source = new string[7]
                  {
                    "DeploymentHistory",
                    "PackageMachineStatus",
                    "PackageSuperceded",
                    "PackageUserStatus",
                    "ProvisionedPackage",
                    "ProvisionedPackageDeleted",
                    "SRHistory"
                  };
                  strArray = new string[7]
                  {
                    "PackageIdentity",
                    "PackageIdentity",
                    "PackageIdentity",
                    "PackageIdentity",
                    "PackageIdentity",
                    "PackageIdentity",
                    "SRHistory"
                  };
                  goto label_71;
                }
                else
                  goto label_70;
              case 'L':
                if (table == "PackageLocation")
                {
                  try
                  {
                    SqliteDataReader sqliteDataReader = Program.ActiveTransaction.NewCommand("SELECT \"InstalledLocation\" FROM \"" + table + "\" WHERE \"" + key + "\" = \"" + value + "\"").ExecuteReader();
                    while (sqliteDataReader.Read())
                      Assassin.PackageDirectoryList.Add(sqliteDataReader.GetString(0));
                    return;
                  }
                  catch (Exception ex)
                  {
                    return;
                  }
                }
                else
                  goto label_70;
              default:
                goto label_70;
            }
          case 16:
            if (table == "PackageExtension")
            {
              source = new string[2]
              {
                "PublisherCacheFolder",
                "HostRuntime"
              };
              strArray = new string[2]
              {
                "PackageExtension",
                "PackageExtension"
              };
              goto label_71;
            }
            else
              goto label_70;
          case 17:
            if (table == "PackageFamilyUser")
            {
              source = new string[1]
              {
                "PackageFamilyUserResource"
              };
              strArray = new string[1]
              {
                "PackageFamilyUser"
              };
              goto label_71;
            }
            else
              goto label_70;
          case 19:
            switch (table[0])
            {
              case 'A':
                if (table == "ApplicationIdentity")
                {
                  source = new string[2]
                  {
                    "PrimaryTileUser",
                    "SecondaryTileUser"
                  };
                  strArray = new string[2]
                  {
                    "ApplicationIdentity",
                    "ApplicationIdentity"
                  };
                  goto label_71;
                }
                else
                  goto label_70;
              case 'F':
                if (table == "FileTypeAssociation")
                  break;
                goto label_70;
              default:
                goto label_70;
            }
            break;
          case 20:
            switch (table[0])
            {
              case 'A':
                if (table == "ApplicationExtension")
                {
                  source = new string[9]
                  {
                    "ApplicationBackgroundTask",
                    "FileTypeAssociation",
                    "Protocol",
                    "AppExecutionAlias",
                    "AppExtension",
                    "AppExtensionHost",
                    "AppService",
                    "AppUriHandler",
                    "AppUriHandlerGroup"
                  };
                  strArray = new string[9]
                  {
                    "Extension",
                    "Extension",
                    "Extension",
                    "Extension",
                    "Extension",
                    "Extension",
                    "Extension",
                    "Extension",
                    "Extension"
                  };
                  goto label_71;
                }
                else
                  goto label_70;
              case 'D':
                if (table == "DynamicAppUriHandler")
                  break;
                goto label_70;
              default:
                goto label_70;
            }
            break;
          case 21:
            if (table == "OptionalBundlePackage")
            {
              source = new string[1]
              {
                "OptionalBundleResource"
              };
              strArray = new string[1]
              {
                "OptionalBundlePackage"
              };
              goto label_71;
            }
            else
              goto label_70;
          case 23:
            if (table == "PackageExternalLocation")
            {
              try
              {
                SqliteDataReader sqliteDataReader = Program.ActiveTransaction.NewCommand("SELECT \"Path\" FROM \"" + table + "\" WHERE \"" + key + "\" = \"" + value + "\"").ExecuteReader();
                while (sqliteDataReader.Read())
                  Assassin.PackageDirectoryList.Add(sqliteDataReader.GetString(0));
                return;
              }
              catch (Exception ex)
              {
                return;
              }
            }
            else
              goto label_70;
          default:
            goto label_70;
        }
        try
        {
          SqliteDataReader sqliteDataReader = Program.ActiveTransaction.NewCommand("SELECT \"ProgID\" FROM \"" + table + "\" WHERE \"" + key + "\" = \"" + value + "\"").ExecuteReader();
          while (sqliteDataReader.Read())
          {
            if (!Assassin.ApplicationProgIDList.Contains(sqliteDataReader.GetString(0)))
              Assassin.ApplicationProgIDList.Add(sqliteDataReader.GetString(0));
          }
          return;
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not get ProgID value from table " + table + ".\r\nException: " + ex.Message);
          return;
        }
      }
label_70:
      source = new string[0];
      strArray = new string[0];
label_71:
      int index = 0;
      foreach (string table1 in ((IEnumerable<string>) source).Where<string>((Func<string, bool>) (subTable => Program.TableList.Contains(subTable))))
      {
        string str3 = strArray[index];
        ++index;
        SqliteDataReader sqliteDataReader3 = Program.ActiveTransaction.NewCommand("SELECT \"name\" FROM PRAGMA_table_info(\"" + table1 + "\") WHERE \"pk\" = 1").ExecuteReader();
        while (sqliteDataReader3.Read())
        {
          string key1 = sqliteDataReader3.GetString(0);
          SqliteDataReader sqliteDataReader4 = Program.ActiveTransaction.NewCommand("SELECT \"" + key1 + "\" FROM \"" + table1 + "\" WHERE \"" + str3 + "\" = \"" + value + "\"").ExecuteReader();
          while (sqliteDataReader4.Read())
          {
            string str4 = sqliteDataReader4.GetString(0);
            try
            {
              Assassin.ManualData(key1, str4, table1);
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: " + ex.Message);
            }
          }
        }
      }
      if (Program.Verbose)
        Console.WriteLine("\r\nRemoving " + key + " value " + value + " from table " + table + "...");
      try
      {
        Program.ActiveTransaction.NewCommand("DELETE FROM \"" + table + "\" WHERE \"" + key + "\" = \"" + value + "\"").ExecuteNonQuery();
      }
      catch (Exception ex)
      {
        throw new Exception("Could not remove " + key + " value " + value + " from table " + table + ".\r\nException: " + ex.Message);
      }
    }

    private static void MachineDependentsData(string id)
    {
      if (!Program.TableList.Contains("DependencyGraph"))
        return;
      SqliteDataReader sqliteDataReader1 = Program.ActiveTransaction.NewCommand("SELECT \"DependentPackage\" FROM \"DependencyGraph\" WHERE \"SupplierPackage\" = \"" + id + "\"").ExecuteReader();
      while (sqliteDataReader1.Read())
      {
        string str1 = sqliteDataReader1.GetString(0);
        SqliteDataReader sqliteDataReader2 = Program.ActiveTransaction.NewCommand("SELECT \"PackageFullName\" FROM \"Package\" WHERE \"_PackageID\" = \"" + str1 + "\"").ExecuteReader();
        while (sqliteDataReader2.Read())
        {
          string str2 = sqliteDataReader2.GetString(0);
          try
          {
            string str3 = str2.Remove(((IEnumerable<string>) str2.Split('_')).First<string>().Length);
            Assassin.PackageFilterList.Add("*" + str3 + "*");
            SqliteDataReader sqliteDataReader3 = Program.ActiveTransaction.NewCommand("SELECT \"_PackageID\",\"PackageFullName\" FROM \"Package\" WHERE \"PackageFullName\" LIKE \"%" + str3 + "%\"").ExecuteReader();
            while (sqliteDataReader3.Read())
            {
              string str4 = sqliteDataReader3.GetString(0);
              string str5 = sqliteDataReader3.GetString(1);
              if (!Program.PackageIdList.Contains(str4))
              {
                Program.PackageIdList.Add(str4);
                Console.WriteLine("\r\nRemoving dependent package " + str5 + "\nfrom machine database...");
                Assassin.MachineData("_PackageID", str4);
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not remove dependent packages.\r\nException: " + ex.Message);
          }
        }
      }
    }

    private static void MachineBundleData(string column, string id)
    {
      SqliteDataReader sqliteDataReader = Program.ActiveTransaction.NewCommand("SELECT MIN(_BundlePackageID), MAX(_BundlePackageID) FROM \"BundlePackage\" WHERE \"" + column + "\" = \"" + id + "\"").ExecuteReader();
      while (sqliteDataReader.Read())
      {
        if (Program.Verbose)
          Console.WriteLine("\r\nRemoving bundle data for bundle ID " + id + "...");
        Program.ActiveTransaction.NewCommand("DELETE FROM \"BundleResource\" WHERE \"BundlePackage\" BETWEEN \"" + sqliteDataReader.GetString(0) + "\" AND \"" + sqliteDataReader.GetString(1) + "\"; DELETE FROM \"BundlePackage\" WHERE Bundle = \"" + id + "\"; DELETE FROM Bundle WHERE _BundleID = \"" + id + "\"").ExecuteNonQuery();
      }
    }

    public static void ClearCache(string filter)
    {
      List<string> stringList = new List<string>();
      try
      {
        stringList.AddRange(Directory.EnumerateDirectories(Environment.GetEnvironmentVariable("PROGRAMFILES") + "\\WindowsApps", filter));
      }
      catch (Exception ex)
      {
      }
      try
      {
        stringList.AddRange(Directory.EnumerateDirectories(Environment.GetEnvironmentVariable("WINDIR") + "\\SystemApps", filter));
      }
      catch (Exception ex)
      {
      }
      foreach (string enumerateDirectory in Directory.EnumerateDirectories(Environment.GetEnvironmentVariable("SYSTEMDRIVE") + "\\Users"))
      {
        try
        {
          stringList.AddRange(Directory.EnumerateDirectories(enumerateDirectory + "\\AppData\\Local\\Microsoft\\WindowsApps", filter));
        }
        catch (Exception ex)
        {
        }
      }
      foreach (string str in stringList)
      {
        string exeDir = str;
        try
        {
          foreach (string enumerateFile in Directory.EnumerateFiles(exeDir, "*.exe"))
          {
            foreach (System.Diagnostics.Process process in ((IEnumerable<System.Diagnostics.Process>) System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(enumerateFile))).Where<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (w => w.MainModule.FileName.Contains(exeDir))))
              Assassin.KillProcess(process);
          }
        }
        catch (Exception ex)
        {
          throw new Exception("Could not kill cache package process.\r\nException: " + ex.Message);
        }
      }
      foreach (string enumerateDirectory1 in Directory.EnumerateDirectories(Environment.GetEnvironmentVariable("SYSTEMDRIVE") + "\\Users"))
      {
        if (Directory.Exists(enumerateDirectory1 + "\\AppData\\Local\\Packages"))
        {
          foreach (string enumerateDirectory2 in Directory.EnumerateDirectories(enumerateDirectory1 + "\\AppData\\Local\\Packages", filter))
          {
            string item = enumerateDirectory2;
            try
            {
              foreach (string enumerateFile in Directory.EnumerateFiles(item, "*.exe"))
              {
                foreach (System.Diagnostics.Process process in ((IEnumerable<System.Diagnostics.Process>) System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(enumerateFile))).Where<System.Diagnostics.Process>((Func<System.Diagnostics.Process, bool>) (w => w.MainModule.FileName.Contains(item))))
                  Assassin.KillProcess(process);
              }
            }
            catch (Exception ex)
            {
              throw new Exception("Could not kill cache package process.\r\nException: " + ex.Message);
            }
            Assassin.FilterDelete(item + "\\TempState", "*");
            if (Directory.Exists(item + "\\LocalState"))
            {
              foreach (string enumerateDirectory3 in Directory.EnumerateDirectories(item + "\\LocalState", "*Cache*"))
                Assassin.FilterDelete(enumerateDirectory3, "*", "SettingsCache.txt");
            }
          }
        }
      }
    }

    public static void KillProcess(System.Diagnostics.Process process)
    {
      if (process.ProcessName == "System")
        return;
      if (process.ProcessName == "Registry")
        return;
      try
      {
        new TaskKillAction()
        {
          ProcessName = process.ProcessName,
          ProcessID = new int?(process.Id)
        }.RunTask();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Could not kill process " + process.ProcessName + ".\r\nException: " + ex.Message);
      }
    }

    public static void Files(string filter, bool app = false)
    {
      if (app)
      {
        foreach (string str in Assassin.AppSubNameList.Where<string>((Func<string, bool>) (w => !w.Equals("App"))))
          Assassin.AppFiles.Add(str + "*");
        List<string> stringList1 = new List<string>();
        foreach (string applicationPackageDir in Program.ApplicationPackageDirList)
        {
          foreach (string appFile in Assassin.AppFiles)
          {
            if (Directory.Exists(applicationPackageDir + "\\" + appFile))
              Assassin.FilterDelete(applicationPackageDir, appFile ?? "");
            else if (File.Exists(applicationPackageDir + "\\" + appFile))
            {
              string str1 = ((IEnumerable<string>) appFile.Split('.')).Last<string>();
              if (str1 == "exe")
              {
                string str2 = ((IEnumerable<string>) appFile.Remove(appFile.Length - str1.Length).Split('\\')).LastOrDefault<string>() + "*";
                Assassin.FilterDelete(applicationPackageDir ?? "", str2);
                foreach (string enumerateDirectory1 in Directory.EnumerateDirectories(Environment.GetEnvironmentVariable("SYSTEMDRIVE") + "\\Users"))
                {
                  List<string> stringList2 = new List<string>();
                  try
                  {
                    stringList2.Add(enumerateDirectory1 + "\\AppData\\Local\\Microsoft\\WindowsApps");
                    foreach (string enumerateDirectory2 in Directory.EnumerateDirectories(enumerateDirectory1 + "\\AppData\\Local\\Microsoft\\WindowsApps", "*" + ((IEnumerable<string>) applicationPackageDir.Split('\\')).LastOrDefault<string>() + "*"))
                      stringList2.Add(enumerateDirectory2);
                  }
                  catch (Exception ex)
                  {
                  }
                  foreach (string path in stringList2)
                  {
                    if (Directory.Exists(path))
                      Directory.EnumerateFiles(path, str2).ToList<string>().ForEach((Action<string>) (x => Assassin.FilterDelete(x, "*")));
                  }
                }
              }
              else
                Assassin.FilterDelete(applicationPackageDir ?? "", appFile ?? "");
            }
          }
          stringList1.Add(Directory.EnumerateFiles(applicationPackageDir, "AppxManifest.xml").FirstOrDefault<string>());
        }
        foreach (string applicationPackage in Program.ApplicationPackageList)
        {
          string package = applicationPackage;
          stringList1.Add(Directory.EnumerateFiles(Environment.GetEnvironmentVariable("PROGRAMDATA") + "\\Microsoft\\Windows\\AppRepository", "*.xml").Where<string>((Func<string, bool>) (f => f.Contains(package))).FirstOrDefault<string>());
        }
        foreach (string filename in stringList1)
        {
          try
          {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);
            foreach (string appSubName in Assassin.AppSubNameList)
            {
              foreach (object obj in xmlDocument.GetElementsByTagName("/Package/Applications"))
                ;
              XmlNode oldChild = xmlDocument.SelectSingleNode("//*[@Id='" + appSubName + "']");
              try
              {
                try
                {
                  Console.WriteLine("\r\nRemoving application xml with Id " + appSubName + " from file " + filename + "...");
                  oldChild.ParentNode.RemoveChild(oldChild);
                }
                catch (NullReferenceException ex)
                {
                }
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove " + appSubName + " from xml document " + filename + ".\r\nException: " + ex.Message);
              }
            }
            xmlDocument.Save(filename);
            foreach (string appFile in Assassin.AppFiles)
            {
              XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
              nsmgr.AddNamespace("spc", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");
              XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/*/spc:Extensions[1]/*/*/*[starts-with(text(), '" + appFile + "')]", nsmgr);
              try
              {
                try
                {
                  foreach (XmlNode xmlNode in xmlNodeList)
                  {
                    XmlNode parentNode1 = xmlNode.ParentNode.ParentNode.ParentNode;
                    XmlNode parentNode2 = xmlNode.ParentNode.ParentNode;
                    Console.WriteLine("\r\nRemoving xml extension item with element " + xmlNode.Name + " " + xmlNode.InnerText + " from file " + filename + "...");
                    parentNode1.RemoveChild(parentNode2);
                  }
                }
                catch (NullReferenceException ex)
                {
                }
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove " + appFile + " extension data from xml document " + filename + ".\r\nException: " + ex.Message);
              }
              xmlDocument.Save(filename);
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not fetch application package xml values.\r\nException:" + ex.Message);
          }
        }
      }
      else
      {
        try
        {
          foreach (string enumerateDirectory in Directory.EnumerateDirectories(Environment.GetEnvironmentVariable("SYSTEMDRIVE") + "\\Users"))
          {
            Assassin.FilterDelete(enumerateDirectory + "\\AppData\\Local\\Packages", filter);
            Assassin.FilterDelete(enumerateDirectory + "\\AppData\\Local\\Microsoft\\WindowsApps", filter);
            foreach (string appFile in Assassin.AppFiles)
            {
              if (Directory.Exists(enumerateDirectory + "\\AppData\\Local\\Microsoft\\WindowsApps"))
              {
                string str = ((IEnumerable<string>) appFile.Split('.')).Last<string>();
                if (str == "exe")
                {
                  string searchPattern = ((IEnumerable<string>) appFile.Remove(appFile.Length - str.Length).Split('\\')).LastOrDefault<string>() + "*";
                  foreach (string enumerateFile in Directory.EnumerateFiles(enumerateDirectory + "\\AppData\\Local\\Microsoft\\WindowsApps", searchPattern))
                    Assassin.FilterDelete(enumerateDirectory + "\\AppData\\Local\\Microsoft\\WindowsApps", ((IEnumerable<string>) enumerateFile.Split('\\')).LastOrDefault<string>());
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not remove user data of specified package or family.\r\nException: " + ex.Message);
        }
        Assassin.FilterDelete(Environment.GetEnvironmentVariable("PROGRAMFILES") + "\\WindowsApps", filter);
        Assassin.FilterDelete(Environment.GetEnvironmentVariable("WINDIR") + "\\SystemApps", filter);
        Assassin.FilterDelete(Environment.GetEnvironmentVariable("PROGRAMDATA") + "\\Packages", filter);
        Assassin.FilterDelete(Environment.GetEnvironmentVariable("PROGRAMDATA") + "\\Microsoft\\Windows\\AppRepository", filter);
        Assassin.FilterDelete(Environment.GetEnvironmentVariable("PROGRAMDATA") + "\\Microsoft\\Windows\\AppRepository\\Packages", filter);
      }
    }

    public static void FilterDelete(string directory, string filter, string exclude = null)
    {
      if (Directory.Exists(directory))
      {
        foreach (string enumerateFile in Directory.EnumerateFiles(directory, filter))
        {
          if (exclude == null || !enumerateFile.EndsWith(exclude))
          {
            try
            {
              Console.WriteLine("\r\nRemoving file " + enumerateFile + "...");
              File.Delete(enumerateFile);
            }
            catch (Exception ex1)
            {
              Console.WriteLine("\r\nError: Could not delete file " + enumerateFile + ".\r\nException: " + ex1.Message + "\n\nAttempting to kill any locking processes...");
              List<System.Diagnostics.Process> processList = new List<System.Diagnostics.Process>();
              try
              {
                processList = FileLock.WhoIsLocking(enumerateFile);
              }
              catch (Exception ex2)
              {
                Console.WriteLine("\r\nError: Could not check file locks on file.\r\nException: " + ex2.Message);
              }
              foreach (System.Diagnostics.Process process in processList)
                Assassin.KillProcess(process);
              try
              {
                Console.WriteLine("\r\nRetry: Removing file " + enumerateFile + "...");
                File.Delete(enumerateFile);
              }
              catch (Exception ex3)
              {
                Console.WriteLine("\r\nError: Could not delete file " + enumerateFile + ".\r\nException: " + ex3.Message);
              }
            }
          }
        }
        foreach (string enumerateDirectory in Directory.EnumerateDirectories(directory, filter))
        {
          if (exclude == null || !enumerateDirectory.EndsWith(exclude))
          {
            try
            {
              Console.WriteLine("\r\nRemoving folder " + enumerateDirectory + "...");
              Directory.Delete(enumerateDirectory, true);
            }
            catch (Exception ex4)
            {
              Console.WriteLine("\r\nError: Could not delete folder " + enumerateDirectory + ".\r\nException: " + ex4.Message);
              Assassin.FilterDelete(enumerateDirectory, "*");
              try
              {
                Console.WriteLine("\r\nRetry: Removing folder " + enumerateDirectory + "...");
                Directory.Delete(enumerateDirectory, true);
              }
              catch (Exception ex5)
              {
                Console.WriteLine("\r\nError: Could not delete folder " + enumerateDirectory + ".\r\nException: " + ex5.Message);
              }
            }
          }
        }
      }
      else
      {
        if (directory.Contains("\\Users\\"))
          return;
        Console.WriteLine("\r\nDirectory " + directory + " does not exist, skipping...");
      }
    }

    public static void RegistryKeys(string filter, bool app = false, bool family = false)
    {
      filter = filter.Replace("*", ":AINV:");
      filter = Regex.Escape(filter);
      filter = filter.Replace(":AINV:", ".*");
      if (!family)
      {
        try
        {
          foreach (string subKeyName in Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\AppxAllUserStore").GetSubKeyNames())
          {
            try
            {
              RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\AppxAllUserStore\\" + subKeyName, true);
              foreach (string subkey in ((IEnumerable<string>) registryKey.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
              {
                Console.WriteLine("\r\nRemoving registry key HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\AppxAllUserStore\\" + subKeyName + "\\" + subkey + "...");
                try
                {
                  registryKey.DeleteSubKeyTree(subkey);
                }
                catch (Exception ex)
                {
                  Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
                }
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not open key HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\AppxAllUserStore\\" + subKeyName + ".\r\nException: " + ex.Message);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not open key HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\AppxAllUserStore.\r\nException: " + ex.Message);
        }
        if (app)
        {
          using (IEnumerator<string> enumerator = ((IEnumerable<string>) Registry.Users.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => w.Contains("Classes") && !w.Contains("_Default"))).GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              string current = enumerator.Current;
              RegistryKey registryKey1 = Registry.Users.OpenSubKey(current);
              foreach (string applicationPackage in Program.ApplicationPackageList)
              {
                string packageName = applicationPackage;
                foreach (string appSubName in Assassin.AppSubNameList)
                {
                  try
                  {
                    RegistryKey registryKey2 = registryKey1.OpenSubKey("Extensions\\ContractId", true);
                    foreach (string subKeyName in registryKey2.GetSubKeyNames())
                    {
                      try
                      {
                        foreach (string str in ((IEnumerable<string>) registryKey2.OpenSubKey(subKeyName + "\\PackageId").GetSubKeyNames()).Where<string>((Func<string, bool>) (w => w.Equals(packageName))))
                        {
                          try
                          {
                            if (registryKey2.OpenSubKey(appSubName) == null)
                              throw new Exception();
                            Console.WriteLine("\r\nRemoving registry key HKU\\" + current + "\\Extensions\\ContractId\\" + subKeyName + "\\PackageId\\" + packageName + "\\ActivatableClassId\\" + appSubName + "...");
                            registryKey2.DeleteSubKeyTree(subKeyName + "\\PackageId\\" + str + "\\ActivatableClassId\\" + appSubName);
                          }
                          catch
                          {
                            if (Program.Verbose)
                              Console.WriteLine("\r\nInfo: Key HKU\\" + current + "\\Extensions\\ContractId\\" + subKeyName + "\\PackageId\\" + packageName + "\\ActivatableClassId\\" + appSubName + "\ndoes not exist.");
                          }
                        }
                      }
                      catch
                      {
                        if (Program.Verbose)
                          Console.WriteLine("\r\nInfo: Key HKU\\" + current + "\\Extensions\\ContractId\\" + subKeyName + "\\PackageId\ndoes not exist.");
                      }
                    }
                  }
                  catch (Exception ex)
                  {
                    Console.WriteLine("Could not open key HKU\\" + current + "\\Extensions\\ContractId\\.\r\nException: " + ex.Message);
                  }
                  try
                  {
                    RegistryKey registryKey3 = registryKey1.OpenSubKey("Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Packages\\" + packageName, true);
                    try
                    {
                      if (registryKey3.OpenSubKey(appSubName) == null)
                        throw new Exception();
                      Console.WriteLine("\r\nRemoving registry key HKU\\" + current + "\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Packages\\" + packageName + "\\" + appSubName + "...");
                      registryKey3.DeleteSubKeyTree(appSubName);
                    }
                    catch
                    {
                      if (Program.Verbose)
                        Console.WriteLine("\r\nInfo: Key HKU\\" + current + "\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Packages\\" + packageName + "\\" + appSubName + "\ndoes not exist.");
                    }
                  }
                  catch
                  {
                    if (Program.Verbose)
                      Console.WriteLine("\r\nInfo: Key HKU\\" + current + "\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Packages\\" + packageName + "\ndoes not exist.");
                  }
                }
              }
            }
            return;
          }
        }
        else
        {
          try
          {
            foreach (string subKeyName in Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\PackageState").GetSubKeyNames())
            {
              try
              {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\PackageState\\" + subKeyName, true);
                foreach (string subkey in ((IEnumerable<string>) registryKey.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
                {
                  Console.WriteLine("\r\nRemoving registry key HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\PackageState\\" + subKeyName + "\\" + subkey + "...");
                  try
                  {
                    registryKey.DeleteSubKeyTree(subkey);
                  }
                  catch (Exception ex)
                  {
                    Console.WriteLine("Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
                  }
                }
              }
              catch (Exception ex)
              {
                Console.WriteLine("Could not open key HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\PackageState\\" + subKeyName + ".\r\nException: " + ex.Message);
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not open key HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\PackageState.\r\nException: " + ex.Message);
          }
        }
      }
      foreach (string name in ((IEnumerable<string>) Registry.Users.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => w.Contains("Classes") && !w.Contains("_Default"))))
      {
        RegistryKey registryKey4 = Registry.Users.OpenSubKey(name, true);
        RegistryKey registryKey5 = registryKey4.OpenSubKey("Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion");
        if (family)
        {
          try
          {
            RegistryKey registryKey6 = registryKey5.OpenSubKey("AppModel\\Repository\\Families", true);
            foreach (string subkey in ((IEnumerable<string>) registryKey6.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
            {
              Console.WriteLine("\r\nRemoving registry key HKU\\" + name + "\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Families\\" + subkey + "...");
              try
              {
                registryKey6.DeleteSubKeyTree(subkey);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
              }
            }
          }
          catch (Exception ex)
          {
          }
        }
        else
        {
          foreach (string subkey in ((IEnumerable<string>) registryKey4.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Assassin.ApplicationProgIDList.Contains(w))))
          {
            try
            {
              registryKey4.DeleteSubKeyTree(subkey);
            }
            catch (Exception ex)
            {
              Console.WriteLine(string.Format("\r\nError: Could not remove ProgID key {0} in user key {1}.\r\nException: {2}", (object) subkey, (object) registryKey4, (object) ex.Message));
            }
          }
          try
          {
            RegistryKey registryKey7 = registryKey4.OpenSubKey("Extensions\\ContractId", true);
            foreach (string subKeyName in registryKey7.GetSubKeyNames())
            {
              try
              {
                foreach (string str in ((IEnumerable<string>) registryKey7.OpenSubKey(subKeyName + "\\PackageId").GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
                {
                  Console.WriteLine("\r\nRemoving registry key HKU\\" + name + "\\Extensions\\ContractId\\" + subKeyName + "\\PackageId\\" + str + "...");
                  try
                  {
                    registryKey7.DeleteSubKeyTree(subKeyName + "\\PackageId\\" + str);
                  }
                  catch (Exception ex)
                  {
                    Console.WriteLine("\r\nError: Could not remove registry key " + str + ".\r\nException: " + ex.Message);
                  }
                }
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not open contract subkey " + subKeyName + "\\PackageId in " + name + ".\r\nException: " + ex.Message);
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("Could not open key HKU\\" + name + "\\Extensions\\ContractId\\Windows.Launch\\PackageId.\r\nException: " + ex.Message);
          }
          try
          {
            RegistryKey registryKey8 = registryKey4.OpenSubKey("Local Settings\\MrtCache", true);
            foreach (string subkey in ((IEnumerable<string>) registryKey8.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
            {
              Console.WriteLine("\r\nRemoving registry key HKU\\" + name + "\\Local Settings\\MrtCache\\" + subkey + "...");
              try
              {
                registryKey8.DeleteSubKeyTree(subkey);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
              }
            }
            RegistryKey mappingsKey = registryKey5.OpenSubKey("AppContainer\\Mappings", true);
            foreach (string subkey in ((IEnumerable<string>) mappingsKey.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(mappingsKey.OpenSubKey(w).GetValue("Moniker").ToString(), filter, RegexOptions.IgnoreCase).Success)))
            {
              Console.WriteLine("\r\nRemoving registry key HKU\\" + name + "\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppContainer\\Mappings\\" + subkey + "...");
              try
              {
                mappingsKey.DeleteSubKeyTree(subkey);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not open MrtCache or Mappings key.\r\nException: " + ex.Message);
          }
          try
          {
            RegistryKey registryKey9 = registryKey5.OpenSubKey("AppContainer\\Storage", true);
            foreach (string subkey in ((IEnumerable<string>) registryKey9.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
            {
              Console.WriteLine("\r\nRemoving registry key HKU\\" + name + "\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppContainer\\Storage\\" + subkey + "...");
              try
              {
                registryKey9.DeleteSubKeyTree(subkey);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
              }
            }
            RegistryKey registryKey10 = registryKey5.OpenSubKey("AppModel\\PolicyCache", true);
            foreach (string subkey in ((IEnumerable<string>) registryKey10.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
            {
              Console.WriteLine("\r\nRemoving registry key HKU\\" + name + "\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\PolicyCache\\" + subkey + "...");
              try
              {
                registryKey10.DeleteSubKeyTree(subkey);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
              }
            }
            RegistryKey registryKey11 = registryKey5.OpenSubKey("AppModel\\SystemAppData", true);
            foreach (string subkey in ((IEnumerable<string>) registryKey11.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
            {
              Console.WriteLine("\r\nRemoving registry key HKU\\" + name + "\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\SystemAppData\\" + subkey + "...");
              try
              {
                registryKey11.DeleteSubKeyTree(subkey);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not open Storage, PolicyCache, or SystemAppData key.\r\nException: " + ex.Message);
          }
          try
          {
            RegistryKey registryKey12 = registryKey5.OpenSubKey("AppModel\\Repository\\Packages", true);
            foreach (string subkey in ((IEnumerable<string>) registryKey12.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
            {
              Console.WriteLine("\r\nRemoving registry key HKU\\" + name + "\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Packages\\" + subkey + "...");
              try
              {
                registryKey12.DeleteSubKeyTree(subkey);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not open packages subkey in key HKU\\" + name + "\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository.\r\nException: " + ex.Message);
          }
        }
      }
      RegistryKey registryKey13 = Registry.ClassesRoot.OpenSubKey("Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion");
      if (family)
      {
        try
        {
          RegistryKey registryKey14 = registryKey13.OpenSubKey("AppModel\\Repository\\Families", true);
          foreach (string subkey in ((IEnumerable<string>) registryKey14.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
          {
            Console.WriteLine("\r\nRemoving registry key HKCR\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Families\\" + subkey + "...");
            try
            {
              registryKey14.DeleteSubKeyTree(subkey);
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
            }
          }
        }
        catch (Exception ex)
        {
        }
      }
      else
      {
        try
        {
          RegistryKey registryKey15 = registryKey13.OpenSubKey("AppModel\\PackageRepository\\Extensions\\ProgIDs", true);
          foreach (string applicationProgId in Assassin.ApplicationProgIDList)
          {
            string progId = applicationProgId;
            foreach (string subkey in ((IEnumerable<string>) registryKey15.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => w.Equals(progId))))
            {
              Console.WriteLine("\r\nRemoving registry key HKCR\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\PackageRepository\\Extensions\\ProgIDs\\" + subkey + "...");
              try
              {
                registryKey15.DeleteSubKeyTree(subkey);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
              }
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not open ProgIDs subkey in HKCR\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\PackageRepository. Exception: " + ex.Message);
        }
        try
        {
          RegistryKey registryKey16 = registryKey13.OpenSubKey("AppModel\\PackageRepository\\Packages", true);
          foreach (string subkey in ((IEnumerable<string>) registryKey16.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
          {
            Console.WriteLine("\r\nRemoving registry key HKCR\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Packages\\" + subkey + "...");
            try
            {
              registryKey16.DeleteSubKeyTree(subkey);
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not open packages subkey in key HKCR\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\PackageRepository.\r\nException: " + ex.Message);
        }
        try
        {
          RegistryKey registryKey17 = registryKey13.OpenSubKey("AppModel\\SystemAppData", true);
          foreach (string subkey in ((IEnumerable<string>) registryKey17.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
          {
            Console.WriteLine("\r\nRemoving registry key HKCR\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\SystemAppData\\" + subkey + "...");
            try
            {
              registryKey17.DeleteSubKeyTree(subkey);
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not open SystemAppData subkey in key HKCR\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository.\r\nException: " + ex.Message);
        }
        try
        {
          RegistryKey registryKey18 = registryKey13.OpenSubKey("AppModel\\Repository\\Packages", true);
          foreach (string subkey in ((IEnumerable<string>) registryKey18.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(w, filter, RegexOptions.IgnoreCase).Success)))
          {
            Console.WriteLine("\r\nRemoving registry key HKCR\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Packages\\" + subkey + "...");
            try
            {
              registryKey18.DeleteSubKeyTree(subkey);
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not open packages subkey in key HKCR\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository.\r\nException: " + ex.Message);
        }
        try
        {
          RegistryKey mappingsKey = registryKey13.OpenSubKey("AppModel\\Mappings", true);
          foreach (string subkey in ((IEnumerable<string>) mappingsKey.GetSubKeyNames()).Where<string>((Func<string, bool>) (w => Regex.Match(mappingsKey.OpenSubKey(w).GetValue("Moniker").ToString(), filter, RegexOptions.IgnoreCase).Success)))
          {
            Console.WriteLine("\r\nRemoving registry key HKCR\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Mappings\\" + subkey + "...");
            try
            {
              mappingsKey.DeleteSubKeyTree(subkey);
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not remove registry key " + subkey + ".\r\nException: " + ex.Message);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not open HKCR Mappings key.\r\nException: " + ex.Message);
        }
      }
    }
  }
}
