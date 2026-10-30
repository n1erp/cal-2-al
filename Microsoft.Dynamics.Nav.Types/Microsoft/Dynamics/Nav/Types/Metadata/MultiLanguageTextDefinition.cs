using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("xsd", "2.0.50727.42")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
public class MultiLanguageTextDefinition
{
	private string idField;

	private string valueField;

	[XmlAttribute]
	public string ID
	{
		get
		{
			return idField;
		}
		set
		{
			idField = value;
		}
	}

	[XmlText]
	public string Value
	{
		get
		{
			return valueField;
		}
		set
		{
			valueField = value;
		}
	}

	public static string GetText(MultiLanguageTextDefinition[] texts, CultureInfo culture)
	{
		if (texts == null || culture == null)
		{
			return null;
		}
		return (from text in texts
			where string.Compare(text.ID, culture.ThreeLetterWindowsLanguageName, ignoreCase: true, CultureInfo.InvariantCulture) == 0
			select text.Value).FirstOrDefault();
	}

	public static void SetText(ref MultiLanguageTextDefinition[] texts, CultureInfo culture, string newText)
	{
		if (texts == null)
		{
			texts = new MultiLanguageTextDefinition[1];
		}
		else
		{
			using (IEnumerator<MultiLanguageTextDefinition> enumerator = texts.Where((MultiLanguageTextDefinition text) => string.Compare(text.ID, culture.ThreeLetterWindowsLanguageName, ignoreCase: true, CultureInfo.InvariantCulture) == 0).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					enumerator.Current.Value = newText;
					return;
				}
			}
			Array.Resize(ref texts, texts.Length + 1);
		}
		texts[texts.Length - 1] = new MultiLanguageTextDefinition
		{
			ID = culture.ThreeLetterWindowsLanguageName,
			Value = newText
		};
	}

	public static void RemoveText(ref MultiLanguageTextDefinition[] texts, CultureInfo culture)
	{
		List<MultiLanguageTextDefinition> list = new List<MultiLanguageTextDefinition>(texts);
		list.RemoveAll((MultiLanguageTextDefinition match) => string.Compare(match.ID, culture.ThreeLetterWindowsLanguageName, ignoreCase: true, CultureInfo.InvariantCulture) == 0);
		texts = list.ToArray();
	}
}
