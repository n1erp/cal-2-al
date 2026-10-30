using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Types.Media;

internal sealed class NavMediaImageTypes
{
	internal static readonly Dictionary<ImageFormat, string> ImageTypeCollection = new Dictionary<ImageFormat, string>
	{
		{
			ImageFormat.MemoryBmp,
			"memoryBmp"
		},
		{
			ImageFormat.Bmp,
			"bmp"
		},
		{
			ImageFormat.Emf,
			"emf"
		},
		{
			ImageFormat.Wmf,
			"wmf"
		},
		{
			ImageFormat.Jpeg,
			"jpeg"
		},
		{
			ImageFormat.Png,
			"png"
		},
		{
			ImageFormat.Gif,
			"gif"
		},
		{
			ImageFormat.Tiff,
			"tiff"
		},
		{
			ImageFormat.Exif,
			"exif"
		},
		{
			ImageFormat.Icon,
			"icon"
		}
	};

	private static readonly HashSet<string> SupportedMimeTypes = new HashSet<string>((from c in ImageCodecInfo.GetImageDecoders()
		select c.MimeType).Concat(new string[1] { "application/x-msmetafile" }), StringComparer.OrdinalIgnoreCase);

	public static bool IsSupportedMimeType(string mimeType)
	{
		if (!SupportedMimeTypes.Contains(mimeType))
		{
			return mimeType.StartsWith("image", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
