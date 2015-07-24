//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Management;
using System.Management.Automation;
using System.Web.Http;
using WmBridge.Web.Filters;
using WmBridge.Web.Model;

namespace WmBridge.Web.Controllers
{
    [PSAuthentication, ExceptionFilter, Authorize, Logging]
    public abstract class PSApiController : ApiController
    {
        private const string tagProperty = "__TAG__";

        private static readonly ILog logger = LogManager.GetLogger("WmBridge.PSApiController");

        internal protected static ILog Log { get { return logger; } }

        protected PSDictionary[] InvokePowerShell(string script, PSPropertySelector[] selectors, params object[] arguments)
        {
            using (var powershell = Request.CreatePowerShell())
            {
                ILookup<int, PSPropertySelector> selectorsByTag = null;

                if (selectors != null && selectors.Any(sel => sel.Tag != null))
                {
                    selectorsByTag = selectors.ToLookup(sel => sel.Tag.Value); // selectors may use tag, create index for them
                }
                else
                {
                    if ((selectors != null && selectors.Any()) || !script.EndsWith(";"))
                        script += "|" + BuildSelectCommand(selectors); // append Select, only if there is no tagging
                }

                var psData = Request.InvokeScript(powershell, script, arguments);

                if (selectors == null)
                    return null; // empty result because no selector was provided

                return psData // transform PS results to dictionary suitable for JSON formating
                    .WhereNotNull()
                    .Select(psResult =>
                    {
                        int? tag = null;
                        IEnumerable<PSPropertySelector> localSelectors;

                        if (psResult.Properties[tagProperty] != null)
                        {
                            // row is tagged, use only appropriate tagged selectors
                            tag = (int)psResult.Properties[tagProperty].Value;
                            localSelectors = selectorsByTag[tag.Value];
                        }
                        else
                            localSelectors = selectors; // if no tagging, use all selectors

                        return localSelectors.Select(sel =>
                        {
                            var psProp = psResult.Properties[sel.PSPropertyName];
                            if (psProp == null) return null; // propety with given name does not exists
                            return new { sel.Alias, Value = sel.Transformation(psProp.Value) };
                        })
                        .WhereNotNull() // filter missing properties
                        .ToDictionary(g => g.Alias, g => g.Value, () => new PSDictionary() { Tag = tag });
                    })
                    .ToArray();
            }
        }

        protected PSDictionary[] InvokePowerShell(string script, params object[] arguments)
        {
            return InvokePowerShell(script, null, arguments);
        }

        protected static string AppendSelectCommand(string source, params PSPropertySelector[] selectors)
        {
            return string.Format("({0} | {1})", source, BuildSelectCommand(selectors));
        }

        protected static string BuildSelectCommand(params PSPropertySelector[] selectors)
        {
            if (selectors == null || selectors.Length == 0)
                return "Out-Null";
            else
                return "Select " + CommaSeparated(
                        selectors.Select(BuildPropertyNotation)
                        .Distinct().ToArray());
        }

        protected static string BuildPropertyNotation(PSPropertySelector sel)
        {
            return sel.PSExpression == null ? sel.PSPropertyName : string.Format("@{{N='{0}';E={{{1}}}}}", sel.PSPropertyName, sel.PSExpression);
        }

        protected static PSPropertySelector[] PSSelect(params PSPropertySelector[][] selectors)
        {
            return selectors.SelectMany(sel => sel)
                .Where(sel => sel.PSPropertyName != tagProperty)
                .ToArray();
        }

        protected static PSPropertySelector[] PSSelectTagged(int tag, params object[] properties)
        {
            var result = PSSelect(new object[] { tagProperty.Expression(tag.ToString()) }.Concat(properties).ToArray());

            foreach (var item in result.Skip(1))
                item.Tag = tag;

            return result;
        }

        protected static PSPropertySelector[] PSSelect(params object[] properties)
        {
            return properties.Select(prop =>
            {
                if (prop is string)
                    return (prop as string).Alias(prop as string);
                else if (prop is PSPropertySelector)
                    return prop as PSPropertySelector;
                else
                    throw new ArgumentException("Invalid property type");

            }).ToArray();
        }

        protected static string CommaSeparated(params object[] values)
        {
            return string.Join(",", values);
        }

        protected static string PSArray(params object[] values)
        {
            return "@(" + string.Join(",", values) + ")";
        }

        /// <param name="timeout">Timeout in seconds</param>
        protected object PSJobProgress(string jobInstanceId, int timeout)
        {
            // receive-job exception if any and remove job when finished
            var result = InvokePowerShell("$job = Get-Job -InstanceId $args[0] -ea SilentlyContinue; if ($job.PSEndTime) { try {Receive-Job $job | Out-Null} finally {Remove-Job $job} }; if ($job) {Wait-Job $job -Timeout " + timeout + " | Out-Null}; $job",
                PSSelect(
                "PercentComplete".Expression("$_.Progress.PercentComplete"),
                "State".As<string>()
                ), jobInstanceId).SingleOrDefault();

            if (result == null)
                throw new HttpResponseException(Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, "Action canceled."));
            else
                return result;
        }
    }
}
