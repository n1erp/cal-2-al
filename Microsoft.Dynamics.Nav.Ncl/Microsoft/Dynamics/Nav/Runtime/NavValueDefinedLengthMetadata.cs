using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavValueDefinedLengthMetadata : NavValueMetadata
{
	private int definedLength;

	public override int NavDefinedLengthMetadata => definedLength;

	public NavValueDefinedLengthMetadata(NavNclType nclType, int definedLength)
		: base(nclType)
	{
		this.definedLength = definedLength;
	}

	public NavValueDefinedLengthMetadata(NavType navType, int definedLength)
		: this(NavNclTypeHelper.GetNclTypeFromNavType(navType), definedLength)
	{
		this.definedLength = definedLength;
	}
}
