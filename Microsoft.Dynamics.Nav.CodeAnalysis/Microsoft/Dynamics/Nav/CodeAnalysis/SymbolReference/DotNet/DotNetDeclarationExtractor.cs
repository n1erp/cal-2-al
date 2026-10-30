using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference.DotNet;

public class DotNetDeclarationExtractor : IDotNetDeclarationExtractor
{
	private class OrdinalCaseInsensitiveEqualityComparer : IEqualityComparer<string>
	{
		public bool Equals(string x, string y)
		{
			return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
		}

		public int GetHashCode(string obj)
		{
			return obj?.ToLowerInvariant().GetHashCode() ?? 0;
		}
	}

	private class VersionComparer : IComparer<Version>
	{
		public static VersionComparer Instance { get; } = new VersionComparer();

		private VersionComparer()
		{
		}

		public int Compare(Version x, Version y)
		{
			if (x == null && y == null)
			{
				return 0;
			}
			if (x == null)
			{
				return 1;
			}
			if (y == null)
			{
				return -1;
			}
			return x.CompareTo(y);
		}
	}

	private const string Separator = "(,\\s*)";

	private const string QualifiedName = "(\\w+\\.)*\\w+";

	private const string QualifiedTypeName = "(\\w+\\.)*\\w+`?(\\d+)?";

	private const string VersionAtom = "(\\d+|\\*)";

	private static readonly string AssemblyName = FormattableString.Invariant(FormattableStringFactory.Create("(?<AssemblyName>{0})", "(\\w+\\.)*\\w+"));

	private static readonly string TypeName = FormattableString.Invariant(FormattableStringFactory.Create("(?<TypeName>({0}(\\+{1})*))", "(\\w+\\.)*\\w+`?(\\d+)?", "(\\w+\\.)*\\w+`?(\\d+)?"));

	private static readonly string Version = FormattableString.Invariant(FormattableStringFactory.Create("Version=(?<Version>({0}\\.{1}\\.{2}\\.{3})){4}?", "(\\d+|\\*)", "(\\d+|\\*)", "(\\d+|\\*)", "(\\d+|\\*)", "(,\\s*)"));

	private static readonly string Culture = FormattableString.Invariant(FormattableStringFactory.Create("Culture=(?<Culture>\\w+){0}?", "(,\\s*)"));

	private const string PublicKeyToken = "PublicKeyToken=(?<PublicKeyToken>\\w+)";

	private static readonly string[] GenericTypeParameterNames = (from c in Enumerable.Range(84, 7).Concat(Enumerable.Range(65, 19))
		select ((char)c).ToString()).ToArray();

	private static readonly string AssemblyQualifiedTypeName = FormattableString.Invariant(FormattableStringFactory.Create("'(?<FullAssemblyName>{0}({1})?({2})?({3})?({4})?)'\\.{5}", AssemblyName, "(,\\s*)", Version, Culture, "PublicKeyToken=(?<PublicKeyToken>\\w+)", TypeName));

	private static readonly Regex DotNetVariableRegex = new Regex(AssemblyQualifiedTypeName);

	private static readonly Regex DotNetTypeDeclarationRegex = new Regex(FormattableString.Invariant($"DotNet \"(?<AssemblyQualifiedTypeName>{AssemblyQualifiedTypeName})\""));

	private readonly Dictionary<string, DotNetAssemblyDefinition> AssemblyDeclarations = new Dictionary<string, DotNetAssemblyDefinition>(new OrdinalCaseInsensitiveEqualityComparer());

	private readonly Dictionary<string, (DotNetTypeDeclarationDefinition Type, string AssemblyQualifiedName)> TypeDeclarations = new Dictionary<string, (DotNetTypeDeclarationDefinition, string)>(new OrdinalCaseInsensitiveEqualityComparer());

	private readonly object myLock = new object();

	private readonly string aliasPrefix;

	public bool HasDeclarations => AssemblyDeclarations.Count != 0;

