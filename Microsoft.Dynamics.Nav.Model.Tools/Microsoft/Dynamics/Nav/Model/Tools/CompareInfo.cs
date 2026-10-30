using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public class CompareInfo
{
	public string ObjectType { get; private set; }

	public int Id { get; private set; }

	public CompareResult CompareResult { get; private set; }

	public ApplicationObjectFileInfo Delta { get; private set; }

	public ErrorInfo Error { get; private set; }

	public CompareInfo(string type, int id, CompareResult result, ApplicationObjectFileInfo delta = null)
	{
		ObjectType = type;
		Id = id;
		Delta = delta ?? ApplicationObjectFileInfo.Empty;
		CompareResult = result;
	}

	public CompareInfo(InvalidApplicationObject error, string file)
		: this(error.ElementTypeInfo.ElementType.ToString(), error.Id.ObjectNumber, CompareResult.Failed)
	{
		Error = new ErrorInfo(file, error.Error, error.LineNo, error.LinePos);
	}
}
