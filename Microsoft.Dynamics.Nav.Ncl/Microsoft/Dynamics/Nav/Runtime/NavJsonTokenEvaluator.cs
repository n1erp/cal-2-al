using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavJsonTokenEvaluator<T> : NavValueEvaluator<T> where T : NavJsonToken
{
	internal static NavJsonTokenEvaluator<T> Instance { get; } = new NavJsonTokenEvaluator<T>();

	protected override bool InternalEvaluate(DataError dataError, out T value, INavValueMetadata metadata, string source, int number)
	{
		value = GetDefaultValue(metadata);
		return value.ALReadFrom(dataError, source);
	}
}
