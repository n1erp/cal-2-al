using System.Linq.Expressions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public abstract class ValueBinder
{
	public abstract object GetValue(object parent);

	public abstract Expression BuildGetValueExpression(Expression input);

	public virtual bool TrySetValue(object parentInstance, object newChildValue)
	{
		return false;
	}
}
