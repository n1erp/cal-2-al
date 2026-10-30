using System;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavFile : NavComplexValue, INavStreamReader, INavStreamMode, INavStreamTextEncoding, INavStreamWriter, INavStreamProvider, IDisposable
{
	public class Factory : ElementFactory<NavFile>
	{
		public Factory(ITreeObject parentForArray)
			: base(parentForArray)
		{
		}

		public override NavFile CreateElement()
		{
			return new NavFile(ParentForElements);
		}

		public override void AssignElement(NavFile destinationElement, NavFile sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavFile> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone);
		}
	}

	private enum FileOption
	{
		Create,
		CreateTemp,
		Open
	}

	internal struct UsernameTokens
	{
		internal bool IsDomainFormat;

		internal string Domain;

		internal string User;

		internal static UsernameTokens Default()
		{
			UsernameTokens result = default;
			result.IsDomainFormat = false;
			result.Domain = string.Empty;
			result.User = string.Empty;
			return result;
		}
	}

	private string tempDir;

	public const string TempDirectoryName = "TEMP\\";

	private FileStream fileStream;

	private BinaryReader binaryReader;

	private bool textMode;

	private TextEncoding textEncoding;

	private bool writeMode;

	private string fileName;

	private static readonly string serverUsersFolder = NavEnvironment.Instance.TemporaryPathHelper.UserPath;

	public string ALName
	{
		get
		{
			if (InternalStreamChecked != null)
			{
				return fileName;
			}
			return string.Empty;
		}
	}

	public int ALPos => (int)InternalStreamChecked.Position;

	public int ALLen => (int)InternalStreamChecked.Length;

	public bool ALWriteMode
	{
		get
		{
			return writeMode;
		}
		set
		{
			if (InternalStream != null)
			{
				throw new NavNCLFileAlreadyOpenedException();
			}
			writeMode = value;
		}
	}

	public bool ALTextMode
	{
		get
		{
			return textMode;
		}
		set
		{
			textMode = value;
		}
	}

	private BinaryReader BinaryReader
	{
		get
		{
			if (binaryReader == null)
			{
				binaryReader = new BinaryReader(InternalStreamCheckedForRead, (textEncoding == TextEncoding.UTF16) ? Encoding.Unicode : Encoding.UTF8);
			}
			return binaryReader;
		}
	}

	private static string DefaultDirectory => GetUserDirectoryAndEnsureExists(DataError.ThrowError, tempPath: false);

	private Stream InternalStreamChecked
	{
		get
		{
			Stream internalStream = InternalStream;
			if (internalStream == null)
			{
				throw new NavNCLFileNotOpenedException();
			}
			return internalStream;
		}
	}

	private Stream InternalStreamCheckedForRead
	{
		get
		{
			Stream internalStreamChecked = InternalStreamChecked;
			if (!internalStreamChecked.CanRead)
			{
				throw new NavNCLStreamAccessException();
			}
			return internalStreamChecked;
		}
	}

	private Stream InternalStreamCheckedForWrite
	{
		get
		{
			Stream internalStreamChecked = InternalStreamChecked;
			if (!internalStreamChecked.CanWrite)
			{
				throw new NavNCLStreamAccessException();
			}
			return internalStreamChecked;
		}
	}

	internal Stream InternalStream => fileStream;

	public override NavNclType NclType => NavNclType.NavFile;

	public override int GetBytesSize
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	TextEncoding INavStreamTextEncoding.TextEncoding => textEncoding;

	NavStreamMode INavStreamMode.Mode
	{
		get
		{
			if (!textMode)
			{
				return NavStreamMode.FileBinary;
			}
			return NavStreamMode.FileText;
		}
	}

	public NavInStream ALCreateInStream(ITreeObject parentForResult)
	{
		return new NavInStream(parentForResult, this, textEncoding);
	}

	public void ALCreateInStream(ITreeObject parentForResult, NavInStream inStream)
	{
		inStream.ALAssign(new NavInStream(parentForResult, this, textEncoding));
	}

	public NavOutStream ALCreateOutStream(ITreeObject parentForResult)
	{
		if (!ALWriteMode)
		{
			throw new NavNCLStreamAccessException();
		}
		return new NavOutStream(parentForResult, this, textEncoding);
	}

	public void ALCreateOutStream(ITreeObject parentForResult, NavOutStream outStream)
	{
		if (!ALWriteMode)
		{
			throw new NavNCLStreamAccessException();
		}
		outStream.ALAssign(new NavOutStream(parentForResult, this, textEncoding));
	}

	public bool ALCreate(string createName)
	{
		return ALCreate(DataError.ThrowError, createName);
	}

	public bool ALCreate(string createName, TextEncoding encoding)
	{
		return ALCreate(DataError.ThrowError, createName, encoding);
	}

	public bool ALCreate(DataError errorLevel, string createName)
	{
		return ALCreate(errorLevel, createName, TextEncoding.MSDos);
	}

	public bool ALCreate(DataError errorLevel, string createName, TextEncoding encoding)
	{
		try
		{
			try
			{
				textEncoding = encoding;
				fileName = ExpandFileName(errorLevel, createName);
				writeMode = true;
				return InternalCreateOrOpen(errorLevel, FileOption.Create, checkFileType: true);
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return false;
	}

	public bool ALCreateTempFile()
	{
		return ALCreateTempFile(DataError.ThrowError, TextEncoding.MSDos);
	}

	public bool ALCreateTempFile(TextEncoding encoding)
	{
		return ALCreateTempFile(DataError.ThrowError, encoding);
	}

	public bool ALCreateTempFile(DataError errorLevel)
	{
		return ALCreateTempFile(errorLevel, TextEncoding.MSDos);
	}

	public bool ALCreateTempFile(DataError errorLevel, TextEncoding encoding)
	{
		try
		{
			try
			{
				if (string.IsNullOrEmpty(tempDir))
				{
					tempDir = GetUserDirectoryAndEnsureExists(errorLevel, tempPath: true);
				}
				textEncoding = encoding;
				fileName = CreateTempFileName(errorLevel, tempDir);
				writeMode = true;
				return InternalCreateOrOpen(errorLevel, FileOption.CreateTemp, checkFileType: false);
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return false;
	}

	public bool ALOpen(string openName)
	{
		return ALOpen(DataError.ThrowError, openName, TextEncoding.MSDos);
	}

	public bool ALOpen(string openName, TextEncoding encoding)
	{
		return ALOpen(DataError.ThrowError, openName, encoding);
	}

	public bool ALOpen(DataError errorLevel, string openName)
	{
		return ALOpen(errorLevel, openName, TextEncoding.MSDos);
	}

	public bool ALOpen(DataError errorLevel, string openName, TextEncoding encoding)
	{
		try
		{
			try
			{
				textEncoding = encoding;
				fileName = ExpandFileName(errorLevel, openName);
				return InternalCreateOrOpen(errorLevel, FileOption.Open, checkFileType: true);
			}
			catch (Exception ex)
			{
				fileName = null;
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return false;
	}

	public void ALClose()
	{
		if (InternalStreamChecked != null)
		{
			InternalClose(DataError.ThrowError);
		}
	}

	public void ALSeek(int position)
	{
		try
		{
			InternalStreamChecked.Seek(position, SeekOrigin.Begin);
		}
		catch (Exception ex)
		{
			NavFileHelper.MapAndRethrowClrException(ex, writeExceptionToEventLog: true);
			throw;
		}
	}

	public void ALTrunc()
	{
		try
		{
			Stream internalStreamCheckedForWrite = InternalStreamCheckedForWrite;
			internalStreamCheckedForWrite.SetLength(internalStreamCheckedForWrite.Position);
		}
		catch (Exception ex)
		{
			NavFileHelper.MapAndRethrowClrException(ex, writeExceptionToEventLog: true);
			throw;
		}
	}

	public static bool ALIsPathTemporary(DataError errorLevel, string filePath)
	{
		try
		{
			ExpandFileName(DataError.ThrowError, filePath, enforceUserPath: true);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		return true;
	}

	public static bool ALErase(string fileName)
	{
		return ALErase(DataError.ThrowError, fileName);
	}

	public static bool ALErase(DataError errorLevel, string fileName)
	{
		try
		{
			VerifyALFileAccessEnabled();
			try
			{
				string path = ExpandFileName(errorLevel, fileName);
				ALExists(errorLevel, path);
				File.Delete(path);
				return true;
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return false;
	}

	public static bool ALRename(string oldFileName, string newFileName)
	{
		return ALRename(DataError.ThrowError, oldFileName, newFileName);
	}

	public static bool ALRename(DataError errorLevel, string oldFileName, string newFileName)
	{
		try
		{
			VerifyALFileAccessEnabled();
			try
			{
				string sourceFileName = ExpandFileName(errorLevel, oldFileName);
				string destFileName = ExpandFileName(errorLevel, newFileName);
				if (!FileTypeFilter.IsFileTypeAllowedByConfiguration(newFileName))
				{
					throw new NavNCLFileLoadException(Lang.FileTypeNotAllowedRename);
				}
				File.Move(sourceFileName, destFileName);
				return true;
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return false;
	}

	public static bool ALCopy(string fromFileName, string toFileName)
	{
		return ALCopy(DataError.ThrowError, fromFileName, toFileName);
	}

	public static bool ALCopy(DataError errorLevel, string fromFileName, string toFileName)
	{
		VerifyALFileAccessEnabled();
		try
		{
			try
			{
				string sourceFileName = ExpandFileName(errorLevel, fromFileName);
				string destFileName = ExpandFileName(errorLevel, toFileName);
				if (!FileTypeFilter.IsFileTypeAllowedByConfiguration(toFileName))
				{
					throw new NavNCLFileLoadException(Lang.FileTypeNotAllowedCopy);
				}
				File.Copy(sourceFileName, destFileName, overwrite: true);
				return true;
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return false;
	}

	public static bool ALUploadIntoStream(string dialogTitle, string fromInitialFolder, string fromTypeFilter, ByRef<NavText> fromFileName, ByRef<NavInStream> navInStream, Guid automationId)
	{
		return ALUploadIntoStream(DataError.ThrowError, dialogTitle, fromInitialFolder, fromTypeFilter, fromFileName, navInStream, automationId);
	}

	public static bool ALUploadIntoStream(DataError errorLevel, string dialogTitle, string fromInitialFolder, string fromTypeFilter, ByRef<NavText> fromFileName, ByRef<NavInStream> navInStream, Guid automationId)
	{
		NavOemText navOemValue = NavOemText.Empty;
		ByRef<NavOemText> byRef = new ByRef<NavOemText>(() => navOemValue, (NavOemText value) =>
		{
			navOemValue = value;
		});
		byRef.Value = new NavOemText(2048, fromFileName.Value);
		bool result = ALUploadIntoStream(errorLevel, dialogTitle, fromInitialFolder, fromTypeFilter, byRef, navInStream, automationId);
		fromFileName.Value = new NavText(byRef.Value.ToString());
		return result;
	}

	public static bool ALUploadIntoStream(string dialogTitle, string fromInitialFolder, string fromTypeFilter, ByRef<NavOemText> fromFileName, ByRef<NavInStream> stream, Guid automationId)
	{
		return ALUploadIntoStream(DataError.ThrowError, dialogTitle, fromInitialFolder, fromTypeFilter, fromFileName, stream, automationId);
	}

	public static bool ALUploadIntoStream(DataError errorLevel, string dialogTitle, string fromInitialFolder, string fromTypeFilter, ByRef<NavOemText> fromFileName, ByRef<NavInStream> navInStream, Guid automationId)
	{
		string value = fromFileName.Value;
		bool result = false;
		try
		{
			try
			{
				FileBufferedStream fileBufferedStream = InternalUpload(displayDialog: true, dialogTitle, fromInitialFolder, ref value, fromTypeFilter, automationId);
				if (fileBufferedStream != null)
				{
					fileBufferedStream.ShouldDeleteFileOnDispose = true;
					navInStream.Value = new NavInStream(navInStream.Value.Tree.Parent, new NavStreamProvider(fileBufferedStream, navInStream.Value.Tree.Parent), navInStream.Value.TextEncoding);
					fromFileName.Value = new NavOemText(fromFileName.Value.NavDefinedLengthMetadata, value);
					result = true;
				}
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return result;
	}

	public static bool ALUpload(string dialogTitle, string fromInitialFolder, string fromTypeFilter, string fromFileName, ByRef<NavText> toFileName, Guid automationId)
	{
		return ALUpload(DataError.ThrowError, dialogTitle, fromInitialFolder, fromTypeFilter, fromFileName, toFileName, automationId);
	}

	public static bool ALUpload(DataError errorLevel, string dialogTitle, string fromInitialFolder, string fromTypeFilter, string fromFileName, ByRef<NavText> toFileName, Guid automationId)
	{
		NavOemText navOemTextValue = NavOemText.Empty;
		ByRef<NavOemText> byRef = new ByRef<NavOemText>(() => navOemTextValue, (NavOemText value) =>
		{
			navOemTextValue = value;
		});
		byRef.Value = new NavOemText(2048, toFileName.Value);
		bool result = ALUpload(errorLevel, dialogTitle, fromInitialFolder, fromTypeFilter, fromFileName, byRef, automationId);
		toFileName.Value = new NavText(byRef.Value.ToString());
		return result;
	}

	public static bool ALUpload(string dialogTitle, string fromInitialFolder, string fromTypeFilter, string fromFileName, ByRef<NavOemText> toFileName, Guid automationId)
	{
		return ALUpload(DataError.ThrowError, dialogTitle, fromInitialFolder, fromTypeFilter, fromFileName, toFileName, automationId);
	}

	public static bool ALUpload(DataError errorLevel, string dialogTitle, string fromInitialFolder, string fromTypeFilter, string fromFileName, ByRef<NavOemText> toFileName, Guid automationId)
	{
		string localPath = toFileName.Value;
		bool result = false;
		try
		{
			try
			{
				if (InternalUpload(errorLevel, displayDialog: true, dialogTitle, fromInitialFolder, fromFileName, fromTypeFilter, ref localPath, automationId))
				{
					toFileName.Value = new NavOemText(toFileName.Value.NavDefinedLengthMetadata, localPath);
					result = true;
				}
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return result;
	}

	public static bool ALDownloadFromStream(NavInStream source, string dialogTitle, string toInitialFolder, string toTypeFilter, ByRef<NavText> toFileName, Guid automationId)
	{
		return ALDownloadFromStream(DataError.ThrowError, source, dialogTitle, toInitialFolder, toTypeFilter, toFileName, automationId);
	}

	public static bool ALDownloadFromStream(DataError errorLevel, NavInStream source, string dialogTitle, string toInitialFolder, string toTypeFilter, ByRef<NavText> toFileName, Guid automationId)
	{
		NavOemText navOemTextValue = NavOemText.Empty;
		ByRef<NavOemText> byRef = new ByRef<NavOemText>(() => navOemTextValue, (NavOemText value) =>
		{
			navOemTextValue = value;
		});
		byRef.Value = new NavOemText(2048, toFileName.Value);
		bool result = ALDownloadFromStream(errorLevel, source, dialogTitle, toInitialFolder, toTypeFilter, byRef, automationId);
		toFileName.Value = new NavText(byRef.Value.ToString());
		return result;
	}

	public static bool ALDownloadFromStream(NavInStream source, string dialogTitle, string toInitialFolder, string toTypeFilter, ByRef<NavOemText> toFileName, Guid automationId)
	{
		return ALDownloadFromStream(DataError.ThrowError, source, dialogTitle, toInitialFolder, toTypeFilter, toFileName, automationId);
	}

	public static bool ALDownloadFromStream(DataError errorLevel, NavInStream source, string dialogTitle, string toInitialFolder, string toTypeFilter, ByRef<NavOemText> toFileName, Guid automationId)
	{
		string value = toFileName.Value;
		bool result = false;
		try
		{
			try
			{
				if (InternalDownload(displayDialog: true, dialogTitle, toInitialFolder, ref value, toTypeFilter, source.InternalStreamChecked, automationId))
				{
					toFileName.Value = new NavOemText(toFileName.Value.NavDefinedLengthMetadata, value);
					result = true;
				}
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return result;
	}

	public static bool ALDownload(string fromFileName, string dialogTitle, string toInitialFolder, string toTypeFilter, ByRef<NavText> toFileName, Guid automationId)
	{
		return ALDownload(DataError.ThrowError, fromFileName, dialogTitle, toInitialFolder, toTypeFilter, toFileName, automationId);
	}

	public static bool ALDownload(DataError errorLevel, string fromFileName, string dialogTitle, string toInitialFolder, string toTypeFilter, ByRef<NavText> toFileName, Guid automationId)
	{
		NavOemText navOemTextValue = NavOemText.Empty;
		ByRef<NavOemText> byRef = new ByRef<NavOemText>(() => navOemTextValue, (NavOemText value) =>
		{
			navOemTextValue = value;
		});
		byRef.Value = new NavOemText(2048, toFileName.Value);
		bool result = ALDownload(errorLevel, fromFileName, dialogTitle, toInitialFolder, toTypeFilter, byRef, automationId);
		toFileName.Value = new NavText(byRef.Value.ToString());
		return result;
	}

	public static bool ALDownload(string fromFileName, string dialogTitle, string toInitialFolder, string toTypeFilter, ByRef<NavOemText> toFileName, Guid automationId)
	{
		return ALDownload(DataError.ThrowError, fromFileName, dialogTitle, toInitialFolder, toTypeFilter, toFileName, automationId);
	}

	public static bool ALDownload(DataError errorLevel, string fromFileName, string dialogTitle, string toInitialFolder, string toTypeFilter, ByRef<NavOemText> toFileName, Guid automationId)
	{
		string value = toFileName.Value;
		bool result = false;
		try
		{
			try
			{
				if (InternalDownload(displayDialog: true, dialogTitle, toInitialFolder, ref value, toTypeFilter, fromFileName, automationId))
				{
					toFileName.Value = new NavOemText(toFileName.Value.NavDefinedLengthMetadata, value);
					result = true;
				}
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return result;
	}

	public static bool ALExists(string fileName)
	{
		return ALExists(DataError.ThrowError, fileName);
	}

	public static bool ALExists(DataError errorLevel, string fileName)
	{
		bool flag = false;
		try
		{
			VerifyALFileAccessEnabled();
			try
			{
				string path = ExpandFileName(errorLevel, fileName);
				flag = File.Exists(path);
				if (!flag && errorLevel == DataError.ThrowError)
				{
					File.Open(path, FileMode.Open);
				}
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return flag;
	}

	public static bool ALGetStamp(string fileName, ByRef<NavDate> date)
	{
		return ALGetStamp(DataError.ThrowError, fileName, date);
	}

	public static bool ALGetStamp(DataError errorLevel, string fileName, ByRef<NavDate> date)
	{
		return ALGetStamp(errorLevel, fileName, date, null);
	}

	public static bool ALGetStamp(string fileName, ByRef<NavDate> date, ByRef<NavTime> time)
	{
		return ALGetStamp(DataError.ThrowError, fileName, date, time);
	}

	public static bool ALGetStamp(DataError errorLevel, string fileName, ByRef<NavDate> date, ByRef<NavTime> time)
	{
		try
		{
			VerifyALFileAccessEnabled();
			string path = ExpandFileName(errorLevel, fileName);
			if (!ALExists(path))
			{
				throw new NavNCLFileNotFoundException(path);
			}
			try
			{
				NavDateTime dateTime = NavDateTime.Create(File.GetLastWriteTimeUtc(path));
				date.Value = ALSystemDate.ALDT2Date(dateTime);
				if (time != null)
				{
					time.Value = ALSystemDate.ALDT2Time(dateTime);
				}
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		return true;
	}

	public static bool ALSetStamp(string fileName, NavDate date)
	{
		return ALSetStamp(DataError.ThrowError, fileName, date);
	}

	public static bool ALSetStamp(DataError errorLevel, string fileName, NavDate date)
	{
		return ALSetStamp(errorLevel, fileName, date, NavTime.Default);
	}

	public static bool ALSetStamp(string fileName, NavDate date, NavTime time)
	{
		return ALSetStamp(DataError.ThrowError, fileName, date, time);
	}

	public static bool ALSetStamp(DataError errorLevel, string fileName, NavDate date, NavTime time)
	{
		try
		{
			VerifyALFileAccessEnabled();
			string path = ExpandFileName(errorLevel, fileName);
			if (!ALExists(path))
			{
				throw new NavNCLFileNotFoundException(path);
			}
			try
			{
				File.SetLastWriteTime(path, ALSystemDate.ALCreateDateTime(date, time).Value);
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		return true;
	}

	public NavFile(ITreeObject parent)
		: base(parent)
	{
		textEncoding = TextEncoding.MSDos;
	}

	internal static string ExpandFileName(DataError errorLevel, string fileName, bool enforceUserPath = false)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return fileName;
		}
		try
		{
			string userDirectoryAndEnsureExists = GetUserDirectoryAndEnsureExists(errorLevel, tempPath: false);
			if (!Path.IsPathRooted(fileName))
			{
				fileName = Path.Combine(userDirectoryAndEnsureExists, fileName);
			}
			fileName = Path.GetFullPath(fileName);
			if ((enforceUserPath || (!NavEnvironment.Topology.IsServiceRunningInLocalEnvironment && !FeatureSwitches.Instance.DisableFilePathLockdown)) && !fileName.StartsWith(Path.GetFullPath(userDirectoryAndEnsureExists), StringComparison.OrdinalIgnoreCase))
			{
				throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000282", Category.Runtime, new NavNCLInvalidPathException(Lang.FileAccessLimitedToUserFolder, DiagnosticParameter.Euii(fileName)));
			}
		}
		catch (Exception ex)
		{
			NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
			throw;
		}
		return fileName;
	}

	private bool InternalCreateOrOpen(DataError errorLevel, FileOption options, bool checkFileType)
	{
		VerifyALFileAccessEnabled();
		if (InternalStream != null)
		{
			throw new NavNCLFileAlreadyOpenedException();
		}
		if (string.IsNullOrEmpty(fileName))
		{
			throw new NavNCLMissingFileNameException();
		}
		if (checkFileType && !FileTypeFilter.IsFileTypeAllowedByConfiguration(fileName))
		{
			throw new NavNCLFileLoadException(Lang.FileTypeNotAllowedCreate);
		}
		try
		{
			switch (options)
			{
			case FileOption.Create:
				fileStream = File.Create(fileName, 4096, FileOptions.RandomAccess);
				break;
			case FileOption.CreateTemp:
				fileStream = File.Create(fileName, 4096, FileOptions.RandomAccess | FileOptions.DeleteOnClose);
				break;
			case FileOption.Open:
				fileStream = File.Open(fileName, FileMode.Open, (!writeMode) ? FileAccess.Read : FileAccess.ReadWrite, (!writeMode) ? FileShare.Read : FileShare.None);
				break;
			}
		}
		catch (Exception ex)
		{
			NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
			throw;
		}
		return InternalStream != null;
	}

	internal static void VerifyALFileAccessEnabled()
	{
		if (!ServerUserSettings.Instance.EnableALServerFileAccess.Value)
		{
			throw new NavNCLFileLoadException(Lang.ServerFileAccessNotAllowed);
		}
	}

	internal static string GetUserDirectoryAndEnsureExists(DataError errorLevel, bool tempPath)
	{
		try
		{
			NavSession session = NavCurrentThread.Session;
			string id = session.Tenant.Id;
			UsernameTokens usernameTokens = SplitDomainAndUser(session.User.Name);
			string text = EncodeName(usernameTokens.User);
			string path = EncodeName(usernameTokens.Domain);
			string text2 = (usernameTokens.IsDomainFormat ? Path.Combine(serverUsersFolder, id, path, text) : Path.Combine(serverUsersFolder, id, text));
			if (!Directory.Exists(text2))
			{
				DirectorySecurity directorySecurity = NavDirectorySecurity.CreateSecurityForDomainDirectory();
				DirectoryInfo directoryInfo2;
				if (usernameTokens.IsDomainFormat)
				{
					string path2 = Path.Combine(serverUsersFolder, id, path);
					DirectoryInfo directoryInfo = NavDirectorySecurity.CreateDirectoryWithFolderSecurity(path2, directorySecurity, session);
					directoryInfo2 = directoryInfo.CreateSubdirectory(text);
				}
				else
				{
					directoryInfo2 = Directory.CreateDirectory(Path.Combine(serverUsersFolder, id, text), directorySecurity);
				}
				directoryInfo2.CreateSubdirectory("TEMP\\");
			}
			if (tempPath)
			{
				text2 = Path.Combine(text2, "TEMP\\");
				if (!Directory.Exists(text2))
				{
					Directory.CreateDirectory(text2);
				}
			}
			return text2;
		}
		catch (Exception ex)
		{
			NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
			throw;
		}
	}

	private static string EncodeName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		if (name.IndexOfAny(invalidFileNameChars) < 0)
		{
			return name;
		}
		return EncodeStringAsValidFilenameString(name);
	}

	private static UsernameTokens SplitDomainAndUser(string domainAndUserName)
	{
		if (string.IsNullOrEmpty(domainAndUserName))
		{
			return UsernameTokens.Default();
		}
		string[] array = domainAndUserName.Split('\\');
		UsernameTokens result = default;
		result.IsDomainFormat = array.Length == 2;
		if (result.IsDomainFormat)
		{
			result.Domain = array[0];
			result.User = array[1];
		}
		else
		{
			result.Domain = string.Empty;
			result.User = domainAndUserName;
		}
		result.IsDomainFormat &= !string.IsNullOrEmpty(result.Domain);
		return result;
	}

	private static string EncodeStringAsValidFilenameString(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return string.Empty;
		}
		byte[] array = new byte[str.Length * 2];
		Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
		return Convert.ToBase64String(array).Replace('/', '_');
	}

	internal static string GetTempFileName()
	{
		return CreateTempFileName(DataError.ThrowError, string.Empty);
	}

	private static string CreateTempFileName(DataError errorLevel, string tempDir)
	{
		if (string.IsNullOrEmpty(tempDir))
		{
			tempDir = GetUserDirectoryAndEnsureExists(errorLevel, tempPath: true);
		}
		return ExpandFileName(errorLevel, Path.GetFullPath(Path.Combine(tempDir, "__TEMP__" + Guid.NewGuid().ToString("N") + ".tmp")));
	}

	private void InternalClose(DataError errorLevel)
	{
		try
		{
			if (fileStream == null)
			{
				return;
			}
			try
			{
				fileStream.Close();
				if (binaryReader != null)
				{
					binaryReader.Close();
				}
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, errorLevel == DataError.ThrowError);
				throw;
			}
		}
		finally
		{
			fileStream = null;
			binaryReader = null;
			fileName = string.Empty;
		}
	}

	private static bool InternalDownload(bool displayDialog, string title, string initialFolder, ref string fileName, string typeFilter, string localPath, Guid automationId)
	{
		VerifyALFileAccessEnabled();
		if (string.IsNullOrEmpty(localPath))
		{
			throw new NavNCLMissingFileNameException();
		}
		if (!displayDialog && string.IsNullOrEmpty(fileName))
		{
			throw new NavNCLMissingFileNameException();
		}
		IClientCallback clientCallback = NavCurrentThread.Session.ClientCallback;
		using NavFile navFile = new NavFile(NavCurrentThread.Session);
		navFile.ALOpen(localPath);
		return clientCallback.DownloadFileAction(navFile.InternalStreamChecked, displayDialog, title, initialFolder, typeFilter, ref fileName, automationId);
	}

	internal static bool InternalDownload(bool displayDialog, string title, string initialFolder, ref string fileName, string typeFilter, Stream source, Guid automationId)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (!displayDialog && string.IsNullOrEmpty(fileName))
		{
			throw new NavNCLMissingFileNameException();
		}
		return NavCurrentThread.Session.ClientCallback.DownloadFileAction(source, displayDialog, title, initialFolder, typeFilter, ref fileName, automationId);
	}

	private static bool InternalUpload(DataError errorLevel, bool displayDialog, string title, string initialFolder, string fileName, string typeFilter, ref string localPath, Guid automationId)
	{
		VerifyALFileAccessEnabled();
		FileBufferedStream uploadedStream = GetUploadedStream(displayDialog, title, initialFolder, ref fileName, typeFilter, automationId);
		if (uploadedStream != null)
		{
			fileName = (string.IsNullOrEmpty(localPath) ? Path.GetFileName(fileName) : ((!string.IsNullOrEmpty(Path.GetFileName(localPath))) ? localPath : Path.Combine(Path.GetDirectoryName(localPath), Path.GetFileName(fileName))));
			string persistedFilePath = uploadedStream.PersistedFilePath;
			uploadedStream.Dispose();
			try
			{
				ALErase(DataError.TrapError, fileName);
				ALRename(persistedFilePath, fileName);
				localPath = ExpandFileName(errorLevel, fileName);
			}
			finally
			{
				if (File.Exists(persistedFilePath))
				{
					File.Delete(persistedFilePath);
				}
			}
			return true;
		}
		return false;
	}

	internal static FileBufferedStream InternalUpload(bool displayDialog, string title, string initialFolder, ref string fileName, string typeFilter, Guid automationId)
	{
		return GetUploadedStream(displayDialog, title, initialFolder, ref fileName, typeFilter, automationId);
	}

	private static FileBufferedStream GetUploadedStream(bool displayDialog, string title, string initialFolder, ref string fileName, string typeFilter, Guid automationId)
	{
		if (!displayDialog && string.IsNullOrEmpty(fileName))
		{
			throw new NavNCLMissingFileNameException();
		}
		return NavCurrentThread.Session.ClientCallback.UploadFileAction(displayDialog, title, initialFolder, typeFilter, ref fileName, automationId);
	}

	public static NavFile Default(ITreeObject parent)
	{
		return new NavFile(parent);
	}

	public NavFile Clone(ITreeObject parentForClone)
	{
		NavFile navFile = new NavFile(parentForClone);
		navFile.ALAssign(this);
		return navFile;
	}

	public void ALAssign(NavFile other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (this != other)
		{
			InternalClose(DataError.ThrowError);
			writeMode = other.writeMode;
			textMode = other.textMode;
			if (other.fileStream != null)
			{
				fileName = other.fileName;
				InternalCreateOrOpen(DataError.ThrowError, FileOption.Open, checkFileType: false);
			}
		}
	}

	public NavFile ALByValue(ITreeObject parentOfResult)
	{
		return Clone(parentOfResult);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !IsDisposed)
			{
				InternalClose(DataError.ThrowError);
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public override void Clear()
	{
		InternalClose(DataError.ThrowError);
	}

	public override int GetHashCode()
	{
		if (fileName != null)
		{
			return fileName.GetHashCode();
		}
		return -1;
	}

	Stream INavStreamProvider.GetStream()
	{
		return fileStream;
	}

	string INavStreamReader.ReadChar()
	{
		if (textEncoding == TextEncoding.UTF16 || textEncoding == TextEncoding.UTF8)
		{
			try
			{
				return BinaryReader.ReadChar().ToString();
			}
			catch (EndOfStreamException)
			{
				return string.Empty;
			}
			catch (ArgumentException)
			{
				throw new NavNCLArgumentException(Lang.InvalidDataInStream);
			}
			catch (IOException)
			{
				throw new NavNCLStreamIOException();
			}
		}
		throw new NotSupportedException();
	}

	int INavStreamReader.ReadByte()
	{
		if (textEncoding == TextEncoding.UTF16 || textEncoding == TextEncoding.UTF8)
		{
			try
			{
				return BinaryReader.ReadByte();
			}
			catch (IOException)
			{
				throw new NavNCLStreamIOException();
			}
		}
		return InternalStreamCheckedForRead.ReadByte();
	}

	int INavStreamReader.PeekByte()
	{
		int num = InternalStream.ReadByte();
		if (num >= 0)
		{
			InternalStream.Seek(-1L, SeekOrigin.Current);
		}
		return num;
	}

	byte[] INavStreamReader.ReadBytes(int length)
	{
		if (textEncoding == TextEncoding.UTF16 || textEncoding == TextEncoding.UTF8)
		{
			try
			{
				return BinaryReader.ReadBytes(length);
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new NavNCLArgumentOutOfRangeException();
			}
			catch (ArgumentException)
			{
				throw new NavNCLArgumentException(Lang.InvalidDataInStream);
			}
			catch (IOException)
			{
				throw new NavNCLStreamIOException();
			}
		}
		return NCLStreamHelper.ReadToByteArray(InternalStreamCheckedForRead, length);
	}

	byte[] INavStreamReader.PeekBytes(int length)
	{
		byte[] array = new byte[length];
		Array.Resize(ref array, InternalStream.Read(array, 0, length));
		if (array.Length >= 0)
		{
			InternalStream.Seek(-array.Length, SeekOrigin.Current);
		}
		return array;
	}

	void INavStreamWriter.WriteByte(byte value)
	{
		InternalStreamCheckedForWrite.WriteByte(value);
	}

	void INavStreamWriter.WriteBytes(byte[] bytes, int length)
	{
		InternalStreamCheckedForWrite.Write(bytes, 0, length);
	}
}
