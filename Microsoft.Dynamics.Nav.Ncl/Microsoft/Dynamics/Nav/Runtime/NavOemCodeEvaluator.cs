using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavOemCodeEvaluator : NavValueEvaluator<NavOemCode>
{
	private static readonly NavOemCodeEvaluator instance = new NavOemCodeEvaluator();

	internal static NavOemCodeEvaluator Instance => instance;

	private NavOemCodeEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavOemCode value, INavValueMetadata metadata, string source, int number)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		value = new NavOemCode(metadata.NavDefinedLengthMetadata, source);
		return true;
	}

	protected override NavOemCode GetDefaultValue(INavValueMetadata metadata)
	{
		return NavOemCode.Default(metadata.NavDefinedLengthMetadata);
	}
}
