using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.Cci;

internal interface ILocalDefinition : INamedEntity
{
	MetadataConstant CompileTimeValue { get; }

	ImmutableArray<ICustomModifier> CustomModifiers { get; }

	bool IsPinned { get; }

	bool IsReference { get; }

	LocalSlotConstraints Constraints { get; }

	LocalVariableAttributes PdbAttributes { get; }

	ImmutableArray<bool> DynamicTransformFlags { get; }

	ImmutableArray<string> TupleElementNames { get; }

	ITypeReference Type { get; }

	Location Location { get; }

	int SlotIndex { get; }

	byte[] Signature { get; }

	LocalSlotDebugInfo SlotInfo { get; }
}
