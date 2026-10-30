using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public class EnumDeclarationExtractor : IEnumDeclarationExtractor
{
	private readonly object enumListLock = new object();

	private static readonly char[] splitChar = new char[1] { ',' };

	private readonly Dictionary<int, EnumTypeDefinition> idDictionary = new Dictionary<int, EnumTypeDefinition>();

	private readonly HashSet<string> enumNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public bool HasDeclarations => idDictionary.Count > 0;

	public void AddEnumDeclaration(int id, string name, bool extensible, string optionString, string optionCaptionML)
	{
		string[] array = optionString.Split(',');
		lock (enumListLock)
		{
			bool flag = idDictionary.TryGetValue(id, out var value);
			bool flag2 = enumNames.Contains(name);
			if (!flag && !flag2)
			{
				EnumTypeDefinition enumTypeDefinition = new EnumTypeDefinition();
				enumTypeDefinition.Id = id;
				enumTypeDefinition.Name = name;
				enumTypeDefinition.Properties = new PropertyDefinition[1]
				{
					new PropertyDefinition
					{
						Name = "Extensible",
						Value = extensible.ToString().ToLowerInvariant()
					}
				};
				EnumTypeDefinition enumTypeDefinition2 = enumTypeDefinition;
				Dictionary<string, string[]> dictionary = (from cultureAndCaption in ModelTextHelper.MLTextSplitter(optionCaptionML)
					where cultureAndCaption.Key.ThreeLetterWindowsLanguageName != null
					select cultureAndCaption).ToDictionary((KeyValuePair<CultureInfo, string> d) => d.Key.ThreeLetterWindowsLanguageName, (KeyValuePair<CultureInfo, string> d) => d.Value.Split(splitChar));
				List<EnumValueDefinition> list = new List<EnumValueDefinition>();
				for (int num = 0; num < array.Length; num++)
				{
					if (string.IsNullOrEmpty(array[num]))
					{
						continue;
					}
					List<string> list2 = new List<string>();
					foreach (string key in dictionary.Keys)
					{
						string[] array2 = dictionary[key];
						if (num < array2.Length)
						{
							list2.Add(key + "='" + array2[num] + "'");
						}
					}
					list.Add(new EnumValueDefinition
					{
						Id = num,
						Ordinal = num,
						Name = array[num],
						Properties = ((list2.Count == 0) ? Array.Empty<PropertyDefinition>() : new PropertyDefinition[1]
						{
							new PropertyDefinition
							{
								Name = "CaptionML",
								Value = string.Join(",", list2)
							}
						})
					});
				}
				enumTypeDefinition2.Values = list.ToArray();
				idDictionary.Add(id, enumTypeDefinition2);
				enumNames.Add(name);
			}
			else if (flag & flag2)
			{
				bool flag3 = array.Length == value.Values.Length;
				if (flag3)
				{
					for (int num2 = 0; num2 < value.Values.Length; num2++)
					{
						if (!SemanticFacts.IsSameName(value.Values[num2].Name, array[num2]) || value.Values[num2].Ordinal != num2)
						{
							flag3 = false;
							break;
						}
					}
				}
				if (!flag3)
				{
					throw new AlExporterException(FormattableString.Invariant($"An enum with ID '{id}' and name '{name}' is already defined, but with different values (OptionString)."));
				}
			}
			else
			{
				if (flag)
				{
					throw new AlExporterException(FormattableString.Invariant($"An enum with ID '{id}' is already defined with a different name."));
				}
				if (flag2)
				{
					throw new AlExporterException(FormattableString.Invariant($"An enum with name '{name}' is already defined with a different ID."));
				}
			}
		}
	}

	public EnumTypeDefinition[] GetDeclarations()
	{
		return idDictionary.Values.ToArray();
	}
}
