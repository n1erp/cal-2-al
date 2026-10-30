using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundDataItemLinkPropertyValue : BoundPropertyValue
{
	public ImmutableArray<BoundDataItemLinkExpression> Joins { get; private set; }

	internal override object ValueObject => Syntax.ToString();

	internal override string ValueText => Syntax.ToString();

	public BoundDataItemLinkPropertyValue(SyntaxNode syntax, ImmutableArray<BoundDataItemLinkExpression> joins, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.DataItemLinkPropertyValue, syntax, type, hasErrors || joins.HasErrors())
	{
		Joins = joins;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDataItemLinkPropertyValue(this);
	}

	public BoundDataItemLinkPropertyValue Update(ImmutableArray<BoundDataItemLinkExpression> joins, TypeSymbol type)
	{
		if (joins != Joins || type != Type)
		{
			return new BoundDataItemLinkPropertyValue(Syntax, joins, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
