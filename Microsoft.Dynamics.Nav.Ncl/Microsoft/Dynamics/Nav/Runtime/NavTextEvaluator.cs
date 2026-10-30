using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTextEvaluator : NavValueEvaluator<NavText>
{
	private static readonly NavTextEvaluator instance = new NavTextEvaluator();

	internal static NavTextEvaluator Instance => instance;

	private NavTextEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavText value, INavValueMetadata metadata, string source, int number)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		value = new NavText(metadata.NavDefinedLengthMetadata, source);
		return true;
	}

	protected override NavText GetDefaultValue(INavValueMetadata metadata)
	{
		return NavText.Default(metadata.NavDefinedLengthMetadata);
	}
}
