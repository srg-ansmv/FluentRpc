using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FluentRpc.Common;

public readonly record struct Error(string Code, string Reason)
{
    [DoesNotReturn]
    public T Throw<T>() => throw new InvalidOperationException($"[{Code}] with {Reason}");
    [DoesNotReturn]
    public void Throw() => throw new InvalidOperationException($"[{Code}] with {Reason}");
}

public static class Errors
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error FailedMapping<TIn, TOut>()
        => new("FailedMapping", $"Mapping from {typeof(TIn).FullName} to {typeof(TOut)} has failed");
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error FailedFailure<TIn, TOut>()
        => new("FailedFailure", $"Trying to create fail from {typeof(TIn).FullName} to {typeof(TOut)} failed");
}