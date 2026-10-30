using System.Reflection;

namespace Microsoft.Cci;

internal interface IPlatformInvokeInformation
{
	string ModuleName { get; }

	string EntryPointName { get; }

	MethodImportAttributes Flags { get; }
}
