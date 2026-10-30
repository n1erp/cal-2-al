using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceFieldSymbolWithSyntaxReference : SourceFieldSymbol
{
	private readonly string _name;

	private readonly Location _location;

	private readonly SyntaxReference _syntaxReference;

	private string _lazyDocComment;

	private ConstantValue _lazyConstantEarlyDecodingValue = Microsoft.CodeAnalysis.ConstantValue.Unset;

	private ConstantValue _lazyConstantValue = Microsoft.CodeAnalysis.ConstantValue.Unset;

	public SyntaxTree SyntaxTree => _syntaxReference.SyntaxTree;

	public CSharpSyntaxNode SyntaxNode => (CSharpSyntaxNode)_syntaxReference.GetSyntax();

	public sealed override string Name => _name;

	public sealed override ImmutableArray<Location> Locations => ImmutableArray.Create(_location);

	internal sealed override Location ErrorLocation => _location;

	public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(_syntaxReference);

	protected SourceFieldSymbolWithSyntaxReference(SourceMemberContainerTypeSymbol containingType, string name, SyntaxReference syntax, Location location)
		: base(containingType)
	{
		_name = name;
		_syntaxReference = syntax;
		_location = location;
	}

	internal override LexicalSortKey GetLexicalSortKey()
	{
		return new LexicalSortKey(_location, DeclaringCompilation);
	}

	public sealed override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(this, expandIncludes, ref _lazyDocComment);
	}

	internal sealed override ConstantValue GetConstantValue(ConstantFieldsInProgress inProgress, bool earlyDecodingWellKnownAttributes)
	{
		ConstantValue lazyConstantValue = GetLazyConstantValue(earlyDecodingWellKnownAttributes);
		if (lazyConstantValue != Microsoft.CodeAnalysis.ConstantValue.Unset)
		{
			return lazyConstantValue;
		}
		if (!inProgress.IsEmpty)
		{
			inProgress.AddDependency(this);
			return Microsoft.CodeAnalysis.ConstantValue.Unset;
		}
		ArrayBuilder<ConstantEvaluationHelpers.FieldInfo> instance = ArrayBuilder<ConstantEvaluationHelpers.FieldInfo>.GetInstance();
		this.OrderAllDependencies(instance, earlyDecodingWellKnownAttributes);
		foreach (ConstantEvaluationHelpers.FieldInfo item in instance)
		{
			item.Field.BindConstantValueIfNecessary(earlyDecodingWellKnownAttributes, item.StartsCycle);
		}
		instance.Free();
		return GetLazyConstantValue(earlyDecodingWellKnownAttributes);
	}

	internal ImmutableHashSet<SourceFieldSymbolWithSyntaxReference> GetConstantValueDependencies(bool earlyDecodingWellKnownAttributes)
	{
		ConstantValue lazyConstantValue = GetLazyConstantValue(earlyDecodingWellKnownAttributes);
		if (lazyConstantValue != Microsoft.CodeAnalysis.ConstantValue.Unset)
		{
			return ImmutableHashSet<SourceFieldSymbolWithSyntaxReference>.Empty;
		}
		PooledHashSet<SourceFieldSymbolWithSyntaxReference> instance = PooledHashSet<SourceFieldSymbolWithSyntaxReference>.GetInstance();
		DiagnosticBag instance2 = DiagnosticBag.GetInstance();
		lazyConstantValue = MakeConstantValue(instance, earlyDecodingWellKnownAttributes, instance2);
		ImmutableHashSet<SourceFieldSymbolWithSyntaxReference> result;
		if (instance.Count == 0 && lazyConstantValue != null && !lazyConstantValue.IsBad && lazyConstantValue != Microsoft.CodeAnalysis.ConstantValue.Unset && instance2.IsEmptyWithoutResolution)
		{
			SetLazyConstantValue(lazyConstantValue, earlyDecodingWellKnownAttributes, instance2, startsCycle: false);
			result = ImmutableHashSet<SourceFieldSymbolWithSyntaxReference>.Empty;
		}
		else
		{
			result = ImmutableHashSet<SourceFieldSymbolWithSyntaxReference>.Empty.Union(instance);
		}
		instance2.Free();
		instance.Free();
		return result;
	}

	private void BindConstantValueIfNecessary(bool earlyDecodingWellKnownAttributes, bool startsCycle)
	{
		if (!(GetLazyConstantValue(earlyDecodingWellKnownAttributes) != Microsoft.CodeAnalysis.ConstantValue.Unset))
		{
			PooledHashSet<SourceFieldSymbolWithSyntaxReference> instance = PooledHashSet<SourceFieldSymbolWithSyntaxReference>.GetInstance();
			DiagnosticBag instance2 = DiagnosticBag.GetInstance();
			if (startsCycle)
			{
				instance2.Add(ErrorCode.ERR_CircConstValue, _location, this);
			}
			ConstantValue value = MakeConstantValue(instance, earlyDecodingWellKnownAttributes, instance2);
			SetLazyConstantValue(value, earlyDecodingWellKnownAttributes, instance2, startsCycle);
			instance2.Free();
			instance.Free();
		}
	}

	private ConstantValue GetLazyConstantValue(bool earlyDecodingWellKnownAttributes)
	{
		if (!earlyDecodingWellKnownAttributes)
		{
			return _lazyConstantValue;
		}
		return _lazyConstantEarlyDecodingValue;
	}

	private void SetLazyConstantValue(ConstantValue value, bool earlyDecodingWellKnownAttributes, DiagnosticBag diagnostics, bool startsCycle)
	{
		if (earlyDecodingWellKnownAttributes)
		{
			Interlocked.CompareExchange(ref _lazyConstantEarlyDecodingValue, value, Microsoft.CodeAnalysis.ConstantValue.Unset);
		}
		else if (Interlocked.CompareExchange(ref _lazyConstantValue, value, Microsoft.CodeAnalysis.ConstantValue.Unset) == Microsoft.CodeAnalysis.ConstantValue.Unset)
		{
			AddDeclarationDiagnostics(diagnostics);
			DeclaringCompilation.SymbolDeclaredEvent(this);
			state.NotePartComplete(CompletionPart.TypeMembers);
		}
	}

	protected abstract ConstantValue MakeConstantValue(HashSet<SourceFieldSymbolWithSyntaxReference> dependencies, bool earlyDecodingWellKnownAttributes, DiagnosticBag diagnostics);
}
