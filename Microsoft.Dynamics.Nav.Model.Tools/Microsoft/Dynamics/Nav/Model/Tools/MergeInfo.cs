using System.Diagnostics;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Model.Tools;

[DebuggerDisplay("{ObjectType} {Id} {MergeResult}")]
public class MergeInfo
{
	public string ObjectType { get; private set; }

	public int Id { get; private set; }

	public MergeResult MergeResult { get; private set; }

	public ApplicationObjectFileInfo Original { get; private set; }

	public ApplicationObjectFileInfo Target { get; private set; }

	public ApplicationObjectFileInfo Modified { get; private set; }

	public ApplicationObjectFileInfo Result { get; private set; }

	public ApplicationObjectFileInfo Conflict { get; private set; }

	public ErrorInfo Error { get; private set; }

	public MergeInfo(InvalidApplicationObject error, string file)
		: this(MergeResult.Failed, null, null, null, null, null)
	{
		ObjectType = error.RootElement.ElementTypeInfo.ElementType.ToString();
		Id = error.Id.ObjectNumber;
		Error = new ErrorInfo(file, error.Error, error.LineNo, error.LinePos);
	}

	public MergeInfo(MergeResult mergeResult, ApplicationObjectFileInfo original, ApplicationObjectFileInfo modified, ApplicationObjectFileInfo target, ApplicationObjectFileInfo result, ApplicationObjectFileInfo conflict)
	{
		ApplicationObjectFileInfo applicationObjectFileInfo = original ?? modified ?? target;
		if (applicationObjectFileInfo != null)
		{
			ObjectType = applicationObjectFileInfo.ObjectType.ToString();
			Id = applicationObjectFileInfo.Id;
		}
		MergeResult = mergeResult;
		Original = original ?? ApplicationObjectFileInfo.Empty;
		Target = target ?? ApplicationObjectFileInfo.Empty;
		Modified = modified ?? ApplicationObjectFileInfo.Empty;
		Result = result ?? ApplicationObjectFileInfo.Empty;
		Conflict = conflict ?? ApplicationObjectFileInfo.Empty;
	}
}
