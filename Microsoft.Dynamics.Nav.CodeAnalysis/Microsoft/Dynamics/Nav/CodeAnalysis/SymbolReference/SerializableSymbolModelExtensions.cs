using System;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

internal static class SerializableSymbolModelExtensions
{
	private static readonly char[] SplitChars = new char[1] { ',' };

	public static TypeDefinition GetTypeDefinition(this ControlDefinition control)
	{
		if (control.TypeDefinition != null)
		{
			return control.TypeDefinition;
		}
		return new TypeDefinition
		{
			Name = control.Type
		};
	}

	public static TypeDefinition GetTypeDefinition(this FieldDefinition field)
	{
		if (field.TypeDefinition != null)
		{
			return field.TypeDefinition;
		}
		return new TypeDefinition
		{
			Name = field.Type,
			OptionMembers = ConvertOptionMembers(field.OptionMembers)
		};
	}

	public static Version GetVersion(this DotNetAssemblyDefinition assembly)
	{
		string text = Array.Find(assembly.Properties, (PropertyDefinition p) => p.Name == "Version")?.Value;
		if (text == null)
		{
			return null;
		}
		return Version.Parse(text);
	}

	public static string GetCulture(this DotNetAssemblyDefinition assembly)
	{
		return Array.Find(assembly.Properties, (PropertyDefinition p) => p.Name == "Culture")?.Value;
	}

	public static string GetPublicKeyToken(this DotNetAssemblyDefinition assembly)
	{
		return Array.Find(assembly.Properties, (PropertyDefinition p) => p.Name == "PublicKeyToken")?.Value;
	}

	public static TypeDefinition GetTypeDefinition(this VariableDefinition variable)
	{
		if (variable.TypeDefinition != null)
		{
			return variable.TypeDefinition;
		}
		return new TypeDefinition
		{
			Name = variable.Type,
			OptionMembers = ConvertOptionMembers(variable.OptionMembers),
			ArrayDimensions = ConvertArrayDimensions(variable.ArrayDimensions),
			Subtype = variable.Subtype
		};
	}

	public static TypeDefinition GetReturnTypeDefinition(this MethodDefinition methodDefinition)
	{
		if (methodDefinition.ReturnTypeDefinition != null)
		{
			return methodDefinition.ReturnTypeDefinition;
		}
		return new TypeDefinition
		{
			Name = methodDefinition.ReturnType
		};
	}

	public static TypeDefinition GetTypeDefinition(this ParameterDefinition parameter)
	{
		if (parameter.TypeDefinition != null)
		{
			return parameter.TypeDefinition;
		}
		return new TypeDefinition
		{
			Name = parameter.Type,
			ArrayDimensions = ConvertArrayDimensions(parameter.ArrayDimensions),
			OptionMembers = ConvertOptionMembers(parameter.OptionMembers),
			Subtype = parameter.Subtype
		};
	}

	internal static string Name(this LanguageElement element)
	{
		return CreateName(element.Name);
	}

	internal static string Name(this ParameterDefinition parameter)
	{
		return CreateName(parameter.Name);
	}

	private static int[] ConvertArrayDimensions(string arrayDimensions)
	{
		if (string.IsNullOrEmpty(arrayDimensions))
		{
			return null;
		}
		return arrayDimensions.Split(',').Select(int.Parse).ToArray();
	}

	private static string[] ConvertOptionMembers(string optionMembers)
	{
		if (optionMembers == null)
		{
			return null;
		}
		if (optionMembers.Length == 0)
		{
			return new string[0];
		}
		return optionMembers.Split(SplitChars, StringSplitOptions.None);
	}

	private static string CreateName(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return name;
		}
		return string.Empty;
	}
}
