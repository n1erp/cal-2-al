using System;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class PageResponseDependencyEntry
{
	private MasterPage masterPage;

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

	public MasterPage MasterPage
	{
		get
		{
			return masterPage;
		}
		set
		{
			masterPage = value;
		}
	}
}
