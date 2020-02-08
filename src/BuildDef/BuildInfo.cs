using System.Reflection;

// global attributes for Core assemblies
[assembly: AssemblyCompany("Highlander")]
[assembly: AssemblyProduct("Higlander Core Components")]
[assembly: AssemblyCopyright("Copyright © Alex Watt and Simon Dudley 2019")]
[assembly: AssemblyTrademark("Highlander")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// assembly version
[assembly: AssemblyVersion("1.1.1.1")]

// product version and revision/release
[assembly: AssemblyInformationalVersion("1.1.1.1")]

// file version and build date and number
// the 4 is to trick legacy infrastructure
// it should be a 1.1.1501.1
[assembly: AssemblyFileVersion("3.4.1723.1")]
