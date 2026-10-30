using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "ROD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RelatedObjectDefinition : ElementDefinition
{
	private AccessByObjectType objectTypeField;

	private int objectIDField;

	private int permissionMaskField;

	private RelationType relatedByField;

	[XmlAttribute]
	[DataMember(Name = "ROD_1", EmitDefaultValue = false, IsRequired = false)]
	public AccessByObjectType ObjectType
	{
		get
		{
			return objectTypeField;
		}
		set
		{
			objectTypeField = value;
			ObjectTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ObjectTypeSpecified
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
	[DataMember(Name = "ROD_2", EmitDefaultValue = false, IsRequired = false)]
	public int ObjectID
	{
		get
		{
			return objectIDField;
		}
		set
		{
			objectIDField = value;
			ObjectIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ObjectIDSpecified
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
	[DataMember(Name = "ROD_3", EmitDefaultValue = false, IsRequired = false)]
	public int PermissionMask
	{
		get
		{
			return permissionMaskField;
		}
		set
		{
			permissionMaskField = value;
			PermissionMaskSpecified = true;
		}
	}

	[XmlIgnore]
	public bool PermissionMaskSpecified
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
	[DataMember(Name = "ROD_4", EmitDefaultValue = false, IsRequired = false)]
	public RelationType RelatedBy
	{
		get
		{
			return relatedByField;
		}
		set
		{
			relatedByField = value;
			RelatedBySpecified = true;
		}
	}

	[XmlIgnore]
	public bool RelatedBySpecified
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

	protected override int MaxSpecifiedBits => 4;

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
		return (RelatedObjectDefinition)base.Clone();
	}
}
