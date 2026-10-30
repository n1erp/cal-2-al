using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundConstantValueListPropertyValue : BoundPropertyValue
{
	public ImmutableArray<ConstantValue> Values { get; private set; }

	internal override object ValueObject => Values;

	internal override string ValueText => Syntax.ToString();

	public BoundConstantValueListPropertyValue(SyntaxNode syntax, ImmutableArray<ConstantValue> values, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ConstantValueListPropertyValue, syntax, type, hasErrors)
	{
		Values = values;
	}

	public BoundConstantValueListPropertyValue(SyntaxNode syntax, ImmutableArray<ConstantValue> values, TypeSymbol type)
		: base(BoundKind.ConstantValueListPropertyValue, syntax, type)
	{
		Values = values;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConstantValueListPropertyValue(this);
	}

	public BoundConstantValueListPropertyValue Update(ImmutableArray<ConstantValue> values, TypeSymbol type)
	{
		if (values != Values || type != Type)
		{
			return new BoundConstantValueListPropertyValue(Syntax, values, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
