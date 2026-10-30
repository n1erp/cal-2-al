using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class LocalConstantDefinition : ILocalDefinition, INamedEntity
{
	public string Name { get; }

	public Location Location { get; }

	public MetadataConstant CompileTimeValue { get; }

	public ITypeReference Type => CompileTimeValue.Type;

	public bool IsConstant => true;

	public ImmutableArray<ICustomModifier> CustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	public bool IsModified => false;

	public bool IsPinned => false;

	public bool IsReference => false;

	public LocalSlotConstraints Constraints => LocalSlotConstraints.None;

	public LocalVariableAttributes PdbAttributes => LocalVariableAttributes.None;

	public ImmutableArray<bool> DynamicTransformFlags { get; }

	public ImmutableArray<string> TupleElementNames { get; }

	public int SlotIndex => -1;

	public byte[] Signature => null;

	public LocalSlotDebugInfo SlotInfo => new LocalSlotDebugInfo(SynthesizedLocalKind.UserDefined, LocalDebugId.None);

	public LocalConstantDefinition(string name, Location location, MetadataConstant compileTimeValue, ImmutableArray<bool> dynamicTransformFlags, ImmutableArray<string> tupleElementNames)
	{
		Name = name;
		Location = location;
		CompileTimeValue = compileTimeValue;
		DynamicTransformFlags = dynamicTransformFlags.NullToEmpty();
		TupleElementNames = tupleElementNames.NullToEmpty();
	}
}
