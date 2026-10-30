using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DependentProperty
{
	internal string Name { get; private set; }

	internal ISet<string> AllowedValues { get; private set; }

	internal DependentProperty(string dependentPropertyValue)
	{
		Name = string.Empty;
		AllowedValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		ParseDependantPropertyValue(dependentPropertyValue);
	}

	private void ParseDependantPropertyValue(string dependentPropertyValue)
	{
		StringBuilder stringBuilder = new StringBuilder(dependentPropertyValue.Length);
		int i;
		for (i = 0; i < dependentPropertyValue.Length && dependentPropertyValue[i] != '('; i++)
		{
			stringBuilder.Append(dependentPropertyValue[i]);
		}
		Name = stringBuilder.ToString();
		for (int j = i + 1; j < dependentPropertyValue.Length; j++)
		{
			StringBuilder stringBuilder2 = new StringBuilder(dependentPropertyValue.Length - j + 1);
			for (; dependentPropertyValue[j] != ',' && dependentPropertyValue[j] != ')'; j++)
			{
				stringBuilder2.Append(dependentPropertyValue[j]);
			}
			AllowedValues.Add(stringBuilder2.ToString());
		}
	}

	public static bool operator ==(DependentProperty left, DependentProperty right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(DependentProperty signatureHelpInfo1, DependentProperty signatureHelpInfo2)
	{
		return !(signatureHelpInfo1 == signatureHelpInfo2);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		DependentProperty other = (DependentProperty)obj;
		return Equals(other);
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode() ^ AllowedValues.GetHashCode();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.Append(Name);
		if (AllowedValues.Count > 0)
		{
			stringBuilder.Append('(');
			stringBuilder.Append(ConcatenateValues());
			stringBuilder.Append(')');
		}
		return stringBuilder.ToString();
	}

	internal string ConcatenateValues()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		int num = 0;
		foreach (string allowedValue in AllowedValues)
		{
			stringBuilder.Append(allowedValue);
			if (num < AllowedValues.Count - 1)
			{
				stringBuilder.Append(',');
			}
			num++;
		}
		return stringBuilder.ToString();
	}

	public bool Equals(DependentProperty other)
	{
		if (other == null)
		{
			return false;
		}
		if (string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return AllowedValues.SequenceEqual(other.AllowedValues);
		}
		return false;
	}
}
