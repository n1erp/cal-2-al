using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class ExplicitSizeStruct : DefaultTypeDef, INestedTypeDefinition, INamedTypeDefinition, ITypeDefinition, IDefinition, IReference, ITypeReference, INamedTypeReference, INamedEntity, ITypeDefinitionMember, ITypeMemberReference, INestedTypeReference
{
	private readonly uint _size;

	private readonly INamedTypeDefinition _containingType;

	private readonly ITypeReference _sysValueType;

	public override ushort Alignment => 1;

	public override LayoutKind Layout => LayoutKind.Explicit;

	public override uint SizeOf => _size;

	public string Name => "__StaticArrayInitTypeSize=" + _size;

	public ITypeDefinition ContainingTypeDefinition => _containingType;

	public TypeMemberVisibility Visibility => TypeMemberVisibility.Private;

	public override bool IsValueType => true;

	public override INestedTypeReference AsNestedTypeReference => this;

	internal ExplicitSizeStruct(uint size, PrivateImplementationDetails containingType, ITypeReference sysValueType)
	{
		_size = size;
		_containingType = containingType;
		_sysValueType = sysValueType;
	}

	public override string ToString()
	{
		return _containingType.ToString() + "." + Name;
	}

	public override ITypeReference GetBaseClass(EmitContext context)
	{
		return _sysValueType;
	}

	public override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	public ITypeReference GetContainingType(EmitContext context)
	{
		return _containingType;
	}

	public override INestedTypeDefinition AsNestedTypeDefinition(EmitContext context)
	{
		return this;
	}
}
