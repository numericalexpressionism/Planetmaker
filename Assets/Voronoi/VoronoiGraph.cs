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
      var uv = GetUV(mainTexture, p);

      int py = Mathf.FloorToInt(uv.y);

      float xScale = Mathf.Clamp(1.0f / Mathf.Cos((uv.y/ mainTexture.height - 0.5f) * Mathf.PI ), 1, mainTexture.width);

      int xMin = Mathf.RoundToInt(uv.x - xScale / 2.0f) % mainTexture.width;
      int xMax = Mathf.RoundToInt(uv.x + xScale / 2.0f) % mainTexture.width;

      if (xMax < xMin)
      {
        // wrapping happened
        for (int i = xMin; i < mainTexture.width; i++)
        {
          mainTexture.SetPixel(i, py, Color.red);
        }
        for (int i = 0; i < xMax; i++)
        {
          mainTexture.SetPixel(i, py, Color.red);
        }
      }
      else
      {
        for (int i = xMin; i < xMax; i++)
        {
          mainTexture.SetPixel(i, py, Color.red);
        }
        //no wrapping
      }

      var edges = node.Value.GetEdges();
      foreach (var edge in edges)
      {
        var src = GetUV(mainTexture, edge._one);
        var dst = GetUV(mainTexture, edge._other);
      }
    }
    mainTexture.Apply();
  }

  private Vector2 GetUV(Texture2D mainTexture, Vector2 latLng)
  {
    return new Vector2(
      (1 - ((latLng.x + 180) / 360f)) * mainTexture.width,
      (1 - (latLng.y / 180f)) * mainTexture.height
      );
  }
}