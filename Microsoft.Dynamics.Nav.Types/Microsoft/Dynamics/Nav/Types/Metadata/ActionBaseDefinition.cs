using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlRoot(ElementName = "Actions", Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects", IsNullable = false)]
[XmlInclude(typeof(ActionDefinition))]
[XmlInclude(typeof(ActionContainerPlaceHolder))]
[XmlInclude(typeof(ActionGroupBaseDefinition))]
[XmlInclude(typeof(NavigationGroupDefinition))]
[XmlInclude(typeof(ActionGroupDefinition))]
[XmlInclude(typeof(SeparatorDefinition))]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[KnownType(typeof(ActionDefinition))]
[KnownType(typeof(ActionContainerPlaceHolder))]
[KnownType(typeof(ActionGroupBaseDefinition))]
[KnownType(typeof(NavigationGroupDefinition))]
[KnownType(typeof(ActionGroupDefinition))]
[KnownType(typeof(SeparatorDefinition))]
[DataContract(Name = "ABD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class ActionBaseDefinition : ControlBaseDefinition
{
	[DataMember(Name = "ABD_1", EmitDefaultValue = false, IsRequired = false)]
	private string imageField;

	private bool isIconBigField;

	internal PageActionData PageActionData { get; set; }

	[XmlAttribute]
	public string Image
	{
		get
		{
			return imageField;
		}
		set
		{
			imageField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "ABD_2", EmitDefaultValue = false, IsRequired = false)]
	public bool IsIconBig
	{
		get
		{
			return isIconBigField;
		}
		set
		{
			isIconBigField = value;
			IsIconBigSpecified = true;
		}
	}

	[XmlIgnore]
	public bool IsIconBigSpecified
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

	protected override int MaxSpecifiedBits => 10;

	protected override ControlType ComponentType()
	{
		return ControlType.ActionItem;
	}

	protected override int ComponentId()
	{
		return ID;
	}

	public override ElementDefinition Find(Predicate<ElementDefinition> match)
	{
		if (match(this))
		{
			return this;
		}
		return base.Find(match);
	}

	public override object Clone()
	{
		return (ActionBaseDefinition)base.Clone();
	}

	public override void SolveName(MetaField field)
	{
		if (string.IsNullOrEmpty(Name))
		{
			if (!string.IsNullOrEmpty(CaptionML))
			{
				string languageValue = MultiLanguage.GetLanguageValue("ENU", null, CaptionML);
				if (!string.IsNullOrEmpty(languageValue))
				{
					Name = languageValue;
				}
			}
			if (string.IsNullOrEmpty(Name))
			{
				Name = string.Format(CultureInfo.InvariantCulture, "Action{0}", ID);
			}
		}
		CachedALIdentifier = Name;
	}
}
