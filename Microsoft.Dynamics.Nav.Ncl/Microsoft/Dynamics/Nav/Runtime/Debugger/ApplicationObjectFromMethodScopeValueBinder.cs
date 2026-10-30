using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class ApplicationObjectFromMethodScopeValueBinder : ValueBinder
{
	private static readonly PropertyInfo applicationObjectPropertyInfo = typeof(NavMethodScope).GetProperty("ApplicationObject");

	public override object GetValue(object parent)
	{
		return ((NavMethodScope)parent).ApplicationObject;
	}

	public override Expression BuildGetValueExpression(Expression input)
	{
		if (!typeof(NavMethodScope).IsAssignableFrom(input.Type))
		{
			input = Expression.Convert(input, typeof(NavMethodScope));
		}
		return Expression.Property(input, applicationObjectPropertyInfo);
	}
}
