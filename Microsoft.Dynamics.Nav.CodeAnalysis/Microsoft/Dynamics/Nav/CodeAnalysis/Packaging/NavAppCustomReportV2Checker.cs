using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class NavAppCustomReportV2Checker : NavAppChecker
{
	protected override NavAppFileTypes FileTypes => NavAppFileTypes.CustomReportLayout;

	public NavAppCustomReportV2Checker(IFileSystem fileSystem)
		: base(new string[1] { ".layoutdata" }, fileSystem)
	{
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	protected override NavAppCheckerResult InternalVerify(NavAppCheckerResult result)
	{
		foreach (NavAppSourceFile filteredSource in FilteredSources)
		{
			try
			{
				using MemoryStream input = new MemoryStream(FileSystem.ReadBytes(filteredSource.FileLocation));
				XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
				xmlReaderSettings.XmlResolver = null;
				using XmlReader reader = XmlReader.Create(input, xmlReaderSettings);
				XDocument.Load(reader);
			}
			catch (Exception exception)
			{
				filteredSource.AddError(Category, NavAppError.FromException(exception));
			}
		}
		return result;
	}
}
