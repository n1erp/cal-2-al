using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace System.Reflection.Internal;

internal static class MemoryMapLightUp
{
	private static Type s_lazyMemoryMappedFileType;

	private static Type s_lazyMemoryMappedViewAccessorType;

	private static Type s_lazyMemoryMappedFileAccessType;

	private static Type s_lazyMemoryMappedFileSecurityType;

	private static Type s_lazyHandleInheritabilityType;

	private static MethodInfo s_lazyCreateFromFile;

	private static MethodInfo s_lazyCreateFromFileClassic;

	private static MethodInfo s_lazyCreateViewAccessor;

	private static PropertyInfo s_lazySafeMemoryMappedViewHandle;

	private static PropertyInfo s_lazyPointerOffset;

	private static FieldInfo s_lazyInternalViewField;

	private static PropertyInfo s_lazyInternalPointerOffset;

	private static readonly object s_MemoryMappedFileAccess_Read = 1;

	private static readonly object s_HandleInheritability_None = 0;

	private static readonly object s_LongZero = 0L;

	private static readonly object s_True = true;

	private static bool? s_lazyIsAvailable;

	internal static bool IsAvailable
	{
		get
		{
			if (!s_lazyIsAvailable.HasValue)
			{
				s_lazyIsAvailable = TryLoadTypes();
			}
			return s_lazyIsAvailable.Value;
		}
	}

	private static bool TryLoadType(string typeName, string modernAssembly, string classicAssembly, out Type type)
	{
		type = LightUpHelper.GetType(typeName, modernAssembly, classicAssembly);
		return (object)type != null;
	}

	private static bool TryLoadTypes()
	{
		TryLoadType("System.IO.MemoryMappedFiles.MemoryMappedFileSecurity", "System.IO.MemoryMappedFiles, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", out s_lazyMemoryMappedFileSecurityType);
		if ((object)FileStreamReadLightUp.FileStreamType.Value != null && TryLoadType("System.IO.MemoryMappedFiles.MemoryMappedFile", "System.IO.MemoryMappedFiles, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", out s_lazyMemoryMappedFileType) && TryLoadType("System.IO.MemoryMappedFiles.MemoryMappedViewAccessor", "System.IO.MemoryMappedFiles, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", out s_lazyMemoryMappedViewAccessorType) && TryLoadType("System.IO.MemoryMappedFiles.MemoryMappedFileAccess", "System.IO.MemoryMappedFiles, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", out s_lazyMemoryMappedFileAccessType) && TryLoadType("System.IO.HandleInheritability", "System.Runtime.Handles, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", out s_lazyHandleInheritabilityType))
		{
			return TryLoadMembers();
		}
		return false;
	}

	private static bool TryLoadMembers()
	{
		s_lazyCreateFromFile = LightUpHelper.GetMethod(s_lazyMemoryMappedFileType, "CreateFromFile", FileStreamReadLightUp.FileStreamType.Value, typeof(string), typeof(long), s_lazyMemoryMappedFileAccessType, s_lazyHandleInheritabilityType, typeof(bool));
		if ((object)s_lazyCreateFromFile == null)
		{
			if ((object)s_lazyMemoryMappedFileSecurityType != null)
			{
				s_lazyCreateFromFileClassic = LightUpHelper.GetMethod(s_lazyMemoryMappedFileType, "CreateFromFile", FileStreamReadLightUp.FileStreamType.Value, typeof(string), typeof(long), s_lazyMemoryMappedFileAccessType, s_lazyMemoryMappedFileSecurityType, s_lazyHandleInheritabilityType, typeof(bool));
			}
			if ((object)s_lazyCreateFromFileClassic == null)
			{
				return false;
			}
		}
		s_lazyCreateViewAccessor = LightUpHelper.GetMethod(s_lazyMemoryMappedFileType, "CreateViewAccessor", typeof(long), typeof(long), s_lazyMemoryMappedFileAccessType);
		if ((object)s_lazyCreateViewAccessor == null)
		{
			return false;
		}
		s_lazySafeMemoryMappedViewHandle = s_lazyMemoryMappedViewAccessorType.GetTypeInfo().GetDeclaredProperty("SafeMemoryMappedViewHandle");
		if ((object)s_lazySafeMemoryMappedViewHandle == null)
		{
			return false;
		}
		s_lazyPointerOffset = s_lazyMemoryMappedViewAccessorType.GetTypeInfo().GetDeclaredProperty("PointerOffset");
		if ((object)s_lazyPointerOffset == null)
		{
			s_lazyInternalViewField = s_lazyMemoryMappedViewAccessorType.GetTypeInfo().GetDeclaredField("m_view");
			if ((object)s_lazyInternalViewField == null)
			{
				return false;
			}
			s_lazyInternalPointerOffset = s_lazyInternalViewField.FieldType.GetTypeInfo().GetDeclaredProperty("PointerOffset");
			if ((object)s_lazyInternalPointerOffset == null)
			{
				return false;
			}
		}
		return true;
	}

