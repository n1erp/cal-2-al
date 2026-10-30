using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundLabelPropertyValue : BoundPropertyValue
{
	public BoundLabel BoundLabel { get; private set; }

	internal override object ValueObject => Syntax.ToString();

	internal override string ValueText => BoundLabel.Text.StringValue;

	public BoundLabelPropertyValue(SyntaxNode syntax, BoundLabel boundLabel, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.LabelPropertyValue, syntax, type, hasErrors || boundLabel.HasErrors())
	{
		BoundLabel = boundLabel;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLabelPropertyValue(this);
	}

	public BoundLabelPropertyValue Update(BoundLabel boundLabel, TypeSymbol type)
	{
		if (boundLabel != BoundLabel || type != Type)
		{
			return new BoundLabelPropertyValue(Syntax, boundLabel, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
