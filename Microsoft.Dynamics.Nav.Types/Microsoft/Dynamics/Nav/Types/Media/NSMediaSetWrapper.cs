using System;

namespace Microsoft.Dynamics.Nav.Types.Media;

public class NSMediaSetWrapper : NSMediaWrapperBase
{
	public NSMediaSetWrapper(byte[] data)
		: base(data)
	{
	}

	public NSMediaSetWrapper(object data)
		: base(data)
	{
	}

	public NSMediaSetWrapper(NavMediaDescriptionCollection data)
		: base(data)
	{
	}

	public NSMediaSetWrapper(Guid data)
		: base(data)
	{
	}
}
