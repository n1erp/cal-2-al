using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal abstract class GenericTypeInstanceReference : NamedTypeReference, IGenericTypeInstanceReference, ITypeReference, IReference
{
	public GenericTypeInstanceReference(NamedTypeSymbol underlyingNamedType)
		: base(underlyingNamedType)
	{
	}

	public sealed override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	ImmutableArray<ITypeReference> IGenericTypeInstanceReference.GetGenericArguments(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		ArrayBuilder<ITypeReference> instance = ArrayBuilder<ITypeReference>.GetInstance();
		foreach (TypeSymbol typeArgumentsNoUseSiteDiagnostic in UnderlyingNamedType.TypeArgumentsNoUseSiteDiagnostics)
		{
			instance.Add(pEModuleBuilder.Translate(typeArgumentsNoUseSiteDiagnostic, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics));
		}
		return instance.ToImmutableAndFree();
	}

	INamedTypeReference IGenericTypeInstanceReference.GetGenericType(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(UnderlyingNamedType.OriginalDefinition, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics, fromImplements: false, needDeclaration: true);
	}
}
