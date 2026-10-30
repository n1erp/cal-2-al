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
[DataContract(Name = "FCD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class FilterControlDefinition : ControlDataboundDefinition
{
	private ViewDefinition dataItemTableViewField;

	[DataMember(Name = "FCD_2", EmitDefaultValue = false, IsRequired = false)]
	private string reqFilterFieldsField;

	private int filterTableIDField;

	private bool sortingField;

	[DataMember(Name = "FCD_1", EmitDefaultValue = false, IsRequired = false)]
	public ViewDefinition DataItemTableView
	{
		get
		{
			return dataItemTableViewField;
		}
		set
		{
			dataItemTableViewField = value;
		}
	}

	[XmlAttribute]
	public string ReqFilterFields
	{
		get
		{
			return reqFilterFieldsField;
		}
		set
		{
			reqFilterFieldsField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "FCD_3", EmitDefaultValue = false, IsRequired = false)]
	public int FilterTableID
	{
		get
		{
			return filterTableIDField;
		}
		set
		{
			filterTableIDField = value;
			FilterTableIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool FilterTableIDSpecified
	{
		get
		{
			return GetSpecified(12);
		}
		set
		{
			SetSpecified(12, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "FCD_4", EmitDefaultValue = false, IsRequired = false)]
	public bool Sorting
	{
		get
		{
			return sortingField;
		}
		set
		{
			sortingField = value;
			SortingSpecified = true;
		}
	}

	[XmlIgnore]
	public bool SortingSpecified
	{
		get
		{
			return GetSpecified(13);
		}
		set
		{
			SetSpecified(13, value);
		}
	}

	protected override int MaxSpecifiedBits => 14;

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (dataItemTableViewField != null)
		{
			dataItemTableViewField.SetLanguage(language, primaryLanguage);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (dataItemTableViewField != null)
		{
			dataItemTableViewField.MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (dataItemTableViewField != null)
		{
			return dataItemTableViewField.Find(match);
		}
		return elementDefinition;
	}

	public override void FindAllCore(List<ElementDefinition> result, Predicate<ElementDefinition> match)
	{
		base.FindAllCore(result, match);
		if (dataItemTableViewField != null)
		{
			dataItemTableViewField.FindAllCore(result, match);
		}
	}

	public override object Clone()
	{
		FilterControlDefinition filterControlDefinition = (FilterControlDefinition)base.Clone();
		if (dataItemTableViewField != null)
		{
			filterControlDefinition.DataItemTableView = (ViewDefinition)dataItemTableViewField.Clone();
		}
		return filterControlDefinition;
	}
}
