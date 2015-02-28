//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;

namespace WmBridge.Web.Terminal
{
    public class Host : PSHost, IPowerShellFactory, IDisposable
    {
        internal RemoteTerminal Terminal { get; private set; }

        HostUI _hostUI;
        Guid _instanceId;
        IPowerShellFactory _psFactory;

        public Host(IPowerShellFactory psFactory)
        {
            _instanceId = Guid.NewGuid();

            _psFactory = psFactory;

            Terminal = new RemoteTerminal(this);

            _hostUI = new HostUI(Terminal);
        }

        public override CultureInfo CurrentCulture
        {
            get { return Thread.CurrentThread.CurrentCulture; }
        }

        public override CultureInfo CurrentUICulture
        {
            get { return Thread.CurrentThread.CurrentUICulture; }
        }

        internal PowerShell CreatePowerShell()
        {
            return _psFactory.Create();
        }

        // nested prompts are not supported on remote sessions
        public override void EnterNestedPrompt() { throw new NotImplementedException(); }
        public override void ExitNestedPrompt() { throw new NotImplementedException(); }

        public override Guid InstanceId { get { return _instanceId; } }

        public override string Name
        {
            get { return "WinRM Bridge Host"; }
        }

        public override void NotifyBeginApplication() { }

        public override void NotifyEndApplication() { }

        public override void SetShouldExit(int exitCode)
        {
            Terminal.QueueInstruction(TerminalInstructionCode.SetShouldExit, exitCode);
        }

        public override PSHostUserInterface UI
        {
            get { return _hostUI; }
        }

        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        PowerShell IPowerShellFactory.Create()
        {
            return _psFactory.Create();
        }

        public void Dispose()
        {
            Terminal.Dispose();
        }
    }
}
