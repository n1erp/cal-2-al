using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CodeGen;

internal class PermissionSetAttributeWithFileReference : ICustomAttribute
{
	private struct HexPropertyMetadataNamedArgument(ITypeReference type, IMetadataExpression value) : IMetadataNamedArgument, IMetadataExpression
	{
		private readonly ITypeReference _type = type;

		private readonly IMetadataExpression _value = value;

		public string ArgumentName => "Hex";

		public IMetadataExpression ArgumentValue => _value;

		public bool IsField => false;

		ITypeReference IMetadataExpression.Type => _type;

		void IMetadataExpression.Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	private readonly ICustomAttribute _sourceAttribute;

	private readonly string _resolvedPermissionSetFilePath;

	internal const string FilePropertyName = "File";

	internal const string HexPropertyName = "Hex";

	public int ArgumentCount => _sourceAttribute.ArgumentCount;

	public ushort NamedArgumentCount => 1;

	public bool AllowMultiple => _sourceAttribute.AllowMultiple;

	public PermissionSetAttributeWithFileReference(ICustomAttribute sourceAttribute, string resolvedPermissionSetFilePath)
	{
		_sourceAttribute = sourceAttribute;
		_resolvedPermissionSetFilePath = resolvedPermissionSetFilePath;
	}

	public ImmutableArray<IMetadataExpression> GetArguments(EmitContext context)
	{
		return _sourceAttribute.GetArguments(context);
	}

	public IMethodReference Constructor(EmitContext context, bool reportDiagnostics)
	{
		return _sourceAttribute.Constructor(context, reportDiagnostics);
	}

	public ImmutableArray<IMetadataNamedArgument> GetNamedArguments(EmitContext context)
	{
		ITypeReference platformType = context.Module.GetPlatformType(PlatformType.SystemString, context);
		XmlReferenceResolver xmlReferenceResolver = context.Module.CommonCompilation.Options.XmlReferenceResolver;
		string value;
		try
		{
			using Stream stream = xmlReferenceResolver.OpenReadChecked(_resolvedPermissionSetFilePath);
			value = ConvertToHex(stream);
		}
		catch (IOException ex)
		{
			throw new PermissionSetFileReadException(ex.Message, _resolvedPermissionSetFilePath);
		}
		return ImmutableArray.Create((IMetadataNamedArgument)new HexPropertyMetadataNamedArgument(platformType, new MetadataConstant(platformType, value)));
	}

	internal static string ConvertToHex(Stream stream)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		int num;
		while ((num = stream.ReadByte()) >= 0)
		{
			builder.Append(ConvertHexToChar((num >> 4) & 0xF));
			builder.Append(ConvertHexToChar(num & 0xF));
		}
		return instance.ToStringAndFree();
	}

	private static char ConvertHexToChar(int b)
	{
		return (char)((b < 10) ? (48 + b) : (97 + b - 10));
	}

	public ITypeReference GetType(EmitContext context)
	{
		return _sourceAttribute.GetType(context);
	}
}
