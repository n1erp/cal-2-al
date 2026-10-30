using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
[Flags]
public enum SymbolFilter
{
	None = 0,
	Namespace = 1,
	Type = 2,
	Member = 4,
	TypeAndMember = Type | Member,
	All = TypeAndMember | Namespace
}
