using System.Collections.Generic;
using UnityEngine;

public class VoronoiEdgesList
{
  readonly Dictionary<VectorPair, List<Vector3>> _edges = new Dictionary<VectorPair, List<Vector3>>();

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

  public VoronoiGraph CompileGraph()
  {
    var graph = new VoronoiGraph();
    foreach (var edge in _edges)
    {
      var node1 = graph.GetOrCreateNode(edge.Key._one);
      var node2 = graph.GetOrCreateNode(edge.Key._other);
      node1.LinkTo(node2, edge.Value);
    }
    return graph;
  }
}

public class VectorPair
{
  readonly public Vector3 _one;
  readonly public Vector3 _other;
  public VectorPair(Vector3 one, Vector3 other)
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