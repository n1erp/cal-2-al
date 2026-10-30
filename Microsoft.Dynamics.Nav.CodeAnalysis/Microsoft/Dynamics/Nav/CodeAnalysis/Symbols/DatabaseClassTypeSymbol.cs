using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class DatabaseClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<DatabaseClassTypeSymbol> instance = new Lazy<DatabaseClassTypeSymbol>(() => new DatabaseClassTypeSymbol("Database", isAutoWith: true, (Symbol containingSymbol) => new MethodSymbol[24]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "UserId", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", "ALUserID", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "CompanyName", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Commit", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "SerialNumber", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "CheckLicenseFile", SymbolCompilationScope.Internal, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "KeyNumber", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SelectLatestVersion", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "CurrentTransactionType", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TransactionTypeType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TransactionTypeType, "TransactionType", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "LockTimeout", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "LockTimeout", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SID", SymbolCompilationScope.Internal, isStatic: true, "ALDatabase", "ALSid", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "UserAccount", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "UserSecurityId", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetUserPassword", SymbolCompilationScope.Internal, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "USID", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Password", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ChangeUserPassword", SymbolCompilationScope.Internal, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "OldPassword", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NewPassword", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TenantId", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", "ALTenantID", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "SessionId", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", "ALSessionID", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "ServiceInstanceId", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", "ALServiceInstanceID", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "CopyCompany", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "SourceName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "DestinationName", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ImportData", SymbolCompilationScope.Internal, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[5]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "ShowDialog", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "FileName", 1, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "IncludeApplicationData", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "IncludeGlobalData", 3, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "CompanyRecord", 4, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ExportData", SymbolCompilationScope.Internal, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[7]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "ShowDialog", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "FileName", 1, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Description", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "IncludeApplication", 3, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "IncludeApplicationData", 4, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "IncludeGlobalData", 5, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "CompanyRecord", 6, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DataFileInformation", SymbolCompilationScope.Internal, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[9]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "ShowDialog", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "FileName", 1, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Description", 2, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "HasApplication", 3, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "HasApplicationData", 4, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "HasGlobalData", 5, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "tenantId", 6, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.DateTimeType, "exportDate", 7, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "CompanyRecord", 8, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RegisterTableConnection", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableConnectionTypeType, "Type", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Connection", 2)
		}, addCompilationTarget: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "UnregisterTableConnection", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableConnectionTypeType, "Type", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetDefaultTableConnection", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableConnectionTypeType, "Type", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 1),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Scoped", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetDefaultTableConnection", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableConnectionTypeType, "Type", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "HasTableConnection", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDatabase", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableConnectionTypeType, "Type", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 1)
		})
	}), isThreadSafe: true);

	internal static DatabaseClassTypeSymbol Instance => instance.Value;

	private DatabaseClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.ApplicationObject, NavTypeKind.Table, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
