using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.DotNet;

public class DeltaDotNetAddInsInjector : BaseDotNetAddInsInjector
{
	private readonly DotNetPackageSyntax[] dotNetPackages;

	public DeltaDotNetAddInsInjector(string source)
	{
		dotNetPackages = (from d in SyntaxTree.ParseObjectText(source).GetRoot().ChildNodes()
			where d.Kind == SyntaxKind.DotNetPackage
			select d).Cast<DotNetPackageSyntax>().ToArray();
		if (dotNetPackages.Length == 0)
		{
			throw new AlExporterException("The .NET package file does not contain a package definition.");
		}
	}

	public override DotNetPackageDefinition InjectAddIns(DotNetPackageDefinition package)
	{
		DotNetPackageSyntax[] array = dotNetPackages;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (DotNetAssemblySyntax assembly in array[i].Assemblies)
			{
				string assemblyName = assembly.AssemblyName.Unquoted();
				foreach (DotNetTypeDeclarationSyntax typeDeclaration in assembly.TypeDeclarations)
				{
					string typeName = typeDeclaration.TypeName.Unquoted();
					string addInName = typeDeclaration.TypeAlias?.Unquoted() ?? typeDeclaration.GetTypeAlias();
					AddAddIn(package, assemblyName, typeName, addInName);
				}
			}
		}
		return package;
	}
}
