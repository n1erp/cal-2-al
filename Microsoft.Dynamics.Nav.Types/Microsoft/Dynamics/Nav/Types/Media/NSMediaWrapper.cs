using System;

namespace Microsoft.Dynamics.Nav.Types.Media;

public class NSMediaWrapper : NSMediaWrapperBase
{
	public NSMediaWrapper(byte[] data)
		: base(data)
	{
	}

	public NSMediaWrapper(object data)
		: base(data)
	{
	}

	public NSMediaWrapper(NavMediaDescriptionCollection data)
		: base(data)
	{
	}

	public NSMediaWrapper(Guid data)
		: base(data)
	{
	}
}