	internal static IDisposable CreateMemoryMap(Stream stream)
	{
		try
		{
			if ((object)s_lazyCreateFromFile != null)
			{
				return (IDisposable)s_lazyCreateFromFile.Invoke(null, new object[6] { stream, null, s_LongZero, s_MemoryMappedFileAccess_Read, s_HandleInheritability_None, s_True });
			}
			return (IDisposable)s_lazyCreateFromFileClassic.Invoke(null, new object[7] { stream, null, s_LongZero, s_MemoryMappedFileAccess_Read, null, s_HandleInheritability_None, s_True });
		}
		catch (MemberAccessException)
		{
			s_lazyIsAvailable = false;
			return null;
		}
		catch (InvalidOperationException)
		{
			s_lazyIsAvailable = false;
			return null;
		}
		catch (TargetInvocationException ex3)
		{
			ExceptionDispatchInfo.Capture(ex3.InnerException).Throw();
			throw;
		}
	}

	internal static IDisposable CreateViewAccessor(object memoryMap, long start, int size)
	{
		try
		{
			return (IDisposable)s_lazyCreateViewAccessor.Invoke(memoryMap, new object[3]
			{
				start,
				(long)size,
				s_MemoryMappedFileAccess_Read
			});
		}
		catch (MemberAccessException)
		{
			s_lazyIsAvailable = false;
			return null;
		}
		catch (InvalidOperationException)
		{
			s_lazyIsAvailable = false;
			return null;
		}
		catch (TargetInvocationException ex3) when (ex3.InnerException is UnauthorizedAccessException)
		{
			throw new IOException(ex3.InnerException.Message, ex3.InnerException);
		}
		catch (TargetInvocationException ex4)
		{
			ExceptionDispatchInfo.Capture(ex4.InnerException).Throw();
			throw;
		}
	}

	internal static bool TryGetSafeBufferAndPointerOffset(object accessor, out SafeBuffer safeBuffer, out long offset)
	{
		safeBuffer = (SafeBuffer)s_lazySafeMemoryMappedViewHandle.GetValue(accessor);
		offset = 0L;
		try
		{
			if ((object)s_lazyPointerOffset != null)
			{
				offset = (long)s_lazyPointerOffset.GetValue(accessor);
			}
			else
			{
				object value = s_lazyInternalViewField.GetValue(accessor);
				offset = (long)s_lazyInternalPointerOffset.GetValue(value);
			}
			return true;
		}
		catch (MemberAccessException)
		{
			s_lazyIsAvailable = false;
			return false;
		}
		catch (InvalidOperationException)
		{
			s_lazyIsAvailable = false;
			return false;
		}
		catch (TargetInvocationException ex3)
		{
			ExceptionDispatchInfo.Capture(ex3.InnerException).Throw();
			throw;
		}
	}
}
