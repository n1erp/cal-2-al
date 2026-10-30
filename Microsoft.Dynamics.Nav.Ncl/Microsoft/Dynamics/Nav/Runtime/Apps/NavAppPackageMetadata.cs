using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppPackageMetadata : INavAppPackageMetadata
{
	public PackageId PackageId { get; }

	public NavAppManifest Manifest { get; }

	public NavAppMediaIdListing MediaIdListing { get; }

	public IEnumerable<NavAppMedia> Media { get; }

	public IList<NavAppObjectMetadata> ObjectMetadata { get; }

	public NavAppCombinedProfileMetadata CombinedProfileMetadata { get; }

	public byte[] Symbols { get; }

	public string ContentHash { get; private set; }

	public NavAppPackageType PackageType { get; }

	private NavAppPackageMetadata(PackageId packageId, NavAppManifest manifest, NavAppMediaIdListing mediaIdListing, IEnumerable<NavAppMedia> media, byte[] symbols, NavAppPackageType packageType)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		ContentHash = string.Empty;
		ObjectMetadata = new List<NavAppObjectMetadata>();
		CombinedProfileMetadata = new NavAppCombinedProfileMetadata();
		PackageId = packageId;
		Manifest = manifest;
		MediaIdListing = mediaIdListing;
		Media = media;
		Symbols = symbols;
		PackageType = packageType;
	}

	public static Result<NavAppPackageMetadata> Create(NavAppPackageReader reader, NavAppPackageType packageType)
	{
		NavAppPackageMetadata builder = new NavAppPackageMetadata(PackageId.op_Implicit(reader.PackageId), reader.ReadNavAppManifest(), reader.MediaIdListing, reader.ResolvePackageMedia(), reader.ResolvePackageSymbols(), packageType);
		return ResultExtensions.OnSuccess<NavAppPackageMetadata>(builder.FillFromRuntimePackage(reader), (Func<NavAppPackageMetadata>)(() => builder));
	}

	private Result FillFromRuntimePackage(NavAppPackageReader reader)
	{
		try
		{
			Stream stream = reader.ReadFile("/bin/EmittedContent.json");
			ICollection<PackageEmittedContent> collection = PackageEmittedContentWriter.ReadContent(stream);
			foreach (PackageEmittedContent item in collection)
			{
				if (item.MetadataVersion != 130000)
				{
					return Result.Fail(Error.FromMessage(string.Format(CultureInfo.CurrentCulture, Lang.RuntimePackageVersionMismatch, item.MetadataVersion, 130000), ErrorType.Unspecified));
				}
				if (item.IsProfileObject)
				{
					AddCustomizationMetadata(reader, item);
				}
				else
				{
					ObjectMetadata.Add(ConvertToObjectMetadata(reader, item));
				}
			}
			ContentHash = CalculateHash();
			return Result.Ok();
		}
		catch (Exception ex)
		{
			return Result.Fail(Error.FromException(ex));
		}
	}

	private void AddCustomizationMetadata(NavAppPackageReader reader, PackageEmittedContent emittedObject)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected Obj, but got Unknown
		switch (emittedObject.TypeKind)
		{
		case NavTypeKind.Profile:
		{
			string text = reader.ReadFileAsString(emittedObject.MetadataFileName);
			XDocument xDocument = XDocument.Parse(text);
			NavAppProfileMetadata item = NavAppProfileMetadata.FromXml(xDocument);
			CombinedProfileMetadata.ProfileMetadata.Add(item);
			break;
		}
		case NavTypeKind.PageCustomization:
		{
			NavAppProfilePageMetadata val = new NavAppProfilePageMetadata
			{
				Name = emittedObject.Name
			};
			if (int.TryParse(emittedObject.ApplicationObjectSubtype, out var result))
			{
				val.TargetPageId = result;
			}
			val.Metadata = reader.ReadFileAsString(emittedObject.MetadataFileName);
			CombinedProfileMetadata.ProfilePageMetadata.Add(val);
			break;
		}
		}
	}

	private NavAppObjectMetadata ConvertToObjectMetadata(NavAppPackageReader reader, PackageEmittedContent emittedObject)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected Obj, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		NavAppObjectMetadata val = new NavAppObjectMetadata
		{
			AppPackageId = PackageId,
			ObjectName = emittedObject.Name,
			ObjectType = emittedObject.TypeKind.ToObjectType(),
			ObjectSubtype = emittedObject.ApplicationObjectSubtype,
			Format = emittedObject.TypeKind.ToObjectFormat(),
			MetadataVersion = emittedObject.MetadataVersion
		};
		if (emittedObject.Id.HasValue)
		{
			val.ObjectId = emittedObject.Id.Value;
		}
		if (!string.IsNullOrEmpty(emittedObject.CodeFileName))
		{
			string text = reader.ReadFileAsString(emittedObject.CodeFileName);
			Sha256Hash userCodeHash = (string.IsNullOrWhiteSpace(text) ? null : Sha256Hash.ComputeHashForContent(Encoding.UTF8.GetBytes(text)));
			val.UserCode = text;
			val.UserCodeHash = userCodeHash;
			val.ObjectFlags = NavAppObjectFlagsCalculator.CalculateFlags(val.ObjectType, text);
		}
		if (!string.IsNullOrEmpty(emittedObject.MetadataFileName))
		{
			string text2 = reader.ReadFileAsString(emittedObject.MetadataFileName);
			Sha256Hash metadataHash = (string.IsNullOrWhiteSpace(text2) ? null : Sha256Hash.ComputeHashForContent(Encoding.UTF8.GetBytes(text2)));
			val.Metadata = text2;
			val.MetadataHash = metadataHash;
		}
		if (!string.IsNullOrEmpty(emittedObject.SourceFileName))
		{
			val.UserALCode = reader.ReadFileAsString(emittedObject.SourceFileName);
		}
		return val;
	}

	private string CalculateHash()
	{
		List<byte[]> list = new List<byte[]>();
		foreach (NavAppObjectMetadata objectMetadatum in ObjectMetadata)
		{
			if (!string.IsNullOrEmpty(objectMetadatum.UserCode))
			{
				list.Add(Encoding.Unicode.GetBytes(objectMetadatum.UserCode));
			}
			if (!string.IsNullOrEmpty(objectMetadatum.Metadata))
			{
				list.Add(Encoding.Unicode.GetBytes(objectMetadatum.Metadata));
			}
		}
		return NavAppContentHasher.ComputeHash((IEnumerable<byte[]>)list, (NavAppHashAlgorithm)1);
	}
}
