//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using WmBridge.Web.Model;

namespace WmBridge.Web
{
    public static class RequestExtensions
    {
        public static object GetPSConnection(this HttpRequestMessage request)
        {
            object result;
            request.Properties.TryGetValue(PSSessionManager.PSConnectionKey, out result);
            return result;
        }

        public static IDictionary<string, object> GetPSState(this HttpRequestMessage request)
        {
            object result;
            request.Properties.TryGetValue(PSSessionManager.PSConnectionStateKey, out result);
            return (IDictionary<string, object>)result;
        }

        public static Version GetPSVersion(this HttpRequestMessage request)
        {
            object objPsVersion;
            request.GetPSState().TryGetValue(PSSessionManager.PSVersionKey, out objPsVersion);
            return objPsVersion as Version;
        }

        public static string GetPSVersionString(this HttpRequestMessage request)
        {
            var version = request.GetPSVersion();
            if (version == null)
                return string.Empty;
            else
                return version.ToString();
        }

        public static Version GetClientVersion(this HttpRequestMessage request)
        {
            object objVersion;
            request.GetPSState().TryGetValue(PSSessionManager.ClientVersionKey, out objVersion);
            return objVersion as Version;
        }

        public static PowerShell CreatePowerShell(this HttpRequestMessage request)
        {
            var result = PowerShell.Create();

            var psrun = request.GetPSConnection();

            if (psrun is Runspace)
                result.Runspace = (Runspace)psrun;

            if (psrun is RunspacePool)
                result.RunspacePool = (RunspacePool)psrun;

            return result;
        }

        public static Collection<PSObject> InvokeScript(this HttpRequestMessage request, PowerShell powershell, string script, params object[] arguments)
        {
            powershell.AddScript(script);
            powershell.AddArguments(arguments);

            Collection<PSObject> psData = null;
            try
            {
                psData = powershell.Invoke();
            }
            catch (RemoteException ex)
            {
                request.ThrowHttpException(HttpStatusCode.InternalServerError, ex);
            }
            catch (Exception)
            {
                // connections was aborted or has invalid state
                request.CloseSharedConnection();
                throw;
            }

            return psData;
        }

        public static void ThrowHttpException(this HttpRequestMessage request, HttpStatusCode statusCode, Exception ex)
        {
            if (ex is HttpResponseException)
                throw ex;

            bool isAuthenticated = GetPSConnection(request) != null; // send detailed exception only if current session is authenticated
            throw new HttpResponseException(request.CreateErrorResponse(statusCode, new HttpError(ex, isAuthenticated)));
        }

        public static void CloseSharedConnection(this HttpRequestMessage request)
        {
            string connectionHash = request.GetOwinContext().Authentication.User.Claims
                .Where(c => c.Type == ClaimTypes.Hash).Single().Value;

            PSSessionManager.Default.Disconnect(connectionHash);
        }
    }
}
