using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;

public struct ClassifiedSpan : IEquatable<ClassifiedSpan>
{
	public string ClassificationType { get; }

	public TextSpan TextSpan { get; }

	public ClassifiedSpan(string classificationType, TextSpan textSpan)
		: this(textSpan, classificationType)
	{
	}

	public ClassifiedSpan(TextSpan textSpan, string classificationType)
	{
		this = default;
		ClassificationType = classificationType;
		TextSpan = textSpan;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(ClassificationType.GetHashCode(), TextSpan.GetHashCode());
	}

	public override bool Equals(object obj)
	{
		if (obj is ClassifiedSpan)
		{
			return Equals((ClassifiedSpan)obj);
		}
		return false;
	}

	public bool Equals(ClassifiedSpan other)
	{
		if (ClassificationType == other.ClassificationType)
		{
			return TextSpan == other.TextSpan;
		}
		return false;
	}

	public static bool operator ==(ClassifiedSpan left, ClassifiedSpan right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(ClassifiedSpan left, ClassifiedSpan right)
	{
		return !object.Equals(left, right);
	}
}
