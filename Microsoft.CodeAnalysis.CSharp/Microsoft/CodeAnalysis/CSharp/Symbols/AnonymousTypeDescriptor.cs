using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal struct AnonymousTypeDescriptor(ImmutableArray<AnonymousTypeField> fields, Location location) : IEquatable<AnonymousTypeDescriptor>
{
	public readonly Location Location = location;

	public readonly ImmutableArray<AnonymousTypeField> Fields = fields;

	public readonly string Key = ComputeKey(fields, (AnonymousTypeField f) => f.Name);

	internal static string ComputeKey<T>(ImmutableArray<T> fields, Func<T, string> getName)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		foreach (T item in fields)
		{
			instance.Builder.Append('|');
			instance.Builder.Append(getName(item));
		}
		return instance.ToStringAndFree();
	}

	[Conditional("DEBUG")]
	internal void AssertIsGood()
	{
		foreach (AnonymousTypeField field in Fields)
		{
			_ = field;
		}
	}

	public bool Equals(AnonymousTypeDescriptor desc)
	{
		return Equals(desc, TypeCompareKind.ConsiderEverything);
	}

	internal bool Equals(AnonymousTypeDescriptor other, TypeCompareKind comparison)
	{
		if (Key != other.Key)
		{
			return false;
		}
		ImmutableArray<AnonymousTypeField> fields = Fields;
		int length = fields.Length;
		ImmutableArray<AnonymousTypeField> fields2 = other.Fields;
		for (int i = 0; i < length; i++)
		{
			if (!fields[i].Type.Equals(fields2[i].Type, comparison))
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is AnonymousTypeDescriptor)
		{
			return Equals((AnonymousTypeDescriptor)obj, TypeCompareKind.ConsiderEverything);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Key.GetHashCode();
	}

	internal AnonymousTypeDescriptor WithNewFieldsTypes(ImmutableArray<TypeSymbol> newFieldTypes)
	{
		AnonymousTypeField[] array = new AnonymousTypeField[Fields.Length];
		for (int i = 0; i < array.Length; i++)
		{
			AnonymousTypeField anonymousTypeField = Fields[i];
			array[i] = new AnonymousTypeField(anonymousTypeField.Name, anonymousTypeField.Location, newFieldTypes[i]);
		}
		return new AnonymousTypeDescriptor(array.AsImmutable(), Location);
	}
}
