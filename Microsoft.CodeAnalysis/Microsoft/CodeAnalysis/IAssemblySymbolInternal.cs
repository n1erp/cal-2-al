using System;

namespace Microsoft.CodeAnalysis;

internal interface IAssemblySymbolInternal : IAssemblySymbol, ISymbol, IEquatable<ISymbol>
{
	Version AssemblyVersionPattern { get; }
}
