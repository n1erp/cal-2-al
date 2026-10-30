using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class AppMediaLoader
{
	private readonly NavSqlConnectionScope connectionScope;

	public AppMediaLoader(NavSqlConnectionScope connectionScope)
	{
		this.connectionScope = connectionScope;
	}

	public Result LoadNavAppMedia(IEnumerable<NavAppMedia> media, MediaSetId setId)
	{
		try
		{
			AppMediaStorage mediaStorage = new AppMediaStorage(NavGlobal.AppDatabase, connectionScope);
			AppMediaSetStorage mediaSetStorage = new AppMediaSetStorage(NavGlobal.AppDatabase, connectionScope);
			return ResultExtensions.OnSuccess(ApplyBatchOperationToDatabase(media, (NavAppMedia x) => (!((WrappedValue<Guid>)(object)x.Id).Value.Equals(Guid.Empty)) ? mediaStorage.AddAppMedia(x) : Result.Ok(), Exceptions.Database_AddingAppMediaFailed), (Func<Result>)(() => (!((WrappedValue<Guid>)(object)setId).Value.Equals(Guid.Empty)) ? ApplyBatchOperationToDatabase(media, (NavAppMedia x) => (!((WrappedValue<Guid>)(object)x.Id).Value.Equals(Guid.Empty) && !x.IsLogo) ? mediaSetStorage.AddAppScreenShot(setId, x) : Result.Ok(), Exceptions.Database_AddingMediaSetFailed) : Result.Ok()));
		}
		catch (NavCSideException ex)
		{
			return Result.Fail(Error.FromException((Exception)ex));
		}
		catch (NavSqlException ex2)
		{
			return Result.Fail((Exception)ex2);
		}
	}

	public Result DeleteAppMedia(MediaId logoId, IEnumerable<MediaSetId> setIds)
	{
		AppMediaStorage mediaStorage = new AppMediaStorage(NavGlobal.AppDatabase, connectionScope);
		AppMediaSetStorage mediaSetStorage = new AppMediaSetStorage(NavGlobal.AppDatabase, connectionScope);
		return ResultExtensions.OnSuccess(mediaStorage.DeleteAppMedia(logoId), (Func<Result>)(() => IEnumerableResultExtensions.Combine<Result, Result>(setIds.Select((MediaSetId s) => ResultExtensions.OnSuccess(ResultExtensions.OnSuccess<IEnumerable<MediaId>>(mediaSetStorage.GetAppMediaIdsForMediaSet(s), (Func<IEnumerable<MediaId>, Result>)((IEnumerable<MediaId> media) => ApplyBatchOperationToDatabase(media, (MediaId x) => mediaStorage.DeleteAppMedia(x), Exceptions.Database_RemovingMediaFailed))), (Func<Result>)(() => mediaSetStorage.DeleteAppMediaSet(s)))), (Func<IEnumerable<Result>, Result>)((IEnumerable<Result> successes) => Result.Ok()), (Func<IEnumerable<Result>, Result>)((IEnumerable<Result> failures) => Result.Fail(Error.FromMultipleErrors("Delete Failures", failures.Select((Result f) => f.Error)))))));
	}

	private static Result ApplyBatchOperationToDatabase<T>(IEnumerable<T> source, Func<T, Result> applyToDatabase, string errorMessage)
	{
		return IEnumerableResultExtensions.Combine<Result, Result>(source.Select(applyToDatabase), (Func<IEnumerable<Result>, Result>)((IEnumerable<Result> success) => Result.Ok()), (Func<IEnumerable<Result>, Result>)((IEnumerable<Result> failures) => Result.Fail(Error.FromMultipleErrors(errorMessage, failures.Select((Result f) => f.Error)))));
	}
}
