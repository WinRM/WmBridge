//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Web.Http;
using WmBridge.Web.Model;

namespace WmBridge.Web.Controllers
{
    [RoutePrefix("local")]
    public class LocalAccountController : PSApiController
    {
        const int ADS_UF_ACCOUNTDISABLE = 0x00000002;
        const int ADS_UF_PASSWD_CANT_CHANGE = 0x00000040;
        const int ADS_UF_DONT_EXPIRE_PASSWD = 0x00010000;

        [Route("groups"), HttpGet]
        public IHttpActionResult GetGroups()
        {
            return Json(InvokePowerShell("Get-WmiObject Win32_Group -F 'LocalAccount=true'", PSSelect("Name", "Domain", "Description", "SID")));
        }

        [Route("users"), HttpGet]
        public IHttpActionResult GetUsers()
        {
            return Json(InvokePowerShell("Get-WmiObject Win32_UserAccount -F 'LocalAccount=true'", PSSelect(
                "Name","Domain","Description","AccountType","Disabled","FullName","Lockout","PasswordChangeable","PasswordExpires","PasswordRequired","SID")));
        }

        [Route("group/{group}/members"), HttpGet]
        public IHttpActionResult GetGroupMembers(string group)
        {
            return Json(InvokePowerShell("Get-WmiObject Win32_GroupUser -F (\"GroupComponent=\"\"Win32_Group.Domain='$env:COMPUTERNAME',Name='$($args[0])'\"\"\") | %{ if ($_.PartComponent -match '(?<=Domain\\=\")(?<Domain>[^\"]+).*(?<=Name\\=\")(?<Name>[^\"]+)') { New-Object PSObject -Property $Matches }}",
                PSSelect("Name", "Domain"), group));
        }

        [Route("user/{user}/membership"), HttpGet]
        public IHttpActionResult GetUserGroupMembership(string user)
        {
            return Json(InvokePowerShell("Get-WmiObject Win32_GroupUser -F (\"PartComponent=\"\"Win32_UserAccount.Domain='$env:COMPUTERNAME',Name='$($args[0])'\"\"\") | %{ if ($_.GroupComponent -match '(?<=Domain\\=\")(?<Domain>[^\"]+).*(?<=Name\\=\")(?<Name>[^\"]+)') { New-Object PSObject -Property $Matches }}",
                PSSelect("Name", "Domain"), user));
        }

        [Route("group/{group}/add"), HttpGet]
        public IHttpActionResult AddGroupMember(string group, [FromUri] string user)
        {
            InvokePowerShell("([ADSI]\"WinNT://./$($args[0]),Group\").Add(\"WinNT://$($args[1])\")", group, user.Replace('\\', '/'));
            return Ok();
        }

        [Route("group/{group}/remove"), HttpGet]
        public IHttpActionResult RemoveGroupMember(string group, [FromUri] string user)
        {
            InvokePowerShell("([ADSI]\"WinNT://./$($args[0]),Group\").Remove(\"WinNT://$($args[1])\")", group, user.Replace('\\', '/'));
            return Ok();
        }

        [Route("user/{user}/disable"), HttpGet]
        public IHttpActionResult DisableUser(string user)
        {
            return _SetUserProperty_WMI(user, "Disabled", true);
        }

        [Route("user/{user}/enable"), HttpGet]
        public IHttpActionResult EnableUser(string user)
        {
            return _SetUserProperty_WMI(user, "Disabled", false);
        }

        [Route("user/{user}/unlock"), HttpGet]
        public IHttpActionResult UnlockUser(string user)
        {
            return _SetUserProperty_WMI(user, "Lockout", false);
        }

        [Route("user/{user}/reset"), HttpGet]
        public IHttpActionResult SetUserPassword(string user, [FromUri] string password)
        {
            InvokePowerShell("([ADSI]\"WinNT://./$($args[0]),User\").SetPassword($args[1])", user, password);
            return Ok();
        }

        [Route("user/{user}/expires"), HttpGet]
        public IHttpActionResult SetUserExpires(string user, [FromUri] int value)
        {
            return _SetUserProperty_WMI(user, "PasswordExpires", value == 1);
        }

        [Route("user/{user}/fullname"), HttpGet]
        public IHttpActionResult SetUserFullName(string user, [FromUri] string value)
        {
            return _SetUserProperty_WMI(user, "FullName", value);
        }

        [Route("user/{user}/description"), HttpGet]
        public IHttpActionResult SetUserDescription(string user, [FromUri] string value)
        {
            return _SetUserProperty_ADSI(user, "Description", value);
        }

