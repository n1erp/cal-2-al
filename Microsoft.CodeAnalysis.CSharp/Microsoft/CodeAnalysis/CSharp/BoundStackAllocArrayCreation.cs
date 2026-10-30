using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal class BoundStackAllocArrayCreation : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => StaticCast<BoundNode>.From(GetChildInitializers(InitializerOpt).Insert(0, Count));

	public override object Display => (FormattableString)$"stackalloc {ElementType}[{(Count.WasCompilerGenerated ? null : Count.Syntax.ToString())}]";

	public TypeSymbol ElementType { get; }

	public BoundExpression Count { get; }

	public BoundArrayInitialization InitializerOpt { get; }

	internal static ImmutableArray<BoundExpression> GetChildInitializers(BoundArrayInitialization arrayInitializer)
	{
		return arrayInitializer?.Initializers ?? ImmutableArray<BoundExpression>.Empty;
	}

	protected BoundStackAllocArrayCreation(BoundKind kind, SyntaxNode syntax, TypeSymbol elementType, BoundExpression count, BoundArrayInitialization initializerOpt, TypeSymbol type, bool hasErrors = false)
		: base(kind, syntax, type, hasErrors)
	{
		ElementType = elementType;
		Count = count;
		InitializerOpt = initializerOpt;
	}

	public BoundStackAllocArrayCreation(SyntaxNode syntax, TypeSymbol elementType, BoundExpression count, BoundArrayInitialization initializerOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.StackAllocArrayCreation, syntax, type, hasErrors || count.HasErrors() || initializerOpt.HasErrors())
	{
		ElementType = elementType;
		Count = count;
		InitializerOpt = initializerOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitStackAllocArrayCreation(this);
	}

	public BoundStackAllocArrayCreation Update(TypeSymbol elementType, BoundExpression count, BoundArrayInitialization initializerOpt, TypeSymbol type)
	{
		if (elementType != ElementType || count != Count || initializerOpt != InitializerOpt || type != Type)
		{
			return new BoundStackAllocArrayCreation(Syntax, elementType, count, initializerOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
