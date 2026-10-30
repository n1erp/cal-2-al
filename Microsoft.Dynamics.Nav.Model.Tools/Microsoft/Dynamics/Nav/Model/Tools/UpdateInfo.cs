using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public class UpdateInfo
{
	public string ObjectType { get; private set; }

	public int Id { get; private set; }

	public UpdateResult UpdateResult { get; private set; }

	public ApplicationObjectFileInfo Result { get; private set; }

	public ApplicationObjectFileInfo Conflict { get; private set; }

	public ErrorInfo Error { get; private set; }

	public UpdateInfo(string type, int id, UpdateResult updateResult, ApplicationObjectFileInfo result = null, ApplicationObjectFileInfo conflict = null)
	{
		ObjectType = type;
		Id = id;
		UpdateResult = updateResult;
		Result = result ?? ApplicationObjectFileInfo.Empty;
		Conflict = conflict ?? ApplicationObjectFileInfo.Empty;
	}

	public UpdateInfo(UpdateResult updateResult, ApplicationObjectFileInfo result, ApplicationObjectFileInfo conflict = null)
		: this(result.ObjectType.ToString(), result.Id, updateResult, result, conflict)
	{
	}

	public UpdateInfo(InvalidApplicationObject error, string file)
		: this(error.ElementTypeInfo.ElementType.ToString(), error.Id.ObjectNumber, UpdateResult.Failed)
	{
		Error = new ErrorInfo(file, error.Error, error.LineNo, error.LinePos);
	}
}
