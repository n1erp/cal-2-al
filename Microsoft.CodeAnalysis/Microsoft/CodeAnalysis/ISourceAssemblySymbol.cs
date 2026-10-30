using System;

namespace Microsoft.CodeAnalysis;

public interface ISourceAssemblySymbol : IAssemblySymbol, ISymbol, IEquatable<ISymbol>
{
	Compilation Compilation { get; }
}
