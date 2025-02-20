using UnionStruct.Unions;

namespace FluentRpc.Common;

[Union]
public readonly partial struct Result<T>
{
    [UnionPart(AddMap = true, State = "Ok")]
    private readonly T? _value;

    [UnionPart] private readonly Error? _error;
    [UnionPart(State = "Failure")] private readonly Exception? _exception;
}

public static class ResExtensions
{
    public static Result<TOther> Fail<T, TOther>(this Result<T> src) => src.State switch
    {
        _ when src.IsOk(out _) => Result<TOther>.Error(Errors.FailedFailure<T, TOther>()),
        _ when src.IsError(out var error) => Result<TOther>.Error(error.Value),
        _ when src.IsFailure(out var exception) => Result<TOther>.Failure(exception),
        _ => throw new ArgumentOutOfRangeException()
    };
}

[Union("Ok")]
public readonly partial struct UnitResult
{
    [UnionPart] private readonly Error? _error;
    [UnionPart(State = "Failure")] private readonly Exception? _exception;
}