using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDesignerImage
{
	private Bitmap bitmap;

	public string MediaName { get; }

	public NavDesignerImage(string mediaName, Bitmap bitmap)
	{
		if (string.IsNullOrEmpty(mediaName) || bitmap == null)
		{
			throw new ArgumentException();
		}
		MediaName = mediaName;
		this.bitmap = bitmap;
	}

	public void SaveToStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		bitmap.Save(stream, ImageFormat.Png);
	}
}