        [Route("user/{user}/passmustchange"), HttpGet]
        public IHttpActionResult SetUserPassMustChange(string user, [FromUri] int value)
        {
            return _SetUserProperty_ADSI(user, "PasswordExpired", value);
        }

        [Route("user/{user}/passcantchange"), HttpGet]
        public IHttpActionResult SetUserPassCantChange(string user, [FromUri] int value)
        {
            return _SetUserFlag_ADSI(user, value == 1, ADS_UF_PASSWD_CANT_CHANGE);
        }

        [Route("user/{user}/create"), HttpGet]
        public IHttpActionResult CreateUser(string user)
        {
            InvokePowerShell("([ADSI]'WinNT://.,Computer').Create('User',$args[0]).SetInfo()", user);
            return Ok();
        }

        [Route("user/{user}/delete"), HttpGet]
        public IHttpActionResult DeleteUser(string user)
        {
            InvokePowerShell("([ADSI]'WinNT://.,Computer').Delete('User',$args[0])", user);
            return Ok();
        }

        [Route("user/{oldUser}/rename"), HttpGet]
        public IHttpActionResult RenameUser(string oldUser, [FromUri] string user)
        {
            InvokePowerShell("(Get-WmiObject Win32_UserAccount -F \"LocalAccount=true and Name='$($args[0])'\").Rename($args[1])", oldUser, user);
            return Ok();
        }

        [Route("user/{user}/detail"), HttpGet]
        public IHttpActionResult GetUserDetail(string user)
        {
            var wmiAccount= PSSelectTagged(0,
                "Name", 
                "Domain", 
                "FullName",
                "Description",
                "Disabled",
                "Lockout",
                "PasswordExpires",
                "PasswordChangeable",
                "PasswordRequired",
                "AccountType", 
                "SID");

            var adsiAccount = PSSelectTagged(1,
                "AuthenticationType".As<string>(),
                PSPropertyValue("AutoUnlockInterval"),
                PSPropertyValue("BadPasswordAttempts"),
                PSPropertyValue("HomeDirDrive"),
                PSPropertyValue("LockoutObservationInterval"),
                PSPropertyValue("MaxBadPasswordsAllowed"),
                "MaxLogins".As<int>(),
                PSPropertyValue("MaxPasswordAge"),
                PSPropertyValue("MinPasswordAge"),
                PSPropertyValue("MinPasswordLength"),
                PSPropertyValue("PasswordAge"),
                "PasswordExpirationDate".As<DateTime>(),
                PSPropertyValue("PasswordExpired").Transform(x => (int?)x == 1),
                PSPropertyValue("PasswordHistoryLength"),
                "PasswordLastChanged".As<DateTime>(),
                "PasswordMinimumLength".As<int>(),
                PSPropertyValue("UserFlags"));

            string script = PSArray(
                AppendSelectCommand("Get-WmiObject Win32_UserAccount -F \"LocalAccount=true and Name='$($args[0])'\"", wmiAccount),
                AppendSelectCommand("[ADSI]\"WinNT://./$($args[0]),User\"", adsiAccount));

            return Json(InvokePowerShell(script, PSSelect(wmiAccount, adsiAccount), user).Merge());
        }

        #region Helpers
        PSPropertySelector PSPropertyValue(string psPropertyName)
        {
            return psPropertyName.Expression(string.Format("($_.{0}).Value", psPropertyName));
        }

        IHttpActionResult _SetUserProperty_WMI(string user, string property, object value)
        {
            InvokePowerShell("$account = Get-WmiObject Win32_UserAccount -F \"LocalAccount=true and Name='$($args[0])'\";  $account." + property + " = $args[1]; $account.Put()", user, value);
            return Ok();
        }

        IHttpActionResult _SetUserProperty_ADSI(string user, string property, object value)
        {
            InvokePowerShell("$account = [ADSI]\"WinNT://./$($args[0]),User\"; $account." + property + " = $args[1]; $account.SetInfo()", user, value);
            return Ok();
        }

        IHttpActionResult _SetUserFlag_ADSI(string user, bool state, int flag)
        {
            InvokePowerShell("$account = [ADSI]\"WinNT://./$($args[0]),User\"; $account.UserFlags.Value = $account.UserFlags.Value " + (state ? "-bor" : "-bxor") + " $args[1]; $account.SetInfo()", user, flag);
            return Ok();
        }

        #endregion
    }
}
