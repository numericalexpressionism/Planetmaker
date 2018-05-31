using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoronoiGraph
{
  public readonly Dictionary<Vector3, VoronoiNode> _nodes = new Dictionary<Vector3, VoronoiNode>();

  public VoronoiNode GetOrCreateNode(Vector2 centre)
  {
    if (!_nodes.ContainsKey(centre))
    {
      _nodes.Add(centre, new VoronoiNode(centre));
    }
    return _nodes[centre];
  }

  public VoronoiNode First()
  {
    return _nodes.First().Value;
  }
}