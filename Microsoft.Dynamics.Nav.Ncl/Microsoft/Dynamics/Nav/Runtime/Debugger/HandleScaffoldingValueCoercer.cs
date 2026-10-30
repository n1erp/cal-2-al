using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public class HandleScaffoldingValueCoercer : ScaffoldingValueCoercerBase
{
	private readonly NavNclType nclType;

	public HandleScaffoldingValueCoercer(NavNclType type)
	{
		nclType = type;
	}

	public override object CoerceValue(object input)
	{
		if (!(input is INavApplicationObjectBaseHandle navApplicationObjectBaseHandle))
		{
			return null;
		}
		if (!navApplicationObjectBaseHandle.HasTarget)
		{
			return null;
		}
		return navApplicationObjectBaseHandle.Target;
	}

	public override bool IsCompatible(object input)
	{
		if (input is INavApplicationObjectBaseHandle navApplicationObjectBaseHandle)
		{
			return navApplicationObjectBaseHandle.HasTarget;
		}
		return false;
	}

	public override Expression BuildCoerceValueExpression(Expression input)
	{
		Type type = nclType switch
		{
			NavNclType.NavRecordReference => typeof(INavRecordHandle), 
			NavNclType.NavCodeunitReference => typeof(NavCodeunitHandle), 
			NavNclType.NavFormReference => typeof(NavFormHandle), 
			NavNclType.NavXmlPortReference => typeof(NavXmlPortHandle), 
			NavNclType.NavReportReference => typeof(NavXmlPortHandle), 
			NavNclType.NavTestPageReference => typeof(NavTestPageHandle), 
			_ => typeof(INavApplicationObjectBaseHandle), 
		};
		PropertyInfo property = type.GetProperty("Target");
		if (!type.IsAssignableFrom(input.Type))
		{
			input = Expression.Convert(input, type);
		}
		return Expression.Property(input, property);
	}
}
