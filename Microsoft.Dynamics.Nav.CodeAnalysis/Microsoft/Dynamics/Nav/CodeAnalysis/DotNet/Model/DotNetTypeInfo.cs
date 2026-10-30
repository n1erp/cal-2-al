using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

[Serializable]
[DebuggerDisplay("{FullName}")]
public sealed class DotNetTypeInfo : IEquatable<DotNetTypeInfo>
{
	private string lazyAssemblyQualifiedName;

	private string lazyAssemblyQualifiedTypeName;

	private string lazyRefQualifiedAssemblyQualifiedTypeName;

	public DotNetAssemblyInfo Assembly { get; set; }

	public string DisplayName { get; set; }

	public string TypeName { get; set; }

	public string FullName { get; set; }

	public string AssemblyQualifiedName
	{
		get
		{
			if (lazyAssemblyQualifiedName != null)
			{
				return lazyAssemblyQualifiedName;
			}
			Interlocked.CompareExchange(ref lazyAssemblyQualifiedName, DotNetUtilities.GetAssemblyQualifiedTypeName(Assembly, FullName), null);
			return lazyAssemblyQualifiedName;
		}
	}

	internal string InternalTypeName { get; set; }

	public bool IsValueType { get; set; }

	public bool IsStatic { get; set; }

	public DotNetConstructorInfo[] Constructors { get; set; }

	public DotNetMethodInfo[] Methods { get; set; }

	public DotNetPropertyInfo[] Properties { get; set; }

	public DotNetEnumeratorInfo[] EnumMembers { get; set; }

	public DotNetEventInfo[] Events { get; set; }

	public bool IsEnumerable => EnumeratorElementType != null;

	public DotNetTypeInfo EnumeratorElementType { get; set; }

	public DotNetSpecialType SpecialType { get; set; }

	public bool RequiresCompletion { get; set; } = true;

	public bool IsEnum => EnumMembers.Length != 0;

	public DotNetTypeInfo[] BaseTypes { get; set; }

	public DotNetTypeInfo[] Interfaces { get; set; }

	public bool IsByRef { get; set; }

	public bool IsInterface { get; set; }

	public bool IsControlAddIn => ControlAddInName != null;

	public string ControlAddInName { get; set; }

	private string RefQualifiedAssemblyQualifiedTypeName
	{
		get
		{
			if (lazyRefQualifiedAssemblyQualifiedTypeName != null)
			{
				return lazyRefQualifiedAssemblyQualifiedTypeName;
			}
			Interlocked.CompareExchange(ref lazyRefQualifiedAssemblyQualifiedTypeName, DotNetUtilities.GetAssemblyQualifiedTypeName(Assembly, IsByRef ? TypeName : (TypeName + "&")), null);
			return lazyRefQualifiedAssemblyQualifiedTypeName;
		}
	}

	private string AssemblyQualifiedTypeName
	{
		get
		{
			if (lazyAssemblyQualifiedTypeName != null)
			{
				return lazyAssemblyQualifiedTypeName;
			}
			Interlocked.CompareExchange(ref lazyAssemblyQualifiedTypeName, DotNetUtilities.GetAssemblyQualifiedTypeName(Assembly, TypeName), null);
			return lazyAssemblyQualifiedTypeName;
		}
	}

	public bool Equals(DotNetTypeInfo other)
	{
		if (other == null)
		{
			return false;
		}
		return AssemblyQualifiedName == other.AssemblyQualifiedName;
	}

	public override bool Equals(object obj)
	{
		DotNetTypeInfo other = obj as DotNetTypeInfo;
		return Equals(other);
	}

	public override int GetHashCode()
	{
		return AssemblyQualifiedName.GetHashCode();
	}

	internal bool IsEquivalentToReferenceType(DotNetTypeInfo other)
	{
		if (other == null)
		{
			return false;
		}
		return RefQualifiedAssemblyQualifiedTypeName == other.RefQualifiedAssemblyQualifiedTypeName;
	}

	internal bool IsEquivalentToType(DotNetTypeInfo other)
	{
		if (other == null)
		{
			return false;
		}
		return AssemblyQualifiedTypeName == other.AssemblyQualifiedTypeName;
	}
}
