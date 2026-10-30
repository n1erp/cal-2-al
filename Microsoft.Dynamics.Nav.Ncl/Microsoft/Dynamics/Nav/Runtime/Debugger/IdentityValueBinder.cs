using System.Linq.Expressions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public class IdentityValueBinder : ValueBinder
{
	public override object GetValue(object parent)
	{
		return parent;
	}

	public override Expression BuildGetValueExpression(Expression input)
	{
		return input;
	}
}
