Before you install a WinRM bridge service on another computer, please ensure
you meet these requirements:

 Supported Operating Systems (Minimum OS Versions):
  * Windows 7 SP1
  * Windows 8 or higher
  * Windows Server 2008 SP2
  * Windows Server 2008 R2 SP1
  * Windows Server 2012 or higher

 OS Component requirements:
  * Full .NET Framework 4.5 or higher
  * PowerShell 3.0 or higher

Otherwise, you get an error message during the installation process. If that
happens, check the log folder for more detailed error. Log folder will be
created when you first run WmBridge.exe.

When configured, this package also contains all necessary certificates
to secure SSL connections.
These certificates will be installed to appropriate stores and the .CER, .PFX
files will be deleted accordingly.

This package contains sensitive data. Please, be careful especially with .PFX
file that contains the private key of your WinRM bridge service.
For the same reason, don't distribute or share this pre-configured package to
any unnecessary locations or media.


To install WinRM bridge service manually from pre-configured package, start
a command prompt (cmd.exe) as an Administrator, go to the extracted 
WinRM Bridge folder and run this command:

    wmbridge install


To uninstall bridge service manually, run this command as an Administrator:

    wmbridge uninstall
    

For more advanced scenarios, please visit http://winrmapp.com/installation
