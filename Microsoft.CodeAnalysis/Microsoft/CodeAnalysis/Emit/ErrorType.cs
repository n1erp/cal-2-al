using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

internal class ErrorType : INamespaceTypeReference, INamedTypeReference, ITypeReference, IReference, INamedEntity
{
	private sealed class ErrorAssembly : IAssemblyReference, IModuleReference, IUnitReference, IReference, INamedEntity
	{
		public static readonly ErrorAssembly Singleton = new ErrorAssembly();

		private static readonly AssemblyIdentity s_identity = new AssemblyIdentity("Error" + Guid.NewGuid().ToString("B"), AssemblyIdentity.NullVersion, "", ImmutableArray<byte>.Empty);

		AssemblyIdentity IAssemblyReference.Identity => s_identity;

		Version IAssemblyReference.AssemblyVersionPattern => null;

		string INamedEntity.Name => s_identity.Name;

		IAssemblyReference IModuleReference.GetContainingAssembly(EmitContext context)
		{
			return this;
		}

		IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
		{
			return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit(this);
		}

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			return null;
		}
	}

	public static readonly ErrorType Singleton = new ErrorType();

	private static readonly string s_name = "Error" + Guid.NewGuid().ToString("B");

	string INamespaceTypeReference.NamespaceName => "";

	ushort INamedTypeReference.GenericParameterCount => 0;

	bool INamedTypeReference.MangleName => false;

	bool ITypeReference.IsEnum => false;

	bool ITypeReference.IsValueType => false;

	Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode => Microsoft.Cci.PrimitiveTypeCode.NotPrimitive;

	TypeDefinitionHandle ITypeReference.TypeDef => default;

	IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference => null;

	IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference => null;

	IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference => null;

	INamespaceTypeReference ITypeReference.AsNamespaceTypeReference => this;

	INestedTypeReference ITypeReference.AsNestedTypeReference => null;

	ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference => null;

	string INamedEntity.Name => s_name;

	IUnitReference INamespaceTypeReference.GetUnit(EmitContext context)
	{
		return ErrorAssembly.Singleton;
	}

	ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
	{
		return null;
	}

	INamespaceTypeDefinition ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
	{
		return null;
	}

	INestedTypeDefinition ITypeReference.AsNestedTypeDefinition(EmitContext context)
	{
		return null;
	}

	ITypeDefinition ITypeReference.AsTypeDefinition(EmitContext context)
	{
		return null;
	}

	IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return null;
	}
}
