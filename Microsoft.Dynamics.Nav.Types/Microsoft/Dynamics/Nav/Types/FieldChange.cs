using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

public class FieldChange : ServerResponse
{
	[DataMember]
	public int FieldIndex { get; set; }

	[DataMember]
	public string ChangeType { get; set; }

	[DataMember]
	public bool HasChanges { get; set; }

	public FieldChange()
	{
	}

	public FieldChange(int fieldIndex, string changeType, bool hasChanges)
	{
		FieldIndex = fieldIndex;
		ChangeType = changeType;
		HasChanges = hasChanges;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}:{1}", FieldIndex, ChangeType);
	}
}
