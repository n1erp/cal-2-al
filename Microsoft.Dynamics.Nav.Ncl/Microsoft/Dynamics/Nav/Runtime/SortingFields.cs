using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SortingFields
{
	private readonly SortingFieldList definedSortingFields;

	private readonly SortingFieldList actualSortingFields;

	public SortingFieldList DefinedSortingFields
	{
		[DebuggerStepThrough]
		get
		{
			return definedSortingFields;
		}
	}

	public SortingFieldList ActualSortingFields
	{
		[DebuggerStepThrough]
		get
		{
			return actualSortingFields;
		}
	}

	public SortingFields(SortingFieldList definedSortingFields, SortingFieldList actualSortingFields)
	{
		this.definedSortingFields = definedSortingFields;
		this.actualSortingFields = actualSortingFields;
	}

	public SortingFields(FieldList definedSortingFields, FieldList actualSortingFields, SortOrder sortOrder)
	{
		this.definedSortingFields = SortingFieldList.Create(definedSortingFields, sortOrder);
		this.actualSortingFields = SortingFieldList.Create(actualSortingFields, sortOrder);
	}

	public SortingFields CreateNewSortingFieldsWithAllFieldsSetToOrder(SortOrder order)
	{
		return new SortingFields(definedSortingFields.DuplicateWithNewOrder(order), actualSortingFields.DuplicateWithNewOrder(order));
	}

	public SortingFields CreateNewSortingWithSingleFieldSetToOrder(NCLMetaField fieldToChange, SortOrder order)
	{
		return new SortingFields(definedSortingFields.CreateNewWithSingleFieldSetToOrder(fieldToChange, order), actualSortingFields.CreateNewWithSingleFieldSetToOrder(fieldToChange, order));
	}

	public override int GetHashCode()
	{
		return actualSortingFields.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is SortingFields sortingFields))
		{
			return false;
		}
		return actualSortingFields.Equals(sortingFields.actualSortingFields);
	}
}
