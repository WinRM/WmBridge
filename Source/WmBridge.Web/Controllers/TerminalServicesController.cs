//
//  Copyright (c) 2016 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System.Web.Http;
using WmBridge.Web.Filters;

namespace WmBridge.Web.Controllers
{
    [RoutePrefix("ts")]
    [RequireSupportLibrary]
    public class TerminalServicesController : PSApiController
    {
        [Route("sessions"), HttpGet]
        public IHttpActionResult GetSessions()
        {
            return Json(InvokePowerShell("[WmBridge.Support.TerminalServices]::GetSessions() | ? {$_.UserAccount -ne $null}", PSSelect(
                "SessionId",
                "ClientName",
                "ClientIPAddress".As<string>(),
                "ClientProtocolType".As<string>(),
                "ConnectionState".As<string>(),
                "RemoteEndPoint".As<string>(),
                "SessionIPAddress".As<string>(),
                "DomainName",
                "UserName",
                "UserAccount".As<string>(),
                "WindowStationName",
                "CurrentTime".Expression("($_.CurrentTime).ToUniversalTime().Ticks").TransformTruncUtcTicks(),
                "ConnectTime".Expression("($_.ConnectTime).ToUniversalTime().Ticks").TransformTruncUtcTicks(),
                "LoginTime".Expression("($_.LoginTime).ToUniversalTime().Ticks").TransformTruncUtcTicks(),
                "DisconnectTime".Expression("($_.DisconnectTime).ToUniversalTime().Ticks").TransformTruncUtcTicks(),
                "LastInputTime".Expression("($_.LastInputTime).ToUniversalTime().Ticks").TransformTruncUtcTicks(),
                "IdleTime".Expression("$_.IdleTime.Ticks"),
                "IncomingBytes".Expression("$_.IncomingStatistics.Bytes"),
                "OutgoingBytes".Expression("$_.OutgoingStatistics.Bytes"))));
        }

        [Route("{session}/{operation:regex(^(logoff|disconnect)$)}"), HttpGet]
        public IHttpActionResult SessionOperation(int session, string operation)
        {
            InvokePowerShell("[WmBridge.Support.TerminalServices]::" + operation + "($args[0], $true)", session);
            return Ok();
        }
    }
}
