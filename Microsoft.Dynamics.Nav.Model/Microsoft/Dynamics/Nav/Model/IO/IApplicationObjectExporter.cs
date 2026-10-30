using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Model.IO;

public interface IApplicationObjectExporter
{
	void ExportObject(ApplicationObject applicationObject, Stream stream);

	void ExportObjects(IEnumerable<ApplicationObject> applicationObjects, Stream stream, Action progressAction = null);
}
