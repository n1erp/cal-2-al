using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

public class IndexChange : ServerResponse
{
	[DataMember]
	public int KeyIndex { get; set; }

	[DataMember]
	public string ChangeType { get; set; }

	[DataMember]
	public bool HasChanges { get; set; }

	public IndexChange()
	{
	}

	public IndexChange(int keyIndex, string changeType, bool hasChanges)
	{
		KeyIndex = keyIndex;
		ChangeType = changeType;
		HasChanges = hasChanges;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}:{1}", KeyIndex, ChangeType);
	}
}
