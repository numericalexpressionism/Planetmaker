using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class generates a classic sphere with edges follow meridians and circles of lattitude. 
/// It is very simple and 
/// </summary>
public class MeridianSphere
{
  public static Mesh Create(float radius, int meridians, int latitudeCircles)
  {
    // to create minimal mesh we need at least 3 meridians. 
    if (meridians < 3)
    {
      throw new InvalidOperationException("can't make mesh with " + meridians + " meridians. At least 3 are needed");
    }
    if (latitudeCircles < 1)
    {
      throw new InvalidOperationException("can't make mesh with" + latitudeCircles + " latitude circles. At least 1 is needed");
    }
    List<Vector3> points = new List<Vector3>();
    List<Vector2> uv0 = new List<Vector2>();
    List<int> indices = new List<int>();

    // we actually generate a tube-like shape, then deform it to look like sphere.
    // also it will have a seam on zero meridian, so the texture doesn't have any artifacts
    var latitudeBelts = latitudeCircles + 1;
    var latitudePoints = latitudeCircles + 2;

    GeneratePoints(meridians, latitudeBelts, points);
    GenerateUVs(meridians, latitudeBelts, uv0);
    GenerateIndices(meridians, latitudePoints, indices);

    List<Vector3> normals = points;
    List<Vector3> vertices = normals.Select(n => n * radius).ToList();

    var result = new Mesh();
    result.SetVertices(vertices);
    result.SetNormals(normals);
    result.SetUVs(0, uv0);
    result.SetTriangles(indices, 0);

    return result;
  }

  private static void GenerateUVs(int meridians, int latitudeBelts, List<Vector2> uvs)
  {
    var uStep = new Vector2(1f / meridians, 0);
    var vStep = new Vector2(0, -(1f / latitudeBelts));

    Vector2 pointOnLatitude = new Vector2(0, 1);
    for (int i = 0; i < meridians + 1; i++)
    {
      Vector2 point = pointOnLatitude;
      for (int j = 0; j < latitudeBelts + 1; j++)
      {
        uvs.Add(point);
        point += vStep;
      }
      pointOnLatitude += uStep;
    }
  }
  private static void GeneratePoints(int meridians, int latitudeBelts, List<Vector3> points)
  {
    for (int i = 0; i < meridians + 1; i++)
    {
      var phi = Mathf.PI * 2 / meridians * i;

      for (int j = 0; j < latitudeBelts + 1; j++)
      {
        // theta [θ] and phi [φ] are standard notation in polar coordinates
        var theta = Mathf.PI / (latitudeBelts) * j;

        /* wikipedia says, that polar-to-cartesian formula is

          x=r *sin(theta)*cos(phi)
          y=r *sin(theta)*sin(phi)
          z=r *cos(theta)

          but that works for X-Y-Z coordinates, while unity has X-Z-Y

          */
        var point = new Vector3(
          -Mathf.Sin(theta) * Mathf.Cos(phi),
          Mathf.Cos(theta),
          -Mathf.Sin(theta) * Mathf.Sin(phi)
          );
        points.Add(point);
      }
    }
  }

  private static void GenerateIndices(int meridians, int latitudePoints, List<int> indices)
  {
    for (int i = 0; i < meridians; i++)
    {
      for (int j = 0; j < latitudePoints - 1; j++)
      {
        int a = i * latitudePoints + j;
        int b = a + 1;
        int c = a + latitudePoints;
        int d = c + 1;
        indices.AddRange(new[]{ a, c, b, c, d, b});
      }
    }
  }
}