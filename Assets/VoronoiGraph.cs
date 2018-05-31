using System.Collections.Generic;
using UnityEngine;

class VoronoiGraph
{
  Dictionary<VectorPair, List<Vector3>> _edges = new Dictionary<VectorPair, List<Vector3>>();
  class VectorPair
  {
    readonly private Vector2 _one;
    readonly private Vector2 _other;
    public VectorPair(Vector2 one, Vector2 other)
    {
      _one = one;
      _other = other;
    }

    protected bool Equals(VectorPair other)
    {
      return (_one == other._one && _other == other._other) || (_one == other._other && _other == other._one);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((VectorPair)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return ((_one + _other).GetHashCode() * 397) ^ (Vector2.Dot(_one, _other)).GetHashCode();
      }
    }

    public static bool operator ==(VectorPair left, VectorPair right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(VectorPair left, VectorPair right)
    {
      return !Equals(left, right);
    }
  }

  public void AddVert(Vector2 a, Vector2 b, Vector3 node)
  {
    if (a == b)
    {
      Debug.LogError("A and B are SAME!!!!!!");
    }
    var key = new VectorPair(a, b);
    if (!_edges.ContainsKey(key))
    {
      _edges[key] = new List<Vector3>(2);
      _edges[key].Add(node);
    }
    else
    {
      if (_edges[key].Contains(node))
      {
        return;
      }
      _edges[key].Add(node);
      if (_edges[key].Count > 2)
      {
        Debug.LogError("ERROR - TOO MANY.");
      }
    }
  }

  public void DrawDebug()
  {
    foreach (var edge in _edges)
    {
      if (edge.Value.Count == 1)
      {
        DebugHelper.DrawPoint(edge.Value[0], 0.2f, Color.blue);
      }
      else
      {
        DebugHelper.DrawArc(edge.Value[0], edge.Value[1], Color.blue, 8);
      }
    }
  }
}