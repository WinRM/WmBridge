//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Caching;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json.Linq;
using log4net;

namespace WmBridge.Web.Model
{
    public class PSSessionManager : IDisposable
    {
        // singleton backing field
        private static Lazy<PSSessionManager> _instance = new Lazy<PSSessionManager>(() => new PSSessionManager(TimeSpan.FromMinutes(20)));

        private static readonly ILog logger = LogManager.GetLogger("WmBridge.PSSessionManager");

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static PSSessionManager Default { get { return _instance.Value; } }

        public const string XPSSessionHeader = "X-PS-Session";
        public const string PSConnectionKey = "PSConnection";
        public const string PSConnectionStateKey = "PSConnectionState";
        public const string PSHostClientKey = "PSHostClient";
        public const string RefCounterKey = "RefCounter";

        private bool disposed = false;

        readonly MemoryCache sessions = new MemoryCache("PSSessions");
        // state data for given runspace connection
        readonly ConcurrentDictionary<string, object> sessionState = new ConcurrentDictionary<string, object>();

        readonly CacheItemPolicy sessionPolicy;
        readonly CacheItemPolicy shortTimeSessionPolicy;
        readonly CacheItemPolicy runspacePolicy;

        private class PsFactory : WmBridge.Web.Terminal.IPowerShellFactory
        {
            public Runspace Runspace { get; set; }
            
            public PowerShell Create()
            {
                var ps = PowerShell.Create();
                ps.Runspace = this.Runspace;
                return ps;
            }
        }

