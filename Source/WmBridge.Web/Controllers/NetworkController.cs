//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Net.Http;
using System.Management.Automation;

namespace WmBridge.Web.Controllers
{
    [RoutePrefix("net")]
    public class NetworkController : PSApiController
    {
        [Route("address"), HttpGet]
        public IHttpActionResult GetAddress()
        {
            return Json(InvokePowerShell(
                "$addresses = (Get-WmiObject Win32_NetworkAdapterConfiguration | ? {$_.IPEnabled -and $_.IPAddress} | select -expand IPAddress);" +
                "New-Object PSObject -Property @{'IPv4'= @($addresses | ? {([IPAddress]$_).AddressFamily -eq 'Internetwork'}); 'IPv6'= @($addresses | ? {([IPAddress]$_).AddressFamily -eq 'InterNetworkV6'})}",
                PSSelect("IPv4".Transform(x => ((PSObject)x).BaseObject), "IPv6".Transform(x => ((PSObject)x).BaseObject))).Single());
        }

        [Route("adapter"), HttpGet]
        public IHttpActionResult GetAdapters()
        {
            return Json(InvokePowerShell("Get-WmiObject Win32_NetworkAdapter -F 'NetConnectionID IS NOT NULL'", PSSelect(
                "DeviceId", "Name", "NetConnectionId", "MACAddress", "Manufacturer", "ProductName", "Speed", "AdapterType", "AdapterTypeId")));
        }

        [Route("adapter/{deviceId}/detail"), HttpGet]
        public IHttpActionResult GetAdapterDetail(int deviceId)
        {
            return Json(InvokePowerShell("Get-WmiObject Win32_NetworkAdapterConfiguration -F ('Index=' + (gwmi Win32_NetworkAdapterSetting -F \"Element='Win32_NetworkAdapter.DeviceID=\"\"$($args[0])\"\"'\" | %{ if ($_.Setting -match '.*\\.Index=(?<Index>\\d+)$') { [int]$Matches[\"Index\"] }}))", PSSelect(
                "Index",
                "DHCPLeaseExpires".TransformDmtfDate(),
                "Description",
                "DHCPEnabled",
                "DHCPLeaseObtained".TransformDmtfDate(),
                "DHCPServer",
                "DNSDomain",
                "DNSDomainSuffixSearchOrder".TransformArray(),
                "DNSEnabledForWINSResolution",
                "DNSHostName",
                "DNSServerSearchOrder".TransformArray(),
                "DomainDNSRegistrationEnabled",
                "FullDNSRegistrationEnabled",
                "IPAddress".TransformArray(),
                "IPSubnet".TransformArray(),
                "IPConnectionMetric",
                "IPEnabled",
                "WINSEnableLMHostsLookup",
                "WINSHostLookupFile",
                "WINSPrimaryServer",
                "WINSScopeID",
                "WINSSecondaryServer",
                "DefaultIPGateway".TransformArray(),
                "GatewayCostMetric".TransformArray(),
                "DefaultTOS",
                "DefaultTTL",
                "MACAddress"), deviceId).Single());
        }

        [Route("adapter/{index}/dhcp"), HttpGet]
        public IHttpActionResult EnableDHCP(int index)
        {
            return Json(InvokePowerShell("(Get-WmiObject Win32_NetworkAdapterConfiguration -F \"Index=$($args[0])\").EnableDHCP()", PSSelectReturnValue(), index).Single());
        }

        [Route("adapter/{index}/static"), HttpPost]
        public IHttpActionResult EnableStatic(int index, [FromBody] string[] addressWithBits)
        {
            var addressWithMask = addressWithBits.Select(addr=>
            {
                var parts = addr.Split('/'); 
                if (parts.Length != 2) throw new Exception("Invalid address format.");
                return new { ip = parts[0], mask = string.Join(".", BitConverter.GetBytes(~(UInt32.MaxValue >> (int.Parse(parts[1])))).Reverse()) };
            }).ToArray();

            return Json(InvokePowerShell("(Get-WmiObject Win32_NetworkAdapterConfiguration -F \"Index=$($args[0])\").EnableStatic($args[1], $args[2])", PSSelectReturnValue(), index, 
                addressWithMask.Select(a => a.ip).ToArray(),
                addressWithMask.Select(a => a.mask).ToArray())
                .Single());
        }

