using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
public interface IQueryColumnSymbol : IQueryElementSymbol, ISymbol, IEquatable<ISymbol>
{
}