        public PSSessionManager(TimeSpan idleTimeout)
        {
            runspacePolicy = new CacheItemPolicy() { RemovedCallback = ConnectionRemoved, SlidingExpiration = idleTimeout };
            sessionPolicy = new CacheItemPolicy() { RemovedCallback = SessionRemoved, SlidingExpiration = idleTimeout };
            shortTimeSessionPolicy = new CacheItemPolicy() { RemovedCallback = SessionRemoved, SlidingExpiration = TimeSpan.FromMinutes(1) };

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private object InitConnection(PSConnection options, Dictionary<string, object> sessionStateVars)
        {
            Uri connectTo = null;
            string hostName = options.ComputerName;

            if (hostName != null && hostName.IndexOf(':') == -1) hostName += ":5985";

            connectTo = new Uri(String.Format("http://{0}/wsman", hostName));

            var user = options.UserName;
            if (!user.Contains("\\")) user = ".\\" + user;

            var connection = new WSManConnectionInfo(connectTo, null, new PSCredential(user, options.Password));
            connection.AuthenticationMechanism = options.Authentication;

            Runspace runspace;

            if (options.InteractiveTerminal)
            {
                var factory = new PsFactory();
                var interactiveHost = new WmBridge.Web.Terminal.Host(factory);

                if (options.Options != null)
                {
                    dynamic size;
                    if (options.Options.TryGetValue("BufferSize", out size))
                        interactiveHost.Terminal.BufferSize = new System.Management.Automation.Host.Size(Convert.ToInt32(size.Width), Convert.ToInt32(size.Height));

                    if (options.Options.TryGetValue("WindowSize", out size))
                        interactiveHost.Terminal.WindowSize = new System.Management.Automation.Host.Size(Convert.ToInt32(size.Width), Convert.ToInt32(size.Height));
                }

                runspace = RunspaceFactory.CreateRunspace(interactiveHost, connection);
                factory.Runspace = runspace;
                sessionStateVars.Add(PSSessionManager.PSHostClientKey, interactiveHost);
            }
            else
            {
                runspace = RunspaceFactory.CreateRunspace(connection);
            }

            logger.InfoFormat("Opening runspace: {0}", runspace.ConnectionInfo.ComputerName);

            try
            {
                runspace.Open();
                logger.InfoFormat("Runspace opened: {0}; InstanceId: {1}", runspace.ConnectionInfo.ComputerName, runspace.InstanceId);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Failed to open runspace: {0} - {1}", runspace.ConnectionInfo.ComputerName, ex.Message));
                throw;
            }

            using (var ps = PowerShell.Create())
            {
                ps.Runspace = runspace;

                string script = options.InteractiveTerminal ? "" : "$ErrorActionPreference = 'stop';";

                if (!string.IsNullOrEmpty(options.ExecutionPolicy))
                    script += "Set-ExecutionPolicy $args[0] -Scope Process -Force";

                ps.AddScript(script);
                ps.AddArgument(options.ExecutionPolicy);
                ps.Invoke();
            }

            return runspace;
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            this.Dispose(); // otherwise process may crash (weird powershell bug)
        }

        void ConnectionRemoved(CacheEntryRemovedArguments arguments)
        {
            try
            {
                var lazy = arguments.CacheItem.Value as Lazy<object>;
                if (lazy != null)
                {
                    if (lazy.Value is Runspace)
                        logger.InfoFormat("Closing runspace: {0}", (lazy.Value as Runspace).ConnectionInfo.ComputerName);

                    var disposable = lazy.Value as IDisposable;
                    if (disposable != null) disposable.Dispose();
                }
            }
            catch (Exception ex)  
            {
                logger.Error(ex);
            }
        }

        void SessionRemoved(CacheEntryRemovedArguments arguments)
        {
            string hash = (string)arguments.CacheItem.Value;
            int value = (int)((ConcurrentDictionary<string, object>)sessionState[hash]) 
                .AddOrUpdate(RefCounterKey, 0, (key, current) => (int)current - 1);

            if (value == 0)
            {
                object tmp;
                sessionState.TryRemove(hash, out tmp);

                var dict = tmp as ConcurrentDictionary<string, object>;
                if (dict != null)
                {
                    foreach (var val in dict.Values) // dispose all session related disposable objects
                    {
                        if (val is IDisposable)
                            ((IDisposable)val).Dispose();
                    }
                }

                sessions.Remove(hash);
            }
        }

        string GetConnectionHash(PSConnection connection)
        {
            using (MD5 md5 = MD5.Create())
                return System.Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(connection))));
        }

        /// <summary>
        /// Creates new remote PS session if neccesary, and returns unique session identifier
        /// </summary>
        public string Connect(PSConnection options)
        {
            var connectionHash = GetConnectionHash(options);

            var sessionStateVars = new Dictionary<string, object>(); // state vars as result from InitConnection

            var newLazy = new Lazy<object>(() => InitConnection(options, sessionStateVars));
            var lazyFromCache = (Lazy<object>)sessions.AddOrGetExisting(connectionHash, newLazy, runspacePolicy);

            try
            {
                // force lazy initialization
                var connection = (lazyFromCache ?? newLazy).Value;
            }
            catch (Exception)
            {
                sessions.Remove(connectionHash);
                throw;
            }

            var state = ((ConcurrentDictionary<string, object>)sessionState
                .GetOrAdd(connectionHash, _ => new ConcurrentDictionary<string, object>()));
                
            state.AddOrUpdate(RefCounterKey, 1, (key, current) => (int)current + 1);

            foreach (var sesVars in sessionStateVars)
                state.TryAdd(sesVars.Key, sesVars.Value);

            string newSession = Guid.NewGuid().ToString();
            sessions.Add(newSession, connectionHash, options.ShortTimeConnection ? shortTimeSessionPolicy : sessionPolicy); // session refers to shared connection

            return newSession;
        }

        public void Disconnect(string session)
        {
            sessions.Remove(session);
        }

        /// <summary>
        /// Returns runspace object if exists, otherwise null
        /// </summary>
        public object GetConnection(string session, out string connectionHash, out object state)
        {
            connectionHash = sessions.Get(session) as string;

            if (connectionHash != null)
            {
                sessionState.TryGetValue(connectionHash, out state);

                var lazy = sessions.Get(connectionHash) as Lazy<object>;
                if (lazy != null) return lazy.Value;
            }

            state = null;
            return null;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                sessions.Dispose();
                AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
            }
        }

        ~PSSessionManager()
        {
            Dispose(false);
        }
    }
}
