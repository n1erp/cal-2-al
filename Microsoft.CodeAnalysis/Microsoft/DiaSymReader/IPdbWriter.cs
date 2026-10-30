using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.DiaSymReader;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("98ECEE1E-752D-11d3-8D56-00C04F680B2B")]
[SuppressUnmanagedCodeSecurity]
internal interface IPdbWriter
{
	int __SetPath();

	int __OpenMod();

	int __CloseMod();

	int __GetPath();

	void GetSignatureAge(out uint sig, out int age);
}
