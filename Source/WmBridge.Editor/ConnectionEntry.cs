using System;
using System.Collections.Generic;

namespace WmBridge.Editor
{
    public class ConnectionEntry
    {
        public string Description { get; set; }
        public string HostName { get; set; }
        public string GroupName { get; set; }
        public string WebServiceURL { get; set; }
        public string UserName { get; set; }
        public string ExecutionPolicy { get; set; }
        public bool UseCredSSP { get; set; }
        public bool ShowAvailability { get; set; }
        public string StartupScript { get; set; }

        public override string ToString()
        {
            return (string.IsNullOrEmpty(Description) ? HostName : Description) ?? "";
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(HostName) ||
                string.IsNullOrEmpty(WebServiceURL) ||
                string.IsNullOrEmpty(UserName))
                return false;

            return true;
        }

        public ConnectionEntry Clone()
        {
            return (ConnectionEntry)MemberwiseClone();
        }
    }
}
