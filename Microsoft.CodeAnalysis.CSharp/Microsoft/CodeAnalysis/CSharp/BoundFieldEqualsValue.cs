using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundFieldEqualsValue : BoundEqualsValue
{
	public FieldSymbol Field { get; }

	public BoundFieldEqualsValue(SyntaxNode syntax, FieldSymbol field, ImmutableArray<LocalSymbol> locals, BoundExpression value, bool hasErrors = false)
		: base(BoundKind.FieldEqualsValue, syntax, locals, value, hasErrors || value.HasErrors())
	{
		Field = field;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitFieldEqualsValue(this);
	}

	public BoundFieldEqualsValue Update(FieldSymbol field, ImmutableArray<LocalSymbol> locals, BoundExpression value)
	{
		if (field != Field || locals != Locals || value != Value)
		{
			return new BoundFieldEqualsValue(Syntax, field, locals, value, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
