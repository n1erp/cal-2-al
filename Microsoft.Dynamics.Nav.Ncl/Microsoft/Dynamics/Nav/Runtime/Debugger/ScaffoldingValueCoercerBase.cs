using System.Linq.Expressions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public abstract class ScaffoldingValueCoercerBase
{
	public abstract object CoerceValue(object input);

	public abstract bool IsCompatible(object input);

	public abstract Expression BuildCoerceValueExpression(Expression input);

	public virtual bool TryReverseCoerce(object outerValue, object innerValue)
	{
		return false;
	}
}
