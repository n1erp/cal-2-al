using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class PropertyInfoValueBinder : ValueBinder
{
	private readonly PropertyInfo property;

	public PropertyInfoValueBinder(PropertyInfo property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		this.property = property;
	}

	public override object GetValue(object parent)
	{
		try
		{
			return property.GetValue(parent, null);
		}
		catch (TargetInvocationException)
		{
		}
		return null;
	}

	public override bool TrySetValue(object parentInstance, object newChildValue)
	{
		try
		{
			property.SetValue(parentInstance, newChildValue, null);
			return true;
		}
		catch (TargetInvocationException)
		{
		}
		catch (ArgumentException)
		{
		}
		return false;
	}

	public override Expression BuildGetValueExpression(Expression input)
	{
		if (!property.DeclaringType.IsAssignableFrom(input.Type))
		{
			input = Expression.Convert(input, property.DeclaringType);
		}
		return Expression.Property(input, property);
	}
}
