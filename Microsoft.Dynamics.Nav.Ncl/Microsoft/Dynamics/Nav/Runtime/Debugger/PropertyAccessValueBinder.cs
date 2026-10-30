using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class PropertyAccessValueBinder<T> : ValueBinder
{
	private readonly Func<T, object> func;

	private readonly string propertyName;

	private readonly bool isSetSupported;

	public PropertyAccessValueBinder(Func<T, object> func, string propertyName)
		: this(func, propertyName, false)
	{
	}

	public PropertyAccessValueBinder(Func<T, object> func, string propertyName, bool isSetSupported)
	{
		this.func = func;
		this.propertyName = propertyName;
		this.isSetSupported = isSetSupported;
	}

	public override object GetValue(object parent)
	{
		return func((T)parent);
	}

	public override Expression BuildGetValueExpression(Expression input)
	{
		if (!typeof(T).IsAssignableFrom(input.Type))
		{
			input = Expression.Convert(input, typeof(T));
		}
		return Expression.Property(input, propertyName);
	}

	public override bool TrySetValue(object parentInstance, object newChildValue)
	{
		if (isSetSupported)
		{
			PropertyInfo property = parentInstance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
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
			}
		}
		return false;
	}
}
