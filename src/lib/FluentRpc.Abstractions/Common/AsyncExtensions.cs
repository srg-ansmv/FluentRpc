using System.Runtime.CompilerServices;

namespace FluentRpc.Common;

internal static class AsyncExtensions
{
    public static Task<T> ToTask<T>(this T value) => Task.FromResult(value);
    public static ValueTask<T> ToValueTask<T>(this T value) => ValueTask.FromResult(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TIn> Chain<TIn>(
        this Task<TIn> src,
        Action<TIn> value,
        Action<Exception> exception
    ) => src.ContinueWith(t =>
    {
        switch (t)
        {
            case { Exception: null }:
                value(t.Result);
                return t.Result;
            case { Exception: not null }:
                exception(t.Exception);
                throw t.Exception;
            default: throw new NotSupportedException();
        }
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TOut> Transform<TIn, TOut>(
        this Task<TIn> src,
        Func<TIn, TOut> value,
        Func<Exception, TOut> exception
    ) => src.ContinueWith(
        t => t switch
        {
            { Exception: null } => value(t.Result),
            { Exception: not null } => exception(t.Exception)
        }
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TOut> Transform<TIn, TOut>(
        this Task<TIn> src,
        Func<TIn, Task<TOut>> value,
        Func<Exception, Task<TOut>> exception
    ) => src.ContinueWith(
        t => t switch
        {
            { Exception: null } => value(t.Result),
            { Exception: not null } => exception(t.Exception)
        }
    ).Unwrap();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task Chain<TIn>(
        this Task<TIn> src,
        Func<TIn, Task> value,
        Func<Exception, Task> exception
    ) => src.ContinueWith(
        t => t switch
        {
            { Exception: null } => value(t.Result),
            { Exception: not null } => exception(t.Exception)
        }
    ).Unwrap();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<TOut> Transform<TIn, TOut>(
        this ValueTask<TIn> src,
        Func<TIn, TOut> value,
        Func<Exception, TOut> exception
    )
    {
        try
        {
            var result = await src;
            return value(result);
        }
        catch (Exception e)
        {
            return exception(e);
        }
    }
}