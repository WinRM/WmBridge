//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;

namespace WmBridge.Service
{
    public class ConfigSection : ConfigurationSection
    {
        private const string s_listeners = "listeners";
        private const string s_autodiscovery = "autodiscovery";
        private const string s_url = "url";
        private const string s_installation = "installation";
        private const string s_firewallException = "firewallException";
        private const string s_certificate = "certificate";
        private const string s_issuer = "issuer";

        internal static ConfigSection Default { get { return (ConfigSection)ConfigurationManager.GetSection("wmbridge"); } }

        [ConfigurationProperty(s_listeners, IsRequired = false)]
        public UrlCollection<ListenerUrlElement> Listeners
        {
            get { return (UrlCollection<ListenerUrlElement>)this[s_listeners]; }
        }

        [ConfigurationProperty(s_autodiscovery, IsRequired = false)]
        public UrlCollection<UrlElement> Autodiscovery
        {
            get { return (UrlCollection<UrlElement>)this[s_autodiscovery]; }
        }

        [ConfigurationProperty(s_installation, IsRequired = false)]
        public InstallationCollection Installation
        {
            get { return (InstallationCollection)this[s_installation]; }
        }

        public class UrlElement : ConfigurationElement
        {
            [ConfigurationProperty(s_url, IsRequired = true)]
            public String Url
            {
                get { return (String)this[s_url]; }
                set { this[s_url] = value; }
            }
        }

        public class ListenerUrlElement : UrlElement
        {
            [ConfigurationProperty(s_certificate, IsRequired = false)]
            public String Certificate
            {
                get { return (String)this[s_certificate]; }
                set { this[s_certificate] = value; }
            }

            [ConfigurationProperty(s_issuer, IsRequired = false)]
            public String Issuer
            {
                get { return (String)this[s_issuer]; }
                set { this[s_issuer] = value; }
            }

            internal string BindingIpPort
            {
                get
                {
                    string ip; UInt16 port;
                    if (ParseUrl(out ip, out port))
                        return ip + ":" + port;
                    else
                        return null;
                }
            }

            internal UInt16 Port
            {
                get
                {
                    string ip; UInt16 port;
                    if (ParseUrl(out ip, out port))
                        return port;
                    else
                        return 0;
                }
            }

            private bool ParseUrl(out string ip, out UInt16 port)
            {
                Regex rx = new Regex(@"((?<scheme>http[s]?)):\/\/(?<host>[^:\/\s]+)(:(?<port>(\d+)))?.*");

                ip = "";
                port = 0;

                var m = rx.Match(this.Url);
                if (m.Success)
                {
                    var scheme = m.Groups["scheme"].Value.ToUpper();
                    ip = m.Groups["host"].Value;

                    if (ip == "*")
                        ip = "0.0.0.0";

                    if (!UInt16.TryParse(m.Groups["port"].Value, out port))
                    {
                        if (scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase)) port = 80;
                        if (scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase)) port = 443;
                    }

                    return true;
                }

                return false;
            }
        }

        public class UrlCollection<T> : ConfigurationElementCollection, IEnumerable<T>
            where T : UrlElement, new()
        {
            public override ConfigurationElementCollectionType CollectionType
            {
                get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new T();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((T)element).Url;
            }

            public T this[int index]
            {
                get { return (T)BaseGet(index); }
            }

            public void Add(T element)
            {
                BaseAdd(element, true);
            }

            public void RemoveAll()
            {
                for (int i = Count - 1; i >= 0; i--) BaseRemoveAt(i);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return this.OfType<T>().GetEnumerator();
            }
        }

        public class InstallationCollection : KeyValueConfigurationCollection
        {
            public bool FirewallException
            {
                get { return Convert.ToBoolean(Get(s_firewallException)); }
                set { Set(s_firewallException, value); }
            }

            /// <summary>
            /// Returns <c>true</c> if config section contains explicit definition of FirewallException property
            /// </summary>
            public bool HasFirewallException
            {
                get { return this[s_firewallException] != null; }
            }

            private string Get(string key)
            {
                var elm = this[key];
                return elm == null ? null : elm.Value;
            }

            private void Set(string key, object value)
            {
                if (BaseGet(key) != null)
                    BaseRemove(key);

                if (value != null)
                    Add(key, Convert.ToString(value));
            }

        }
    }
}
