using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.MetaModel;

public class InvalidApplicationObject : ApplicationObject
{
	public override bool IsValid => false;

	public string Error { get; private set; }

	public int LineNo { get; private set; }

	public int LinePos { get; private set; }

	public InvalidApplicationObject(ElementType type, int id, string error, int lineNo, int linePos)
		: base(GenericElement.Create(type))
	{
		if (type != ElementType.Unknown)
		{
			RootElement.SetInt32Property(PropertyType.Id, id);
		}
		Error = error;
		LineNo = lineNo;
		LinePos = linePos;
	}
}
