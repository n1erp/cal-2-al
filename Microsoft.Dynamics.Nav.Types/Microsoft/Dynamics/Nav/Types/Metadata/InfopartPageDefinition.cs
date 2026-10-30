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
[DataContract(Name = "IPD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class InfopartPageDefinition : InfopartBaseDefinition
{
	private List<FilterDefinition> subFormLinkField;

	private ViewDefinition subFormViewField;

	[DataMember(Name = "IPD_3", EmitDefaultValue = false, IsRequired = false)]
	private string editableField;

	[DataMember(Name = "IPD_4", EmitDefaultValue = false, IsRequired = false)]
	private string showFilterField;

	private int providerIDField;

	private int pagePartIDField;

	private int accessByObjectIDField;

	private AccessByObjectType accessByObjectTypeField;

	private int accessByPermissionMaskField;

	[DataMember(Name = "IPD_10", EmitDefaultValue = false, IsRequired = false)]
	private string applicationAreaField;

	private int runObjectSrcTableField;

	private UpdatePropagation updatePropagationField;

	[DataMember(Name = "IPD_13", EmitDefaultValue = false, IsRequired = false)]
	private string entitySetNameField;

	[DataMember(Name = "IPD_14", EmitDefaultValue = false, IsRequired = false)]
	private string entityNameField;

	[XmlElement("SubFormLink")]
	[DataMember(Name = "IPD_1", EmitDefaultValue = false, IsRequired = false)]
	public List<FilterDefinition> SubFormLink
	{
		get
		{
			return subFormLinkField;
		}
		set
		{
			subFormLinkField = value;
		}
	}

	[DataMember(Name = "IPD_2", EmitDefaultValue = false, IsRequired = false)]
	public ViewDefinition SubFormView
	{
		get
		{
			return subFormViewField;
		}
		set
		{
			subFormViewField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("true")]
	public string Editable
	{
		get
		{
			if (editableField == null)
			{
				return "true";
			}
			return editableField;
		}
		set
		{
			editableField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("true")]
	public string ShowFilter
	{
		get
		{
			if (showFilterField == null)
			{
				return "true";
			}
			return showFilterField;
		}
		set
		{
			showFilterField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "IPD_5", EmitDefaultValue = false, IsRequired = false)]
	public int ProviderID
	{
		get
		{
			return providerIDField;
		}
		set
		{
			providerIDField = value;
			ProviderIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ProviderIDSpecified
	{
		get
		{
			return GetSpecified(9);
		}
		set
		{
			SetSpecified(9, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "IPD_6", EmitDefaultValue = false, IsRequired = false)]
	public int PagePartID
	{
		get
		{
			return pagePartIDField;
		}
		set
		{
			pagePartIDField = value;
			PagePartIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool PagePartIDSpecified
	{
		get
		{
			return GetSpecified(10);
		}
		set
		{
			SetSpecified(10, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "IPD_7", EmitDefaultValue = false, IsRequired = false)]
	public int AccessByObjectID
	{
		get
		{
			return accessByObjectIDField;
		}
		set
		{
			accessByObjectIDField = value;
			AccessByObjectIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool AccessByObjectIDSpecified
	{
		get
		{
			return GetSpecified(11);
		}
		set
		{
			SetSpecified(11, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "IPD_8", EmitDefaultValue = false, IsRequired = false)]
	public AccessByObjectType AccessByObjectType
	{
		get
		{
			return accessByObjectTypeField;
		}
		set
		{
			accessByObjectTypeField = value;
			AccessByObjectTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool AccessByObjectTypeSpecified
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
	[DataMember(Name = "IPD_9", EmitDefaultValue = false, IsRequired = false)]
	public int AccessByPermissionMask
	{
		get
		{
			return accessByPermissionMaskField;
		}
		set
		{
			accessByPermissionMaskField = value;
			AccessByPermissionMaskSpecified = true;
		}
	}

	[XmlIgnore]
	public bool AccessByPermissionMaskSpecified
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

	[XmlAttribute]
	public string ApplicationArea
	{
		get
		{
			return applicationAreaField;
		}
		set
		{
			applicationAreaField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "IPD_11", EmitDefaultValue = false, IsRequired = false)]
	public int RunObjectSrcTable
	{
		get
		{
			return runObjectSrcTableField;
		}
		set
		{
			runObjectSrcTableField = value;
			RunObjectSrcTableSpecified = true;
		}
	}

	[XmlIgnore]
	public bool RunObjectSrcTableSpecified
	{
		get
		{
			return GetSpecified(14);
		}
		set
		{
			SetSpecified(14, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "IPD_12", EmitDefaultValue = false, IsRequired = false)]
	public UpdatePropagation UpdatePropagation
	{
		get
		{
			return updatePropagationField;
		}
		set
		{
			updatePropagationField = value;
			UpdatePropagationSpecified = true;
		}
	}

	[XmlIgnore]
	public bool UpdatePropagationSpecified
	{
		get
		{
			return GetSpecified(15);
		}
		set
		{
			SetSpecified(15, value);
		}
	}

	[XmlAttribute]
	public string EntitySetName
	{
		get
		{
			return entitySetNameField;
		}
		set
		{
			entitySetNameField = value;
		}
	}

	[XmlAttribute]
	public string EntityName
	{
		get
		{
			return entityNameField;
		}
		set
		{
			entityNameField = value;
		}
	}

	protected override int MaxSpecifiedBits => 16;

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		id = PagePartID;
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (subFormLinkField != null)
		{
			for (int i = 0; i < subFormLinkField.Count; i++)
			{
				subFormLinkField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
		if (subFormViewField != null)
		{
			subFormViewField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (subFormLinkField != null)
		{
			for (int i = 0; i < subFormLinkField.Count; i++)
			{
				subFormLinkField[i].SetLanguage(language, primaryLanguage);
			}
		}
		if (subFormViewField != null)
		{
			subFormViewField.SetLanguage(language, primaryLanguage);
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
		if (subFormViewField != null)
		{
			elementDefinition = subFormViewField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (subFormLinkField != null)
		{
			for (int i = 0; i < subFormLinkField.Count; i++)
			{
				elementDefinition = subFormLinkField[i].Find(match);
				if (elementDefinition != null)
				{
					return elementDefinition;
				}
			}
		}
		return elementDefinition;
	}

	public override void FindAllCore(List<ElementDefinition> result, Predicate<ElementDefinition> match)
	{
		base.FindAllCore(result, match);
		if (subFormViewField != null)
		{
			subFormViewField.FindAllCore(result, match);
		}
		if (subFormLinkField != null)
		{
			for (int i = 0; i < subFormLinkField.Count; i++)
			{
				subFormLinkField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		InfopartPageDefinition infopartPageDefinition = (InfopartPageDefinition)base.Clone();
		if (subFormViewField != null)
		{
			infopartPageDefinition.SubFormView = (ViewDefinition)subFormViewField.Clone();
		}
		if (subFormLinkField != null)
		{
			infopartPageDefinition.SubFormLink = new List<FilterDefinition>();
			for (int i = 0; i < subFormLinkField.Count; i++)
			{
				infopartPageDefinition.SubFormLink.Add((FilterDefinition)subFormLinkField[i].Clone());
			}
		}
		return infopartPageDefinition;
	}
}
