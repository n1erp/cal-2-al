using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Media;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal sealed class NavMediaImage : NavMediaBase
{
	private enum ImageDataType : short
	{
		None,
		Byte,
		String,
		Short,
		Long,
		Double,
		SignedByte,
		Undefined,
		SignedShort,
		SignedLong,
		SignedDouble,
		SingleFloat,
		DoubleFloat
	}

	private Image image;

	private readonly Stream imageStream;

	private Image builtinThumbnail;

	private byte[] builtinThumbnailData;

	private List<int> propertyIdList;

	private const int DefaultImageQuality = 90;

	private const int RationalByteSize = 8;

	public override byte[] BuiltinThumbnailData => builtinThumbnailData;

	public override int BuiltinThumbnailHeight => builtinThumbnail?.Height ?? 0;

	public override int BuiltinThumbnailWidth => builtinThumbnail?.Width ?? 0;

	public override int Height => image.Height;

	public override int Width => image.Width;

	public ushort Orientation { get; private set; }

	public CompositingQuality CompositingQuality { get; set; }

	public SmoothingMode SmoothingMode { get; set; }

	public InterpolationMode InterpolationMode { get; set; }

	public ImageFormat ThumbnailFormat { get; set; }

	public override string MimeType => GetMimeType(image.RawFormat);

	public override string ThumbnailMimeType => GetMimeType(ThumbnailFormat);

	public override string BuiltinThumbnailMimeType => GetMimeType(builtinThumbnail.RawFormat);

	internal NavMediaImage(Image imageObject)
	{
		if (imageObject == null)
		{
			throw new ArgumentNullException("imageObject");
		}
		image = imageObject;
		Initialize();
	}

	public NavMediaImage(Stream imageSourceStream, bool saveStream)
	{
		if (imageSourceStream == null)
		{
			throw new ArgumentNullException("imageSourceStream");
		}
		if (saveStream)
		{
			imageStream = new MemoryStream();
			imageSourceStream.CopyTo(imageStream);
		}
		ValidateContentHeader(saveStream ? imageStream : imageSourceStream);
		Initialize();
	}

	public NavMediaImage(Image imageObject, Stream imageStream1)
		: this(imageObject)
	{
		imageStream = imageStream1;
	}

	private void Initialize()
	{
		propertyIdList = new List<int>(image.PropertyIdList);
		builtinThumbnailData = GetPropertyItem(NavExifPropertyTag.PropertyTag.PropertyTagThumbnailData)?.Value;
		if (builtinThumbnailData != null && builtinThumbnailData.Length != 0)
		{
			using MemoryStream stream = new MemoryStream(builtinThumbnailData);
			builtinThumbnail = Image.FromStream(stream);
			ThumbnailFormat = builtinThumbnail.RawFormat;
		}
		else
		{
			ThumbnailFormat = (image.RawFormat.Equals(ImageFormat.Jpeg) ? ImageFormat.Jpeg : ImageFormat.Png);
		}
		PropertyItem propertyItem = GetPropertyItem(NavExifPropertyTag.PropertyTag.PropertyTagOrientation);
		if (propertyItem != null)
		{
			Orientation = Convert.ToUInt16(GetValue(propertyItem), CultureInfo.InvariantCulture);
		}
		CompositingQuality = CompositingQuality.HighQuality;
		SmoothingMode = SmoothingMode.HighQuality;
		InterpolationMode = InterpolationMode.HighQualityBicubic;
	}

	public PropertyItem GetPropertyItem(NavExifPropertyTag.PropertyTag propertyTag)
	{
		if (!propertyIdList.Contains((int)propertyTag))
		{
			return null;
		}
		return image.GetPropertyItem((int)propertyTag);
	}

	public override byte[] Bytes()
	{
		ImageConverter imageConverter = new ImageConverter();
		byte[] array = (byte[])imageConverter.ConvertTo(image, typeof(byte[]));
		return array ?? new byte[0];
	}

	public override NavMediaThumbnail Scale(int width)
	{
		RotateFlipType rotateFlipType = ExifOrientationToRotateFlipType(Orientation);
		if (width > Width && rotateFlipType == RotateFlipType.RotateNoneFlipNone)
		{
			return GetNavMediaThumbnail(Width, Height, Bytes(), ThumbnailFormat);
		}
		double num = (double)Width / (double)Height;
		int num2 = (RotationCausesAspectRatioChange(rotateFlipType) ? Math.Min(Width, width) : ((int)Math.Round((double)width / num)));
		width = (int)Math.Round(num * (double)num2);
		MemoryStream memoryStream = new MemoryStream();
		using (Bitmap img = new Bitmap(width, num2))
		{
			DrawRotateAndSaveImage(img, 0, 0, width, num2, rotateFlipType, memoryStream);
		}
		return GetNavMediaThumbnail(width, num2, BytesFromMemoryStream(memoryStream), ThumbnailFormat);
	}

	private static NavMediaThumbnail GetNavMediaThumbnail(int width, int height, byte[] data, ImageFormat format)
	{
		return new NavMediaThumbnail
		{
			Width = width,
			Height = height,
			Data = data,
			imageFormat = format
		};
	}

	public override NavMediaThumbnail Scale(int width, int height)
	{
		if (width <= 0)
		{
			throw new NavNCLArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, Lang.ParameterMustBePositive, "width"));
		}
		if (height <= 0)
		{
			throw new NavNCLArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, Lang.ParameterMustBePositive, "height"));
		}
		RotateFlipType rotateFlipType = ExifOrientationToRotateFlipType(Orientation);
		if (Height < height && Width < width && rotateFlipType == RotateFlipType.RotateNoneFlipNone)
		{
			return GetNavMediaThumbnailOnBackground(width, height, ThumbnailFormat);
		}
		bool flag = RotationCausesAspectRatioChange(rotateFlipType);
		int x;
		int y;
		int newWidth;
		int newHeight;
		if (flag)
		{
			GetScaledImageLocationAndSize(height, width, out x, out y, out newWidth, out newHeight);
		}
		else
		{
			GetScaledImageLocationAndSize(width, height, out x, out y, out newWidth, out newHeight);
		}
		MemoryStream memoryStream = new MemoryStream();
		using (Bitmap img = (flag ? new Bitmap(height, width) : new Bitmap(width, height)))
		{
			DrawRotateAndSaveImage(img, x, y, newWidth, newHeight, rotateFlipType, memoryStream);
		}
		return GetNavMediaThumbnail(width, height, BytesFromMemoryStream(memoryStream), ThumbnailFormat);
	}

	private NavMediaThumbnail GetNavMediaThumbnailOnBackground(int width, int height, ImageFormat thumbnailFormat)
	{
		Bitmap bitmap = new Bitmap(width, height);
		bitmap.MakeTransparent();
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.CompositingQuality = CompositingQuality.HighQuality;
		graphics.CompositingMode = CompositingMode.SourceOver;
		graphics.SmoothingMode = SmoothingMode.HighQuality;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		int x = (bitmap.Width - image.Width) / 2;
		int y = (bitmap.Height - image.Height) / 2;
		graphics.FillRectangle(rect: new Rectangle(0, 0, bitmap.Width, bitmap.Height), brush: Brushes.White);
		graphics.DrawImage(image, new Point(x, y));
		graphics.Flush();
		ImageCodecInfo imageCodeInfo = GetImageCodeInfo(thumbnailFormat.Guid);
		using MemoryStream memoryStream = new MemoryStream();
		ImageFormat imageFormat = ((imageCodeInfo != null) ? thumbnailFormat : image.RawFormat);
		bitmap.Save(memoryStream, imageFormat);
		memoryStream.Position = 0L;
		return new NavMediaThumbnail
		{
			Width = width,
			Height = height,
			Data = BytesFromMemoryStream(memoryStream),
			imageFormat = imageFormat
		};
	}

	private static ImageCodecInfo GetImageCodeInfo(Guid imageId)
	{
		ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
		return imageEncoders.FirstOrDefault((ImageCodecInfo x) => x.FormatID == imageId);
	}

	private void GetScaledImageLocationAndSize(int width, int height, out int x, out int y, out int newWidth, out int newHeight)
	{
		if (Width > width && Height > height)
		{
			double num = (double)Width / (double)Height;
			double num2 = (double)width / (double)height;
			if (num > num2)
			{
				newWidth = (int)Math.Round(num * (double)height);
				newHeight = height;
			}
			else
			{
				newWidth = width;
				newHeight = (int)Math.Round((double)width / num);
			}
		}
		else
		{
			newWidth = Width;
			newHeight = Height;
		}
		x = (width - newWidth) / 2;
		y = (height - newHeight) / 2;
	}

	private void DrawRotateAndSaveImage(Image img, int x, int y, int width, int height, RotateFlipType rotateFlip, Stream scaledStream)
	{
		using Graphics graphics = Graphics.FromImage(img);
		graphics.CompositingQuality = CompositingQuality;
		graphics.SmoothingMode = SmoothingMode;
		graphics.InterpolationMode = InterpolationMode;
		graphics.DrawImage(image, x, y, width, height);
		if (rotateFlip != RotateFlipType.RotateNoneFlipNone)
		{
			img.RotateFlip(rotateFlip);
		}
		img.Save(scaledStream, ThumbnailFormat);
	}

	private static bool RotationCausesAspectRatioChange(RotateFlipType rotateFlip)
	{
		return (rotateFlip & RotateFlipType.Rotate90FlipNone) != 0;
	}

	private static byte[] BytesFromMemoryStream(MemoryStream dataStream)
	{
		return dataStream.GetBuffer();
	}

	private static ImageDataType GetImageDataType(PropertyItem i)
	{
		try
		{
			return (ImageDataType)i.Type;
		}
		catch (InvalidCastException)
		{
			return ImageDataType.None;
		}
	}

	private static object GetValue(PropertyItem item)
	{
		return GetImageDataType(item) switch
		{
			ImageDataType.Byte => (object)item.Value[0], 
			ImageDataType.String => Encoding.ASCII.GetString(item.Value), 
			ImageDataType.Short => BitConverter.ToUInt16(item.Value, 0), 
			ImageDataType.SignedLong => BitConverter.ToInt32(item.Value, 0), 
			ImageDataType.Long => BitConverter.ToUInt32(item.Value, 0), 
			ImageDataType.Double => FormatTagRational(item), 
			ImageDataType.SignedDouble => BitConverter.ToDouble(item.Value, 0), 
			ImageDataType.Undefined => item.Value[0], 
			ImageDataType.SignedByte => (int)item.Value[0], 
			ImageDataType.SignedShort => BitConverter.ToInt16(item.Value, 0), 
			ImageDataType.SingleFloat => BitConverter.ToSingle(item.Value, 0), 
			ImageDataType.DoubleFloat => BitConverter.ToDouble(item.Value, 0), 
			_ => null, 
		};
	}

	private static double[] FormatTagRational(PropertyItem propItem)
	{
		List<double> list = new List<double>();
		for (int i = 0; i < propItem.Len; i += 8)
		{
			uint num = BitConverter.ToUInt32(propItem.Value, i);
			uint num2 = BitConverter.ToUInt32(propItem.Value, i + 4);
			double item = ((num2 == 0) ? 0.0 : ((double)num / (double)num2));
			list.Add(item);
		}
		return list.ToArray();
	}

	private static RotateFlipType ExifOrientationToRotateFlipType(ushort value)
	{
		return value switch
		{
			1 => RotateFlipType.RotateNoneFlipNone, 
			2 => RotateFlipType.RotateNoneFlipX, 
			3 => RotateFlipType.Rotate180FlipNone, 
			4 => RotateFlipType.Rotate180FlipX, 
			5 => RotateFlipType.Rotate90FlipX, 
			6 => RotateFlipType.Rotate90FlipNone, 
			7 => RotateFlipType.Rotate270FlipX, 
			8 => RotateFlipType.Rotate270FlipNone, 
			_ => RotateFlipType.RotateNoneFlipNone, 
		};
	}

	public sealed override void ValidateContentHeader(Stream contentStream)
	{
		try
		{
			image = Image.FromStream(contentStream, useEmbeddedColorManagement: true);
		}
		catch (Exception exception)
		{
			throw NavMediaImageExceptionMapper.RemapImageException(exception);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		image.Dispose();
		try
		{
			if (imageStream != null)
			{
				imageStream.Dispose();
			}
		}
		catch (ObjectDisposedException)
		{
		}
	}

	internal static string GetMimeType(ImageFormat imageFormat)
	{
		if (!NavMediaImageTypes.ImageTypeCollection.TryGetValue(imageFormat, out var value))
		{
			value = imageFormat.ToString();
		}
		else
		{
			string mimeType = imageFormat.GetMimeType();
			if (!string.IsNullOrEmpty(mimeType))
			{
				return mimeType;
			}
		}
		return NavMimeMapping.GetMimeMapping("." + value);
	}
}
