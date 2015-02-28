//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using WmBridge.Service;

namespace WmBridge.Config
{
    public class ConfigBuilder
    {
        private const string svcExeName = "WmBridge.exe";
        private const string sectionName = "wmbridge";

        public string ListenerProtocol { get; set; }
        public string ListenerAddress { get; set; }
        public UInt16 ListenerPort { get; set; }

        public string SslCertificateHash { get; set; }
        public string SslIssuerCertificateHash { get; set; }

        public bool DiscoveryEnabled { get; set; }
        public string DiscoveryProtocol { get; set; }
        public string DiscoveryHost { get; set; }
        public UInt16 DiscoveryPort { get; set; }

        public bool SetupFirewall { get; set; }


        public static Configuration GetExeConfiguration(string directory = null)
        {
            string fileName;

            if (string.IsNullOrEmpty(directory))
                fileName = svcExeName;
            else
                fileName = Path.Combine(directory, svcExeName);

            var cfg = ConfigurationManager.OpenExeConfiguration(fileName);

            CreateSectionExternalFile(cfg);

            return cfg;
        }

        private static void CreateSectionExternalFile(Configuration config)
        {
            XDocument xdoc = XDocument.Load(config.FilePath);
            var xsection = xdoc.Descendants(sectionName).FirstOrDefault();
            if (xsection != null)
            {
                var attr = xsection.Attribute("configSource");

                if (attr != null)
                {
                    string configSource = attr.Value;
                    string extConfigFile = Path.Combine(Path.GetDirectoryName(config.FilePath), configSource);

                    if (!File.Exists(extConfigFile))
                        File.WriteAllText(extConfigFile, "<wmbridge />");
                }
            }
        }

        public static ConfigBuilder GetDefault()
        {
            return new ConfigBuilder(ConfigBuilder.GetExeConfiguration());
        }

        private class SimpleUri
        {
            public string Scheme;
            public string Host;
            public UInt16 Port;

            public SimpleUri() { }

            public SimpleUri(string scheme, string host, UInt16 port)
            {
                this.Scheme = scheme;
                this.Host = host;
                this.Port = port;
            }

            public override string ToString()
            {
                return string.Format("{0}://{1}:{2}", Scheme.ToLower(), Host, Port);
            }
        }

        private static SimpleUri ParseUri(string uri)
        {
            Regex rx = new Regex(@"((?<scheme>http[s]?)):\/\/(?<host>[^:\/\s]+)(:(?<port>(\d+)))?.*");

            var m = rx.Match(uri);
            if (m.Success)
            {
                var result = new SimpleUri()
                {
                    Scheme = m.Groups["scheme"].Value.ToUpper(),
                    Host = m.Groups["host"].Value,
                };

                UInt16 port;
                if (UInt16.TryParse(m.Groups["port"].Value, out port))
                {
                    result.Port = port;
                }
                else
                {
                    if (result.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase)) result.Port = 80;
                    if (result.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase)) result.Port = 443;
                }

                return result;
            }

            return null;
        }

        public ConfigBuilder(Configuration cfg)
        {
            var section = GetSection(cfg);

            var listener = section.Listeners.FirstOrDefault();
            if (listener != null)
            {
                var uri = ParseUri(listener.Url);
                ListenerProtocol = uri.Scheme;
                ListenerAddress = uri.Host;
                ListenerPort = uri.Port;

                SslCertificateHash = listener.Certificate;
                SslIssuerCertificateHash = listener.Issuer;
            }

            var autodiscovery = section.Autodiscovery.FirstOrDefault();
            if (autodiscovery != null)
            {
                var uri = ParseUri(autodiscovery.Url);
                DiscoveryProtocol = uri.Scheme;
                DiscoveryHost = uri.Host;
                DiscoveryPort = uri.Port;

                DiscoveryEnabled = true;
            }

            if (listener == null && autodiscovery == null)
            {
                // probably first initialization
                DiscoveryEnabled = true; 
            }

            SetupFirewall = section.Installation.FirewallException || !section.Installation.HasFirewallException;
        }

        private static ConfigSection GetSection(Configuration cfg)
        {
            var section = (ConfigSection)cfg.GetSection(sectionName);

            if (section == null)
                throw new Exception("Configuration section '" + sectionName + "' was not found.");
            else
                return section;
        }

        public void WriteTo(Configuration cfg)
        {
            var section = GetSection(cfg);

            section.Listeners.RemoveAll();
            if (!string.IsNullOrEmpty(ListenerAddress))
            {
                section.Listeners.Add(new ConfigSection.ListenerUrlElement()
                    {
                        Url = new SimpleUri(ListenerProtocol, ListenerAddress, ListenerPort) + "/",
                        Certificate = this.SslCertificateHash,
                        Issuer = this.SslIssuerCertificateHash
                    });
            }

            section.Autodiscovery.RemoveAll();
            if (DiscoveryEnabled && !string.IsNullOrEmpty(DiscoveryHost))
                section.Autodiscovery.Add(new ConfigSection.UrlElement() { Url = new SimpleUri(DiscoveryProtocol, DiscoveryHost, DiscoveryPort).ToString() });

            section.Installation.FirewallException = this.SetupFirewall;
        }

        public void WriteTo(string directory = null)
        {
            var cfg = ConfigBuilder.GetExeConfiguration(directory);
            WriteTo(cfg);
            cfg.Save();
        }
    }
}
