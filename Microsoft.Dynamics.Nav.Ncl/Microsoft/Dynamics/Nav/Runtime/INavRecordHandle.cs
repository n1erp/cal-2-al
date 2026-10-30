namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavRecordHandle : INavApplicationObjectBaseHandle
{
	new NavRecord Target { get; }

	INavRecordHandle Clone(ITreeObject parentOfClone);

	void ALAssign(INavRecordHandle other);

	NavRecordHandle ALByValue(ITreeObject parentOfResult, bool cloneAsTemporary);

	void Clear();
}
