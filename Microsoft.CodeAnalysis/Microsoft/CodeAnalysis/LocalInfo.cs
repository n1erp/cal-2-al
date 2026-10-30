using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis;

[StructLayout(LayoutKind.Auto)]
internal struct LocalInfo<TypeSymbol> where TypeSymbol : class
{
	internal readonly byte[] SignatureOpt;

	internal readonly TypeSymbol Type;

	internal readonly ImmutableArray<ModifierInfo<TypeSymbol>> CustomModifiers;

	internal readonly LocalSlotConstraints Constraints;

	public bool IsByRef => (Constraints & LocalSlotConstraints.ByRef) != 0;

	public bool IsPinned => (Constraints & LocalSlotConstraints.Pinned) != 0;

	internal LocalInfo(TypeSymbol type, ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers, LocalSlotConstraints constraints, byte[] signatureOpt)
	{
		Type = type;
		CustomModifiers = customModifiers;
		Constraints = constraints;
		SignatureOpt = signatureOpt;
	}

	internal LocalInfo<TypeSymbol> WithSignature(byte[] signature)
	{
		return new LocalInfo<TypeSymbol>(Type, CustomModifiers, Constraints, signature);
	}
}
