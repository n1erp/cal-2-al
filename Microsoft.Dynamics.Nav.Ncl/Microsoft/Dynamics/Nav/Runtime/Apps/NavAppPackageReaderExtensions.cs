using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Model.Tools.IO.Nea;
using Microsoft.Dynamics.Nav.Runtime.Media;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class NavAppPackageReaderExtensions
{
	internal static IEnumerable<NavAppMedia> ResolvePackageMedia(this NavAppPackageReader reader)
	{
		List<NavAppMedia> list = new List<NavAppMedia>();
		foreach (NavAppMediaEntry item in reader.ReadMedia())
		{
			using Stream stream = reader.ReadMediaFile(item);
			Image image = null;
			try
			{
				image = Image.FromStream(stream);
				stream.Seek(0L, SeekOrigin.Begin);
				list.Add(NavAppMedia.CreateFromStream(stream, MediaId.op_Implicit(item.Id), item.IsLogo, (string)null));
			}
			catch (Exception exception)
			{
				throw NavMediaImageExceptionMapper.RemapImageException(exception, item.FileName);
			}
			finally
			{
				image?.Dispose();
			}
		}
		return list;
	}

	internal static byte[] ResolvePackageSymbols(this NavAppPackageReader reader)
	{
		using Stream stream = reader.ReadSymbolReferenceFile();
		if (stream.CanRead)
		{
			byte[] array = new byte[stream.Length];
			stream.Position = 0L;
			stream.Read(array, 0, array.Length);
			return array;
		}
		return null;
	}

	public static NavAppPackageReader CreateReaderFromPotentialNeaStream(Stream subject, bool leaveOpen = false)
	{
		if (!NeaStreamReader.IsSupported(subject))
		{
			return NavAppPackageReader.Create(subject, leaveOpen);
		}
		using NeaStreamReader neaStreamReader = new NeaStreamReader(subject, leaveOpen);
		MemoryStream memoryStream = new MemoryStream();
		neaStreamReader.CopyTo(memoryStream);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return NavAppPackageReader.Create(memoryStream);
	}
}
