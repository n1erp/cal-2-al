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
[DataContract(Name = "CAIMD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ControlAddInManifestDefinition : ElementDefinition
{
	private List<ControlAddInResourceDefinition> resourcesField;

	[DataMember(Name = "CAIMD_2", EmitDefaultValue = false, IsRequired = false)]
	private List<string> scriptUrlsField;

	[DataMember(Name = "CAIMD_4", EmitDefaultValue = false, IsRequired = false)]
	private List<string> styleSheetUrlsField;

	private int requestedHeightField;

	private int requestedWidthField;

	private int minimumHeightField;

	private int minimumWidthField;

	private int maximumHeightField;

	private int maximumWidthField;

	private bool verticalShrinkField;

	private bool horizontalShrinkField;

	private bool verticalStretchField;

	private bool horizontalStretchField;

	private bool allowCaptionField;

	[DataMember(Name = "CAIMD_17", EmitDefaultValue = false, IsRequired = false)]
	private string scriptField;

	[DataMember(Name = "CAIMD_18", EmitDefaultValue = false, IsRequired = false)]
	private string recreateScriptField;

	[DataMember(Name = "CAIMD_19", EmitDefaultValue = false, IsRequired = false)]
	private string refreshScriptField;

	[DataMember(Name = "CAIMD_20", EmitDefaultValue = false, IsRequired = false)]
	private string versionField;

	[DataMember(Name = "CAIMD_21", EmitDefaultValue = false, IsRequired = false)]
	private string errorMessageField;

	[XmlElement("Resources")]
	[DataMember(Name = "CAIMD_1", EmitDefaultValue = false, IsRequired = false)]
	public List<ControlAddInResourceDefinition> Resources
	{
		get
		{
			return resourcesField;
		}
		set
		{
			resourcesField = value;
		}
	}

	[XmlElement("ScriptUrls")]
	[DataMember(Name = "CAIMD_3", EmitDefaultValue = false, IsRequired = false)]
	public List<string> ScriptUrls
	{
		get
		{
			return scriptUrlsField;
		}
		set
		{
			scriptUrlsField = value;
		}
	}

	[XmlElement("StyleSheetUrls")]
	[DataMember(Name = "CAIMD_5", EmitDefaultValue = false, IsRequired = false)]
	public List<string> StyleSheetUrls
	{
		get
		{
			return styleSheetUrlsField;
		}
		set
		{
			styleSheetUrlsField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CAIMD_6", EmitDefaultValue = false, IsRequired = false)]
	public int RequestedHeight
	{
		get
		{
			return requestedHeightField;
		}
		set
		{
			requestedHeightField = value;
			RequestedHeightSpecified = true;
		}
	}

	[XmlIgnore]
	public bool RequestedHeightSpecified
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
	[DataMember(Name = "CAIMD_7", EmitDefaultValue = false, IsRequired = false)]
	public int RequestedWidth
	{
		get
		{
			return requestedWidthField;
		}
		set
		{
			requestedWidthField = value;
			RequestedWidthSpecified = true;
		}
	}

	[XmlIgnore]
	public bool RequestedWidthSpecified
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
	[DataMember(Name = "CAIMD_8", EmitDefaultValue = false, IsRequired = false)]
	public int MinimumHeight
	{
		get
		{
			return minimumHeightField;
		}
		set
		{
			minimumHeightField = value;
			MinimumHeightSpecified = true;
		}
	}

	[XmlIgnore]
	public bool MinimumHeightSpecified
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
	[DataMember(Name = "CAIMD_9", EmitDefaultValue = false, IsRequired = false)]
	public int MinimumWidth
	{
		get
		{
			return minimumWidthField;
		}
		set
		{
			minimumWidthField = value;
			MinimumWidthSpecified = true;
		}
	}

	[XmlIgnore]
	public bool MinimumWidthSpecified
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
	[DataMember(Name = "CAIMD_10", EmitDefaultValue = false, IsRequired = false)]
	public int MaximumHeight
	{
		get
		{
			return maximumHeightField;
		}
		set
		{
			maximumHeightField = value;
			MaximumHeightSpecified = true;
		}
	}

	[XmlIgnore]
	public bool MaximumHeightSpecified
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
	[DataMember(Name = "CAIMD_11", EmitDefaultValue = false, IsRequired = false)]
	public int MaximumWidth
	{
		get
		{
			return maximumWidthField;
		}
		set
		{
			maximumWidthField = value;
			MaximumWidthSpecified = true;
		}
	}

	[XmlIgnore]
	public bool MaximumWidthSpecified
	{
		get
		{
			return GetSpecified(5);
		}
		set
		{
			SetSpecified(5, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CAIMD_12", EmitDefaultValue = false, IsRequired = false)]
	public bool VerticalShrink
	{
		get
		{
			return verticalShrinkField;
		}
		set
		{
			verticalShrinkField = value;
			VerticalShrinkSpecified = true;
		}
	}

	[XmlIgnore]
	public bool VerticalShrinkSpecified
	{
		get
		{
			return GetSpecified(6);
		}
		set
		{
			SetSpecified(6, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CAIMD_13", EmitDefaultValue = false, IsRequired = false)]
	public bool HorizontalShrink
	{
		get
		{
			return horizontalShrinkField;
		}
		set
		{
			horizontalShrinkField = value;
			HorizontalShrinkSpecified = true;
		}
	}

	[XmlIgnore]
	public bool HorizontalShrinkSpecified
	{
		get
		{
			return GetSpecified(7);
		}
		set
		{
			SetSpecified(7, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CAIMD_14", EmitDefaultValue = false, IsRequired = false)]
	public bool VerticalStretch
	{
		get
		{
			return verticalStretchField;
		}
		set
		{
			verticalStretchField = value;
			VerticalStretchSpecified = true;
		}
	}

	[XmlIgnore]
	public bool VerticalStretchSpecified
	{
		get
		{
			return GetSpecified(8);
		}
		set
		{
			SetSpecified(8, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CAIMD_15", EmitDefaultValue = false, IsRequired = false)]
	public bool HorizontalStretch
	{
		get
		{
			return horizontalStretchField;
		}
		set
		{
			horizontalStretchField = value;
			HorizontalStretchSpecified = true;
		}
	}

	[XmlIgnore]
	public bool HorizontalStretchSpecified
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
	[DataMember(Name = "CAIMD_16", EmitDefaultValue = false, IsRequired = false)]
	public bool AllowCaption
	{
		get
		{
			return allowCaptionField;
		}
		set
		{
			allowCaptionField = value;
			AllowCaptionSpecified = true;
		}
	}

	[XmlIgnore]
	public bool AllowCaptionSpecified
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
	public string Script
	{
		get
		{
			return scriptField;
		}
		set
		{
			scriptField = value;
		}
	}

	[XmlAttribute]
	public string RecreateScript
	{
		get
		{
			return recreateScriptField;
		}
		set
		{
			recreateScriptField = value;
		}
	}

	[XmlAttribute]
	public string RefreshScript
	{
		get
		{
			return refreshScriptField;
		}
		set
		{
			refreshScriptField = value;
		}
	}

	[XmlAttribute]
	public string Version
	{
		get
		{
			return versionField;
		}
		set
		{
			versionField = value;
		}
	}

	[XmlAttribute]
	public string ErrorMessage
	{
		get
		{
			return errorMessageField;
		}
		set
		{
			errorMessageField = value;
		}
	}

	protected override int MaxSpecifiedBits => 11;

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (resourcesField != null)
		{
			for (int i = 0; i < resourcesField.Count; i++)
			{
				resourcesField[i].SetLanguage(language, primaryLanguage);
			}
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (resourcesField != null)
		{
			for (int i = 0; i < resourcesField.Count; i++)
			{
				resourcesField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
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
		if (resourcesField != null)
		{
			for (int i = 0; i < resourcesField.Count; i++)
			{
				elementDefinition = resourcesField[i].Find(match);
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
		if (resourcesField != null)
		{
			for (int i = 0; i < resourcesField.Count; i++)
			{
				resourcesField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		ControlAddInManifestDefinition controlAddInManifestDefinition = (ControlAddInManifestDefinition)base.Clone();
		if (resourcesField != null)
		{
			controlAddInManifestDefinition.Resources = new List<ControlAddInResourceDefinition>();
			for (int i = 0; i < resourcesField.Count; i++)
			{
				controlAddInManifestDefinition.Resources.Add((ControlAddInResourceDefinition)resourcesField[i].Clone());
			}
		}
		if (scriptUrlsField != null)
		{
			controlAddInManifestDefinition.ScriptUrls = new List<string>();
			for (int j = 0; j < scriptUrlsField.Count; j++)
			{
				controlAddInManifestDefinition.ScriptUrls.Add((string)scriptUrlsField[j].Clone());
			}
		}
		if (styleSheetUrlsField != null)
		{
			controlAddInManifestDefinition.StyleSheetUrls = new List<string>();
			for (int k = 0; k < styleSheetUrlsField.Count; k++)
			{
				controlAddInManifestDefinition.StyleSheetUrls.Add((string)styleSheetUrlsField[k].Clone());
			}
		}
		return controlAddInManifestDefinition;
	}
}
