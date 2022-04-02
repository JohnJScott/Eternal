using System.Reflection;

// General Information about an assembly is controlled through the following set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle( "Ionic's Zip Library (Reduced)" )]

#if DEBUG
[assembly: AssemblyConfiguration( "Debug" )]
[assembly: AssemblyDescription( "a library for handling zip archives. http://www.codeplex.com/DotNetZip.  This is a reduced version; it lacks SFX support. (Flavor=Debug)" )]
#else

[assembly: AssemblyConfiguration( "Retail" )]
[assembly: AssemblyDescription( "a library for handling zip archives. http://www.codeplex.com/DotNetZip.  This is a reduced version; it lacks SFX support. (Flavor=Retail)" )]
#endif
