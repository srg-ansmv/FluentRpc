using System.Buffers;
using System.Runtime.InteropServices;

namespace FluentRpc.Common;

public class CompositeMemoryOwner : IMemoryOwner<byte>
{
    private readonly IEnumerable<IMemoryOwner<byte>> _owners;

    public CompositeMemoryOwner(IEnumerable<IMemoryOwner<byte>> owners)
    {
        _owners = owners;
    }

    private Memory<byte>? _memory;
    
    public Memory<byte> Memory
    {
        get
        {
            if (_memory.HasValue)
            {
                return _memory.Value;
            }
            
            var owners = CollectionsMarshal.AsSpan(_owners.ToList());
            var totalSize = 0;
            foreach (var memoryOwner in owners)
            {
                totalSize += memoryOwner.Memory.Length;
            }

            var totalMemory = new Memory<byte>(new byte[totalSize]);
            int copied = 0;
            foreach (var memoryOwner in owners)
            {
                var memory = memoryOwner.Memory;
                memory.CopyTo(totalMemory[copied..]);
                copied += memory.Length;
            }

            _memory = totalMemory;
            return _memory.Value;
        }
    }

    public void Dispose()
    {
        foreach (var memoryOwner in _owners)
        {
            memoryOwner.Dispose();
        }
    }
}