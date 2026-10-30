using System.Drawing.Imaging;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

public static class NavImageExtensions
{
	public static string GetMimeType(this ImageFormat imageFormat)
	{
		ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
		ImageCodecInfo imageCodecInfo = imageEncoders.FirstOrDefault((ImageCodecInfo codec) => codec.FormatID == imageFormat.Guid);
		if (imageCodecInfo != null)
		{
			return imageCodecInfo.MimeType;
		}
		return ImageMimeType(imageFormat);
	}

	private static string ImageMimeType(ImageFormat imf)
	{
		string text = ImageExtension(imf);
		if (!string.IsNullOrEmpty(text))
		{
			return "image/" + text;
		}
		return "[ImageFormat: " + imf.Guid.ToString() + "]";
	}

	internal static string ImageExtension(ImageFormat imf)
	{
		if (imf.Equals(ImageFormat.MemoryBmp))
		{
			return "memorybmp";
		}
		if (imf.Equals(ImageFormat.Bmp))
		{
			return "bmp";
		}
		if (imf.Equals(ImageFormat.Emf))
		{
			return "x-emf";
		}
		if (imf.Equals(ImageFormat.Wmf))
		{
			return "wmf";
		}
		if (imf.Equals(ImageFormat.Gif))
		{
			return "gif";
		}
		if (imf.Equals(ImageFormat.Jpeg))
		{
			return "jpeg";
		}
		if (imf.Equals(ImageFormat.Png))
		{
			return "png";
		}
		if (imf.Equals(ImageFormat.Tiff))
		{
			return "tiff";
		}
		if (imf.Equals(ImageFormat.Exif))
		{
			return "exif";
		}
		if (imf.Equals(ImageFormat.Icon))
		{
			return "icon";
		}
		return string.Empty;
	}
}
