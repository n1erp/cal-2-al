using System.Collections.ObjectModel;

namespace Microsoft.Dynamics.Nav.Types;

public class SavePageDataPersonalizationRequest
{
	private readonly Collection<ObjectPersonalizationData> objectDependencies;

	public ObjectPersonalizationData PersonalizationData { get; set; }

	public Collection<ObjectPersonalizationData> ObjectDependencies => objectDependencies;

	public SavePageDataPersonalizationRequest()
	{
		objectDependencies = new Collection<ObjectPersonalizationData>();
	}
}
