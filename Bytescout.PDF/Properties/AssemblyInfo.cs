using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("ByteScout PDF SDK")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ByteScout Inc.")]
[assembly: AssemblyProduct("ByteScout PDF SDK")]
[assembly: AssemblyCopyright("Copyright © 2015-2018 ByteScout Inc.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if FULL
[assembly: AssemblyDescription("ByteScout PDF SDK [FULL]")]
#else
[assembly: AssemblyDescription("ByteScout PDF SDK [TRIAL]")]
#endif

[assembly: ComVisible(true)]
[assembly: Guid("3726fc50-455b-46c5-95b3-4040bc40acfe")]

#if NET4
[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]
#endif

[assembly: CLSCompliant(true)]

[assembly: InternalsVisibleTo("Bytescout.PDF.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d53e6a28e23c1e8496a5b0059875c64e641eec47e8dd4c29fde83befb508234d1f542de402f594b157e2a61d3bb88f36e59471b643a92f9ca4e1551e576e0a57e90cb6b7cf805bf035686f0ab64216c175812b84f1f31138fb780ef916604b3afcfc2dc65d95a8d60ce3464de2a50e59ed717fcc2bc1ee35bb1210b57192d7b2")]