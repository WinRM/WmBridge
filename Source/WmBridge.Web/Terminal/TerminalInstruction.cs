//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System.Threading;

namespace WmBridge.Web.Terminal
{
    public enum TerminalInstructionCode
    {
        FillBufferContents = 1,
        SetBufferContents = 2,
        ChangeWindowTitle = 3,
        AppendText = 4,
        AppendTokenizedText = 5,
        ChangeProgress = 6,
        ReadLine = 7,
        ReadLineAsSecureString = 8,
        SetShouldExit = 9,
        PipelineCreated = 10,
        PipelineFinished = 11,
        PSPromptChanged = 12,
        AskPrompt = 13,
        PromptForChoice = 14,
        PromptForMultipleChoice = 15
    }

    public class TerminalInstruction
    {
        private static long _currentId = 0;

        public TerminalInstruction()
        {
            Seq = ++_currentId;
        }

        public static long LastSequenceNumber { get { return _currentId; } }

        public long Seq { get; set; }
        public TerminalInstructionCode Code { get; set; }

        object _data;

        public object Data 
        {
            get { return _data; }
            set { _data = value != null && (value is string || value.GetType().IsValueType) ? new { val = value } : value; }
        }

        public override string ToString()
        {
            return Data == null ? Code.ToString() : string.Format("{0}: {1}", Code, Data);
        }
    }

    internal class SynchronousTerminalInstruction : TerminalInstruction
    {
        internal EventWaitHandle WaitHandle { get; set; }
        internal object Result { get; private set; }
        internal bool Canceled { get; private set; }

        public SynchronousTerminalInstruction()
        {
            WaitHandle = new ManualResetEvent(false);
        }

        public void Cancel()
        {
            Canceled = true;
            WaitHandle.Set();
        }

        public void SetResult(object data)
        {
            Result = data;
            WaitHandle.Set();
        }
    }

}
