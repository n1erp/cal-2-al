using System;
using System.Linq.Expressions;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public class ByRefScaffoldingValueCoercer : ScaffoldingValueCoercerBase
{
	private readonly Type byRefType;

	public ByRefScaffoldingValueCoercer(Type byRefType)
	{
		this.byRefType = byRefType;
	}

	public override object CoerceValue(object input)
	{
		try
		{
			return (input == null) ? null : ((IByRef)input).ObjectValue;
		}
		catch (NavNCLException)
		{
			return null;
		}
	}

	public override bool TryReverseCoerce(object outerValue, object innerValue)
	{
		IByRef byRef = (IByRef)outerValue;
		try
		{
			byRef.ObjectValue = innerValue;
			return true;
		}
		catch (InvalidCastException)
		{
			return false;
		}
	}

	public override bool IsCompatible(object input)
	{
		IByRef byRef = input as IByRef;
		return byRef != null;
	}

	public override Expression BuildCoerceValueExpression(Expression input)
	{
		if (!byRefType.IsAssignableFrom(input.Type))
		{
			input = Expression.Convert(input, byRefType);
		}
		return Expression.Property(input, "Value");
	}
}
