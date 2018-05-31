using System.Collections.Generic;
using UnityEngine;

public class VoronoiNode
{
  public readonly Vector3 centre;
  private readonly Dictionary<VoronoiNode, VectorPair> _edgeVerts;

  public VoronoiNode(Vector2 centre)
  {
    this.centre = centre;
    _edgeVerts = new Dictionary<VoronoiNode, VectorPair>();
  }

  public void LinkTo(VoronoiNode other, List<Vector3> edgeVerts)
  {
    var pair = new VectorPair(edgeVerts[0], edgeVerts[1]);
    _edgeVerts.Add(other, pair);
    other._edgeVerts.Add(this, pair);
  }

  public IEnumerable<VectorPair> GetEdges()
  {
    return _edgeVerts.Values;
  }

  public IEnumerable<VoronoiNode> GetNeighbors()
  {
    return _edgeVerts.Keys;
  }
}