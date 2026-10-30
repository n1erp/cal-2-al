using System.IO;
using System.Runtime.InteropServices;

namespace System.Reflection.Internal;

internal static class FileStreamReadLightUp
{
	internal static Lazy<Type> FileStreamType = new Lazy<Type>(() => LightUpHelper.GetType("System.IO.FileStream", "System.IO.FileSystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken = b03f5f7f11d50a3a", "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));

	internal static Lazy<PropertyInfo> SafeFileHandle = new Lazy<PropertyInfo>(() => FileStreamType.Value.GetTypeInfo().GetDeclaredProperty("SafeFileHandle"));

	internal static bool readFileNotAvailable;

	internal static bool safeFileHandleNotAvailable;

	internal static bool IsFileStream(Stream stream)
	{
		if ((object)FileStreamType.Value == null)
		{
			return false;
		}
		Type type = stream.GetType();
		if ((object)type != FileStreamType.Value)
		{
			return type.GetTypeInfo().IsSubclassOf(FileStreamType.Value);
		}
		return true;
	}

	internal static SafeHandle GetSafeFileHandle(Stream stream)
	{
		if (safeFileHandleNotAvailable)
		{
			return null;
		}
		PropertyInfo value = SafeFileHandle.Value;
		if ((object)value == null)
		{
			safeFileHandleNotAvailable = true;
			return null;
		}
		SafeHandle safeHandle;
		try
		{
			safeHandle = (SafeHandle)value.GetValue(stream);
		}
		catch (MemberAccessException)
		{
			safeFileHandleNotAvailable = true;
			return null;
		}
		catch (InvalidOperationException)
		{
			safeFileHandleNotAvailable = true;
			return null;
		}
		catch (TargetInvocationException)
		{
			return null;
		}
		if (safeHandle != null && safeHandle.IsInvalid)
		{
			return null;
		}
		return safeHandle;
	}

	internal unsafe static bool TryReadFile(Stream stream, byte* buffer, long start, int size)
	{
		if (readFileNotAvailable)
		{
			return false;
		}
		SafeHandle safeFileHandle = GetSafeFileHandle(stream);
		if (safeFileHandle == null)
		{
			return false;
		}
		int bytesRead = 0;
		bool flag;
		try
		{
			flag = ReadFile(safeFileHandle, buffer, size, out bytesRead, IntPtr.Zero);
		}
		catch
		{
			readFileNotAvailable = true;
			return false;
		}
		if (!flag || bytesRead != size)
		{
			return false;
		}
		return true;
	}

	[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal unsafe static extern bool ReadFile(SafeHandle fileHandle, byte* buffer, int byteCount, out int bytesRead, IntPtr overlapped);
}
