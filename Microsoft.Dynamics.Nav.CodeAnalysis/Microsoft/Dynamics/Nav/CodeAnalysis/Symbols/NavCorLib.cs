using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class NavCorLib : ModuleSymbol
{
	internal static readonly TypeSymbol ActionType = ActionSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol ClientTypeType = ClientTypeSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol DataClassificationType = DataClassificationSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol DataScopeType = DataScopeSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol DefaultLayoutType = DefaultLayoutSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol ErrorTypeType = ErrorTypeSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol ExecutionContextType = ExecutionContextSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol ExecutionModeType = ExecutionModeSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol FieldClassType = FieldClassSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol FieldTypeType = FieldTypeSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol NotificationScopeType = NotificationScopeSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol ObjectTypeType = ObjectTypeSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol ReportFormatType = ReportFormatSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol SecurityFilterType = SecurityFilterSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol TableConnectionTypeType = TableConnectionTypeSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol TestPermissionsType = TestPermissionsSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol TextEncodingType = TextEncodingSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol TransactionModelType = TransactionModelSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol TransactionTypeType = TransactionTypeSystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol VerbosityType = VerbositySystemOptionTypeSymbol.Instance;

	internal static readonly TypeSymbol WebServiceActionResultCodeType = WebServiceActionResultCodeSystemOptionTypeSymbol.Instance;

	private static readonly Lazy<NavCorLib> instance = new Lazy<NavCorLib>(LazyThreadSafetyMode.ExecutionAndPublication);

	private readonly ImmutableArray<Symbol> members;

	private readonly ImmutableSortedDictionary<string, Symbol> memberMap;

	private readonly ImmutableArray<Symbol> membersWithImplicitAccess;

	internal static readonly TypeSymbol None = new SimpleNavTypeSymbol(NavTypeKind.None);

	internal static readonly TypeSymbol NoneType = None;

	internal static readonly TypeSymbol ByteType = new SimpleNavTypeSymbol(NavTypeKind.Byte);

	internal static readonly TypeSymbol CharType = new SimpleNavTypeSymbol(NavTypeKind.Char);

	internal static readonly TypeSymbol IntegerType = new SimpleNavTypeSymbol(NavTypeKind.Integer);

	internal static readonly TypeSymbol DateTimeType = new SimpleNavTypeSymbol(NavTypeKind.DateTime);

	internal static readonly TypeSymbol BooleanType = new SimpleNavTypeSymbol(NavTypeKind.Boolean);

	internal static readonly TypeSymbol CodeType = new SimpleNavTypeSymbol(NavTypeKind.Code);

	internal static readonly TypeSymbol DateFormulaType = new SimpleNavTypeSymbol(NavTypeKind.DateFormula);

	internal static readonly TypeSymbol DurationType = new SimpleNavTypeSymbol(NavTypeKind.Duration);

	internal static readonly TypeSymbol TextType = TextTypeSymbol.GetInstance(NavTypeKind.Text);

	internal static readonly TypeSymbol Text1024Type = TextTypeSymbol.GetInstance(NavTypeKind.Text, 1024);

	internal static readonly TypeSymbol TextConstType = new SimpleNavTypeSymbol(NavTypeKind.TextConst);

	internal static readonly TypeSymbol OemTextType = new SimpleNavTypeSymbol(NavTypeKind.OemText);

	internal static readonly TypeSymbol LabelType = new SimpleNavTypeSymbol(NavTypeKind.Label);

	internal static readonly TypeSymbol JokerType = new SimpleNavTypeSymbol(NavTypeKind.Joker);

	internal static readonly TypeSymbol StringType = new SimpleNavTypeSymbol(NavTypeKind.String, () => TextClassTypeSymbol.Instance, NavTypeKind.Text.ToString());

	internal static readonly TypeSymbol BigIntegerType = new SimpleNavTypeSymbol(NavTypeKind.BigInteger);

	internal static readonly TypeSymbol NotificationType = new SimpleNavTypeSymbol(NavTypeKind.Notification, () => NotificationClassTypeSymbol.Instance);

	internal static readonly TypeSymbol DateType = new SimpleNavTypeSymbol(NavTypeKind.Date);

	internal static readonly TypeSymbol TimeType = new SimpleNavTypeSymbol(NavTypeKind.Time);

	internal static readonly TypeSymbol DecimalType = new SimpleNavTypeSymbol(NavTypeKind.Decimal);

	internal static readonly TypeSymbol DotNetType = new SimpleNavTypeSymbol(NavTypeKind.DotNet);

	internal static readonly TypeSymbol GuidType = new SimpleNavTypeSymbol(NavTypeKind.Guid);

	internal static readonly TypeSymbol InStreamType = new SimpleNavTypeSymbol(NavTypeKind.InStream, () => InStreamClassTypeSymbol.Instance);

	internal static readonly TypeSymbol OutStreamType = new SimpleNavTypeSymbol(NavTypeKind.OutStream, () => OutStreamClassTypeSymbol.Instance);

	internal static readonly TypeSymbol VariantType = new SimpleNavTypeSymbol(NavTypeKind.Variant, () => VariantClassTypeSymbol.Instance);

	internal static readonly TypeSymbol TableType = new SimpleNavTypeSymbol(NavTypeKind.Table);

	internal static readonly TypeSymbol RecordType = new SimpleNavTypeSymbol(NavTypeKind.Record);

	internal static readonly TypeSymbol CodeunitType = new SimpleNavTypeSymbol(NavTypeKind.AnyCodeunit, null, NavTypeKind.Codeunit.ToString());

	internal static readonly TypeSymbol RecordIdType = new SimpleNavTypeSymbol(NavTypeKind.RecordId, () => RecordIdClassTypeSymbol.Instance);

	internal static readonly TypeSymbol FieldRefType = new SimpleNavTypeSymbol(NavTypeKind.FieldRef, () => FieldRefClassTypeSymbol.Instance);

	internal static readonly TypeSymbol KeyRefType = new SimpleNavTypeSymbol(NavTypeKind.KeyRef, () => KeyRefClassTypeSymbol.Instance);

	internal static readonly TypeSymbol RecordRefType = new SimpleNavTypeSymbol(NavTypeKind.RecordRef, () => RecordRefClassTypeSymbol.Instance);

	internal static readonly TypeSymbol TestActionType = new SimpleNavTypeSymbol(NavTypeKind.TestAction, () => TestActionClassTypeSymbol.Instance);

	internal static readonly TypeSymbol TestFieldType = new SimpleNavTypeSymbol(NavTypeKind.TestField, () => TestFieldClassTypeSymbol.Instance);

	internal static readonly TypeSymbol TestPartType = new SimpleNavTypeSymbol(NavTypeKind.TestPart, () => TestPartClassTypeSymbol.Instance);

	internal static readonly TypeSymbol TestFilterFieldType = new SimpleNavTypeSymbol(NavTypeKind.TestFilterField);

	internal static readonly TypeSymbol TestFilterType = new SimpleNavTypeSymbol(NavTypeKind.TestFilter, () => TestFilterClassTypeSymbol.Instance);

	internal static readonly TypeSymbol OptionType = new SimpleNavTypeSymbol(NavTypeKind.Option);

	internal static readonly TypeSymbol TableFilterType = new SimpleNavTypeSymbol(NavTypeKind.TableFilter);

	internal static readonly TypeSymbol BlobType = new SimpleNavTypeSymbol(NavTypeKind.Blob, () => BlobClassTypeSymbol.Instance);

	internal static readonly TypeSymbol MediaType = new SimpleNavTypeSymbol(NavTypeKind.Media, () => MediaClassTypeSymbol.Instance);

	internal static readonly TypeSymbol MediaSetType = new SimpleNavTypeSymbol(NavTypeKind.MediaSet, () => MediaSetClassTypeSymbol.Instance);

	internal static readonly TypeSymbol DialogType = new SimpleNavTypeSymbol(NavTypeKind.Dialog, () => DialogClassTypeSymbol.Instance);

	internal static readonly TypeSymbol BigTextType = new SimpleNavTypeSymbol(NavTypeKind.BigText, () => BigTextClassTypeSymbol.Instance);

	internal static readonly TypeSymbol FileType = new SimpleNavTypeSymbol(NavTypeKind.File, () => FileClassTypeSymbol.Instance);

	internal static readonly TypeSymbol FilterPageBuilderType = new SimpleNavTypeSymbol(NavTypeKind.FilterPageBuilder, () => FilterPageBuilderClassTypeSymbol.Instance);

	internal static readonly TypeSymbol MethodReferenceType = new SimpleNavTypeSymbol(NavTypeKind.MethodReference);

	internal static readonly TypeSymbol SessionSettingsType = new SimpleNavTypeSymbol(NavTypeKind.SessionSettings, () => SessionSettingsClassTypeSymbol.Instance);

	internal static readonly TypeSymbol QueryFilterType = new SimpleNavTypeSymbol(NavTypeKind.QueryFilter);

	internal static readonly TypeSymbol JsonTokenType = new SimpleNavTypeSymbol(NavTypeKind.JsonToken, () => JsonTokenClassTypeSymbol.Instance);

	internal static readonly TypeSymbol JsonObjectType = new SimpleNavTypeSymbol(NavTypeKind.JsonObject, () => JsonObjectClassTypeSymbol.Instance);

	internal static readonly TypeSymbol JsonArrayType = new SimpleNavTypeSymbol(NavTypeKind.JsonArray, () => JsonArrayClassTypeSymbol.Instance);

	internal static readonly TypeSymbol JsonValueType = new SimpleNavTypeSymbol(NavTypeKind.JsonValue, () => JsonValueClassTypeSymbol.Instance);

	internal static readonly TypeSymbol HttpClientType = new SimpleNavTypeSymbol(NavTypeKind.HttpClient, () => HttpClientClassTypeSymbol.Instance);

	internal static readonly TypeSymbol HttpContentType = new SimpleNavTypeSymbol(NavTypeKind.HttpContent, () => HttpContentClassTypeSymbol.Instance);

	internal static readonly TypeSymbol HttpRequestMessageType = new SimpleNavTypeSymbol(NavTypeKind.HttpRequestMessage, () => HttpRequestMessageClassTypeSymbol.Instance);

	internal static readonly TypeSymbol HttpResponseMessageType = new SimpleNavTypeSymbol(NavTypeKind.HttpResponseMessage, () => HttpResponseMessageClassTypeSymbol.Instance);

	internal static readonly TypeSymbol HttpHeadersType = new SimpleNavTypeSymbol(NavTypeKind.HttpHeaders, () => HttpHeadersClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlAttributeType = new SimpleNavTypeSymbol(NavTypeKind.XmlAttribute, () => XmlAttributeClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlAttributeCollectionType = new SimpleNavTypeSymbol(NavTypeKind.XmlAttributeCollection, () => XmlAttributeCollectionClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlCommentType = new SimpleNavTypeSymbol(NavTypeKind.XmlComment, () => XmlCommentClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlCDataType = new SimpleNavTypeSymbol(NavTypeKind.XmlCData, () => XmlCDataClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlDeclarationType = new SimpleNavTypeSymbol(NavTypeKind.XmlDeclaration, () => XmlDeclarationClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlDocumentType = new SimpleNavTypeSymbol(NavTypeKind.XmlDocument, () => XmlDocumentClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlDocumentTypeType = new SimpleNavTypeSymbol(NavTypeKind.XmlDocumentType, () => XmlDocumentTypeClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlElementType = new SimpleNavTypeSymbol(NavTypeKind.XmlElement, () => XmlElementClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlNamespaceManagerType = new SimpleNavTypeSymbol(NavTypeKind.XmlNamespaceManager, () => XmlNamespaceManagerClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlNameTableType = new SimpleNavTypeSymbol(NavTypeKind.XmlNameTable, () => XmlNameTableClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlNodeType = new SimpleNavTypeSymbol(NavTypeKind.XmlNode, () => XmlNodeClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlNodeListType = new SimpleNavTypeSymbol(NavTypeKind.XmlNodeList, () => XmlNodeListClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlProcessingInstructionType = new SimpleNavTypeSymbol(NavTypeKind.XmlProcessingInstruction, () => XmlProcessingInstructionClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlTextType = new SimpleNavTypeSymbol(NavTypeKind.XmlText, () => XmlTextClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlReadOptionsType = new SimpleNavTypeSymbol(NavTypeKind.XmlReadOptions, () => XmlReadOptionsClassTypeSymbol.Instance);

	internal static readonly TypeSymbol XmlWriteOptionsType = new SimpleNavTypeSymbol(NavTypeKind.XmlWriteOptions, () => XmlWriteOptionsClassTypeSymbol.Instance);

	internal static readonly TypeSymbol TestPageType = new SimpleNavTypeSymbol(NavTypeKind.TestPage, () => TestPageClassTypeSymbol.Instance);

	internal static readonly TypeSymbol TestRequestPageType = new SimpleNavTypeSymbol(NavTypeKind.TestRequestPage, () => TestRequestPageClassTypeSymbol.Instance);

	internal static readonly TypeSymbol TextBuilderType = new SimpleNavTypeSymbol(NavTypeKind.TextBuilder, () => TextBuilderClassTypeSymbol.Instance);

	internal static readonly TypeSymbol PageType = new SimpleNavTypeSymbol(NavTypeKind.Page, () => PageClassTypeSymbol.Instance);

	internal static readonly TypeSymbol ReportType = new SimpleNavTypeSymbol(NavTypeKind.Report, () => ReportClassTypeSymbol.Instance);

	internal static readonly TypeSymbol VersionType = new SimpleNavTypeSymbol(NavTypeKind.Version, () => VersionClassTypeSymbol.Instance);

	internal static readonly TypeSymbol ModuleInfoType = new SimpleNavTypeSymbol(NavTypeKind.ModuleInfo, () => ModuleInfoClassTypeSymbol.Instance);

	internal static readonly TypeSymbol ModuleDependencyInfoType = new SimpleNavTypeSymbol(NavTypeKind.ModuleDependencyInfo, () => ModuleDependencyInfoClassTypeSymbol.Instance);

	internal static readonly TypeSymbol WebServiceActionContextType = new SimpleNavTypeSymbol(NavTypeKind.WebServiceActionContext, () => WebServiceActionContextClassTypeSymbol.Instance);

	internal static readonly TypeSymbol ErrorInfoType = new SimpleNavTypeSymbol(NavTypeKind.ErrorInfo, () => ErrorInfoClassTypeSymbol.Instance);

	internal override IDotNetResolver DotNetResolver => NullDotNetResolver.Instance;

	internal static NavCorLib Instance => instance.Value;

	public override string PublisherName { get; }

	public override Version Version { get; }

	public override Guid AppId { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	private static ImmutableArray<Symbol> InitializeCorLib()
	{
		return ImmutableArray.Create(new Symbol[48]
		{
			CodeunitClassTypeSymbol.Instance,
			CompanyPropertyClassTypeSymbol.Instance,
			DatabaseClassTypeSymbol.Instance,
			DebuggerClassTypeSymbol.Instance,
			DialogClassTypeSymbol.Instance,
			FileClassTypeSymbol.Instance,
			IsolatedStorageClassTypeSymbol.Instance,
			NavAppClassTypeSymbol.Instance,
			PageClassTypeSymbol.Instance,
			ProductNameClassTypeSymbol.Instance,
			QueryClassTypeSymbol.Instance,
			ReportClassTypeSymbol.Instance,
			SessionClassTypeSymbol.Instance,
			SystemClassTypeSymbol.Instance,
			TaskSchedulerClassTypeSymbol.Instance,
			TextClassTypeSymbol.Instance,
			VersionClassTypeSymbol.Instance,
			XmlAttributeClassTypeSymbol.Instance,
			XmlCDataClassTypeSymbol.Instance,
			XmlCommentClassTypeSymbol.Instance,
			XmlDeclarationClassTypeSymbol.Instance,
			XmlDocumentClassTypeSymbol.Instance,
			XmlDocumentTypeClassTypeSymbol.Instance,
			XmlElementClassTypeSymbol.Instance,
			XmlportClassTypeSymbol.Instance,
			XmlProcessingInstructionClassTypeSymbol.Instance,
			XmlTextClassTypeSymbol.Instance,
			ActionSystemOptionTypeSymbol.Instance,
			ClientTypeSystemOptionTypeSymbol.Instance,
			DataClassificationSystemOptionTypeSymbol.Instance,
			DataScopeSystemOptionTypeSymbol.Instance,
			DefaultLayoutSystemOptionTypeSymbol.Instance,
			ErrorTypeSystemOptionTypeSymbol.Instance,
			ExecutionContextSystemOptionTypeSymbol.Instance,
			ExecutionModeSystemOptionTypeSymbol.Instance,
			FieldClassSystemOptionTypeSymbol.Instance,
			FieldTypeSystemOptionTypeSymbol.Instance,
			NotificationScopeSystemOptionTypeSymbol.Instance,
			ObjectTypeSystemOptionTypeSymbol.Instance,
			ReportFormatSystemOptionTypeSymbol.Instance,
			SecurityFilterSystemOptionTypeSymbol.Instance,
			TableConnectionTypeSystemOptionTypeSymbol.Instance,
			TestPermissionsSystemOptionTypeSymbol.Instance,
			TextEncodingSystemOptionTypeSymbol.Instance,
			TransactionModelSystemOptionTypeSymbol.Instance,
			TransactionTypeSystemOptionTypeSymbol.Instance,
			VerbositySystemOptionTypeSymbol.Instance,
			WebServiceActionResultCodeSystemOptionTypeSymbol.Instance
		});
	}

	public NavCorLib()
	{
		DebugAssertHelper.Assert(IntegerType != null);
		members = InitializeCorLib();
		memberMap = members.ToDictionary((Symbol m) => m.Name, (Symbol m) => m).ToImmutableSortedDictionary(SemanticFacts.NameComparer);
		membersWithImplicitAccess = (from c in members.OfType<LanguageClassTypeSymbol>()
			where c.ImplicitMemberAccess
			select c).SelectMany((LanguageClassTypeSymbol l) => l.GetMembers()).ToImmutableArray();
		PublisherName = "Microsoft";
		Version = Version.Parse("1.0.0.0");
		AppId = Guid.Empty;
	}

	internal static NavTypeSymbol ConstructGenericType(IGenericLanguageClassTypeSymbolFactory factory, params TypeSymbol[] typeArguments)
	{
		DebugAssertHelper.Assert(factory.ValidateTemplateArguments(typeArguments).All((bool x) => x));
		return factory.Create(typeArguments);
	}

	public override ImmutableArray<ISymbol> GetDeclaredObjectSymbols()
	{
		return ImmutableArray<ISymbol>.Empty;
	}

	public override ImmutableArray<ISymbol> GetObjectSymbols(SymbolKind kind)
	{
		return ImmutableArray<ISymbol>.Empty;
	}

	public override ISymbolWithId GetObjectSymbolById(SymbolKind kind, int id)
	{
		return null;
	}

	public override ISymbol GetObjectSymbolByName(SymbolKind kind, string name)
	{
		return null;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return members;
	}

	internal void AddLookupSymbols(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		foreach (string key in memberMap.Keys)
		{
			Symbol symbol = memberMap[key];
			if (originalBinder.CanAddLookupSymbolInfo(symbol, options, null))
			{
				result.AddSymbol(symbol, key, 0);
			}
		}
		foreach (Symbol item in membersWithImplicitAccess)
		{
			if (originalBinder.CanAddLookupSymbolInfo(item, options, null))
			{
				result.AddSymbol(item, item.Name, 0);
			}
		}
	}

	internal void LookupSymbol(LookupResult result, string name, LookupOptions options, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics, Binder originalBinder)
	{
		if (memberMap.TryGetValue(name, out var value) && originalBinder.CanAddLookupSymbolInfo(value, options, null))
		{
			SingleLookupResult result2 = originalBinder.CheckViability(value, options, null, diagnose, ref useSiteDiagnostics);
			result.MergeEqual(result2);
		}
		foreach (LanguageClassTypeSymbol item in from LanguageClassTypeSymbol c in members.Where((Symbol m) => m is LanguageClassTypeSymbol)
			where c.ImplicitMemberAccess
			select c)
		{
			foreach (Symbol member in item.GetMembers(name))
			{
				if (originalBinder.CanAddLookupSymbolInfo(member, options, null))
				{
					SingleLookupResult result3 = originalBinder.CheckViability(member, options, null, diagnose, ref useSiteDiagnostics);
					result.MergeEqual(result3);
				}
			}
		}
	}
}
