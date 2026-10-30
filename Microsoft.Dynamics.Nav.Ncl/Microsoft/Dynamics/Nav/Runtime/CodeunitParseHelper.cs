using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class CodeunitParseHelper
{
	private static readonly string NavCodeUnitOptionsAttributeFullName = typeof(NavCodeunitOptionsAttribute).Name;

	private const string NavCodeUnitOptionsAttributeShortName = "NavCodeunitOptions";

	private static readonly string NavEventSubscriberAttributeFullName = typeof(NavEventSubscriberAttribute).Name;

	private const string NavEventSubscriberAttributeShortName = "NavEventSubscriber";

	internal static bool TryParseSubType(int objectNumber, out CodeunitSubType? subType, TextReader codeContentProvider = null)
	{
		int? tableId;
		NavCodeunitOptions? codeUnitOptions;
		return TryParseCodeunitOptionsAttribute(objectNumber, out subType, out tableId, out codeUnitOptions, codeContentProvider);
	}

	internal static bool TryParseCodeunitOptionsAttribute(int objectNumber, out CodeunitSubType? subType, out int? tableId, out NavCodeunitOptions? codeUnitOptions, TextReader codeContentProvider = null)
	{
		if (codeContentProvider == null)
		{
			NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
			NavAppRuntimeMetadata objectOwner = navAppGroup.GetObjectOwner(ObjectType.CodeUnit, objectNumber);
			if (objectOwner != null)
			{
				ApplicationObjectId objectId = new ApplicationObjectId(ObjectType.CodeUnit, objectNumber);
				NavAppObjectMetadataBlob objectMetadataBlob = NavEnvironment.Instance.NavAppMetadataRetriever.GetObjectMetadataBlob(objectId, objectOwner.PackageId, (NavAppObjectMetadataBlobType)1);
				return TryParseCodeunitDeclarationSyntaxForCodeunitOptionsAttribute(objectMetadataBlob.Content, out subType, out tableId, out codeUnitOptions);
			}
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000071);
			navRecord.SetFieldValue(3, NavInteger.Create(5));
			navRecord.SetFieldValue(6, NavInteger.Create(objectNumber));
			navRecord.CalcFields(DataError.ThrowError, new int[1] { 15 });
			using StreamReader reader = new StreamReader(((NavBLOB)navRecord.GetFieldValue(15)).GetStream());
			return TryParseCodeunitDeclarationSyntaxForCodeunitOptionsAttribute(reader, out subType, out tableId, out codeUnitOptions);
		}
		return TryParseCodeunitDeclarationSyntaxForCodeunitOptionsAttribute(codeContentProvider, out subType, out tableId, out codeUnitOptions);
	}

	internal static bool HasEventsubscriber(string code)
	{
		if (string.IsNullOrEmpty(code))
		{
			return false;
		}
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
		CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
		return HasEventSubscriber(compilationUnit);
	}

	internal static bool HasEventsubscriber(byte[] astBytes)
	{
		if (astBytes == null || astBytes.Length == 0)
		{
			return false;
		}
		using MemoryStream stream = MemoryStreamManager.GetStream(astBytes);
		CompilationUnitSyntax compilationUnit = CSharpSyntaxNode.DeserializeFrom(stream) as CompilationUnitSyntax;
		return HasEventSubscriber(compilationUnit);
	}

	private static bool HasEventSubscriber(CompilationUnitSyntax compilationUnit)
	{
		foreach (MemberDeclarationSyntax member in compilationUnit.Members)
		{
			if (member.Kind() != SyntaxKind.NamespaceDeclaration)
			{
				continue;
			}
			foreach (MemberDeclarationSyntax member2 in ((NamespaceDeclarationSyntax)member).Members)
			{
				if (member2.Kind() != SyntaxKind.ClassDeclaration)
				{
					continue;
				}
				foreach (MemberDeclarationSyntax member3 in ((ClassDeclarationSyntax)member2).Members)
				{
					if (member3.Kind() != SyntaxKind.MethodDeclaration)
					{
						continue;
					}
					MethodDeclarationSyntax methodDeclarationSyntax = (MethodDeclarationSyntax)member3;
					foreach (AttributeListSyntax attributeList in methodDeclarationSyntax.AttributeLists)
					{
						foreach (AttributeSyntax attribute in attributeList.Attributes)
						{
							string text = attribute.Name.ToString();
							if (text.Equals(NavEventSubscriberAttributeFullName, StringComparison.Ordinal) || text.Equals("NavEventSubscriber", StringComparison.Ordinal))
							{
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	private static bool TryParseCodeunitDeclarationSyntaxForCodeunitOptionsAttribute(CompilationUnitSyntax compilationUnit, out CodeunitSubType? subType, out int? tableId, out NavCodeunitOptions? codeUnitOptions)
	{
		subType = null;
		tableId = null;
		codeUnitOptions = null;
		foreach (MemberDeclarationSyntax member in compilationUnit.Members)
		{
			if (member.Kind() != SyntaxKind.NamespaceDeclaration)
			{
				continue;
			}
			foreach (MemberDeclarationSyntax member2 in ((NamespaceDeclarationSyntax)member).Members)
			{
				if (member2.Kind() == SyntaxKind.ClassDeclaration)
				{
					return TryParseClassDeclarationSyntaxForCodeunitOptionsAttribute((ClassDeclarationSyntax)member2, out subType, out tableId, out codeUnitOptions);
				}
			}
		}
		return false;
	}

	private static bool TryParseCodeunitDeclarationSyntaxForCodeunitOptionsAttribute(string code, out CodeunitSubType? subType, out int? tableId, out NavCodeunitOptions? codeUnitOptions)
	{
		subType = null;
		tableId = null;
		codeUnitOptions = null;
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
		CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
		return TryParseCodeunitDeclarationSyntaxForCodeunitOptionsAttribute(compilationUnit, out subType, out tableId, out codeUnitOptions);
	}

	private static bool TryParseCodeunitDeclarationSyntaxForCodeunitOptionsAttribute(TextReader reader, out CodeunitSubType? subType, out int? tableId, out NavCodeunitOptions? codeUnitOptions)
	{
		return TryParseCodeunitDeclarationSyntaxForCodeunitOptionsAttribute(reader.ReadToEnd(), out subType, out tableId, out codeUnitOptions);
	}

	private static bool TryParseClassDeclarationSyntaxForCodeunitOptionsAttribute(ClassDeclarationSyntax classDeclarationSyntax, out CodeunitSubType? subType, out int? tableId, out NavCodeunitOptions? codeUnitOptions)
	{
		subType = null;
		tableId = null;
		codeUnitOptions = null;
		foreach (AttributeListSyntax attributeList in classDeclarationSyntax.AttributeLists)
		{
			foreach (AttributeSyntax attribute in attributeList.Attributes)
			{
				string text = attribute.Name.ToString();
				if (!text.Equals("NavCodeunitOptions", StringComparison.Ordinal) && !text.Equals(NavCodeUnitOptionsAttributeFullName, StringComparison.Ordinal))
				{
					continue;
				}
				_ = attribute.ArgumentList.Arguments;
				if (attribute.ArgumentList.Arguments.Count != 3)
				{
					return false;
				}
				for (int i = 0; i < attribute.ArgumentList.Arguments.Count; i++)
				{
					AttributeArgumentSyntax attributeArgumentSyntax = attribute.ArgumentList.Arguments[i];
					if (attributeArgumentSyntax.Expression == null)
					{
						return false;
					}
					if (TryGetNumericalSyntaxValue(attributeArgumentSyntax.Expression, out var value))
					{
						switch (i)
						{
						case 0:
							codeUnitOptions = (NavCodeunitOptions)value;
							break;
						case 1:
							tableId = value;
							break;
						case 2:
							subType = (CodeunitSubType)value;
							break;
						}
					}
					switch (i)
					{
					case 0:
						if (!codeUnitOptions.HasValue)
						{
							TryGetEnumValue(attributeArgumentSyntax.Expression, out codeUnitOptions);
						}
						break;
					case 2:
						if (!subType.HasValue)
						{
							TryGetEnumValue(attributeArgumentSyntax.Expression, out subType);
						}
						break;
					}
				}
				if (subType.HasValue && codeUnitOptions.HasValue)
				{
					return tableId.HasValue;
				}
				return false;
			}
		}
		return false;
	}

	private static bool TryGetNumericalSyntaxValue(ExpressionSyntax expression, out int value)
	{
		value = 0;
		if (expression.Kind() != SyntaxKind.NumericLiteralExpression)
		{
			return false;
		}
		LiteralExpressionSyntax literalExpressionSyntax = (LiteralExpressionSyntax)expression;
		value = (int)literalExpressionSyntax.Token.Value;
		return true;
	}

	private static bool TryGetEnumValue<T>(ExpressionSyntax expression, out T? value) where T : struct
	{
		value = null;
		if (expression.Kind() != SyntaxKind.SimpleMemberAccessExpression)
		{
			return false;
		}
		MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)expression;
		if (Enum.TryParse<T>(memberAccessExpressionSyntax.Name.Identifier.ValueText, ignoreCase: true, out var result))
		{
			value = result;
		}
		return value.HasValue;
	}
}
