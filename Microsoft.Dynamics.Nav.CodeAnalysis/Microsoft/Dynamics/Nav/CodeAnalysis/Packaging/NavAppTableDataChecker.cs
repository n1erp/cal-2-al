using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal sealed class NavAppTableDataChecker : NavAppChecker
{
	public const int ExpectedMagic = 55930;

	protected override NavAppFileTypes FileTypes => NavAppFileTypes.TableData;

	private IList<NavAppSourceFile> TableDataFiles => FilteredSources.Where((NavAppSourceFile s) => s.FileLocation.EndsWith(".navxdata", StringComparison.OrdinalIgnoreCase)).ToList();

	public NavAppTableDataChecker(IFileSystem fileSystem)
		: base(new string[2] { ".navxdata", ".txt" }, fileSystem)
	{
	}

	public static bool IsDataInKnownFormat(byte[] source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		using MemoryStream input = new MemoryStream(source);
		using BinaryReader binaryReader = new BinaryReader(input, Encoding.UTF8);
		if (binaryReader.BaseStream.Position >= binaryReader.BaseStream.Length)
		{
			return false;
		}
		return binaryReader.ReadInt32() == 55930;
	}

	protected override IEnumerable<NavAppSourceFile> FilterSourceFiles(IEnumerable<NavAppSourceFile> sources, ICollection<NavAppError> errors)
	{
		return sources.Where((NavAppSourceFile s) => Path.GetExtension(s.FileLocation).Equals(".navxdata", StringComparison.OrdinalIgnoreCase) || s.FileTypes.HasFlag(NavAppFileTypes.NavObjectSourceFile));
	}

	protected override NavAppCheckerResult InternalVerify(NavAppCheckerResult result)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		MakeSureEachDataFileIsOfTheCorrectFormat(result);
		TableDataFiles.Do((NavAppSourceFile file) =>
		{
			file.FileTypes |= NavAppFileTypes.TableData;
		});
		return result;
	}

	private void MakeSureEachDataFileIsOfTheCorrectFormat(NavAppCheckerResult result)
	{
		TableDataFiles.Do((NavAppSourceFile source) =>
		{
			if (!IsDataInKnownFormat(FileSystem.ReadBytes(source.FileLocation)))
			{
				string message = string.Format(CultureInfo.InvariantCulture, NavAppCheckerStrings.NavAppsCannotValidateTableData, Path.GetFileName(source.FileLocation));
				result.AddError(NavAppError.FromMessage(message, NavAppErrorType.InvalidOperation));
			}
		});
	}
}
