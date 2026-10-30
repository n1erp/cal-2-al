using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavCodeEvaluator : NavValueEvaluator<NavCode>
{
	private static readonly NavCodeEvaluator instance = new NavCodeEvaluator();

	internal static NavCodeEvaluator Instance => instance;

	private NavCodeEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavCode value, INavValueMetadata metadata, string source, int number)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		value = new NavCode(metadata.NavDefinedLengthMetadata, source);
		return true;
	}

	protected override NavCode GetDefaultValue(INavValueMetadata metadata)
	{
		return NavCode.Default(metadata.NavDefinedLengthMetadata);
	}
}
