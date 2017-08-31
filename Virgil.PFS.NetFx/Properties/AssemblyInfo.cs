using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Virgil.PFS")]
[assembly: AssemblyDescription("Perfect Forward Secrecy (PFS) is a technique that protects previously intercepted traffic from being decrypted even if the main private key is compromised. \nWith PFS enabled communication, a hacker could only access information that is actively transmitted because PFS forces a system to create different keys per session.In other words, PFS makes sure there is no master key to decrypt all the traffic.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Virgil Security, Inc")]
[assembly: AssemblyProduct("Virgil.PFS")]
[assembly: AssemblyCopyright("© 2016 Virgil Security, Inc.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("3d033017-eadc-4b96-9092-7c183b56281c")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: InternalsVisibleTo("Virgil.PFS.Tests")]
