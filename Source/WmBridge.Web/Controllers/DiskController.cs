//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System.Web.Http;

namespace WmBridge.Web.Controllers
{
    [RoutePrefix("disk")]
    public class DiskController : PSApiController
    {
        [Route("logical"), HttpGet]
        public IHttpActionResult LogicalDisks()
        {
            return Json(InvokePowerShell("Get-WmiObject Win32_LogicalDisk", PSSelect(
                "Name", "VolumeName", "Description", "FileSystem", "DriveType", "Size", "FreeSpace", "VolumeSerialNumber")));
        }
    }
}
