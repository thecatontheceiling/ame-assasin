// Decompiled with JetBrains decompiler
// Type: ame_assassin.Triggers
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

#nullable disable
namespace ame_assassin
{
  internal static class Triggers
  {
    private static List<string> TriggerSqlList;

    public static void Save(string database = null)
    {
      Triggers.TriggerSqlList = new List<string>();
      if (Initializer.ActiveLink == null && database == null)
        throw new Exception("Could not save triggers. Function supplied with missing parameters.");
      if (Initializer.ActiveLink == null)
        Initializer.Hook(database);
      Initializer.Transaction transaction = new Initializer.Transaction("SELECT \"sql\" FROM main.sqlite_master WHERE \"type\" = \"trigger\"");
      SqliteDataReader sqliteDataReader = transaction.Command.ExecuteReader();
      while (sqliteDataReader.Read())
      {
        string str = sqliteDataReader.GetString(0);
        Triggers.TriggerSqlList.Add(str);
      }
      transaction.Commit();
    }

    public static void Drop(string database = null)
    {
      if (Initializer.ActiveLink == null && database == null)
        throw new Exception("Could not drop triggers. Function supplied with missing parameters.");
      if (Initializer.ActiveLink == null)
        Initializer.Hook(database);
      Initializer.Transaction transaction = new Initializer.Transaction("SELECT \"name\" FROM main.sqlite_master WHERE \"type\" = \"trigger\"");
      SqliteDataReader sqliteDataReader = transaction.Command.ExecuteReader();
      while (sqliteDataReader.Read())
      {
        string str = sqliteDataReader.GetString(0);
        transaction.NewCommand("DROP TRIGGER " + str).ExecuteNonQuery();
      }
      transaction.Commit();
    }

    public static void Restore(string database = null)
    {
      if (Initializer.ActiveLink == null && database == null)
        throw new Exception("Could not restore triggers. Function supplied with missing parameters.");
      if (Initializer.ActiveLink == null)
        Initializer.Hook(database);
      if (Triggers.TriggerSqlList == null)
        throw new Exception("Could not restore triggers. Function supplied with missing parameters.");
      Initializer.Transaction transaction = new Initializer.Transaction("SELECT \"sql\" FROM main.sqlite_master WHERE \"type\" = \"trigger\"");
      foreach (string triggerSql in Triggers.TriggerSqlList)
        transaction.NewCommand(triggerSql).ExecuteNonQuery();
      transaction.Commit();
    }
  }
}
