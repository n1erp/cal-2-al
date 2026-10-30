using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTextConstantCaptionResolver
{
	private readonly ObjectType objType;

	private readonly int objId;

	private readonly int codeId;

	private readonly int txtId;

	private readonly int fieldId;

	private readonly int ctrlType;

	private readonly bool isRequestPage;

	private readonly int[] originalLcids;

	private readonly string[] originalValues;

	public NavTextConstantCaptionResolver(int[] originalLcids, string[] originalValues)
	{
		this.originalLcids = originalLcids;
		this.originalValues = originalValues;
	}

	public NavTextConstantCaptionResolver(ObjectType objType, int objId, int codeId, int txtId, int fieldId, int ctrlType, bool isRequestPage, int[] originalLcids, string[] originalValues)
	{
		this.objType = objType;
		this.objId = objId;
		this.codeId = codeId;
		this.txtId = txtId;
		this.fieldId = fieldId;
		this.ctrlType = ctrlType;
		this.isRequestPage = isRequestPage;
		this.originalLcids = originalLcids;
		this.originalValues = originalValues;
	}

	public NCLCaptionStrings ResolveCaptionStrings(int languageAppGroupId)
	{
		int[] languageIds = originalLcids;
		string[] languageStrings = originalValues;
		if (objId != 0 && LanguageProvider.Provider.HasDataFor(objType, objId, languageAppGroupId))
		{
			ControlType controlType = ResolveControlType();
			LanguageElements globalLanguageElementsTextConst = LanguageProvider.Provider.GetGlobalLanguageElementsTextConst(objType, objId, controlType, fieldId, codeId, txtId, isRequestPage);
			LanguageElements navAppLanguageElementsTextConst = LanguageProvider.Provider.GetNavAppLanguageElementsTextConst(objType, objId, controlType, fieldId, codeId, txtId, isRequestPage, languageAppGroupId);
			DoMergeForAppElements(ref languageIds, ref languageStrings, navAppLanguageElementsTextConst);
			DoMergeForGlobalElements(ref languageIds, ref languageStrings, globalLanguageElementsTextConst);
		}
		return NCLCaptionStrings.CreateTextConstant(languageIds, languageStrings);
	}

	private ControlType ResolveControlType()
	{
		ControlType result = ControlType.None;
		if (objType == ObjectType.Table && fieldId != -1)
		{
			result = ControlType.Field;
		}
		else if ((objType == ObjectType.Page || isRequestPage) && fieldId != -1)
		{
			result = ((ctrlType == 1) ? ControlType.Control : ControlType.ActionItem);
		}
		else if (objType == ObjectType.Report && fieldId != -1)
		{
			result = ((ctrlType == 3) ? ControlType.DataItem : ControlType.Column);
		}
		return result;
	}

	private static void DoMergeForAppElements(ref int[] languageIds, ref string[] languageStrings, LanguageElements appElements)
	{
		if (appElements == null || !appElements.LanguageIds.Any())
		{
			return;
		}
		List<int> list = new List<int>(languageIds);
		List<string> list2 = new List<string>(languageStrings);
		for (int i = 0; i < appElements.LanguageIds.Count; i++)
		{
			int num = appElements.LanguageIds[i];
			string text = appElements.Messages[i];
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (flag)
				{
					break;
				}
				if (list[j] == num)
				{
					flag = true;
					list2[j] = text;
				}
			}
			if (!flag)
			{
				list.Add(num);
				list2.Add(text);
			}
		}
		languageIds = list.ToArray();
		languageStrings = list2.ToArray();
	}

	private static void DoMergeForGlobalElements(ref int[] languageIds, ref string[] languageStrings, LanguageElements globalElements)
	{
		if (globalElements == null || !globalElements.LanguageIds.Any())
		{
			return;
		}
		List<int> list = new List<int>(languageIds);
		List<string> list2 = new List<string>(languageStrings);
		int count = globalElements.LanguageIds.Count;
		for (int i = 0; i < count; i++)
		{
			if (!list.Contains(globalElements.LanguageIds[i]))
			{
				list.Add(globalElements.LanguageIds[i]);
				list2.Add(string.IsNullOrEmpty(globalElements.Messages[i]) ? "\"\"" : globalElements.Messages[i]);
			}
		}
		languageIds = list.ToArray();
		languageStrings = list2.ToArray();
	}
}
