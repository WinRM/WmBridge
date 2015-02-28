//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Management.Automation.Runspaces;
using System.Security;
using Newtonsoft.Json;

namespace WmBridge.Web.Model
{
    public class PSConnection
    {
        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ClientVersion { get; set; }

        [Required]
        public string SystemVersion { get; set; }

        [Required]
        public string ComputerName { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [JsonConverter(typeof(SecureStringJsonConverter))]
        public SecureString Password { get; set; }

        public string ExecutionPolicy { get; set; }

        public AuthenticationMechanism Authentication { get; set; }

        public bool InteractiveTerminal { get; set; }

        public bool ShortTimeConnection { get; set; }

        public Dictionary<string, object> Options { get; set; }

        public override string ToString()
        {
            string isShort = ShortTimeConnection ? "[SHORT-TIME] " : "";
            return isShort + string.Format("Client {0} (v{1} / {2}) -> {3}@{4} [{5}]", ClientId, ClientVersion, SystemVersion, UserName, ComputerName, ExecutionPolicy);
        }

    }
}
