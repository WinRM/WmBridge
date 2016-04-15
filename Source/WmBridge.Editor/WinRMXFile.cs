using Newtonsoft.Json;
using PlistCS;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace WmBridge.Editor
{
    public static class WinRMXFile
    {
        private class WinRMXFileStruct
        {
            public string version;
            public List<ComputerInfo> computers;
            public List<CredentialsInfo> credentials;
            public List<WebServiceInfo> webServices;
        }

        private class ComputerInfo
        {
            public int authentication;
            public int credentialsIndex;
            public int webServiceIndex;
            public string displayName;
            public string executionPolicy;
            public string group;
            public string hostName;
            public bool showAvailability;
            public string startupScript;

            public override string ToString()
            {
                return string.IsNullOrEmpty(displayName) ? hostName : displayName;
            }
        }

        private class CredentialsInfo
        {
            public string userName;

            public override bool Equals(object obj)
            {
                var other = obj as CredentialsInfo;
                if (other == null)
                    return false;

                return userName.Equals(other.userName);
            }

            public override int GetHashCode()
            {
                return userName.GetHashCode();
            }

            public override string ToString()
            {
                return userName;
            }
        }

        private class WebServiceInfo
        {
            public string url;

            public override int GetHashCode()
            {
                return url.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var other = obj as WebServiceInfo;
                if (other == null)
                    return false;

                return url.Equals(other.url);
            }

            public override string ToString()
            {
                return url;
            }
        }

        public static List<ConnectionEntry> Load(string path)
        {
            var plist = Plist.readPlist(Decompress(File.ReadAllBytes(path)));

            // map dictionary to object using JsonConvert
            var data = JsonConvert.DeserializeObject<WinRMXFileStruct>(JsonConvert.SerializeObject(plist));

            var result = new List<ConnectionEntry>();

            foreach (var comp in data.computers)
            {
                var entry = new ConnectionEntry()
                {
                    Description = comp.displayName,
                    HostName = comp.hostName,
                    WebServiceURL = data.webServices[comp.webServiceIndex].url,
                    ExecutionPolicy = comp.executionPolicy,
                    GroupName = comp.group,
                    StartupScript = comp.startupScript,
                    UseCredSSP = comp.authentication == 4,
                    UserName = data.credentials[comp.credentialsIndex].userName,
                    ShowAvailability = comp.showAvailability
                };

                result.Add(entry);
            }

            return result;
        }

        public static void Save(string path, IEnumerable<ConnectionEntry> list)
        {
            var computers = new List<ComputerInfo>();
            var credentials = new List<CredentialsInfo>();
            var webServices = new List<WebServiceInfo>();

            var usedCredentials = new Dictionary<CredentialsInfo, int>();
            var usedWebServices = new Dictionary<WebServiceInfo, int>();
            
            foreach (var item in list)
            {
                var cred = new CredentialsInfo() { userName = item.UserName };
                var ws = new WebServiceInfo() { url = item.WebServiceURL };

                if (!usedCredentials.ContainsKey(cred))
                {
                    credentials.Add(cred);
                    usedCredentials.Add(cred, usedCredentials.Count);
                }

                if (!usedWebServices.ContainsKey(ws))
                {
                    webServices.Add(ws);
                    usedWebServices.Add(ws, usedWebServices.Count);
                }

                var comp = new ComputerInfo()
                {
                    displayName = item.Description,
                    hostName = item.HostName,
                    authentication = item.UseCredSSP ? 4 : 0,
                    executionPolicy = item.ExecutionPolicy,
                    group = item.GroupName,
                    startupScript = item.StartupScript,
                    showAvailability = item.ShowAvailability,
                    credentialsIndex = usedCredentials[cred],
                    webServiceIndex = usedWebServices[ws]
                };

                computers.Add(comp);
            }

            var data = new WinRMXFileStruct()
            {
                version = "1.9", // compatible version
                computers = computers,
                credentials = credentials,
                webServices = webServices
            };

            var plist = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(data));
            File.WriteAllBytes(path, Compress(Encoding.UTF8.GetBytes(Plist.writeXml(plist))));
        }

        static byte[] Compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}
