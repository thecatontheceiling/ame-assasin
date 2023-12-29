// Decompiled with JetBrains decompiler
// Type: ame_assassin.Win32
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using TrustedUninstaller.Shared.Tasks;

#nullable disable
namespace ame_assassin
{
  public static class Win32
  {
    public const int STATUS_SUCCESS = 0;
    public static readonly int STATUS_INFO_LENGTH_MISMATCH = Convert.ToInt32("0xC0000004", 16);
    public const int ERROR_BAD_LENGTH = 24;
    public const int ERROR_INSUFFICIENT_BUFFER = 122;
    public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr LocalFree(IntPtr hMem);

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    public static class Service
    {
      [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern IntPtr CreateService(
        IntPtr hSCManager,
        string lpServiceName,
        string lpDisplayName,
        uint dwDesiredAccess,
        uint dwServiceType,
        uint dwStartType,
        uint dwErrorControl,
        string lpBinaryPathName,
        [Optional] string lpLoadOrderGroup,
        [Optional] string lpdwTagId,
        [Optional] string lpDependencies,
        [Optional] string lpServiceStartName,
        [Optional] string lpPassword);

      [DllImport("advapi32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool DeleteService(IntPtr hService);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern IntPtr OpenService(
        IntPtr hSCManager,
        string lpServiceName,
        ame_assassin.Win32.Service.SERVICE_ACCESS dwDesiredAccess);

      [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", CharSet = CharSet.Unicode, SetLastError = true)]
      public static extern IntPtr OpenSCManager(
        string machineName,
        string databaseName,
        ame_assassin.Win32.Service.SCM_ACCESS dwDesiredAccess);

      [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      public static extern bool QueryServiceStatusEx(
        IntPtr Service,
        int InfoLevel,
        ref ame_assassin.Win32.Service.SERVICE_STATUS_PROCESS ServiceStatus,
        int BufSize,
        out int BytesNeeded);

      [DllImport("advapi32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool CloseServiceHandle(IntPtr hSCObject);

      [DllImport("Advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      public static extern bool EnumServicesStatusEx(
        IntPtr hSCManager,
        uint InfoLevel,
        ame_assassin.Win32.Service.SERVICE_TYPE dwServiceType,
        uint dwServiceState,
        IntPtr lpServices,
        int cbBufSize,
        out int pcbBytesNeeded,
        out int lpServicesReturned,
        ref int lpResumeHandle,
        string pszGroupName);

      public struct ENUM_SERVICE_STATUS_PROCESS
      {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpServiceName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpDisplayName;
        public ame_assassin.Win32.Service.SERVICE_STATUS_PROCESS ServiceStatusProcess;
      }

      public struct SERVICE_STATUS_PROCESS
      {
        public ame_assassin.Win32.Service.SERVICE_TYPE ServiceType;
        public ame_assassin.Win32.Service.SERVICE_STATE CurrentState;
        public ame_assassin.Win32.Service.SERVICE_ACCEPT ControlsAccepted;
        public int Win32ExitCode;
        public int ServiceSpecificExitCode;
        public int CheckPoint;
        public int WaitHint;
        public int ProcessID;
        public ame_assassin.Win32.Service.SERVICE_FLAGS ServiceFlags;
      }

      public enum SERVICE_STATE
      {
        Stopped = 1,
        StartPending = 2,
        StopPending = 3,
        Running = 4,
        ContinuePending = 5,
        PausePending = 6,
        Paused = 7,
      }

      public enum SERVICE_ACCEPT
      {
        Stop = 1,
        PauseContinue = 2,
        Shutdown = 4,
        ParamChange = 8,
        NetBindChange = 16, // 0x00000010
        HardwareProfileChange = 32, // 0x00000020
        PowerEvent = 64, // 0x00000040
        SessionChange = 128, // 0x00000080
        PreShutdown = 256, // 0x00000100
      }

      public enum SERVICE_FLAGS
      {
        None,
        RunsInSystemProcess,
      }

      [Flags]
      public enum SERVICE_TYPE : uint
      {
        SERVICE_KERNEL_DRIVER = 1,
        SERVICE_FILE_SYSTEM_DRIVER = 2,
        SERVICE_WIN32_OWN_PROCESS = 16, // 0x00000010
        SERVICE_WIN32_SHARE_PROCESS = 32, // 0x00000020
        SERVICE_INTERACTIVE_PROCESS = 256, // 0x00000100
      }

      public enum SERVICE_START : uint
      {
        SERVICE_BOOT_START,
        SERVICE_SYSTEM_START,
        SERVICE_AUTO_START,
        SERVICE_DEMAND_START,
        SERVICE_DISABLED,
      }

      public enum SERVICE_ERROR
      {
        SERVICE_ERROR_IGNORE,
        SERVICE_ERROR_NORMAL,
        SERVICE_ERROR_SEVERE,
        SERVICE_ERROR_CRITICAL,
      }

      [Flags]
      public enum SERVICE_ACCESS : uint
      {
        STANDARD_RIGHTS_REQUIRED = 983040, // 0x000F0000
        SERVICE_QUERY_CONFIG = 1,
        SERVICE_CHANGE_CONFIG = 2,
        SERVICE_QUERY_STATUS = 4,
        SERVICE_ENUMERATE_DEPENDENTS = 8,
        SERVICE_START = 16, // 0x00000010
        SERVICE_STOP = 32, // 0x00000020
        SERVICE_PAUSE_CONTINUE = 64, // 0x00000040
        SERVICE_INTERROGATE = 128, // 0x00000080
        SERVICE_USER_DEFINED_CONTROL = 256, // 0x00000100
        SERVICE_ALL_ACCESS = SERVICE_USER_DEFINED_CONTROL | SERVICE_INTERROGATE | SERVICE_PAUSE_CONTINUE | SERVICE_STOP | SERVICE_START | SERVICE_ENUMERATE_DEPENDENTS | SERVICE_QUERY_STATUS | SERVICE_CHANGE_CONFIG | SERVICE_QUERY_CONFIG | STANDARD_RIGHTS_REQUIRED, // 0x000F01FF
      }

      [Flags]
      public enum SCM_ACCESS : uint
      {
        STANDARD_RIGHTS_REQUIRED = 983040, // 0x000F0000
        SC_MANAGER_CONNECT = 1,
        SC_MANAGER_CREATE_SERVICE = 2,
        SC_MANAGER_ENUMERATE_SERVICE = 4,
        SC_MANAGER_LOCK = 8,
        SC_MANAGER_QUERY_LOCK_STATUS = 16, // 0x00000010
        SC_MANAGER_MODIFY_BOOT_CONFIG = 32, // 0x00000020
        SC_MANAGER_ALL_ACCESS = SC_MANAGER_MODIFY_BOOT_CONFIG | SC_MANAGER_QUERY_LOCK_STATUS | SC_MANAGER_LOCK | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_CREATE_SERVICE | SC_MANAGER_CONNECT | STANDARD_RIGHTS_REQUIRED, // 0x000F003F
      }
    }

    public static class ServiceEx
    {
      public static bool IsPendingDeleteOrDeleted(string serviceName)
      {
        IntPtr num = ame_assassin.Win32.Service.OpenSCManager((string) null, (string) null, ame_assassin.Win32.Service.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
        if (num == IntPtr.Zero)
        {
          if (new RegistryValueAction()
          {
            KeyName = ("HKLM\\SYSTEM\\CurrentControlSet\\Services\\" + serviceName),
            Value = "DeleteFlag",
            Type = RegistryValueType.REG_DWORD,
            Data = ((object) 1)
          }.GetStatus() == UninstallTaskStatus.Completed)
            return true;
          return new RegistryKeyAction()
          {
            KeyName = ("HKLM\\SYSTEM\\CurrentControlSet\\Services\\" + serviceName),
            Operation = RegistryKeyOperation.Delete
          }.GetStatus() == UninstallTaskStatus.Completed;
        }
        IntPtr service = ame_assassin.Win32.Service.CreateService(num, serviceName, "AME Deletion Check", 983551U, 16U, 4U, 0U, "ame-deletion-check");
        if (service == IntPtr.Zero)
        {
          if (Marshal.GetLastWin32Error() == 1072)
            return true;
          if (new RegistryValueAction()
          {
            KeyName = ("HKLM\\SYSTEM\\CurrentControlSet\\Services\\" + serviceName),
            Value = "DeleteFlag",
            Type = RegistryValueType.REG_DWORD,
            Data = ((object) 1)
          }.GetStatus() == UninstallTaskStatus.Completed)
            return true;
          return new RegistryKeyAction()
          {
            KeyName = ("HKLM\\SYSTEM\\CurrentControlSet\\Services\\" + serviceName),
            Operation = RegistryKeyOperation.Delete
          }.GetStatus() == UninstallTaskStatus.Completed;
        }
        ame_assassin.Win32.Service.DeleteService(service);
        ame_assassin.Win32.Service.CloseServiceHandle(service);
        ame_assassin.Win32.Service.CloseServiceHandle(num);
        return true;
      }

      public static bool IsPendingStopOrStopped(string serviceName, out bool pending)
      {
        pending = false;
        IntPtr num1 = ame_assassin.Win32.Service.OpenSCManager((string) null, (string) null, ame_assassin.Win32.Service.SCM_ACCESS.SC_MANAGER_ENUMERATE_SERVICE);
        if (num1 == IntPtr.Zero)
          return false;
        IntPtr num2 = ame_assassin.Win32.Service.OpenService(num1, serviceName, ame_assassin.Win32.Service.SERVICE_ACCESS.SERVICE_QUERY_STATUS);
        if (num2 == IntPtr.Zero)
          return true;
        ame_assassin.Win32.Service.SERVICE_STATUS_PROCESS ServiceStatus = new ame_assassin.Win32.Service.SERVICE_STATUS_PROCESS();
        if (!ame_assassin.Win32.Service.QueryServiceStatusEx(num2, 0, ref ServiceStatus, Marshal.SizeOf<ame_assassin.Win32.Service.SERVICE_STATUS_PROCESS>(ServiceStatus), out int _))
        {
          ame_assassin.Win32.Service.CloseServiceHandle(num2);
          ame_assassin.Win32.Service.CloseServiceHandle(num1);
          return false;
        }
        ame_assassin.Win32.Service.CloseServiceHandle(num2);
        ame_assassin.Win32.Service.CloseServiceHandle(num1);
        pending = ServiceStatus.CurrentState == ame_assassin.Win32.Service.SERVICE_STATE.StopPending;
        return pending || ServiceStatus.CurrentState == ame_assassin.Win32.Service.SERVICE_STATE.Stopped;
      }

      public static int GetServiceProcessId(string serviceName)
      {
        IntPtr num1 = ame_assassin.Win32.Service.OpenSCManager((string) null, (string) null, ame_assassin.Win32.Service.SCM_ACCESS.SC_MANAGER_ENUMERATE_SERVICE);
        IntPtr num2 = !(num1 == IntPtr.Zero) ? ame_assassin.Win32.Service.OpenService(num1, serviceName, ame_assassin.Win32.Service.SERVICE_ACCESS.SERVICE_QUERY_STATUS) : throw new Exception("Error opening service manager.");
        ame_assassin.Win32.Service.SERVICE_STATUS_PROCESS ServiceStatus = new ame_assassin.Win32.Service.SERVICE_STATUS_PROCESS();
        if (!ame_assassin.Win32.Service.QueryServiceStatusEx(num2, 0, ref ServiceStatus, Marshal.SizeOf<ame_assassin.Win32.Service.SERVICE_STATUS_PROCESS>(ServiceStatus), out int _))
        {
          ame_assassin.Win32.Service.CloseServiceHandle(num2);
          ame_assassin.Win32.Service.CloseServiceHandle(num1);
          throw new Exception("Error querying service ProcessId: " + Marshal.GetLastWin32Error().ToString());
        }
        ame_assassin.Win32.Service.CloseServiceHandle(num2);
        ame_assassin.Win32.Service.CloseServiceHandle(num1);
        return ServiceStatus.ProcessID != 0 ? ServiceStatus.ProcessID : -1;
      }

      public static IEnumerable<string> GetServicesFromProcessId(int processId)
      {
        return ((IEnumerable<ame_assassin.Win32.Service.ENUM_SERVICE_STATUS_PROCESS>) ame_assassin.Win32.ServiceEx.GetServices(ame_assassin.Win32.Service.SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS | ame_assassin.Win32.Service.SERVICE_TYPE.SERVICE_WIN32_SHARE_PROCESS)).Where<ame_assassin.Win32.Service.ENUM_SERVICE_STATUS_PROCESS>((Func<ame_assassin.Win32.Service.ENUM_SERVICE_STATUS_PROCESS, bool>) (x => x.ServiceStatusProcess.ProcessID == processId)).Select<ame_assassin.Win32.Service.ENUM_SERVICE_STATUS_PROCESS, string>((Func<ame_assassin.Win32.Service.ENUM_SERVICE_STATUS_PROCESS, string>) (x => x.lpServiceName));
      }

      private static ame_assassin.Win32.Service.ENUM_SERVICE_STATUS_PROCESS[] GetServices(
        ame_assassin.Win32.Service.SERVICE_TYPE serviceTypes)
      {
        IntPtr hSCManager = ame_assassin.Win32.Service.OpenSCManager((string) null, (string) null, ame_assassin.Win32.Service.SCM_ACCESS.SC_MANAGER_ENUMERATE_SERVICE);
        if (hSCManager == IntPtr.Zero)
          throw new Exception("Could not open service manager.");
        int pcbBytesNeeded = 0;
        int lpServicesReturned = 0;
        int lpResumeHandle = 0;
        ame_assassin.Win32.Service.EnumServicesStatusEx(hSCManager, 0U, ame_assassin.Win32.Service.SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS | ame_assassin.Win32.Service.SERVICE_TYPE.SERVICE_WIN32_SHARE_PROCESS, 1U, IntPtr.Zero, 0, out pcbBytesNeeded, out lpServicesReturned, ref lpResumeHandle, (string) null);
        IntPtr num1 = Marshal.AllocHGlobal(pcbBytesNeeded);
        ame_assassin.Win32.Service.EnumServicesStatusEx(hSCManager, 0U, ame_assassin.Win32.Service.SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS | ame_assassin.Win32.Service.SERVICE_TYPE.SERVICE_WIN32_SHARE_PROCESS, 1U, num1, pcbBytesNeeded, out pcbBytesNeeded, out lpServicesReturned, ref lpResumeHandle, (string) null);
        ame_assassin.Win32.Service.ENUM_SERVICE_STATUS_PROCESS[] services = new ame_assassin.Win32.Service.ENUM_SERVICE_STATUS_PROCESS[lpServicesReturned];
        int num2 = Marshal.SizeOf(typeof (ame_assassin.Win32.Service.ENUM_SERVICE_STATUS_PROCESS));
        for (int index = 0; index < lpServicesReturned; ++index)
        {
          IntPtr ptr = new IntPtr(num1.ToInt64() + (long) (index * num2));
          services[index] = Marshal.PtrToStructure<ame_assassin.Win32.Service.ENUM_SERVICE_STATUS_PROCESS>(ptr);
        }
        Marshal.FreeHGlobal(num1);
        return services;
      }
    }

    public static class Tokens
    {
      public const byte SECURITY_STATIC_TRACKING = 0;
      public static readonly ame_assassin.Win32.LUID ANONYMOUS_LOGON_LUID = new ame_assassin.Win32.LUID()
      {
        LowPart = 998,
        HighPart = 0
      };
      public static readonly ame_assassin.Win32.LUID SYSTEM_LUID = new ame_assassin.Win32.LUID()
      {
        LowPart = 999,
        HighPart = 0
      };
      public const string SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";
      public const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";
      public const string SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";
      public const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
      public const string SE_MACHINE_ACCOUNT_NAME = "SeMachineAccountPrivilege";
      public const string SE_TCB_NAME = "SeTcbPrivilege";
      public const string SE_SECURITY_NAME = "SeSecurityPrivilege";
      public const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
      public const string SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";
      public const string SE_SYSTEM_PROFILE_NAME = "SeSystemProfilePrivilege";
      public const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";
      public const string SE_PROFILE_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
      public const string SE_INCREASE_BASE_PRIORITY_NAME = "SeIncreaseBasePriorityPrivilege";
      public const string SE_CREATE_PAGEFILE_NAME = "SeCreatePagefilePrivilege";
      public const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";
      public const string SE_BACKUP_NAME = "SeBackupPrivilege";
      public const string SE_RESTORE_NAME = "SeRestorePrivilege";
      public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
      public const string SE_DEBUG_NAME = "SeDebugPrivilege";
      public const string SE_AUDIT_NAME = "SeAuditPrivilege";
      public const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
      public const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";
      public const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";
      public const string SE_UNDOCK_NAME = "SeUndockPrivilege";
      public const string SE_SYNC_AGENT_NAME = "SeSyncAgentPrivilege";
      public const string SE_ENABLE_DELEGATION_NAME = "SeEnableDelegationPrivilege";
      public const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";
      public const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";
      public const string SE_CREATE_GLOBAL_NAME = "SeCreateGlobalPrivilege";
      public const string SE_TRUSTED_CREDMAN_ACCESS_NAME = "SeTrustedCredManAccessPrivilege";
      public const string SE_RELABEL_NAME = "SeRelabelPrivilege";
      public const string SE_INCREASE_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";
      public const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";
      public const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";
      public const string SE_DELEGATE_SESSION_USER_IMPERSONATE_NAME = "SeDelegateSessionUserImpersonatePrivilege";

      [DllImport("advapi32.dll", SetLastError = true)]
      public static extern bool OpenProcessToken(
        IntPtr hProcess,
        ame_assassin.Win32.Tokens.TokenAccessFlags DesiredAccess,
        out ame_assassin.Win32.TokensEx.SafeTokenHandle hToken);

      [DllImport("advapi32.dll", SetLastError = true)]
      public static extern bool GetTokenInformation(
        ame_assassin.Win32.TokensEx.SafeTokenHandle TokenHandle,
        ame_assassin.Win32.Tokens.TOKEN_INFORMATION_CLASS TokenInformationClass,
        IntPtr TokenInformation,
        int TokenInformationLength,
        out int ReturnLength);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool SetTokenInformation(
        ame_assassin.Win32.TokensEx.SafeTokenHandle hToken,
        ame_assassin.Win32.Tokens.TOKEN_INFORMATION_CLASS tokenInfoClass,
        IntPtr pTokenInfo,
        int tokenInfoLength);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool SetTokenInformation(
        ame_assassin.Win32.TokensEx.SafeTokenHandle hToken,
        ame_assassin.Win32.Tokens.TOKEN_INFORMATION_CLASS tokenInfoClass,
        ref uint pTokenInfo,
        int tokenInfoLength);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern bool DuplicateTokenEx(
        ame_assassin.Win32.TokensEx.SafeTokenHandle hExistingToken,
        ame_assassin.Win32.Tokens.TokenAccessFlags dwDesiredAccess,
        IntPtr lpTokenAttributes,
        ame_assassin.Win32.Tokens.SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
        ame_assassin.Win32.Tokens.TOKEN_TYPE TokenType,
        out ame_assassin.Win32.TokensEx.SafeTokenHandle phNewToken);

      [DllImport("advapi32.dll", SetLastError = true)]
      public static extern bool ImpersonateLoggedOnUser(ame_assassin.Win32.TokensEx.SafeTokenHandle hToken);

      [DllImport("advapi32.dll", SetLastError = true)]
      public static extern bool LookupPrivilegeValue(
        IntPtr lpSystemName,
        string lpName,
        out ame_assassin.Win32.LUID lpLuid);

      [DllImport("ntdll.dll", SetLastError = true)]
      public static extern IntPtr RtlAdjustPrivilege(
        ame_assassin.Win32.LUID privilege,
        bool bEnablePrivilege,
        bool isThreadPrivilege,
        out bool previousValue);

      [DllImport("ntdll.dll")]
      public static extern int ZwCreateToken(
        out ame_assassin.Win32.TokensEx.SafeTokenHandle TokenHandle,
        ame_assassin.Win32.Tokens.TokenAccessFlags DesiredAccess,
        ref ame_assassin.Win32.Tokens.OBJECT_ATTRIBUTES ObjectAttributes,
        ame_assassin.Win32.Tokens.TOKEN_TYPE TokenType,
        ref ame_assassin.Win32.LUID AuthenticationId,
        ref ame_assassin.Win32.LARGE_INTEGER ExpirationTime,
        ref ame_assassin.Win32.Tokens.TOKEN_USER TokenUser,
        ref ame_assassin.Win32.Tokens.TOKEN_GROUPS TokenGroups,
        ref ame_assassin.Win32.Tokens.TOKEN_PRIVILEGES TokenPrivileges,
        ref ame_assassin.Win32.Tokens.TOKEN_OWNER TokenOwner,
        ref ame_assassin.Win32.Tokens.TOKEN_PRIMARY_GROUP TokenPrimaryGroup,
        ref ame_assassin.Win32.Tokens.TOKEN_DEFAULT_DACL TokenDefaultDacl,
        ref ame_assassin.Win32.Tokens.TOKEN_SOURCE TokenSource);

      public struct OBJECT_ATTRIBUTES : IDisposable
      {
        public int Length;
        public IntPtr RootDirectory;
        private IntPtr objectName;
        public uint Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;

        public OBJECT_ATTRIBUTES(string name, uint attrs)
        {
          this.Length = 0;
          this.RootDirectory = IntPtr.Zero;
          this.objectName = IntPtr.Zero;
          this.Attributes = attrs;
          this.SecurityDescriptor = IntPtr.Zero;
          this.SecurityQualityOfService = IntPtr.Zero;
          this.Length = Marshal.SizeOf<ame_assassin.Win32.Tokens.OBJECT_ATTRIBUTES>(this);
          this.ObjectName = new ame_assassin.Win32.UNICODE_STRING(name);
        }

        public ame_assassin.Win32.UNICODE_STRING ObjectName
        {
          get
          {
            return (ame_assassin.Win32.UNICODE_STRING) Marshal.PtrToStructure(this.objectName, typeof (ame_assassin.Win32.UNICODE_STRING));
          }
          set
          {
            bool fDeleteOld = this.objectName != IntPtr.Zero;
            if (!fDeleteOld)
              this.objectName = Marshal.AllocHGlobal(Marshal.SizeOf<ame_assassin.Win32.UNICODE_STRING>(value));
            Marshal.StructureToPtr<ame_assassin.Win32.UNICODE_STRING>(value, this.objectName, fDeleteOld);
          }
        }

        public void Dispose()
        {
          if (!(this.objectName != IntPtr.Zero))
            return;
          Marshal.DestroyStructure(this.objectName, typeof (ame_assassin.Win32.UNICODE_STRING));
          Marshal.FreeHGlobal(this.objectName);
          this.objectName = IntPtr.Zero;
        }
      }

      [Flags]
      public enum TokenAccessFlags : uint
      {
        TOKEN_ADJUST_DEFAULT = 128, // 0x00000080
        TOKEN_ADJUST_GROUPS = 64, // 0x00000040
        TOKEN_ADJUST_PRIVILEGES = 32, // 0x00000020
        TOKEN_ADJUST_SESSIONID = 256, // 0x00000100
        TOKEN_ASSIGN_PRIMARY = 1,
        TOKEN_DUPLICATE = 2,
        TOKEN_EXECUTE = 131072, // 0x00020000
        TOKEN_IMPERSONATE = 4,
        TOKEN_QUERY = 8,
        TOKEN_QUERY_SOURCE = 16, // 0x00000010
        TOKEN_READ = TOKEN_QUERY | TOKEN_EXECUTE, // 0x00020008
        TOKEN_WRITE = TOKEN_EXECUTE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT, // 0x000200E0
        TOKEN_ALL_ACCESS = 983551, // 0x000F01FF
        MAXIMUM_ALLOWED = 33554432, // 0x02000000
      }

      public enum TOKEN_TYPE
      {
        TokenPrimary = 1,
        TokenImpersonation = 2,
      }

      public enum TOKEN_INFORMATION_CLASS
      {
        TokenUser = 1,
        TokenGroups = 2,
        TokenPrivileges = 3,
        TokenOwner = 4,
        TokenPrimaryGroup = 5,
        TokenDefaultDacl = 6,
        TokenSource = 7,
        TokenType = 8,
        TokenImpersonationLevel = 9,
        TokenStatistics = 10, // 0x0000000A
        TokenRestrictedSids = 11, // 0x0000000B
        TokenSessionId = 12, // 0x0000000C
        TokenGroupsAndPrivileges = 13, // 0x0000000D
        TokenSessionReference = 14, // 0x0000000E
        TokenSandBoxInert = 15, // 0x0000000F
        TokenAuditPolicy = 16, // 0x00000010
        TokenOrigin = 17, // 0x00000011
        TokenElevationType = 18, // 0x00000012
        TokenLinkedToken = 19, // 0x00000013
        TokenElevation = 20, // 0x00000014
        TokenHasRestrictions = 21, // 0x00000015
        TokenAccessInformation = 22, // 0x00000016
        TokenVirtualizationAllowed = 23, // 0x00000017
        TokenVirtualizationEnabled = 24, // 0x00000018
        TokenIntegrityLevel = 25, // 0x00000019
        TokenUIAccess = 26, // 0x0000001A
        TokenMandatoryPolicy = 27, // 0x0000001B
        TokenLogonSid = 28, // 0x0000001C
        MaxTokenInfoClass = 29, // 0x0000001D
      }

      public struct TOKEN_GROUPS
      {
        public int GroupCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public ame_assassin.Win32.SID.SID_AND_ATTRIBUTES[] Groups;

        public TOKEN_GROUPS(int privilegeCount)
        {
          this.GroupCount = privilegeCount;
          this.Groups = new ame_assassin.Win32.SID.SID_AND_ATTRIBUTES[32];
        }
      }

      public struct TOKEN_PRIMARY_GROUP
      {
        public IntPtr PrimaryGroup;

        public TOKEN_PRIMARY_GROUP(IntPtr _sid) => this.PrimaryGroup = _sid;
      }

      public struct TOKEN_SOURCE
      {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] SourceName;
        public ame_assassin.Win32.LUID SourceIdentifier;

        public TOKEN_SOURCE(string name)
        {
          this.SourceName = new byte[8];
          Encoding.GetEncoding(1252).GetBytes(name, 0, name.Length, this.SourceName, 0);
          if (!ame_assassin.Win32.SID.AllocateLocallyUniqueId(out this.SourceIdentifier))
            throw new Win32Exception();
        }
      }

      public struct TOKEN_USER
      {
        public ame_assassin.Win32.SID.SID_AND_ATTRIBUTES User;

        public TOKEN_USER(IntPtr _sid)
        {
          this.User = new ame_assassin.Win32.SID.SID_AND_ATTRIBUTES()
          {
            Sid = _sid,
            Attributes = 0U
          };
        }
      }

      public struct TOKEN_OWNER
      {
        public IntPtr Owner;

        public TOKEN_OWNER(IntPtr _owner) => this.Owner = _owner;
      }

      public struct TOKEN_DEFAULT_DACL
      {
        public IntPtr DefaultDacl;
      }

      public struct TOKEN_PRIVILEGES
      {
        public int PrivilegeCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
        public ame_assassin.Win32.LUID_AND_ATTRIBUTES[] Privileges;

        public TOKEN_PRIVILEGES(int privilegeCount)
        {
          this.PrivilegeCount = privilegeCount;
          this.Privileges = new ame_assassin.Win32.LUID_AND_ATTRIBUTES[36];
        }
      }

      public enum SECURITY_IMPERSONATION_LEVEL
      {
        SecurityAnonymous,
        SecurityIdentification,
        SecurityImpersonation,
        SecurityDelegation,
      }

      public struct SECURITY_QUALITY_OF_SERVICE
      {
        public int Length;
        public ame_assassin.Win32.Tokens.SECURITY_IMPERSONATION_LEVEL ImpersonationLevel;
        public byte ContextTrackingMode;
        public byte EffectiveOnly;

        public SECURITY_QUALITY_OF_SERVICE(
          ame_assassin.Win32.Tokens.SECURITY_IMPERSONATION_LEVEL _impersonationLevel,
          byte _contextTrackingMode,
          byte _effectiveOnly)
        {
          this.Length = 0;
          this.ImpersonationLevel = _impersonationLevel;
          this.ContextTrackingMode = _contextTrackingMode;
          this.EffectiveOnly = _effectiveOnly;
          this.Length = Marshal.SizeOf<ame_assassin.Win32.Tokens.SECURITY_QUALITY_OF_SERVICE>(this);
        }
      }

      [Flags]
      public enum SE_PRIVILEGE_ATTRIBUTES : uint
      {
        SE_PRIVILEGE_ENABLED_BY_DEFAULT = 1,
        SE_PRIVILEGE_ENABLED = 2,
        SE_PRIVILEGE_USED_FOR_ACCESS = 2147483648, // 0x80000000
      }

      [Flags]
      public enum SE_GROUP_ATTRIBUTES : uint
      {
        SE_GROUP_MANDATORY = 1,
        SE_GROUP_ENABLED_BY_DEFAULT = 2,
        SE_GROUP_ENABLED = 4,
        SE_GROUP_OWNER = 8,
        SE_GROUP_USE_FOR_DENY_ONLY = 16, // 0x00000010
        SE_GROUP_INTEGRITY = 32, // 0x00000020
        SE_GROUP_INTEGRITY_ENABLED = 64, // 0x00000040
        SE_GROUP_RESOURCE = 536870912, // 0x20000000
        SE_GROUP_LOGON_ID = 3221225472, // 0xC0000000
      }

      public struct TOKEN_MANDATORY_LABEL
      {
        public ame_assassin.Win32.SID.SID_AND_ATTRIBUTES Label;
      }
    }

    public static class TokensEx
    {
      public static bool CreateTokenPrivileges(
        string[] privs,
        out ame_assassin.Win32.Tokens.TOKEN_PRIVILEGES tokenPrivileges)
      {
        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (ame_assassin.Win32.Tokens.TOKEN_PRIVILEGES)));
        tokenPrivileges = (ame_assassin.Win32.Tokens.TOKEN_PRIVILEGES) Marshal.PtrToStructure(ptr, typeof (ame_assassin.Win32.Tokens.TOKEN_PRIVILEGES));
        tokenPrivileges.PrivilegeCount = privs.Length;
        for (int index = 0; index < tokenPrivileges.PrivilegeCount; ++index)
        {
          ame_assassin.Win32.LUID lpLuid;
          if (!ame_assassin.Win32.Tokens.LookupPrivilegeValue(IntPtr.Zero, privs[index], out lpLuid))
            return false;
          tokenPrivileges.Privileges[index].Attributes = 3U;
          tokenPrivileges.Privileges[index].Luid = lpLuid;
        }
        return true;
      }

      public static void AdjustCurrentPrivilege(string privilege)
      {
        ame_assassin.Win32.LUID lpLuid;
        ame_assassin.Win32.Tokens.LookupPrivilegeValue(IntPtr.Zero, privilege, out lpLuid);
        ame_assassin.Win32.Tokens.RtlAdjustPrivilege(lpLuid, true, true, out bool _);
      }

      public static IntPtr GetInfoFromToken(
        ame_assassin.Win32.TokensEx.SafeTokenHandle token,
        ame_assassin.Win32.Tokens.TOKEN_INFORMATION_CLASS information)
      {
        int ReturnLength;
        ame_assassin.Win32.Tokens.GetTokenInformation(token, information, IntPtr.Zero, 0, out ReturnLength);
        IntPtr TokenInformation = Marshal.AllocHGlobal(ReturnLength);
        ame_assassin.Win32.Tokens.GetTokenInformation(token, information, TokenInformation, ReturnLength, out ReturnLength);
        return TokenInformation;
      }

      public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
      {
        public SafeTokenHandle(IntPtr preexistingHandle)
          : base(true)
        {
          this.SetHandle(preexistingHandle);
        }

        public SafeTokenHandle()
          : base(true)
        {
        }

        protected override bool ReleaseHandle() => ame_assassin.Win32.CloseHandle(this.handle);
      }
    }

    public static class Process
    {
      [DllImport("kernel32.dll", SetLastError = true)]
      public static extern IntPtr GetCurrentProcess();

      [DllImport("kernel32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

      [DllImport("kernel32.dll", SetLastError = true)]
      public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

      [DllImport("userenv.dll", SetLastError = true)]
      public static extern bool CreateEnvironmentBlock(
        out IntPtr lpEnvironment,
        ame_assassin.Win32.TokensEx.SafeTokenHandle hToken,
        bool bInherit);

      [DllImport("userenv.dll", SetLastError = true)]
      public static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

      [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      public static extern bool CreateProcessAsUser(
        ame_assassin.Win32.TokensEx.SafeTokenHandle hToken,
        string lpApplicationName,
        StringBuilder lpCommandLine,
        ame_assassin.Win32.SECURITY_ATTRIBUTES lpProcessAttributes,
        ame_assassin.Win32.SECURITY_ATTRIBUTES lpThreadAttributes,
        bool bInheritHandles,
        int dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ame_assassin.Win32.Process.STARTUPINFO lpStartupInfo,
        ame_assassin.Win32.Process.PROCESS_INFORMATION lpProcessInformation);

      [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern bool CreateProcessWithToken(
        ame_assassin.Win32.TokensEx.SafeTokenHandle hToken,
        ame_assassin.Win32.Process.LogonFlags dwLogonFlags,
        string lpApplicationName,
        string lpCommandLine,
        ame_assassin.Win32.Process.ProcessCreationFlags dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref ame_assassin.Win32.Process.STARTUPINFO lpStartupInfo,
        out ame_assassin.Win32.Process.PROCESS_INFORMATION lpProcessInformation);

      [DllImport("kernel32.dll", SetLastError = true)]
      internal static extern IntPtr OpenProcess(
        ame_assassin.Win32.Process.ProcessAccessFlags dwDesiredAccess,
        bool bInheritHandle,
        int dwProcessId);

      [Flags]
      internal enum ProcessAccessFlags : uint
      {
        All = 2035711, // 0x001F0FFF
        Terminate = 1,
        CreateThread = 2,
        VirtualMemoryOperation = 8,
        VirtualMemoryRead = 16, // 0x00000010
        VirtualMemoryWrite = 32, // 0x00000020
        DuplicateHandle = 64, // 0x00000040
        CreateProcess = 128, // 0x00000080
        SetQuota = 256, // 0x00000100
        SetInformation = 512, // 0x00000200
        QueryInformation = 1024, // 0x00000400
        QueryLimitedInformation = 4096, // 0x00001000
        Synchronize = 1048576, // 0x00100000
      }

      public enum LogonFlags
      {
        WithProfile = 1,
        NetCredentialsOnly = 2,
      }

      public struct PROCESS_INFORMATION
      {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
      }

      [Flags]
      public enum ProcessCreationFlags : uint
      {
        DEBUG_PROCESS = 1,
        DEBUG_ONLY_THIS_PROCESS = 2,
        CREATE_SUSPENDED = 4,
        DETACHED_PROCESS = 8,
        CREATE_NEW_CONSOLE = 16, // 0x00000010
        CREATE_NEW_PROCESS_GROUP = 512, // 0x00000200
        CREATE_UNICODE_ENVIRONMENT = 1024, // 0x00000400
        CREATE_SEPARATE_WOW_VDM = 2048, // 0x00000800
        CREATE_SHARED_WOW_VDM = 4096, // 0x00001000
        INHERIT_PARENT_AFFINITY = 65536, // 0x00010000
        CREATE_PROTECTED_PROCESS = 262144, // 0x00040000
        EXTENDED_STARTUPINFO_PRESENT = 524288, // 0x00080000
        CREATE_BREAKAWAY_FROM_JOB = 16777216, // 0x01000000
        CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 33554432, // 0x02000000
        CREATE_DEFAULT_ERROR_MODE = 67108864, // 0x04000000
        CREATE_NO_WINDOW = 134217728, // 0x08000000
      }

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      public struct STARTUPINFO
      {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
      }
    }

    public static class IO
    {
      [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern IntPtr CreateFile(
        [MarshalAs(UnmanagedType.LPTStr)] string filename,
        [MarshalAs(UnmanagedType.U4)] ame_assassin.Win32.IO.FileAccess access,
        [MarshalAs(UnmanagedType.U4)] ame_assassin.Win32.IO.FileShare share,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] ame_assassin.Win32.IO.FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] ame_assassin.Win32.IO.FileAttributes flagsAndAttributes,
        IntPtr templateFile);

      [Flags]
      public enum FileAccess : uint
      {
        AccessSystemSecurity = 16777216, // 0x01000000
        MaximumAllowed = 33554432, // 0x02000000
        Delete = 65536, // 0x00010000
        ReadControl = 131072, // 0x00020000
        WriteDAC = 262144, // 0x00040000
        WriteOwner = 524288, // 0x00080000
        Synchronize = 1048576, // 0x00100000
        StandardRightsRequired = WriteOwner | WriteDAC | ReadControl | Delete, // 0x000F0000
        StandardRightsRead = ReadControl, // 0x00020000
        StandardRightsWrite = StandardRightsRead, // 0x00020000
        StandardRightsExecute = StandardRightsWrite, // 0x00020000
        StandardRightsAll = StandardRightsExecute | Synchronize | WriteOwner | WriteDAC | Delete, // 0x001F0000
        SpecificRightsAll = 65535, // 0x0000FFFF
        FILE_READ_DATA = 1,
        FILE_LIST_DIRECTORY = FILE_READ_DATA, // 0x00000001
        FILE_WRITE_DATA = 2,
        FILE_ADD_FILE = FILE_WRITE_DATA, // 0x00000002
        FILE_APPEND_DATA = 4,
        FILE_ADD_SUBDIRECTORY = FILE_APPEND_DATA, // 0x00000004
        FILE_CREATE_PIPE_INSTANCE = FILE_ADD_SUBDIRECTORY, // 0x00000004
        FILE_READ_EA = 8,
        FILE_WRITE_EA = 16, // 0x00000010
        FILE_EXECUTE = 32, // 0x00000020
        FILE_TRAVERSE = FILE_EXECUTE, // 0x00000020
        FILE_DELETE_CHILD = 64, // 0x00000040
        FILE_READ_ATTRIBUTES = 128, // 0x00000080
        FILE_WRITE_ATTRIBUTES = 256, // 0x00000100
        GenericRead = 2147483648, // 0x80000000
        GenericWrite = 1073741824, // 0x40000000
        GenericExecute = 536870912, // 0x20000000
        GenericAll = 268435456, // 0x10000000
        SPECIFIC_RIGHTS_ALL = 65535, // 0x0000FFFF
        FILE_ALL_ACCESS = FILE_WRITE_ATTRIBUTES | FILE_READ_ATTRIBUTES | FILE_DELETE_CHILD | FILE_TRAVERSE | FILE_WRITE_EA | FILE_READ_EA | FILE_CREATE_PIPE_INSTANCE | FILE_ADD_FILE | FILE_LIST_DIRECTORY | StandardRightsAll, // 0x001F01FF
        FILE_GENERIC_READ = FILE_READ_ATTRIBUTES | FILE_READ_EA | FILE_LIST_DIRECTORY | StandardRightsExecute | Synchronize, // 0x00120089
        FILE_GENERIC_WRITE = FILE_WRITE_ATTRIBUTES | FILE_WRITE_EA | FILE_CREATE_PIPE_INSTANCE | FILE_ADD_FILE | StandardRightsExecute | Synchronize, // 0x00120116
        FILE_GENERIC_EXECUTE = FILE_READ_ATTRIBUTES | FILE_TRAVERSE | StandardRightsExecute | Synchronize, // 0x001200A0
      }

      [Flags]
      public enum FileShare : uint
      {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
      }

      public enum FileMode : uint
      {
        New = 1,
        CreateAlways = 2,
        OpenExisting = 3,
        OpenAlways = 4,
        TruncateExisting = 5,
      }

      [Flags]
      public enum FileAttributes : uint
      {
        Readonly = 1,
        Hidden = 2,
        System = 4,
        Directory = 16, // 0x00000010
        Archive = 32, // 0x00000020
        Device = 64, // 0x00000040
        Normal = 128, // 0x00000080
        Temporary = 256, // 0x00000100
        SparseFile = 512, // 0x00000200
        ReparsePoint = 1024, // 0x00000400
        Compressed = 2048, // 0x00000800
        Offline = 4096, // 0x00001000
        NotContentIndexed = 8192, // 0x00002000
        Encrypted = 16384, // 0x00004000
        Write_Through = 2147483648, // 0x80000000
        Overlapped = 1073741824, // 0x40000000
        NoBuffering = 536870912, // 0x20000000
        RandomAccess = 268435456, // 0x10000000
        SequentialScan = 134217728, // 0x08000000
        DeleteOnClose = 67108864, // 0x04000000
        BackupSemantics = 33554432, // 0x02000000
        PosixSemantics = 16777216, // 0x01000000
        OpenReparsePoint = 2097152, // 0x00200000
        OpenNoRecall = 1048576, // 0x00100000
        FirstPipeInstance = 524288, // 0x00080000
      }
    }

    public static class SID
    {
      public static ame_assassin.Win32.SID.SID_IDENTIFIER_AUTHORITY SECURITY_MANDATORY_LABEL_AUTHORITY = new ame_assassin.Win32.SID.SID_IDENTIFIER_AUTHORITY(new byte[6]
      {
        (byte) 0,
        (byte) 0,
        (byte) 0,
        (byte) 0,
        (byte) 0,
        (byte) 16
      });
      public const int NtSecurityAuthority = 5;
      public const int AuthenticatedUser = 11;
      public const string DOMAIN_ALIAS_RID_ADMINS = "S-1-5-32-544";
      public const string DOMAIN_ALIAS_RID_LOCAL_AND_ADMIN_GROUP = "S-1-5-114";
      public const string TRUSTED_INSTALLER_RID = "S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";
      public const string INTEGRITY_UNTRUSTED_SID = "S-1-16-0";
      public const string INTEGRITY_LOW_SID = "S-1-16-4096";
      public const string INTEGRITY_MEDIUM_SID = "S-1-16-8192";
      public const string INTEGRITY_MEDIUMPLUS_SID = "S-1-16-8448";
      public const string INTEGRITY_HIGH_SID = "S-1-16-12288";
      public const string INTEGRITY_SYSTEM_SID = "S-1-16-16384";
      public const string INTEGRITY_PROTECTEDPROCESS_SID = "S-1-16-20480";

      [DllImport("advapi32.dll", SetLastError = true)]
      public static extern bool AllocateAndInitializeSid(
        ref ame_assassin.Win32.SID.SID_IDENTIFIER_AUTHORITY pIdentifierAuthority,
        byte nSubAuthorityCount,
        int dwSubAuthority0,
        int dwSubAuthority1,
        int dwSubAuthority2,
        int dwSubAuthority3,
        int dwSubAuthority4,
        int dwSubAuthority5,
        int dwSubAuthority6,
        int dwSubAuthority7,
        out IntPtr pSid);

      [DllImport("advapi32.dll")]
      public static extern bool AllocateLocallyUniqueId(out ame_assassin.Win32.LUID allocated);

      [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern bool ConvertSidToStringSid(IntPtr pSid, out string strSid);

      [DllImport("advapi32.dll", SetLastError = true)]
      public static extern bool ConvertStringSidToSid(string StringSid, out IntPtr ptrSid);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern int GetLengthSid(IntPtr pSid);

      [DllImport("advapi32.dll")]
      public static extern IntPtr FreeSid(IntPtr pSid);

      public struct SID_AND_ATTRIBUTES
      {
        public IntPtr Sid;
        public uint Attributes;
      }

      public struct SID_IDENTIFIER_AUTHORITY
      {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.I1)]
        public byte[] Value;

        public SID_IDENTIFIER_AUTHORITY(byte[] value) => this.Value = value;
      }

      public enum SECURITY_MANDATORY_LABEL
      {
        Untrusted = 0,
        Low = 4096, // 0x00001000
        Medium = 8192, // 0x00002000
        MediumPlus = 8448, // 0x00002100
        High = 12288, // 0x00003000
        System = 16384, // 0x00004000
      }
    }

    public static class WTS
    {
      [DllImport("wtsapi32.dll", SetLastError = true)]
      public static extern int WTSEnumerateSessions(
        IntPtr hServer,
        int Reserved,
        int Version,
        ref IntPtr ppSessionInfo,
        ref int pCount);

      [DllImport("wtsapi32.dll", SetLastError = true)]
      public static extern bool WTSEnumerateProcesses(
        IntPtr serverHandle,
        int reserved,
        int version,
        ref IntPtr ppProcessInfo,
        ref int pCount);

      [DllImport("kernel32.dll")]
      public static extern uint WTSGetActiveConsoleSessionId();

      [DllImport("wtsapi32.dll", SetLastError = true)]
      public static extern bool WTSQueryUserToken(
        uint sessionId,
        out ame_assassin.Win32.TokensEx.SafeTokenHandle Token);

      [DllImport("wtsapi32.dll")]
      public static extern void WTSFreeMemory(IntPtr pMemory);

      public struct WTS_SESSION_INFO
      {
        public int SessionID;
        [MarshalAs(UnmanagedType.LPStr)]
        public string pWinStationName;
        public ame_assassin.Win32.WTS.WTS_CONNECTSTATE_CLASS State;
      }

      public enum WTS_CONNECTSTATE_CLASS
      {
        WTSActive,
        WTSConnected,
        WTSConnectQuery,
        WTSShadow,
        WTSDisconnected,
        WTSIdle,
        WTSListen,
        WTSReset,
        WTSDown,
        WTSInit,
      }

      public struct WTS_PROCESS_INFO
      {
        public int SessionID;
        public int ProcessID;
        public IntPtr ProcessName;
        public IntPtr UserSid;
      }
    }

    public static class LSA
    {
      [DllImport("secur32.dll")]
      public static extern uint LsaFreeReturnBuffer(IntPtr buffer);

      [DllImport("secur32.dll")]
      public static extern uint LsaEnumerateLogonSessions(
        out ulong LogonSessionCount,
        out IntPtr LogonSessionList);

      [DllImport("secur32.dll")]
      public static extern uint LsaGetLogonSessionData(IntPtr luid, out IntPtr ppLogonSessionData);

      public struct LSA_UNICODE_STRING
      {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr buffer;
      }

      public struct SECURITY_LOGON_SESSION_DATA
      {
        public uint Size;
        public ame_assassin.Win32.LUID LoginID;
        public ame_assassin.Win32.LSA.LSA_UNICODE_STRING Username;
        public ame_assassin.Win32.LSA.LSA_UNICODE_STRING LoginDomain;
        public ame_assassin.Win32.LSA.LSA_UNICODE_STRING AuthenticationPackage;
        public uint LogonType;
        public uint Session;
        public IntPtr PSiD;
        public ulong LoginTime;
        public ame_assassin.Win32.LSA.LSA_UNICODE_STRING LogonServer;
        public ame_assassin.Win32.LSA.LSA_UNICODE_STRING DnsDomainName;
        public ame_assassin.Win32.LSA.LSA_UNICODE_STRING Upn;
      }

      public enum SECURITY_LOGON_TYPE : uint
      {
        Interactive = 2,
        Network = 3,
        Batch = 4,
        Service = 5,
        Proxy = 6,
        Unlock = 7,
        NetworkCleartext = 8,
        NewCredentials = 9,
        RemoteInteractive = 10, // 0x0000000A
        CachedInteractive = 11, // 0x0000000B
        CachedRemoteInteractive = 12, // 0x0000000C
        CachedUnlock = 13, // 0x0000000D
      }
    }

    public struct LUID
    {
      public uint LowPart;
      public uint HighPart;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct LUID_AND_ATTRIBUTES
    {
      public ame_assassin.Win32.LUID Luid;
      public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SECURITY_ATTRIBUTES
    {
      public int nLength = 12;
      public ame_assassin.Win32.SafeLocalMemHandle lpSecurityDescriptor = new ame_assassin.Win32.SafeLocalMemHandle(IntPtr.Zero, false);
      public bool bInheritHandle;
    }

    [SuppressUnmanagedCodeSecurity]
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
      [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
      internal SafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle)
        : base(ownsHandle)
      {
        this.SetHandle(existingHandle);
      }

      [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
      [DllImport("kernel32.dll")]
      private static extern IntPtr LocalFree(IntPtr hMem);

      protected override bool ReleaseHandle()
      {
        return ame_assassin.Win32.SafeLocalMemHandle.LocalFree(this.handle) == IntPtr.Zero;
      }
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct LARGE_INTEGER
    {
      [FieldOffset(0)]
      public int Low;
      [FieldOffset(4)]
      public int High;
      [FieldOffset(0)]
      public long QuadPart;

      public LARGE_INTEGER(long _quad)
      {
        this.Low = 0;
        this.High = 0;
        this.QuadPart = _quad;
      }

      public long ToInt64() => (long) this.High << 32 | (long) (uint) this.Low;

      public static ame_assassin.Win32.LARGE_INTEGER FromInt64(long value)
      {
        return new ame_assassin.Win32.LARGE_INTEGER()
        {
          Low = (int) value,
          High = (int) (value >> 32)
        };
      }
    }

    public struct UNICODE_STRING : IDisposable
    {
      public ushort Length;
      public ushort MaximumLength;
      private IntPtr buffer;

      public UNICODE_STRING(string s)
      {
        this.Length = (ushort) (s.Length * 2);
        this.MaximumLength = (ushort) ((uint) this.Length + 2U);
        this.buffer = Marshal.StringToHGlobalUni(s);
      }

      public void Dispose()
      {
        Marshal.FreeHGlobal(this.buffer);
        this.buffer = IntPtr.Zero;
      }

      public override string ToString() => Marshal.PtrToStringUni(this.buffer);
    }

    public enum NtStatus : uint
    {
      Success = 0,
      Wait0 = 0,
      Wait1 = 1,
      Wait2 = 2,
      Wait3 = 3,
      Wait63 = 63, // 0x0000003F
      Abandoned = 128, // 0x00000080
      AbandonedWait0 = 128, // 0x00000080
      AbandonedWait1 = 129, // 0x00000081
      AbandonedWait2 = 130, // 0x00000082
      AbandonedWait3 = 131, // 0x00000083
      AbandonedWait63 = 191, // 0x000000BF
      UserApc = 192, // 0x000000C0
      KernelApc = 256, // 0x00000100
      Alerted = 257, // 0x00000101
      Timeout = 258, // 0x00000102
      Pending = 259, // 0x00000103
      Reparse = 260, // 0x00000104
      MoreEntries = 261, // 0x00000105
      NotAllAssigned = 262, // 0x00000106
      SomeNotMapped = 263, // 0x00000107
      OpLockBreakInProgress = 264, // 0x00000108
      VolumeMounted = 265, // 0x00000109
      RxActCommitted = 266, // 0x0000010A
      NotifyCleanup = 267, // 0x0000010B
      NotifyEnumDir = 268, // 0x0000010C
      NoQuotasForAccount = 269, // 0x0000010D
      PrimaryTransportConnectFailed = 270, // 0x0000010E
      PageFaultTransition = 272, // 0x00000110
      PageFaultDemandZero = 273, // 0x00000111
      PageFaultCopyOnWrite = 274, // 0x00000112
      PageFaultGuardPage = 275, // 0x00000113
      PageFaultPagingFile = 276, // 0x00000114
      CrashDump = 278, // 0x00000116
      ReparseObject = 280, // 0x00000118
      NothingToTerminate = 290, // 0x00000122
      ProcessNotInJob = 291, // 0x00000123
      ProcessInJob = 292, // 0x00000124
      ProcessCloned = 297, // 0x00000129
      FileLockedWithOnlyReaders = 298, // 0x0000012A
      FileLockedWithWriters = 299, // 0x0000012B
      Informational = 1073741824, // 0x40000000
      ObjectNameExists = 1073741824, // 0x40000000
      ThreadWasSuspended = 1073741825, // 0x40000001
      WorkingSetLimitRange = 1073741826, // 0x40000002
      ImageNotAtBase = 1073741827, // 0x40000003
      RegistryRecovered = 1073741833, // 0x40000009
      Warning = 2147483648, // 0x80000000
      GuardPageViolation = 2147483649, // 0x80000001
      DatatypeMisalignment = 2147483650, // 0x80000002
      Breakpoint = 2147483651, // 0x80000003
      SingleStep = 2147483652, // 0x80000004
      BufferOverflow = 2147483653, // 0x80000005
      NoMoreFiles = 2147483654, // 0x80000006
      HandlesClosed = 2147483658, // 0x8000000A
      PartialCopy = 2147483661, // 0x8000000D
      DeviceBusy = 2147483665, // 0x80000011
      InvalidEaName = 2147483667, // 0x80000013
      EaListInconsistent = 2147483668, // 0x80000014
      NoMoreEntries = 2147483674, // 0x8000001A
      LongJump = 2147483686, // 0x80000026
      DllMightBeInsecure = 2147483691, // 0x8000002B
      Error = 3221225472, // 0xC0000000
      Unsuccessful = 3221225473, // 0xC0000001
      NotImplemented = 3221225474, // 0xC0000002
      InvalidInfoClass = 3221225475, // 0xC0000003
      InfoLengthMismatch = 3221225476, // 0xC0000004
      AccessViolation = 3221225477, // 0xC0000005
      InPageError = 3221225478, // 0xC0000006
      PagefileQuota = 3221225479, // 0xC0000007
      InvalidHandle = 3221225480, // 0xC0000008
      BadInitialStack = 3221225481, // 0xC0000009
      BadInitialPc = 3221225482, // 0xC000000A
      InvalidCid = 3221225483, // 0xC000000B
      TimerNotCanceled = 3221225484, // 0xC000000C
      InvalidParameter = 3221225485, // 0xC000000D
      NoSuchDevice = 3221225486, // 0xC000000E
      NoSuchFile = 3221225487, // 0xC000000F
      InvalidDeviceRequest = 3221225488, // 0xC0000010
      EndOfFile = 3221225489, // 0xC0000011
      WrongVolume = 3221225490, // 0xC0000012
      NoMediaInDevice = 3221225491, // 0xC0000013
      NoMemory = 3221225495, // 0xC0000017
      NotMappedView = 3221225497, // 0xC0000019
      UnableToFreeVm = 3221225498, // 0xC000001A
      UnableToDeleteSection = 3221225499, // 0xC000001B
      IllegalInstruction = 3221225501, // 0xC000001D
      AlreadyCommitted = 3221225505, // 0xC0000021
      AccessDenied = 3221225506, // 0xC0000022
      BufferTooSmall = 3221225507, // 0xC0000023
      ObjectTypeMismatch = 3221225508, // 0xC0000024
      NonContinuableException = 3221225509, // 0xC0000025
      BadStack = 3221225512, // 0xC0000028
      NotLocked = 3221225514, // 0xC000002A
      NotCommitted = 3221225517, // 0xC000002D
      InvalidParameterMix = 3221225520, // 0xC0000030
      ObjectNameInvalid = 3221225523, // 0xC0000033
      ObjectNameNotFound = 3221225524, // 0xC0000034
      ObjectNameCollision = 3221225525, // 0xC0000035
      ObjectPathInvalid = 3221225529, // 0xC0000039
      ObjectPathNotFound = 3221225530, // 0xC000003A
      ObjectPathSyntaxBad = 3221225531, // 0xC000003B
      DataOverrun = 3221225532, // 0xC000003C
      DataLate = 3221225533, // 0xC000003D
      DataError = 3221225534, // 0xC000003E
      CrcError = 3221225535, // 0xC000003F
      SectionTooBig = 3221225536, // 0xC0000040
      PortConnectionRefused = 3221225537, // 0xC0000041
      InvalidPortHandle = 3221225538, // 0xC0000042
      SharingViolation = 3221225539, // 0xC0000043
      QuotaExceeded = 3221225540, // 0xC0000044
      InvalidPageProtection = 3221225541, // 0xC0000045
      MutantNotOwned = 3221225542, // 0xC0000046
      SemaphoreLimitExceeded = 3221225543, // 0xC0000047
      PortAlreadySet = 3221225544, // 0xC0000048
      SectionNotImage = 3221225545, // 0xC0000049
      SuspendCountExceeded = 3221225546, // 0xC000004A
      ThreadIsTerminating = 3221225547, // 0xC000004B
      BadWorkingSetLimit = 3221225548, // 0xC000004C
      IncompatibleFileMap = 3221225549, // 0xC000004D
      SectionProtection = 3221225550, // 0xC000004E
      EasNotSupported = 3221225551, // 0xC000004F
      EaTooLarge = 3221225552, // 0xC0000050
      NonExistentEaEntry = 3221225553, // 0xC0000051
      NoEasOnFile = 3221225554, // 0xC0000052
      EaCorruptError = 3221225555, // 0xC0000053
      FileLockConflict = 3221225556, // 0xC0000054
      LockNotGranted = 3221225557, // 0xC0000055
      DeletePending = 3221225558, // 0xC0000056
      CtlFileNotSupported = 3221225559, // 0xC0000057
      UnknownRevision = 3221225560, // 0xC0000058
      RevisionMismatch = 3221225561, // 0xC0000059
      InvalidOwner = 3221225562, // 0xC000005A
      InvalidPrimaryGroup = 3221225563, // 0xC000005B
      NoImpersonationToken = 3221225564, // 0xC000005C
      CantDisableMandatory = 3221225565, // 0xC000005D
      NoLogonServers = 3221225566, // 0xC000005E
      NoSuchLogonSession = 3221225567, // 0xC000005F
      NoSuchPrivilege = 3221225568, // 0xC0000060
      PrivilegeNotHeld = 3221225569, // 0xC0000061
      InvalidAccountName = 3221225570, // 0xC0000062
      UserExists = 3221225571, // 0xC0000063
      NoSuchUser = 3221225572, // 0xC0000064
      GroupExists = 3221225573, // 0xC0000065
      NoSuchGroup = 3221225574, // 0xC0000066
      MemberInGroup = 3221225575, // 0xC0000067
      MemberNotInGroup = 3221225576, // 0xC0000068
      LastAdmin = 3221225577, // 0xC0000069
      WrongPassword = 3221225578, // 0xC000006A
      IllFormedPassword = 3221225579, // 0xC000006B
      PasswordRestriction = 3221225580, // 0xC000006C
      LogonFailure = 3221225581, // 0xC000006D
      AccountRestriction = 3221225582, // 0xC000006E
      InvalidLogonHours = 3221225583, // 0xC000006F
      InvalidWorkstation = 3221225584, // 0xC0000070
      PasswordExpired = 3221225585, // 0xC0000071
      AccountDisabled = 3221225586, // 0xC0000072
      NoneMapped = 3221225587, // 0xC0000073
      TooManyLuidsRequested = 3221225588, // 0xC0000074
      LuidsExhausted = 3221225589, // 0xC0000075
      InvalidSubAuthority = 3221225590, // 0xC0000076
      InvalidAcl = 3221225591, // 0xC0000077
      InvalidSid = 3221225592, // 0xC0000078
      InvalidSecurityDescr = 3221225593, // 0xC0000079
      ProcedureNotFound = 3221225594, // 0xC000007A
      InvalidImageFormat = 3221225595, // 0xC000007B
      NoToken = 3221225596, // 0xC000007C
      BadInheritanceAcl = 3221225597, // 0xC000007D
      RangeNotLocked = 3221225598, // 0xC000007E
      DiskFull = 3221225599, // 0xC000007F
      ServerDisabled = 3221225600, // 0xC0000080
      ServerNotDisabled = 3221225601, // 0xC0000081
      TooManyGuidsRequested = 3221225602, // 0xC0000082
      GuidsExhausted = 3221225603, // 0xC0000083
      InvalidIdAuthority = 3221225604, // 0xC0000084
      AgentsExhausted = 3221225605, // 0xC0000085
      InvalidVolumeLabel = 3221225606, // 0xC0000086
      SectionNotExtended = 3221225607, // 0xC0000087
      NotMappedData = 3221225608, // 0xC0000088
      ResourceDataNotFound = 3221225609, // 0xC0000089
      ResourceTypeNotFound = 3221225610, // 0xC000008A
      ResourceNameNotFound = 3221225611, // 0xC000008B
      ArrayBoundsExceeded = 3221225612, // 0xC000008C
      FloatDenormalOperand = 3221225613, // 0xC000008D
      FloatDivideByZero = 3221225614, // 0xC000008E
      FloatInexactResult = 3221225615, // 0xC000008F
      FloatInvalidOperation = 3221225616, // 0xC0000090
      FloatOverflow = 3221225617, // 0xC0000091
      FloatStackCheck = 3221225618, // 0xC0000092
      FloatUnderflow = 3221225619, // 0xC0000093
      IntegerDivideByZero = 3221225620, // 0xC0000094
      IntegerOverflow = 3221225621, // 0xC0000095
      PrivilegedInstruction = 3221225622, // 0xC0000096
      TooManyPagingFiles = 3221225623, // 0xC0000097
      FileInvalid = 3221225624, // 0xC0000098
      InstanceNotAvailable = 3221225643, // 0xC00000AB
      PipeNotAvailable = 3221225644, // 0xC00000AC
      InvalidPipeState = 3221225645, // 0xC00000AD
      PipeBusy = 3221225646, // 0xC00000AE
      IllegalFunction = 3221225647, // 0xC00000AF
      PipeDisconnected = 3221225648, // 0xC00000B0
      PipeClosing = 3221225649, // 0xC00000B1
      PipeConnected = 3221225650, // 0xC00000B2
      PipeListening = 3221225651, // 0xC00000B3
      InvalidReadMode = 3221225652, // 0xC00000B4
      IoTimeout = 3221225653, // 0xC00000B5
      FileForcedClosed = 3221225654, // 0xC00000B6
      ProfilingNotStarted = 3221225655, // 0xC00000B7
      ProfilingNotStopped = 3221225656, // 0xC00000B8
      NotSameDevice = 3221225684, // 0xC00000D4
      FileRenamed = 3221225685, // 0xC00000D5
      CantWait = 3221225688, // 0xC00000D8
      PipeEmpty = 3221225689, // 0xC00000D9
      CantTerminateSelf = 3221225691, // 0xC00000DB
      InternalError = 3221225701, // 0xC00000E5
      InvalidParameter1 = 3221225711, // 0xC00000EF
      InvalidParameter2 = 3221225712, // 0xC00000F0
      InvalidParameter3 = 3221225713, // 0xC00000F1
      InvalidParameter4 = 3221225714, // 0xC00000F2
      InvalidParameter5 = 3221225715, // 0xC00000F3
      InvalidParameter6 = 3221225716, // 0xC00000F4
      InvalidParameter7 = 3221225717, // 0xC00000F5
      InvalidParameter8 = 3221225718, // 0xC00000F6
      InvalidParameter9 = 3221225719, // 0xC00000F7
      InvalidParameter10 = 3221225720, // 0xC00000F8
      InvalidParameter11 = 3221225721, // 0xC00000F9
      InvalidParameter12 = 3221225722, // 0xC00000FA
      MappedFileSizeZero = 3221225758, // 0xC000011E
      TooManyOpenedFiles = 3221225759, // 0xC000011F
      Cancelled = 3221225760, // 0xC0000120
      CannotDelete = 3221225761, // 0xC0000121
      InvalidComputerName = 3221225762, // 0xC0000122
      FileDeleted = 3221225763, // 0xC0000123
      SpecialAccount = 3221225764, // 0xC0000124
      SpecialGroup = 3221225765, // 0xC0000125
      SpecialUser = 3221225766, // 0xC0000126
      MembersPrimaryGroup = 3221225767, // 0xC0000127
      FileClosed = 3221225768, // 0xC0000128
      TooManyThreads = 3221225769, // 0xC0000129
      ThreadNotInProcess = 3221225770, // 0xC000012A
      TokenAlreadyInUse = 3221225771, // 0xC000012B
      PagefileQuotaExceeded = 3221225772, // 0xC000012C
      CommitmentLimit = 3221225773, // 0xC000012D
      InvalidImageLeFormat = 3221225774, // 0xC000012E
      InvalidImageNotMz = 3221225775, // 0xC000012F
      InvalidImageProtect = 3221225776, // 0xC0000130
      InvalidImageWin16 = 3221225777, // 0xC0000131
      LogonServer = 3221225778, // 0xC0000132
      DifferenceAtDc = 3221225779, // 0xC0000133
      SynchronizationRequired = 3221225780, // 0xC0000134
      DllNotFound = 3221225781, // 0xC0000135
      IoPrivilegeFailed = 3221225783, // 0xC0000137
      OrdinalNotFound = 3221225784, // 0xC0000138
      EntryPointNotFound = 3221225785, // 0xC0000139
      ControlCExit = 3221225786, // 0xC000013A
      PortNotSet = 3221226323, // 0xC0000353
      DebuggerInactive = 3221226324, // 0xC0000354
      CallbackBypass = 3221226755, // 0xC0000503
      PortClosed = 3221227264, // 0xC0000700
      MessageLost = 3221227265, // 0xC0000701
      InvalidMessage = 3221227266, // 0xC0000702
      RequestCanceled = 3221227267, // 0xC0000703
      RecursiveDispatch = 3221227268, // 0xC0000704
      LpcReceiveBufferExpected = 3221227269, // 0xC0000705
      LpcInvalidConnectionUsage = 3221227270, // 0xC0000706
      LpcRequestsNotAllowed = 3221227271, // 0xC0000707
      ResourceInUse = 3221227272, // 0xC0000708
      ProcessIsProtected = 3221227282, // 0xC0000712
      VolumeDirty = 3221227526, // 0xC0000806
      FileCheckedOut = 3221227777, // 0xC0000901
      CheckOutRequired = 3221227778, // 0xC0000902
      BadFileType = 3221227779, // 0xC0000903
      FileTooLarge = 3221227780, // 0xC0000904
      FormsAuthRequired = 3221227781, // 0xC0000905
      VirusInfected = 3221227782, // 0xC0000906
      VirusDeleted = 3221227783, // 0xC0000907
      TransactionalConflict = 3222863873, // 0xC0190001
      InvalidTransaction = 3222863874, // 0xC0190002
      TransactionNotActive = 3222863875, // 0xC0190003
      TmInitializationFailed = 3222863876, // 0xC0190004
      RmNotActive = 3222863877, // 0xC0190005
      RmMetadataCorrupt = 3222863878, // 0xC0190006
      TransactionNotJoined = 3222863879, // 0xC0190007
      DirectoryNotRm = 3222863880, // 0xC0190008
      CouldNotResizeLog = 3222863881, // 0xC0190009
      TransactionsUnsupportedRemote = 3222863882, // 0xC019000A
      LogResizeInvalidSize = 3222863883, // 0xC019000B
      RemoteFileVersionMismatch = 3222863884, // 0xC019000C
      CrmProtocolAlreadyExists = 3222863887, // 0xC019000F
      TransactionPropagationFailed = 3222863888, // 0xC0190010
      CrmProtocolNotFound = 3222863889, // 0xC0190011
      TransactionSuperiorExists = 3222863890, // 0xC0190012
      TransactionRequestNotValid = 3222863891, // 0xC0190013
      TransactionNotRequested = 3222863892, // 0xC0190014
      TransactionAlreadyAborted = 3222863893, // 0xC0190015
      TransactionAlreadyCommitted = 3222863894, // 0xC0190016
      TransactionInvalidMarshallBuffer = 3222863895, // 0xC0190017
      CurrentTransactionNotValid = 3222863896, // 0xC0190018
      LogGrowthFailed = 3222863897, // 0xC0190019
      ObjectNoLongerExists = 3222863905, // 0xC0190021
      StreamMiniversionNotFound = 3222863906, // 0xC0190022
      StreamMiniversionNotValid = 3222863907, // 0xC0190023
      MiniversionInaccessibleFromSpecifiedTransaction = 3222863908, // 0xC0190024
      CantOpenMiniversionWithModifyIntent = 3222863909, // 0xC0190025
      CantCreateMoreStreamMiniversions = 3222863910, // 0xC0190026
      HandleNoLongerValid = 3222863912, // 0xC0190028
      NoTxfMetadata = 3222863913, // 0xC0190029
      LogCorruptionDetected = 3222863920, // 0xC0190030
      CantRecoverWithHandleOpen = 3222863921, // 0xC0190031
      RmDisconnected = 3222863922, // 0xC0190032
      EnlistmentNotSuperior = 3222863923, // 0xC0190033
      RecoveryNotNeeded = 3222863924, // 0xC0190034
      RmAlreadyStarted = 3222863925, // 0xC0190035
      FileIdentityNotPersistent = 3222863926, // 0xC0190036
      CantBreakTransactionalDependency = 3222863927, // 0xC0190037
      CantCrossRmBoundary = 3222863928, // 0xC0190038
      TxfDirNotEmpty = 3222863929, // 0xC0190039
      IndoubtTransactionsExist = 3222863930, // 0xC019003A
      TmVolatile = 3222863931, // 0xC019003B
      RollbackTimerExpired = 3222863932, // 0xC019003C
      TxfAttributeCorrupt = 3222863933, // 0xC019003D
      EfsNotAllowedInTransaction = 3222863934, // 0xC019003E
      TransactionalOpenNotAllowed = 3222863935, // 0xC019003F
      TransactedMappingUnsupportedRemote = 3222863936, // 0xC0190040
      TxfMetadataAlreadyPresent = 3222863937, // 0xC0190041
      TransactionScopeCallbacksNotSet = 3222863938, // 0xC0190042
      TransactionRequiredPromotion = 3222863939, // 0xC0190043
      CannotExecuteFileInTransaction = 3222863940, // 0xC0190044
      TransactionsNotFrozen = 3222863941, // 0xC0190045
      MaximumNtStatus = 4294967295, // 0xFFFFFFFF
    }
  }
}
