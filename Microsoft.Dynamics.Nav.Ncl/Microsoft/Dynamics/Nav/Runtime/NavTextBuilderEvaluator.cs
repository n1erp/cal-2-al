using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTextBuilderEvaluator : NavValueEvaluator<NavTextBuilder>
{
	private static readonly NavTextBuilderEvaluator instance = new NavTextBuilderEvaluator();

	internal static NavTextBuilderEvaluator Instance { get; } = instance;

	private NavTextBuilderEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavTextBuilder value, INavValueMetadata metadata, string source, int number)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		value = new NavTextBuilder(source);
		return true;
	}

	protected override NavTextBuilder GetDefaultValue(INavValueMetadata metadata)
	{
		NavText navText = NavText.Default(metadata.NavDefinedLengthMetadata);
		return new NavTextBuilder(navText);
	}
}
