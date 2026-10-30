using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

public class NavMediaImport : ITreeObject, IDisposable
{
	private readonly TreeHandler tree;

	private readonly string companyName;

	private static readonly List<Tuple<int, int>> PrescaleDimensions = new List<Tuple<int, int>>
	{
		new Tuple<int, int>(70, 70),
		new Tuple<int, int>(240, 240),
		new Tuple<int, int>(360, 0)
	};

	public TreeHandler Tree => tree;

	public bool IsDisposed => Tree.IsDisposed;

	public NavMediaImport(ITreeObject parent, string companyName)
	{
		tree = TreeHandler.CreateTreeHandler(parent, this);
		this.companyName = companyName ?? string.Empty;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Tree.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal Guid ImportMediaObject(int parentTableId, Stream mediaStream, string fileName, string description, string mimeType = "")
	{
		if (mediaStream.CanSeek && mediaStream.CanWrite && !mediaStream.CanTimeout)
		{
			return ImportMediaObjectCore(parentTableId, mediaStream, fileName, description, mimeType);
		}
		using MemoryStream memoryStream = MemoryStreamManager.GetStream();
		mediaStream.CopyTo(memoryStream);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return ImportMediaObjectCore(parentTableId, memoryStream, fileName, description, mimeType);
	}

	internal Guid ImportMediaObject(int parentTableId, Stream mediaStream, string description, string mimeType = "")
	{
		return ImportMediaObject(parentTableId, mediaStream, string.Empty, description, mimeType);
	}

	private Guid ImportMediaObjectCore(int parentTableId, Stream mediaStream, string fileName, string description, string mimeType)
	{
		if (mediaStream == null || mediaStream.Length == 0L)
		{
			throw new NavEmptyMediaStreamException();
		}
		if (parentTableId == 0)
		{
			throw new NavNCLArgumentException();
		}
		using NavMediaBase navMediaBase = NavMediaFactory.ProcessMediaObject(mediaStream, saveStream: false, mimeType);
		int mediaTableId = NavMediaHelper.GetMediaTableId(parentTableId);
		NavSession session = Tree.Session;
		Guid guid;
		using (NavRecord navRecord = session.GetGlobalRecordInstance(mediaTableId))
		{
			guid = Guid.NewGuid();
			if (string.IsNullOrEmpty(mimeType))
			{
				mimeType = navMediaBase.MimeType;
			}
			DiagnosticsResolver.GetMostSpecificInstance(session).SendTraceTag("000027L", Category.Runtime, Verbosity.Normal, "Import media object of type {0}", DiagnosticParameter.SystemMetadata(mimeType));
			navRecord.ALInit();
			navRecord.SetFieldValue(1, new NavGuid(guid));
			navRecord.SetFieldValue(3, new NavBLOB(navMediaBase.Bytes()));
			navRecord.SetFieldValue(6, NavInteger.Create(navMediaBase.Width));
			navRecord.SetFieldValue(5, NavInteger.Create(navMediaBase.Height));
			navRecord.SetFieldValue(4, new NavText(navRecord.GetFieldValue(4).NavDefinedLengthMetadata, mimeType));
			navRecord.SetFieldValue(11, new NavText(navRecord.GetFieldValue(11).NavDefinedLengthMetadata, fileName));
			navRecord.SetFieldValue(2, new NavText(navRecord.GetFieldValue(2).NavDefinedLengthMetadata, description));
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(parentTableId);
			if (metaTableById != null && metaTableById.DataPerCompany)
			{
				navRecord.SetFieldValue(7, new NavText(navRecord.GetFieldValue(7).NavDefinedLengthMetadata, companyName));
			}
			navRecord.ALInsert(DataError.ThrowError);
			session.Diagnostics.SendTraceTag("000017D", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, FormattableString.Invariant($"Imported Media object of type {mimeType} into table {parentTableId} with id {guid}"));
			if (navMediaBase.AllowCustomThumbnails)
			{
				byte[] builtinThumbnailData = navMediaBase.BuiltinThumbnailData;
				if (builtinThumbnailData != null && builtinThumbnailData.Length != 0)
				{
					NavMediaHelper.InsertThumbnail(navRecord, guid, builtinThumbnailData, navMediaBase.BuiltinThumbnailWidth, navMediaBase.BuiltinThumbnailHeight, navMediaBase.BuiltinThumbnailMimeType, embedded: true);
				}
				CreatePrescaledThumbnails(navRecord, navMediaBase, guid);
			}
		}
		return guid;
	}

	private static void CreatePrescaledThumbnails(NavRecord parent, NavMediaBase navImage, Guid mediaId)
	{
		if (!ServerUserSettings.Instance.EnablePreScaledMedia.Value)
		{
			return;
		}
		foreach (Tuple<int, int> prescaleDimension in PrescaleDimensions)
		{
			int item = prescaleDimension.Item1;
			int item2 = prescaleDimension.Item2;
			NavMediaThumbnail navMediaThumbnail = ((item2 == 0) ? navImage.Scale(item) : navImage.Scale(item, item2));
			NavMediaHelper.InsertThumbnail(parent, mediaId, navMediaThumbnail.Data, navMediaThumbnail.Width, navMediaThumbnail.Height, navImage.ThumbnailMimeType);
		}
	}
}
