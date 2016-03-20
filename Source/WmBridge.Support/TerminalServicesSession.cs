using System;
using System.ComponentModel;
using System.Net;
using System.Security.Principal;

namespace WmBridge.Support
{
    public class TerminalServicesSession
    {
        public int SessionId { get; private set; }
        public string UserName { get; private set; }
        public EndPoint RemoteEndPoint { get; private set; }
        public ClientProtocolType ClientProtocolType { get; private set; }
        public IPAddress ClientIPAddress { get; private set; }
        public string ClientName { get; private set; }
        public IPAddress SessionIPAddress { get; private set; }
        public string WindowStationName { get; private set; }
        public string DomainName { get; private set; }
        public ConnectionState ConnectionState { get; private set; }
        public DateTime? ConnectTime { get; private set; }
        public DateTime? CurrentTime { get; private set; }
        public DateTime? DisconnectTime { get; private set; }
        public DateTime? LastInputTime { get; private set; }
        public DateTime? LoginTime { get; private set; }
        public ProtocolStatistics IncomingStatistics { get; private set; }
        public ProtocolStatistics OutgoingStatistics { get; private set; }

        public NTAccount UserAccount
        {
            get { return (string.IsNullOrEmpty(UserName) ? null : new NTAccount(DomainName, UserName)); }
        }

        public TimeSpan IdleTime
        {
            get
            {
                if (ConnectionState == ConnectionState.Disconnected)
                {
                    if (CurrentTime != null && DisconnectTime != null)
                    {
                        return CurrentTime.Value - DisconnectTime.Value;
                    }
                }
                else
                {
                    if (CurrentTime != null && LastInputTime != null)
                    {
                        return CurrentTime.Value - LastInputTime.Value;
                    }
                }
                return TimeSpan.Zero;
            }
        }

        public TerminalServicesSession(int sessionId)
        {
            SessionId = sessionId;
            ClientProtocolType = (ClientProtocolType)Win32Native.QuerySessionInformationForShort(SessionId, WTS_INFO_CLASS.WTSClientProtocolType);
            ClientName = Win32Native.QuerySessionInformationForString(SessionId, WTS_INFO_CLASS.WTSClientName);

            try
            {
                RemoteEndPoint = Win32Native.QuerySessionInformationForEndPoint(SessionId);
            }
            catch (Win32Exception) { }

            try
            {
                var clientAddress = Win32Native.QuerySessionInformationForStruct<WTS_CLIENT_ADDRESS>(SessionId, WTS_INFO_CLASS.WTSClientAddress);
                ClientIPAddress = Win32Native.ExtractIPAddress(clientAddress.AddressFamily, clientAddress.Address);
            }
            catch (Win32Exception) { }

            try
            {
                var sessionAddress = Win32Native.QuerySessionInformationForStruct<WTS_SESSION_ADDRESS>(SessionId, WTS_INFO_CLASS.WTSSessionAddressV4);
                SessionIPAddress = Win32Native.ExtractIPAddress(sessionAddress.AddressFamily, sessionAddress.Address);
            }
            catch (Win32Exception) { }


            if (IsVistaSp1OrHigher)
                LoadWtsInfoProperties();
            else
                LoadWinStationInformationProperties();

        }

        internal TerminalServicesSession(WTS_SESSION_INFO sessionInfo)
            : this(sessionInfo.SessionID)
        {
            WindowStationName = sessionInfo.WinStationName;
            ConnectionState = sessionInfo.State;
        }

        private static bool IsVistaSp1OrHigher
        {
            get { return Environment.OSVersion.Version >= new Version(6, 0, 6001); }
        }

        private void LoadWinStationInformationProperties()
        {
            var wsInfo = Win32Native.GetWinStationInformation(SessionId);
            WindowStationName = wsInfo.WinStationName;
            ConnectionState = wsInfo.State;
            ConnectTime = Win32Native.FileTimeToDateTime(wsInfo.ConnectTime);
            CurrentTime = Win32Native.FileTimeToDateTime(wsInfo.CurrentTime);
            DisconnectTime = Win32Native.FileTimeToDateTime(wsInfo.DisconnectTime);
            LastInputTime = Win32Native.FileTimeToDateTime(wsInfo.LastInputTime);
            LoginTime = Win32Native.FileTimeToDateTime(wsInfo.LoginTime);
            UserName = wsInfo.UserName;
            DomainName = wsInfo.Domain;
            IncomingStatistics = new ProtocolStatistics(wsInfo.ProtocolStatus.Input);
            OutgoingStatistics = new ProtocolStatistics(wsInfo.ProtocolStatus.Output);
        }

        private void LoadWtsInfoProperties()
        {
            var info = Win32Native.QuerySessionInformationForStruct<WTSINFO>(SessionId, WTS_INFO_CLASS.WTSSessionInfo);
            ConnectionState = info.State;
            IncomingStatistics = new ProtocolStatistics(info.IncomingBytes, info.IncomingFrames, info.IncomingCompressedBytes);
            OutgoingStatistics = new ProtocolStatistics(info.OutgoingBytes, info.OutgoingFrames, info.OutgoingCompressedBytes);
            WindowStationName = info.WinStationName;
            DomainName = info.Domain;
            UserName = info.UserName;
            ConnectTime = Win32Native.FileTimeToDateTime(info.ConnectTime);
            DisconnectTime = Win32Native.FileTimeToDateTime(info.DisconnectTime);
            LastInputTime = Win32Native.FileTimeToDateTime(info.LastInputTime);
            LoginTime = Win32Native.FileTimeToDateTime(info.LogonTime);
            CurrentTime = Win32Native.FileTimeToDateTime(info.CurrentTime);
        }

    }

    public class ProtocolStatistics
    {
        public int Bytes { get; private set; }
        public int Frames { get; private set; }
        public int CompressedBytes { get; private set; }

        public ProtocolStatistics(int bytes, int frames, int compressedBytes)
        {
            Bytes = bytes;
            Frames = frames;
            CompressedBytes = compressedBytes;
        }

        internal ProtocolStatistics(PROTOCOLCOUNTERS counters) : this (counters.Bytes, counters.Frames, counters.CompressedBytes) { }

        public override string ToString()
        {
            return Bytes.ToString();
        }
    }
}
