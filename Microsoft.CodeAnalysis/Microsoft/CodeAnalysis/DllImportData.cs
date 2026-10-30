using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public sealed class DllImportData : IPlatformInvokeInformation
{
	private readonly string _moduleName;

	private readonly string _entryPointName;

	private readonly MethodImportAttributes _flags;

	public string ModuleName => _moduleName;

	public string EntryPointName => _entryPointName;

	MethodImportAttributes IPlatformInvokeInformation.Flags => _flags;

	public bool ExactSpelling => (_flags & MethodImportAttributes.ExactSpelling) != 0;

	public CharSet CharacterSet => (_flags & MethodImportAttributes.CharSetAuto) switch
	{
		MethodImportAttributes.CharSetAnsi => CharSet.Ansi, 
		MethodImportAttributes.CharSetUnicode => CharSet.Unicode, 
		MethodImportAttributes.CharSetAuto => CharSet.Auto, 
		MethodImportAttributes.None => CharSet.None, 
		_ => throw ExceptionUtilities.UnexpectedValue(_flags), 
	};

	public bool SetLastError => (_flags & MethodImportAttributes.SetLastError) != 0;

	public System.Runtime.InteropServices.CallingConvention CallingConvention => (_flags & MethodImportAttributes.CallingConventionMask) switch
	{
		MethodImportAttributes.CallingConventionCDecl => System.Runtime.InteropServices.CallingConvention.Cdecl, 
		MethodImportAttributes.CallingConventionStdCall => System.Runtime.InteropServices.CallingConvention.StdCall, 
		MethodImportAttributes.CallingConventionThisCall => System.Runtime.InteropServices.CallingConvention.ThisCall, 
		MethodImportAttributes.CallingConventionFastCall => System.Runtime.InteropServices.CallingConvention.FastCall, 
		_ => System.Runtime.InteropServices.CallingConvention.Winapi, 
	};

	public bool? BestFitMapping => (_flags & MethodImportAttributes.BestFitMappingMask) switch
	{
		MethodImportAttributes.BestFitMappingEnable => (bool?)true, 
		MethodImportAttributes.BestFitMappingDisable => false, 
		_ => null, 
	};

	public bool? ThrowOnUnmappableCharacter => (_flags & MethodImportAttributes.ThrowOnUnmappableCharMask) switch
	{
		MethodImportAttributes.ThrowOnUnmappableCharEnable => (bool?)true, 
		MethodImportAttributes.ThrowOnUnmappableCharDisable => false, 
		_ => null, 
	};

	internal DllImportData(string moduleName, string entryPointName, MethodImportAttributes flags)
	{
		_moduleName = moduleName;
		_entryPointName = entryPointName;
		_flags = flags;
	}

	internal static MethodImportAttributes MakeFlags(bool exactSpelling, CharSet charSet, bool setLastError, System.Runtime.InteropServices.CallingConvention callingConvention, bool? useBestFit, bool? throwOnUnmappable)
	{
		MethodImportAttributes methodImportAttributes = MethodImportAttributes.None;
		if (exactSpelling)
		{
			methodImportAttributes |= MethodImportAttributes.ExactSpelling;
		}
		switch (charSet)
		{
		case CharSet.Ansi:
			methodImportAttributes |= MethodImportAttributes.CharSetAnsi;
			break;
		case CharSet.Unicode:
			methodImportAttributes |= MethodImportAttributes.CharSetUnicode;
			break;
		case CharSet.Auto:
			methodImportAttributes |= MethodImportAttributes.CharSetAuto;
			break;
		}
		if (setLastError)
		{
			methodImportAttributes |= MethodImportAttributes.SetLastError;
		}
		methodImportAttributes = callingConvention switch
		{
			System.Runtime.InteropServices.CallingConvention.Cdecl => methodImportAttributes | MethodImportAttributes.CallingConventionCDecl, 
			System.Runtime.InteropServices.CallingConvention.StdCall => methodImportAttributes | MethodImportAttributes.CallingConventionStdCall, 
			System.Runtime.InteropServices.CallingConvention.ThisCall => methodImportAttributes | MethodImportAttributes.CallingConventionThisCall, 
			System.Runtime.InteropServices.CallingConvention.FastCall => methodImportAttributes | MethodImportAttributes.CallingConventionFastCall, 
			_ => methodImportAttributes | MethodImportAttributes.CallingConventionWinApi, 
		};
		if (throwOnUnmappable.HasValue)
		{
			methodImportAttributes = ((!throwOnUnmappable.Value) ? (methodImportAttributes | MethodImportAttributes.ThrowOnUnmappableCharDisable) : (methodImportAttributes | MethodImportAttributes.ThrowOnUnmappableCharEnable));
		}
		if (useBestFit.HasValue)
		{
			methodImportAttributes = ((!useBestFit.Value) ? (methodImportAttributes | MethodImportAttributes.BestFitMappingDisable) : (methodImportAttributes | MethodImportAttributes.BestFitMappingEnable));
		}
		return methodImportAttributes;
	}
}
