// Decompiled with JetBrains decompiler
// Type: ame_assassin.FileLock
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#nullable disable
namespace ame_assassin
{
  public static class FileLock
  {
    public static bool HasKilledExplorer;
    private const int RmRebootReasonNone = 0;
    private const int CCH_RM_MAX_APP_NAME = 255;
    private const int CCH_RM_MAX_SVC_NAME = 63;

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmRegisterResources(
      uint pSessionHandle,
      uint nFiles,
      string[] rgsFilenames,
      uint nApplications,
      [In] FileLock.RM_UNIQUE_PROCESS[] rgApplications,
      uint nServices,
      string[] rgsServiceNames);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
    private static extern int RmStartSession(
      out uint pSessionHandle,
      int dwSessionFlags,
      string strSessionKey);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmEndSession(uint pSessionHandle);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmGetList(
      uint dwSessionHandle,
      out uint pnProcInfoNeeded,
      ref uint pnProcInfo,
      [In, Out] FileLock.RM_PROCESS_INFO[] rgAffectedApps,
      ref uint lpdwRebootReasons);

    public static List<System.Diagnostics.Process> WhoIsLocking(string path)
    {
      string strSessionKey = Guid.NewGuid().ToString();
      List<System.Diagnostics.Process> processList = new List<System.Diagnostics.Process>();
      uint pSessionHandle;
      if (FileLock.RmStartSession(out pSessionHandle, 0, strSessionKey) != 0)
        throw new Exception("Could not begin restart session.  Unable to determine file locker.");
      try
      {
        uint pnProcInfoNeeded = 0;
        uint pnProcInfo1 = 0;
        uint lpdwRebootReasons = 0;
        string[] rgsFilenames = new string[1]{ path };
        if (FileLock.RmRegisterResources(pSessionHandle, (uint) rgsFilenames.Length, rgsFilenames, 0U, (FileLock.RM_UNIQUE_PROCESS[]) null, 0U, (string[]) null) != 0)
          throw new Exception("Could not register resource.");
        int list1 = FileLock.RmGetList(pSessionHandle, out pnProcInfoNeeded, ref pnProcInfo1, (FileLock.RM_PROCESS_INFO[]) null, ref lpdwRebootReasons);
        switch (list1)
        {
          case 0:
            break;
          case 234:
            FileLock.RM_PROCESS_INFO[] rgAffectedApps = new FileLock.RM_PROCESS_INFO[(int) pnProcInfoNeeded + 3];
            uint pnProcInfo2 = pnProcInfoNeeded;
            int list2 = FileLock.RmGetList(pSessionHandle, out pnProcInfoNeeded, ref pnProcInfo2, rgAffectedApps, ref lpdwRebootReasons);
            if (list2 != 0)
              throw new Exception("Could not list processes locking resource: " + list2.ToString());
            processList = new List<System.Diagnostics.Process>((int) pnProcInfo2);
            for (int index = 0; (long) index < (long) pnProcInfo2; ++index)
            {
              try
              {
                processList.Add(System.Diagnostics.Process.GetProcessById(rgAffectedApps[index].Process.dwProcessId));
              }
              catch (ArgumentException ex)
              {
              }
            }
            break;
          default:
            throw new Exception("Could not list processes locking resource. Could not get size of result." + string.Format(" Result value: {0}", (object) list1));
        }
      }
      finally
      {
        FileLock.RmEndSession(pSessionHandle);
      }
      return processList;
    }

    private struct RM_UNIQUE_PROCESS
    {
      public readonly int dwProcessId;
      public readonly System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    private enum RM_APP_TYPE
    {
      RmUnknownApp = 0,
      RmMainWindow = 1,
      RmOtherWindow = 2,
      RmService = 3,
      RmExplorer = 4,
      RmConsole = 5,
      RmCritical = 1000, // 0x000003E8
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RM_PROCESS_INFO
    {
      public readonly FileLock.RM_UNIQUE_PROCESS Process;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public readonly string strAppName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public readonly string strServiceShortName;
      public readonly FileLock.RM_APP_TYPE ApplicationType;
      public readonly uint AppStatus;
      public readonly uint TSSessionId;
      [MarshalAs(UnmanagedType.Bool)]
      public readonly bool bRestartable;
    }
  }
}
