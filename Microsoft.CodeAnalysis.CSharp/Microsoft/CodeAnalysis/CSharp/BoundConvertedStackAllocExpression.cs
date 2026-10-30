using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundConvertedStackAllocExpression : BoundStackAllocArrayCreation
{
	protected override ImmutableArray<BoundNode> Children => StaticCast<BoundNode>.From(BoundStackAllocArrayCreation.GetChildInitializers(InitializerOpt).Insert(0, Count));

	public BoundConvertedStackAllocExpression(SyntaxNode syntax, TypeSymbol elementType, BoundExpression count, BoundArrayInitialization initializerOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ConvertedStackAllocExpression, syntax, elementType, count, initializerOpt, type, hasErrors || count.HasErrors() || initializerOpt.HasErrors())
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConvertedStackAllocExpression(this);
	}

	public new BoundConvertedStackAllocExpression Update(TypeSymbol elementType, BoundExpression count, BoundArrayInitialization initializerOpt, TypeSymbol type)
	{
		if (elementType != ElementType || count != Count || initializerOpt != InitializerOpt || type != Type)
		{
			return new BoundConvertedStackAllocExpression(Syntax, elementType, count, initializerOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
