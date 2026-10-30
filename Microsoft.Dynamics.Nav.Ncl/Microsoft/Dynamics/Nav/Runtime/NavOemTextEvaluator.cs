using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavOemTextEvaluator : NavValueEvaluator<NavOemText>
{
	private static readonly NavOemTextEvaluator instance = new NavOemTextEvaluator();

	internal static NavOemTextEvaluator Instance => instance;

	private NavOemTextEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavOemText value, INavValueMetadata metadata, string source, int number)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		value = new NavOemText(metadata.NavDefinedLengthMetadata, source);
		return true;
	}

	protected override NavOemText GetDefaultValue(INavValueMetadata metadata)
	{
		return NavOemText.Default(metadata.NavDefinedLengthMetadata);
	}
}
