using System;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types;

public abstract class TypesThreadLocalStorage
{
	[ThreadStatic]
	protected static TypesThreadLocalStorage typesInstance;

	private OperationContext operationContext;

	public OperationContext OperationContext
	{
		get
		{
			return operationContext ?? OperationContext.Current;
		}
		set
		{
			operationContext = value;
		}
	}

	public Guid? ClientSessionId { get; set; }

	public Guid? ClientActivityId { get; set; }

	public NavSqlDebuggingStatistics SqlDebuggingStatics { get; set; }

	public static TypesThreadLocalStorage Current => typesInstance;
}
