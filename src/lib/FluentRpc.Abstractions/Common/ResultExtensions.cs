using System.Runtime.CompilerServices;

namespace FluentRpc.Common;

public static class ResultExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Result<TOut>> MapOk<TIn, TOut>(this Result<TIn> src, Func<TIn, Task<TOut>> value)
        => src.IsOk(out var ok)
            ? value(ok).Transform(Result<TOut>.Ok, Result<TOut>.Failure)
            : Result<TOut>.Error(Errors.FailedMapping<TIn, TOut>()).ToTask();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TOut> MapOk<TIn, TOut>(this Result<TIn> src, Func<TIn, TOut> value)
        => src.IsOk(out var ok) ? Result<TOut>.Ok(value(ok)) : Result<TOut>.Error(Errors.FailedMapping<TIn, TOut>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Result<TOut>> MapOk<TIn, TOut>(this Task<Result<TIn>> src, Func<TIn, TOut> value)
        => src.Transform(r => r.MapOk(value), Result<TOut>.Failure);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Result<TOut>> MapOk<TIn, TOut>(this Task<Result<TIn>> src, Func<TIn, Task<TOut>> value)
        => src.Transform(r => r.MapOk(value), ex => Result<TOut>.Failure(ex).ToTask());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult SwitchDispose<T, TResult>(
        this Result<T> src,
        Func<T, TResult> ok,
        Func<Error, TResult> error,
        Func<Exception, TResult> exception
    ) where T : IDisposable => src switch
    {
        { } when src.IsOk(out var okVal) => CallDispose(ok)(okVal),
        { } when src.IsError(out var errorVal) => error(errorVal.Value),
        { } when src.IsFailure(out var exVal) => exception(exVal),
        _ => throw new NotSupportedException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Action<TIn> CallDispose<TIn>(Action<TIn> func)
        where TIn : IDisposable => x =>
    {
        try
        {
            func(x);
        }
        finally
        {
            x.Dispose();
        }
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Func<TIn, TOut> CallDispose<TIn, TOut>(Func<TIn, TOut> func)
        where TIn : IDisposable => x =>
    {
        try
        {
            return func(x);
        }
        finally
        {
            x.Dispose();
        }
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult Switch<T, TResult>(
        this Result<T> src,
        Func<T, TResult> ok,
        Func<Error, TResult> error,
        Func<Exception, TResult> exception
    ) => src switch
    {
        { } when src.IsOk(out var okVal) => ok(okVal),
        { } when src.IsError(out var errorVal) => error(errorVal.Value),
        { } when src.IsFailure(out var exVal) => exception(exVal),
        _ => throw new NotSupportedException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> Switch<T, TResult>(
        this Result<T> src,
        Func<T, Task<TResult>> ok,
        Func<Error, Task<TResult>> error,
        Func<Exception, Task<TResult>> exception
    ) => src switch
    {
        { } when src.IsOk(out var okVal) => ok(okVal),
        { } when src.IsError(out var errorVal) => error(errorVal.Value),
        { } when src.IsFailure(out var exVal) => exception(exVal),
        _ => throw new NotSupportedException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> Switch<T, TResult>(
        this Task<Result<T>> src,
        Func<T, TResult> ok,
        Func<Error, TResult> error,
        Func<Exception, TResult> exception
    ) => src.Transform(v => v.Switch(ok, error, exception), ex => throw ex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> Switch<T, TResult>(
        this Task<Result<T>> src,
        Func<T, Task<TResult>> ok,
        Func<Error, Task<TResult>> error,
        Func<Exception, Task<TResult>> exception
    ) => src.Transform(v => v.Switch(ok, error, exception), ex => throw ex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task Switch<T>(
        this Task<Result<T>> src,
        Func<T, Task> ok,
        Func<Error, Task> error,
        Func<Exception, Task> exception
    ) => src.Chain(v => v.Switch(ok, error, exception), ex => throw ex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task SwitchDispose<T>(
        this Task<Result<T>> src,
        Func<T, Task> ok,
        Func<Error, Task> error,
        Func<Exception, Task> exception
    ) where T : IDisposable => src.Chain(v => v.SwitchDispose(ok, error, exception), ex => throw ex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<TResult> Switch<T, TResult>(
        this ValueTask<Result<T>> src,
        Func<T, TResult> ok,
        Func<Error, TResult> error,
        Func<Exception, TResult> exception
    ) => src.Transform(r => r switch
    {
        { } when r.IsOk(out var okVal) => ok(okVal),
        { } when r.IsError(out var errorVal) => error(errorVal.Value),
        { } when r.IsFailure(out var exVal) => exception(exVal),
        _ => throw new NotSupportedException()
    }, ex => throw ex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Switch(
        this Result src,
        Action ok,
        Action<Error> error,
        Action<Exception> exception
    )
    {
        switch (src)
        {
            case { } when src.IsOk():
                ok();
                break;
            case { } when src.IsError(out var errorVal):
                error(errorVal.Value);
                break;
            case { } when src.IsFailure(out var exVal):
                exception(exVal);
                break;
            default: throw new NotSupportedException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Switch<T>(
        this Result<T> src,
        Action<T> ok,
        Action<Error> error,
        Action<Exception> exception
    )
    {
        switch (src)
        {
            case { } when src.IsOk(out var okVal):
                ok(okVal);
                break;
            case { } when src.IsError(out var errorVal):
                error(errorVal.Value);
                break;
            case { } when src.IsFailure(out var exVal):
                exception(exVal);
                break;
            default: throw new NotSupportedException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwitchDispose<T>(
        this Result<T> src,
        Action<T> ok,
        Action<Error> error,
        Action<Exception> exception
    ) where T : IDisposable
    {
        switch (src)
        {
            case { } when src.IsOk(out var okVal):
                CallDispose(ok)(okVal);
                break;
            case { } when src.IsError(out var errorVal):
                error(errorVal.Value);
                break;
            case { } when src.IsFailure(out var exVal):
                exception(exVal);
                break;
            default: throw new NotSupportedException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask Switch(
        this ValueTask<Result> src,
        Action ok,
        Action<Error> error,
        Action<Exception> exception
    ) => (await src).Switch(ok, error, exception);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Value<T>(this Result<T> result) => result.Switch(
        ok => ok,
        err => err.Throw<T>(),
        ex => throw ex
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<T> Value<T>(this Task<Result<T>> result) => result.Switch(
        ok => ok,
        err => err.Throw<T>(),
        ex => throw ex
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> Value<T>(this ValueTask<Result<T>> result)
        => (await result).Value();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Void(this Result result) => result.Switch(
        () => { },
        err => err.Throw(),
        ex => throw ex
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask Void(this ValueTask<Result> result)
        => (await result).Void();
}