using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.Types;

public static class NavWindowIdHelper
{
	public static readonly Guid UnknownDefaultId = Guid.Empty;

	public static readonly Guid FormDefaultId = CreateWindowGuid(NavWindowType.Form, NavObjectType.TableData, 0, 0, 0);

	public static readonly Guid DialogDefaultId = CreateWindowGuid(NavWindowType.Dialog, NavObjectType.TableData, 0, 0, 0);

	public static readonly Guid MessageDefaultId = CreateWindowGuid(NavWindowType.Message, NavObjectType.TableData, 0, 0, 0);

	public static readonly Guid ConfirmDefaultId = CreateWindowGuid(NavWindowType.Confirm, NavObjectType.TableData, 0, 0, 0);

	public static readonly Guid ComSecurityConfirmId = CreateWindowGuid(NavWindowType.Confirm, NavObjectType.TableData, 0, 0, 1);

	public static readonly Guid DotNetSecurityConfirmId = CreateWindowGuid(NavWindowType.Confirm, NavObjectType.TableData, 0, 0, 2);

	public static readonly Guid StringMenuDefaultId = CreateWindowGuid(NavWindowType.DialogSelectionMenu, NavObjectType.TableData, 0, 0, 0);

	public static readonly Guid OpenFileDefaultId = CreateWindowGuid(NavWindowType.OpenFileDialog, NavObjectType.TableData, 0, 0, 0);

	public static readonly Guid SaveFileDefaultId = CreateWindowGuid(NavWindowType.SaveFileDialog, NavObjectType.TableData, 0, 0, 0);

	public static readonly Guid ErrorDefaultId = CreateWindowGuid(NavWindowType.Error, NavObjectType.TableData, 0, 0, 0);

	public static readonly Guid ServerErrorDefaultId = CreateWindowGuid(NavWindowType.ServerError, NavObjectType.TableData, 0, 0, 0);

	public static readonly Guid HyperlinkDefaultId = CreateWindowGuid(NavWindowType.Hyperlink, NavObjectType.TableData, 0, 0, 0);

	public static Guid CreateWindowGuid(NavWindowType windowType, NavObjectType objectType, int objectId, int functionId, short sourceHash)
	{
		using MemoryStream memoryStream = new MemoryStream(16);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(objectId);
		binaryWriter.Write(functionId);
		binaryWriter.Write((byte)windowType);
		binaryWriter.Write((byte)objectType);
		binaryWriter.Write(sourceHash);
		binaryWriter.Write((byte)131);
		binaryWriter.Write((byte)107);
		binaryWriter.Write((byte)210);
		binaryWriter.Write((byte)210);
		byte[] array = new byte[16];
		binaryWriter.Flush();
		Array.Copy(memoryStream.GetBuffer(), array, 16);
		return new Guid(array);
	}

	public static void GetWindowInformation(Guid sourceGuid, out NavWindowType windowType, out NavObjectType objectType, out int objectId, out int functionId, out short sourceHash)
	{
		byte[] array = sourceGuid.ToByteArray();
		objectId = BitConverter.ToInt32(array, 0);
		functionId = BitConverter.ToInt32(array, 4);
		windowType = (NavWindowType)array[8];
		objectType = (NavObjectType)array[9];
		sourceHash = BitConverter.ToInt16(array, 10);
	}
}
