using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IReportTypeSymbol : IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
}
