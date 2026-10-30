using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Cci;

internal static class Constants
{
	public const CharSet CharSet_None = CharSet.None;

	public const CharSet CharSet_Auto = CharSet.Auto;

	public const System.Runtime.InteropServices.CallingConvention CallingConvention_FastCall = System.Runtime.InteropServices.CallingConvention.FastCall;

	public const UnmanagedType UnmanagedType_CustomMarshaler = UnmanagedType.CustomMarshaler;

	public const UnmanagedType UnmanagedType_IDispatch = UnmanagedType.IDispatch;

	public const UnmanagedType UnmanagedType_SafeArray = UnmanagedType.SafeArray;

	public const UnmanagedType UnmanagedType_VBByRefStr = UnmanagedType.VBByRefStr;

	public const UnmanagedType UnmanagedType_AnsiBStr = UnmanagedType.AnsiBStr;

	public const UnmanagedType UnmanagedType_TBStr = UnmanagedType.TBStr;

	public const ComInterfaceType ComInterfaceType_InterfaceIsDual = ComInterfaceType.InterfaceIsDual;

	public const ComInterfaceType ComInterfaceType_InterfaceIsIDispatch = ComInterfaceType.InterfaceIsIDispatch;

	public const ClassInterfaceType ClassInterfaceType_AutoDispatch = ClassInterfaceType.AutoDispatch;

	public const ClassInterfaceType ClassInterfaceType_AutoDual = ClassInterfaceType.AutoDual;

	public const int CompilationRelaxations_NoStringInterning = 8;

	public const TypeAttributes TypeAttributes_TypeForwarder = (TypeAttributes)2097152;
}
