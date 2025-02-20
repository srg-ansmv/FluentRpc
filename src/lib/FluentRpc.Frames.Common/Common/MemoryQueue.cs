using System.Buffers;
using FluentRpc.Connections.Abstraction;

namespace FluentRpc.Common;

public class MemoryQueue
{
    private readonly Queue<Memory<byte>> _queue;
    private int _size;

    public MemoryQueue()
    {
        _queue = new();
    }

    public bool IsEmpty => _queue.Count == 0;

    public MemoryQueue EnqueueMemory(Memory<byte> bytes)
    {
        _queue.Enqueue(bytes);
        _size += bytes.Length;
        return this;
    }

    public MemoryQueue EnqueuePacket(ReadPacket readPacket)
    {
        var (buffer, size) = readPacket.ReleaseOwnership();
        try
        {
            if (buffer.Memory.IsEmpty)
            {
                return this;
            }

            var memory = new Memory<byte>(new byte[size]);
            buffer.Memory[..size].CopyTo(memory);

            _queue.Enqueue(memory);
            _size += size;

            return this;
        }
        finally
        {
            buffer.Dispose();
        }
    }

    public IMemoryOwner<byte> DequeueAll(DequeueAllOptions options)
    {
        var memoryResult = options.MemoryPool?.Rent(_size)
                           ?? new NotPooledMemoryOwner(new byte[_size]);

        var deqSize = 0;
        while (!IsEmpty)
        {
            var memory = _queue.Dequeue();
            memory.CopyTo(memoryResult.Memory[deqSize..]);

            deqSize += memory.Length;
        }

        return memoryResult;
    }
}

public readonly struct DequeueAllOptions
{
    public MemoryPool<byte>? MemoryPool { get; init; }
}