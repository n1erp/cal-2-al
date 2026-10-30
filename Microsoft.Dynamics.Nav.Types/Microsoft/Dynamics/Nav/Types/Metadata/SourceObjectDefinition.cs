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
[DataContract(Name = "SOD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SourceObjectDefinition : ElementDefinition
{
	private ViewDefinition sourceTableViewField;

	private int sourceTableField;

	private bool insertAllowedField;

	private bool modifyAllowedField;

	private bool linksAllowedField;

	private bool deleteAllowedField;

	private bool showFilterField;

	private bool delayedInsertField;

	private bool multipleNewLinesField;

	private bool saveValuesField;

	private bool autoSplitKeyField;

	private bool sourceTableTemporaryField;

	private bool populateAllFieldsField;

	[DataMember(Name = "SOD_14", EmitDefaultValue = false, IsRequired = false)]
	private string dataCaptionFieldsField;

	[DataMember(Name = "SOD_15", EmitDefaultValue = false, IsRequired = false)]
	private string oDataKeyFieldsField;

	private DataSourceType pageDataSourceField;

	[DataMember(Name = "SOD_17", EmitDefaultValue = false, IsRequired = false)]
	private string indirectPermissionsField;

	[DataMember(Name = "SOD_1", EmitDefaultValue = false, IsRequired = false)]
	public ViewDefinition SourceTableView
	{
		get
		{
			return sourceTableViewField;
		}
		set
		{
			sourceTableViewField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "SOD_2", EmitDefaultValue = false, IsRequired = false)]
	public int SourceTable
	{
		get
		{
			return sourceTableField;
		}
		set
		{
			sourceTableField = value;
			SourceTableSpecified = true;
		}
	}

	[XmlIgnore]
	public bool SourceTableSpecified
	{
		get
		{
			return GetSpecified(0);
		}
		set
		{
			SetSpecified(0, value);
		}
	}

	[XmlAttribute]
	[DefaultValue(true)]
	[DataMember(Name = "SOD_3", EmitDefaultValue = false, IsRequired = false)]
	public bool InsertAllowed
	{
		get
		{
			return insertAllowedField;
		}
		set
		{
			insertAllowedField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(true)]
	[DataMember(Name = "SOD_4", EmitDefaultValue = false, IsRequired = false)]
	public bool ModifyAllowed
	{
		get
		{
			return modifyAllowedField;
		}
		set
		{
			modifyAllowedField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(true)]
	[DataMember(Name = "SOD_5", EmitDefaultValue = false, IsRequired = false)]
	public bool LinksAllowed
	{
		get
		{
			return linksAllowedField;
		}
		set
		{
			linksAllowedField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(true)]
	[DataMember(Name = "SOD_6", EmitDefaultValue = false, IsRequired = false)]
	public bool DeleteAllowed
	{
		get
		{
			return deleteAllowedField;
		}
		set
		{
			deleteAllowedField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "SOD_7", EmitDefaultValue = false, IsRequired = false)]
	public bool ShowFilter
	{
		get
		{
			return showFilterField;
		}
		set
		{
			showFilterField = value;
			ShowFilterSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ShowFilterSpecified
	{
		get
		{
			return GetSpecified(1);
		}
		set
		{
			SetSpecified(1, value);
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "SOD_8", EmitDefaultValue = false, IsRequired = false)]
	public bool DelayedInsert
	{
		get
		{
			return delayedInsertField;
		}
		set
		{
			delayedInsertField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "SOD_9", EmitDefaultValue = false, IsRequired = false)]
	public bool MultipleNewLines
	{
		get
		{
			return multipleNewLinesField;
		}
		set
		{
			multipleNewLinesField = value;
			MultipleNewLinesSpecified = true;
		}
	}

	[XmlIgnore]
	public bool MultipleNewLinesSpecified
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

	[XmlAttribute]
	[DataMember(Name = "SOD_10", EmitDefaultValue = false, IsRequired = false)]
	public bool SaveValues
	{
		get
		{
			return saveValuesField;
		}
		set
		{
			saveValuesField = value;
			SaveValuesSpecified = true;
		}
	}

	[XmlIgnore]
	public bool SaveValuesSpecified
	{
		get
		{
			return GetSpecified(3);
		}
		set
		{
			SetSpecified(3, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "SOD_11", EmitDefaultValue = false, IsRequired = false)]
	public bool AutoSplitKey
	{
		get
		{
			return autoSplitKeyField;
		}
		set
		{
			autoSplitKeyField = value;
			AutoSplitKeySpecified = true;
		}
	}

	[XmlIgnore]
	public bool AutoSplitKeySpecified
	{
		get
		{
			return GetSpecified(4);
		}
		set
		{
			SetSpecified(4, value);
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "SOD_12", EmitDefaultValue = false, IsRequired = false)]
	public bool SourceTableTemporary
	{
		get
		{
			return sourceTableTemporaryField;
		}
		set
		{
			sourceTableTemporaryField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "SOD_13", EmitDefaultValue = false, IsRequired = false)]
	public bool PopulateAllFields
	{
		get
		{
			return populateAllFieldsField;
		}
		set
		{
			populateAllFieldsField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string DataCaptionFields
	{
		get
		{
			if (dataCaptionFieldsField == null)
			{
				return "";
			}
			return dataCaptionFieldsField;
		}
		set
		{
			dataCaptionFieldsField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string ODataKeyFields
	{
		get
		{
			if (oDataKeyFieldsField == null)
			{
				return "";
			}
			return oDataKeyFieldsField;
		}
		set
		{
			oDataKeyFieldsField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(DataSourceType.Table)]
	[DataMember(Name = "SOD_16", EmitDefaultValue = false, IsRequired = false)]
	public DataSourceType PageDataSource
	{
		get
		{
			return pageDataSourceField;
		}
		set
		{
			pageDataSourceField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string IndirectPermissions
	{
		get
		{
			if (indirectPermissionsField == null)
			{
				return "";
			}
			return indirectPermissionsField;
		}
		set
		{
			indirectPermissionsField = value;
		}
	}

	protected override int MaxSpecifiedBits => 5;

	public SourceObjectDefinition()
	{
		insertAllowedField = true;
		modifyAllowedField = true;
		linksAllowedField = true;
		deleteAllowedField = true;
		delayedInsertField = false;
		sourceTableTemporaryField = false;
		populateAllFieldsField = false;
		pageDataSourceField = DataSourceType.Table;
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (sourceTableViewField != null)
		{
			sourceTableViewField.SetLanguage(language, primaryLanguage);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (sourceTableViewField != null)
		{
			sourceTableViewField.MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (sourceTableViewField != null)
		{
			return sourceTableViewField.Find(match);
		}
		return elementDefinition;
	}

	public override void FindAllCore(List<ElementDefinition> result, Predicate<ElementDefinition> match)
	{
		base.FindAllCore(result, match);
		if (sourceTableViewField != null)
		{
			sourceTableViewField.FindAllCore(result, match);
		}
	}

	public override object Clone()
	{
		SourceObjectDefinition sourceObjectDefinition = (SourceObjectDefinition)base.Clone();
		if (sourceTableViewField != null)
		{
			sourceObjectDefinition.SourceTableView = (ViewDefinition)sourceTableViewField.Clone();
		}
		return sourceObjectDefinition;
	}
}
