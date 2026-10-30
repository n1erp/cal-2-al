using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundParameter : BoundExpression
{
	public override Symbol ExpressionSymbol => ParameterSymbol;

	public ParameterSymbol ParameterSymbol { get; }

	public BoundParameter(SyntaxNode syntax, ParameterSymbol parameterSymbol, bool hasErrors = false)
		: this(syntax, parameterSymbol, parameterSymbol.Type, hasErrors)
	{
	}

	public BoundParameter(SyntaxNode syntax, ParameterSymbol parameterSymbol)
		: this(syntax, parameterSymbol, parameterSymbol.Type)
	{
	}

	public BoundParameter(SyntaxNode syntax, ParameterSymbol parameterSymbol, TypeSymbol type, bool hasErrors)
		: base(BoundKind.Parameter, syntax, type, hasErrors)
	{
		ParameterSymbol = parameterSymbol;
	}

	public BoundParameter(SyntaxNode syntax, ParameterSymbol parameterSymbol, TypeSymbol type)
		: base(BoundKind.Parameter, syntax, type)
	{
		ParameterSymbol = parameterSymbol;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitParameter(this);
	}

	public BoundParameter Update(ParameterSymbol parameterSymbol, TypeSymbol type)
	{
		if (parameterSymbol != ParameterSymbol || type != Type)
		{
			return new BoundParameter(Syntax, parameterSymbol, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
