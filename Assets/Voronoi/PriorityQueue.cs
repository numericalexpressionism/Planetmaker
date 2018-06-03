using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class PriorityQueue<TValue>: IEnumerable<TValue>
{
  class RoughFloatComparer : IComparer<float>
  {
    public int Compare(float x, float y)
    {
      if (x - y > Constants.Sigma)
      {
        return 1;
      }
      if (x - y < -Constants.Sigma)
      {
        return -1;
      }
      return 0;
    }
  }

  private readonly Func<TValue, float> _keyFunc;
  private readonly SortedList<float, Queue<TValue>> _sortedList;

  public PriorityQueue(Func<TValue, float> keyFunc)
  {
    _keyFunc = keyFunc;
    _sortedList = new SortedList<float, Queue<TValue>>(new RoughFloatComparer());
  }

  public int Count { get; private set; }

  public void Enqueue(TValue value)
  {
    Count++;
    var key = _keyFunc(value);
    if (!_sortedList.ContainsKey(key))
    {
      _sortedList.Add(key, new Queue<TValue>());
    }
    // we extract from head, so head will be List[Last] and then tail becomes T[0]
    // this makes extraction cheap and adding expensive
    _sortedList[key].Enqueue(value);
  }

  public TValue Peek()
  {
    if (_sortedList.Count == 0)
    {
      throw new InvalidOperationException("queue is empty");
    }
    var top = _sortedList.ElementAt(0).Value;
    var result = top.Peek();
    return result;
  }

  public TValue Dequeue()
  {
    if (_sortedList.Count == 0)
    {
      throw new InvalidOperationException("queue is empty");
    }

    var top = _sortedList.ElementAt(0).Value;
    // we extract from head, so head will be List[Last] and then tail becomes T[0]
    // this makes extraction cheap and adding expensive
    var result = top.Dequeue();
    Count--;
    if (top.Count == 0)
    {
      _sortedList.RemoveAt(0);
    }
    return result;
  }

  public void Clear()
  {
    _sortedList.Clear();
  }

  public IEnumerator<TValue> GetEnumerator()
  {
    foreach (var partialQueue in _sortedList.Values)
    {
      foreach (var v in partialQueue)
      {
        yield return v;
      }
    }
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}