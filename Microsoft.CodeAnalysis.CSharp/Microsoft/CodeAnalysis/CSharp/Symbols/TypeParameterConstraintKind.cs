using System;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

[Flags]
internal enum TypeParameterConstraintKind
{
	None = 0,
	ReferenceType = 1,
	ValueType = 2,
	Constructor = 4,
	Unmanaged = 8
}
