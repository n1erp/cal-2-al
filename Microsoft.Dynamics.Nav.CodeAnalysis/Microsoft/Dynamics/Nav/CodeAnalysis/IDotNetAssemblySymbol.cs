using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IDotNetAssemblySymbol : ISymbol, IEquatable<ISymbol>
{
	DotNetAssemblyInfo AssemblyInfo { get; }
}
