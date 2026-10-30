using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavEnumTextEncodingEvaluator : NavEnumTypeEvaluator<TextEncoding, NavEnumTextEncoding>
{
	private static readonly NavEnumTextEncodingEvaluator instance = new NavEnumTextEncodingEvaluator();

	internal static NavEnumTextEncodingEvaluator Instance => instance;

	private NavEnumTextEncodingEvaluator()
		: base((Func<TextEncoding, NavEnumTextEncoding>)((TextEncoding objectType) => new NavEnumTextEncoding(objectType)))
	{
	}

	protected override NavEnumTextEncoding GetDefaultValue(INavValueMetadata metadata)
	{
		return NavEnumTextEncoding.Default;
	}
}
