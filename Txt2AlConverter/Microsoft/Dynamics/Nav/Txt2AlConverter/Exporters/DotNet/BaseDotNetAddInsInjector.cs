using System;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.DotNet;

public abstract class BaseDotNetAddInsInjector : IDotNetAddInsInjector
{
	public abstract DotNetPackageDefinition InjectAddIns(DotNetPackageDefinition package);

	protected void AddAddIn(DotNetPackageDefinition package, string assemblyName, string typeName, string addInName)
	{
		DotNetAssemblyDefinition assembly = AddAddInAssembly(package, assemblyName);
		SetControlAddInProperty(AddAddInType(typeName, addInName, assembly));
	}

	private static void SetControlAddInProperty(DotNetTypeDeclarationDefinition type)
	{
		if (type.Properties == null)
		{
			type.Properties = new PropertyDefinition[0];
		}
		if (!type.Properties.Any((PropertyDefinition p) => p.Name == "IsControlAddIn"))
		{
			type.Properties = type.Properties.Append(new PropertyDefinition
			{
				Name = "IsControlAddIn",
				Value = "true"
			}).ToArray();
		}
	}

	private static DotNetTypeDeclarationDefinition AddAddInType(string typeName, string addInName, DotNetAssemblyDefinition assembly)
	{
		if (assembly.TypeDeclarations == null)
		{
			assembly.TypeDeclarations = Array.Empty<DotNetTypeDeclarationDefinition>();
		}
		DotNetTypeDeclarationDefinition dotNetTypeDeclarationDefinition = assembly.TypeDeclarations.FirstOrDefault((DotNetTypeDeclarationDefinition t) => t.Name == typeName);
		if (dotNetTypeDeclarationDefinition == null || dotNetTypeDeclarationDefinition.AliasName != addInName)
		{
			dotNetTypeDeclarationDefinition = new DotNetTypeDeclarationDefinition
			{
				Name = addInName,
				AliasName = addInName,
				TypeName = typeName
			};
			assembly.TypeDeclarations = assembly.TypeDeclarations.Append(dotNetTypeDeclarationDefinition).ToArray();
		}
		return dotNetTypeDeclarationDefinition;
	}

	private static DotNetAssemblyDefinition AddAddInAssembly(DotNetPackageDefinition package, string assemblyName)
	{
		if (package.AssemblyDeclarations == null)
		{
			package.AssemblyDeclarations = Array.Empty<DotNetAssemblyDefinition>();
		}
		DotNetAssemblyDefinition dotNetAssemblyDefinition = package.AssemblyDeclarations.FirstOrDefault((DotNetAssemblyDefinition a) => a.Name == assemblyName);
		if (dotNetAssemblyDefinition == null)
		{
			dotNetAssemblyDefinition = new DotNetAssemblyDefinition
			{
				Name = assemblyName,
				TypeDeclarations = Array.Empty<DotNetTypeDeclarationDefinition>(),
				Properties = Array.Empty<PropertyDefinition>()
			};
			package.AssemblyDeclarations = package.AssemblyDeclarations.Append(dotNetAssemblyDefinition).ToArray();
		}
		return dotNetAssemblyDefinition;
	}
}
