using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IDotNetTypeSymbol : IRootTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	DotNetTypeInfo DotNetTypeInfo { get; }
}
