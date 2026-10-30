using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal class SpecializedMethodReference : MethodReference, ISpecializedMethodReference, IMethodReference, ISignature, ITypeMemberReference, IReference, INamedEntity
{
	IMethodReference ISpecializedMethodReference.UnspecializedVersion => UnderlyingMethod.OriginalDefinition;

	public override ISpecializedMethodReference AsSpecializedMethodReference => this;

	public SpecializedMethodReference(MethodSymbol underlyingMethod)
		: base(underlyingMethod)
	{
	}

	public override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}
}
