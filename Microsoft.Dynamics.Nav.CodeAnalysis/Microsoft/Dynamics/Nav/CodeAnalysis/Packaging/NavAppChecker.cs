using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal abstract class NavAppChecker
{
	private readonly List<string> filters = new List<string>();

	internal IFileSystem FileSystem { get; }

	protected IEnumerable<NavAppSourceFile> FilteredSources { get; private set; }

	protected string Category { get; private set; }

	protected abstract NavAppFileTypes FileTypes { get; }

	protected NavAppChecker(IEnumerable<string> extensionFilters, IFileSystem fileSystem)
	{
		filters.AddRange(extensionFilters);
		FileSystem = fileSystem;
	}

	public NavAppCheckerResult Verify(NavAppCheckerResult result)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		IEnumerable<NavAppSourceFile> enumerable;
		if (!filters.Any())
		{
			enumerable = result.SourceFiles;
		}
		else
		{
			IEnumerable<NavAppSourceFile> enumerable2 = result.SourceFiles.Where((NavAppSourceFile source) => filters.Any((string f) => source.FileLocation.EndsWith(f, StringComparison.OrdinalIgnoreCase))).ToArray();
			enumerable = enumerable2;
		}
		IEnumerable<NavAppSourceFile> sources = enumerable;
		List<NavAppError> list = new List<NavAppError>();
		FilteredSources = FilterSourceFiles(sources, list).ToArray();
		list.ForEach((NavAppError error) =>
		{
			result.AddError(error);
		});
		Category = GetType().FullName;
		FilteredSources.Do((NavAppSourceFile s) =>
		{
			s.ErrorCategories.Add(Category, new HashSet<string>());
			s.WarningCategories.Add(Category, new HashSet<string>());
		});
		NavAppCheckerResult result2 = result;
		if (FilteredSources.Any())
		{
			FilteredSources.Do((NavAppSourceFile source) =>
			{
				source.CheckerCategories.Add(Category);
				source.FileTypes |= FileTypes;
			});
			result2 = InternalVerify(result);
		}
		return result2;
	}

	protected virtual IEnumerable<NavAppSourceFile> FilterSourceFiles(IEnumerable<NavAppSourceFile> sources, ICollection<NavAppError> errors)
	{
		return sources;
	}

	protected abstract NavAppCheckerResult InternalVerify(NavAppCheckerResult result);
}
