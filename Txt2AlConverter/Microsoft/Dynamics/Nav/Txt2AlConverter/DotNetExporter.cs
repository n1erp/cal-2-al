using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.DotNet;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class DotNetExporter
{
	private readonly IDotNetAddInsInjector dotNetAddInInjector;

	public DotNetExporter(IDotNetAddInsInjector dotNetAddInInjector)
	{
		this.dotNetAddInInjector = dotNetAddInInjector;
	}

	public void Export(DotNetPackageDefinition package, IAlWriter writer)
	{
		package = dotNetAddInInjector.InjectAddIns(package);
		writer.WriteSection("dotnet", () =>
		{
			DotNetAssemblyDefinition[] assemblyDeclarations = package.AssemblyDeclarations;
			foreach (DotNetAssemblyDefinition value in assemblyDeclarations)
			{
				WriteAssemblyDeclaration(value, writer);
			}
		});
	}

	private void WriteAssemblyDeclaration(DotNetAssemblyDefinition value, IAlWriter writer)
	{
		writer.WriteSection("assembly(\"" + value.Name + "\")", () =>
		{
			bool flag = false;
			Version version = value.GetVersion();
			if (version != null)
			{
				writer.WriteLine($"Version='{version}';");
				flag = true;
			}
			string culture = value.GetCulture();
			if (culture != null)
			{
				writer.WriteLine("Culture='" + culture + "';");
				flag = true;
			}
			string publicKeyToken = value.GetPublicKeyToken();
			if (publicKeyToken != null)
			{
				writer.WriteLine("PublicKeyToken='" + publicKeyToken + "';");
				flag = true;
			}
			if (flag)
			{
				writer.WriteLine();
			}
			for (int i = 0; i < value.TypeDeclarations.Length; i++)
			{
				WriteTypeDeclaration(value.TypeDeclarations[i], writer);
				if (i != value.TypeDeclarations.Length - 1)
				{
					writer.WriteLine();
				}
			}
		});
		writer.WriteLine();
	}

	private void WriteTypeDeclaration(DotNetTypeDeclarationDefinition type, IAlWriter writer)
	{
		writer.WriteSection("type(\"" + type.TypeName + "\";\"" + type.AliasName + "\")", () =>
		{
			if (type.Properties != null)
			{
				PropertyDefinition[] properties = type.Properties;
				foreach (PropertyDefinition propertyDefinition in properties)
				{
					writer.WriteProperty(propertyDefinition.Name, propertyDefinition.Value);
				}
			}
		});
	}
}
