using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal abstract class NavMediaDocument : NavMediaBase
{
	private readonly Stream mediaStream;

	private NavMediaImage navMediaImage;

	private NavMediaImage MediaBitmap => navMediaImage ?? (navMediaImage = PictureFromMimetype(MimeType));

	public override byte[] BuiltinThumbnailData => MediaBitmap.Bytes();

	public override int BuiltinThumbnailWidth => MediaBitmap.Width;

	public override int BuiltinThumbnailHeight => MediaBitmap.Height;

	public override string BuiltinThumbnailMimeType => MediaBitmap.BuiltinThumbnailMimeType;

	public override bool AllowCustomThumbnails => false;

	public override int Height => 0;

	public override int Width => 0;

	public override string ThumbnailMimeType => "image/png";

	protected NavMediaDocument(Stream mediaSourceStream, string mimeType)
	{
		if (mediaSourceStream == null)
		{
			throw new ArgumentNullException("mediaSourceStream");
		}
		if (string.IsNullOrEmpty(mimeType))
		{
			throw new NavNCLArgumentException(Lang.InvalidOrEmptyMimeType);
		}
		InnerMimeType = mimeType;
		mediaStream = new MemoryStream();
		mediaSourceStream.CopyTo(mediaStream);
	}

	public override byte[] Bytes()
	{
		mediaStream.Seek(0L, SeekOrigin.Begin);
		int num = (int)mediaStream.Length;
		byte[] array = new byte[num];
		mediaStream.Read(array, 0, num);
		return array;
	}

	public override NavMediaThumbnail Scale(int width)
	{
		return MediaBitmap.Scale(width);
	}

	public override NavMediaThumbnail Scale(int width, int height)
	{
		return MediaBitmap.Scale(width, height);
	}

	private static Bitmap TextToImage(string txt, string fontname, int fontsize, Color backgroundColor, Color foregroundColor, int width, int height)
	{
		using Bitmap bitmap = new Bitmap(width, height);
		using Graphics graphics = Graphics.FromImage(bitmap);
		using Font font = new Font(string.IsNullOrEmpty(fontname) ? SystemFonts.IconTitleFont.Name : fontname, fontsize);
		RectangleF rectangleF = new RectangleF(0f, 0f, bitmap.Width, bitmap.Height);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
		graphics.FillRectangle(new SolidBrush(backgroundColor), rectangleF);
		graphics.DrawString(string.Format(CultureInfo.CurrentCulture, Lang.NoMediaPreviewForType, txt), font, new SolidBrush(foregroundColor), rectangleF, new StringFormat
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center
		});
		graphics.Flush();
		MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, ImageFormat.Png);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		Image image = Image.FromStream(memoryStream);
		return (Bitmap)image;
	}

	internal static NavMediaImage PictureFromMimetype(string mimeType)
	{
		return new NavMediaImage(TextToImage(mimeType, "", 20, Color.White, Color.Black, 360, 360));
	}
}
