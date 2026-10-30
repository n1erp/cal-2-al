using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public class ApplicationObjectFileInfo
{
	private static readonly ApplicationObjectFileInfo empty = new ApplicationObjectFileInfo(string.Empty, null);

	public static ApplicationObjectFileInfo Empty => empty;

	public string FileName { get; private set; }

	public ElementType ObjectType { get; private set; }

	public int Id { get; private set; }

	public string VersionList { get; private set; }

	public string Date { get; private set; }

	public string Time { get; private set; }

	public bool Modified { get; private set; }

	internal ApplicationObject ApplicationObject { get; private set; }

	internal ApplicationObjectFileInfo(string fileName, ApplicationObject applicationObject)
	{
		FileName = fileName;
		ApplicationObject = applicationObject;
		if (applicationObject != null)
		{
			ObjectType = applicationObject.ElementTypeInfo.ElementType;
			Id = applicationObject.Id.ObjectNumber;
			VersionList = applicationObject.ObjectProperties.VersionList;
			Date = applicationObject.ObjectProperties.Date;
			Time = applicationObject.ObjectProperties.Time;
			Modified = BooleanType.Create(applicationObject.ObjectProperties.Modified);
		}
	}

	public override string ToString()
	{
		return FileName;
	}
}
