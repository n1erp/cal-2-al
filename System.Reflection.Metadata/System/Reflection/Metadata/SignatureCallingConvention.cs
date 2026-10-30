namespace System.Reflection.Metadata;

public enum SignatureCallingConvention : byte
{
	Default,
	CDecl,
	StdCall,
	ThisCall,
	FastCall,
	VarArgs
}
