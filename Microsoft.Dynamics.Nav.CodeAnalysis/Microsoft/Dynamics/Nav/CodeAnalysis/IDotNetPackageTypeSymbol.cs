using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
internal interface IDotNetPackageTypeSymbol : IObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers, IRootTypeSymbol
{
}