	public DotNetDeclarationExtractor(string aliasPrefix)
	{
		this.aliasPrefix = aliasPrefix;
	}

	public DotNetDeclarationExtractor()
		: this(string.Empty)
	{
	}

	public static void AddTypeDeclarationsFromTxtSource(IDotNetDeclarationExtractor dotNetExtractor, string txtSource)
	{
		MatchCollection matchCollection = DotNetTypeDeclarationRegex.Matches(txtSource);
		for (int i = 0; i < matchCollection.Count; i++)
		{
			string value = matchCollection[i].Groups["AssemblyQualifiedTypeName"].Value;
			dotNetExtractor.AddTypeDeclarationAndGetAlias(value);
		}
	}

	public string AddTypeDeclarationAndGetAlias(string assemblyQualifiedName)
	{
		Match match = DotNetVariableRegex.Match(assemblyQualifiedName);
		string value = match.Groups["AssemblyName"].Value;
		Version version = GetVersion(GetNullIfEmpty(match.Groups["Version"].Value));
		string nullIfEmpty = GetNullIfEmpty(match.Groups["Culture"].Value);
		string nullIfEmpty2 = GetNullIfEmpty(match.Groups["PublicKeyToken"].Value);
		string value2 = match.Groups["FullAssemblyName"].Value;
		string value3 = match.Groups["TypeName"].Value;
		lock (myLock)
		{
			DotNetTypeDeclarationDefinition item = GetTypeDeclaration(value, value3).Type;
			GetAssemblyDeclaration(value2, value, version, nullIfEmpty, nullIfEmpty2);
			return item.AliasName;
		}
	}

	public DotNetPackageDefinition GetDeclaration()
	{
		DotNetAssemblyDefinition[] assemblyDeclarations = AssemblyDeclarations.Values.GroupJoin(TypeDeclarations.Values, (DotNetAssemblyDefinition assembly) => assembly.Name, ((DotNetTypeDeclarationDefinition Type, string AssemblyQualifiedName) typeDecl) => GetAssemblyName(typeDecl.AssemblyQualifiedName), (DotNetAssemblyDefinition assembly, IEnumerable<(DotNetTypeDeclarationDefinition Type, string AssemblyQualifiedName)> typeDecl) =>
		{
			assembly.TypeDeclarations = typeDecl.Select(((DotNetTypeDeclarationDefinition Type, string AssemblyQualifiedName) t) => t.Type).ToArray();
			return assembly;
		}).ToArray();
		return new DotNetPackageDefinition
		{
			AssemblyDeclarations = assemblyDeclarations
		};
	}

	internal static string GetAssemblyQualifiedTypeName(string assemblyName, string typeName)
	{
		return FormattableString.Invariant($"{assemblyName}, {typeName}");
	}

