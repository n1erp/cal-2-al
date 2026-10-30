using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[XmlRoot(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects", IsNullable = false)]
[DataContract(Name = "MN", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MasterNavigation : ConceptDefinition
{
	public List<string> RemovedNavigationControls;

	private CommandBarDefinition commandBarField;

	private CommandBarDefinition departmentsCommandBarField;

	private NavigationPaneDefinition navigationPaneField;

	private long versionNumberField;

	[DataMember(Name = "MN_1", EmitDefaultValue = false, IsRequired = false)]
	public CommandBarDefinition CommandBar
	{
		get
		{
			return commandBarField;
		}
		set
		{
			commandBarField = value;
		}
	}

	[DataMember(Name = "MN_2", EmitDefaultValue = false, IsRequired = false)]
	public CommandBarDefinition DepartmentsCommandBar
	{
		get
		{
			return departmentsCommandBarField;
		}
		set
		{
			departmentsCommandBarField = value;
		}
	}

	[DataMember(Name = "MN_3", EmitDefaultValue = false, IsRequired = false)]
	public NavigationPaneDefinition NavigationPane
	{
		get
		{
			return navigationPaneField;
		}
		set
		{
			navigationPaneField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "MN_4", EmitDefaultValue = false, IsRequired = false)]
	public long VersionNumber
	{
		get
		{
			return versionNumberField;
		}
		set
		{
			versionNumberField = value;
			VersionNumberSpecified = true;
		}
	}

	[XmlIgnore]
	public bool VersionNumberSpecified
	{
		get
		{
			return GetSpecified(2);
		}
		set
		{
			SetSpecified(2, value);
		}
	}

	protected override int MaxSpecifiedBits => 3;

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (commandBarField != null)
		{
			commandBarField.SetLanguage(language, primaryLanguage);
		}
		if (departmentsCommandBarField != null)
		{
			departmentsCommandBarField.SetLanguage(language, primaryLanguage);
		}
		if (navigationPaneField != null)
		{
			navigationPaneField.SetLanguage(language, primaryLanguage);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (commandBarField != null)
		{
			commandBarField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
		if (departmentsCommandBarField != null)
		{
			departmentsCommandBarField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
		if (navigationPaneField != null)
		{
			navigationPaneField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
	}

	public override ElementDefinition Find(Predicate<ElementDefinition> match)
	{
		if (match(this))
		{
			return this;
		}
		ElementDefinition elementDefinition = base.Find(match);
		if (elementDefinition != null)
		{
			return elementDefinition;
		}
		if (commandBarField != null)
		{
			elementDefinition = commandBarField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (departmentsCommandBarField != null)
		{
			elementDefinition = departmentsCommandBarField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (navigationPaneField != null)
		{
			return navigationPaneField.Find(match);
		}
		return elementDefinition;
	}

	public override void FindAllCore(List<ElementDefinition> result, Predicate<ElementDefinition> match)
	{
		base.FindAllCore(result, match);
		if (commandBarField != null)
		{
			commandBarField.FindAllCore(result, match);
		}
		if (departmentsCommandBarField != null)
		{
			departmentsCommandBarField.FindAllCore(result, match);
		}
		if (navigationPaneField != null)
		{
			navigationPaneField.FindAllCore(result, match);
		}
	}

	public override object Clone()
	{
		MasterNavigation masterNavigation = (MasterNavigation)base.Clone();
		if (commandBarField != null)
		{
			masterNavigation.CommandBar = (CommandBarDefinition)commandBarField.Clone();
		}
		if (departmentsCommandBarField != null)
		{
			masterNavigation.DepartmentsCommandBar = (CommandBarDefinition)departmentsCommandBarField.Clone();
		}
		if (navigationPaneField != null)
		{
			masterNavigation.NavigationPane = (NavigationPaneDefinition)navigationPaneField.Clone();
		}
		return masterNavigation;
	}
}
