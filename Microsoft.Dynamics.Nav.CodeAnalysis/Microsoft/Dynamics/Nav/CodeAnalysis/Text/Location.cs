using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public abstract class Location
{
	public abstract LocationKind Kind { get; }

	public bool IsInSource => SourceTree != null;

	public virtual SyntaxTree SourceTree => null;

	public virtual TextSpan SourceSpan => default;

	public static Location None => NoLocation.Singleton;

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public virtual FileLinePositionSpan GetLineSpan()
	{
		return default;
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public virtual FileLinePositionSpan GetMappedLineSpan()
	{
		return default;
	}

	public abstract override bool Equals(object obj);

	public abstract override int GetHashCode();

	public override string ToString()
	{
		string text = Kind.ToString();
		FileLinePositionSpan lineSpan = GetLineSpan();
		if (lineSpan.Path != null)
		{
			text = text + "(" + lineSpan.Path + "@" + (lineSpan.StartLinePosition.Line + 1) + ":" + (lineSpan.StartLinePosition.Character + 1) + ")";
		}
		return text;
	}

	public static bool operator ==(Location left, Location right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(Location left, Location right)
	{
		return !(left == right);
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	protected virtual string GetDebuggerDisplay()
	{
		string text = GetType().Name;
		FileLinePositionSpan lineSpan = GetLineSpan();
		if (lineSpan.Path != null)
		{
			text = text + "(" + lineSpan.Path + "@" + (lineSpan.StartLinePosition.Line + 1) + ":" + (lineSpan.StartLinePosition.Character + 1) + ")";
		}
		return text;
	}

	public static Location Create(SyntaxTree syntaxTree, TextSpan textSpan)
	{
		if (syntaxTree == null)
		{
			throw new ArgumentNullException("syntaxTree");
		}
		return new SourceLocation(syntaxTree, textSpan);
	}

	public static Location Create(string filePath, TextSpan textSpan, LinePositionSpan lineSpan)
	{
		if (filePath == null)
		{
			throw new ArgumentNullException("filePath");
		}
		return new ExternalFileLocation(filePath, textSpan, lineSpan);
	}
}
