using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundWildCardPropertyValueExpression : BoundPropertyValueExpression
{
	public BoundLiteral Pattern { get; private set; }

	public BoundWildCardPropertyValueExpression(SyntaxNode syntax, BoundLiteral pattern, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.WildCardPropertyValueExpression, syntax, type, hasErrors || pattern.HasErrors())
	{
		Pattern = pattern;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitWildCardPropertyValueExpression(this);
	}

	public BoundWildCardPropertyValueExpression Update(BoundLiteral pattern, TypeSymbol type)
	{
		if (pattern != Pattern || type != Type)
		{
			return new BoundWildCardPropertyValueExpression(Syntax, pattern, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
