//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace WmBridge.Web.Terminal
{
    public class RemoteTerminal : IDisposable
    {
        Host _host;

        List<TerminalInstruction> _instructions = new List<TerminalInstruction>(); // instruction queue for client
        EventWaitHandle _waitHandle = new AutoResetEvent(false); // signaled when a new instruction is added

        PowerShell _pipeline; // current pipeline - must by null if not executing
        IAsyncResult _pipelineResult; // asynchronous result from pipeline BeginInvoke

        string _currentPrompt; // last string from prompt function
        SynchronousTerminalInstruction _currentSynchronousInstruction; // instruction in awaiting state (called from ExecuteInstruction)

        bool _disposed = false;

        private static readonly ILog logger = LogManager.GetLogger("WmBridge.RemoteTerminal");

        public Size BufferSize { get; set; }
        public Size WindowSize { get; set; }

        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }
        public string WindowTitle { get; set; } // current title set by host

        public RemoteCompletion Completion { get; private set; }

        public bool IsPipelineExecuted { get { return _pipeline != null; } }

        public RemoteTerminal(Host host)
        {
            _host = host;

            BackgroundColor = ConsoleColor.DarkMagenta;
            ForegroundColor = ConsoleColor.DarkYellow;

            Completion = new RemoteCompletion(host);
        }

        /// <summary>
        /// Send instruction to client
        /// </summary>
        public void QueueInstruction(TerminalInstructionCode instruction, object data = null)
        {
            QueueInstruction(new TerminalInstruction() { Code = instruction, Data = data });
        }

        void QueueInstruction(TerminalInstruction instruction)
        {
            lock (_instructions)
                _instructions.Add(instruction);

            _waitHandle.Set();
        }

        /// <summary>
        /// Get all newer instructions from specific ID and block the call if there is no instruction queued.
        /// </summary>
        public List<TerminalInstruction> GetInstructions(long fromSeq, int count, int millisecondsTimeout)
        {
            CheckPipelineState();

            var result = FindNewerAndDeleteOlder(fromSeq, count);
            if (result.Any() || millisecondsTimeout == 0)
                return result;

            WaitHandle[] handles;
            if (IsPipelineExecuted)
                handles = new[] { _waitHandle, _pipelineResult.AsyncWaitHandle };
            else
                handles = new[] { _waitHandle };

            WaitHandle.WaitAny(handles, millisecondsTimeout);

            CheckPipelineState();

            return FindNewerAndDeleteOlder(fromSeq, count);
        }

        /// <summary>
        /// Called by client to set a response for synchronous instruction
        /// </summary>
        public void SetSynchronousInstructionResult(object data)
        {
            _currentSynchronousInstruction.SetResult(data);
        }

        /// <summary>
        /// Execute instruction on client side and wait for input
        /// </summary>
        public bool ExecuteInstruction(TerminalInstructionCode instruction, object data, out object result)
        {
            try
            {
                _currentSynchronousInstruction = new SynchronousTerminalInstruction() { Code = instruction, Data = data };

                // send to client
                QueueInstruction(_currentSynchronousInstruction);

                // wait for client response or cancelation
                _currentSynchronousInstruction.WaitHandle.WaitOne();

                result = _currentSynchronousInstruction.Result;
                return !_currentSynchronousInstruction.Canceled;
            }
            finally
            {
                // cleanup
                _currentSynchronousInstruction.WaitHandle.Dispose();
                _currentSynchronousInstruction = null;
            }
        }

        /// <summary>
        /// Executes powershell command
        /// </summary>
        public void ExecuteCommand(string cmd)
        {
            if (IsPipelineExecuted) // don't execute another command if the last pipeline not ended
                throw new InvalidOperationException("Previous pipeline not properly ended.");
            
            try
            {
                QueueInstruction(TerminalInstructionCode.PipelineCreated);

                _pipeline = _host.CreatePowerShell();
                _pipeline.AddScript(cmd);
                _pipeline.AddCommand("Out-Default");
                _pipeline.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                lock (_instructions) // all write-hosts followed immediately after invoking the script are blocked because we need write tokenized script first
                {
                    _pipelineResult = _pipeline.BeginInvoke();
                    WriteTokenized(cmd); // write script only if BeginInvoke not crashed
                }
            }
            catch
            {
                FinishPipeline(); // dispose and send notification about finish
                throw; // probably runspace is broken, so throw it out
            }
        }

        void WriteTokenized(string text)
        {
            Collection<PSParseError> errors;
            var tokens = PSParser.Tokenize(text, out errors);

            int index = 0;
            foreach (var t in tokens)
            {
                if (index < t.Start) // whitespace characters in between tokens
                {
                    QueueInstruction(TerminalInstructionCode.AppendTokenizedText, new { txt = text.Substring(index, t.Start - index) });
                    index = t.Start;
                }

                QueueInstruction(TerminalInstructionCode.AppendTokenizedText, new { txt = text.Substring(index, t.Length), type = t.Type });

                index += t.Length;
            }

            if (index < text.Length) // whitespace characters at the end
                QueueInstruction(TerminalInstructionCode.AppendTokenizedText, new { txt = text.Substring(index, text.Length - index) });
        }

        public void AbortPipeline()
        {
            if (!IsPipelineExecuted)
                throw new InvalidOperationException("There is no executing pipeline.");

            var op = _currentSynchronousInstruction;
            if (op != null) op.Cancel();

            var p = _pipeline;
            if (p != null) p.Stop();
        }

        List<TerminalInstruction> FindNewerAndDeleteOlder(long fromSeq, int count)
        {
            List<TerminalInstruction> list = new List<TerminalInstruction>();

            lock (_instructions)
                for (int i = 0; i < _instructions.Count && list.Count < count; i++)
                {
                    var op = _instructions[i];

                    if (op.Seq > fromSeq)
                        list.Add(op);
                    else
                        _instructions.RemoveAt(i--); // remove older (confirmed) instructions - will not be needed anymore hence client ask for newer
                }

            return list;
        }

        /// <summary>
        /// Send notification if pipeline is ended
        /// </summary>
        private void CheckPipelineState()
        {
            var r = _pipelineResult;
            if (r != null && r.IsCompleted)
            {
                var p = _pipeline;
                if (p != null)
                {
                    try
                    {
                        p.EndInvoke(r);
                    }
                    catch (Exception ex)
                    {
                        ReportException(ex);
                    }
                    finally
                    {
                        FinishPipeline(); // dispose and send notification about finish
                    }
                }
            }
        }

        private void ReportException(Exception e)
        {
            object error;

            if (e is IContainsErrorRecord)
                error = (e as IContainsErrorRecord).ErrorRecord;
            else
                error = new ErrorRecord(e, "Host.ReportException", ErrorCategory.NotSpecified, null);

            try
            {
                using (var ps = _host.CreatePowerShell())
                {
                    ps.AddScript("$input").AddCommand("Out-String");

                    // Do not merge errors, this function will swallow errors.
                    Collection<PSObject> result;
                    PSDataCollection<object> inputCollection = new PSDataCollection<object>();
                    inputCollection.Add(error);
                    inputCollection.Complete();
                    result = ps.Invoke(inputCollection);

                    if (result.Any())
                    {
                        var str = result.First().BaseObject as string;
                        if (!string.IsNullOrEmpty(str))
                        {
                            _host.UI.WriteErrorLine(str.Substring(0, str.Length - 2));
                        }
                    }
                }
            }
            catch (Exception ex) // exception reporting might fail, depends on runspace state - just ignore any exception
            {
                logger.Error(ex);
            } 
        }

        public void CheckPromptChange(bool force = false)
        {
            try
            {
                using (var ps = _host.CreatePowerShell())
                {
                    var prompt = string.Join("\n", ps.AddScript("prompt | Out-String -Width ([Int32]::MaxValue)").Invoke());
                    if (_currentPrompt != prompt || force)
                    {
                        _currentPrompt = prompt;
                        QueueInstruction(TerminalInstructionCode.PSPromptChanged, prompt);
                    }
                }
            }
            catch (Exception ex) // might fail depends on runspace state - act like no prompt change was made
            {
                logger.Error(ex);
            } 
        }

        private void FinishPipeline()
        {
            _pipelineResult = null;

            if (_pipeline != null)
            { // first set null, then dispose
                var tmp = _pipeline;
                _pipeline = null;
                tmp.Dispose();
            }

            CheckPromptChange();

            QueueInstruction(TerminalInstructionCode.PipelineFinished);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if (_currentSynchronousInstruction != null)
                    _currentSynchronousInstruction.Cancel();
            }
        }

        ~RemoteTerminal()
        {
            Dispose(false);
        }
    }
    
}
