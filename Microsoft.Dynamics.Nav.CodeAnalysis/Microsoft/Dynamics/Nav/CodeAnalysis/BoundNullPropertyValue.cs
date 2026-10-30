using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundNullPropertyValue : BoundPropertyValueExpression
{
	public BoundNullPropertyValue(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.NullPropertyValue, syntax, type, hasErrors)
	{
	}

	public BoundNullPropertyValue(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.NullPropertyValue, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNullPropertyValue(this);
	}

	public BoundNullPropertyValue Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundNullPropertyValue(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
