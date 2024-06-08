using System.Collections.Concurrent;

namespace UrlFrontier;

public class PriorityQueue<T> where T : struct
{
    private readonly ConcurrentDictionary<T?, int> _items = new ConcurrentDictionary<T?, int>();
    private readonly BlockingCollection<T?> _queue = new BlockingCollection<T?>();






    public void Enqueue(T item, int priority)
    {
        _items.TryAdd(item, priority);
        _queue.Add(item);
    }






    public T? Dequeue()
    {
        while (true)
        {
            var item = _queue.Take();
            if (_items.TryRemove(item, out _))
            {
                return item;
            }
        }
    }






    public int Count => _items.Count;
}