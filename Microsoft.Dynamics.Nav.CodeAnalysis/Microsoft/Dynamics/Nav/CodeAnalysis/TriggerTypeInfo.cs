using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class TriggerTypeInfo
{
	public TriggerTypeKind Kind { get; }

	internal string Name { get; }

	internal string SyntaxOwner { get; }

	internal TypeSymbol ReturnType { get; }

	internal ExpressionSyntax DefaultReturnValueEmittedSyntax { get; }

	internal ImmutableArray<TriggerParameter> Parameters { get; }

	internal SyntaxAnnotation TriggerAnnotation { get; }

	internal ImmutableArray<DependentProperty> DependentParentProperties { get; }

	internal string LegacyName { get; }

	internal VersionCompatibility Compatibility { get; }

	internal TriggerTypeInfo(string syntaxOwner, string name, string legacyName, TypeSymbol returnType, string defaultReturnValue, ImmutableArray<DependentProperty> dependentParentProperties, ImmutableArray<TriggerParameter> parameters, VersionCompatibility compatibility = null)
	{
		Name = name;
		LegacyName = legacyName ?? name;
		SyntaxOwner = syntaxOwner;
		ReturnType = returnType;
		DefaultReturnValueEmittedSyntax = ((defaultReturnValue != null) ? Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression(defaultReturnValue) : null);
		Kind = (TriggerTypeKind)Enum.Parse(typeof(TriggerTypeKind), SyntaxOwner + Name);
		Parameters = parameters;
		TriggerAnnotation = new SyntaxAnnotation(AnnotationKind.TriggerType, this);
		DependentParentProperties = dependentParentProperties;
		Compatibility = compatibility;
	}

	internal string GetSignatureString(bool appendKeyword = true)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		if (appendKeyword)
		{
			builder.Append(SyntaxKind.TriggerKeyword.GetText());
			builder.Append(" ");
		}
		builder.Append(Name);
		builder.Append(SyntaxKind.OpenParenToken.GetText());
		for (int i = 0; i < Parameters.Length; i++)
		{
			TriggerParameter triggerParameter = Parameters[i];
			if (triggerParameter.IsVar)
			{
				builder.Append(SyntaxKind.VarKeyword.GetText());
				builder.Append(" ");
			}
			if (i > 0)
			{
				builder.Append(SyntaxKind.SemicolonToken.GetText());
				builder.Append(" ");
			}
			builder.Append(triggerParameter.Name);
			builder.Append(SyntaxKind.ColonToken.GetText());
			builder.Append(" ");
			builder.Append(triggerParameter.Type.ToDisplayString());
		}
		builder.Append(SyntaxKind.CloseParenToken.GetText());
		if (!ReturnType.IsNoneType())
		{
			builder.Append(SyntaxKind.ColonToken.GetText());
			builder.Append(" ");
			builder.Append(ReturnType.Name);
		}
		return instance.ToStringAndFree();
	}
}
