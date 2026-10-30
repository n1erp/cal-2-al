namespace Microsoft.Dynamics.Nav.MetaMetaModel;

public class ElementRelation
{
	private readonly ElementType parentType;

	private readonly ElementType childType;

	private MultiplicityType multiplicity;

	private string containerName;

	private int attributeId;

	public ElementType ParentType => parentType;

	public ElementType ChildType => childType;

	public string ContainerName => containerName;

	public int AttributeId => attributeId;

	public string Name
	{
		get
		{
			if (!string.IsNullOrEmpty(containerName))
			{
				return containerName;
			}
			return ChildType.ToString();
		}
	}

	public MultiplicityType Multiplicity => multiplicity;

	public bool MultiplicityIsSingle
	{
		get
		{
			if (multiplicity != MultiplicityType.ZeroToOne)
			{
				return multiplicity == MultiplicityType.One;
			}
			return true;
		}
	}

	public bool MultiplicityIsMany
	{
		get
		{
			if (multiplicity != MultiplicityType.ZeroToMany)
			{
				return multiplicity == MultiplicityType.OneToMany;
			}
			return true;
		}
	}

	public ElementTypeInfo Parent => ElementTypeInfo.Lookup.GetValue(parentType);

	public ElementTypeInfo Child => ElementTypeInfo.Lookup.GetValue(childType);

	internal ElementRelation(ElementType parentType, ElementType childType, MultiplicityType multiplicity = MultiplicityType.ZeroToOne, string containerName = null, int attributeId = 0)
	{
		this.parentType = parentType;
		this.childType = childType;
		this.multiplicity = multiplicity;
		this.containerName = containerName;
		this.attributeId = attributeId;
	}

	public override string ToString()
	{
		return ChildType.ToString();
	}
}
