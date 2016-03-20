
namespace WmBridge.Support
{
    public static class TerminalServices
    {
        public static TerminalServicesSession[] GetSessions()
        {
            var infos = Win32Native.GetSessionInfos();

            var result = new TerminalServicesSession[infos.Count];

            for (int i = 0; i < infos.Count; i++)
                result[i] = new TerminalServicesSession(infos[i]);

            return result;
        }

        public static void Logoff(int sessionId, bool wait)
        {
            Win32Native.LogoffSession(sessionId, wait);
        }
        
        public static void Disconnect(int sessionId, bool wait)
        {
            Win32Native.DisconnectSession(sessionId, wait);
        }

        public static RemoteMessageBoxResult MessageBox(int sessionId, string title, string message, int style, int timeout, bool wait)
        {
            return Win32Native.SendMessage(sessionId, title, message, style, timeout, wait);
        }
    }
}
