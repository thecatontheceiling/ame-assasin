﻿// Decompiled with JetBrains decompiler
// Type: TrustedUninstaller.Shared.Tasks.ITaskAction
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

#nullable disable
namespace TrustedUninstaller.Shared.Tasks
{
  public interface ITaskAction
  {
    int GetProgressWeight();

    void ResetProgress();

    string ErrorString();

    UninstallTaskStatus GetStatus();

    void RunTask();
  }
}
