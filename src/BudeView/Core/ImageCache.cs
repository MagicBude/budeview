using Avalonia.Media.Imaging;

namespace BudeView.Core;

public sealed class ImageCache : IDisposable
{
    private sealed class Entry(Bitmap bitmap, long cost)
    {
        public Bitmap Bitmap { get; } = bitmap;
        public long Cost { get; } = cost;
        public LinkedListNode<string>? Node { get; set; }
    }

    private readonly object _gate = new();
    private readonly long _budgetBytes;
    private readonly Dictionary<string, Entry> _entries =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> _lru = new();

    private long _totalCost;

    public ImageCache(long budgetBytes)
    {
        _budgetBytes = Math.Max(1, budgetBytes);
    }

    public bool TryGet(string path, out Bitmap bitmap)
    {
        lock (_gate)
        {
            if (!_entries.TryGetValue(path, out var entry))
            {
                bitmap = null!;
                return false;
            }

            Touch(path, entry);
            bitmap = entry.Bitmap;
            return true;
        }
    }

    public void Add(string path, Bitmap bitmap, string? protectedPath = null)
    {
        lock (_gate)
        {
            if (_entries.ContainsKey(path))
            {
                return;
            }

            var cost = EstimateCost(bitmap);
            var entry = new Entry(bitmap, cost);
            entry.Node = _lru.AddLast(path);
            _entries[path] = entry;
            _totalCost += cost;

            Trim(protectedPath);
        }
    }

    private void Trim(string? protectedPath)
    {
        while (_totalCost > _budgetBytes && _entries.Count > 1)
        {
            var node = _lru.First;
            while (node is not null &&
                   string.Equals(node.Value, protectedPath, StringComparison.OrdinalIgnoreCase))
            {
                node = node.Next;
            }

            if (node is null)
            {
                return;
            }

            Remove(node.Value);
        }
    }

    private void Remove(string path)
    {
        if (!_entries.Remove(path, out var entry))
        {
            return;
        }

        if (entry.Node is not null)
        {
            _lru.Remove(entry.Node);
        }

        _totalCost -= entry.Cost;
        entry.Bitmap.Dispose();
    }

    private void Touch(string path, Entry entry)
    {
        if (entry.Node is not null)
        {
            _lru.Remove(entry.Node);
        }

        entry.Node = _lru.AddLast(path);
    }

    private static long EstimateCost(Bitmap bitmap)
        => checked((long)bitmap.PixelSize.Width * bitmap.PixelSize.Height * 4L);

    public void Dispose()
    {
        lock (_gate)
        {
            foreach (var entry in _entries.Values)
            {
                entry.Bitmap.Dispose();
            }

            _entries.Clear();
            _lru.Clear();
            _totalCost = 0;
        }
    }
}
