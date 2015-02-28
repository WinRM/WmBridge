//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Management.Automation.Host;

namespace WmBridge.Web.Terminal
{
    public class RawUI : PSHostRawUserInterface
    {
        RemoteTerminal _terminal;

        public RawUI(RemoteTerminal terminal)
        {
            _terminal = terminal;
        }

        public override ConsoleColor ForegroundColor
        {
            get { return _terminal.ForegroundColor; }
            set { _terminal.ForegroundColor = value; }
        }

        public override ConsoleColor BackgroundColor
        {
            get { return _terminal.BackgroundColor; }
            set { _terminal.BackgroundColor = value; }
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            _terminal.QueueInstruction(TerminalInstructionCode.FillBufferContents, new { rect = rectangle, fill });
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            _terminal.QueueInstruction(TerminalInstructionCode.SetBufferContents, new { origin, contents });
        }

        public override string WindowTitle
        {
            get
            {
                return _terminal.WindowTitle;
            }
            set
            {
                _terminal.WindowTitle = value;
                _terminal.QueueInstruction(TerminalInstructionCode.ChangeWindowTitle, value);
            }
        }

        public override Size BufferSize { get { return _terminal.BufferSize; } set { } }
        public override Size WindowSize { get { return _terminal.WindowSize; } set { } }
        public override Size MaxWindowSize { get { return _terminal.WindowSize; } }
        public override Size MaxPhysicalWindowSize { get { return _terminal.WindowSize; } }
        public override Coordinates WindowPosition { get { return default(Coordinates); } set { } }

        public override Coordinates CursorPosition { get { return default(Coordinates); } set { } }
        public override int CursorSize { get { return 25; } set { } }


        public override bool KeyAvailable { get { throw new NotImplementedException(); } }
        public override KeyInfo ReadKey(ReadKeyOptions options) { throw new NotImplementedException(); }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle) { throw new NotImplementedException(); }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill) { }

        public override void FlushInputBuffer() { }
    }
}
