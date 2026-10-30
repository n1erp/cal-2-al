using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundLiteralPropertyValue : BoundPropertyValue
{
	public ConstantValue Value { get; private set; }

	public override ConstantValue ConstantValue => Value;

	internal override object ValueObject => Value.Value;

	internal override string ValueText
	{
		get
		{
			if (Value.IsBoolean)
			{
				if (!Value.BooleanValue)
				{
					return "0";
				}
				return "1";
			}
			return Value.NavTypeKind switch
			{
				NavTypeKind.Decimal => Value.DecimalValue.ToString(CultureInfo.InvariantCulture), 
				NavTypeKind.Integer => Value.Int32Value.ToString(CultureInfo.InvariantCulture), 
				NavTypeKind.BigInteger => Value.Int64Value.ToString(CultureInfo.InvariantCulture), 
				NavTypeKind.Time => Value.TimeValue.ToString(CultureInfo.InvariantCulture), 
				NavTypeKind.Date => ObjectDisplay.FormatDate(Value.DateValue), 
				NavTypeKind.DateTime => Value.DateTimeValue.ToString(CultureInfo.InvariantCulture), 
				_ => Value.StringValue, 
			};
		}
	}

	public BoundLiteralPropertyValue(SyntaxNode syntax, ConstantValue value, TypeSymbol type, bool hasErrors)
		: base(BoundKind.LiteralPropertyValue, syntax, type, hasErrors)
	{
		Value = value;
	}

	public BoundLiteralPropertyValue(SyntaxNode syntax, ConstantValue value, TypeSymbol type)
		: base(BoundKind.LiteralPropertyValue, syntax, type)
	{
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLiteralPropertyValue(this);
	}

	public BoundLiteralPropertyValue Update(ConstantValue value, TypeSymbol type)
	{
		if (value != Value || type != Type)
		{
			return new BoundLiteralPropertyValue(Syntax, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
