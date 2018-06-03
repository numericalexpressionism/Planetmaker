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

  public void RenderEquirectangular(Texture2D mainTexture)
  {
    foreach (var node in _nodes)
    {
      var p = node.Value.centre;

      int px = Mathf.RoundToInt((1 - ((p.x+180) / 360f)) * mainTexture.width);
      int py = Mathf.RoundToInt((1 - (p.y / 180f)) * mainTexture.height);
      mainTexture.SetPixel(px, py, Color.red);
    }
    mainTexture.Apply();
  }
}