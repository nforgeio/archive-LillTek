﻿//-----------------------------------------------------------------------------
// FILE:        AssemblyInfo.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright (c) 2005-2015 by Jeffrey Lill.  All rights reserved.
// DESCRIPTION: 

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("LillTek Telephony Client Library")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration(LillTek.Build.Configuration)]
[assembly: AssemblyCompany(LillTek.Build.Company)]
[assembly: AssemblyProduct(LillTek.Build.Product)]
[assembly: AssemblyCopyright(LillTek.Build.Copyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion(LillTek.Build.Version)]
[assembly: AssemblyFileVersion(LillTek.Build.Version)]

[assembly: InternalsVisibleTo("LillTek.Telephony.Test, PublicKey=" + LillTek.AssemblySigningKey.UnitTestKey)]
[assembly: InternalsVisibleTo("LillTek.Telephony.NeonSwitch, PublicKey=" + LillTek.AssemblySigningKey.PlatformKey)]
[assembly: InternalsVisibleTo("LillTek.Telephony.NeonSwitchCore, PublicKey=" + LillTek.AssemblySigningKey.PlatformKey)]