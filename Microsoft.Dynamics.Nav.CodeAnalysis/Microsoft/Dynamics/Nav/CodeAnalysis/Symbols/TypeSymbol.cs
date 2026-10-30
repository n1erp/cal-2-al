using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class TypeSymbol : ContainerSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>
{
	public abstract TypeCategoryKind TypeCategory { get; }

	public virtual NavTypeKind NavTypeKind => NavTypeKind.None;

	public new virtual TypeSymbol OriginalDefinition => this;

	internal virtual TypeSymbol BaseType => null;

	public virtual int Length => 0;

	public virtual bool HasLength => false;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitType(this, a);
	}

	ImmutableArray<ISymbol> IContainerSymbol.GetMembers()
	{
		return StaticCast<ISymbol>.From(GetMembers());
	}

	ImmutableArray<ISymbol> IContainerSymbol.GetMembers(string name)
	{
		return StaticCast<ISymbol>.From(GetMembers(name));
	}

	internal virtual bool Equals(TypeSymbol t2)
	{
		return (object)this == t2;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	internal static TypeSymbol GetNavTypeSymbol(NavTypeKind navTypeKind)
	{
		return navTypeKind switch
		{
			NavTypeKind.Byte => NavCorLib.ByteType, 
			NavTypeKind.Char => NavCorLib.CharType, 
			NavTypeKind.Integer => NavCorLib.IntegerType, 
			NavTypeKind.BigInteger => NavCorLib.BigIntegerType, 
			NavTypeKind.Decimal => NavCorLib.DecimalType, 
			NavTypeKind.Option => NavCorLib.OptionType, 
			NavTypeKind.Date => NavCorLib.DateType, 
			NavTypeKind.Time => NavCorLib.TimeType, 
			NavTypeKind.DateTime => NavCorLib.DateTimeType, 
			NavTypeKind.Duration => NavCorLib.DurationType, 
			NavTypeKind.DateFormula => NavCorLib.DateFormulaType, 
			NavTypeKind.Boolean => NavCorLib.BooleanType, 
			NavTypeKind.Guid => NavCorLib.GuidType, 
			NavTypeKind.RecordRef => NavCorLib.RecordRefType, 
			NavTypeKind.KeyRef => NavCorLib.KeyRefType, 
			NavTypeKind.FieldRef => NavCorLib.FieldRefType, 
			NavTypeKind.RecordId => NavCorLib.RecordIdType, 
			NavTypeKind.TableFilter => NavCorLib.TableFilterType, 
			NavTypeKind.InStream => NavCorLib.InStreamType, 
			NavTypeKind.OutStream => NavCorLib.OutStreamType, 
			NavTypeKind.OemText => NavCorLib.OemTextType, 
			NavTypeKind.Text => NavCorLib.TextType, 
			NavTypeKind.Code => NavCorLib.TextType, 
			NavTypeKind.String => NavCorLib.StringType, 
			NavTypeKind.TextConst => NavCorLib.TextConstType, 
			NavTypeKind.Label => NavCorLib.LabelType, 
			NavTypeKind.Blob => NavCorLib.BlobType, 
			NavTypeKind.Media => NavCorLib.MediaType, 
			NavTypeKind.MediaSet => NavCorLib.MediaSetType, 
			NavTypeKind.Variant => NavCorLib.VariantType, 
			NavTypeKind.Action => NavCorLib.ActionType, 
			NavTypeKind.ClientType => NavCorLib.ClientTypeType, 
			NavTypeKind.NotificationScope => NavCorLib.NotificationScopeType, 
			NavTypeKind.Notification => NavCorLib.NotificationType, 
			NavTypeKind.DataClassification => NavCorLib.DataClassificationType, 
			NavTypeKind.DefaultLayout => NavCorLib.DefaultLayoutType, 
			NavTypeKind.ErrorInfo => NavCorLib.ErrorInfoType, 
			NavTypeKind.ErrorType => NavCorLib.ErrorTypeType, 
			NavTypeKind.ExecutionMode => NavCorLib.ExecutionModeType, 
			NavTypeKind.FieldType => NavCorLib.FieldTypeType, 
			NavTypeKind.FieldClass => NavCorLib.FieldClassType, 
			NavTypeKind.ObjectType => NavCorLib.ObjectTypeType, 
			NavTypeKind.TransactionModel => NavCorLib.TransactionModelType, 
			NavTypeKind.TestPermissions => NavCorLib.TestPermissionsType, 
			NavTypeKind.ReportFormat => NavCorLib.ReportFormatType, 
			NavTypeKind.SecurityFilter => NavCorLib.SecurityFilterType, 
			NavTypeKind.TableConnectionType => NavCorLib.TableConnectionTypeType, 
			NavTypeKind.TextEncoding => NavCorLib.TextEncodingType, 
			NavTypeKind.TransactionType => NavCorLib.TransactionTypeType, 
			NavTypeKind.Verbosity => NavCorLib.VerbosityType, 
			NavTypeKind.ExecutionContext => NavCorLib.ExecutionContextType, 
			NavTypeKind.DataScope => NavCorLib.DataScopeType, 
			NavTypeKind.Dialog => NavCorLib.DialogType, 
			NavTypeKind.BigText => NavCorLib.BigTextType, 
			NavTypeKind.File => NavCorLib.FileType, 
			NavTypeKind.FilterPageBuilder => NavCorLib.FilterPageBuilderType, 
			NavTypeKind.SessionSettings => NavCorLib.SessionSettingsType, 
			NavTypeKind.HttpClient => NavCorLib.HttpClientType, 
			NavTypeKind.HttpContent => NavCorLib.HttpContentType, 
			NavTypeKind.HttpRequestMessage => NavCorLib.HttpRequestMessageType, 
			NavTypeKind.HttpResponseMessage => NavCorLib.HttpResponseMessageType, 
			NavTypeKind.HttpHeaders => NavCorLib.HttpHeadersType, 
			NavTypeKind.JsonToken => NavCorLib.JsonTokenType, 
			NavTypeKind.JsonObject => NavCorLib.JsonObjectType, 
			NavTypeKind.JsonArray => NavCorLib.JsonArrayType, 
			NavTypeKind.JsonValue => NavCorLib.JsonValueType, 
			NavTypeKind.TextBuilder => NavCorLib.TextBuilderType, 
			NavTypeKind.Joker => NavCorLib.JokerType, 
			NavTypeKind.DotNet => NavCorLib.DotNetType, 
			NavTypeKind.XmlAttribute => NavCorLib.XmlAttributeType, 
			NavTypeKind.XmlAttributeCollection => NavCorLib.XmlAttributeCollectionType, 
			NavTypeKind.XmlComment => NavCorLib.XmlCommentType, 
			NavTypeKind.XmlCData => NavCorLib.XmlCDataType, 
			NavTypeKind.XmlDeclaration => NavCorLib.XmlDeclarationType, 
			NavTypeKind.XmlDocument => NavCorLib.XmlDocumentType, 
			NavTypeKind.XmlDocumentType => NavCorLib.XmlDocumentTypeType, 
			NavTypeKind.XmlElement => NavCorLib.XmlElementType, 
			NavTypeKind.XmlNamespaceManager => NavCorLib.XmlNamespaceManagerType, 
			NavTypeKind.XmlNameTable => NavCorLib.XmlNameTableType, 
			NavTypeKind.XmlNode => NavCorLib.XmlNodeType, 
			NavTypeKind.XmlNodeList => NavCorLib.XmlNodeListType, 
			NavTypeKind.XmlProcessingInstruction => NavCorLib.XmlProcessingInstructionType, 
			NavTypeKind.XmlText => NavCorLib.XmlTextType, 
			NavTypeKind.XmlReadOptions => NavCorLib.XmlReadOptionsType, 
			NavTypeKind.XmlWriteOptions => NavCorLib.XmlWriteOptionsType, 
			NavTypeKind.Version => NavCorLib.VersionType, 
			NavTypeKind.ModuleInfo => NavCorLib.ModuleInfoType, 
			NavTypeKind.ModuleDependencyInfo => NavCorLib.ModuleDependencyInfoType, 
			NavTypeKind.WebServiceActionContext => NavCorLib.WebServiceActionContextType, 
			NavTypeKind.WebServiceActionResultCode => NavCorLib.WebServiceActionResultCodeType, 
			_ => null, 
		};
	}

	public static IGenericLanguageClassTypeSymbolFactory GetGenericNavTypeSymbolFactory(NavTypeKind kind)
	{
		return kind switch
		{
			NavTypeKind.List => (IGenericLanguageClassTypeSymbolFactory)ListClassTypeSymbolFactory.Instance, 
			NavTypeKind.Dictionary => DictionaryClassTypeSymbolFactory.Instance, 
			_ => null, 
		};
	}
}
