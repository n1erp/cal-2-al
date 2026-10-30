using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class AttributeTypeInfo
{
	private readonly AttributeTagKind[] tags;

	public string Name { get; }

	public string MetadataName { get; }

	public string EmitConstantExpressionArgument { get; }

	public IAttributeEmitter Emitter { get; }

	public bool NoEmit { get; }

	public AttributeCategory Category { get; }

	public AttributeKind Kind { get; }

	public CodeunitSubtypeKind? RequiredCodeunitSubtype { get; }

	public AttributeAllowedOn AllowedOn { get; }

	public SymbolKind? AllowedIn { get; }

	public ImmutableArray<AttributeParameter> Parameters { get; }

	public ImmutableArray<AttributeRequiredMethodSignature> RequiredMethodSignature { get; }

	public SyntaxAnnotation AttributeAnnotation { get; }

	public bool IsObsolete { get; }

	public SymbolCompilationScope? CompilationScope { get; }

	public IAttributeValidator Validator { get; }

	public int MinParameterCount { get; }

	public int MaxParameterCount { get; }

	public VersionCompatibility Compatibility { get; }

	public bool HasRequiredSignature { get; }

	public AttributeTypeInfo(string name, string metadataName, AttributeCategory category, AttributeKind kind, AttributeTagKind[] tags, AttributeAllowedOn allowedOn, SymbolKind? allowedIn, CodeunitSubtypeKind? requiredCodeunitSubtype, string emitConstantExpressionArgument, bool noEmit, bool isObsolete, SymbolCompilationScope? compilationScope, IAttributeEmitter emitter, IAttributeValidator validator, AttributeParameter[] parameters, AttributeRequiredMethodSignature[] requiredSignatures, VersionCompatibility compatibility)
	{
		Name = name;
		MetadataName = metadataName;
		Category = category;
		Kind = kind;
		this.tags = tags;
		AllowedOn = allowedOn;
		AllowedIn = allowedIn;
		Parameters = parameters.AsImmutableOrEmpty();
		RequiredCodeunitSubtype = requiredCodeunitSubtype;
		EmitConstantExpressionArgument = emitConstantExpressionArgument;
		NoEmit = noEmit;
		Emitter = emitter ?? BaseAttributeEmitter.Instance;
		Validator = validator ?? BaseAttributeValidator.Instance;
		RequiredMethodSignature = requiredSignatures.AsImmutableOrNull();
		AttributeAnnotation = new SyntaxAnnotation(AnnotationKind.AttributeType, this);
		IsObsolete = isObsolete;
		CompilationScope = compilationScope;
		MaxParameterCount = Parameters.Length;
		MinParameterCount = ComputeMinimumParameterCount(Parameters);
		Compatibility = compatibility;
		HasRequiredSignature = RequiredMethodSignature != null && RequiredMethodSignature.Length != 0;
	}

	internal bool ContainsTag(AttributeTagKind kind)
	{
		return tags?.Contains(kind) ?? false;
	}

	internal static int ComputeMinimumParameterCount(ImmutableArray<AttributeParameter> parameters)
	{
		int num = 0;
		foreach (AttributeParameter item in parameters)
		{
			if (!item.IsOptional)
			{
				num++;
			}
		}
		return num;
	}

	internal bool IsAllowedOn(AttributeAllowedOn allowedOn)
	{
		return (AllowedOn & allowedOn) != 0;
	}

	internal string GetIsAllowedOnText()
	{
		return AllowedOn.ToString().ToLowerInvariant();
	}
}
