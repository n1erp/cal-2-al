using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis;

internal static class TypeAttributesExtensions
{
	public static bool IsInterface(this TypeAttributes flags)
	{
		return (flags & TypeAttributes.ClassSemanticsMask) != 0;
	}

	public static bool IsWindowsRuntime(this TypeAttributes flags)
	{
		return (flags & TypeAttributes.WindowsRuntime) != 0;
	}

	public static bool IsPublic(this TypeAttributes flags)
	{
		return (flags & TypeAttributes.Public) != 0;
	}

	public static bool IsSpecialName(this TypeAttributes flags)
	{
		return (flags & TypeAttributes.SpecialName) != 0;
	}

	internal static CharSet ToCharSet(this TypeAttributes flags)
	{
		return (flags & TypeAttributes.StringFormatMask) switch
		{
			TypeAttributes.AutoClass => CharSet.Auto, 
			TypeAttributes.NotPublic => CharSet.Ansi, 
			TypeAttributes.UnicodeClass => CharSet.Unicode, 
			_ => (CharSet)0, 
		};
	}
}
