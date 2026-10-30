namespace Microsoft.Dynamics.Nav.Runtime;

public class TreeSharedObjectContainer : TreeObject, ITreeSharedObjectContainer, ITreeObject
{
	public TreeSharedObjectContainer(ITreeObject parent)
		: base(parent)
	{
	}
}
