using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal interface IDotNetControlAddInMemberSymbol : ISymbol, IEquatable<ISymbol>
{
	bool IsVisibleInControlAddIn { get; }
}
