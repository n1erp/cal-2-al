using System;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class PageRequestDependencyEntry
{
	private int pageId;

	private string personalizationId;

	public string PersonalizationId
	{
		get
		{
			return personalizationId;
		}
		set
		{
			personalizationId = value;
		}
	}

	public int PageId
	{
		get
		{
			return pageId;
		}
		set
		{
			pageId = value;
		}
	}
}
