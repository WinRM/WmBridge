//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using log4net;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using WmBridge.Web.Model;

namespace WmBridge.Web.Terminal
{
    public class RemoteCompletionResult
    {
        public int Index { get; set; }
        public int Length { get; set; }
        public RemoteCompletionItem[] Matches { get; set; }

        public override string ToString()
        {
            return string.Join<RemoteCompletionItem>(", ", Matches);
        }
    }

    public class RemoteCompletionItem
    {
        public string Text { get; set; }
        public string Caption { get; set; }
        public CompletionResultType Type { get; set; }

        public RemoteCompletionItem(string text)
        {
            this.Text = text;
            this.Caption = text;
        }

        public override string ToString()
        {
            return Caption;
        }
    }

    public class RemoteCompletion
    {
        private IPowerShellFactory _psFactory;
        private int tabExpansionVersion = -1;

        private static readonly ILog logger = LogManager.GetLogger("WmBridge.RemoteCompletion");

        public RemoteCompletion(IPowerShellFactory psFactory)
        {
            _psFactory = psFactory;
        }

        public RemoteCompletionResult Complete(string line, int index)
        {
            return Complete(line, index, (x, _) => x);
        }

        public PSHtmlHelp QuickHelpHtml(string line, int index, string forCaption)
        {
            PSHtmlHelp result = null;
            Complete(line, index, (rci, pso) =>
            {
                if (rci.Caption.Equals(forCaption))
                {
                    var toolTip = pso.Properties.SingleOrDefault(p => p.Name == "ToolTip");
                    string toolTipValue = null;

                    if (toolTip != null && toolTip.Value != null)
                        toolTipValue = toolTip.Value.ToString();

                    result = GenerateQuickHelpHtml(rci, toolTipValue);
                }

                return null;
            });

            return result;
        }

        private PSHtmlHelp GenerateQuickHelpHtml(RemoteCompletionItem item, string toolTip)
        {
            if ((item.Type == CompletionResultType.Command || item.Type == CompletionResultType.ProviderItem || tabExpansionVersion == 1))
            {
                try
                {
                    using (var ps = _psFactory.Create())
                    {
                        ps.AddScript("&{$pp = $ProgressPreference; $ProgressPreference = 'SilentlyContinue'; $tmp = @(Get-Help $args[0] -Full -ErrorAction SilentlyContinue); $ProgressPreference = $pp; if ($tmp.count -eq 1) { $tmp[0] }} $args[0]");
                        ps.AddArgument(item.Text);

                        return PSHelpFormatter.BuildHtmlHelp(ps.Invoke().SingleOrDefault());
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }

            if (string.IsNullOrEmpty(toolTip))
                toolTip = "Can not find any help in current context.";

            return PSHelpFormatter.BuildHtmlHelp(toolTip);
        }

        RemoteCompletionResult Complete(string line, int index, Func<RemoteCompletionItem, PSObject, RemoteCompletionItem> interceptor)
        {
            int maxItems = 50;

            if (tabExpansionVersion == -1)
                tabExpansionVersion = GetTabExpansionVersion();

            if (index > 0 && line.Length > 0 && line[index - 1] == ' ')
                return null;

            using (var ps = _psFactory.Create())
            {
                #region TabExpansion
                if (tabExpansionVersion == 1)
                {
                    Collection<PSParseError> errors;
                    var token = PSParser.Tokenize(line, out errors)
                        .FirstOrDefault(t => t.Start < index && index < t.EndColumn);
                    
                    if (token != null)
                    {
                        ps.AddScript("TabExpansion $args[0] $args[1]");
                        ps.AddArgument(line.Substring(0, token.EndColumn - 1));
                        ps.AddArgument(token.Content);

                        return new RemoteCompletionResult()
                        {
                            Index = token.Start,
                            Length = token.Length,
                            Matches = ps.Invoke().Select(o => interceptor(new RemoteCompletionItem(o.BaseObject.ToString()), o))
                            .ToArray().WhereNotNull().LessOrEmpty(maxItems).OrderBy(x => x.Caption).ToArray()
                        };
                    }
                }
                #endregion

                #region TabExpansion2
                if (tabExpansionVersion == 2)
                {
                    ps.AddScript("TabExpansion2 $args[0] $args[1]");
                    ps.AddArgument(line);
                    ps.AddArgument(index);

                    dynamic cmdCompletion = ps.Invoke().SingleOrDefault();

                    if (cmdCompletion == null)
                        return null;
                    
                    var matches = (((PSObject)cmdCompletion.CompletionMatches).BaseObject as ArrayList).Cast<dynamic>();

                    return new RemoteCompletionResult()
                    {
                        Index = cmdCompletion.ReplacementIndex,
                        Length = cmdCompletion.ReplacementLength,
                        Matches = matches.Select(o => interceptor(new RemoteCompletionItem(o.CompletionText) 
                        {
                            Caption = o.ListItemText,
                            Type = (CompletionResultType)Enum.Parse(typeof(CompletionResultType), o.ResultType)
                        }, o) as RemoteCompletionItem).ToArray().WhereNotNull().LessOrEmpty(maxItems).OrderBy(x => x.Caption).ToArray()
                    };
                }
                #endregion
            }

            return null; // not found
        }

        private int GetTabExpansionVersion()
        {
            try {
                using (var ps = _psFactory.Create())
                    return Convert.ToInt32(ps.AddScript("if ($Function:TabExpansion2) {2} else { if($Function:TabExpansion) {1} else {0} }").Invoke().First().BaseObject);
            }
            catch {
                return 0;
            }
        }
    }
}
