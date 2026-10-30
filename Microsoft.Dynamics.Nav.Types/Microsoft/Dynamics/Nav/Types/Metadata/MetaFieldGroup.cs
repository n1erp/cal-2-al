using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MetaFieldGroup : MetaBase
{
	public const string DropDownGroupName = "DropDown";

	public const string BrickGroupName = "Brick";

	private const string PropertyCaption = "Caption";

	private const string PropertyCaptionMultiLanguage = "CaptionML";

	private const string PropertyFieldIds = "GroupFields";

	private const string PropertyFieldGroupId = "GroupID";

	private const string PropertyFieldGroupName = "GroupName";

	private int id;

	private string name;

	private string caption;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<int> fieldIds = new List<int>();

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string captionMultiLanguage;

	[DefaultValue(0)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	[DefaultValue("")]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public List<int> FieldIds => fieldIds;

	[DefaultValue("")]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string Caption
	{
		get
		{
			return caption;
		}
		set
		{
			caption = value;
		}
	}

	[XmlIgnore]
	[XmlAttribute]
	public string CaptionMultiLanguage => captionMultiLanguage;

	public MetaFieldGroup()
	{
	}

	public MetaFieldGroup(XmlNode root)
		: base(root)
	{
		Initialize(root);
	}

	public override object Clone()
	{
		MetaFieldGroup metaFieldGroup = (MetaFieldGroup)base.Clone();
		if (fieldIds != null)
		{
			metaFieldGroup.fieldIds = new List<int>(FieldIds);
		}
		return metaFieldGroup;
	}

	private void Initialize(XmlNode root)
	{
		foreach (XmlAttribute attribute in root.Attributes)
		{
			switch (attribute.Name)
			{
			case "GroupFields":
			{
				string[] array = attribute.Value.Replace("Field", "").Split(new char[1] { ',' }, StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					if (int.TryParse(array[i], out var result))
					{
						fieldIds.Add(result);
					}
				}
				break;
			}
			case "GroupID":
				id = MetaBase.Int32Value(attribute.Value);
				break;
			case "GroupName":
				name = attribute.Value;
				break;
			case "Caption":
				caption = attribute.Value;
				break;
			case "CaptionML":
				captionMultiLanguage = attribute.Value;
				break;
			}
		}
	}

	public void ApplyLanguage(string language, string primaryLanguage)
	{
		if (CaptionMultiLanguage != null)
		{
			caption = MultiLanguage.GetLanguageValue(language, primaryLanguage, CaptionMultiLanguage);
		}
	}

	internal void MergeMultiLanguageTexts(MetaTable parent)
	{
		int languageAppGroupId = 0;
		captionMultiLanguage = MetaTable.MultiLanguageTextMerge(captionMultiLanguage, 0, ObjectType.Table, parent.Id, Id, languageAppGroupId);
	}
}
