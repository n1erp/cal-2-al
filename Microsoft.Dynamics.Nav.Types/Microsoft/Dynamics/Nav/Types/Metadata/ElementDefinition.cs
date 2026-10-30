using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlInclude(typeof(PageViewDefinition))]
[XmlInclude(typeof(ControlContainerDefinition))]
[XmlInclude(typeof(ContentDefinition))]
[XmlInclude(typeof(ActionContainerDefinition))]
[XmlInclude(typeof(PageTriggerDefinition))]
[XmlInclude(typeof(DataFieldDefinition))]
[XmlInclude(typeof(NavigationAreaDefinition))]
[XmlInclude(typeof(SourceObjectDefinition))]
[XmlInclude(typeof(PageProperties))]
[XmlInclude(typeof(AreaBaseDefinition))]
[XmlInclude(typeof(ContentAreaDefinition))]
[XmlInclude(typeof(InfopartsAreaDefinition))]
[XmlInclude(typeof(ControlAddInResourceDefinition))]
[XmlInclude(typeof(ControlAddInManifestDefinition))]
[XmlInclude(typeof(ControlTriggerDefinition))]
[XmlInclude(typeof(RelatedObjectDefinition))]
[XmlInclude(typeof(ConditionDefinition))]
[XmlInclude(typeof(TableRelationDefinition))]
[XmlInclude(typeof(ViewDefinitionChart))]
[XmlInclude(typeof(UserFilterDefinition))]
[XmlInclude(typeof(PersonalizedFilterControlDefinition))]
[XmlInclude(typeof(ActionTriggerDefinition))]
[XmlInclude(typeof(ViewDefinitionSorting))]
[XmlInclude(typeof(ViewDefinition))]
[XmlInclude(typeof(FilterDefinition))]
[XmlInclude(typeof(LinkDefinition))]
[XmlInclude(typeof(IDElementDefinition))]
[XmlInclude(typeof(NamedElementDefinition))]
[XmlInclude(typeof(UIElementDefinition))]
[XmlInclude(typeof(CommandBarDefinition))]
[XmlInclude(typeof(ControlBaseDefinition))]
[XmlInclude(typeof(InfopartBaseDefinition))]
[XmlInclude(typeof(InfopartChartDefinition))]
[XmlInclude(typeof(InfopartSystemDefinition))]
[XmlInclude(typeof(InfopartPageDefinition))]
[XmlInclude(typeof(ControlContainerPlaceHolder))]
[XmlInclude(typeof(ControlGroupBaseDefinition))]
[XmlInclude(typeof(StackGroupDefinition))]
[XmlInclude(typeof(RepeaterDefinition))]
[XmlInclude(typeof(GridLayoutDefinition))]
[XmlInclude(typeof(ColumnLayoutDefinition))]
[XmlInclude(typeof(ControlGroupDefinition))]
[XmlInclude(typeof(ReportDefinition))]
[XmlInclude(typeof(RecordLinksControlDefinition))]
[XmlInclude(typeof(OutlookOverviewDefinition))]
[XmlInclude(typeof(ControlDataboundDefinition))]
[XmlInclude(typeof(FilterControlDefinition))]
[XmlInclude(typeof(ControlDefinition))]
[XmlInclude(typeof(ProgressControlDefinition))]
[XmlInclude(typeof(NotesControlDefinition))]
[XmlInclude(typeof(ActionBaseDefinition))]
[XmlInclude(typeof(ActionDefinition))]
[XmlInclude(typeof(ActionContainerPlaceHolder))]
[XmlInclude(typeof(ActionGroupBaseDefinition))]
[XmlInclude(typeof(NavigationGroupDefinition))]
[XmlInclude(typeof(ActionGroupDefinition))]
[XmlInclude(typeof(SeparatorDefinition))]
[XmlInclude(typeof(NavigationPaneDefinition))]
[XmlInclude(typeof(ConceptDefinition))]
[XmlInclude(typeof(PageDefinition))]
[XmlInclude(typeof(NavigationDefinition))]
[XmlInclude(typeof(MasterPage))]
[XmlInclude(typeof(MasterNavigation))]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[KnownType(typeof(PageViewDefinition))]
[KnownType(typeof(ControlContainerDefinition))]
[KnownType(typeof(ContentDefinition))]
[KnownType(typeof(ActionContainerDefinition))]
[KnownType(typeof(PageTriggerDefinition))]
[KnownType(typeof(DataFieldDefinition))]
[KnownType(typeof(NavigationAreaDefinition))]
[KnownType(typeof(SourceObjectDefinition))]
[KnownType(typeof(PageProperties))]
[KnownType(typeof(AreaBaseDefinition))]
[KnownType(typeof(ContentAreaDefinition))]
[KnownType(typeof(InfopartsAreaDefinition))]
[KnownType(typeof(ControlAddInResourceDefinition))]
[KnownType(typeof(ControlAddInManifestDefinition))]
[KnownType(typeof(ControlTriggerDefinition))]
[KnownType(typeof(RelatedObjectDefinition))]
[KnownType(typeof(ConditionDefinition))]
[KnownType(typeof(TableRelationDefinition))]
[KnownType(typeof(ViewDefinitionChart))]
[KnownType(typeof(UserFilterDefinition))]
[KnownType(typeof(PersonalizedFilterControlDefinition))]
[KnownType(typeof(ActionTriggerDefinition))]
[KnownType(typeof(ViewDefinitionSorting))]
[KnownType(typeof(ViewDefinition))]
[KnownType(typeof(FilterDefinition))]
[KnownType(typeof(LinkDefinition))]
[KnownType(typeof(IDElementDefinition))]
[KnownType(typeof(NamedElementDefinition))]
[KnownType(typeof(UIElementDefinition))]
[KnownType(typeof(CommandBarDefinition))]
[KnownType(typeof(ControlBaseDefinition))]
[KnownType(typeof(InfopartBaseDefinition))]
[KnownType(typeof(InfopartChartDefinition))]
[KnownType(typeof(InfopartSystemDefinition))]
[KnownType(typeof(InfopartPageDefinition))]
[KnownType(typeof(ControlContainerPlaceHolder))]
[KnownType(typeof(ControlGroupBaseDefinition))]
[KnownType(typeof(StackGroupDefinition))]
[KnownType(typeof(RepeaterDefinition))]
[KnownType(typeof(GridLayoutDefinition))]
[KnownType(typeof(ColumnLayoutDefinition))]
[KnownType(typeof(ControlGroupDefinition))]
[KnownType(typeof(ReportDefinition))]
[KnownType(typeof(RecordLinksControlDefinition))]
[KnownType(typeof(OutlookOverviewDefinition))]
[KnownType(typeof(ControlDataboundDefinition))]
[KnownType(typeof(FilterControlDefinition))]
[KnownType(typeof(ControlDefinition))]
[KnownType(typeof(ProgressControlDefinition))]
[KnownType(typeof(NotesControlDefinition))]
[KnownType(typeof(ActionBaseDefinition))]
[KnownType(typeof(ActionDefinition))]
[KnownType(typeof(ActionContainerPlaceHolder))]
[KnownType(typeof(ActionGroupBaseDefinition))]
[KnownType(typeof(NavigationGroupDefinition))]
[KnownType(typeof(ActionGroupDefinition))]
[KnownType(typeof(SeparatorDefinition))]
[KnownType(typeof(NavigationPaneDefinition))]
[KnownType(typeof(ConceptDefinition))]
[KnownType(typeof(PageDefinition))]
[KnownType(typeof(NavigationDefinition))]
[KnownType(typeof(MasterPage))]
[KnownType(typeof(MasterNavigation))]
[DataContract(Name = "ED", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class ElementDefinition : ICloneable, IExtensibleDataObject
{
	private BitArray specifiedProperties;

	[NonSerialized]
	private ExtensionDataObject unknownDataBag;

	private static readonly ConcurrentDictionary<int, string> threeLetterLanguageNames = new ConcurrentDictionary<int, string>();

	protected const string SymbolGenerationLanguage = "ENU";

	public virtual ExtensionDataObject ExtensionData
	{
		get
		{
			return unknownDataBag;
		}
		set
		{
			unknownDataBag = value;
		}
	}

	[DataMember(Name = "EDSpec", EmitDefaultValue = false, IsRequired = false)]
	[XmlIgnore]
	private long SpecifiedProperties
	{
		get
		{
			long num = 0L;
			int num2 = 0;
			foreach (bool item in Specified)
			{
				num |= (long)(item ? 1 : 0) << num2;
				num2++;
			}
			return num;
		}
		set
		{
			long num = value;
			Specified.SetAll(value: false);
			int num2 = 0;
			while (num > 0)
			{
				int num3 = 1 << num2;
				bool flag = Convert.ToBoolean(num3 & num);
				if (flag)
				{
					Specified.Set(num2, flag);
					num ^= num3;
				}
				num2++;
			}
		}
	}

	protected virtual int MaxSpecifiedBits => 0;

	private BitArray Specified
	{
		get
		{
			if (specifiedProperties == null)
			{
				specifiedProperties = new BitArray(MaxSpecifiedBits);
			}
			return specifiedProperties;
		}
	}

	public virtual void SetLanguage(string language, string primaryLanguage)
	{
	}

	public virtual void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
	}

	public virtual ElementDefinition Find(Predicate<ElementDefinition> match)
	{
		ElementDefinition result = null;
		if (match(this))
		{
			return this;
		}
		return result;
	}

	public List<ElementDefinition> FindAll(Predicate<ElementDefinition> match)
	{
		List<ElementDefinition> result = new List<ElementDefinition>();
		FindAllCore(result, match);
		return result;
	}

	public virtual void FindAllCore(List<ElementDefinition> result, Predicate<ElementDefinition> match)
	{
		if (match(this))
		{
			result.Add(this);
		}
	}

	public virtual object Clone()
	{
		ElementDefinition elementDefinition = (ElementDefinition)MemberwiseClone();
		CustomClone(elementDefinition);
		return elementDefinition;
	}

	public void SetSpecified(int index, bool specified)
	{
		Specified.Set(index, specified);
	}

	public bool GetSpecified(int index)
	{
		return Specified.Get(index);
	}

	protected virtual void CustomClone(object newClone)
	{
		ElementDefinition elementDefinition = (ElementDefinition)newClone;
		if (specifiedProperties != null)
		{
			elementDefinition.specifiedProperties = (BitArray)specifiedProperties.Clone();
		}
	}

	protected virtual ControlType ComponentType()
	{
		return ControlType.None;
	}

	protected virtual int ComponentId()
	{
		return -1;
	}

	public virtual void MergeMenuSuiteMultiLanguage(int languageAppGroupId)
	{
	}

	private static string GetValueFromCulture(int lcid)
	{
		return new CultureInfo(lcid).ThreeLetterWindowsLanguageName;
	}

	public static string MergeGlobalLanguageStrings(string attributeValue, LanguageElements globalElements)
	{
		if (globalElements == null)
		{
			return attributeValue;
		}
		bool value = ServerUserSettings.Instance.OverwriteExistingTranslations.Value;
		return BuildMergedLanguageString(attributeValue, globalElements, value);
	}

	public static string MergeAppLanguageStrings(string attributeValue, LanguageElements appElements)
	{
		if (appElements?.LanguageIds == null || !appElements.LanguageIds.Any())
		{
			return attributeValue;
		}
		bool overwriteExistingTranslations = true;
		return BuildMergedLanguageString(attributeValue, appElements, overwriteExistingTranslations);
	}

	private static string BuildMergedLanguageString(string attributeValue, LanguageElements languageElements, bool overwriteExistingTranslations)
	{
		try
		{
			return MultiLanguage.JoinMultiLanguageString(BuildLanguageDictionary(attributeValue, languageElements, overwriteExistingTranslations));
		}
		catch (CultureNotFoundException)
		{
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		return attributeValue;
	}

	private static Dictionary<string, string> BuildLanguageDictionary(string attributeValue, LanguageElements languageElements, bool overwriteExistingTranslations)
	{
		Dictionary<string, string> dictionary = ((attributeValue != null) ? MultiLanguage.SplitMultiLanguageString(attributeValue, forceRaw: true) : new Dictionary<string, string>());
		_ = languageElements.LanguageIds.Count;
		for (int i = 0; i < languageElements.LanguageIds.Count; i++)
		{
			int key = languageElements.LanguageIds[i];
			string orAdd = threeLetterLanguageNames.GetOrAdd(key, GetValueFromCulture);
			string text = languageElements.Messages[i];
			string value = (string.IsNullOrWhiteSpace(text) ? "\"\"" : QuoteString(text));
			if (overwriteExistingTranslations || !dictionary.ContainsKey(orAdd))
			{
				dictionary[orAdd] = value;
			}
		}
		return dictionary;
	}

	private static string QuoteString(string message)
	{
		if (!message.Contains(";"))
		{
			return message;
		}
		message = message.Replace("\"", "\"\"");
		return "\"" + message + "\"";
	}

	public virtual void SolveProperties(MetaField field, MetaTable metaTable, MasterPage masterPage)
	{
	}
}
