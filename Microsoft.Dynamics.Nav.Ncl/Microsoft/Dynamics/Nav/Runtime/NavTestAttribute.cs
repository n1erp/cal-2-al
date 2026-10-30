using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public sealed class NavTestAttribute : Attribute
{
	public string Handlers { get; set; }

	public string CalName { get; private set; }

	public int TestMethodNo { get; set; }

	public TestTransactionModel TransactionModel { get; set; }

	public NavTestPermissions TestPermissions { get; set; }

	public NavTestAttribute(string calName)
	{
		CalName = calName;
	}
}
