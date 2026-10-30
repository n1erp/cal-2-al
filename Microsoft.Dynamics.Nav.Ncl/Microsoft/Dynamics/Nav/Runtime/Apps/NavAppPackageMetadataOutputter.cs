using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppPackageMetadataOutputter : CodeModuleOutputter, INavAppPackageMetadata
{
	private IModuleSymbol module;

	private readonly List<byte[]> contentHashItems = new List<byte[]>();

	public PackageId PackageId { get; set; }

	public NavAppManifest Manifest { get; set; }

	public NavAppMediaIdListing MediaIdListing { get; set; }

	public IEnumerable<NavAppMedia> Media { get; set; }

	public IList<NavAppObjectMetadata> ObjectMetadata { get; }

	public NavAppCombinedProfileMetadata CombinedProfileMetadata { get; }

	public byte[] Symbols { get; set; }

	public NavAppPackageType PackageType { get; set; }

	public string ContentHash { get; private set; } = string.Empty;

	public NavAppPackageMetadataOutputter(EmitOptions emitOptions = null)
		: base(emitOptions)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		ObjectMetadata = new List<NavAppObjectMetadata>();
		CombinedProfileMetadata = new NavAppCombinedProfileMetadata();
	}

	public override void InitializeModule(IModuleSymbol moduleSymbol)
	{
		module = moduleSymbol;
	}

	public override void FinalizeModule()
	{
		WriteSymbolReference();
		ContentHash = NavAppContentHasher.ComputeHash((IEnumerable<byte[]>)contentHashItems, (NavAppHashAlgorithm)1);
	}

	protected void WriteSymbolReference()
	{
		if (module != null)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				CompilationUtilities.WriteSymbolReference(memoryStream, module);
				Symbols = memoryStream.GetBuffer();
			}
		}
	}

	public override void AddApplicationObject(IApplicationObjectTypeSymbol symbol, byte[] code, string metadata, string debugCode)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected Obj, but got Unknown
		NavAppObjectFormat format = symbol.NavTypeKind.ToObjectFormat();
		string userCode = string.Empty;
		if (code != null && code.Length != 0)
		{
			userCode = Encoding.UTF8.GetString(code);
		}
		Sha256Hash metadataHash = (string.IsNullOrWhiteSpace(metadata) ? null : Sha256Hash.ComputeHashForContent(Encoding.UTF8.GetBytes(metadata)));
		Sha256Hash userCodeHash = ((code == null) ? null : Sha256Hash.ComputeHashForContent(code));
		ObjectMetadata.Add(new NavAppObjectMetadata
		{
			AppPackageId = PackageId,
			ObjectType = symbol.NavTypeKind.ToObjectType(),
			ObjectId = symbol.Id,
			Format = format,
			MetadataVersion = 130000,
			ObjectSubtype = symbol.GetApplicationObjectSubtype(),
			ObjectName = symbol.Name,
			UserCode = userCode,
			UserCodeHash = userCodeHash,
			Metadata = metadata,
			MetadataHash = metadataHash,
			UserALCode = debugCode,
			ObjectFlags = NavAppObjectFlagsCalculator.CalculateFlags(symbol.NavTypeKind.ToObjectType(), userCode)
		});
		contentHashItems.Add(Encoding.Unicode.GetBytes(debugCode));
	}

	public override void AddProfileObject(ISymbol symbol, byte[] code, string metadata, string debugCode)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Profile:
			ProcessProfileMetadata(metadata);
			contentHashItems.Add(Encoding.Unicode.GetBytes(debugCode));
			break;
		case SymbolKind.PageCustomization:
			ProcessProfilePageMetadata((IApplicationObjectExtensionTypeSymbol)symbol, metadata, debugCode);
			contentHashItems.Add(Encoding.Unicode.GetBytes(debugCode));
			break;
		}
	}

	private void ProcessProfileMetadata(string metadata)
	{
		XDocument xDocument = XDocument.Parse(metadata);
		NavAppProfileMetadata item = NavAppProfileMetadata.FromXml(xDocument);
		CombinedProfileMetadata.ProfileMetadata.Add(item);
	}

	private void ProcessProfilePageMetadata(IApplicationObjectExtensionTypeSymbol symbol, string metadata, string debugCode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected Obj, but got Unknown
		NavAppProfilePageMetadata item = new NavAppProfilePageMetadata
		{
			TargetPageId = symbol.Target.Id,
			Name = symbol.Name,
			Metadata = metadata,
			UserALCode = debugCode
		};
		CombinedProfileMetadata.ProfilePageMetadata.Add(item);
	}

	public override ImmutableArray<Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic> GetDiagnostics()
	{
		return ImmutableArray<Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic>.Empty;
	}
}
