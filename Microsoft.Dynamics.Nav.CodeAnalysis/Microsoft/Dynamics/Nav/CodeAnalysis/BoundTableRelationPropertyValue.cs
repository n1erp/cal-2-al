using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTableRelationPropertyValue : BoundPropertyValue
{
	public BoundTableRelationExpression TableRelationExpression { get; private set; }

	internal override object ValueObject => null;

	internal override string ValueText => string.Empty;

	public BoundTableRelationPropertyValue(SyntaxNode syntax, BoundTableRelationExpression tableRelationExpression, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TableRelationPropertyValue, syntax, type, hasErrors || tableRelationExpression.HasErrors())
	{
		TableRelationExpression = tableRelationExpression;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTableRelationPropertyValue(this);
	}

	public BoundTableRelationPropertyValue Update(BoundTableRelationExpression tableRelationExpression, TypeSymbol type)
	{
		if (tableRelationExpression != TableRelationExpression || type != Type)
		{
			return new BoundTableRelationPropertyValue(Syntax, tableRelationExpression, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
