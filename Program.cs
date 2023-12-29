// Decompiled with JetBrains decompiler
// Type: ame_assassin.Program
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;

#nullable disable
namespace ame_assassin
{
  internal static class Program
  {
    public static string ProcessHacker;
    public static bool Verbose;
    public static bool UseKernelDriver;
    public static Initializer.Transaction ActiveTransaction;
    public static List<string> TableList;
    public static List<string> PackageIdList;
    public static List<string> ApplicationPackageList;
    public static List<string> ApplicationPackageDirList;

    private static void Main(string[] args)
    {
      Program.Verbose = false;
      string str1 = "\r\nAME Assassin v0.4\r\nSurgically removes APPX components (mostly).\r\n\r\nAME_Assassin [-Family|-Package|-App|-ClearCache] <string> [Optional Arguments]\r\nAccepts wildcards (*).\r\n\r\n-Family        Removes specified package family(s).\r\n-Package       Removes specified package(s).\r\n-App           Removes specified application(s) from a package(s).\r\n-ClearCache    Clears the TempState cache for a given package in all user profiles.\r\n-Verbose       Provides verbose informational output to console.\r\n-Unregister    Only unregisters the specified AppX, instead of removing files.\r\n\r\nExamples:\r\n\r\n    AME_Assassin -Family \"Microsoft.BingWeather_8wekyb3d8bbwe\"\r\n    AME_Assassin -Package *FeedbackHub* -Verbose\r\n    AME_Assassin -App *WebExperienceHost* -Unregister";
      if (!string.Equals(WindowsIdentity.GetCurrent().User.Value, "S-1-5-18", StringComparison.OrdinalIgnoreCase))
      {
        Console.WriteLine("\r\nYou must be TrustedInstaller in order to use AME Assassin.");
        Environment.Exit(1);
      }
      if (args.Length == 0 || args[0] == "/?" || args[0] == "-?" || args[0] == "/help" || args[0] == "-help" || args[0] == "--?" || args[0] == "--help")
      {
        Console.WriteLine(str1);
        Environment.Exit(0);
      }
      Program.UseKernelDriver = ((IEnumerable<string>) args).Contains<string>("-UseKernelDriver");
      string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      if (File.Exists(directoryName + "\\ProcessHacker\\x64\\ProcessHacker.exe"))
        Program.ProcessHacker = "\"" + directoryName + "\\ProcessHacker\\x64\\ProcessHacker.exe\"";
      else if (File.Exists(string.Format("{0}\\ProcessHacker\\x64\\ProcessHacker.exe", (object) Directory.GetParent(directoryName))))
        Program.ProcessHacker = string.Format("\"{0}\\ProcessHacker\\x64\\ProcessHacker.exe\"", (object) Directory.GetParent(directoryName));
      else if (Program.UseKernelDriver)
      {
        Console.WriteLine("\r\nProcessHacker executable not detected.");
        Environment.Exit(0);
      }
      if (args.Length != 0 && args[0].Equals("-SystemPackage", StringComparison.OrdinalIgnoreCase))
      {
        SystemPackage.Start(args);
        RegistryManager.UnhookUserHives();
        RegistryManager.UnhookComponentsHive();
        Environment.Exit(0);
      }
      if (args.Length != 2 && args.Length != 3)
      {
        Console.WriteLine("Invalid syntax.");
        Console.WriteLine(str1);
        Environment.Exit(1);
      }
      else if (!string.Equals(args[0], "-App", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(args[0], "-Package", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(args[0], "-Family", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(args[0], "-ClearCache", StringComparison.CurrentCultureIgnoreCase))
      {
        Console.WriteLine("Invalid syntax.");
        Console.WriteLine(str1);
        Environment.Exit(1);
      }
      if (args.Length >= 3)
      {
        foreach (string a in ((IEnumerable<string>) args).Skip<string>(2))
        {
          if (string.Equals(a, "-Verbose", StringComparison.CurrentCultureIgnoreCase))
            Program.Verbose = true;
          else if (!string.Equals(a, "-UseKernelDriver", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(a, "-Unregister", StringComparison.CurrentCultureIgnoreCase))
          {
            Console.WriteLine("Invalid syntax.");
            Console.WriteLine(str1);
          }
        }
      }
      if (string.Equals(args[0], "-ClearCache", StringComparison.CurrentCultureIgnoreCase))
      {
        try
        {
          Assassin.ClearCache(args[1]);
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: " + ex.Message);
          Environment.Exit(10);
        }
        Console.WriteLine("\r\nComplete!");
        Environment.Exit(0);
      }
      try
      {
        Initializer.Hook(Environment.GetEnvironmentVariable("PROGRAMDATA") + "\\Microsoft\\Windows\\AppRepository\\StateRepository-Machine.srd");
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nFatal Error: Could not connect to machine database.\r\nException: " + ex.Message);
        Environment.Exit(5);
      }
      try
      {
        Console.WriteLine("\r\nDropping triggers...");
        Triggers.Save();
        Triggers.Drop();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nFatal Error: " + ex.Message);
        Environment.Exit(5);
      }
      Console.WriteLine("\r\nFetching values...");
      try
      {
        Program.ActiveTransaction = new Initializer.Transaction("SELECT \"Name\" FROM main.sqlite_master WHERE type = \"table\"");
        Program.TableList = new List<string>();
        SqliteDataReader sqliteDataReader = Program.ActiveTransaction.Command.ExecuteReader();
        while (sqliteDataReader.Read())
        {
          string str2 = sqliteDataReader.GetString(0);
          Program.TableList.Add(str2);
        }
      }
      catch (Exception ex1)
      {
        try
        {
          Program.ActiveTransaction.Abort();
          Triggers.Restore();
        }
        catch (Exception ex2)
        {
          Console.WriteLine("\r\nFatal Error: " + ex2.Message);
        }
        Console.WriteLine("\r\nFatal Error: Could not fetch tables from database.\r\nException: " + ex1.Message);
      }
      Program.ActiveTransaction.Commit();
      string str3 = (string) null;
      string str4 = (string) null;
      string str5 = (string) null;
      Assassin.PackageFilterList = new List<string>();
      Assassin.AppSubNameList = new List<string>();
      Assassin.ApplicationProgIDList = new List<string>();
      switch (args[0].ToLower())
      {
        case "-family":
          Assassin.PackageFilterList.Add(args[1]);
          str3 = "PackageFamily";
          str4 = "PackageFamilyName";
          str5 = "family";
          break;
        case "-package":
          Assassin.PackageFilterList.Add(args[1]);
          str3 = "Package";
          str4 = "PackageFullName";
          str5 = "package";
          break;
        case "-app":
          str3 = "Application";
          str4 = "ApplicationUserModelID";
          str5 = "application";
          break;
        default:
          try
          {
            Triggers.Restore();
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nFatal Error: " + ex.Message);
          }
          Console.WriteLine("\r\nFatal Error: Unable to resolve arguments.");
          Environment.Exit(3);
          break;
      }
      try
      {
        Program.ActiveTransaction = new Initializer.Transaction("SELECT \"_" + str3 + "ID\",\"" + str4 + "\" FROM \"" + str3 + "\" WHERE \"" + str4 + "\" LIKE \"" + args[1].Replace('*', '%') + "\"");
      }
      catch (Exception ex3)
      {
        try
        {
          Program.ActiveTransaction.Abort();
          Triggers.Restore();
        }
        catch (Exception ex4)
        {
          Console.WriteLine("\r\nFatal Error: " + ex4.Message);
        }
        Console.WriteLine("\r\nError: Could not initiate transaction.\r\nException: " + ex3.Message);
        Environment.Exit(5);
      }
      Program.PackageIdList = new List<string>();
      Program.ApplicationPackageList = new List<string>();
      Program.ApplicationPackageDirList = new List<string>();
      List<string> stringList = new List<string>();
      bool flag = false;
      bool app = false;
      try
      {
        SqliteDataReader sqliteDataReader1 = Program.ActiveTransaction.Command.ExecuteReader();
        while (sqliteDataReader1.Read())
        {
          flag = true;
          string str6 = sqliteDataReader1.GetString(0);
          if (str3 == "Package")
            Program.PackageIdList.Add(str6);
          if (str3 == "Application")
          {
            app = true;
            SqliteDataReader sqliteDataReader2 = Program.ActiveTransaction.NewCommand("SELECT \"Package\" FROM \"" + str3 + "\" WHERE \"_ApplicationID\" = \"" + str6 + "\"").ExecuteReader();
            while (sqliteDataReader2.Read())
            {
              SqliteDataReader sqliteDataReader3 = Program.ActiveTransaction.NewCommand("SELECT \"PackageFullName\",\"_PackageID\" FROM \"Package\" WHERE \"_PackageID\" = \"" + sqliteDataReader2.GetString(0) + "\"").ExecuteReader();
              while (sqliteDataReader3.Read())
              {
                Program.ApplicationPackageList.Add(sqliteDataReader3.GetString(0));
                stringList.Add(sqliteDataReader3.GetString(1));
              }
              SqliteDataReader sqliteDataReader4 = Program.ActiveTransaction.NewCommand("SELECT \"InstalledLocation\" FROM \"PackageLocation\" WHERE \"Package\" = \"" + sqliteDataReader2.GetString(0) + "\"").ExecuteReader();
              while (sqliteDataReader4.Read())
                Program.ApplicationPackageDirList.Add(sqliteDataReader4.GetString(0));
            }
          }
          Console.WriteLine("\r\nRemoving " + str5 + " " + sqliteDataReader1.GetString(1) + " from machine database...");
          Assassin.MachineData("_" + str3 + "ID", str6, app);
          try
          {
            Program.ActiveTransaction.NewCommand("DELETE FROM \"" + str3 + "\" WHERE \"_" + str3 + "ID\" = \"" + str6 + "\"").ExecuteNonQuery();
          }
          catch (Exception ex5)
          {
            try
            {
              Program.ActiveTransaction.Abort();
              Triggers.Restore();
            }
            catch (Exception ex6)
            {
              Console.WriteLine("\r\nFatal Error: " + ex6.Message);
            }
            Console.WriteLine("\r\nError: Could not remove specified " + str5 + " from machine database.\r\nException: " + ex5.Message);
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Could not remove\n" + str5 + " " + args[1] + ".\r\nException: " + ex.Message);
      }
      if (!flag)
        Console.WriteLine("\r\nCould not find any data with the matching criteria.");
      try
      {
        Program.ActiveTransaction.Commit();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Could not commit database changes.\r\nException: " + ex.Message);
      }
      Console.WriteLine("\r\nRestoring triggers...");
      try
      {
        Triggers.Restore();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nFatal Error: " + ex.Message);
      }
      Initializer.ActiveLink.Close();
      SqliteConnection.ClearAllPools();
      try
      {
        Initializer.Hook(Environment.GetEnvironmentVariable("PROGRAMDATA") + "\\Microsoft\\Windows\\AppRepository\\StateRepository-Deployment.srd");
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Could not connect to deployment database.\r\nException: " + ex.Message);
        goto label_138;
      }
      try
      {
        Triggers.Save();
        Triggers.Drop();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: " + ex.Message);
        goto label_138;
      }
      Program.TableList = new List<string>();
      try
      {
        Program.ActiveTransaction = new Initializer.Transaction("SELECT \"Name\" FROM main.sqlite_master WHERE type = \"table\"");
        SqliteDataReader sqliteDataReader = Program.ActiveTransaction.Command.ExecuteReader();
        while (sqliteDataReader.Read())
        {
          string str7 = sqliteDataReader.GetString(0);
          Program.TableList.Add(str7);
        }
      }
      catch (Exception ex7)
      {
        try
        {
          Program.ActiveTransaction.Abort();
          Triggers.Restore();
        }
        catch (Exception ex8)
        {
          Console.WriteLine("\r\nError: " + ex8.Message);
          goto label_138;
        }
        Console.WriteLine("\r\nError: Could not fetch tables from database.\r\nException: " + ex7.Message);
        goto label_138;
      }
      if (app && Program.TableList.Contains("AppxManifest"))
      {
        foreach (string str8 in stringList)
        {
          try
          {
            Program.ActiveTransaction.DisableForeignKeys();
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not disable foreign keys.\r\nException: " + ex.Message);
          }
          if (Program.Verbose)
            Console.WriteLine("\r\nRemoving application package " + str8 + " XML data from deployment database...");
          try
          {
            Program.ActiveTransaction.NewCommand("UPDATE \"AppxManifest\" SET \"Xml\" = \"00\" WHERE \"Package\" = \"" + str8 + "\"").ExecuteNonQuery();
          }
          catch (Exception ex)
          {
            Console.WriteLine("\r\nError: Could not remove application package xml data fromm deployment database.\r\nException: " + ex.Message);
          }
        }
      }
      foreach (string packageId in Program.PackageIdList)
      {
        try
        {
          Program.ActiveTransaction.DisableForeignKeys();
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not disable foreign keys.\r\nException: " + ex.Message);
        }
        if (Program.Verbose)
          Console.WriteLine("\r\nRemoving package value " + packageId + " from deployment database...");
        foreach (string table in Program.TableList)
        {
          if (table == "ContentGroup")
          {
            try
            {
              SqliteDataReader sqliteDataReader = Program.ActiveTransaction.NewCommand("SELECT \"Package\" FROM \"" + table + "\" WHERE \"Package\" = \"" + packageId + "\"").ExecuteReader();
              while (sqliteDataReader.Read())
                Assassin.ManualData("Package", packageId, table);
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: " + ex.Message);
            }
          }
          else
          {
            try
            {
              if (Program.Verbose)
                Console.WriteLine("\r\nRemoving row with package ID " + packageId + " from deployment table " + table + "...");
              Program.ActiveTransaction.NewCommand("DELETE FROM \"" + table + "\" WHERE \"Package\" = \"" + packageId + "\"").ExecuteNonQuery();
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not remove package " + packageId + " from deployment table " + table + ".\r\nException: " + ex.Message);
            }
          }
        }
      }
      try
      {
        Program.ActiveTransaction.Commit();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nError: Could not commit deployment database changes.\r\nException: " + ex.Message);
      }
      Initializer.ActiveLink.Close();
      SqliteConnection.ClearAllPools();
label_138:
      if (!((IEnumerable<string>) args).Contains<string>("-Unregister", (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
      {
        switch (str3)
        {
          case "PackageFamily":
            foreach (string packageFilter in Assassin.PackageFilterList)
            {
              try
              {
                Assassin.Files(packageFilter);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove files belonging to package " + packageFilter + ".\r\nException: " + ex.Message);
              }
              try
              {
                Assassin.RegistryKeys(packageFilter);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove registry keys belonging to package " + packageFilter + ".\r\nException: " + ex.Message);
              }
            }
            foreach (string packageDirectory in Assassin.PackageDirectoryList)
            {
              try
              {
                Assassin.FilterDelete(Directory.GetParent(packageDirectory).FullName, Path.GetFileName(packageDirectory));
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove package directory " + packageDirectory + ".\r\nException: " + ex.Message);
              }
            }
            try
            {
              Assassin.RegistryKeys(args[1], family: true);
              break;
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not remove registry keys belonging to family " + args[0] + ".\r\nException: " + ex.Message);
              break;
            }
          case "Package":
            foreach (string packageFilter in Assassin.PackageFilterList)
            {
              try
              {
                Assassin.Files(packageFilter);
                Assassin.Files(args[1]);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove files belonging to package " + packageFilter + ".\r\nException: " + ex.Message);
              }
              try
              {
                Assassin.RegistryKeys(packageFilter);
                Assassin.Files(args[1]);
              }
              catch (Exception ex)
              {
                Console.WriteLine("\r\nError: Could not remove registry keys belonging to package " + packageFilter + ".\r\nException: " + ex.Message);
              }
            }
            using (List<string>.Enumerator enumerator = Assassin.PackageDirectoryList.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                string current = enumerator.Current;
                try
                {
                  Assassin.FilterDelete(Directory.GetParent(current).FullName, Path.GetFileName(current));
                }
                catch (Exception ex)
                {
                  Console.WriteLine("\r\nError: Could not remove package directory " + current + ".\r\nException: " + ex.Message);
                }
              }
              break;
            }
          case "Application":
            try
            {
              Assassin.Files("|NONE|", true);
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not remove files belonging to application " + args[1] + ".\r\nException: " + ex.Message);
            }
            try
            {
              Assassin.RegistryKeys(args[1], true);
              break;
            }
            catch (Exception ex)
            {
              Console.WriteLine("\r\nError: Could not remove registry keys belonging to application " + args[1] + ".\r\nException: " + ex.Message);
              break;
            }
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
      Environment.Exit(0);
    }
  }
}
