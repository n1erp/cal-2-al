using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundParameterEqualsValue : BoundEqualsValue
{
	public ParameterSymbol Parameter { get; }

	public BoundParameterEqualsValue(SyntaxNode syntax, ParameterSymbol parameter, ImmutableArray<LocalSymbol> locals, BoundExpression value, bool hasErrors = false)
		: base(BoundKind.ParameterEqualsValue, syntax, locals, value, hasErrors || value.HasErrors())
	{
		Parameter = parameter;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitParameterEqualsValue(this);
	}

	public BoundParameterEqualsValue Update(ParameterSymbol parameter, ImmutableArray<LocalSymbol> locals, BoundExpression value)
	{
		if (parameter != Parameter || locals != Locals || value != Value)
		{
			return new BoundParameterEqualsValue(Syntax, parameter, locals, value, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
