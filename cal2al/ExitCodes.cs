namespace Cal2AL.CLI;

/// <summary>Process exit codes. Documented in docs/command-line-reference.md.</summary>
public static class ExitCodes
{
    /// <summary>The command completed and no diagnostic reached the --fail-on level.</summary>
    public const int Success = 0;

    /// <summary>The command completed, but diagnostics at or above the --fail-on level were reported.</summary>
    public const int DiagnosticsAtFailLevel = 1;

    /// <summary>The command line was invalid.</summary>
    public const int InvalidArguments = 2;

    /// <summary>The source does not exist or contains no C/AL objects.</summary>
    public const int NoInput = 3;

    /// <summary>An unexpected error stopped the command.</summary>
    public const int InternalError = 4;

    /// <summary>The run was cancelled (Ctrl+C).</summary>
    public const int Cancelled = 130;
}
