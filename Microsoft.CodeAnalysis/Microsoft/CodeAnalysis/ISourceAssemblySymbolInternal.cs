using System;
using System.Reflection;

namespace Microsoft.CodeAnalysis;

internal interface ISourceAssemblySymbolInternal : ISourceAssemblySymbol, IAssemblySymbol, ISymbol, IEquatable<ISymbol>
{
	AssemblyFlags AssemblyFlags { get; }

	string SignatureKey { get; }

	AssemblyHashAlgorithm HashAlgorithm { get; }

	Version AssemblyVersionPattern { get; }

	bool InternalsAreVisible { get; }
}
