using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal abstract class DefaultTypeDef : ITypeDefinition, IDefinition, IReference, ITypeReference
{
	public IEnumerable<IGenericTypeParameter> GenericParameters => SpecializedCollections.EmptyEnumerable<IGenericTypeParameter>();

	public ushort GenericParameterCount => 0;

	public bool HasDeclarativeSecurity => false;

	public bool IsAbstract => false;

	public bool IsBeforeFieldInit => false;

	public bool IsComObject => false;

	public bool IsGeneric => false;

	public bool IsInterface => false;

	public bool IsRuntimeSpecial => false;

	public bool IsSerializable => false;

	public bool IsSpecialName => false;

	public bool IsWindowsRuntimeImport => false;

	public bool IsSealed => true;

	public IEnumerable<SecurityAttribute> SecurityAttributes => SpecializedCollections.EmptyEnumerable<SecurityAttribute>();

	public CharSet StringFormat => CharSet.Ansi;

	public bool IsEnum => false;

	public Microsoft.Cci.PrimitiveTypeCode TypeCode => Microsoft.Cci.PrimitiveTypeCode.NotPrimitive;

	public TypeDefinitionHandle TypeDef
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public IGenericMethodParameterReference AsGenericMethodParameterReference => null;

	public IGenericTypeInstanceReference AsGenericTypeInstanceReference => null;

	public IGenericTypeParameterReference AsGenericTypeParameterReference => null;

	public virtual INamespaceTypeReference AsNamespaceTypeReference => null;

	public ISpecializedNestedTypeReference AsSpecializedNestedTypeReference => null;

	public virtual INestedTypeReference AsNestedTypeReference => null;

	public bool MangleName => false;

	public virtual ushort Alignment => 0;

	public virtual LayoutKind Layout => LayoutKind.Auto;

	public virtual uint SizeOf => 0u;

	public virtual bool IsValueType => false;

	public IEnumerable<IEventDefinition> GetEvents(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<IEventDefinition>();
	}

	public IEnumerable<Microsoft.Cci.MethodImplementation> GetExplicitImplementationOverrides(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<Microsoft.Cci.MethodImplementation>();
	}

	public virtual IEnumerable<IFieldDefinition> GetFields(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<IFieldDefinition>();
	}

	public IEnumerable<TypeReferenceWithAttributes> Interfaces(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<TypeReferenceWithAttributes>();
	}

	public virtual IEnumerable<IMethodDefinition> GetMethods(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<IMethodDefinition>();
	}

	public virtual IEnumerable<INestedTypeDefinition> GetNestedTypes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<INestedTypeDefinition>();
	}

	public IEnumerable<IPropertyDefinition> GetProperties(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<IPropertyDefinition>();
	}

	public virtual IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	public IDefinition AsDefinition(EmitContext context)
	{
		return this;
	}

	public ITypeDefinition GetResolvedType(EmitContext context)
	{
		return this;
	}

	public virtual INamespaceTypeDefinition AsNamespaceTypeDefinition(EmitContext context)
	{
		return null;
	}

	public virtual INestedTypeDefinition AsNestedTypeDefinition(EmitContext context)
	{
		return null;
	}

	public ITypeDefinition AsTypeDefinition(EmitContext context)
	{
		return this;
	}

	public virtual ITypeReference GetBaseClass(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public virtual void Dispatch(MetadataVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
