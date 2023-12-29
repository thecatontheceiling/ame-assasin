// Decompiled with JetBrains decompiler
// Type: ame_assassin.Initializer
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using Microsoft.Data.Sqlite;
using System;
using System.IO;

#nullable disable
namespace ame_assassin
{
  internal static class Initializer
  {
    public static SqliteConnection ActiveLink;

    public static void Hook(string database, bool disableConstraints = false)
    {
      if (!File.Exists(database))
      {
        Console.WriteLine("\r\nError: Machine database was unable to be located.");
        Environment.Exit(2);
      }
      try
      {
        Initializer.ActiveLink = new SqliteConnection("Data Source='" + database + "';");
        Initializer.ActiveLink.Open();
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\nFatal Error: Could not connect to machine database.\r\nException: " + ex.Message);
        Environment.Exit(3);
      }
    }

    public class Transaction
    {
      private readonly SqliteTransaction transaction;
      public SqliteCommand Command;
      private readonly bool keysDisabled;

      public Transaction(string text = null, bool fkeysDisabled = false)
      {
        if (fkeysDisabled)
          this.keysDisabled = true;
        this.transaction = Initializer.ActiveLink.BeginTransaction();
        this.Command = Initializer.ActiveLink.CreateCommand();
        this.Command.Transaction = this.transaction;
        if (text == null)
          return;
        this.Command.CommandText = text;
      }

      public SqliteCommand NewCommand(string text)
      {
        SqliteCommand command = Initializer.ActiveLink.CreateCommand();
        command.Transaction = this.transaction;
        command.CommandText = text;
        return command;
      }

      public void Commit() => this.transaction.Commit();

      public void Abort()
      {
        if (this.transaction.Connection == null)
          return;
        this.transaction.Rollback();
        this.transaction.Commit();
        this.transaction.Dispose();
      }

      public void DisableForeignKeys()
      {
        if (this.keysDisabled)
          return;
        try
        {
          SqliteDataReader sqliteDataReader = this.NewCommand("SELECT \"foreign_keys\" FROM PRAGMA_foreign_keys").ExecuteReader();
          while (sqliteDataReader.Read())
          {
            if (sqliteDataReader.GetString(0) == "1")
            {
              if (Program.Verbose)
                Console.WriteLine("\r\nDisabling foreign keys...");
              this.transaction.Commit();
              try
              {
                new SqliteCommand("PRAGMA foreign_keys=off", Initializer.ActiveLink).ExecuteNonQuery();
              }
              catch (Exception ex)
              {
                Program.ActiveTransaction = new Initializer.Transaction();
                Console.WriteLine("\r\nError: Could not disable foreign keys.\r\nException: " + ex.Message);
              }
              Program.ActiveTransaction = new Initializer.Transaction(fkeysDisabled: true);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("\r\nError: Could not query foreign key setting.\r\nException: " + ex.Message);
        }
      }
    }
  }
}
