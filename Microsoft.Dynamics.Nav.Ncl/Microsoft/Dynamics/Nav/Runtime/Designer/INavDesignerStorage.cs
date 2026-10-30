using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal interface INavDesignerStorage
{
	NavAppManifest GetManifest(AppId appId, Version version = null);

	NavAppPackageType GetPackageType(AppId appId, Version version = null);

	NavDesignerObjectCollection GetObjects(AppId appId, Version version = null);

	IDictionary<string, byte[]> GetPackageFiles(AppId appId, Version version = null);

	Result SaveData(AppId appId, Stream packageStream, NavAppPackageType packageType);
}
