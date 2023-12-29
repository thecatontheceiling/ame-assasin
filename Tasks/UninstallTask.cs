// Decompiled with JetBrains decompiler
// Type: TrustedUninstaller.Shared.Tasks.UninstallTask
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace TrustedUninstaller.Shared.Tasks
{
  public class UninstallTask
  {
    public 
    #nullable disable
    string Title { get; set; }

    public 
    #nullable enable
    string? Description { get; set; }

    public int? MinVersion { get; set; }

    public int? MaxVersion { get; set; }

    public UninstallTaskStatus Status { get; set; } = UninstallTaskStatus.ToDo;

    public 
    #nullable disable
    List<ITaskAction> Actions { get; set; }

    public int Priority { get; set; } = 1;

    public UninstallTaskPrivilege Privilege { get; set; }

    public List<string> Features { get; set; } = new List<string>();

    public void Update()
    {
      List<UninstallTaskStatus> list = this.Actions.Select<ITaskAction, UninstallTaskStatus>((Func<ITaskAction, UninstallTaskStatus>) (entry => entry.GetStatus())).ToList<UninstallTaskStatus>();
      if (list.Any<UninstallTaskStatus>((Func<UninstallTaskStatus, bool>) (entry => entry == UninstallTaskStatus.InProgress)))
        this.Status = UninstallTaskStatus.InProgress;
      else if (list.All<UninstallTaskStatus>((Func<UninstallTaskStatus, bool>) (entry => entry == UninstallTaskStatus.Completed)))
        this.Status = UninstallTaskStatus.Completed;
      else
        this.Status = UninstallTaskStatus.ToDo;
    }
  }
}
