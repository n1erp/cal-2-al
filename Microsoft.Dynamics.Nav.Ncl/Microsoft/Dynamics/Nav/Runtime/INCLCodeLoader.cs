using System;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INCLCodeLoader
{
	Task CompileBusinessAssembliesAsync();

	void DeleteUnneededSources();

	void CompileBusinessEventAssemblies();

	Type GetBaseObjectClrType(ObjectType objectType, int objectId);

	ReadOnlyRecordBuffer GetBaseApplicationObjectRecord(ApplicationObjectId objectId);
}
