using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FluentRpc.Common;

public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;
    private readonly Exception? _exception;

    public Result() => throw new InvalidOperationException();
    private Result(T value) => (_value, State) = (value, ResultState.Ok);
    private Result(Error error) => (_error, State) = (error, ResultState.Error);
    private Result(Exception exception) => (_exception, State) = (exception, ResultState.Failure);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Ok(T value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Error(Error error) => new(error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Failure(Exception exception) => new(exception);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TOther> Fail<TOther>() => State switch
    {
        ResultState.Ok => new(Errors.FailedFailure<T, TOther>()),
        ResultState.Error => new(_error!.Value),
        ResultState.Failure => new(_exception!),
        _ => throw new NotSupportedException($"state {State} not supported")
    };

    public ResultState State { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOk([NotNullWhen(true)] out T? value)
    {
        if (State == ResultState.Ok)
        {
            value = _value!;
            return true;
        }

        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsError([NotNullWhen(true)] out Error? value)
    {
        if (State == ResultState.Error)
        {
            value = _error!;
            return true;
        }

        value = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFailure([NotNullWhen(true)] out Exception? value)
    {
        if (State == ResultState.Failure)
        {
            value = _exception!;
            return true;
        }

        value = null;
        return false;
    }
}

public readonly struct Result
{
    private readonly Error? _error;
    private readonly Exception? _exception;

    public Result() => State = (ResultState.Ok);
    private Result(Error error) => (_error, State) = (error, ResultState.Error);
    private Result(Exception exception) => (_exception, State) = (exception, ResultState.Failure);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Ok() => new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Error(Error error) => new(error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Failure(Exception exception) => new(exception);

    public ResultState State { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOk() => State == ResultState.Ok;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsError([NotNullWhen(true)] out Error? value)
    {
        if (State == ResultState.Ok)
        {
            value = _error!;
            return true;
        }

        value = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFailure([NotNullWhen(true)] out Exception? value)
    {
        if (State == ResultState.Ok)
        {
            value = _exception!;
            return true;
        }

        value = null;
        return false;
    }
}