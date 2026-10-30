using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public class Formatter
{
	private class FakeWorkspace : Workspace
	{
		public FakeWorkspace()
			: base(HostServices.DefaultHost, "FakeWorkspace")
		{
		}

		public Project AddProject(string name, string language)
		{
			ProjectInfo projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), name, name, language);
			return AddProject(projectInfo);
		}

		public Project AddProject(ProjectInfo projectInfo)
		{
			if (projectInfo == null)
			{
				throw new ArgumentNullException("projectInfo");
			}
			OnProjectAdded(projectInfo);
			return CurrentSolution.GetProject(projectInfo.Id);
		}

		public Document AddDocument(DocumentInfo documentInfo)
		{
			if (documentInfo == null)
			{
				throw new ArgumentNullException("documentInfo");
			}
			OnDocumentAdded(documentInfo);
			return CurrentSolution.GetDocument(documentInfo.Id);
		}
	}

	public static async Task FormatFilesInFolder(string folder)
	{
		foreach (Dictionary<string, string> item in PartitionFilesInFolder(folder))
		{
			await FormatFilesInWorkspace(item);
		}
	}

	private static IEnumerable<Dictionary<string, string>> PartitionFilesInFolder(string folder)
	{
		string[] filesToFormat = Directory.GetFiles(folder, "*.al", SearchOption.AllDirectories);
		int numberOfChunks = filesToFormat.Length / 100 + 1;
		int i = 0;
		while (i < numberOfChunks)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			int num = i * 100;
			int num2 = (i + 1) * 100;
			for (int j = num; j < num2 && j < filesToFormat.Length; j++)
			{
				dictionary.Add(filesToFormat[j], File.ReadAllText(filesToFormat[j]));
			}
			yield return dictionary;
			int num3 = i + 1;
			i = num3;
		}
	}

	private static async Task FormatFilesInWorkspace(Dictionary<string, string> filesInWorkspace)
	{
		using Workspace workspace = CreateSimpleWorkspace(filesInWorkspace);
		Project project = workspace.CurrentSolution.Projects.Single();
		foreach (Document document in project.Documents)
		{
			try
			{
				Console.WriteLine("Formatting: " + document.FilePath);
				string contents = (await (await Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Formatter.FormatAsync(document)).GetTextAsync()).ToString();
				File.WriteAllText(document.FilePath, contents);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to format: " + document.FilePath + ". Exception: " + ex.Message);
			}
		}
	}

	private static Workspace CreateSimpleWorkspace(IDictionary<string, string> sourceFiles)
	{
		FakeWorkspace fakeWorkspace = new FakeWorkspace();
		Project project = fakeWorkspace.AddProject("FakeProject", "AL");
		VersionStamp version = VersionStamp.Create();
		foreach (KeyValuePair<string, string> sourceFile in sourceFiles)
		{
			DocumentInfo documentInfo = DocumentInfo.Create(DocumentId.CreateNewId(project.Id), sourceFile.Key, null, TextLoader.From(TextAndVersion.Create(SourceText.From(sourceFile.Value), version)), sourceFile.Key);
			fakeWorkspace.AddDocument(documentInfo);
		}
		return fakeWorkspace;
	}
}
