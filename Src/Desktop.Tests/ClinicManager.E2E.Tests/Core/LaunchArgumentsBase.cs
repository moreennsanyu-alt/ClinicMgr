using System.Runtime.CompilerServices;

namespace ClinicManager.E2E.Tests.Core;

public abstract record LaunchArgumentsBase
{
    protected string? CreateArg(object? value, [CallerArgumentExpression(nameof(value))] string propertyName = null!) => value is null ? null : $"--{propertyName}=\"{value}\"";

    public abstract string ToArguments();
}
