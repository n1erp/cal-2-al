using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundBadPropertyValue : BoundPropertyValue
{
	public BoundBadExpression BadPropertyValue { get; private set; }

	internal override object ValueObject => null;

	internal override string ValueText => string.Empty;

	public BoundBadPropertyValue(SyntaxNode syntax, BoundBadExpression badPropertyValue, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.BadPropertyValue, syntax, type, hasErrors || badPropertyValue.HasErrors())
	{
		BadPropertyValue = badPropertyValue;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBadPropertyValue(this);
	}

	public BoundBadPropertyValue Update(BoundBadExpression badPropertyValue, TypeSymbol type)
	{
		if (badPropertyValue != BadPropertyValue || type != Type)
		{
			return new BoundBadPropertyValue(Syntax, badPropertyValue, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
