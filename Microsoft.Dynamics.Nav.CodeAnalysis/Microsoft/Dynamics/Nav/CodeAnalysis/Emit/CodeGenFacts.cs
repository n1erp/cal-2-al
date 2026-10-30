using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class CodeGenFacts
{
	internal const string RuntimeNavActionTypeName = "FormResult";

	internal const string RuntimeNavClientTypeTypeName = "NavClientType";

	internal const string RuntimeNavDataClassificationTypeName = "DataClassification";

	internal const string RuntimeNavDataScopeTypeName = "DataScope";

	internal const string RuntimeNavDefaultLayoutTypeName = "NavDefaultLayout";

	internal const string RuntimeNavErrorTypeTypeName = "ALErrorType";

	internal const string RuntimeNavExecutionContextTypeName = "ExecutionContext";

	internal const string RuntimeNavExecutionModeTypeName = "ExecutionMode";

	internal const string RuntimeNavFieldClassTypeName = "FieldClass";

	internal const string RuntimeNavFieldTypeTypeName = "NavType";

	internal const string RuntimeNavNotificationScopeTypeName = "NotificationScope";

	internal const string RuntimeNavObjectTypeTypeName = "NavObjectType";

	internal const string RuntimeNavReportFormatTypeName = "NavReportFormat";

	internal const string RuntimeNavSecurityFilterTypeName = "SecurityFiltering";

	internal const string RuntimeNavTableConnectionTypeTypeName = "TableConnectionType";

	internal const string RuntimeNavTestPermissionsTypeName = "NavTestPermissions";

	internal const string RuntimeNavTextEncodingTypeName = "TextEncoding";

	internal const string RuntimeNavTransactionModelTypeName = "TestTransactionModel";

	internal const string RuntimeNavTransactionTypeTypeName = "TransactionType";

	internal const string RuntimeNavVerbosityTypeName = "ALVerbosity";

	internal const string RuntimeNavWebServiceActionResultCodeTypeName = "WebServiceActionResultCode";

	internal const string ALCompilerObjectToActionMethodName = "ObjectToFormResult";

	internal const string ALCompilerObjectToClientTypeMethodName = "ObjectToClientType";

	internal const string ALCompilerObjectToDataClassificationMethodName = "ObjectToDataClassification";

	internal const string ALCompilerObjectToDataScopeMethodName = "ObjectToDataScope";

	internal const string ALCompilerObjectToDefaultLayoutMethodName = "ObjectToDefaultLayout";

	internal const string ALCompilerObjectToErrorTypeMethodName = "ObjectToALErrorType";

	internal const string ALCompilerObjectToExecutionContextMethodName = "ObjectToExecutionContext";

	internal const string ALCompilerObjectToExecutionModeMethodName = "ObjectToExecutionMode";

	internal const string ALCompilerObjectToFieldClassMethodName = "ObjectToFieldClass";

	internal const string ALCompilerObjectToFieldTypeMethodName = "ObjectToNavType";

	internal const string ALCompilerObjectToNotificationScopeMethodName = "ObjectToNotificationScope";

	internal const string ALCompilerObjectToObjectTypeMethodName = "ObjectToObjectType";

	internal const string ALCompilerObjectToReportFormatMethodName = "ObjectToReportFormat";

	internal const string ALCompilerObjectToSecurityFilterMethodName = "ObjectToSecurityFiltering";

	internal const string ALCompilerObjectToTableConnectionTypeMethodName = "ObjectToTableConnectionType";

	internal const string ALCompilerObjectToTestPermissionsMethodName = "ObjectToTestPermissions";

	internal const string ALCompilerObjectToTextEncodingMethodName = "ObjectToTextEncoding";

	internal const string ALCompilerObjectToTransactionModelMethodName = "ObjectToTestTransactionModel";

	internal const string ALCompilerObjectToTransactionTypeMethodName = "ObjectToTransactionType";

	internal const string ALCompilerObjectToVerbosityMethodName = "ObjectToVerbosity";

	internal const string ALCompilerObjectToWebServiceActionResultCodeMethodName = "ObjectToWebServiceActionResultCode";

	internal static InvocationExpressionSyntax ALCompilerObjectToActionCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToFormResult"));

	internal static InvocationExpressionSyntax ALCompilerObjectToClientTypeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToClientType"));

	internal static InvocationExpressionSyntax ALCompilerObjectToDataClassificationCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToDataClassification"));

	internal static InvocationExpressionSyntax ALCompilerObjectToDataScopeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToDataScope"));

	internal static InvocationExpressionSyntax ALCompilerObjectToDefaultLayoutCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToDefaultLayout"));

	internal static InvocationExpressionSyntax ALCompilerObjectToErrorTypeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToALErrorType"));

	internal static InvocationExpressionSyntax ALCompilerObjectToExecutionContextCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToExecutionContext"));

	internal static InvocationExpressionSyntax ALCompilerObjectToExecutionModeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToExecutionMode"));

	internal static InvocationExpressionSyntax ALCompilerObjectToFieldClassCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToFieldClass"));

	internal static InvocationExpressionSyntax ALCompilerObjectToFieldTypeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToNavType"));

	internal static InvocationExpressionSyntax ALCompilerObjectToNotificationScopeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToNotificationScope"));

	internal static InvocationExpressionSyntax ALCompilerObjectToObjectTypeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToObjectType"));

	internal static InvocationExpressionSyntax ALCompilerObjectToReportFormatCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToReportFormat"));

	internal static InvocationExpressionSyntax ALCompilerObjectToSecurityFilterCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToSecurityFiltering"));

	internal static InvocationExpressionSyntax ALCompilerObjectToTableConnectionTypeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToTableConnectionType"));

	internal static InvocationExpressionSyntax ALCompilerObjectToTestPermissionsCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToTestPermissions"));

	internal static InvocationExpressionSyntax ALCompilerObjectToTextEncodingCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToTextEncoding"));

	internal static InvocationExpressionSyntax ALCompilerObjectToTransactionModelCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToTestTransactionModel"));

	internal static InvocationExpressionSyntax ALCompilerObjectToTransactionTypeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToTransactionType"));

	internal static InvocationExpressionSyntax ALCompilerObjectToVerbosityCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToVerbosity"));

	internal static InvocationExpressionSyntax ALCompilerObjectToWebServiceActionResultCodeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToWebServiceActionResultCode"));

	internal static TypeSyntax RuntimeNavActionTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("FormResult");

	internal static TypeSyntax RuntimeNavClientTypeTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavClientType");

	internal static TypeSyntax RuntimeNavDataClassificationTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("DataClassification");

	internal static TypeSyntax RuntimeNavDataScopeTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("DataScope");

	internal static TypeSyntax RuntimeNavDefaultLayoutTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavDefaultLayout");

	internal static TypeSyntax RuntimeNavErrorTypeTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("ALErrorType");

	internal static TypeSyntax RuntimeNavExecutionContextTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("ExecutionContext");

	internal static TypeSyntax RuntimeNavExecutionModeTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("ExecutionMode");

	internal static TypeSyntax RuntimeNavFieldClassTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("FieldClass");

	internal static TypeSyntax RuntimeNavFieldTypeTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavType");

	internal static TypeSyntax RuntimeNavNotificationScopeTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NotificationScope");

	internal static TypeSyntax RuntimeNavObjectTypeTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavObjectType");

	internal static TypeSyntax RuntimeNavReportFormatTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavReportFormat");

	internal static TypeSyntax RuntimeNavSecurityFilterTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("SecurityFiltering");

	internal static TypeSyntax RuntimeNavTableConnectionTypeTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("TableConnectionType");

	internal static TypeSyntax RuntimeNavTestPermissionsTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTestPermissions");

	internal static TypeSyntax RuntimeNavTextEncodingTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("TextEncoding");

	internal static TypeSyntax RuntimeNavTransactionModelTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("TestTransactionModel");

	internal static TypeSyntax RuntimeNavTransactionTypeTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("TransactionType");

	internal static TypeSyntax RuntimeNavVerbosityTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("ALVerbosity");

	internal static TypeSyntax RuntimeNavWebServiceActionResultCodeTypeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("WebServiceActionResultCode");

	internal static IdentifierNameSyntax RuntimeEqualsMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Equals");

	internal static SyntaxToken EventScopeFieldIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("\\u03b3eventScope");

	internal static SyntaxToken ScopeIdFieldIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("\\u03b1scopeId");

	internal static SyntaxToken InternalParentParameterIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("\\u03b2parent");

	internal static SyntaxToken RecParameterIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("\\u03b5rec");

	internal static SyntaxToken RecFieldIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("Rec");

	internal static SyntaxToken NavMethodScopeTypeIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("NavMethodScope");

	internal static SyntaxToken NavEventMethodScopeTypeIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("NavEventMethodScope");

	internal static SyntaxToken NavTriggerMethodScopeTypeIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("NavTriggerMethodScope");

	internal static SyntaxToken RunMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("Run");

	internal static SyntaxToken OnRunMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("OnRun");

	internal static SyntaxToken ScopeLocalIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("\\u03b2scope");

	internal static SyntaxToken DefaultReturnValueIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("\\u03b3retVal");

	internal static SyntaxToken ParentParameterIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("parent");

	internal static SyntaxToken MetaFormParameterIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("metaForm");

	internal static SyntaxToken ObjectNamePropertyIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("ObjectName");

	internal static SyntaxToken IsSingleInstancePropertyIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("IsSingleInstance");

	internal static SyntaxToken IsDebuggablePropertyIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("IsDebuggable");

	internal static SyntaxToken OnTestRunMethodsHaveTestPermissionsParameterIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("OnTestRunMethodsHaveTestPermissionsParameter");

	internal static SyntaxToken CommitTestCodeunitsIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("CommitTestCodeunits");

	internal static SyntaxToken CommitTestFunctionsIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("CommitTestFunctions");

	internal static SyntaxToken ConstructMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("__Construct");

	internal static SyntaxToken InitializeComponentMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("InitializeComponent");

	internal static SyntaxToken OnClearMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("OnClear");

	internal static SyntaxToken ClearMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("Clear");

	internal static SyntaxToken GetFieldValueSafeMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("GetFieldValueSafe");

	internal static SyntaxToken GetFieldRefSafeMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("GetFieldRefSafe");

	internal static SyntaxToken GetColumnValueSafeMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("GetColumnValueSafe");

	internal static SyntaxToken GetColumnOptionSafeMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("GetColumnOptionSafe");

	internal static SyntaxToken SetFieldValueSafeMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("SetFieldValueSafe");

	internal static SyntaxToken ByRefTypeIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("ByRef");

	internal static SyntaxToken ByRefSetValueIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("setValue");

	internal static SyntaxToken OnInvokeMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("OnInvoke");

	internal static SyntaxToken OnInvokeMemberIdParameterIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("memberId");

	internal static SyntaxToken OnInvokeArgsParameterIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("args");

	internal static SyntaxToken TargetPropertyIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("Target");

	internal static SyntaxToken NavTypeEnumIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("NavType");

	internal static SyntaxToken GetActionMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("GetAction");

	internal static SyntaxToken GetFieldMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("GetField");

	internal static SyntaxToken GetPartMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("GetPart");

	internal static SyntaxToken GetDataItemMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("GetDataItem");

	internal static SyntaxToken GetBuiltInActionMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("GetBuiltInAction");

	internal static SyntaxToken ALFilterPropertyIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("ALFilter");

	internal static SyntaxToken NavArrayTypeIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("NavArray");

	internal static SyntaxToken NavListTypeIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("NavList");

	internal static SyntaxToken NavDictionaryTypeIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("NavDictionary");

	internal static SyntaxToken ALAssignMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("ALAssign");

	internal static SyntaxToken FieldNoParameterIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("fieldNo");

	internal static NameSyntax NavVoidTypeIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavVoidType");

	internal static NameSyntax NavDotNetTypeIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavDotNet");

	internal static NameSyntax NavObjectIdAttributeNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavObjectId");

	internal static NameSyntax NavCodeunitOptionsAttributeNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavCodeunitOptions");

	internal static NameSyntax NavByReferenceAttributeNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavByReferenceAttribute");

	internal static NameSyntax NavNameAttributeNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavName");

	internal static NameSyntax NavDotNetTypeInfoAttributeNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavDotNetTypeInfo");

	internal static NameSyntax SourceSpansAttributeNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("SourceSpans");

	internal static NameSyntax ReturnValueAttributeNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("ReturnValue");

	internal static NameSyntax FieldTriggerHandlerAttributeNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("FieldTriggerHandler");

	internal static NameSyntax ParentParameterNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("parent");

	internal static NameSyntax NavDialogNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavDialog");

	internal static NameSyntax NavBigTextNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavBigText");

	internal static SyntaxToken NavDotNetInvokeMethodNameIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("InvokeMethod");

	internal static SyntaxToken NavDotNetInvokeStaticMethodNameIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("InvokeStaticMethod");

	internal static SyntaxToken NavDotNetInvokePropertyGetMethodNameIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("InvokePropertyGet");

	internal static SyntaxToken NavDotNetInvokeStaticPropertyGetMethodNameIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("InvokeStaticPropertyGet");

	internal static SyntaxToken NavDotNetInvokePropertySetMethodNameIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("InvokePropertySet");

	internal static SyntaxToken NavDotNetInvokeStaticPropertySetMethodNameIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("InvokeStaticPropertySet");

	internal static SyntaxToken NavDotNetCreateDotNetMethodNameIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("CreateDotNet");

	internal static SyntaxToken NavDotNetInvokeStaticFieldMethodNameIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("InvokeStaticField");

	internal static SyntaxToken NavDotNetAsMethodNameIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("As");

	internal static IdentifierNameSyntax NavDotNetCreateDotNetEnumeratorMethodNameIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CreateDotNetEnumerator");

	internal static IdentifierNameSyntax ArrayFactoryClassNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Factory");

	internal static IdentifierNameSyntax ToStringMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ToString");

	internal static IdentifierNameSyntax NavOptionCreateInstanceMethodIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CreateInstance");

	internal static SyntaxToken EventHandlerParentApplicationObjectFieldNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("applicationObject");

	internal static IdentifierNameSyntax EventHandlerParentApplicationObjectFieldIdentifierNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(EventHandlerParentApplicationObjectFieldNameSyntax);

	internal static IdentifierNameSyntax RequestOptionsPageNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RequestOptionsPage");

	internal static IdentifierNameSyntax ByRefSetValueNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(ByRefSetValueIdentifier);

	internal static IdentifierNameSyntax InitializeComponentMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(InitializeComponentMethodIdentifier);

	internal static IdentifierNameSyntax NavRuntimeHelpersNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavRuntimeHelpers");

	internal static IdentifierNameSyntax NavRuntimeHelpersCompilationErrorNameMethodSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CompilationError");

	internal static IdentifierNameSyntax LangClassNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Lang");

	internal static IdentifierNameSyntax LangWrongReferenceNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("WrongReference");

	internal static IdentifierNameSyntax OnClearMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(OnClearMethodIdentifier);

	internal static IdentifierNameSyntax ClearMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(ClearMethodIdentifier);

	internal static IdentifierNameSyntax GetFieldValueSafeMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetFieldValueSafeMethodIdentifier);

	internal static IdentifierNameSyntax GetFieldRefSafeMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetFieldRefSafeMethodIdentifier);

	internal static IdentifierNameSyntax SetFieldValueSafeMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(SetFieldValueSafeMethodIdentifier);

	internal static IdentifierNameSyntax GetColumnValueSafeMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetColumnValueSafeMethodIdentifier);

	internal static IdentifierNameSyntax GetColumnOptionSafeMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetColumnOptionSafeMethodIdentifier);

	internal static IdentifierNameSyntax OldRecordPropertyNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("OldRecord");

	internal static IdentifierNameSyntax SourceTablePropertyNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("SourceTable");

	internal static IdentifierNameSyntax ParentObjectPropertyNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ParentObject");

	internal static IdentifierNameSyntax GetActionMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetActionMethodIdentifier);

	internal static IdentifierNameSyntax GetFieldMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetFieldMethodIdentifier);

	internal static IdentifierNameSyntax GetPartMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetPartMethodIdentifier);

	internal static IdentifierNameSyntax GetDataItemMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetDataItemMethodIdentifier);

	internal static IdentifierNameSyntax GetBuiltInActionMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetBuiltInActionMethodIdentifier);

	internal static GenericNameSyntax InvokeEnumeratorMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(NavDotNetInvokeStaticFieldMethodNameIdentifier, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList((TypeSyntax)NavDotNetTypeIdentifier)));

	internal static GenericNameSyntax NavValueToNavValueMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("NavValueToNavValue"));

	internal static IdentifierNameSyntax ALFilterPropertyNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(ALFilterPropertyIdentifier);

	internal static IdentifierNameSyntax ALValuePropertyNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ALValue");

	internal static IdentifierNameSyntax NavTypeEnumNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(NavTypeEnumIdentifier);

	internal static IdentifierNameSyntax NavValueToInt32MethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ToInt32");

	internal static IdentifierNameSyntax NavValueToCharMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavValueToChar");

	internal static IdentifierNameSyntax NavValueToInt64MethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ToInt64");

	internal static IdentifierNameSyntax NavValueToDecimalMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ToDecimal");

	internal static IdentifierNameSyntax NavValueToBooleanMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ToBoolean");

	internal static IdentifierNameSyntax NavValueToByteMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ToByte");

	internal static IdentifierNameSyntax NavValueToGuidMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ToGuid");

	internal static IdentifierNameSyntax NavFieldRefNumberPropertyNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ALNumber");

	internal static IdentifierNameSyntax ALAssignMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(ALAssignMethodIdentifier);

	internal static IdentifierNameSyntax ALMaxStrLenMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ALMaxStrLen");

	internal static IdentifierNameSyntax OnInvokeArgsParameterNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("args");

	internal static IdentifierNameSyntax OnInvokeMemberIdParameterNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("memberId");

	internal static IdentifierNameSyntax BusinessApplicationNamespaceNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Microsoft.Dynamics.Nav.BusinessApplication");

	internal static IdentifierNameSyntax ByRefValuePropertyNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Value");

	internal static IdentifierNameSyntax NavScopeParentPropertyNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Parent");

	internal static IdentifierNameSyntax NavScopeClassNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavScope");

	internal static IdentifierNameSyntax ALSystemStringClassNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ALSystemString");

	internal static IdentifierNameSyntax NavValueDefaultMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Default");

	internal static IdentifierNameSyntax ALCompilerClassNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ALCompiler");

	internal static IdentifierNameSyntax NavOptionMetadataPropertyNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavOptionMetadata");

	internal static IdentifierNameSyntax TargetPropertyNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(TargetPropertyIdentifier);

	internal static IdentifierNameSyntax InvokeMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Invoke");

	internal static IdentifierNameSyntax GetSubArrayMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("GetSubArray");

	internal static IdentifierNameSyntax NavValueByValueMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ALByValue");

	internal static IdentifierNameSyntax ClearFieldValueMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ClearFieldValue");

	internal static IdentifierNameSyntax ClearReferenceMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ClearReference");

	internal static IdentifierNameSyntax ModifyLengthMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ModifyLength");

	internal static IdentifierNameSyntax ALCompilerObjectToExactINavRecordHandleMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ObjectToExactINavRecordHandle");

	internal static IdentifierNameSyntax ALCompilerObjectToExactNavCodeunitHandleMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ObjectToExactNavCodeunitHandle");

	internal static IdentifierNameSyntax ALCompilerObjectToNavInStreamMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ObjectToNavInStream");

	internal static IdentifierNameSyntax AlCompilerObjectToNavOutStreamMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ObjectToNavOutStream");

	internal static IdentifierNameSyntax AlCompilerObjectToIntegerMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ObjectToInt32");

	internal static IdentifierNameSyntax CompareToMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CompareTo");

	internal static IdentifierNameSyntax InitializeFormMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("InitializeForm");

	internal static IdentifierNameSyntax RegisterUIPartMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RegisterUIPart");

	internal static IdentifierNameSyntax RegisterSourceExpressionMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RegisterSourceExpression");

	internal static IdentifierNameSyntax RegisterAutoFormatExpressionMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RegisterAutoFormatExpression");

	internal static IdentifierNameSyntax RegisterGetDecimalStringExpressionMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RegisterGetDecimalStringExpression");

	internal static IdentifierNameSyntax GetDecimalStringMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("GetDecimalString");

	internal static IdentifierNameSyntax RegisterFormatStringExpressionMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RegisterFormatStringExpression");

	internal static IdentifierNameSyntax RegisterDynamicCaptionExpressionMethodNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RegisterDynamicCaptionExpression");

	internal static IdentifierNameSyntax EmptyPropertyNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Empty");

	internal const string BusinessApplicationNamespaceName = "Microsoft.Dynamics.Nav.BusinessApplication";

	internal const string EventScopeFieldName = "\\u03b3eventScope";

	internal const string EventScopePropertyName = "EventScope";

	internal const string ScopeIdFieldName = "\\u03b1scopeId";

	internal const string ScopeIdPropertyName = "RawScopeId";

	internal const string EventHandlerParentApplicationObjectFieldName = "applicationObject";

	internal const string NavFunctionVisibilityAttributeName = "NavFunctionVisibility";

	internal const string FunctionVisibilityEnumName = "FunctionVisibility";

	internal const string FunctionVisibilityEnum_InternalName = "Internal";

	internal const string FunctionVisibilityEnum_ExternalName = "External";

	internal const string ObjectNamePropertyName = "ObjectName";

	internal const string IsDebuggablePropertyName = "IsDebuggable";

	internal const string IsSingleInstancePropertyName = "IsSingleInstance";

	internal const string OnTestRunMethodsHaveTestPermissionsParameterPropertyName = "OnTestRunMethodsHaveTestPermissionsParameter";

	internal const string CommitTestCodeunitsPropertyName = "CommitTestCodeunits";

	internal const string CommitTestFunctionsPropertyName = "CommitTestFunctions";

	internal const string ConstructMethodName = "__Construct";

	internal const string InitializeComponentMethodName = "InitializeComponent";

	internal const string OnClearMethodName = "OnClear";

	internal const string ClearMethodName = "Clear";

	internal const string NavDotNetEventHandlerClassNameInfix = "Eventhandler";

	internal const string GetFieldValueSafeMethodName = "GetFieldValueSafe";

	internal const string SetFieldValueSafeMethodName = "SetFieldValueSafe";

	internal const string GetFieldRefSafeMethodName = "GetFieldRefSafe";

	internal const string GetColumnValueSafeMethodName = "GetColumnValueSafe";

	internal const string GetColumnOptionSafeMethodName = "GetColumnOptionSafe";

	internal const string GetActionMethodName = "GetAction";

	internal const string GetFieldMethodName = "GetField";

	internal const string GetBuiltInActionMethodName = "GetBuiltInAction";

	internal const string InternalParentParameterName = "\\u03b2parent";

	internal const string RecParameterName = "\\u03b5rec";

	internal const string RecFieldName = "Rec";

	internal const string ScopeLocalName = "\\u03b2scope";

	internal const string DefaultReturnValueName = "\\u03b3retVal";

	internal const string InternalDialogPrefix = "\\u03b4";

	internal const string TemporaryReturnValueName = "__\\u03b3tmp__";

	internal const string TemporaryForLoopVariableName = "@tmp";

	internal const string TemporaryGetTextVariableName = "@tmp0";

	internal const string TemporarySetTextVariableName = "@tmp1";

	internal const string RecPropertyName = "Rec";

	internal const string xRecPropertyName = "xRec";

	internal const string CurrPagePropertyName = "CurrPage";

	internal const string ParentObjectPropertyName = "ParentObject";

	internal const string CurrReportPropertyName = "CurrReport";

	internal const string CurrXmlPortPropertyName = "currXMLport";

	internal const string CurrQueryPropertyName = "CurrQuery";

	internal const string DataItemTypeName = "DataItem";

	internal const string NavMethodScopeTypeName = "NavMethodScope";

	internal const string NavEventMethodScopeTypeName = "NavEventMethodScope";

	internal const string NavTriggerMethodScopeTypeName = "NavTriggerMethodScope";

	internal const string NavTriggerTypeName = "NavTrigger";

	internal const string NavEventScopeTypeName = "NavEventScope";

	internal const string INavRecordHandleTypeName = "INavRecordHandle";

	internal const string NavRecordHandleTypeName = "NavRecordHandle";

	internal const string ITreeObjectTypeName = "ITreeObject";

	internal const string RuntimeGuidTypeName = "Guid";

	internal const string RuntimeVarTypeName = "var";

	internal const string RuntimeNavVoidTypeName = "NavVoidType";

	internal const string RuntimeEqualsMethodName = "Equals";

	internal const string RuntimeGuidEmptyPropertyName = "Empty";

	internal const string RuntimeGuidNewGuidMethodName = "NewGuid";

	internal const string NavGuidTypeName = "NavGuid";

	internal const string RunMethodName = "Run";

	internal const string OnRunMethodName = "OnRun";

	internal const string ScopeClassNameNonMethodSuffix = "_Scope";

	internal const string ScopeClassNameSuffix = "_Scope_";

	internal const string NodeClassNameSuffix = "_Node";

	internal const string NodeNameVariableName = "nodeName";

	internal const string NavMethodScopeAssertErrorMethodName = "AssertError";

	internal const string NavObjectIdAttributeName = "NavObjectId";

	internal const string NavCodeunitOptionsAttributeName = "NavCodeunitOptions";

	internal const string NavCodeunitOptionsEnumName = "NavCodeunitOptions";

	internal const string NavCodeunitSubtypeEnumName = "CodeunitSubType";

	internal const string NavCodeunitOptionsSingleInstanceName = "SingleInstance";

	internal const string NavCodeunitOptionsEventManualBindingName = "EventManualBinding";

	internal const string ObjectIdAttributeParameterName = "ObjectId";

	internal const string NavByReferenceAttributeName = "NavByReferenceAttribute";

	internal const string NavNameAttributeName = "NavName";

	internal const string NavDotNetTypeInfoAttributeName = "NavDotNetTypeInfo";

	internal const string SourceSpansAttributeName = "SourceSpans";

	internal const string ReturnValueAttributeName = "ReturnValue";

	internal const string FieldTriggerHandlerAttributeName = "FieldTriggerHandler";

	internal const string FieldTriggerTypeEnumName = "FieldTriggerType";

	internal const string NavTypeBlobName = "BLOB";

	internal const string NavTypeEnumName = "NavType";

	internal const string NavTypeEnumRecordIdName = "RecordID";

	internal const string NavTypeEnumGuidName = "GUID";

	internal const string NavTypeEnumOptionName = "Option";

	internal const string NavValueToCharMethodName = "NavValueToChar";

	internal const string NavValueToInt32MethodName = "ToInt32";

	internal const string NavValueToInt64MethodName = "ToInt64";

	internal const string NavValueToDecimalMethodName = "ToDecimal";

	internal const string NavValueToBooleanMethodName = "ToBoolean";

	internal const string NavValueToByteMethodName = "ToByte";

	internal const string NavValueToGuidMethodName = "ToGuid";

	internal const string NavFieldRefNumberPropertyName = "ALNumber";

	internal const string SourceExpressionTypeEnumName = "SourceExpressionType";

	internal const string SourceExpressionTypeEnumDataCaption = "DataCaption";

	internal const string SourceExpressionTypeEnumSourceExpression = "SourceExpression";

	internal const string SourceExpressionTypeEnumDataItem = "DataItem";

	internal const string SourceExpressionTypeEnumVariableIncludedInDataSet = "VariableIncludedInDataSet";

	internal const string SourceExpressionTypeEnumDynamicCueStyle = "DynamicCueStyle";

	internal const string SourceExpressionTypeProperty = "Property";

	internal const string GetDynamicCueStyleMethodName = "GetDynamicCueStyle";

	internal const string NavCurrentThreadSessionCompanySharedObjectsEnumName = "NavCurrentThread.Session.Company.SharedObjects";

	internal const string ParentParameterName = "parent";

	internal const string MetaFormParameterName = "metaForm";

	internal const string CreateMethodName = "Create";

	internal const string NavScopeParentPropertyName = "Parent";

	internal const string NavScopeClassName = "NavScope";

	internal const string ClearFieldValueMethodName = "ClearFieldValue";

	internal const string ClearReferenceMethodName = "ClearReference";

	internal const string ByRefValuePropertyName = "Value";

	internal const string NavValueDefaultMethodName = "Default";

	internal const string NavValueByValueMethodName = "ALByValue";

	internal const string ModifyLengthMethodName = "ModifyLength";

	internal const string OnInvokeMethodName = "OnInvoke";

	internal const string OnInvokeMemberIdParameterName = "memberId";

	internal const string OnInvokeArgsParameterName = "args";

	internal const string LengthPropertyName = "Length";

	internal const string NavRuntimeHelpersName = "NavRuntimeHelpers";

	internal const string NavRuntimeHelpersCompilationErrorMethodName = "CompilationError";

	internal const string NavNCLInvalidNumberOfArgumentsExceptionName = "NavNCLInvalidNumberOfArgumentsException";

	internal const string LangClassName = "Lang";

	internal const string LangWrongReferencePropertyName = "WrongReference";

	internal const string TargetPropertyName = "Target";

	internal const string InvokeMethodName = "Invoke";

	internal const string ValueArgumentName = "value";

	internal const string OldRecordPropertyName = "OldRecord";

	internal const string SourceTablePropertyName = "SourceTable";

	internal const string NavBlobTypeName = "NavBLOB";

	internal const string NavMediaTypeName = "NavMedia";

	internal const string NavMediaSetTypeName = "NavMediaSet";

	internal const string NavBooleanTypeName = "NavBoolean";

	internal const string NavByteTypeName = "NavByte";

	internal const string NavCharTypeName = "NavChar";

	internal const string NavIntegerTypeName = "NavInteger";

	internal const string NavBigIntegerTypeName = "NavBigInteger";

	internal const string NavTextTypeName = "NavText";

	internal const string NavOemTextTypeName = "NavOemText";

	internal const string NavCodeTypeName = "NavCode";

	internal const string NavTextConstTypeName = "NavTextConstant";

	internal const string NavDateTypeName = "NavDate";

	internal const string NavTimeTypeName = "NavTime";

	internal const string NavDateTimeTypeName = "NavDateTime";

	internal const string NavDurationTypeName = "NavDuration";

	internal const string NavDateFormulaTypeName = "NavDateFormula";

	internal const string NavVariantTypeName = "NavVariant";

	internal const string NavInStreamTypeName = "NavInStream";

	internal const string NavOutStreamTypeName = "NavOutStream";

	internal const string NavDotNetTypeName = "NavDotNet";

	internal const string NavVoidTypeName = "NavVoidType";

	internal const string ALStreamTypeName = "ALStream";

	internal const string ALStreamDefaultLengthTypeName = "DefaultLength";

	internal const string NavHttpClientTypeName = "NavHttpClient";

	internal const string NavHttpContentTypeName = "NavHttpContent";

	internal const string NavHttpRequestMessageTypeName = "NavHttpRequestMessage";

	internal const string NavHttpResponseMessageTypeName = "NavHttpResponseMessage";

	internal const string NavHttpHeadersTypeName = "NavHttpHeaders";

	internal const string NavJsonTokenTypeName = "NavJsonToken";

	internal const string NavJsonObjectTypeName = "NavJsonObject";

	internal const string NavJsonArrayTypeName = "NavJsonArray";

	internal const string NavJsonValueTypeName = "NavJsonValue";

	internal const string NavTextBuilderTypeName = "NavTextBuilder";

	internal const string NavXmlAttributeTypeName = "NavXmlAttribute";

	internal const string NavXmlAttributeCollectionTypeName = "NavXmlAttributeCollection";

	internal const string NavXmlCommentTypeName = "NavXmlComment";

	internal const string NavXmlCDataTypeName = "NavXmlCData";

	internal const string NavXmlDeclarationTypeName = "NavXmlDeclaration";

	internal const string NavXmlDocumentTypeName = "NavXmlDocument";

	internal const string NavXmlDocumentTypeTypeName = "NavXmlDocumentType";

	internal const string NavXmlElementTypeName = "NavXmlElement";

	internal const string NavXmlNamespaceManagerTypeName = "NavXmlNamespaceManager";

	internal const string NavXmlNameTableTypeName = "NavXmlNameTable";

	internal const string NavXmlNodeTypeName = "NavXmlNode";

	internal const string NavXmlNodeListTypeName = "NavXmlNodeList";

	internal const string NavXmlProcessingInstructionTypeName = "NavXmlProcessingInstruction";

	internal const string NavXmlTextTypeName = "NavXmlText";

	internal const string NavXmlReadOptionsTypeName = "NavXmlReadOptions";

	internal const string NavXmlWriteOptionsTypeName = "NavXmlWriteOptions";

	internal const string NavListTypeName = "NavList";

	internal const string NavDictionaryTypeName = "NavDictionary";

	internal const string NavWebServiceActionContextName = "NavWebServiceActionContext";

	internal const string NavVersionTypeName = "NavVersion";

	internal const string NavModuleInfoName = "NavModuleInfo";

	internal const string NavModuleDependencyInfoName = "NavModuleDependencyInfo";

	internal const string NavDotNetInvokeMethodName = "InvokeMethod";

	internal const string NavDotNetInvokeStaticMethodName = "InvokeStaticMethod";

	internal const string NavDotNetInvokePropertyGetMethodName = "InvokePropertyGet";

	internal const string NavDotNetInvokeStaticPropertyGetMethodName = "InvokeStaticPropertyGet";

	internal const string NavDotNetInvokePropertySetMethodName = "InvokePropertySet";

	internal const string NavDotNetInvokeStaticPropertySetMethodName = "InvokeStaticPropertySet";

	internal const string NavDotNetCreateDotNetMethodName = "CreateDotNet";

	internal const string NavDotNetCreateDotNetEnumeratorMethodName = "CreateDotNetEnumerator";

	internal const string NavDotNetInvokeStaticFieldMethodName = "InvokeStaticField";

	internal const string NavDotNetAsMethodName = "As";

	internal const string NavDotNetGetEnumeratorMethodName = "GetEnumerator";

	internal const string NavDotNetEnumeratorMoveNextMethodName = "MoveNext";

	internal const string NavDotNetEnumeratorCurrentPropertyName = "Current";

	internal const string NavRecordRefTypeName = "NavRecordRef";

	internal const string NavFieldRefTypeName = "NavFieldRef";

	internal const string NavKeyRefTypeName = "NavKeyRef";

	internal const string NavRecordIdTypeName = "NavRecordId";

	internal const string NavTableFilterTypeName = "NavTableFilter";

	internal const string ALValuePropertyName = "ALValue";

	internal const string RuntimeDecimalTypeName = "Decimal18";

	internal const string NavDecimalTypeName = "NavDecimal";

	internal const string Decimal18ZeroMethodName = "Zero";

	internal const string DataErrorEnumName = "DataError";

	internal const string DataErrorTrapErrorName = "TrapError";

	internal const string DataErrorThrowErrorName = "ThrowError";

	internal const string CompilationTargetName = "CompilationTarget";

	internal const string CompilationTargetPersonalizationName = "Personalization";

	internal const string CompilationTargetExtensionName = "Extension";

	internal const string CompilationTargetSolutionName = "Solution";

	internal const string CompilationTargetInternalName = "Internal";

	internal const string ToStringMethodName = "ToString";

	internal const string NavOptionCreateInstanceMethodName = "CreateInstance";

	internal const string RuntimeNotificationTypeName = "NavNotification";

	internal const string NavExecutionModeTypeName = "NavExecutionMode";

	internal const string NavOptionTypeName = "NavOption";

	internal const string NclOptionMetadataTypeName = "NCLOptionMetadata";

	internal const string NclOptionMetadataDefaultPropertyName = "Default";

	internal const string NclEnumMetadataTypeName = "NCLEnumMetadata";

	internal const string NavOptionMetadataPropertyName = "NavOptionMetadata";

	internal const string NclMetaTableTypeName = "NCLMetaTable";

	internal const string NavCodeunitHandleTypeName = "NavCodeunitHandle";

	internal const string NavCodeunitTypeName = "NavCodeunit";

	internal const string RequestPageTypeName = "RequestPage";

	internal const string MasterPageTypeName = "MasterPage";

	internal const string NavTestCodeunitTypeName = "NavTestCodeunit";

	internal const string NavTestRunnerCodeunitTypeName = "NavTestRunnerCodeUnit";

	internal const string NavUpgradeCodeunitTypeName = "NavUpgradeCodeunit";

	internal const string NavQueryHandleTypeName = "NavQueryHandle";

	internal const string NavQueryTypeName = "NavQuery";

	internal const string NavRecordTypeName = "NavRecord";

	internal const string NavRecordExtensionTypeName = "NavRecordExtension";

	internal const string NavInteropEventHandlerBaseTypeName = "NavInteropEventHandlerBase";

	internal const string NavPageTypeName = "NavForm";

	internal const string NavRequestPageBaseTypeName = "RequestPageBase";

	internal const string NavPageExtensionTypeName = "NavFormExtension";

	internal const string NavPageHandleTypeName = "NavFormHandle";

	internal const string NavTestPageHandleTypeName = "NavTestPageHandle";

	internal const string NavTestPageTypeName = "NavTestPage";

	internal const string NavReportHandleTypeName = "NavReportHandle";

	internal const string NavReportTypeName = "NavReport";

	internal const string NavXmlPortHandleTypeName = "NavXmlPortHandle";

	internal const string NavXmlPortTypeName = "NavXmlPort";

	internal const string NavXmlPortTextNodeTypeName = "NavXmlPortTextNode";

	internal const string NavXmlPortFieldNodeTypeName = "NavXmlPortFieldNode";

	internal const string NavXmlPortTableNodeTypeName = "NavXmlPortTableNode";

	internal const string NavArrayTypeName = "NavArray";

	internal const string GetSubArrayMethodName = "GetSubArray";

	internal const string NavDialogTypeName = "NavDialog";

	internal const string NavBigTextTypeName = "NavBigText";

	internal const string NavFileTypeName = "NavFile";

	internal const string NavFilterPageBuilderTypeName = "NavFilterPageBuilder";

	internal const string RuntimeSessionSettingsTypeName = "NavSessionSettings";

	internal const string RuntimeErrorInfoTypeName = "NavALErrorInfo";

	internal const string FormResultTypeName = "FormResult";

	internal const string ByRefTypeName = "ByRef";

	internal const string ByRefSetValueName = "setValue";

	internal const string ArrayFactoryClassName = "Factory";

	internal const string StmtHitMethodName = "StmtHit";

	internal const string CStmtHitMethodName = "CStmtHit";

	internal const string ALAssignMethodName = "ALAssign";

	internal const string ALSystemStringClassName = "ALSystemString";

	internal const string ALMaxStrLenMethodName = "ALMaxStrLen";

	internal const string ALSystemStringSetCharMethodName = "SetChar";

	internal const string NavDotNetClassName = "NavDotNet";

	internal const string NavDotNetGetDotNetTypeMethodName = "GetDotNetType";

	internal const string ALCompilerClassName = "ALCompiler";

	internal const string ALCompilerObjectToBooleanMethodName = "ObjectToBoolean";

	internal const string ALCompilerObjectToByteMethodName = "ObjectToByte";

	internal const string ALCompilerObjectToCharMethodName = "ObjectToChar";

	internal const string ALCompilerObjectToIntegerMethodName = "ObjectToInt32";

	internal const string ALCompilerObjectToBigIntegerMethodName = "ObjectToInt64";

	internal const string ALCompilerObjectToDecimalMethodName = "ObjectToDecimal";

	internal const string ALCompilerObjectToGuidMethodName = "ObjectToGuid";

	internal const string ALCompilerObjectToStringMethodName = "ObjectToString";

	internal const string ALCompilerObjectToFormResultMethodName = "ObjectToFormResult";

	internal const string ALCompilerObjectToConnectionTypeMethodName = "ObjectToConnectionType";

	internal const string ALCompilerObjectToFilterPageBuilderMethodName = "ObjectToFilterPageBuilder";

	internal const string ALCompilerObjectToEncodingMethodName = "ObjectToEncoding";

	internal const string ALCompilerObjectToExactINavRecordHandleMethodName = "ObjectToExactINavRecordHandle";

	internal const string ALCompilerObjectToExactNavCodeunitHandleMethodName = "ObjectToExactNavCodeunitHandle";

	internal const string ALCompilerObjectToNavInStreamMethodName = "ObjectToNavInStream";

	internal const string ALCompilerObjectToNavOutStreamMethodName = "ObjectToNavOutStream";

	internal const string ALCompilerObjectToExactNavValueMethodName = "ObjectToExactNavValue";

	internal const string ALCompilerDotNetToNavOutStreamMethodName = "DotNetToNavOutStream";

	internal const string ALCompilerDotNetToNavInStreamMethodName = "DotNetToNavInStream";

	internal const string ALCompilerInStreamToDotNetMethodName = "InStreamToDotNet";

	internal const string ALCompilerOutStreamToDotNetMethodName = "OutStreamToDotNet";

	internal const string ALCompilerDotNetToCharMethodName = "NavDotNetToChar";

	internal const string ALCompilerNavCodeToDotNet = "NavCodeToDotNet";

	internal const string ALCompilerNavTextToDotNet = "NavTextToDotNet";

	internal const string ALCompilerDateTimeToDotNet = "DateTimeToDotNet";

	internal const string NavBigTextCreateFromObject = "CreateFromObject";

	internal const string NavTextCreateFromObject = "CreateFromObject";

	internal const string NavCodeCreateFromObject = "CreateFromObject";

	internal const string ALCompilerSafeCastCheck = "SafeCastCheck";

	internal const string ALCompilerToNavValueMethodName = "ToNavValue";

	internal const string ALCompilerObjectToNavValueMethodName = "ObjectToNavValue";

	internal const string ALCompilerToByteMethodName = "ToByte";

	internal const string ALCompilerToCharMethodName = "ToChar";

	internal const string ALCompilerToIntegerMethodName = "ToInt32";

	internal const string ALCompilerToBigIntegerMethodName = "ToInt64";

	internal const string ALCompilerToDecimalMethodName = "ToDecimal";

	internal const string ALCompilerToBooleanMethodName = "ToBoolean";

	internal const string ALCompilerToDurationMethodName = "ToDuration";

	internal const string ALCompilerGuidToNavCodeMethodName = "GuidToNavCode";

	internal const string ALCompilerToSByteMethodName = "ToSByte";

	internal const string ALCompilerToInt16MethodName = "ToInt16";

	internal const string ALCompilerToUInt16MethodName = "ToUInt16";

	internal const string ALCompilerToUInt32MethodName = "ToUInt32";

	internal const string ALCompilerToUInt64MethodName = "ToUInt64";

	internal const string ALCompilerToSingleMethodName = "ToSingle";

	internal const string ALCompilerToDoubleMethodName = "ToDouble";

	internal const string ALCompilerGuidToNavTextMethodName = "GuidToNavText";

	internal const string ALCompilerToVariantMethodName = "ToVariant";

	internal const string ALCompilerNavRecordToVariantMethodName = "NavRecordToVariant";

	internal const string ALCompilerNavValueToVariantMethodName = "NavValueToVariant";

	internal const string ALCompilerGuidToStringMethodName = "GuidToString";

	internal const string ALCompilerStringToGuidMethodName = "StringToGuid";

	internal const string ALCompilerNavIndirectValueToByteMethodName = "NavIndirectValueToByte";

	internal const string ALCompilerNavIndirectValueToCharMethodName = "NavIndirectValueToChar";

	internal const string ALCompilerNavIndirectValueToNavCodeunitHandleMethodName = "NavIndirectValueToNavCodeunitHandle";

	internal const string ALCompilerNavIndirectValueToBooleanMethodName = "NavIndirectValueToBoolean";

	internal const string ALCompilerNavIndirectValueToInt32MethodName = "NavIndirectValueToInt32";

	internal const string ALCompilerNavIndirectValueToInt64MethodName = "NavIndirectValueToInt64";

	internal const string ALCompilerNavIndirectValueToDecimalMethodName = "NavIndirectValueToDecimal";

	internal const string ALCompilerNavIndirectValueToFormResultMethodName = "NavIndirectValueToFormResult";

	internal const string ALCompilerNavIndirectValueToGuidMethodName = "NavIndirectValueToGuid";

	internal const string ALCompilerNavIndirectValueToINavRecordHandleMethodName = "NavIndirectValueToINavRecordHandle";

	internal const string ALCompilerNavIndirectValueToStringMethodName = "NavIndirectValueToString";

	internal const string ALCompilerNavIndirectValueToTransactionTypeMethodName = "NavIndirectValueToTransactionType";

	internal const string ALCompilerNavIndirectValueToNavValueMethodName = "NavIndirectValueToNavValue";

	internal const string ALCompilerToNavDotNetMethodName = "ToNavDotNet";

	internal const string ALCompilerCompareNavValuesMethodName = "CompareNavValues";

	internal const string NavValueDefinedLengthMetadataTypeName = "NavValueDefinedLengthMetadata";

	internal const string NavDefinedLengthMetadataTypeName = "NavDefinedLengthMetadata";

	internal const string CompareToMethodName = "CompareTo";

	internal const string InitializeFormMethodName = "InitializeForm";

	internal const string CallInitializeComponentExtensionMethodName = "CallInitializeComponentExtensionMethod";

	internal const string RegisterUIPartMethodName = "RegisterUIPart";

	internal const string RegisterSourceExpressionMethodName = "RegisterSourceExpression";

	internal const string RegisterAutoFormatExpressionMethodName = "RegisterAutoFormatExpression";

	internal const string RegisterGetDecimalStringExpressionMethodName = "RegisterGetDecimalStringExpression";

	internal const string RegisterFormatStringExpressionMethodName = "RegisterFormatStringExpression";

	internal const string RegisterDynamicCaptionExpressionMethodName = "RegisterDynamicCaptionExpression";

	internal const string ConvertToDotNetFormatStringMethodName = "ConvertToDotNetFormatString";

	internal const string GetDecimalStringMethodName = "GetDecimalString";

	internal const string StringHelperTypeName = "StringHelper";

	internal const string CompareMethodName = "Compare";

	internal const string InListClassName = "InList";

	internal const string InListNavValueClassName = "InListNavValue";

	internal const string ALSystemNumericClassName = "ALSystemNumeric";

	internal const string ALSystemNumericTruncateMethodName = "ALTruncate";

	internal const string ALSystemVariableClassName = "ALSystemVariable";

	internal const string ALSystemVariableClearMethodName = "Clear";

	internal const string SecurityFilteringEnumName = "SecurityFiltering";

	internal const string SecurityFilteringValidatedName = "Validated";

	internal const string SecurityFilteringDisallowedName = "Disallowed";

	internal const string SecurityFilteringIgnoredName = "Ignored";

	internal const string SecurityFilteringFilteredName = "Filtered";

	internal const string NavPageConstructorRecordParamName = "record";

	internal const string NavRecordConstructorMetaTableParamName = "metaTable";

	internal const string NavRecordConstructorIsTemporaryParamName = "isTemporary";

	internal const string NavRecordConstructorSharedTableParamName = "sharedTable";

	internal const string NavRecordConstructorCompanyNameParamName = "companyName";

	internal const string NavRecordConstructorSecurityFilteringParamName = "securityFiltering";

	internal const string NavConstructorMetadataParamName = "metadata";

	internal const string InitializeGlobalVariablesMethodName = "InitializeGlobalVariables";

	internal const string OnMetadataLoadedMethodName = "OnMetadataLoaded";

	internal const string GetPartMethodName = "GetPart";

	internal const string CreateNavFormHandleMethodName = "CreateNavFormHandle";

	internal const string GetDataItemMethodName = "GetDataItem";

	internal const string ALFilterPropertyName = "ALFilter";

	internal const string ExtensionIdParameterName = "expressionId";

	internal const string PartIdParameterName = "partId";

	internal const string GetAutoFormatStringMethodName = "GetAutoFormatString";

	internal const string GetDecimalPlacesMethodName = "GetDecimalPlaces";

	internal const string FieldNoParameterName = "fieldNo";

	internal const string HandledParameterName = "handled";

	internal const string EnsureGlobalVariablesInitializedMethodName = "EnsureGlobalVariablesInitialized";

	internal const string EvaluateCaptionClassMethodName = "EvaluateCaptionClass";

	internal const string EmptyPropertyName = "Empty";

	internal const string InstancePropertyName = "Instance";

	internal const string InvokeAutoFormatTranslateMethodName = "InvokeAutoFormatTranslate";

	internal const string SetTextVariableValueMethodName = "SetTextVariableValue";

	internal const string GetTextVariableValueMethodName = "GetTextVariableValue";

	internal const string CallOnMetadataLoadedExtensionMethodName = "CallOnMetadataLoadedExtensionMethod";

	internal const string CallGetAutoFormatStringExtensionMethodName = "CallGetAutoFormatStringExtensionMethod";

	internal const string CallGetDecimalPlacesExtensionMethodName = "CallGetDecimalPlacesExtensionMethod";

	internal const string CallEvaluateCaptionClassExtensionMethod = "CallEvaluateCaptionClassExtensionMethod";

	internal const string ResultVariableName = "result";

	internal const string EvaluateSourceExpression = "EvaluateSourceExpression";

	internal const string BeginInitializationMethodName = "BeginInitialization";

	internal const string EndInitializationMethodName = "EndInitialization";

	internal const string FieldNameParameterName = "fieldName";

	internal const string NavValueTypeName = "NavValue";

	internal const string PermissionFieldName = "indirectPermissionList";

	internal const string PermissionPropertyName = "IndirectPermissionList";

	internal const string RequestOptionsPagePropertyName = "RequestOptionsPage";

	internal const string EvaluateSourceExpressionDecimalFieldSuffix = "Format";

	internal const string DataItemSuffix = "DataItem";

	internal const string AddMethodName = "Add";

	internal const string MetadataObjectName = "Metadata";

	internal const string RequestFormMetadataName = "RequestFormMetadata";

	internal const string GetTableViewMethodName = "GetTableView";

	internal const string SetTableViewMethodName = "SetTableView";

	internal const string NavTableViewMethodName = "NavTableView";

	internal const string ToByteArrayMethodName = "ToByteArray";

	internal const string GetBytesMethodName = "GetBytes";

	internal const string BookmarkPagePropertyName = "BookmarkType";

	internal const string BookmarkEnumTypeName = "BookmarkType";

	internal const string BookmarkEnumTemporaryName = "Temporary";

	internal const string NCLMetaQueryTypeName = "NCLMetaQuery";

	internal const string NCLMetaReportTypeName = "NCLMetaReport";

	internal const string NCLMetaXmlPortTypeName = "NCLMetaXmlPort";

	internal static InvocationExpressionSyntax ALCompilerObjectToBooleanCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToBoolean"));

	internal static InvocationExpressionSyntax ALCompilerObjectToByteCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToByte"));

	internal static InvocationExpressionSyntax ALCompilerObjectToCharCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToChar"));

	internal static InvocationExpressionSyntax ALCompilerObjectToIntegerCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToInt32"));

	internal static InvocationExpressionSyntax ALCompilerObjectToBigIntegerCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToInt64"));

	internal static InvocationExpressionSyntax ALCompilerObjectToDecimalCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToDecimal"));

	internal static InvocationExpressionSyntax ALCompilerObjectToGuidCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToGuid"));

	internal static InvocationExpressionSyntax ALCompilerObjectToStringCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToString"));

	internal static InvocationExpressionSyntax ALCompilerObjectToFormResultCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToFormResult"));

	internal static InvocationExpressionSyntax ALCompilerObjectToConnectionTypeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToConnectionType"));

	internal static InvocationExpressionSyntax ALCompilerObjectToFilterPageBuilderCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToFilterPageBuilder"));

	internal static InvocationExpressionSyntax ALCompilerObjectToEncodingCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToEncoding"));

	internal static InvocationExpressionSyntax ALCompilerObjectToExactINavRecordHandleCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToExactINavRecordHandle"));

	internal static InvocationExpressionSyntax ALCompilerConvertToDotNetFormatStringCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ConvertToDotNetFormatString"));

	internal static InvocationExpressionSyntax ALCompilerToByteCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToByte"));

	internal static InvocationExpressionSyntax ALCompilerToCharCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToChar"));

	internal static InvocationExpressionSyntax ALCompilerToIntegerCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToInt32"));

	internal static InvocationExpressionSyntax ALCompilerToBigIntegerCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToInt64"));

	internal static InvocationExpressionSyntax ALCompilerToDecimalCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToDecimal"));

	internal static InvocationExpressionSyntax ALCompilerToBooleanCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToBoolean"));

	internal static InvocationExpressionSyntax ALCompilerToDurationCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToDuration"));

	internal static InvocationExpressionSyntax ALCompilerGuidToNavCodeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "GuidToNavCode"));

	internal static InvocationExpressionSyntax ALCompilerGuidToNavTextCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "GuidToNavText"));

	internal static InvocationExpressionSyntax ALCompilerToSByteCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToSByte"));

	internal static InvocationExpressionSyntax ALCompilerToInt16Call = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToInt16"));

	internal static InvocationExpressionSyntax ALCompilerToUInt16Call = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToUInt16"));

	internal static InvocationExpressionSyntax ALCompilerToUInt32Call = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToUInt32"));

	internal static InvocationExpressionSyntax ALCompilerToUInt64Call = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToUInt64"));

	internal static InvocationExpressionSyntax ALCompilerToSingleCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToSingle"));

	internal static InvocationExpressionSyntax ALCompilerToDoubleCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToDouble"));

	internal static InvocationExpressionSyntax ALCompilerToVariantCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToVariant"));

	internal static InvocationExpressionSyntax ALCompilerNavRecordToVariantCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavRecordToVariant"));

	internal static InvocationExpressionSyntax ALCompilerNavValueToVariantCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavValueToVariant"));

	internal static InvocationExpressionSyntax ALCompilerGuidToStringCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "GuidToString"));

	internal static InvocationExpressionSyntax ALCompilerStringToGuidCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "StringToGuid"));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToByteCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToByte"));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToCharCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToChar"));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToNavCodeunitHandleCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToNavCodeunitHandle"));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToBooleanCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToBoolean"));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToInt32Call = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToInt32"));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToInt64Call = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToInt64"));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToDecimalCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToDecimal"));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToFormResultCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToFormResult"));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToGuidCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToGuid"));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToINavRecordHandle = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToINavRecordHandle"));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToStringCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToString"));

	internal static InvocationExpressionSyntax ALCompilerToNavDotNetCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToNavDotNet"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(SpecializedCollections.SingletonEnumerable(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression())))));

	internal static InvocationExpressionSyntax ALCompilerNavIndirectValueToTransactionTypeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavIndirectValueToTransactionType"));

	internal static InvocationExpressionSyntax ALCompilerObjectToNavInStreamCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToNavInStream"));

	internal static InvocationExpressionSyntax ALCompilerObjectToNavOutStreamCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToNavOutStream"));

	internal static InvocationExpressionSyntax ALCompilerDotNetToNavOutStreamCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "DotNetToNavOutStream"));

	internal static InvocationExpressionSyntax ALCompilerDotNetToNavInStreamCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "DotNetToNavInStream"));

	internal static InvocationExpressionSyntax ALCompilerInStreamToDotNetCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "InStreamToDotNet"));

	internal static InvocationExpressionSyntax ALCompilerOutStreamToDotNetCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "OutStreamToDotNet"));

	internal static InvocationExpressionSyntax ALCompilerDotNetToCharCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavDotNetToChar"));

	internal static InvocationExpressionSyntax ALCompilerNavCodeToDotNetCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavCodeToDotNet"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(SpecializedCollections.SingletonEnumerable(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression())))));

	internal static InvocationExpressionSyntax ALCompilerNavTextToDotNetCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "NavTextToDotNet"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(SpecializedCollections.SingletonEnumerable(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression())))));

	internal static InvocationExpressionSyntax ALCompilerDateTimeToDotNetCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "DateTimeToDotNet"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(SpecializedCollections.SingletonEnumerable(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression())))));

	internal static InvocationExpressionSyntax NavBigTextCreateFromObjectCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("NavBigText", "CreateFromObject"));

	internal static InvocationExpressionSyntax NavTextCreateFromObjectCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("NavText", "CreateFromObject"));

	internal static InvocationExpressionSyntax NavCodeCreateFromObjectCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("NavCode", "CreateFromObject"));

	internal static InvocationExpressionSyntax ALSystemNumericTruncateCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALSystemNumeric", "ALTruncate"));

	internal static AttributeSyntax NavFunctionVisibilityAttribute_Internal = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavFunctionVisibility"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(CodeGenHelper.MemberAccessExpression("FunctionVisibility", "Internal")))));

	internal static AttributeSyntax NavFunctionVisibilityAttribute_External = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavFunctionVisibility"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(CodeGenHelper.MemberAccessExpression("FunctionVisibility", "External")))));

	internal static InvocationExpressionSyntax NclOptionMetadataCreateCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("NCLOptionMetadata", "Create"));

	internal static InvocationExpressionSyntax NclEnumMetadataCreateCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("NCLEnumMetadata", "Create"));

	internal static InvocationExpressionSyntax NavOptionCreateCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("NavOption", "Create"));

	internal static InvocationExpressionSyntax NavTimeCreateCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("NavTime", "Create"));

	internal static InvocationExpressionSyntax NavDateCreateCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("NavDate", "Create"));

	internal static InvocationExpressionSyntax NavDateTimeCreateCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("NavDateTime", "Create"));

	internal static ObjectCreationExpressionSyntax NavTextConstCreateCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTextConstant"));

	internal static ArrayCreationExpressionSyntax NavTextConstCreateLangIdArray = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.IntKeyword)))).AddTypeRankSpecifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayRankSpecifier().AddSizes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.OmittedArraySizeExpression()));

	internal static ArrayCreationExpressionSyntax NavTextConstCreateLangTextArray = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringKeyword)))).AddTypeRankSpecifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayRankSpecifier().AddSizes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.OmittedArraySizeExpression()));

	internal static InvocationExpressionSyntax ALSystemVariableClearCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALSystemVariable", "Clear"));

	internal static InvocationExpressionSyntax StmtHitCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("StmtHit"));

	internal static InvocationExpressionSyntax CStmtHitCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CStmtHit"));

	internal static MemberAccessExpressionSyntax Decimal18Zero = CodeGenHelper.MemberAccessExpression("Decimal18", "Zero");

	internal static MemberAccessExpressionSyntax DataErrorTrapErrorExpression = CodeGenHelper.MemberAccessExpression("DataError", "TrapError");

	internal static MemberAccessExpressionSyntax DataErrorThrowErrorExpression = CodeGenHelper.MemberAccessExpression("DataError", "ThrowError");

	internal static ExpressionSyntax NavCodeunitOptionsSingleInstance = CodeGenHelper.MemberAccessExpression("NavCodeunitOptions", "SingleInstance");

	internal static ExpressionSyntax NavCodeunitOptionsEventManualBinding = CodeGenHelper.MemberAccessExpression("NavCodeunitOptions", "EventManualBinding");

	internal static ExpressionSyntax NavCodeunitOptionsSingleInstanceEventManualBinding = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.BitwiseOrExpression, NavCodeunitOptionsSingleInstance, NavCodeunitOptionsEventManualBinding);

	internal static ExpressionSyntax NavCodeunitOptionsDefault = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(0));

	internal static MemberAccessExpressionSyntax CompilationTargetPersonalization = CodeGenHelper.MemberAccessExpression("CompilationTarget", "Personalization");

	internal static MemberAccessExpressionSyntax CompilationTargetExtension = CodeGenHelper.MemberAccessExpression("CompilationTarget", "Extension");

	internal static MemberAccessExpressionSyntax CompilationTargetSolution = CodeGenHelper.MemberAccessExpression("CompilationTarget", "Solution");

	internal static MemberAccessExpressionSyntax CompilationTargetInternal = CodeGenHelper.MemberAccessExpression("CompilationTarget", "Internal");

	internal static LiteralExpressionSyntax TrueLiteralExpression = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.TrueLiteralExpression);

	internal static LiteralExpressionSyntax FalseLiteralExpression = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.FalseLiteralExpression);

	internal static InvocationExpressionSyntax AssertErrorCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("AssertError")));

	internal static InvocationExpressionSyntax ALCompilerToNavValueCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ToNavValue"));

	internal static InvocationExpressionSyntax ALCompilerObjectToNavValueCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "ObjectToNavValue"));

	internal static ArrayCreationExpressionSyntax ObjectArrayCreation = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ObjectKeyword))).AddRankSpecifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayRankSpecifier().AddSizes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.OmittedArraySizeExpression())));

	internal static ArrayCreationExpressionSyntax EmptyObjectArrayCreation = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ObjectKeyword))).AddRankSpecifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayRankSpecifier().AddSizes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.OmittedArraySizeExpression()))).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InitializerExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ArrayInitializerExpression));

	internal static InvocationExpressionSyntax ALSystemStringSetCharCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALSystemString", "SetChar"));

	internal static MemberAccessExpressionSyntax GuidEmptyExpression = CodeGenHelper.MemberAccessExpression("Guid", "Empty");

	internal static MemberAccessExpressionSyntax ALStreamDefaultLengthExpression = CodeGenHelper.MemberAccessExpression("ALStream", "DefaultLength");

	internal static InvocationExpressionSyntax StringHelperCompareCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("StringHelper"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Compare")));

	internal static MemberAccessExpressionSyntax NCLOptionMetadataDefault = CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NCLOptionMetadata"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Default"));

	internal static InvocationExpressionSyntax GuidNewGuidExpression = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("Guid", "NewGuid"));

	internal static InvocationExpressionSyntax NavDotNetGetDotNetTypeCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("NavDotNet", "GetDotNetType"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(SpecializedCollections.SingletonEnumerable(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression())))));

	internal static AttributeSyntax DebuggerBrowsableNeverAttribute = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName(typeof(DebuggerBrowsableAttribute).FullName)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(typeof(DebuggerBrowsableState).FullName), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(DebuggerBrowsableState.Never.ToString()))));

	internal static AttributeSyntax DebuggerNonUserCodeAttribute = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName(typeof(DebuggerNonUserCodeAttribute).FullName));

	internal static LiteralExpressionSyntax NullLiteralExpression = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression);

	internal static MemberAccessExpressionSyntax SourceExpressionTypeDataCaptionExpression = CodeGenHelper.MemberAccessExpression("SourceExpressionType", "DataCaption");

	internal static MemberAccessExpressionSyntax SourceExpressionTypeSourceExpressionExpression = CodeGenHelper.MemberAccessExpression("SourceExpressionType", "SourceExpression");

	internal static MemberAccessExpressionSyntax SourceExpressionTypeDataItemExpression = CodeGenHelper.MemberAccessExpression("SourceExpressionType", "DataItem");

	internal static MemberAccessExpressionSyntax SourceExpressionTypeVariableIncludedInDataSetExpression = CodeGenHelper.MemberAccessExpression("SourceExpressionType", "VariableIncludedInDataSet");

	internal static MemberAccessExpressionSyntax SourceExpressionTypePropertyExpression = CodeGenHelper.MemberAccessExpression("SourceExpressionType", "Property");

	internal static MemberAccessExpressionSyntax SourceExpressionTypeDynamicCueStyleExpression = CodeGenHelper.MemberAccessExpression("SourceExpressionType", "DynamicCueStyle");

	internal static MemberAccessExpressionSyntax CurrPageExpressionSyntax = CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CurrPage"));

	internal static InvocationExpressionSyntax GetDynamicCueStyleCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CurrPageExpressionSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("GetDynamicCueStyle")));

	internal static InvocationExpressionSyntax AlCompilerInvokeAutoFormatTranslateCall = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("ALCompiler", "InvokeAutoFormatTranslate"));

	internal static StatementSyntax InvokeCallOnMetadataLoadedExtensionStatement = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CallOnMetadataLoadedExtensionMethod"))));

	internal static StatementSyntax InvokeCallInitializeComponentExtensionMethodStatement = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CallInitializeComponentExtensionMethod"))));

	internal static StatementSyntax BookmarkEqualsBookmarkTemporary = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("BookmarkType")), CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("BookmarkType"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Temporary"))));

	internal static TypeSyntax ClrVoidType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.VoidKeyword));

	internal static TypeSyntax ClrObjectType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ObjectKeyword));

	internal static TypeSyntax ClrBoolType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.BoolKeyword));

	internal static TypeSyntax ClrByteType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ByteKeyword));

	internal static TypeSyntax ClrSByteType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SByteKeyword));

	internal static TypeSyntax ClrCharType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.CharKeyword));

	internal static TypeSyntax ClrInt16Type = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ShortKeyword));

	internal static TypeSyntax ClrUInt16Type = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.UShortKeyword));

	internal static TypeSyntax ClrInt32Type = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.IntKeyword));

	internal static TypeSyntax ClrUInt32Type = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.UIntKeyword));

	internal static TypeSyntax ClrInt64Type = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LongKeyword));

	internal static TypeSyntax ClrUInt64Type = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ULongKeyword));

	internal static TypeSyntax ClrSingleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.FloatKeyword));

	internal static TypeSyntax ClrDoubleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DoubleKeyword));

	internal static TypeSyntax ClrDecimalType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DecimalKeyword));

	internal static TypeSyntax ClrStringType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringKeyword));

	internal static TypeSyntax ClrGuidType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("Guid");

	internal static TypeSyntax ClrTimeSpanType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("TimeSpan");

	internal static TypeSyntax ClrDateTimeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("DateTime");

	internal static TypeSyntax ClrVarType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("var");

	internal static TypeSyntax NavVoidType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavVoidType");

	internal static TypeSyntax RuntimeBlobType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavBLOB");

	internal static TypeSyntax RuntimeMediaType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavMedia");

	internal static TypeSyntax RuntimeMediaSetType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavMediaSet");

	internal static TypeSyntax RuntimeBooleanType = ClrBoolType;

	internal static TypeSyntax NavBooleanType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavBoolean");

	internal static TypeSyntax RuntimeByteType = ClrByteType;

	internal static TypeSyntax NavByteType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavByte");

	internal static TypeSyntax RuntimeCharType = ClrCharType;

	internal static TypeSyntax NavCharType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavChar");

	internal static TypeSyntax RuntimeIntegerType = ClrInt32Type;

	internal static TypeSyntax NavIntegerType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavInteger");

	internal static TypeSyntax RuntimeBigIntegerType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LongKeyword));

	internal static TypeSyntax NavBigIntegerType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavBigInteger");

	internal static TypeSyntax RuntimeDecimalType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("Decimal18");

	internal static TypeSyntax NavDecimalType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavDecimal");

	internal static TypeSyntax RuntimeOptionType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavOption");

	internal static TypeSyntax RuntimeParameterOptionType = ClrInt32Type;

	internal static TypeSyntax NavOptionType = RuntimeOptionType;

	internal static TypeSyntax NclOptionMetadataType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NCLOptionMetadata");

	internal static TypeSyntax NclMetaTableType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NCLMetaTable");

	internal static TypeSyntax RuntimeTextType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavText");

	internal static TypeSyntax RuntimeOemTextType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavOemText");

	internal static TypeSyntax NavTextType = RuntimeTextType;

	internal static TypeSyntax RuntimeCodeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavCode");

	internal static TypeSyntax NavCodeType = RuntimeCodeType;

	internal static TypeSyntax RuntimeTextConstType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTextConstant");

	internal static TypeSyntax NavTextConstType = RuntimeTextConstType;

	internal static TypeSyntax RuntimeDateType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavDate");

	internal static TypeSyntax NavDateType = RuntimeDateType;

	internal static TypeSyntax RuntimeTimeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTime");

	internal static TypeSyntax NavTimeType = RuntimeTimeType;

	internal static TypeSyntax RuntimeDateTimeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavDateTime");

	internal static TypeSyntax NavDateTimeType = RuntimeDateTimeType;

	internal static TypeSyntax RuntimeDurationType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavDuration");

	internal static TypeSyntax NavDurationType = RuntimeDurationType;

	internal static TypeSyntax RuntimeDateFormulaType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavDateFormula");

	internal static TypeSyntax NavDateFormulaType = RuntimeDateFormulaType;

	internal static TypeSyntax RuntimeNavHttpClientType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavHttpClient");

	internal static TypeSyntax NavHttpClientType = RuntimeNavHttpClientType;

	internal static TypeSyntax RuntimeNavHttpContentType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavHttpContent");

	internal static TypeSyntax NavHttpContentType = RuntimeNavHttpContentType;

	internal static TypeSyntax RuntimeNavHttpRequestMessageType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavHttpRequestMessage");

	internal static TypeSyntax NavHttpRequestMessageType = RuntimeNavHttpRequestMessageType;

	internal static TypeSyntax RuntimeNavHttpResponseMessageType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavHttpResponseMessage");

	internal static TypeSyntax NavHttpResponseMessageType = RuntimeNavHttpResponseMessageType;

	internal static TypeSyntax RuntimeNavHttpHeadersMessageType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavHttpHeaders");

	internal static TypeSyntax NavHttpHeadersType = RuntimeNavHttpHeadersMessageType;

	internal static TypeSyntax RuntimeNavTextBuilderType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTextBuilder");

	internal static TypeSyntax NavTextBuilderType = RuntimeNavTextBuilderType;

	internal static TypeSyntax RuntimeNavXmlAttributeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlAttribute");

	internal static TypeSyntax NavXmlAttributeType = RuntimeNavXmlAttributeType;

	internal static TypeSyntax RuntimeNavXmlAttributeCollectionType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlAttributeCollection");

	internal static TypeSyntax NavXmlAttributeCollectionType = RuntimeNavXmlAttributeCollectionType;

	internal static TypeSyntax RuntimeNavXmlCommentType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlComment");

	internal static TypeSyntax NavXmlCommentType = RuntimeNavXmlCommentType;

	internal static TypeSyntax RuntimeNavXmlCDataType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlCData");

	internal static TypeSyntax NavXmlCDataType = RuntimeNavXmlCDataType;

	internal static TypeSyntax RuntimeNavXmlDeclarationType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlDeclaration");

	internal static TypeSyntax NavXmlDeclarationType = RuntimeNavXmlDeclarationType;

	internal static TypeSyntax RuntimeNavXmlDocumentType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlDocument");

	internal static TypeSyntax NavXmlDocumentType = RuntimeNavXmlDocumentType;

	internal static TypeSyntax RuntimeNavXmlDocumentTypeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlDocumentType");

	internal static TypeSyntax NavXmlDocumentTypeType = RuntimeNavXmlDocumentTypeType;

	internal static TypeSyntax RuntimeNavXmlElementType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlElement");

	internal static TypeSyntax NavXmlElementType = RuntimeNavXmlElementType;

	internal static TypeSyntax RuntimeNavXmlNamespaceManagerType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlNamespaceManager");

	internal static TypeSyntax NavXmlNamespaceManagerType = RuntimeNavXmlNamespaceManagerType;

	internal static TypeSyntax RuntimeNavXmlNameTableType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlNameTable");

	internal static TypeSyntax NavXmlNameTableType = RuntimeNavXmlNameTableType;

	internal static TypeSyntax RuntimeNavXmlNodeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlNode");

	internal static TypeSyntax NavXmlNodeType = RuntimeNavXmlNodeType;

	internal static TypeSyntax RuntimeNavXmlNodeListType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlNodeList");

	internal static TypeSyntax NavXmlNodeListType = RuntimeNavXmlNodeListType;

	internal static TypeSyntax RuntimeNavXmlProcessingInstructionType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlProcessingInstruction");

	internal static TypeSyntax NavXmlProcessingInstructionType = RuntimeNavXmlProcessingInstructionType;

	internal static TypeSyntax RuntimeNavXmlTextType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlText");

	internal static TypeSyntax NavXmlTextType = RuntimeNavXmlTextType;

	internal static TypeSyntax RuntimeNavXmlReadOptionsType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlReadOptions");

	internal static TypeSyntax NavXmlReadOptionsType = RuntimeNavXmlReadOptionsType;

	internal static TypeSyntax RuntimeNavXmlWriteOptionsType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlWriteOptions");

	internal static TypeSyntax NavXmlWriteOptionsType = RuntimeNavXmlWriteOptionsType;

	internal static TypeSyntax RuntimeNavVersionType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavVersion");

	internal static TypeSyntax NavVersionType = RuntimeNavVersionType;

	internal static TypeSyntax RuntimeNavModuleInfo = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavModuleInfo");

	internal static TypeSyntax NavModuleInfoType = RuntimeNavModuleInfo;

	internal static TypeSyntax RuntimeNavModuleDependencyInfo = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavModuleDependencyInfo");

	internal static TypeSyntax NavModuleDependencyInfoType = RuntimeNavModuleDependencyInfo;

	internal static TypeSyntax NavWebServiceActionContextType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavWebServiceActionContext");

	internal static TypeSyntax RuntimeNavJsonTokenType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavJsonToken");

	internal static TypeSyntax NavJsonTokenType = RuntimeNavJsonTokenType;

	internal static TypeSyntax RuntimeNavJsonArrayType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavJsonArray");

	internal static TypeSyntax NavJsonArrayType = RuntimeNavJsonArrayType;

	internal static TypeSyntax RuntimeNavJsonObjectType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavJsonObject");

	internal static TypeSyntax NavJsonObjectType = RuntimeNavJsonObjectType;

	internal static TypeSyntax RuntimeNavJsonValueType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavJsonValue");

	internal static TypeSyntax NavJsonValueType = RuntimeNavJsonValueType;

	internal static TypeSyntax DataItemType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("DataItem");

	internal static TypeSyntax RuntimeGuidType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("Guid");

	internal static TypeSyntax NavGuidType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavGuid");

	internal static TypeSyntax RuntimeVariantType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavVariant");

	internal static TypeSyntax NavVariantType = RuntimeVariantType;

	internal static TypeSyntax RuntimeInStreamType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavInStream");

	internal static TypeSyntax NavInStreamType = RuntimeInStreamType;

	internal static TypeSyntax RuntimeOutStreamType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavOutStream");

	internal static TypeSyntax NavOutStreamType = RuntimeOutStreamType;

	internal static TypeSyntax RuntimeRecordRefType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavRecordRef");

	internal static TypeSyntax NavRecordRefType = RuntimeRecordRefType;

	internal static TypeSyntax RuntimeFieldRefType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavFieldRef");

	internal static TypeSyntax NavFieldRefType = RuntimeFieldRefType;

	internal static TypeSyntax RuntimeKeyRefType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavKeyRef");

	internal static TypeSyntax NavKeyRefType = RuntimeKeyRefType;

	internal static TypeSyntax RuntimeRecordIdType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavRecordId");

	internal static TypeSyntax NavRecordIdType = RuntimeRecordIdType;

	internal static TypeSyntax RuntimeTableFilterType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTableFilter");

	internal static TypeSyntax NavTableFilterType = RuntimeTableFilterType;

	internal static TypeSyntax RuntimeNotificationType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavNotification");

	internal static TypeSyntax RuntimeCodeunitHandleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavCodeunitHandle");

	internal static TypeSyntax NavCodeunitHandleType = RuntimeCodeunitHandleType;

	internal static TypeSyntax RuntimeReportHandleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavReportHandle");

	internal static TypeSyntax NavReportHandleType = RuntimeReportHandleType;

	internal static TypeSyntax RuntimeQueryHandleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavQueryHandle");

	internal static TypeSyntax NavQueryHandleType = RuntimeQueryHandleType;

	internal static TypeSyntax RuntimeXmlPortHandleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlPortHandle");

	internal static TypeSyntax NavXmlPortHandleType = RuntimeXmlPortHandleType;

	internal static TypeSyntax NavXmlPortTextNodeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlPortTextNode");

	internal static TypeSyntax NavXmlPortFieldNodeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlPortFieldNode");

	internal static TypeSyntax NavXmlPortTableNodeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlPortTableNode");

	internal static TypeSyntax RuntimeRecordHandleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("INavRecordHandle");

	internal static TypeSyntax NavRecordHandleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavRecordHandle");

	internal static TypeSyntax RuntimePageHandleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavFormHandle");

	internal static TypeSyntax RuntimeTestPageHandleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTestPageHandle");

	internal static TypeSyntax NavTestPageHandleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTestPageHandle");

	internal static TypeSyntax NCLMetaQueryType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NCLMetaQuery");

	internal static TypeSyntax NCLMetaReportType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NCLMetaReport");

	internal static TypeSyntax NCLMetaXmlPortType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NCLMetaXmlPort");

	internal static TypeSyntax NavCurrentThreadSessionCompanySharedObjectsValue = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavCurrentThread.Session.Company.SharedObjects");

	internal static TypeSyntax MasterPageType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("MasterPage");

	internal static TypeSyntax RequestPageType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("RequestPage");

	internal static TypeSyntax NavCodeunitType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavCodeunit");

	internal static TypeSyntax NavRecordType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavRecord");

	internal static TypeSyntax NavPageType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavForm");

	internal static TypeSyntax NavRequestPageBaseType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("RequestPageBase");

	internal static TypeSyntax NavReportType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavReport");

	internal static TypeSyntax NavQueryType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavQuery");

	internal static TypeSyntax NavXmlPortType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavXmlPort");

	internal static TypeSyntax NavTestCodeunitType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTestCodeunit");

	internal static TypeSyntax NavTestRunnerCodeunitType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTestRunnerCodeUnit");

	internal static TypeSyntax NavUpgradeCodeunitType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavUpgradeCodeunit");

	internal static TypeSyntax NavPageExtensionType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavFormExtension");

	internal static TypeSyntax NavRecordExtensionType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavRecordExtension");

	internal static TypeSyntax NavInteropEventHandlerBaseType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavInteropEventHandlerBase");

	internal static TypeSyntax NavArrayType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavArray");

	internal static TypeSyntax ScopeIdType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.UIntKeyword));

	internal static TypeSyntax NavMethodScopeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavMethodScope");

	internal static TypeSyntax NavTriggerMethodScopeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTriggerMethodScope");

	internal static TypeSyntax NavTriggerType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavTrigger");

	internal static TypeSyntax NavEventScopeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavEventScope");

	internal static TypeSyntax INavRecordHandleType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("INavRecordHandle");

	internal static TypeSyntax ITreeObjectType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("ITreeObject");

	internal static TypeSyntax ByRefType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("ByRef");

	internal static TypeSyntax InListNavValueType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("InListNavValue");

	internal static TypeSyntax RuntimeDialogType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavDialog");

	internal static TypeSyntax RuntimeBigTextType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavBigText");

	internal static TypeSyntax NavBigTextType = RuntimeBigTextType;

	internal static TypeSyntax RuntimeFileType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavFile");

	internal static TypeSyntax NavFileType = RuntimeFileType;

	internal static TypeSyntax RuntimeFilterPageBuilderType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavFilterPageBuilder");

	internal static TypeSyntax NavFilterPageBuilderType = RuntimeFilterPageBuilderType;

	internal static TypeSyntax RuntimeSessionSettingsType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavSessionSettings");

	internal static TypeSyntax NavSessionSettingsType = RuntimeSessionSettingsType;

	internal static TypeSyntax FormResultType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("FormResult");

	internal static TypeSyntax NavNCLInvalidNumberOfArgumentsExceptionType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavNCLInvalidNumberOfArgumentsException");

	internal static TypeSyntax NavValueDefinedLengthMetadataType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavValueDefinedLengthMetadata");

	internal static TypeSyntax NavDefinedLengthMetadataType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavDefinedLengthMetadata");

	internal static TypeSyntax NavValueType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavValue");

	internal static TypeSyntax NavDotNetType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavDotNet");

	internal static TypeSyntax NavDotNetFactoryMethodType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("Factory");

	internal static TypeSyntax RuntimeNavVoidType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavVoidType");

	internal static TypeSyntax RuntimeNavALErrorInfoType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("NavALErrorInfo");

	internal static TypeSyntax RuntimeTypeSyntaxSafe_Generated(this NavTypeKind kind, bool asArrayType = false)
	{
		return kind switch
		{
			NavTypeKind.Action => RuntimeNavActionTypeSyntax, 
			NavTypeKind.ClientType => RuntimeNavClientTypeTypeSyntax, 
			NavTypeKind.DataClassification => RuntimeNavDataClassificationTypeSyntax, 
			NavTypeKind.DataScope => RuntimeNavDataScopeTypeSyntax, 
			NavTypeKind.DefaultLayout => RuntimeNavDefaultLayoutTypeSyntax, 
			NavTypeKind.ErrorType => RuntimeNavErrorTypeTypeSyntax, 
			NavTypeKind.ExecutionContext => RuntimeNavExecutionContextTypeSyntax, 
			NavTypeKind.ExecutionMode => RuntimeNavExecutionModeTypeSyntax, 
			NavTypeKind.FieldClass => RuntimeNavFieldClassTypeSyntax, 
			NavTypeKind.FieldType => RuntimeNavFieldTypeTypeSyntax, 
			NavTypeKind.NotificationScope => RuntimeNavNotificationScopeTypeSyntax, 
			NavTypeKind.ObjectType => RuntimeNavObjectTypeTypeSyntax, 
			NavTypeKind.ReportFormat => RuntimeNavReportFormatTypeSyntax, 
			NavTypeKind.SecurityFilter => RuntimeNavSecurityFilterTypeSyntax, 
			NavTypeKind.TableConnectionType => RuntimeNavTableConnectionTypeTypeSyntax, 
			NavTypeKind.TestPermissions => RuntimeNavTestPermissionsTypeSyntax, 
			NavTypeKind.TextEncoding => RuntimeNavTextEncodingTypeSyntax, 
			NavTypeKind.TransactionModel => RuntimeNavTransactionModelTypeSyntax, 
			NavTypeKind.TransactionType => RuntimeNavTransactionTypeTypeSyntax, 
			NavTypeKind.Verbosity => RuntimeNavVerbosityTypeSyntax, 
			NavTypeKind.WebServiceActionResultCode => RuntimeNavWebServiceActionResultCodeTypeSyntax, 
			_ => null, 
		};
	}

	internal static InvocationExpressionSyntax GetObjectToTypeConversionInvocation_Generated(NavTypeKind kind)
	{
		return kind switch
		{
			NavTypeKind.Action => ALCompilerObjectToActionCall, 
			NavTypeKind.ClientType => ALCompilerObjectToClientTypeCall, 
			NavTypeKind.DataClassification => ALCompilerObjectToDataClassificationCall, 
			NavTypeKind.DataScope => ALCompilerObjectToDataScopeCall, 
			NavTypeKind.DefaultLayout => ALCompilerObjectToDefaultLayoutCall, 
			NavTypeKind.ErrorType => ALCompilerObjectToErrorTypeCall, 
			NavTypeKind.ExecutionContext => ALCompilerObjectToExecutionContextCall, 
			NavTypeKind.ExecutionMode => ALCompilerObjectToExecutionModeCall, 
			NavTypeKind.FieldClass => ALCompilerObjectToFieldClassCall, 
			NavTypeKind.FieldType => ALCompilerObjectToFieldTypeCall, 
			NavTypeKind.NotificationScope => ALCompilerObjectToNotificationScopeCall, 
			NavTypeKind.ObjectType => ALCompilerObjectToObjectTypeCall, 
			NavTypeKind.ReportFormat => ALCompilerObjectToReportFormatCall, 
			NavTypeKind.SecurityFilter => ALCompilerObjectToSecurityFilterCall, 
			NavTypeKind.TableConnectionType => ALCompilerObjectToTableConnectionTypeCall, 
			NavTypeKind.TestPermissions => ALCompilerObjectToTestPermissionsCall, 
			NavTypeKind.TextEncoding => ALCompilerObjectToTextEncodingCall, 
			NavTypeKind.TransactionModel => ALCompilerObjectToTransactionModelCall, 
			NavTypeKind.TransactionType => ALCompilerObjectToTransactionTypeCall, 
			NavTypeKind.Verbosity => ALCompilerObjectToVerbosityCall, 
			NavTypeKind.WebServiceActionResultCode => ALCompilerObjectToWebServiceActionResultCodeCall, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	internal static bool ArgumentShouldBeCloned(ParameterSymbol argument)
	{
		if (argument.IsArray)
		{
			return true;
		}
		switch (argument.Type.NavTypeKind)
		{
		case NavTypeKind.RecordRef:
		case NavTypeKind.FieldRef:
		case NavTypeKind.KeyRef:
		case NavTypeKind.Variant:
		case NavTypeKind.File:
		case NavTypeKind.Record:
		case NavTypeKind.Codeunit:
			return true;
		default:
			return false;
		}
	}

	internal static bool UseAssignMethod(TypeSymbol symbolType)
	{
		switch (symbolType.NavTypeKind)
		{
		case NavTypeKind.Dialog:
		case NavTypeKind.TestRequestPage:
		case NavTypeKind.RecordRef:
		case NavTypeKind.FieldRef:
		case NavTypeKind.KeyRef:
		case NavTypeKind.InStream:
		case NavTypeKind.OutStream:
		case NavTypeKind.Variant:
		case NavTypeKind.DotNet:
		case NavTypeKind.TestPage:
		case NavTypeKind.File:
		case NavTypeKind.FilterPageBuilder:
		case NavTypeKind.SessionSettings:
		case NavTypeKind.HttpClient:
		case NavTypeKind.HttpRequestMessage:
		case NavTypeKind.HttpResponseMessage:
		case NavTypeKind.HttpContent:
		case NavTypeKind.Record:
		case NavTypeKind.Codeunit:
		case NavTypeKind.Report:
		case NavTypeKind.XmlPort:
		case NavTypeKind.Query:
		case NavTypeKind.ControlAddIn:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsNavOptionAccess(this BoundExpression expr)
	{
		if (expr.Kind != BoundKind.OptionAccess)
		{
			return false;
		}
		BoundOptionAccess boundOptionAccess = (BoundOptionAccess)expr;
		if (boundOptionAccess.ReceiverOpt.Kind == BoundKind.SystemOptionType)
		{
			return false;
		}
		return boundOptionAccess.ReceiverOpt.Type.NavTypeKind == NavTypeKind.Option;
	}

	internal static bool ShouldBePassedByRef(this ParameterSymbol parameter)
	{
		if (parameter.IsVar && !parameter.IsArray)
		{
			return !parameter.Type.IsUserType();
		}
		return false;
	}

	internal static bool IsHandleType(this TypeSymbol type)
	{
		return type.IsUserType();
	}

	internal static TypeSyntax RuntimeTypeSyntaxSafe(this NavTypeKind kind, bool asArrayType = false)
	{
		switch (kind)
		{
		case NavTypeKind.Blob:
			return RuntimeBlobType;
		case NavTypeKind.Media:
			return RuntimeMediaType;
		case NavTypeKind.MediaSet:
			return RuntimeMediaSetType;
		case NavTypeKind.Boolean:
			return RuntimeBooleanType;
		case NavTypeKind.Byte:
			return RuntimeByteType;
		case NavTypeKind.Char:
			return RuntimeCharType;
		case NavTypeKind.Integer:
			return RuntimeIntegerType;
		case NavTypeKind.BigInteger:
			return RuntimeBigIntegerType;
		case NavTypeKind.Decimal:
			return RuntimeDecimalType;
		case NavTypeKind.Option:
		case NavTypeKind.Enum:
			return RuntimeOptionType;
		case NavTypeKind.Text:
			return RuntimeTextType;
		case NavTypeKind.OemText:
			return RuntimeOemTextType;
		case NavTypeKind.Code:
			return RuntimeCodeType;
		case NavTypeKind.TextConst:
		case NavTypeKind.Label:
			return RuntimeTextConstType;
		case NavTypeKind.Date:
			return RuntimeDateType;
		case NavTypeKind.Time:
			return RuntimeTimeType;
		case NavTypeKind.DateTime:
			return RuntimeDateTimeType;
		case NavTypeKind.Duration:
			return RuntimeDurationType;
		case NavTypeKind.DateFormula:
			return RuntimeDateFormulaType;
		case NavTypeKind.ErrorInfo:
			return RuntimeNavALErrorInfoType;
		case NavTypeKind.Guid:
			return RuntimeGuidType;
		case NavTypeKind.Variant:
			return RuntimeVariantType;
		case NavTypeKind.RecordRef:
			return RuntimeRecordRefType;
		case NavTypeKind.FieldRef:
			return RuntimeFieldRefType;
		case NavTypeKind.KeyRef:
			return RuntimeKeyRefType;
		case NavTypeKind.RecordId:
			return RuntimeRecordIdType;
		case NavTypeKind.TableFilter:
			return RuntimeTableFilterType;
		case NavTypeKind.Codeunit:
			return RuntimeCodeunitHandleType;
		case NavTypeKind.Record:
			if (!asArrayType)
			{
				return RuntimeRecordHandleType;
			}
			return NavRecordHandleType;
		case NavTypeKind.Page:
			return RuntimePageHandleType;
		case NavTypeKind.Report:
			return RuntimeReportHandleType;
		case NavTypeKind.Query:
			return RuntimeQueryHandleType;
		case NavTypeKind.XmlPort:
			return RuntimeXmlPortHandleType;
		case NavTypeKind.ControlAddIn:
			return NavDotNetType;
		case NavTypeKind.Notification:
			return RuntimeNotificationType;
		case NavTypeKind.InStream:
			return RuntimeInStreamType;
		case NavTypeKind.OutStream:
			return RuntimeOutStreamType;
		case NavTypeKind.Dialog:
			return RuntimeDialogType;
		case NavTypeKind.BigText:
			return RuntimeBigTextType;
		case NavTypeKind.File:
			return RuntimeFileType;
		case NavTypeKind.FilterPageBuilder:
			return RuntimeFilterPageBuilderType;
		case NavTypeKind.SessionSettings:
			return RuntimeSessionSettingsType;
		case NavTypeKind.TestRequestPage:
		case NavTypeKind.TestPage:
			return RuntimeTestPageHandleType;
		case NavTypeKind.String:
			return ClrStringType;
		case NavTypeKind.JsonToken:
			return RuntimeNavJsonTokenType;
		case NavTypeKind.JsonObject:
			return RuntimeNavJsonObjectType;
		case NavTypeKind.JsonArray:
			return RuntimeNavJsonArrayType;
		case NavTypeKind.JsonValue:
			return RuntimeNavJsonValueType;
		case NavTypeKind.HttpClient:
			return RuntimeNavHttpClientType;
		case NavTypeKind.HttpContent:
			return RuntimeNavHttpContentType;
		case NavTypeKind.HttpRequestMessage:
			return RuntimeNavHttpRequestMessageType;
		case NavTypeKind.HttpResponseMessage:
			return RuntimeNavHttpResponseMessageType;
		case NavTypeKind.HttpHeaders:
			return RuntimeNavHttpHeadersMessageType;
		case NavTypeKind.TextBuilder:
			return RuntimeNavTextBuilderType;
		case NavTypeKind.XmlAttribute:
			return RuntimeNavXmlAttributeType;
		case NavTypeKind.XmlAttributeCollection:
			return RuntimeNavXmlAttributeCollectionType;
		case NavTypeKind.XmlComment:
			return RuntimeNavXmlCommentType;
		case NavTypeKind.XmlCData:
			return RuntimeNavXmlCDataType;
		case NavTypeKind.XmlDeclaration:
			return RuntimeNavXmlDeclarationType;
		case NavTypeKind.XmlDocument:
			return RuntimeNavXmlDocumentType;
		case NavTypeKind.XmlDocumentType:
			return RuntimeNavXmlDocumentTypeType;
		case NavTypeKind.XmlElement:
			return RuntimeNavXmlElementType;
		case NavTypeKind.XmlNamespaceManager:
			return RuntimeNavXmlNamespaceManagerType;
		case NavTypeKind.XmlNameTable:
			return RuntimeNavXmlNameTableType;
		case NavTypeKind.XmlNode:
			return RuntimeNavXmlNodeType;
		case NavTypeKind.XmlNodeList:
			return RuntimeNavXmlNodeListType;
		case NavTypeKind.XmlProcessingInstruction:
			return RuntimeNavXmlProcessingInstructionType;
		case NavTypeKind.XmlText:
			return RuntimeNavXmlTextType;
		case NavTypeKind.XmlReadOptions:
			return RuntimeNavXmlReadOptionsType;
		case NavTypeKind.XmlWriteOptions:
			return RuntimeNavXmlWriteOptionsType;
		case NavTypeKind.Version:
			return RuntimeNavVersionType;
		case NavTypeKind.ModuleInfo:
			return RuntimeNavModuleInfo;
		case NavTypeKind.ModuleDependencyInfo:
			return RuntimeNavModuleDependencyInfo;
		case NavTypeKind.DotNet:
			return NavDotNetType;
		case NavTypeKind.WebServiceActionContext:
			return NavWebServiceActionContextType;
		case NavTypeKind.None:
			return RuntimeNavVoidType;
		default:
			return kind.RuntimeTypeSyntaxSafe_Generated(asArrayType);
		}
	}

	internal static TypeSyntax RuntimeTypeSyntax(this NavTypeKind kind, bool asArrayType = false)
	{
		return kind.RuntimeTypeSyntaxSafe(asArrayType) ?? throw ExceptionUtilities.UnexpectedValue(kind);
	}

	internal static string GetNavTypeEnumNameString(this NavTypeKind kind)
	{
		return kind switch
		{
			NavTypeKind.Blob => "BLOB", 
			NavTypeKind.RecordId => "RecordID", 
			NavTypeKind.Guid => "GUID", 
			NavTypeKind.Enum => "Option", 
			_ => kind.ToString(), 
		};
	}

	internal static ExpressionSyntax GetNavTypeEnumExpression(this NavTypeKind kind)
	{
		return CodeGenHelper.MemberAccessExpression(NavTypeEnumNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(kind.GetNavTypeEnumNameString()));
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	internal static bool IsNavValueType(this NavTypeKind kind)
	{
		switch (kind)
		{
		case NavTypeKind.OemText:
		case NavTypeKind.TestAction:
		case NavTypeKind.TestField:
		case NavTypeKind.TestFilterField:
		case NavTypeKind.TestPart:
		case NavTypeKind.TestFilter:
		case NavTypeKind.Dialog:
		case NavTypeKind.TestRequestPage:
		case NavTypeKind.RecordRef:
		case NavTypeKind.FieldRef:
		case NavTypeKind.KeyRef:
		case NavTypeKind.InStream:
		case NavTypeKind.OutStream:
		case NavTypeKind.Variant:
		case NavTypeKind.DotNet:
		case NavTypeKind.TestPage:
		case NavTypeKind.File:
		case NavTypeKind.FilterPageBuilder:
		case NavTypeKind.SessionSettings:
		case NavTypeKind.HttpClient:
		case NavTypeKind.HttpRequestMessage:
		case NavTypeKind.HttpResponseMessage:
		case NavTypeKind.HttpContent:
		case NavTypeKind.Record:
		case NavTypeKind.Codeunit:
		case NavTypeKind.Page:
		case NavTypeKind.Report:
		case NavTypeKind.XmlPort:
		case NavTypeKind.Query:
		case NavTypeKind.ControlAddIn:
		case NavTypeKind.List:
		case NavTypeKind.Dictionary:
		case NavTypeKind.WebServiceActionContext:
		case NavTypeKind.Notification:
		case NavTypeKind.JsonToken:
		case NavTypeKind.JsonObject:
		case NavTypeKind.JsonArray:
		case NavTypeKind.JsonValue:
		case NavTypeKind.HttpHeaders:
		case NavTypeKind.TextBuilder:
		case NavTypeKind.XmlAttribute:
		case NavTypeKind.XmlAttributeCollection:
		case NavTypeKind.XmlComment:
		case NavTypeKind.XmlCData:
		case NavTypeKind.XmlDeclaration:
		case NavTypeKind.XmlDocument:
		case NavTypeKind.XmlDocumentType:
		case NavTypeKind.XmlElement:
		case NavTypeKind.XmlNamespaceManager:
		case NavTypeKind.XmlNameTable:
		case NavTypeKind.XmlNode:
		case NavTypeKind.XmlNodeList:
		case NavTypeKind.XmlProcessingInstruction:
		case NavTypeKind.XmlText:
		case NavTypeKind.XmlReadOptions:
		case NavTypeKind.XmlWriteOptions:
		case NavTypeKind.Version:
		case NavTypeKind.ModuleInfo:
		case NavTypeKind.ModuleDependencyInfo:
		case NavTypeKind.ErrorInfo:
		case NavTypeKind.TextConst:
		case NavTypeKind.Label:
		case NavTypeKind.BigText:
		case NavTypeKind.Blob:
		case NavTypeKind.TableFilter:
		case NavTypeKind.Media:
		case NavTypeKind.MediaSet:
		case NavTypeKind.DateFormula:
		case NavTypeKind.RecordId:
		case NavTypeKind.Option:
		case NavTypeKind.DateTime:
		case NavTypeKind.Time:
		case NavTypeKind.Date:
		case NavTypeKind.Duration:
		case NavTypeKind.Enum:
		case NavTypeKind.Text:
		case NavTypeKind.Code:
			return true;
		default:
			return false;
		}
	}

	internal static TypeSyntax GetRuntimeTypeSyntax(this Symbol symbol)
	{
		switch ((int)symbol.Kind)
		{
		case 51:
			if (symbol is SourceOptionSymbol || symbol is ReferenceOptionSymbol)
			{
				return RuntimeIntegerType;
			}
			break;
		case 41:
		{
			MethodSymbol methodSymbol = symbol as MethodSymbol;
			if (methodSymbol?.ReturnType != null)
			{
				return GetRuntimeTypeSyntax(methodSymbol.ReturnType);
			}
			break;
		}
		}
		if ((object)symbol != null)
		{
			if (symbol is ISymbolWithType iSymbolWithType)
			{
				return iSymbolWithType.GetRuntimeTypeSyntax();
			}
			if (symbol is TypeSymbol typeSymbol)
			{
				return GetRuntimeTypeSyntax(typeSymbol);
			}
		}
		throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
	}

	internal static string GetCurrObjectName(this TypeSymbol container)
	{
		return container.NavTypeKind switch
		{
			NavTypeKind.Report => "CurrReport", 
			NavTypeKind.XmlPort => "currXMLport", 
			_ => throw ExceptionUtilities.UnexpectedValue(container.NavTypeKind), 
		};
	}

	internal static TypeSyntax GetRuntimeTypeSyntax(this ISymbolWithType iSymbolWithType, bool asGlobal = false)
	{
		return GetRuntimeTypeSyntax(iSymbolWithType.Type, asGlobal);
	}

	internal static TypeSyntax GetRuntimeTypeSyntax(this ReturnValueSymbol returnValue)
	{
		if (returnValue == null)
		{
			return ClrVoidType;
		}
		return GetRuntimeTypeSyntax(returnValue.Type);
	}

	internal static TypeSyntax GetRuntimeTypeSyntax(TypeSymbol typeSymbol, bool asGlobal = false)
	{
		if (typeSymbol.IsArray())
		{
			ArrayTypeSymbol arrayTypeSymbol = typeSymbol as ArrayTypeSymbol;
			DebugAssertHelper.Assert(arrayTypeSymbol != null);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(NavArrayTypeIdentifier, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeArgumentList().AddArguments(arrayTypeSymbol.ElementType.NavTypeKind.RuntimeTypeSyntax(asArrayType: true)));
		}
		if (typeSymbol.IsGenericType())
		{
			return GetGenericRuntimeTypeSyntax((GenericNavTypeSymbol)typeSymbol);
		}
		return typeSymbol.NavTypeKind.RuntimeTypeSyntax(asGlobal);
	}

	private static TypeSyntax GetGenericRuntimeTypeSyntax(GenericNavTypeSymbol typeSymbol)
	{
		TypeArgumentListSyntax typeArgumentListSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeArgumentList();
		foreach (TypeSymbol typeArgument in typeSymbol.TypeArguments)
		{
			typeArgumentListSyntax = typeArgumentListSyntax.AddArguments(GetRuntimeTypeSyntax(typeArgument));
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(GetGenericTypeIdentifier(typeSymbol), typeArgumentListSyntax);
	}

	private static SyntaxToken GetGenericTypeIdentifier(GenericNavTypeSymbol typeSymbol)
	{
		return typeSymbol.NavTypeKind switch
		{
			NavTypeKind.List => NavListTypeIdentifier, 
			NavTypeKind.Dictionary => NavDictionaryTypeIdentifier, 
			_ => throw ExceptionUtilities.UnexpectedValue(typeSymbol.NavTypeKind), 
		};
	}

	internal static InvocationExpressionSyntax GetObjectConversionInvocation(this ParameterSymbol parameter)
	{
		return parameter.Type.GetObjectConversionInvocation();
	}

	internal static InvocationExpressionSyntax GetObjectConversionInvocation(this TypeSymbol type)
	{
		return type.NavTypeKind switch
		{
			NavTypeKind.Boolean => ALCompilerObjectToBooleanCall, 
			NavTypeKind.Byte => ALCompilerObjectToByteCall, 
			NavTypeKind.Char => ALCompilerObjectToCharCall, 
			NavTypeKind.Integer => ALCompilerObjectToIntegerCall, 
			NavTypeKind.BigInteger => ALCompilerObjectToBigIntegerCall, 
			NavTypeKind.Decimal => ALCompilerObjectToDecimalCall, 
			NavTypeKind.Guid => ALCompilerObjectToGuidCall, 
			NavTypeKind.String => ALCompilerObjectToStringCall, 
			NavTypeKind.FilterPageBuilder => ALCompilerObjectToFilterPageBuilderCall, 
			NavTypeKind.Record => ALCompilerObjectToExactINavRecordHandleCall, 
			_ => GetObjectToTypeConversionInvocation_Generated(type.NavTypeKind), 
		};
	}

	internal static InvocationExpressionSyntax GetVariantConversionInvocation(this NavTypeKind kind)
	{
		switch (kind)
		{
		case NavTypeKind.RecordRef:
		case NavTypeKind.FieldRef:
		case NavTypeKind.KeyRef:
		case NavTypeKind.DotNet:
		case NavTypeKind.File:
		case NavTypeKind.FilterPageBuilder:
		case NavTypeKind.WebServiceActionContext:
		case NavTypeKind.JsonToken:
		case NavTypeKind.JsonObject:
		case NavTypeKind.JsonArray:
		case NavTypeKind.JsonValue:
		case NavTypeKind.TextBuilder:
		case NavTypeKind.XmlAttribute:
		case NavTypeKind.XmlAttributeCollection:
		case NavTypeKind.XmlComment:
		case NavTypeKind.XmlCData:
		case NavTypeKind.XmlDeclaration:
		case NavTypeKind.XmlDocument:
		case NavTypeKind.XmlDocumentType:
		case NavTypeKind.XmlElement:
		case NavTypeKind.XmlNamespaceManager:
		case NavTypeKind.XmlNameTable:
		case NavTypeKind.XmlNode:
		case NavTypeKind.XmlNodeList:
		case NavTypeKind.XmlProcessingInstruction:
		case NavTypeKind.XmlText:
		case NavTypeKind.XmlReadOptions:
		case NavTypeKind.XmlWriteOptions:
		case NavTypeKind.DataClassification:
		case NavTypeKind.BigText:
		case NavTypeKind.Blob:
		case NavTypeKind.DateFormula:
		case NavTypeKind.RecordId:
		case NavTypeKind.DateTime:
		case NavTypeKind.Time:
		case NavTypeKind.Date:
		case NavTypeKind.Duration:
		case NavTypeKind.Text:
		case NavTypeKind.Code:
		{
			GenericNameSyntax name2 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName("NavIndirectValueToNavValue").AddTypeArgumentListArguments(kind.RuntimeTypeSyntax());
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(ALCompilerClassNameSyntax, name2));
		}
		case NavTypeKind.String:
		{
			GenericNameSyntax name = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName("NavIndirectValueToNavValue").AddTypeArgumentListArguments(RuntimeTextType);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(ALCompilerClassNameSyntax, name));
		}
		case NavTypeKind.Action:
			return ALCompilerNavIndirectValueToFormResultCall;
		case NavTypeKind.BigInteger:
			return ALCompilerNavIndirectValueToInt64Call;
		case NavTypeKind.Boolean:
			return ALCompilerNavIndirectValueToBooleanCall;
		case NavTypeKind.Byte:
			return ALCompilerNavIndirectValueToByteCall;
		case NavTypeKind.Char:
			return ALCompilerNavIndirectValueToCharCall;
		case NavTypeKind.Codeunit:
			return ALCompilerNavIndirectValueToNavCodeunitHandleCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()));
		case NavTypeKind.Decimal:
			return ALCompilerNavIndirectValueToDecimalCall;
		case NavTypeKind.Guid:
			return ALCompilerNavIndirectValueToGuidCall;
		case NavTypeKind.InStream:
			return ALCompilerObjectToNavInStreamCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()));
		case NavTypeKind.Integer:
			return ALCompilerNavIndirectValueToInt32Call;
		case NavTypeKind.OutStream:
			return ALCompilerObjectToNavOutStreamCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()));
		case NavTypeKind.Record:
			return ALCompilerNavIndirectValueToINavRecordHandle;
		case NavTypeKind.Table:
			return ALCompilerNavIndirectValueToINavRecordHandle;
		case NavTypeKind.TransactionType:
			return ALCompilerNavIndirectValueToTransactionTypeCall;
		default:
			return null;
		}
	}

	internal static bool UseInstanceMethodToClear(this NavTypeKind kind)
	{
		switch (kind)
		{
		case NavTypeKind.Dialog:
		case NavTypeKind.TestRequestPage:
		case NavTypeKind.RecordRef:
		case NavTypeKind.FieldRef:
		case NavTypeKind.KeyRef:
		case NavTypeKind.InStream:
		case NavTypeKind.OutStream:
		case NavTypeKind.Variant:
		case NavTypeKind.DotNet:
		case NavTypeKind.TestPage:
		case NavTypeKind.File:
		case NavTypeKind.FilterPageBuilder:
		case NavTypeKind.SessionSettings:
		case NavTypeKind.HttpClient:
		case NavTypeKind.HttpRequestMessage:
		case NavTypeKind.HttpResponseMessage:
		case NavTypeKind.HttpContent:
		case NavTypeKind.ControlAddIn:
		case NavTypeKind.Notification:
		case NavTypeKind.ErrorInfo:
		case NavTypeKind.Blob:
			return true;
		default:
			return false;
		}
	}

	internal static bool UseStaticMethodToClear(this NavTypeKind kind)
	{
		switch (kind)
		{
		case NavTypeKind.Notification:
		case NavTypeKind.ErrorInfo:
		case NavTypeKind.BigText:
		case NavTypeKind.DateFormula:
		case NavTypeKind.RecordId:
		case NavTypeKind.DateTime:
		case NavTypeKind.Time:
		case NavTypeKind.Date:
		case NavTypeKind.Duration:
			return true;
		default:
			return false;
		}
	}

	internal static bool SupportsNativeComparison(this NavTypeKind kind)
	{
		if ((uint)(kind - 3014659) <= 1u || (uint)(kind - 12451845) <= 2u)
		{
			return true;
		}
		if (kind.IsClrOption())
		{
			return true;
		}
		return false;
	}
}
