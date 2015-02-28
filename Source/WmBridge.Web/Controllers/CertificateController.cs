//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Http;

namespace WmBridge.Web.Controllers
{
    [RoutePrefix("cert")]
    public class CertificateController : PSApiController
    {
        [Route("{location:regex(^(CurrentUser|LocalMachine)$)}/{store}"), HttpGet]
        public IHttpActionResult GetCertificates(string location, string store)
        {
            return Json(InvokePowerShell("Get-ChildItem \"cert:\\$($args[0])\\$($args[1])\" | sort FriendlyName", GetCertificateProperties(location, store),
                location, store));
        }

        [Route("{location:regex(^(CurrentUser|LocalMachine)$)}/{store}/{thumbprint}"), HttpGet]
        public IHttpActionResult GetCertificate(string location, string store, string thumbprint)
        {
            return Json(InvokePowerShell("Get-Item \"cert:\\$($args[0])\\$($args[1])\\$($args[2])\"", GetCertificateProperties(location, store),
                location, store, thumbprint).Single());
        }

        static protected WmBridge.Web.Model.PSPropertySelector[] GetCertificateProperties(string location, string store)
        {
            return PSSelect("FriendlyName", "HasPrivateKey", "SerialNumber", "Thumbprint", "Issuer", "Subject",
                "NotAfter".Expression("$_.NotAfter.ToUniversalTime()"), "NotBefore".Expression("$_.NotBefore.ToUniversalTime()"),
                "Subject".Alias("IssuedTo").Transform(CertCaption),
                "Issuer".Alias("IssuedBy").Transform(CertCaption),
                "Location".Transform(_ => LocationCaption(location) + "\\" + StoreCaption(store)));
        }

        [Route("{location:regex(^(CurrentUser|LocalMachine)$)}"), HttpGet]
        public IHttpActionResult GetCertStores(string location)
        {
            return Json(InvokePowerShell("Get-ChildItem \"cert:\\$($args[0])\"", PSSelect("Name", "Name".Alias("Caption").Transform(StoreCaption)), location));
        }

        private static readonly Dictionary<string, string> storeCaptions = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) 
        {
            { "AddressBook", "Other People" },
            { "AuthRoot", "Third-Party Root CAs" },
            { "CA", "Intermediate CAs" },
            { "ClientAuthIssuer", "Client Authentication Issuers" },
            { "Disallowed", "Untrused Certificates" },
            { "My", "Personal" },
            { "REQUEST", "Certificate Enrollment Requests" },
            { "Remote Desktop", "Remote Desktop" },
            { "Root", "Trusted Root CAs" },
            { "SmartCardRoot", "Smart Card Trusted Roots" },
            { "Trust", "Enterprise Trust" },
            { "TrustedDevices", "Trusted Devices" },
            { "TrustedPeople", "Trusted People" },
            { "TrustedPublisher", "Trusted Publisher" },
            { "UserDS", "Active Directory User Object" },
            { "WebHosting", "Web Hosting" }
        };

        private static object StoreCaption(object name)
        {
            string caption;
            if (storeCaptions.TryGetValue(name.ToString(), out caption) == false)
                return name;
            else
                return caption;
        }

        private static object LocationCaption(object name)
        {
            if (name.ToString().Equals("CurrentUser", StringComparison.InvariantCultureIgnoreCase))
                return "Current User";
            else if (name.ToString().Equals("LocalMachine", StringComparison.InvariantCultureIgnoreCase))
                return "Local Machine";
            else
                return name;
        }

        private static object CertCaption(object source)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

            string str = (string)source;

            bool inKey = true, inValue = false, inQuote = false;
            string key = "", value = "";
            int start = 0;

            for (int i = 0; i < str.Length; i++)
            {
                if (inKey)
                {
                    i = str.IndexOf('=', start);
                    key = str.Substring(start, i - start).Trim();
                    inValue = true;
                    inKey = false;

                    i++;
                    inQuote = str[i] == '\"';
                    if (inQuote) i++;

                    start = i;
                }

                bool valueEnd = false;
                if (inValue)
                {
                    if (inQuote)
                    {
                        if (str[i] == '"' && str[i-1] != '\\') // skip escaped backslash
                            valueEnd = true;
                    }
                    else
                    {
                        if (str[i] == ',')
                            valueEnd = true;
                    }

                    if (!valueEnd && i >= str.Length - 1)
                    {
                        valueEnd = true;
                        i++;
                    }
                }

                if (valueEnd)
                {
                    value = str.Substring(start, i - start);

                    if (inQuote)
                        value = value.Replace("\\\"", "\"");

                    list.Add(new KeyValuePair<string, string>(key, value));

                    inValue = false;
                    inKey = true;
                    start = i + 1;
                }
            }

            var dict = list.ToLookup(x => x.Key, x => x.Value);

            if (dict.Contains("CN"))
                return dict["CN"].First();

            if (dict.Contains("OU"))
                return dict["OU"].First();

            if (dict.Contains("O"))
                return dict["O"].First();

            return source;
        }
    }
}
