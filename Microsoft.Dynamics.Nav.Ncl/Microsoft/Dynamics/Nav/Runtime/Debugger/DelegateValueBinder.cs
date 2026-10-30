using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class DelegateValueBinder : ValueBinder
{
	private static readonly FieldInfo funcFieldInfo = typeof(DelegateValueBinder).GetField("getFunc", BindingFlags.Instance | BindingFlags.NonPublic);

	private readonly Func<object, object> getFunc;

	private readonly Func<object, object, bool> trySetFunc;

	private Type expressionType;

	public DelegateValueBinder(Func<object, object> getFunc, Type expressionOutputType)
		: this(getFunc, null, expressionOutputType)
	{
	}

	public DelegateValueBinder(Func<object, object> getFunc, Func<object, object, bool> trySetFunc, Type expressionOutputType)
	{
		if (getFunc == null)
		{
			throw new ArgumentNullException("getFunc");
		}
		this.getFunc = getFunc;
		this.trySetFunc = trySetFunc;
		expressionType = expressionOutputType;
	}

	public override object GetValue(object parent)
	{
		return getFunc(parent);
	}

	public override bool TrySetValue(object parentInstance, object newChildValue)
	{
		if (trySetFunc != null)
		{
			return trySetFunc(parentInstance, newChildValue);
		}
		return false;
	}

	public override Expression BuildGetValueExpression(Expression input)
	{
		Expression expression = ((getFunc.Target != null) ? ((Expression)Expression.Invoke(Expression.Field(Expression.Constant(this), funcFieldInfo), input)) : ((Expression)Expression.Call(getFunc.Method, input)));
		return Expression.Convert(expression, expressionType);
	}
}
