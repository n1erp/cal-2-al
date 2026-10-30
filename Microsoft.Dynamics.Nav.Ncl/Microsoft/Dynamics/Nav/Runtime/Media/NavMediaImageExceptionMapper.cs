using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal static class NavMediaImageExceptionMapper
{
	internal static Exception RemapImageException(Exception exception, string fileName = null)
	{
		if (exception is ArgumentException)
		{
			string message = ((fileName != null) ? string.Format(CultureInfo.CurrentCulture, Lang.MediaImageLoadErrorWithName, fileName) : Lang.MediaImageLoadError);
			return new NavImageLoadErrorException(message, exception);
		}
		if (exception is OutOfMemoryException)
		{
			return new NavImageBufferException(exception.Message, exception);
		}
		if (exception is InvalidOperationException)
		{
			return new NavImageLoadErrorException(Lang.MediaImageLoadError, exception);
		}
		if (exception is NotImplementedException)
		{
			return new NavImageLoadErrorException(Lang.MediaImageLoadError, exception);
		}
		if (exception is ExternalException)
		{
			return new NavImageLoadErrorException(Lang.MediaImageLoadError, exception);
		}
		if (exception is FileNotFoundException ex)
		{
			return new NavNCLFileNotFoundException(ex.FileName);
		}
		if (exception is OverflowException)
		{
			return new NavNCLArgumentOutOfRangeException(exception.Message);
		}
		return new NavImageLoadErrorException(Lang.MediaImageLoadError, exception);
	}
}
