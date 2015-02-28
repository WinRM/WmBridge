//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Linq;
using System.Web.Http;
using System.ComponentModel.DataAnnotations;

using WmBridge.Web.Filters;
using WmBridge.Web.Model;
using WmBridge.Web.Terminal;
using System.Net.Http;

namespace WmBridge.Web.Controllers
{
    [RoutePrefix("terminal"), ConnectedTerminal]
    public class TerminalController : PSApiController
    {
        public class SizeArgs
        {
            [Required]
            public int Width { get; set; }

            [Required]
            public int Height { get; set; }
        }

        public class InstructionsArgs
        {
            [Required]
            public long Sequence { get; set; }

            [Required]
            public int Count { get; set; }

            [Required]
            public int Timeout { get; set; }
        }

        public class CompletionArgs
        {
            [Required]
            public string Script { get; set; }

            [Required]
            public int Index { get; set; } // cursor position in script
        }

        public class HelpArgs
        {
            [Required]
            public string Script { get; set; }

            [Required]
            public int Index { get; set; } // cursor position in script

            [Required]
            public string SelectedCaption { get; set; }
        }

        Host PSHost { get { return Request.Properties[PSSessionManager.PSHostClientKey] as Host; } }
        RemoteTerminal Terminal { get { return PSHost.Terminal; } }

        [Route("open"), HttpGet]
        public IHttpActionResult Open()
        {
            if (Terminal.IsPipelineExecuted)
            {
                Terminal.AbortPipeline(); // if client was last time terminated on synchronous instruction and now he tries to reuse old runspace (rarely happens)
            }

            Log.InfoFormat("Opening interactive terminal.");
            return Json(new { LastSequenceNumber = TerminalInstruction.LastSequenceNumber });
        }

        [Route("buffer-size"), HttpPost, ValidateModel]
        public IHttpActionResult SetBufferSize([FromBody] SizeArgs args)
        {
            Terminal.BufferSize = new System.Management.Automation.Host.Size(args.Width, args.Height);
            return Ok();
        }

        [Route("window-size"), HttpPost, ValidateModel]
        public IHttpActionResult SetWindowSize([FromBody] SizeArgs args)
        {
            Terminal.WindowSize = new System.Management.Automation.Host.Size(args.Width, args.Height);
            return Ok();
        }

        [Route("instructions"), HttpPost, ValidateModel]
        public IHttpActionResult GetInstructions([FromBody] InstructionsArgs args)
        {
            return Json(Terminal.GetInstructions(args.Sequence, args.Count, args.Timeout));
        }

        [Route("result"), HttpPost]
        public IHttpActionResult SetSynchronousInstructionResult([FromBody] object data)
        {
            Terminal.SetSynchronousInstructionResult(data);
            return Ok();
        }

        [Route("abort"), HttpGet]
        public IHttpActionResult AbortPipeline()
        {
            Terminal.AbortPipeline();
            return Ok();
        }

        [Route("prompt"), HttpGet]
        public IHttpActionResult DumpPrompt()
        {
            Terminal.CheckPromptChange(true);
            return Ok();
        }

        [Route("welcome"), HttpGet]
        public IHttpActionResult WelcomeMessage()
        {
            var info = InvokePowerShell("New-Object PSObject -Property @{PSVersion = $PSVersionTable.PSVersion.ToString()}", PSSelect("PSVersion")).Single();

            PSHost.UI.WriteLine(string.Format("{0} {1}\n{2}\n", "Windows PowerShell", info["PSVersion"], "Copyright (C) Microsoft Corporation."));

            return Ok();
        }

        [Route("execute"), HttpPost]
        public IHttpActionResult Execute([FromBody] string cmd)
        {
            Terminal.ExecuteCommand(cmd);
            return Ok();
        }

        [Route("complete"), HttpPost]
        public IHttpActionResult Complete([FromBody] CompletionArgs args)
        {
            return Json(Terminal.Completion.Complete(args.Script, args.Index));
        }

        [Route("help"), HttpPost]
        public IHttpActionResult GetHelp([FromBody] HelpArgs args)
        {
            return Json(Terminal.Completion.QuickHelpHtml(args.Script, args.Index, args.SelectedCaption));
        }

        [Route("close"), HttpGet]
        public IHttpActionResult Close()
        {
            Log.InfoFormat("Closing interactive terminal.");

            PSHost.Dispose();
            Request.GetPSState().Remove(PSSessionManager.PSHostClientKey);
            Request.CloseSharedConnection();
            return Ok();
        }

    }
}
