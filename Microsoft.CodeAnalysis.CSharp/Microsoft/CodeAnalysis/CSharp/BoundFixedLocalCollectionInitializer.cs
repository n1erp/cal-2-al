using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundFixedLocalCollectionInitializer : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Expression);

	public TypeSymbol ElementPointerType { get; }

	public Conversion ElementPointerTypeConversion { get; }

	public BoundExpression Expression { get; }

	public MethodSymbol GetPinnableOpt { get; }

	public BoundFixedLocalCollectionInitializer(SyntaxNode syntax, TypeSymbol elementPointerType, Conversion elementPointerTypeConversion, BoundExpression expression, MethodSymbol getPinnableOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.FixedLocalCollectionInitializer, syntax, type, hasErrors || expression.HasErrors())
	{
		ElementPointerType = elementPointerType;
		ElementPointerTypeConversion = elementPointerTypeConversion;
		Expression = expression;
		GetPinnableOpt = getPinnableOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitFixedLocalCollectionInitializer(this);
	}

	public BoundFixedLocalCollectionInitializer Update(TypeSymbol elementPointerType, Conversion elementPointerTypeConversion, BoundExpression expression, MethodSymbol getPinnableOpt, TypeSymbol type)
	{
		if (elementPointerType != ElementPointerType || elementPointerTypeConversion != ElementPointerTypeConversion || expression != Expression || getPinnableOpt != GetPinnableOpt || type != Type)
		{
			return new BoundFixedLocalCollectionInitializer(Syntax, elementPointerType, elementPointerTypeConversion, expression, getPinnableOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
