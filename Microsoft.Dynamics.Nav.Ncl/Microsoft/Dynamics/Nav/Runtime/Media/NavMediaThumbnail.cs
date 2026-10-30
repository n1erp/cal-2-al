using System.Drawing.Imaging;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal struct NavMediaThumbnail
{
	internal byte[] Data;

	internal int Width;

	internal int Height;

	internal ImageFormat imageFormat;
}
