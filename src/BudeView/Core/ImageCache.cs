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
    private readonly TraceWriter _trace;
    private readonly Dictionary<string, Entry> _entries =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> _lru = new();

    private long _totalCost;
    private string? _protectedPath;

    public ImageCache(long budgetBytes, TraceWriter trace)
    {
        _budgetBytes = Math.Max(1, budgetBytes);
        _trace = trace;

        _trace.Write(
            "cache_budget",
            details: new Dictionary<string, string>
            {
                ["bytes"] = _budgetBytes.ToString(),
                ["mib"] = (_budgetBytes / 1024d / 1024d).ToString("0.0")
            });
    }

    public long BudgetBytes => _budgetBytes;

    public void Protect(string? path)
    {
        lock (_gate)
        {
            _protectedPath = path;
            if (path is not null && _entries.TryGetValue(path, out var entry))
            {
                Touch(path, entry);
            }
        }
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

    public Bitmap StoreCurrent(string path, Bitmap bitmap)
    {
        lock (_gate)
        {
            if (_entries.TryGetValue(path, out var existing))
            {
                Touch(path, existing);

                if (!ReferenceEquals(existing.Bitmap, bitmap))
                {
                    bitmap.Dispose();
                }

                return existing.Bitmap;
            }

            AddEntry(path, bitmap);
            TrimToBudget();

            return bitmap;
        }
    }

    public bool TryStorePreload(string path, Bitmap bitmap)
    {
        lock (_gate)
        {
            if (_entries.TryGetValue(path, out var existing))
            {
                Touch(path, existing);

                if (!ReferenceEquals(existing.Bitmap, bitmap))
                {
                    bitmap.Dispose();
                }

                return true;
            }

            var cost = EstimateCost(bitmap);

            if (_totalCost + cost > _budgetBytes)
            {
                _trace.Write(
                    "cache_preload_rejected",
                    Path.GetFileName(path),
                    new Dictionary<string, string>
                    {
                        ["cost_bytes"] = cost.ToString(),
                        ["total_bytes"] = _totalCost.ToString()
                    });

                bitmap.Dispose();
                return false;
            }

            AddEntry(path, bitmap, cost);
            return true;
        }
    }

    private void AddEntry(string path, Bitmap bitmap, long? knownCost = null)
    {
        var cost = knownCost ?? EstimateCost(bitmap);
        var entry = new Entry(bitmap, cost)
        {
            Node = _lru.AddLast(path)
        };

        _entries[path] = entry;
        _totalCost += cost;

        _trace.Write(
            "cache_add",
            Path.GetFileName(path),
            new Dictionary<string, string>
            {
                ["cost_bytes"] = cost.ToString(),
                ["total_bytes"] = _totalCost.ToString(),
                ["count"] = _entries.Count.ToString()
            });
    }

    private void TrimToBudget()
    {
        while (_totalCost > _budgetBytes && _entries.Count > 1)
        {
            var node = _lru.First;

            while (node is not null &&
                   string.Equals(node.Value, _protectedPath, StringComparison.OrdinalIgnoreCase))
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

        _trace.Write(
            "cache_evict",
            Path.GetFileName(path),
            new Dictionary<string, string>
            {
                ["total_bytes"] = _totalCost.ToString(),
                ["count"] = _entries.Count.ToString()
            });
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
            _protectedPath = null;
        }
    }
}
