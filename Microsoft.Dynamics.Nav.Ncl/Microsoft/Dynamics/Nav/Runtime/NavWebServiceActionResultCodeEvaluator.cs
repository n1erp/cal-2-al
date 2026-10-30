using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavWebServiceActionResultCodeEvaluator : NavEnumTypeEvaluator<WebServiceActionResultCode, NavWebServiceActionResultCode>
{
	private static readonly NavWebServiceActionResultCodeEvaluator instance = new NavWebServiceActionResultCodeEvaluator();

	internal static NavWebServiceActionResultCodeEvaluator Instance => instance;

	private NavWebServiceActionResultCodeEvaluator()
		: base((Func<WebServiceActionResultCode, NavWebServiceActionResultCode>)((WebServiceActionResultCode webServiceActionResultCode) => new NavWebServiceActionResultCode(webServiceActionResultCode)))
	{
	}

	protected override NavWebServiceActionResultCode GetDefaultValue(INavValueMetadata metadata)
	{
		return NavWebServiceActionResultCode.Default;
	}
}
