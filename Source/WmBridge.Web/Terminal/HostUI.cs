//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;
using Newtonsoft.Json.Linq;

namespace WmBridge.Web.Terminal
{
    public class HostUI : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
    {
        RemoteTerminal _terminal;
        RawUI _rawUI;

        public HostUI(RemoteTerminal terminal)
        {
            _terminal = terminal;
            _rawUI = new RawUI(terminal);
        }

        public override PSHostRawUserInterface RawUI { get { return _rawUI ; } }

        /// <summary>
        /// Prompts the user for input. 
        /// <param name="caption">The caption or title of the prompt.</param>
        /// <param name="message">The text of the prompt.</param>
        /// <param name="descriptions">A collection of FieldDescription objects  
        /// that describe each field of the prompt.</param>
        /// <returns>A dictionary object that contains the results of the user 
        /// prompts.</returns>
        public override Dictionary<string, PSObject> Prompt(
                                  string caption,
                                  string message,
                                  Collection<FieldDescription> descriptions)
        {
            Dictionary<string, PSObject> results = new Dictionary<string, PSObject>();

            var promptRequest = new
            {
                caption = caption, message = message,
                descriptions = descriptions.Select(fd =>
                    new { name = fd.Name, type = fd.ParameterTypeName, value = GetDefaultValue(fd) })
            };

            object result;
            if (_terminal.ExecuteInstruction(TerminalInstructionCode.AskPrompt, promptRequest, out result))
            {
                var jsonArray = result as JArray;
                if (jsonArray != null && jsonArray.Count == descriptions.Count)
                {
                    for (int i = 0; i < descriptions.Count; i++)
                    {
                        var typeName = descriptions[i].ParameterTypeName;
                        var jsonVal = jsonArray[i] as JObject;

                        if (jsonVal != null)
                        {
                            if (typeName == "PSCredential")
                            {
                                string userName = jsonVal.Value<string>("user");
                                string pass = jsonVal.Value<string>("password"); ;
                                
                                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(pass))
                                    results.Add(descriptions[i].Name, PSObject.AsPSObject(new PSCredential(userName, GetSecureString(pass))));
                            }
                            else if (typeName == "SecureString")
                            {
                                string pass = jsonVal.Value<string>("password");

                                if (!string.IsNullOrEmpty(pass))
                                    results.Add(descriptions[i].Name, PSObject.AsPSObject(GetSecureString(pass)));
                            }
                            else
                            {
                                var jobj = jsonVal.Value<JValue>("value");
                                if (jobj != null)
                                    results.Add(descriptions[i].Name, PSObject.AsPSObject(jobj.Value));
                            }
                        }
                        
                    }
                }
            }
            
            return results;
        }

        private static object GetDefaultValue(FieldDescription fd)
        {
            if (fd.DefaultValue != null)
            {
                var baseObject = fd.DefaultValue.BaseObject;
                var typeName = fd.ParameterTypeName;

                if (typeName == "PSCredential" && baseObject is PSCredential)
                {
                    return new { user = (baseObject as PSCredential).UserName };
                }
                else if (baseObject is string)
                {
                    return new { value = baseObject as string };
                }
            }

            return null;
        }

        private Collection<int> PromptForChoice(
            string caption,
            string message,
            Collection<ChoiceDescription> choices,
            IEnumerable<int> defaultChoices,
            TerminalInstructionCode instructionCode)
        {
            Collection<int> selected = null;

            var promptRequest = new
            {
                caption = caption,
                message = message,
                descriptions = choices.Select((cd, i) =>
                    new { name = cd.Label.Replace("&", ""), selected = (defaultChoices.Contains(i)) })
            };

            object result;
            if (_terminal.ExecuteInstruction(instructionCode, promptRequest, out result))
            {
                var jsonArray = result as JArray;
                if (jsonArray != null && jsonArray.Count == choices.Count)
                {
                    for (int i = 0; i < choices.Count; i++)
                    {
                        var jsonVal = jsonArray[i] as JValue;
                        if (jsonVal != null)
                        {
                            if (selected == null)
                                selected = new Collection<int>();

                            if (jsonVal.Value<bool>() == true)
                            {
                                selected.Add(i);
                            }
                        }
                    }

                }
            }

            return selected;
        }

        /// <summary>

        /// Provides a set of choices that enable the user to choose a 
        /// single option from a set of options. 
        /// </summary>
        /// <param name="caption">Text that proceeds (a title) the choices.</param>
        /// <param name="message">A message that describes the choice.</param>
        /// <param name="choices">A collection of ChoiceDescription objects that  
        /// describ each choice.</param>
        /// <param name="defaultChoice">The index of the label in the Choices  
        /// parameter collection. To indicate no default choice, set to -1.</param>
        /// <returns>The index of the Choices parameter collection element that 
        /// corresponds to the option that is selected by the user.</returns>
        public override int PromptForChoice(
                                            string caption,
                                            string message,
                                            Collection<ChoiceDescription> choices,
                                            int defaultChoice)
        {
            var selected = PromptForChoice(caption, message, choices, new int[]{defaultChoice}, TerminalInstructionCode.PromptForChoice);
            if (selected != null && selected.Count == 1)
                return selected[0];
            else
                return -1;
        }

        #region IHostUISupportsMultipleChoiceSelection Members

