using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.DiaSymReader;

[ComImport]
[Guid("0000000c-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUnsafeComStream
{
	unsafe void Read(byte* pv, int cb, int* pcbRead);

	unsafe void Write(byte* pv, int cb, int* pcbWritten);

	unsafe void Seek(long dlibMove, int dwOrigin, long* plibNewPosition);

	void SetSize(long libNewSize);

	unsafe void CopyTo(IStream pstm, long cb, int* pcbRead, int* pcbWritten);

	void Commit(int grfCommitFlags);

	void Revert();

	void LockRegion(long libOffset, long cb, int dwLockType);

	void UnlockRegion(long libOffset, long cb, int dwLockType);

	void Stat(out STATSTG pstatstg, int grfStatFlag);

	void Clone(out IStream ppstm);
}
