//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

using HtmlAgilityPack;

namespace WmBridge.Web.Model
{
    public class PSHtmlHelp
    {
        public string Content { get; set; }
        public bool IsReduced { get; set; }
    }

    public static class PSHelpFormatter
    {
        //  Inspired by Help.Format.ps1xml PowerShell formatting file

        /*
         * Used style classes (as in DOM hierarchy):
         * 
         * #syntax
         *      cmdlet-name
         *      cmdlet-param
         *          cmdlet-param-bracket
         *          cmdlet-param-dash
         *      cmdlet-param-value
         *          cmdlet-param-value-bracket
         *      cmdlet-param-group
         *          cmdlet-param-group-bracket
         *          cmdlet-param-group-pipe
         *
         * #parameters
         *      cmdlet-param-value-opt-bracket
         *      cmdlet-param-option-key
         *      cmdlet-param-option-value
         *      
         * #inputs
         *      io-type-name
         *      
         * #outputs
         *      io-type-name
         *      
         */

        private const string CmdletNameClass                    = "cmdlet-name";
        private const string CmdletParamClass                   = "cmdlet-param";
        private const string CmdletParamBracketClass            = "cmdlet-param-bracket";
        private const string CmdletParamDashClass               = "cmdlet-param-dash";
        private const string CmdletParamValueClass              = "cmdlet-param-value";
        private const string CmdletParamValueBracketClass       = "cmdlet-param-value-bracket";
        private const string CmdletParamValueOptBracketClass    = "cmdlet-param-value-opt-bracket";
        private const string CmdletParamGroupClass              = "cmdlet-param-group";
        private const string CmdletParamGroupBracketClass       = "cmdlet-param-group-bracket";
        private const string CmdletParamGroupPipeClass          = "cmdlet-param-group-pipe";
        private const string CmdletParamOptionKeyClass          = "cmdlet-param-option-key";
        private const string CmdletParamOptionValueClass        = "cmdlet-param-option-value";
        private const string IoTypeNameClass                    = "io-type-name";
        private const string SectionClass                       = "section";
        private const string CodeClass                          = "code";

        private static void MamlShortDescription(HtmlNode node, dynamic dynObj)
        {
            foreach (var line in dynObj)
            {
                node.AppendParagraphElement(p => p.AppendTextElement((string)(PSStringBaseObject(line is string ? line : line.Text)) ?? ""));
            }
        }

        private static void MamlParameter(HtmlNode node, dynamic syntaxItem)
        {
            bool workflowCommonParameters = Convert.ToBoolean(syntaxItem.WorkflowCommonParameters);
            bool commonParameters = true;

            if (syntaxItem.CommonParameters != null)
                commonParameters = Convert.ToBoolean(syntaxItem.CommonParameters);

            node.AppendSpanElement(CmdletNameClass, (string)syntaxItem.Name);

            foreach (var par in PSArrayListBaseObject(syntaxItem.Parameter))
            {
                bool required = Convert.ToBoolean(par.Required);
                string position = par.Position; int numPosition;
                bool optionalTagForName = (int.TryParse(position, out numPosition)); // position must be a number
                string name = par.Name;

                node.AppendTextElement(" ");

                if (!required)
                    node.AppendSpanElement(CmdletParamBracketClass, "[");

                node.AppendSpanElement(span =>
                {
                    span.SetClassName(CmdletParamClass);

                    if (optionalTagForName)
                        span.AppendSpanElement(CmdletParamBracketClass, "[");

                    span.AppendSpanElement(span2 => span2.SetClassName(CmdletParamDashClass).AppendNonBreakingHyphen());
                    span.AppendTextElement(name);

                    if (optionalTagForName)
                        span.AppendSpanElement(CmdletParamBracketClass, "]");
                });

                MamlParameterValue(node, PSStringBaseObject(par.ParameterValue));
                MamlParameterValueGroup(node, par.ParameterValueGroup);

                if (!required)
                    node.AppendSpanElement(CmdletParamBracketClass, "]");
            }

            if (workflowCommonParameters)
                MamlCommonParameters(node, "WorkflowCommonParameters");

            if (commonParameters)
                MamlCommonParameters(node, "CommonParameters");
        }

        private static void MamlCommonParameters(HtmlNode node, string name)
        {
            node.AppendNonBreakingSpace();
            node.AppendSpanElement(CmdletParamBracketClass, "[");
            MamlParameterValue(node, name, false);
            node.AppendSpanElement(CmdletParamBracketClass, "]");
        }