        /// <summary>
        /// Provides a set of choices that enable the user to choose a one or 
        /// more options from a set of options. 
        /// </summary>
        /// <param name="caption">Text that proceeds (a title) the choices.</param>
        /// <param name="message">A message that describes the choice.</param>
        /// <param name="choices">A collection of ChoiceDescription objects that  
        /// describ each choice.</param>
        /// <param name="defaultChoices">The index of the label in the Choices  
        /// parameter collection. To indicate no default choice, set to -1.</param>
        /// <returns>The index of the Choices parameter collection element that 
        /// corresponds to the option that is selected by the user.</returns>
        public Collection<int> PromptForChoice(
                                               string caption,
                                               string message,
                                               Collection<ChoiceDescription> choices,
                                               IEnumerable<int> defaultChoices)
        {
            return PromptForChoice(caption, message, choices, defaultChoices, TerminalInstructionCode.PromptForMultipleChoice);
        }

        #endregion

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            return PromptForCredential(caption, message, userName, targetName, PSCredentialTypes.Generic | PSCredentialTypes.Domain, PSCredentialUIOptions.Default);
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            var fds = new Collection<FieldDescription>();
            var fd = new FieldDescription("Credential");
            fd.SetParameterType(typeof(PSCredential));
            
            if (!string.IsNullOrEmpty(userName))
                fd.DefaultValue = PSObject.AsPSObject(new PSCredential(userName, new SecureString()));

            fds.Add(fd);
            
            var result = this.Prompt(caption, message, fds);

            if (result != null && result.ContainsKey("Credential"))
            {
                return result["Credential"].BaseObject as PSCredential;
            }
            else
            {
                return null;
            }
        }

        public override string ReadLine()
        {
            object result;
            if (_terminal.ExecuteInstruction(TerminalInstructionCode.ReadLine, null, out result))
                return (string)result;
            else
                return null;
        }

        public override SecureString ReadLineAsSecureString()
        {
            object result;
            if (_terminal.ExecuteInstruction(TerminalInstructionCode.ReadLineAsSecureString, null, out result))
                return GetSecureString((string)result);
            else
                return null;
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            _terminal.QueueInstruction(TerminalInstructionCode.ChangeProgress, record);
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            _terminal.QueueInstruction(TerminalInstructionCode.AppendText, new { fg = foregroundColor, bg = backgroundColor, txt = value});
        }

        public override void Write(string value)
        {
            Write(_terminal.ForegroundColor, _terminal.BackgroundColor, value);
        }

        public override void WriteLine(string value)
        {
            WriteLine(_terminal.ForegroundColor, _terminal.BackgroundColor, value);
        }

        public override void WriteErrorLine(string value)
        {
            WriteLine(ConsoleColor.Red, ConsoleColor.Black, value);
        }

        public override void WriteDebugLine(string message)
        {
            WriteLine(ConsoleColor.DarkYellow, ConsoleColor.Black, "DEBUG: " + message);
        }

        public override void WriteVerboseLine(string message)
        {
            WriteLine(ConsoleColor.Green, ConsoleColor.Black, "VERBOSE: " + message);
        }

        public override void WriteWarningLine(string message)
        {
            WriteLine(ConsoleColor.Yellow, ConsoleColor.Black, "WARNING: " + message);
        }

        public override void WriteLine()
        {
            WriteLine(String.Empty);
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            Write(foregroundColor, backgroundColor, value + "\n");
        }

        /// <summary>
        /// Parse a string containing a hotkey character.
        /// Take a string of the form
        ///    Yes to &amp;all
        /// and returns a two-dimensional array split out as
        ///    "A", "Yes to all".
        /// </summary>
        /// <param name="input">The string to process</param>
        /// <returns>
        /// A two dimensional array containing the parsed components.
        /// </returns>
        private static string[] GetHotkeyAndLabel(string input)
        {
            string[] result = new string[] { String.Empty, String.Empty };
            string[] fragments = input.Split('&');
            if (fragments.Length == 2)
            {
                if (fragments[1].Length > 0)
                {
                    result[0] = fragments[1][0].ToString().
                    ToUpper(CultureInfo.CurrentCulture);
                }

                result[1] = (fragments[0] + fragments[1]).Trim();
            }
            else
            {
                result[1] = input;
            }

            return result;
        }

        private static string[] GetHotkeyAndLabel(FieldDescription input)
        {
            if (string.IsNullOrEmpty(input.Label))
                return GetHotkeyAndLabel(input.Name);
            else
                return GetHotkeyAndLabel(input.Label);
        }

        /// <summary>
        /// This is a private worker function splits out the
        /// accelerator keys from the menu and builds a two
        /// dimentional array with the first access containing the
        /// accelerator and the second containing the label string
        /// with the &amp; removed.
        /// </summary>
        /// <param name="choices">The choice collection to process</param>
        /// <returns>
        /// A two dimensional array containing the accelerator characters
        /// and the cleaned-up labels</returns>
        private static string[,] BuildHotkeysAndPlainLabels(
             Collection<ChoiceDescription> choices)
        {
            // Allocate the result array
            string[,] hotkeysAndPlainLabels = new string[2, choices.Count];

            for (int i = 0; i < choices.Count; ++i)
            {
                string[] hotkeyAndLabel = GetHotkeyAndLabel(choices[i].Label);
                hotkeysAndPlainLabels[0, i] = hotkeyAndLabel[0];
                hotkeysAndPlainLabels[1, i] = hotkeyAndLabel[1];
            }

            return hotkeysAndPlainLabels;
        }

        static SecureString GetSecureString(string source)
        {
            if (source == null)
                return null;
            else
            {
                SecureString result = new SecureString();
                foreach (char c in source.ToCharArray())
                    result.AppendChar(c);

                result.MakeReadOnly();

                return result;
            }
        }
    }
}
