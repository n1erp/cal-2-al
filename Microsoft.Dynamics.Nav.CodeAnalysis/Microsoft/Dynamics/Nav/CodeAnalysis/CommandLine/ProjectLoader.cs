using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

public static class ProjectLoader
{
	public static ProjectManifest LoadFromFolder(string folderPath, IList<Diagnostic> diagnostics, bool manifestIsMandatory = true)
	{
		if (string.IsNullOrEmpty(folderPath))
		{
			return null;
		}
		return LoadFromFile(Path.Combine(folderPath, "app.json"), diagnostics, manifestIsMandatory);
	}

	internal static ProjectManifest LoadFromFile(string projectManifestPath)
	{
		ProjectManifest result = null;
		if (!string.IsNullOrEmpty(projectManifestPath))
		{
			string directoryName = Path.GetDirectoryName(projectManifestPath);
			projectManifestPath = PathUtilities.ResolveRelativePath(projectManifestPath, directoryName);
			if (FileUtilities.ValidateFileExists(projectManifestPath))
			{
				result = ProjectManifest.ReadFromFile(projectManifestPath, new List<Diagnostic>());
			}
		}
		return result;
	}

	[SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
	public static ProjectManifest LoadFromFile(string projectManifestPath, IList<Diagnostic> diagnostics, bool manifestIsMandatory = true)
	{
		ProjectManifest projectManifest = null;
		if (!string.IsNullOrEmpty(projectManifestPath))
		{
			string directoryName = Path.GetDirectoryName(projectManifestPath);
			projectManifestPath = PathUtilities.ResolveRelativePath(projectManifestPath, directoryName);
			if (FileUtilities.ValidateFileExists(projectManifestPath))
			{
				projectManifest = ProjectManifest.ReadFromFile(projectManifestPath, diagnostics);
				if (projectManifest?.AppManifest != null)
				{
					projectManifest.AppManifest.Validate(projectManifestPath, diagnostics);
					projectManifest.AppManifest.ValidateDependencies(diagnostics);
				}
			}
			else if (manifestIsMandatory)
			{
				diagnostics.Add(Diagnostic.Create(MessageProvider.Instance, 1001, projectManifestPath));
			}
		}
		return projectManifest;
	}
}