        private static void MamlParameterValue(HtmlNode node, string paramValue, bool indent = true, bool optional = false)
        {
            if (paramValue == null)
                return;

            if (indent)
                node.AppendNonBreakingSpace();

            node.AppendSpanElement(span => 
            {
                span.SetClassName(CmdletParamValueClass);
                
                if (optional)
                    span.AppendSpanElement(CmdletParamValueOptBracketClass, "[");

                span.AppendSpanElement(CmdletParamValueBracketClass, "<");
                span.AppendTextElement(paramValue);
                span.AppendSpanElement(CmdletParamValueBracketClass, ">");

                if (optional)
                    span.AppendSpanElement(CmdletParamValueOptBracketClass, "]");
            });
        }

        private static void MamlParameterValueGroup(HtmlNode node, dynamic dynObj)
        {
            if (dynObj == null)
                return;

            node.AppendNonBreakingSpace();
            node.AppendSpanElement(span =>
            {
                span.SetClassName(CmdletParamGroupClass);
                span.AppendSpanElement(CmdletParamGroupBracketClass, "{");

                int i = 0;
                foreach (var parGroup in PSArrayListBaseObject(dynObj.ParameterValue))
                {
                    string name = PSStringBaseObject(parGroup);

                    if (string.IsNullOrEmpty(name))
                        continue;

                    if (i > 0)
                        span.AppendSpanElement(CmdletParamGroupPipeClass, " | ");

                    span.AppendTextElement(name);

                    i++;
                }

                span.AppendSpanElement(CmdletParamGroupBracketClass, "}");
            });


        }

        private static void MamlParameterOptionsItem(HtmlNode table, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                table.AppendTrElement(tr =>
                {
                    table.AppendTdElement(CmdletParamOptionKeyClass, td => td.AppendTextElement(key));
                    table.AppendTdElement(CmdletParamOptionValueClass, td => td.AppendTextElement(value));
                });
            }
        }
        private static void MamlParameterOptions(HtmlNode node, dynamic parameter)
        {
            node.AppendTableElement(table =>
            {
                MamlParameterOptionsItem(table, "Aliases", ValueOrNone(parameter.Aliases));
                MamlParameterOptionsItem(table, "Required?", parameter.Required);
                MamlParameterOptionsItem(table, "Position?", parameter.Position);
                MamlParameterOptionsItem(table, "Default Value", ValueOrNone(parameter.DefaultValue));
                MamlParameterOptionsItem(table, "Accept Pipeline Input?", parameter.PipelineInput);
                MamlParameterOptionsItem(table, "Accept Wildcard Characters?", parameter.Globbing);
            });
        }

        private static string ValueOrNone(string value)
        {
            return string.IsNullOrEmpty(value) ? "None" : value;
        }

        private static void MamlType(HtmlNode node, dynamic dynObj)
        {
            node.AppendSpanElement(IoTypeNameClass, (string)dynObj.Name);
            MamlShortDescription(node, PSArrayListBaseObject(dynObj.Description));
        }

