using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Types.Report;

internal interface IRdlcLayoutElement
{
	int DefinitionCount { get; }

	int ReferenceCount { get; }

	bool IsDefined(string name);

	bool IsReferenced(string name);

	void ResolveReference(bool throwOnErrors, XElement value);

	void Clear();

	IReportChangeLogCollection Rename(string oldName, string newName);

	IReportChangeLogCollection Delete(string name, DeleteAction actionOnDelete, string replacementText);
}
