using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundIdentifierNamePropertyValue : BoundPropertyValue
{
	public ConstantValue Value { get; private set; }

	internal override object ValueObject => ValueText;

	internal override string ValueText => Value.StringValue;

	public BoundIdentifierNamePropertyValue(SyntaxNode syntax, ConstantValue value, TypeSymbol type, bool hasErrors)
		: base(BoundKind.IdentifierNamePropertyValue, syntax, type, hasErrors)
	{
		Value = value;
	}

	public BoundIdentifierNamePropertyValue(SyntaxNode syntax, ConstantValue value, TypeSymbol type)
		: base(BoundKind.IdentifierNamePropertyValue, syntax, type)
	{
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitIdentifierNamePropertyValue(this);
	}

	public BoundIdentifierNamePropertyValue Update(ConstantValue value, TypeSymbol type)
	{
		if (value != Value || type != Type)
		{
			return new BoundIdentifierNamePropertyValue(Syntax, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
