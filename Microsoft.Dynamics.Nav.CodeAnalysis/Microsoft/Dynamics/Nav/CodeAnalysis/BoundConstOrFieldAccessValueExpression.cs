using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundConstOrFieldAccessValueExpression : BoundPropertyValueExpression
{
	public BoundExpression Value { get; private set; }

	public BoundConstOrFieldAccessValueExpression(SyntaxNode syntax, BoundExpression value, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ConstOrFieldAccessValueExpression, syntax, type, hasErrors || value.HasErrors())
	{
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConstOrFieldAccessValueExpression(this);
	}

	public BoundConstOrFieldAccessValueExpression Update(BoundExpression value, TypeSymbol type)
	{
		if (value != Value || type != Type)
		{
			return new BoundConstOrFieldAccessValueExpression(Syntax, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
