using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundEnumPropertyValue : BoundPropertyValue
{
	public OptionSymbol Value { get; private set; }

	internal override object ValueObject => Value;

	internal override string ValueText => Value.Name;

	public BoundEnumPropertyValue(SyntaxNode syntax, OptionSymbol value, TypeSymbol type, bool hasErrors)
		: base(BoundKind.EnumPropertyValue, syntax, type, hasErrors)
	{
		Value = value;
	}

	public BoundEnumPropertyValue(SyntaxNode syntax, OptionSymbol value, TypeSymbol type)
		: base(BoundKind.EnumPropertyValue, syntax, type)
	{
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitEnumPropertyValue(this);
	}

	public BoundEnumPropertyValue Update(OptionSymbol value, TypeSymbol type)
	{
		if (value != Value || type != Type)
		{
			return new BoundEnumPropertyValue(Syntax, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
