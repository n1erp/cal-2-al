using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Types;

internal static class PropertyHelpers
{
	public static Action<T, object> BuildUntypedSetter<T>(this PropertyInfo propertyInfo)
	{
		Type declaringType = propertyInfo.DeclaringType;
		MethodInfo setMethod = propertyInfo.GetSetMethod();
		ParameterExpression parameterExpression = Expression.Parameter(declaringType, "t");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "v");
		return Expression.Lambda<Action<T, object>>(Expression.Call(parameterExpression, setMethod, Expression.Convert(parameterExpression2, propertyInfo.PropertyType)), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
	}

	public static Func<T, object> BuildUntypedGetter<T>(this PropertyInfo propertyInfo)
	{
		Type declaringType = propertyInfo.DeclaringType;
		MethodInfo getMethod = propertyInfo.GetGetMethod();
		ParameterExpression parameterExpression = Expression.Parameter(declaringType, "t");
		return Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Call(parameterExpression, getMethod), typeof(object)), new ParameterExpression[1] { parameterExpression }).Compile();
	}
}
