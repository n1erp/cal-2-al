using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPropertyEqualsValue : BoundEqualsValue
{
	public PropertySymbol Property { get; }

	public BoundPropertyEqualsValue(SyntaxNode syntax, PropertySymbol property, ImmutableArray<LocalSymbol> locals, BoundExpression value, bool hasErrors = false)
		: base(BoundKind.PropertyEqualsValue, syntax, locals, value, hasErrors || value.HasErrors())
	{
		Property = property;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPropertyEqualsValue(this);
	}

	public BoundPropertyEqualsValue Update(PropertySymbol property, ImmutableArray<LocalSymbol> locals, BoundExpression value)
	{
		if (property != Property || locals != Locals || value != Value)
		{
			return new BoundPropertyEqualsValue(Syntax, property, locals, value, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
