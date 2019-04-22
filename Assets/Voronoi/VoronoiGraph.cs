using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoronoiGraph
{
  readonly Dictionary<Vector2, VoronoiNode> _nodes = new Dictionary<Vector2, VoronoiNode>();

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

      var color = Color.Lerp(Color.red, Color.blue, node.Value.data.Value / 2.0f + 0.5f);// 

      int py = Mathf.FloorToInt(uv.y);

      float xScale = Mathf.Clamp(1.0f / Mathf.Cos((uv.y/ mainTexture.height - 0.5f) * Mathf.PI ), 1, mainTexture.width);

      int xMin = Mathf.RoundToInt(uv.x - xScale / 2.0f) % mainTexture.width;
      int xMax = Mathf.RoundToInt(uv.x + xScale / 2.0f) % mainTexture.width;

      if (xMax < xMin)
      {
        // wrapping happened
        for (int i = xMin; i < mainTexture.width; i++)
        {
          mainTexture.SetPixel(i, py, color);
        }
        for (int i = 0; i < xMax; i++)
        {
          mainTexture.SetPixel(i, py, color);
        }
      }
      else
      {
        for (int i = xMin; i < xMax; i++)
        {
          mainTexture.SetPixel(i, py, color);
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

  public void SetValue(Func<Vector3, NodeData> getNoise)
  {
    foreach (var node in _nodes)
    {
      _nodes[node.Key].data = getNoise(node.Key);
    }

    //foreach (var node in _nodes)
    //{
    //  var current = node.Value;
    //
    //  List<Vector3> Outputs = new List<Vector3>();
    //  foreach (var neighbor in current.GetNeighbors())
    //  {
    //    //ToDo: add curl from edge midpoints.
    //    var Midpoint = current.GetEdgeMidpointCart(neighbor);
    //    var dr = Quaternion.FromToRotation(MathS.SphToCartesian(current.centre), Midpoint);
    //    var dDiv = neighbor.data.Value - current.data.Value;
    //    float angle;
    //    Vector3 axis;
    //    dr.ToAngleAxis(out angle, out axis);
    //    var partial = Quaternion.AngleAxis(angle * dDiv, axis);
    //    current.data.PartVelocities.Add(partial);
    //    Outputs.Add(partial* MathS.SphToCartesian(current.centre));
    //  }
    //  Vector3 SumMovement = Outputs.Aggregate((prev, item) => prev + item);
    //  current.data.Velocity = Quaternion.FromToRotation(MathS.SphToCartesian(current.centre), SumMovement.normalized);
    //}
  }
}

public class NodeData
{
  public List<Quaternion> PartVelocities = new List<Quaternion>(); //velocity
  public Quaternion Velocity; //velocity
  public float Value; //divirgence
  public float Curl; //curl
}