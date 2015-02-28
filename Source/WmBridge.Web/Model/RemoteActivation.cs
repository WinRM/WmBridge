//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using log4net;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace WmBridge.Web.Model
{
    public static class RemoteActivation
    {
        private static readonly ILog logger = LogManager.GetLogger("WmBridge.RemoteActivation");

        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_PROVIDER_DEFAULT = 0;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool LogonUser(string lpszUsername, string lpszDomain, IntPtr lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken );

        [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        extern static bool CreateProcessAsUser(IntPtr hToken, String lpApplicationName, String lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandle, int dwCreationFlags, IntPtr lpEnvironment,
            String lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx", SetLastError = true)]
        extern static bool DuplicateTokenEx(IntPtr ExistingTokenHandle, uint dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, int TokenType,
            int ImpersonationLevel, ref IntPtr DuplicateTokenHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetExitCodeProcess(IntPtr hProcess, out uint ExitCode);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int lstrlenW(IntPtr ptr);

        [StructLayout(LayoutKind.Sequential)]
        struct SECURITY_ATTRIBUTES
        {
            public int Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct STARTUPINFO
        {
            public int cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        private static readonly Dictionary<int, string> paexecErrors = new Dictionary<int, string>()
        {
            {-1, "Internal error."},
            {-2, "Command line error."},
            {-3, "Failed to launch local application."},
            {-4, "Failed to copy PAExec to remote (connection to ADMIN$ might have failed)."},
            {-5, "Connection to computer taking too long (timeout)."},
            {-6, "Service could not be installed or started on remote computer."},
            {-7, "Could not communicate with remote PAExec service."},
            {-8, "Failed to copy application to remote computer."},
            {-9, "Failed to launch remote application."},
            {-10, "Application was terminated after timeout expired."},
        };

        const int timeoutToKill = 120;

        public static void ConfigureWinRM(string host, string userName, SecureString password)
        {
            string tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            Directory.CreateDirectory(tmpDir);

            try
            {
                GrantAccess(tmpDir); // because impersonated user must be able to read from it

                var quickconfigFilePath = Path.Combine(tmpDir, "quickconfig.cmd");
                var pwdFilePath = Path.Combine(tmpDir, "pwd");

                var directoryPathToCodeBase = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

                string[] domainWithUserName = userName.Split('\\');

                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = Path.Combine(directoryPathToCodeBase, "paexec.exe"),
                    WorkingDirectory = tmpDir,
                    Arguments = string.Format(@"\\{0} -u ""{1}"" -p@ ""{2}"" -p@d -n 25 -to 100 -h -c ""{3}""", host, userName, "pwd", @".\" + Path.GetFileName(quickconfigFilePath)),
                    UserName = domainWithUserName.Last(),
                    Password = password,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (domainWithUserName.Length == 2)
                    psi.Domain = domainWithUserName.First();

                if (File.Exists(psi.FileName) == false)
                    throw new Exception("Can not find the file: " + psi.FileName);

                File.WriteAllLines(quickconfigFilePath, new string[]
                {
                    "@echo off",
                    "call winrm quickconfig -quiet 1>NUL 2>NUL",
                    "call winrm create winrm/config/Listener?Address=*+Transport=HTTP 1>NUL 2>NUL",
                    "call winrm set winrm/config/service/auth @{CredSSP=\"true\"} 1>NUL 2>NUL",
                    "call netsh advfirewall firewall set rule name=\"Windows Remote Management (HTTP-In)\" new enable=Yes 1>NUL 2>NUL",
                    "call sc config winrm start=auto 1>NUL 2>NUL",
                    "call net start winrm 1>NUL 2>NUL",
                    "call winrm e winrm/config/Listener 1>NUL 2>NUL",
                    "exit %errorlevel%"
                });

                using (FileStream stream = new FileStream(pwdFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    SecureStringToStream(stream, password);

                try
                {
                    // mostly work on domain enviroment
                    StartProcessAsUser(psi);
                }
                catch (Exception ex)
                {
                    try
                    {
                        psi.UserName = null;
                        psi.Password = null;
                        psi.Domain = null;

                        // starting process as specified user doesn't work so now try as same user as the WmBridge is running
                        StartProcess(psi); // this work on non-domain env.
                    }
                    catch
                    {
                        logger.Error(ex); // log previous exception too
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
            finally
            {
                Directory.Delete(tmpDir, true);
            }

            logger.InfoFormat("WinRM on computer '{0}' successfully activated by user '{1}'", host, userName);
        }

        private static void GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }

        private static void StartProcess(ProcessStartInfo psi)
        {
            using (var process = Process.Start(psi))
            {
                int exitCode;
                if (!process.WaitForExit(timeoutToKill * 1000))
                {
                    process.Kill(); // wait 2 minutes, then kill the process
                    exitCode = -10;
                }
                else
                    exitCode = process.ExitCode;

                ValidateExitCode(exitCode);
            }
        }

        private static void ValidateExitCode(int exitCode)
        {
            if (exitCode != 0)
            {
                if (paexecErrors.ContainsKey(exitCode))
                    throw new Exception("PAExec: " + paexecErrors[exitCode]);
                else
                    throw new Exception("WinRM remote activation failed.");
            }
        }

        private static void StartProcessAsUser(ProcessStartInfo psi)
        {
            string username = psi.UserName;
            string domain = psi.Domain;
            SecureString password = psi.Password;
            string commandLinePath = "\"" + psi.FileName + "\" " + psi.Arguments;
            string cwd = psi.WorkingDirectory;

            IntPtr Token = IntPtr.Zero;
            IntPtr DupedToken = IntPtr.Zero;

            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.bInheritHandle = false;
            sa.Length = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = IntPtr.Zero;

            if (string.IsNullOrEmpty(domain)) domain = ".";

            IntPtr passwordPtr = Marshal.SecureStringToGlobalAllocUnicode(password ?? new SecureString());
            try
            {
                if (!LogonUser(username, domain, passwordPtr, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref Token))
                {
                    var ex = new Win32Exception();
                    CloseHandle(Token);
                    throw ex;
                }
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(passwordPtr);
            }

            const uint GENERIC_ALL = 0x10000000;

            const int SecurityImpersonation = 2;
            const int TokenType = 1;

            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            try
            {
                if (!DuplicateTokenEx(Token, GENERIC_ALL, ref sa, SecurityImpersonation, TokenType, ref DupedToken))
                    throw new Win32Exception();

                STARTUPINFO si = new STARTUPINFO();
                si.cb = Marshal.SizeOf(si);

                if (!CreateProcessAsUser(DupedToken, null, commandLinePath, ref sa, ref sa, false, 0, (IntPtr)0, cwd, ref si, out pi))
                    throw new Win32Exception();

                uint exitCode = 0;
                bool timeout = false;

                ProcessWaitHandle waitable = new ProcessWaitHandle(pi.hProcess);
                if (!waitable.WaitOne(timeoutToKill * 1000))
                {
                    timeout = true;
                    using (Process process = Process.GetProcessById((int)pi.dwProcessId))
                        if (process != null)
                            process.Kill();
                }
                else
                {
                    if (!GetExitCodeProcess(pi.hProcess, out exitCode))
                        throw new Win32Exception();
                }

                ValidateExitCode(timeout ? -10 : (int)exitCode);
            }
            finally
            {
                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);

                CloseHandle(DupedToken);
                CloseHandle(Token);
            }
        }

        static void SecureStringToStream(Stream stream, SecureString secureString)
        {
            if (secureString == null)
                throw new ArgumentNullException("secureString");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);

                int count = lstrlenW(unmanagedString);

                using (var w = new StreamWriter(stream))
                {
                    for (int i = 0; i < count; i++)
                    {
                        w.Write(Convert.ToChar(Marshal.ReadInt16(unmanagedString, i*2)));
                        w.Flush();
                    }
                }
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        class ProcessWaitHandle : WaitHandle
        {
            public ProcessWaitHandle(IntPtr processHandle)
            {
                this.SafeWaitHandle = new SafeWaitHandle(processHandle, false);
            }
        }

    }
}
