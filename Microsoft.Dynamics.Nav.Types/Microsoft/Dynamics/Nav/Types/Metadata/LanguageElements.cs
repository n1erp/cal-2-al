using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class LanguageElements
{
	public ReadOnlyCollection<int> LanguageIds { get; private set; }

	public ReadOnlyCollection<string> Messages { get; private set; }

	public LanguageElements(ReadOnlyCollection<int> languageIds, ReadOnlyCollection<string> messages)
	{
		LanguageIds = languageIds;
		Messages = messages;
	}

	public LanguageElements MergeWith(LanguageElements newElements)
	{
		if (newElements == null)
		{
			return this;
		}
		List<int> list = LanguageIds.ToList();
		List<string> list2 = Messages.ToList();
		for (int i = 0; i < newElements.LanguageIds.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] == newElements.LanguageIds[i])
				{
					list2[j] = newElements.Messages[i];
					flag = true;
				}
			}
			if (!flag)
			{
				list.Add(newElements.LanguageIds[i]);
				list2.Add(newElements.Messages[i]);
			}
		}
		return new LanguageElements(new ReadOnlyCollection<int>(list), new ReadOnlyCollection<string>(list2));
	}
}
