using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class FieldInfoValueBinder : ValueBinder
{
	private readonly FieldInfo field;

	public FieldInfoValueBinder(FieldInfo field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		this.field = field;
	}

	public override object GetValue(object parent)
	{
		try
		{
			return field.GetValue(parent);
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
			field.SetValue(parentInstance, newChildValue);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
	}

	public override Expression BuildGetValueExpression(Expression input)
	{
		if (!field.DeclaringType.IsAssignableFrom(input.Type))
		{
			input = Expression.Convert(input, field.DeclaringType);
		}
		return Expression.Field(input, field);
	}
}
