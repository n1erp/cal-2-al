using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class SaveValuesSerializer
{
	public static byte[] Serialize(Dictionary<string, NavValue> values)
	{
		using HybridMemoryCompressStream hybridMemoryCompressStream = new HybridMemoryCompressStream(ServerUserSettings.Instance.ClientServicesCompressionThreshold.ValueKB);
		using (BinaryWriter binaryWriter = new BinaryWriter(hybridMemoryCompressStream, Encoding.UTF8, leaveOpen: true))
		{
			binaryWriter.Write(NavCurrentThread.Session.LCID);
			binaryWriter.Write(NavCurrentThread.Session.RegionalSettings.WindowsLCID);
			foreach (KeyValuePair<string, NavValue> value in values)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(value.Key);
				binaryWriter.Write(bytes.Length);
				binaryWriter.Write(bytes);
				value.Value.StoreToBytes(binaryWriter);
				switch (value.Value.NclType)
				{
				case NavNclType.NavOption:
				{
					NavOption navOption = value.Value as NavOption;
					if (navOption != null)
					{
						if (!string.IsNullOrEmpty(navOption.NavOptionMetadata.OptionString))
						{
							byte[] bytes2 = Encoding.UTF8.GetBytes(navOption.NavOptionMetadata.OptionString);
							binaryWriter.Write(bytes2.Length);
							binaryWriter.Write(bytes2);
						}
						else
						{
							binaryWriter.Write(0);
						}
					}
					break;
				}
				case NavNclType.NavOemText:
				case NavNclType.NavOemCode:
				case NavNclType.NavText:
				case NavNclType.NavCode:
				{
					NavStringValue navStringValue = value.Value as NavStringValue;
					if (navStringValue != null)
					{
						binaryWriter.Write(navStringValue.NavDefinedLengthMetadata);
					}
					break;
				}
				}
			}
		}
		return hybridMemoryCompressStream.GetDataStore().Data;
	}

	public static Dictionary<string, NavValue> Deserialize(byte[] objectValue)
	{
		Dictionary<string, NavValue> dictionary = new Dictionary<string, NavValue>();
		try
		{
			int num = 0;
			if (objectValue != null && objectValue.Length > 8)
			{
				int num2 = BitConverter.ToInt32(objectValue, num);
				num += 4;
				int num3 = BitConverter.ToInt32(objectValue, num);
				num += 4;
				if (NavCurrentThread.Session.LCID != num2 || NavCurrentThread.Session.RegionalSettings.WindowsLCID != num3)
				{
					return dictionary;
				}
				while (num < objectValue.Length)
				{
					int num4 = BitConverter.ToInt32(objectValue, num);
					string key = Encoding.UTF8.GetString(objectValue, num + 4, num4);
					NavValue value = null;
					num += 4 + num4;
					num += NavValue.ReadNavValueFromBytes(objectValue, num, out value);
					if (value != null)
					{
						switch (value.NclType)
						{
						case NavNclType.NavOption:
						{
							NavOption navOption = value as NavOption;
							if (navOption != null)
							{
								int num5 = BitConverter.ToInt32(objectValue, num);
								if (num5 > 0)
								{
									string option = Encoding.UTF8.GetString(objectValue, num + 4, num5);
									num += 4 + num5;
									NCLOptionMetadata metadata = NCLOptionMetadata.Create(option);
									value = NavOption.Create(metadata, navOption.Value);
								}
								else
								{
									num += 4;
								}
							}
							break;
						}
						case NavNclType.NavOemCode:
						{
							NavOemCode navOemCode = value as NavOemCode;
							if (navOemCode != null)
							{
								int maxLength3 = BitConverter.ToInt32(objectValue, num);
								value = new NavOemCode(maxLength3, navOemCode.Value);
								num += 4;
							}
							break;
						}
						case NavNclType.NavOemText:
						{
							NavOemText navOemText = value as NavOemText;
							if (navOemText != null)
							{
								int maxLength4 = BitConverter.ToInt32(objectValue, num);
								value = new NavOemText(maxLength4, navOemText.Value);
								num += 4;
							}
							break;
						}
						case NavNclType.NavText:
						{
							NavText navText = value as NavText;
							if (navText != null)
							{
								int maxLength2 = BitConverter.ToInt32(objectValue, num);
								value = new NavText(maxLength2, navText.Value);
								num += 4;
							}
							break;
						}
						case NavNclType.NavCode:
						{
							NavCode navCode = value as NavCode;
							if (navCode != null)
							{
								int maxLength = BitConverter.ToInt32(objectValue, num);
								value = new NavCode(maxLength, navCode.Value);
								num += 4;
							}
							break;
						}
						}
					}
					dictionary.Add(key, value);
				}
			}
		}
		catch (ArgumentOutOfRangeException exception)
		{
			LogWriterHelper.LogExceptionEvent(exception);
			dictionary.Clear();
		}
		catch (NavNCLTypeException exception2)
		{
			LogWriterHelper.LogExceptionEvent(exception2);
			dictionary.Clear();
		}
		catch (NavNCLNotSupportedTypeException exception3)
		{
			LogWriterHelper.LogExceptionEvent(exception3);
			dictionary.Clear();
		}
		return dictionary;
	}
}
