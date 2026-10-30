using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class NavAppCheckerResult
{
	private readonly NavAppSourceFileCollection sourceFiles = new NavAppSourceFileCollection();

	public IEnumerable<string> Errors => SourceFiles.Where((NavAppSourceFile s) => !s.PassedCheck).SelectMany((NavAppSourceFile s) => s.ErrorCategories.SelectMany((KeyValuePair<string, HashSet<string>> e) => e.Value)).Distinct()
		.Concat(PassErrors);

	public HashSet<string> PassErrors { get; } = new HashSet<string>();

	public IEnumerable<NavAppSourceFile> SourceFiles => sourceFiles;

	public NavAppCheckerResult(IEnumerable<string> sourcefiles)
	{
		sourcefiles.Do((string sourceFileLocation) =>
		{
			sourceFiles.Add(new NavAppSourceFile(sourceFileLocation));
		});
	}
}
