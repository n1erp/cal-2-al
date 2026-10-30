using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis;

[StructLayout(LayoutKind.Auto)]
internal struct ParamInfo<TypeSymbol> where TypeSymbol : class
{
	internal bool IsByRef;

	internal TypeSymbol Type;

	internal ParameterHandle Handle;

	internal ImmutableArray<ModifierInfo<TypeSymbol>> RefCustomModifiers;

	internal ImmutableArray<ModifierInfo<TypeSymbol>> CustomModifiers;
}
