using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Model.Tools.IO.Nea;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppPackageLanguageStoreFactory
{
	public LanguageStore GetLanguageStoreForPackage(Guid packageId)
	{
		Stream packageBlob = NavEnvironment.Instance.NavAppMetadataRetriever.GetPackageBlob(PackageId.op_Implicit(packageId));
		List<string> list = new List<string>();
		try
		{
			NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(packageBlob);
			IEnumerable<string> enumerable = navAppPackageReader.ReadTranslationFilePaths();
			foreach (string item2 in enumerable)
			{
				using Stream stream = navAppPackageReader.ReadFile(item2);
				using Stream stream2 = (NeaStreamReader.IsSupported(stream) ? new NeaStreamReader(stream) : stream);
				using StreamReader streamReader = new StreamReader(stream2);
				string item;
				while ((item = streamReader.ReadLine()) != null)
				{
					list.Add(item);
				}
			}
		}
		catch (Exception ex)
		{
			string message = string.Format(CultureInfo.InvariantCulture, "Cannot open the package file due to the following error: {0}", ex.Message);
			throw new InvalidOperationException(message, ex);
		}
		return new LanguageStore(list);
	}
}