        private static void MamlTypeList(HtmlNode node, dynamic dynObj)
        {
            if (dynObj.Count == 1 && dynObj[0].Type.Description == null)
            {
                foreach (string type in (PSStringBaseObject(dynObj[0].Type.Name) ?? "").Split('\n'))
                {
                    var trimmed = type.Trim();

                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        node.AppendLiElement(li =>
                        {
                            MamlType(li, new { Name = trimmed, Description = (object)null });
                        });
                    }
                }
            }
            else
            {
                foreach (var type in dynObj)
                {
                    node.AppendLiElement(li =>
                    {
                        MamlType(li, type.Type);
                        MamlShortDescription(li, PSArrayListBaseObject(type.Description));
                    });
                }
            }
        }

        public static PSHtmlHelp BuildHtmlHelp(PSObject psObject)
        {
            if (psObject.BaseObject is string)
                return BuildHtmlHelp(psObject.BaseObject as string);

            dynamic dynObj = psObject;

            HtmlDocument doc = new HtmlDocument();
            bool isReduced = false;

            doc.DocumentNode.AppendHtmlElement(html =>
            {
                html.AppendHeadElement(head => head.AppendTitleElement((string)dynObj.Details.Name));
                html.AppendBodyElement(body =>
                {
                    dynamic detailedDescArr = PSArrayListBaseObject(dynObj.Description);
                    dynamic syntaxArr = null;
                    dynamic paramArr = null;
                    dynamic inputArr = null;
                    dynamic outputArr = null;
                    dynamic linksArr = null;
                    dynamic notesArr = null;
                    dynamic exampleArr = null;
                    dynamic aliasesObj = dynObj.Aliases;

                    if (IsValidObject(dynObj.AlertSet))
                        notesArr = PSArrayListBaseObject(dynObj.AlertSet.Alert);

                    if (IsValidObject(dynObj.Examples))
                        exampleArr = PSArrayListBaseObject(dynObj.Examples.Example); ;

                    if (IsValidObject(dynObj.Syntax))
                        syntaxArr = PSArrayListBaseObject(dynObj.Syntax.SyntaxItem);

                    if (IsValidObject(dynObj.Parameters))
                        paramArr = PSArrayListBaseObject(dynObj.Parameters.Parameter);

                    if (IsValidObject(dynObj.InputTypes))
                        inputArr = PSArrayListBaseObject(dynObj.InputTypes.InputType);

                    if (IsValidObject(dynObj.ReturnValues))
                        outputArr = PSArrayListBaseObject(dynObj.ReturnValues.ReturnValue);

                    if (IsValidObject(dynObj.RelatedLinks))
                        linksArr = PSArrayListBaseObject(dynObj.RelatedLinks.NavigationLink);

                    body.AppendDivElement("name", outerDiv =>
                    {
                        outerDiv.AppendHeadingElement(1, (string)dynObj.Details.Name);
                        isReduced = true; // for now help has header - still can be reduced unless it has a detailed description
                        outerDiv.AppendDivOfClassElement(SectionClass, div =>
                        {
                            var desc = PSArrayListBaseObject(dynObj.Details.Description);
                            if (desc.Count > 0)
                                MamlShortDescription(div, desc);
                        });
                    });

                    if (HasItems(syntaxArr))
                    {
                        body.AppendDivElement("syntax", outerDiv =>
                        {
                            outerDiv.AppendHeadingElement(2, "Syntax");
                            outerDiv.AppendDivOfClassElement(SectionClass, div =>
                            {
                                foreach (var syntaxItem in syntaxArr)
                                {
                                    div.AppendParagraphElement(p => MamlParameter(p, syntaxItem));
                                }
                            });
                        });
                    }

                    if (HasItems(detailedDescArr))
                    {
                        body.AppendDivElement("description", outerDiv =>
                        {
                            isReduced = false; // help has detailed description so it can't be reduced
                            outerDiv.AppendHeadingElement(2, "Detailed Description");
                            outerDiv.AppendDivOfClassElement(SectionClass, div =>
                            {
                                MamlShortDescription(div, detailedDescArr);
                            });
                        });
                    }

                    if (HasItems(paramArr))
                    {
                        body.AppendDivElement("parameters", outerDiv =>
                        {
                            outerDiv.AppendHeadingElement(2, "Parameters");
                            outerDiv.AppendDivOfClassElement(SectionClass, div =>
                            {
                                foreach (var par in paramArr)
                                {
                                    div.AppendHeadingElement(3, h =>
                                    {
                                        h.AppendSpanElement(span2 => span2.SetClassName(CmdletParamDashClass).AppendNonBreakingHyphen());
                                        h.AppendTextElement((string)par.Name);

                                        string parameterValue = PSStringBaseObject(par.ParameterValue);

                                        if (!string.IsNullOrEmpty(parameterValue) && parameterValue != "SwitchParameter")
                                            MamlParameterValue(h, parameterValue);
                                    });

                                    MamlShortDescription(div, PSArrayListBaseObject(par.Description));
                                    MamlParameterOptions(div, par);
                                }
                            });
                        });
                    }

                    if (HasItems(inputArr))
                    if (inputArr.Count != 1 || !string.IsNullOrEmpty(inputArr[0].Type.Name))
                    {
                        body.AppendDivElement("inputs", outerDiv =>
                        {
                            outerDiv.AppendHeadingElement(2, "Inputs");
                            outerDiv.AppendDivOfClassElement(SectionClass, div =>
                            {
                                div.AppendUlElement(ul => MamlTypeList(ul, inputArr));
                            });
                        });
                    }

                    if (HasItems(outputArr))
                    if (outputArr.Count != 1 || !string.IsNullOrEmpty(outputArr[0].Type.Name))
                    {
                        body.AppendDivElement("outputs", outerDiv =>
                        {
                            outerDiv.AppendHeadingElement(2, "Outputs");
                            outerDiv.AppendDivOfClassElement(SectionClass, div =>
                            {
                                div.AppendUlElement(ul => MamlTypeList(ul, outputArr));
                            });
                        });
                    }

                    if (HasItems(notesArr))
                    {
                        HtmlNode ul = null;
                        foreach (var note in notesArr)
                        {
                            if (!String.Equals(note, ""))
                            {
                                string text = note.Text;

                                if (!string.IsNullOrEmpty(text))
                                {
                                    if (ul == null)
                                    {
                                        body.AppendDivElement("notes", outerDiv =>
                                        {
                                            outerDiv.AppendHeadingElement(2, "Notes");
                                            outerDiv.AppendDivOfClassElement(SectionClass, div => ul = div.AppendUlElement());
                                        });
                                    }

                                    ul.AppendLiElement(li => li.AppendParagraphElement(p => p.AppendTextElement(text)));
                                }
                            }
                        }
                    }

                    if (HasItems(exampleArr))
                    {
                        body.AppendDivElement("examples", outerDiv =>
                        {
                            outerDiv.AppendHeadingElement(2, "Examples");
                            outerDiv.AppendDivOfClassElement(SectionClass, div =>
                            {
                                foreach (var example in exampleArr)
                                {
                                    string title = example.Title;
                                    title = title.Trim('-', ' ');

                                    div.AppendHeadingElement(3, title);

                                    var introduction = PSArrayListBaseObject(example.Introduction);

                                    // fix ugly formatting bug in powershell 2.0
                                    bool introFix = (introduction.Count == 1 && string.Equals(PSStringBaseObject(introduction[0] is string ? introduction[0] : introduction[0].Text), "C:\\PS>"));

                                    if (introFix == false)
                                        MamlShortDescription(div, introduction);

                                    div.AppendDivOfClassElement("code", codeDiv =>
                                    {
                                        foreach (var dynCode in PSArrayListBaseObject(example.Code))
                                        {
                                            string code = PSStringBaseObject(dynCode) ?? "";

                                            if (introFix)
                                            {
                                                code = "C:\\PS> " + code;
                                                introFix = false;
                                            }

                                            codeDiv.AppendPreElement(code);
                                        }
                                    });

                                    MamlShortDescription(div, PSArrayListBaseObject(example.Remarks));
                                };
                            });
                        });
                    }

                    if (aliasesObj != null)
                    {
                        body.AppendDivElement("aliases", outerDiv =>
                        {
                            outerDiv.AppendHeadingElement(2, "Aliases");
                            outerDiv.AppendDivOfClassElement(SectionClass, div =>
                            {
                                div.AppendUlElement(ul =>
                                {
                                    foreach (var alias in ((string)aliasesObj).Split('\n'))
                                    {
                                        string trimmed = alias.Trim();
                                        if (!string.IsNullOrEmpty(trimmed))
                                            ul.AppendLiElement(li => li.AppendTextElement(alias));
                                    }
                                });

                            });
                        });
                    }

                    if (HasItems(linksArr))
                    {
                        body.AppendDivElement("related-links", outerDiv =>
                        {
                            outerDiv.AppendHeadingElement(2, "Related topics");
                            outerDiv.AppendDivOfClassElement(SectionClass, div =>
                            {
                                foreach (var relatedLink in linksArr)
                                {
                                    div.AppendParagraphElement(p =>
                                    {
                                        string text = relatedLink.LinkText;
                                        string uri = relatedLink.URI;

                                        p.AppendTextElement(text);

                                        if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(uri))
                                            p.AppendNonBreakingSpace();

                                        if (!string.IsNullOrEmpty(uri))
                                            p.AppendHyperlinkElement(uri, a => a.AppendTextElement(uri));
                                    });
                                }
                            });
                        });


                    }

                });
            });

            return new PSHtmlHelp() { Content = doc.DocumentNode.OuterHtml, IsReduced = isReduced };
        }

        public static PSHtmlHelp BuildHtmlHelp(string code)
        {
            HtmlDocument doc = new HtmlDocument();

            doc.DocumentNode.AppendHtmlElement(html =>
            {
                html.AppendHeadElement();
                html.AppendBodyElement(body =>
                    body.AppendDivOfClassElement(CodeClass, div =>
                        div.AppendPreElement(code)));
            });

            return new PSHtmlHelp() { Content = doc.DocumentNode.OuterHtml };
        }

        private static bool IsValidObject(object obj)
        {
            return obj != null && !string.Equals(obj, "");
        }

        private static bool HasItems(dynamic dynObj)
        {
            return (dynObj != null && dynObj.Count > 0);
        }

        private static dynamic PSArrayListBaseObject(dynamic dynObj)
        {
            if (dynObj == null)
                return new ArrayList();

            object baseObject = ((PSObject)dynObj).BaseObject;

            if (baseObject is ArrayList)
                return baseObject as ArrayList;
            else
            {
                var list = new ArrayList();
                list.Add(dynObj);
                return list;
            }
        }

        private static string PSStringBaseObject(dynamic dynObj)
        {
            if (dynObj is string)
            {
                return dynObj;
            }
            else
            {
                if (dynObj is PSObject)
                {
                    var pso = dynObj as PSObject;
                    
                    if (pso.BaseObject is string)
                    {
                        return pso.BaseObject as string;
                    }
                    else
                    {
                        if (pso.Properties.Any(p => p.Name == "#text"))
                        {
                            pso = pso.Properties["#text"].Value as PSObject;

                            if (pso != null && pso.BaseObject is ArrayList)
                            {
                                string result = "";
                                foreach (var line in pso.BaseObject as ArrayList)
                                {
                                    if (line is string)
                                        result += line;
                                }
                                return result;
                            }
                        }
                    }
                }
            }

            return null;
        }

    }
}