        [Route("adapter/{index}/gateway"), HttpPost]
        public IHttpActionResult SetGateway(int index, [FromBody] string[] gateway)
        {
            return Json(InvokePowerShell("(Get-WmiObject Win32_NetworkAdapterConfiguration -F \"Index=$($args[0])\").SetGateways($args[1], $null)", PSSelectReturnValue(), index, gateway).Single());
        }

        [Route("adapter/{index}/cleargw"), HttpGet]
        public IHttpActionResult ClearGateway(int index)
        {
            return Json(InvokePowerShell("(Get-WmiObject Win32_NetworkAdapterConfiguration -F \"Index=$($args[0])\").SetGateways((Get-WmiObject Win32_NetworkAdapterConfiguration -F \"Index=$($args[0])\" | % { $_.IPAddress[0] }), $null)", PSSelectReturnValue(), index).Single());
        }

        [Route("adapter/{index}/dns"), HttpPost]
        public IHttpActionResult SetDNSServer(int index, [FromBody] string[] dns)
        {
            return Json(InvokePowerShell("(Get-WmiObject Win32_NetworkAdapterConfiguration -F \"Index=$($args[0])\").SetDNSServerSearchOrder($args[1])", PSSelectReturnValue(), index, dns).Single());
        }

        [Route("adapter/{index}/renew"), HttpGet]
        public IHttpActionResult RenewDHCPLease(int index)
        {
            return Json(InvokePowerShell("(Get-WmiObject Win32_NetworkAdapterConfiguration -F \"Index=$($args[0])\").RenewDHCPLease()", PSSelectReturnValue(), index).Single());
        }

        [Route("adapter/{deviceId}/{operation:regex(^(enable|disable)$)}"), HttpGet]
        public IHttpActionResult AdapterOperation(int deviceId, string operation)
        {
            return Json(InvokePowerShell("(Get-WmiObject Win32_NetworkAdapter -F \"DeviceId=$($args[0])\")." + operation + "()", PSSelect("ReturnValue"), deviceId).Single());
        }

        static readonly Dictionary<int, string> Win32_NetworkAdapterConfiguration_ErrorCodes = new Dictionary<int, string>()
        {
            { 0, "Successful completion, no reboot required." },
            { 1, "Successful completion, reboot required." },
            { 64, "Method not supported on this platform." },
            { 65, "Unknown failure." },
            { 66, "Invalid subnet mask." },
            { 67, "An error occurred while processing an instance that was returned." },
            { 68, "Invalid input parameter." },
            { 69, "More than five gateways specified." },
            { 70, "Invalid IP address." },
            { 71, "Invalid gateway IP address." },
            { 72, "An error occurred while accessing the registry for the requested information." },
            { 73, "Invalid domain name." },
            { 74, "Invalid host name." },
            { 75, "No primary or secondary WINS server defined." },
            { 76, "Invalid file." },
            { 77, "Invalid system path." },
            { 78, "File copy failed." },
            { 79, "Invalid security parameter." },
            { 80, "Unable to configure TCP/IP service." },
            { 81, "Unable to configure DHCP service." },
            { 82, "Unable to renew DHCP lease." },
            { 83, "Unable to release DHCP lease." },
            { 84, "IP not enabled on adapter." },
            { 85, "IPX not enabled on adapter." },
            { 86, "Frame or network number bounds error." },
            { 87, "Invalid frame type." },
            { 88, "Invalid network number." },
            { 89, "Duplicate network number." },
            { 90, "Parameter out of bounds." },
            { 91, "Access denied." },
            { 92, "Out of memory." },
            { 93, "Already exists." },
            { 94, "Path, file, or object not found." },
            { 95, "Unable to notify service." },
            { 96, "Unable to notify DNS service." },
            { 97, "Interface not configurable." },
            { 98, "Not all DHCP leases can be released or renewed." },
            { 100, "DHCP not enabled on adapter." },
        };

        private WmBridge.Web.Model.PSPropertySelector[] PSSelectReturnValue()
        {
            return PSSelect("ReturnValue".Transform(CheckReturnValue));
        }

        private object CheckReturnValue(object returnValue)
        {
            int errorCode = Convert.ToInt32(returnValue);

            if (errorCode == 0)
                return errorCode;

            string error;
            if (!Win32_NetworkAdapterConfiguration_ErrorCodes.TryGetValue(errorCode, out error))
                error = "Undefined error";

            throw new HttpResponseException(Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, error));
        }

    }
}
