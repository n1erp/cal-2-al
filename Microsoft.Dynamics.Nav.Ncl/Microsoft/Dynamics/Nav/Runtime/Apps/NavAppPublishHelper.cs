using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Apps.Win32;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppPublishHelper
{
	public static NavSqlDatabaseLock CreatePackageIdLock(NavSqlConnectionScope appDatabaseScope, PackageId packageId)
	{
		string text = "NAVAPPPUBLISHPACKAGEID_" + ((WrappedValue<Guid>)(object)packageId).Value.ToString();
		int num = Convert.ToInt32(ServerUserSettings.Instance.PublishPackageIdLockTimeout.Value.TotalMilliseconds);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001YK", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Taking extension package ID lock -- Name:{0}; TimeoutMs:{1}", text, num);
		return NavSqlDatabaseLock.LockResource(appDatabaseScope, text, num);
	}

	public static Result EnsureLockWasAcquired(NavSqlDatabaseLock theLock, PackageId packageId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001YL", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Ensuring extension package ID was acquired -- PackageId:{0}", ((WrappedValue<Guid>)(object)packageId).Value);
		if (!theLock.IsLockAcquired())
		{
			string text = string.Format(CultureInfo.InvariantCulture, "The extension package ID lock could not be acquired. Another operation is being performed on the same package -- PackageId:{0}", ((WrappedValue<Guid>)(object)packageId).Value);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001YM", Category.Extensions, Verbosity.Warning, PrivacyClassification.SystemMetadata, text);
			return Result.Fail(Error.FromMessage(text, ErrorType.InvalidOperation));
		}
		return Result.Ok();
	}

	public static Result VerifySignature(Stream packageStream, bool skipSigningVerification)
	{
		Result val = Result.Ok();
		if (!skipSigningVerification)
		{
			try
			{
				string text = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".app");
				using (FileStream destination = File.OpenWrite(text))
				{
					packageStream.Position = 0L;
					packageStream.CopyTo(destination);
				}
				NativeMethods.VerifyAuthenticodeSignature(text);
			}
			catch (Exception ex)
			{
				val = ((ex.HResult != -2146762496 && ex.HResult != -2146762749) ? Result.Fail(Error.FromException(ex)) : Result.Fail(Error.FromMessage(Exceptions.CannotPublishUnsignedPackage, ErrorType.FromExceptionType(ex.GetType()))));
			}
			finally
			{
				packageStream.Position = 0L;
			}
		}
		if (val.Failure)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001YU", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, val.Error.Message);
		}
		return val;
	}
}
