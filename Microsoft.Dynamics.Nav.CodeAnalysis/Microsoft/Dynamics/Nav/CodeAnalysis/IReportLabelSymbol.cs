using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
public interface IReportLabelSymbol : ISymbol, IEquatable<ISymbol>
{
}
