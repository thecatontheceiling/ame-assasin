// Decompiled with JetBrains decompiler
// Type: ame_assassin.ScheduledTaskAction
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using Microsoft.Win32.TaskScheduler;
using System;
using System.Linq;
using TrustedUninstaller.Shared.Tasks;

#nullable enable
namespace ame_assassin
{
  internal class ScheduledTaskAction : ITaskAction
  {
    public void RunTaskOnMainThread() => throw new NotImplementedException();

    public ScheduledTaskOperation Operation { get; set; }

    public string? RawTask { get; set; }

    public 
    #nullable disable
    string Path { get; set; }

    public int ProgressWeight { get; set; } = 1;

    public int GetProgressWeight() => this.ProgressWeight;

    private bool InProgress { get; set; }

    public void ResetProgress() => this.InProgress = false;

    public string ErrorString()
    {
      return "ScheduledTaskAction failed to change task " + this.Path + " to state " + this.Operation.ToString();
    }

    public UninstallTaskStatus GetStatus()
    {
      if (this.InProgress)
        return UninstallTaskStatus.InProgress;
      using (TaskService taskService = new TaskService())
      {
        if (this.Operation != ScheduledTaskOperation.DeleteFolder)
        {
          Task task = taskService.GetTask(this.Path);
          if (task == null)
            return this.Operation == ScheduledTaskOperation.Delete ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo;
          return task.Enabled ? (this.Operation == ScheduledTaskOperation.Enable ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo) : (this.Operation == ScheduledTaskOperation.Disable ? UninstallTaskStatus.Completed : UninstallTaskStatus.ToDo);
        }
        TaskFolder folder = taskService.GetFolder(this.Path);
        return folder == null ? UninstallTaskStatus.Completed : (folder.GetTasks().Any<Task>() ? UninstallTaskStatus.ToDo : UninstallTaskStatus.Completed);
      }
    }

    public void RunTask()
    {
      if (this.GetStatus() == UninstallTaskStatus.Completed)
        return;
      ScheduledTaskOperation operation = this.Operation;
      Console.WriteLine(operation.ToString().TrimEnd('e') + "ing scheduled task '" + this.Path + "'...");
      using (TaskService taskService = new TaskService())
      {
        this.InProgress = true;
        if (this.Operation != ScheduledTaskOperation.DeleteFolder)
        {
          Task task = taskService.GetTask(this.Path);
          if (task == null && (this.Operation == ScheduledTaskOperation.Delete || this.RawTask == null || this.RawTask.Length == 0))
            return;
          operation = this.Operation;
          switch (operation)
          {
            case ScheduledTaskOperation.Delete:
              taskService.RootFolder.DeleteTask(this.Path);
              break;
            case ScheduledTaskOperation.Enable:
            case ScheduledTaskOperation.Disable:
              if (task == null && this.RawTask != null)
                task = taskService.RootFolder.RegisterTask(this.Path, this.RawTask);
              if (task == null)
                throw new ArgumentException("Task provided is null.");
              task.Enabled = this.Operation == ScheduledTaskOperation.Enable;
              break;
            default:
              throw new ArgumentException("Argument out of range.");
          }
          this.InProgress = false;
        }
        else
        {
          TaskFolder folder = taskService.GetFolder(this.Path);
          if (folder == null)
            return;
          folder.GetTasks().ToList<Task>().ForEach((Action<Task>) (x => folder.DeleteTask(x.Name)));
          try
          {
            folder.Parent.DeleteFolder(folder.Name);
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
          }
          this.InProgress = false;
        }
      }
    }
  }
}
