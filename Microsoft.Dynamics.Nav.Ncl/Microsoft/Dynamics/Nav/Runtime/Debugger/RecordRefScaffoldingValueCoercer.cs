using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public class RecordRefScaffoldingValueCoercer : ScaffoldingValueCoercerBase
{
	private static readonly PropertyInfo recordProperty = typeof(NavRecordRef).GetProperty("Record");

	public override object CoerceValue(object input)
	{
		if (input != null)
		{
			return ((NavRecordRef)input).Record;
		}
		return null;
	}

	public override bool IsCompatible(object input)
	{
		if (input is NavRecordRef navRecordRef)
		{
			return navRecordRef.Record != null;
		}
		return false;
	}

	public override Expression BuildCoerceValueExpression(Expression input)
	{
		if (!typeof(NavRecordRef).IsAssignableFrom(input.Type))
		{
			input = Expression.Convert(input, typeof(NavRecordRef));
		}
		return Expression.Property(input, recordProperty);
	}
}