	private static string GetNullIfEmpty(string value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			return value;
		}
		return null;
	}

	private static DotNetAssemblyDefinition CreateDotNetAssemblyDefinition(string assemblyName, Version version, string culture, string publicKeyToken)
	{
		DotNetAssemblyDefinition dotNetAssemblyDefinition = new DotNetAssemblyDefinition
		{
			Name = assemblyName,
			TypeDeclarations = new DotNetTypeDeclarationDefinition[0]
		};
		PooledList<PropertyDefinition> instance = PooledList<PropertyDefinition>.GetInstance();
		try
		{
			if (!string.IsNullOrWhiteSpace(culture))
			{
				instance.Add(new PropertyDefinition
				{
					Name = "Culture",
					Value = culture
				});
			}
			if (!string.IsNullOrWhiteSpace(publicKeyToken))
			{
				instance.Add(new PropertyDefinition
				{
					Name = "PublicKeyToken",
					Value = publicKeyToken
				});
			}
			if (version != null)
			{
				instance.Add(new PropertyDefinition
				{
					Name = "Version",
					Value = version.ToString()
				});
			}
			dotNetAssemblyDefinition.Properties = instance.ToArray();
			return dotNetAssemblyDefinition;
		}
		finally
		{
			instance.Free();
		}
	}

	private string GetAssemblyName(string assemblyQualifiedName)
	{
		return assemblyQualifiedName.Substring(0, assemblyQualifiedName.IndexOf(','));
	}

	private Version GetVersion(string v)
	{
		if (v == null)
		{
			return null;
		}
		return new Version(v.Replace('*', '0'));
	}

	private DotNetAssemblyDefinition GetAssemblyDeclaration(string fullAssemblyName, string assemblyName, Version version, string culture, string publicKeyToken)
	{
		if (!AssemblyDeclarations.TryGetValue(assemblyName, out var value))
		{
			value = CreateDotNetAssemblyDefinition(assemblyName, version, culture, publicKeyToken);
			AssemblyDeclarations[assemblyName] = value;
		}
		else
		{
			Version version2 = value.GetVersion();
			if (version2 != null && VersionComparer.Instance.Compare(version, version2) > 0)
			{
				value = CreateDotNetAssemblyDefinition(assemblyName, version, culture, publicKeyToken);
				AssemblyDeclarations[assemblyName] = value;
			}
		}
		return value;
	}

	private (DotNetTypeDeclarationDefinition Type, string AssemblyQualifiedName) GetTypeDeclaration(string assemblyName, string typeName)
	{
		string assemblyQualifiedTypeName = GetAssemblyQualifiedTypeName(assemblyName, typeName);
		string typeAlias = GetTypeAlias(typeName, assemblyQualifiedTypeName);
		if (!TypeDeclarations.TryGetValue(typeAlias, out (DotNetTypeDeclarationDefinition, string) value))
		{
			value.Item1 = new DotNetTypeDeclarationDefinition
			{
				TypeName = typeName,
				AliasName = typeAlias
			};
			value.Item2 = assemblyQualifiedTypeName;
			TypeDeclarations.Add(typeAlias, value);
		}
		return value;
	}

	private string GetTypeAlias(string typeName, string assemblyQualifiedTypeName)
	{
		string text = GetAliasFromFullTypeName(typeName);
		if (!string.IsNullOrEmpty(aliasPrefix))
		{
			text = aliasPrefix + text;
		}
		return GetNonConflictingAlias(text, assemblyQualifiedTypeName);
	}

	private string GetAliasFromFullTypeName(string fullTypeName)
	{
		fullTypeName = fullTypeName.Replace('+', '_');
		int num = fullTypeName.LastIndexOf('.');
		if (num == -1)
		{
			return fullTypeName;
		}
		return ReplaceGenericParameters(fullTypeName.Substring(num + 1));
	}

	private static string ReplaceGenericParameters(string typeName)
	{
		if (typeName.IndexOf('`') == -1)
		{
			return typeName;
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		int i = 0;
		int num = 0;
		while (i < typeName.Length)
		{
			if (typeName[i] == '`')
			{
				instance.Builder.Append("_Of");
				i++;
				int num2 = i;
				for (; i < typeName.Length && char.IsDigit(typeName[i]); i++)
				{
				}
				int num3 = i;
				int num4 = int.Parse(typeName.Substring(num2, num3 - num2), CultureInfo.InvariantCulture);
				for (int j = num; j < num + num4; j++)
				{
					instance.Builder.Append('_').Append(GenericTypeParameterNames[j]);
				}
				num += num4;
			}
			else
			{
				instance.Builder.Append(typeName[i]);
				i++;
			}
		}
		return instance.ToStringAndFree();
	}

	private string GetNonConflictingAlias(string alias, string assemblyQualifiedTypeName)
	{
		string text = alias;
		int num = 0;
		(DotNetTypeDeclarationDefinition, string) value;
		while (TypeDeclarations.TryGetValue(text, out value) && value.Item2 != assemblyQualifiedTypeName)
		{
			text = alias + num.ToString(CultureInfo.InvariantCulture);
			num++;
		}
		return text;
	}
}
