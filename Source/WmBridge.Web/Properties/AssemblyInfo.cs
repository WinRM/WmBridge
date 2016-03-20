using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: Microsoft.Owin.OwinStartup(typeof(WmBridge.Web.Startup))]
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("WinRM Bridge")]
[assembly: AssemblyCompany("Jan Lucansky")]
[assembly: AssemblyProduct("WmBridge")]
[assembly: AssemblyCopyright("Copyright (c) 2016 Jan Lucansky. All rights reserved.")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("254948c6-8d95-4313-a8ac-a8a02adab90d")]
