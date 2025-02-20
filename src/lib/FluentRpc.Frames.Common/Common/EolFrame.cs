using System.Buffers;
using System.Text;

namespace FluentRpc.Common;

public readonly struct EolFrame : IDisposable
{
    private readonly IMemoryOwner<byte> _buffer;
    private readonly int _size;
    private readonly IReadOnlyList<Range> _ranges;
    private readonly IMemoryOwner<byte>? _additionalMemory;

    public EolFrame(
        IMemoryOwner<byte> buffer,
        int size,
        IReadOnlyList<Range> ranges,
        IMemoryOwner<byte>? additionalMemory = null)
    {
        _buffer = buffer;
        _size = size;
        _ranges = ranges;
        _additionalMemory = additionalMemory;
    }

    public IEnumerable<Message> EnumerateReceivedMessages()
    {
        return _additionalMemory is not null
            ? EnumerateWithAdditional(_additionalMemory)
            : EnumerateWithoutAdditional();
    }

    private IEnumerable<Message> EnumerateWithoutAdditional()
    {
        foreach (var range in _ranges)
        {
            yield return new Message(_buffer, _size, range);
        }
    }
    private IEnumerable<Message> EnumerateWithAdditional(IMemoryOwner<byte> additional)
    {
        var first = _ranges[0];
        var additionalLength = additional.Memory.Length;
        var updatedRange = new Range(0, (first.End.Value + additionalLength));

        yield return new Message(new CompositeMemoryOwner([additional, _buffer]), _size, updatedRange);

        foreach (var range in _ranges.Skip(1))
        {
            yield return new Message(_buffer, _size, range);
        }
    }

    public void Dispose()
    {
        _buffer.Dispose();
        _additionalMemory?.Dispose();
    }
    
    public readonly struct Message
    {
        private readonly IMemoryOwner<byte> _buffer;
        private readonly int _bufferSize;
        private readonly Range _range;

        public Message(IMemoryOwner<byte> buffer, int bufferSize, Range range)
        {
            _buffer = buffer;
            _bufferSize = bufferSize;
            _range = range;
        }

        public string Decode(Encoding encoding) => encoding.GetString(_buffer.Memory.Span[_range]);
    }
}