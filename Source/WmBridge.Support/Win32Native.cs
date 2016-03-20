using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace WmBridge.Support
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct WTS_PROCESS_INFO
    {
        public int SessionId;
        public int ProcessId;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string ProcessName;

        public IntPtr UserSid;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WTS_SESSION_INFO
    {
        public int SessionID;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string WinStationName;

        public ConnectionState State;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WTS_SESSION_ADDRESS
    {
        public AddressFamily AddressFamily;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] Address;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WTS_CLIENT_ADDRESS
    {
        public AddressFamily AddressFamily;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] Address;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROTOCOLCOUNTERS
    {
        public int WdBytes;
        public int WdFrames;
        public int WaitForOutBuf;
        public int Frames;
        public int Bytes;
        public int CompressedBytes;
        public int CompressFlushes;
        public int Errors;
        public int Timeouts;
        public int AsyncFramingError;
        public int AsyncOverrunError;
        public int AsyncOverflowError;
        public int AsyncParityError;
        public int TdErrors;
        public short ProtocolType;
        public short Length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public int[] Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CACHE_STATISTICS
    {
        private readonly short ProtocolType;
        private readonly short Length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        private readonly int[] Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROTOCOLSTATUS
    {
        public PROTOCOLCOUNTERS Output;
        public PROTOCOLCOUNTERS Input;
        public CACHE_STATISTICS Statistics;
        public int AsyncSignal;
        public int AsyncSignalMask;
    }

    /// <summary>
    /// The relevant sections from winsta.h:
    /// 
    /// #define WINSTATIONNAME_LENGTH     32
    /// typedef WCHAR WINSTATIONNAME[ WINSTATIONNAME_LENGTH + 1 ];
    /// 
    /// typedef struct _PROTOCOLCOUNTERS { (size = 460)
    ///     ULONG WdBytes;             
    ///     ULONG WdFrames;            
    ///     ULONG WaitForOutBuf;       
    ///     ULONG Frames;              
    ///     ULONG Bytes;               
    ///     ULONG CompressedBytes;     
    ///     ULONG CompressFlushes;     
    ///     ULONG Errors;              
    ///     ULONG Timeouts;            
    ///     ULONG AsyncFramingError;   
    ///     ULONG AsyncOverrunError;   
    ///     ULONG AsyncOverflowError;  
    ///     ULONG AsyncParityError;    
    ///     ULONG TdErrors;            
    ///     USHORT ProtocolType;       
    ///     USHORT Length;             
    ///     union {
    ///         TSHARE_COUNTERS TShareCounters;
    ///         ULONG           Reserved[100];
    ///     } Specific;
    /// } PROTOCOLCOUNTERS, * PPROTOCOLCOUNTERS;
    /// 
    /// typedef struct CACHE_STATISTICS { (size = 84)
    ///     USHORT ProtocolType;    
    ///     USHORT Length;          
    ///     union {
    ///         RESERVED_CACHE    ReservedCacheStats;
    ///         TSHARE_CACHE TShareCacheStats;
    ///         ULONG        Reserved[20];
    ///     } Specific;
    /// } CACHE_STATISTICS, * PCACHE_STATISTICS;
    /// 
    /// typedef struct _PROTOCOLSTATUS { (size = 1012)
    ///     PROTOCOLCOUNTERS Output;
    ///     PROTOCOLCOUNTERS Input;
    ///     CACHE_STATISTICS Cache;
    ///     ULONG AsyncSignal;     
    ///     ULONG AsyncSignalMask; 
    /// } PROTOCOLSTATUS, * PPROTOCOLSTATUS;
    /// 
    /// #define DOMAIN_LENGTH             17
    /// #define USERNAME_LENGTH           20
    /// 
    /// typedef struct _WINSTATIONINFORMATION {
    ///     WINSTATIONSTATECLASS ConnectState;
    ///     WINSTATIONNAME WinStationName;
    ///     ULONG LogonId;
    ///     LARGE_INTEGER ConnectTime; // There seems to be an extra int just before this field
    ///     LARGE_INTEGER DisconnectTime;
    ///     LARGE_INTEGER LastInputTime;
    ///     LARGE_INTEGER LogonTime;
    ///     PROTOCOLSTATUS Status;
    ///     WCHAR Domain[DOMAIN_LENGTH + 1]; // This is incorrect; it should be USERNAME_LENGTH + 1
    ///     WCHAR UserName[USERNAME_LENGTH + 1];
    ///     LARGE_INTEGER CurrentTime;
    /// } WINSTATIONINFORMATION, * PWINSTATIONINFORMATION;
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WINSTATIONINFORMATIONW
    {
        public ConnectionState State;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string WinStationName;

        public int SessionId;
        public int Unknown;
        public FILETIME ConnectTime;
        public FILETIME DisconnectTime;
        public FILETIME LastInputTime;
        public FILETIME LoginTime;
        public PROTOCOLSTATUS ProtocolStatus;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
        public string Domain;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
        public string UserName;

        public FILETIME CurrentTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEMTIME
    {
        public short Year;
        public short Month;
        public short DayOfWeek;
        public short Day;
        public short Hour;
        public short Minute;
        public short Second;
        public short Milliseconds;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINSTATIONREMOTEADDRESS
    {
        public AddressFamily Family;
        public short Port;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] Address;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] Reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct WTSINFO
    {
        public ConnectionState State;
        public int SessionId;
        public int IncomingBytes;
        public int OutgoingBytes;
        public int IncomingFrames;
        public int OutgoingFrames;
        public int IncomingCompressedBytes;
        public int OutgoingCompressedBytes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string WinStationName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string Domain;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string UserName;

        [MarshalAs(UnmanagedType.I8)]
        public long ConnectTime;

        [MarshalAs(UnmanagedType.I8)]
        public long DisconnectTime;

        [MarshalAs(UnmanagedType.I8)]
        public long LastInputTime;

        [MarshalAs(UnmanagedType.I8)]
        public long LogonTime;

        [MarshalAs(UnmanagedType.I8)]
        public long CurrentTime;
    }

    /// <summary>
    /// Specifies the user's response to a message box shown with the
    /// <see cref="MessageBox(string, string, RemoteMessageBoxButtons, RemoteMessageBoxIcon, RemoteMessageBoxDefaultButton, RemoteMessageBoxOptions, TimeSpan, bool)">
    /// ITerminalServicesSession.MessageBox</see> method.
    /// </summary>
    public enum RemoteMessageBoxResult
    {
        /// <summary>
        /// The user pressed the "OK" button.
        /// </summary>
        Ok = 1,
        /// <summary>
        /// The user pressed the "Cancel" button.
        /// </summary>
        Cancel = 2,
        /// <summary>
        /// The user pressed the "Abort" button.
        /// </summary>
        Abort = 3,
        /// <summary>
        /// The user pressed the "Retry" button.
        /// </summary>
        Retry = 4,
        /// <summary>
        /// The user pressed the "Ignore" button.
        /// </summary>
        Ignore = 5,
        /// <summary>
        /// The user pressed the "Yes" button.
        /// </summary>
        Yes = 6,
        /// <summary>
        /// The user pressed the "No" button.
        /// </summary>
        No = 7,
        /// <summary>
        /// The timeout period expired before the user responded to the message box.
        /// </summary>
        Timeout = 0x7D00,
        /// <summary>
        /// The <c>synchronous</c> parameter of <see cref="MessageBox(string, string, RemoteMessageBoxButtons, RemoteMessageBoxIcon, RemoteMessageBoxDefaultButton, RemoteMessageBoxOptions, TimeSpan, bool)" />
        /// was set to false, so the method returned immediately, without waiting for a response
        /// from the user.
        /// </summary>
        Asynchronous = 0x7D01,
    }

    /// <summary>
    /// Connection state of a session.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// A user is logged on to the session.
        /// </summary>
        Active,
        /// <summary>
        /// A client is connected to the session.
        /// </summary>
        Connected,
        /// <summary>
        /// The session is in the process of connecting to a client.
        /// </summary>
        ConnectQuery,
        /// <summary>
        /// This session is shadowing another session.
        /// </summary>
        Shadowing,
        /// <summary>
        /// The session is active, but the client has disconnected from it.
        /// </summary>
        Disconnected,
        /// <summary>
        /// The session is waiting for a client to connect.
        /// </summary>
        Idle,
        /// <summary>
        /// The session is listening for connections.
        /// </summary>
        Listening,
        /// <summary>
        /// The session is being reset.
        /// </summary>
        Reset,
        /// <summary>
        /// The session is down due to an error.
        /// </summary>
        Down,
        /// <summary>
        /// The session is initializing.
        /// </summary>
        Initializing
    }

    internal enum WTS_INFO_CLASS
    {
        WTSInitialProgram = 0,
        WTSApplicationName = 1,
        WTSWorkingDirectory = 2,
        WTSOEMId = 3,
        WTSSessionId = 4,
        WTSUserName = 5,
        WTSWinStationName = 6,
        WTSDomainName = 7,
        WTSConnectState = 8,
        WTSClientBuildNumber = 9,
        WTSClientName = 10,
        WTSClientDirectory = 11,
        WTSClientProductId = 12,
        WTSClientHardwareId = 13,
        WTSClientAddress = 14,
        WTSClientDisplay = 15,
        WTSClientProtocolType = 16,
        WTSIdleTime = 17,
        WTSLogonTime = 18,
        WTSIncomingBytes = 19,
        WTSOutgoingBytes = 20,
        WTSIncomingFrames = 21,
        WTSOutgoingFrames = 22,
        WTSClientInfo = 23,
        WTSSessionInfo = 24,
        WTSSessionInfoEx = 25,
        WTSConfigInfo = 26,
        WTSValidationInfo = 27,
        WTSSessionAddressV4 = 28,
        WTSIsRemoteSession = 29
    }

    internal enum WINSTATIONINFOCLASS
    {
        WinStationCreateData,
        WinStationConfiguration,
        WinStationPdParams,
        WinStationWd,
        WinStationPd,
        WinStationPrinter,
        WinStationClient,
        WinStationModules,
        WinStationInformation,
        WinStationTrace,
        WinStationBeep,
        WinStationEncryptionOff,
        WinStationEncryptionPerm,
        WinStationNtSecurity,
        WinStationUserToken,
        WinStationUnused1,
        WinStationVideoData,
        WinStationInitialProgram,
        WinStationCd,
        WinStationSystemTrace,
        WinStationVirtualData,
        WinStationClientData,
        WinStationSecureDesktopEnter,
        WinStationSecureDesktopExit,
        WinStationLoadBalanceSessionTarget,
        WinStationLoadIndicator,
        WinStationShadowInfo,
        WinStationDigProductId,
        WinStationLockedState,
        WinStationRemoteAddress,
        WinStationIdleTime,
        WinStationLastReconnectType,
        WinStationDisallowAutoReconnect,
        WinStationUnused2,
        WinStationUnused3,
        WinStationUnused4,
        WinStationUnused5,
        WinStationReconnectedFromId,
        WinStationEffectsPolicy,
        WinStationType,
        WinStationInformationEx
    }

    /// <summary>
    /// The protocol that a client is using to connect to a terminal server as returned by 
    /// <see cref="ITerminalServicesSession.ClientProtocolType" />.
    /// </summary>
    public enum ClientProtocolType : short
    {
        /// <summary>
        /// The client is directly connected to the console session.
        /// </summary>
        Console = 0,
        /// <summary>
        /// This value exists for legacy purposes.
        /// </summary>
        Legacy = 1,
        /// <summary>
        /// The client is connected via the RDP protocol.
        /// </summary>
        Rdp = 2,
    }

    internal static class Win32Native
    {
        [DllImport("Wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass,
                                                             out IntPtr buffer, out int bytesReturned);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Int32 WTSEnumerateSessions(IntPtr hServer, int reserved, int version,
                                                        out IntPtr sessionInfo, out int count);

        [DllImport("wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr memory);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern int WTSLogoffSession(IntPtr hServer, int sessionId, bool wait);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern int WTSDisconnectSession(IntPtr hServer, int sessionId, bool wait);

        [DllImport("winsta.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int WinStationQueryInformation(IntPtr hServer, int sessionId, int information,
                                                            ref WINSTATIONINFORMATIONW buffer, int bufferLength,
                                                            ref int returnedLength);

        [DllImport("winsta.dll", CharSet = CharSet.Unicode, EntryPoint = "WinStationQueryInformationW",
            SetLastError = true)]
        static extern int WinStationQueryInformationRemoteAddress(IntPtr hServer, int sessionId,
                                                                         WINSTATIONINFOCLASS information,
                                                                         ref WINSTATIONREMOTEADDRESS buffer,
                                                                         int bufferLength, out int returnedLength);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int WTSSendMessage(IntPtr hServer, int sessionId,
                                                [MarshalAs(UnmanagedType.LPTStr)] string title, int titleLength,
                                                [MarshalAs(UnmanagedType.LPTStr)] string message, int messageLength,
                                                int style, int timeout, out RemoteMessageBoxResult result, bool wait);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern int WTSShutdownSystem(IntPtr hServer, int shutdownFlag);

        [DllImport("ws2_32.dll")]
        static extern ushort ntohs(ushort netValue);

        [DllImport("kernel32.dll")]
        static extern int FileTimeToSystemTime(ref FILETIME fileTime, ref SYSTEMTIME systemTime);

        [DllImport("kernel32.dll")]
        static extern int WTSGetActiveConsoleSessionId();

        public static ConnectionState GetConnectionState(int sessionId)
        {
            return QuerySessionInformation(sessionId, WTS_INFO_CLASS.WTSConnectState,
                                           (mem, returned) => (ConnectionState)Marshal.ReadInt32(mem));
        }

        private static T QuerySessionInformation<T>(int sessionId, WTS_INFO_CLASS infoClass, ProcessSessionCallback<T> callback)
        {
            int returned;
            IntPtr mem;
            if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, infoClass, out mem, out returned))
            {
                try
                {
                    return callback(mem, returned);
                }
                finally
                {
                    if (mem != IntPtr.Zero)
                    {
                        WTSFreeMemory(mem);
                    }
                }
            }
            throw new Win32Exception();
        }

        public static string QuerySessionInformationForString(int sessionId, WTS_INFO_CLASS infoClass)
        {
            return QuerySessionInformation(sessionId, infoClass, (mem, returned) => mem == IntPtr.Zero ? null : Marshal.PtrToStringAuto(mem));
        }

        public static T QuerySessionInformationForStruct<T>(int sessionId, WTS_INFO_CLASS infoClass) where T : struct
        {
            return QuerySessionInformation(sessionId, infoClass, (mem, returned) => (T)Marshal.PtrToStructure(mem, typeof(T)));
        }

        public static WINSTATIONINFORMATIONW GetWinStationInformation(int sessionId)
        {
            var retLen = 0;
            var wsInfo = new WINSTATIONINFORMATIONW();
            if (
                WinStationQueryInformation(IntPtr.Zero, sessionId,
                                                         (int)WINSTATIONINFOCLASS.WinStationInformation, ref wsInfo,
                                                         Marshal.SizeOf(typeof(WINSTATIONINFORMATIONW)), ref retLen) !=
                0)
            {
                return wsInfo;
            }
            throw new Win32Exception();
        }

        public static DateTime? FileTimeToDateTime(FILETIME ft)
        {
            var sysTime = new SYSTEMTIME();
            if (FileTimeToSystemTime(ref ft, ref sysTime) == 0)
            {
                return null;
            }
            if (sysTime.Year < 1900)
            {
                // Must have gotten a bogus date. This happens sometimes on Windows Server 2003.
                return null;
            }
            return
                new DateTime(sysTime.Year, sysTime.Month, sysTime.Day, sysTime.Hour, sysTime.Minute, sysTime.Second,
                             sysTime.Milliseconds, DateTimeKind.Utc).ToLocalTime();
        }

        public static IList<WTS_SESSION_INFO> GetSessionInfos()
        {
            IntPtr ppSessionInfo;
            int count;

            if (WTSEnumerateSessions(IntPtr.Zero, 0, 1, out ppSessionInfo, out count) == 0)
            {
                throw new Win32Exception();
            }
            try
            {
                return PtrToStructureList<WTS_SESSION_INFO>(ppSessionInfo, count);
            }
            finally
            {
                WTSFreeMemory(ppSessionInfo);
            }
        }

        public static void LogoffSession(int sessionId, bool wait)
        {
            if (WTSLogoffSession(IntPtr.Zero, sessionId, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

        public static void DisconnectSession(int sessionId, bool wait)
        {
            if (WTSDisconnectSession(IntPtr.Zero, sessionId, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

        public static RemoteMessageBoxResult SendMessage(int sessionId, string title, string message, int style, int timeout, bool wait)
        {
            RemoteMessageBoxResult result;
            // If you pass an empty title string to WTSSendMessage, Server 2003 returns a bizarre error code
            // (-2147467259: "The stub received bad data"). Windows 7 and Server 2008 R2 do not return an error code,
            // but display the text "Error" instead of the empty string.
            title = string.IsNullOrEmpty(title) ? " " : title;
            message = message ?? string.Empty;
            if (
                WTSSendMessage(IntPtr.Zero, sessionId, title, title.Length * Marshal.SystemDefaultCharSize,
                                             message, message.Length * Marshal.SystemDefaultCharSize, style, timeout,
                                             out result, wait) == 0)
            {
                throw new Win32Exception();
            }
            return result;
        }

        private static IList<T> PtrToStructureList<T>(IntPtr ppList, int count) where T : struct
        {
            var result = new List<T>();
            var pointer = ppList.ToInt64();
            var sizeOf = Marshal.SizeOf(typeof(T));
            for (var index = 0; index < count; index++)
            {
                var item = (T)Marshal.PtrToStructure(new IntPtr(pointer), typeof(T));
                result.Add(item);
                pointer += sizeOf;
            }
            return result;
        }

        public static int QuerySessionInformationForInt(int sessionId, WTS_INFO_CLASS infoClass)
        {
            return QuerySessionInformation(sessionId, infoClass, (mem, returned) => Marshal.ReadInt32(mem));
        }

        public static void ShutdownSystem(int flags)
        {
            if (WTSShutdownSystem(IntPtr.Zero, flags) == 0)
            {
                throw new Win32Exception();
            }
        }

        public static DateTime? FileTimeToDateTime(long fileTime)
        {
            if (fileTime == 0)
            {
                return null;
            }
            return DateTime.FromFileTime(fileTime);
        }

        public static short QuerySessionInformationForShort(int sessionId, WTS_INFO_CLASS infoClass)
        {
            return QuerySessionInformation(sessionId, infoClass, (mem, returned) => Marshal.ReadInt16(mem));
        }

        public static IPAddress ExtractIPAddress(AddressFamily family, byte[] rawAddress)
        {
            switch (family)
            {
                case AddressFamily.InterNetwork:
                    var v4Addr = new byte[4];
                    // TODO: I'm not sure what type of address structure this is that we need to start at offset 2.
                    Array.Copy(rawAddress, 2, v4Addr, 0, 4);
                    return new IPAddress(v4Addr);
                case AddressFamily.InterNetworkV6:
                    var v6Addr = new byte[16];
                    Array.Copy(rawAddress, 2, v6Addr, 0, 16);
                    return new IPAddress(v6Addr);
            }
            return null;
        }

        public static EndPoint QuerySessionInformationForEndPoint(int sessionId)
        {
            int retLen;
            var remoteAddress = new WINSTATIONREMOTEADDRESS();
            if (
                WinStationQueryInformationRemoteAddress(IntPtr.Zero, sessionId,
                                                                      WINSTATIONINFOCLASS.WinStationRemoteAddress,
                                                                      ref remoteAddress,
                                                                      Marshal.SizeOf(typeof(WINSTATIONREMOTEADDRESS)),
                                                                      out retLen) != 0)
            {
                var ipAddress = ExtractIPAddress(remoteAddress.Family, remoteAddress.Address);
                int port = ntohs((ushort)remoteAddress.Port);
                return ipAddress == null ? null : new IPEndPoint(ipAddress, port);
            }
            throw new Win32Exception();
        }

        public static int? GetActiveConsoleSessionId()
        {
            var sessionId = WTSGetActiveConsoleSessionId();
            return sessionId == -1 ? (int?)null : sessionId;
        }

        #region Nested type: ProcessSessionCallback

        private delegate T ProcessSessionCallback<T>(IntPtr mem, int returnedBytes);

        #endregion
    }
}