using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public class RecordFieldValueBinder : ValueBinder
{
	private static readonly MethodInfo getFieldValueMethodInfo = typeof(NavRecord).GetMethod("GetFieldValue", new Type[1] { typeof(NCLMetaField) });

	private static readonly FieldInfo metaFieldFieldInfo = typeof(RecordFieldValueBinder).GetField("metaField", BindingFlags.Instance | BindingFlags.NonPublic);

	private readonly NCLMetaField metaField;

	public RecordFieldValueBinder(NCLMetaField metaField)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		this.metaField = metaField;
	}

	public override object GetValue(object parent)
	{
		return ((NavRecord)parent).GetFieldValue(metaField);
	}

	public override bool TrySetValue(object parentInstance, object newChildValue)
	{
		try
		{
			((NavRecord)parentInstance).SetFieldValue(metaField, (NavValue)newChildValue);
			return true;
		}
		catch (NavNCLNotSupportedTypeException)
		{
			return false;
		}
	}

	public override Expression BuildGetValueExpression(Expression input)
	{
		if (!typeof(NavRecord).IsAssignableFrom(input.Type))
		{
			input = Expression.Convert(input, typeof(NavRecord));
		}
		return Expression.Convert(Expression.Call(input, getFieldValueMethodInfo, Expression.Field(Expression.Constant(this), metaFieldFieldInfo)), metaField.EmptyValue.GetType());
	}
}
