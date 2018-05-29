using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tetrasphere : MonoBehaviour
{
  [Range(1,8)]
  public int depth;
  private TriangleSphere _sphere;
  public Texture texture;
  public float a;

  // Use this for initialization
  void OnEnable()
  {
    _sphere = new TriangleSphere(depth, a);
    MeshFilter mf = GetComponent<MeshFilter>() ?? new MeshFilter();
    mf.mesh = new Mesh();
    mf.mesh.SetVertices(_sphere.GetVertices());
    mf.mesh.SetUVs(0, _sphere.GetUVs());
    mf.mesh.SetTriangles(_sphere.GetMeshIndices(), 0, true);
    mf.mesh.UploadMeshData(true);

    MeshRenderer mr = GetComponent<MeshRenderer>() ?? new MeshRenderer();
    mr.material.SetTexture("_MainTex", texture);
  }

  // Update is called once per frame
  void Update()
  {
    _sphere.DrawDebug();
  }
}

class TriangleSphere
{
  private Triangle[] _topLevel;

  class Triangle
  {
    public Quaternion[] arcs;
    private Triangle[] _neighbors;
    public Triangle[] children;

    public int[] indices = new int[3];
    public Triangle(int p1Idx, int p2Idx, int p3Idx, Quaternion[]arcs)
    {
      _neighbors = new Triangle[3];
      indices[0] = p1Idx;
      indices[1] = p2Idx;
      indices[2] = p3Idx;
      this.arcs = arcs;
    }

    public void GetIndices(List<int> result)
    {
      if (children == null)
      {
        result.AddRange(indices);
      }
      else
      {
        foreach (var child in children)
        {
          if (child == null)
          {
            continue;//hack for partial filling. ToDo: remove
          }
          child.GetIndices(result);
        }
      }
    }

    public void SetNeighbours(Triangle first, Triangle second, Triangle third)
    {
      _neighbors[0] = first;
      _neighbors[1] = second;
      _neighbors[2] = third;
    }

    public void Subdivide(int i, List<Vector3> vertices, List<Vector2> uvs, float arcAngle)
    {
      if (i <= 0)
      {
        return;
      }
      var mid1 = GetIdxOfNewMidpoint(vertices, uvs, 0, 1, arcs[0]);
      var mid2 = GetIdxOfNewMidpoint(vertices, uvs, 1, 2, arcs[1]);
      var mid3 = GetIdxOfNewMidpoint(vertices, uvs, 2, 0, arcs[2]);

      children = new Triangle[4];

      var innerArc0 = InnerArc(vertices[indices[0]], vertices[mid1], vertices[mid3], arcAngle);
      var innerArc1 = InnerArc(vertices[indices[1]], vertices[mid2], vertices[mid1], arcAngle);
      var innerArc2 = InnerArc(vertices[indices[2]], vertices[mid3], vertices[mid2], arcAngle);
      var innerArc0R = InnerArc(vertices[indices[0]], vertices[mid3], vertices[mid1], arcAngle);
      var innerArc1R = InnerArc(vertices[indices[1]], vertices[mid1], vertices[mid2], arcAngle);
      var innerArc2R = InnerArc(vertices[indices[2]], vertices[mid2], vertices[mid3], arcAngle);

      children[0] = new Triangle(indices[0], mid1, mid3, new [] { HalfArcs(0), innerArc0, HalfArcs(2) });
      children[1] = new Triangle(mid1, indices[1], mid2, new [] { HalfArcs(0), HalfArcs(1), innerArc1 });
      children[2] = new Triangle(mid3, mid2, indices[2], new [] { innerArc2, HalfArcs(1), HalfArcs(2) });
      children[3] = new Triangle(mid2, mid3, mid1,       new [] { innerArc2R, innerArc0R, innerArc1R });
      //recursion is not ok here. todo: reimplement with iterations

      foreach (var child in children)
      {
        if (child == null)
        {
          continue;
        }
        child.Subdivide(i - 1, vertices, uvs, arcAngle);
      }
    }

    private Quaternion Neg(Quaternion innerArc)
    {
      return ScaleArcAngle(innerArc, -1);
    }

    private Quaternion InnerArc(Vector3 midpoint, Vector3 src, Vector3 dst, float a)
    {
      var bArc = Quaternion.FromToRotation(src, dst);
      float barcAngle;
      Vector3 barcAxis;
      bArc.ToAngleAxis(out barcAngle, out barcAxis);
      Vector3 median = Quaternion.AngleAxis(barcAngle/2, barcAxis)* src;
      float shiftAngle = 0;
      Vector3 axisTurnAxis = Vector3.Cross(barcAxis, median);
      float resultAngle = Mathf.Pow(Mathf.Cos(shiftAngle * Mathf.Rad2Deg), 2) * barcAngle;
      Vector3 resultAxis = Quaternion.AngleAxis(a,axisTurnAxis)* barcAxis ;



      return Quaternion.AngleAxis(resultAngle, resultAxis);
    }

    private Quaternion HalfArcs(int i)
    {
      return ScaleArcAngle(arcs[i], 0.5f);
    }

    private Quaternion HalfArcsNeg(int i)
    {
      float angle;
      Vector3 axis;
      arcs[i].ToAngleAxis(out angle, out axis);
      return Quaternion.AngleAxis(angle * 0.5f, -axis);
    }

    public void ConstructGreatCircleArcs(List<Vector3> vertices)
    {
      arcs = new Quaternion[3];
      arcs[0] = Quaternion.FromToRotation(vertices[indices[0]], vertices[indices[1]]);
      arcs[1] = Quaternion.FromToRotation(vertices[indices[1]], vertices[indices[2]]);
      arcs[2] = Quaternion.FromToRotation(vertices[indices[2]], vertices[indices[0]]);
    }

    private int GetIdxOfNewMidpoint(List<Vector3> vertices, List<Vector2> uvs, int a, int b, Quaternion arc)
    {
      Vector3 midpoint = GetArcMidpoint(vertices[indices[a]], vertices[indices[b]], arc);
      Vector2 midUv = (uvs[indices[a]] + uvs[indices[b]])/2;
      vertices.Add(midpoint);
      uvs.Add(midUv);
      return vertices.Count-1;
    }

    private Vector3 GetArcMidpoint(Vector3 a, Vector3 b, Quaternion arc)
    {
      var halfrot = ScaleArcAngle(arc, 0.5f);
      return halfrot*a;
    }

    private static Quaternion ScaleArcAngle(Quaternion arc, float s)
    {
      float angle;
      Vector3 axis;
      arc.ToAngleAxis(out angle, out axis);
      return Quaternion.AngleAxis(angle*s, axis);
    }

    public void LinkChildren()
    {
    }
  }

  private List<Vector3> _vertices;
  private List<Vector2> _uvs;

  public TriangleSphere(int n, float a)
  {
    _topLevel = new Triangle[4];
    _vertices = new List<Vector3>((int)Mathf.Pow(4, n));
    _uvs = new List<Vector2>((int)Mathf.Pow(4, n));

    float sqrt89 = 0.94280904158f;
    float sqrt29 = 0.47140452079f;
    float sqrt23 = 0.81649658092f;
    float oneThird = 0.33333333333f;

    _vertices.Add(new Vector3(+sqrt89, -oneThird, 0.00000f));
    _vertices.Add(new Vector3(-sqrt29, -oneThird, +sqrt23));
    _vertices.Add(new Vector3(-sqrt29, -oneThird, -sqrt23));
    _vertices.Add(new Vector3(0.00000f, +1.00000f, 0.00000f));
    _vertices.Add(new Vector3(0.00000f, +1.00000f, 0.00000f));
    _vertices.Add(new Vector3(0.00000f, +1.00000f, 0.00000f));

    const float h2 = 0.43301270189f;
    const float w4 = 0.25000000000f;

    _uvs.Add(new Vector2(w4 * 2, h2 * 0));
    _uvs.Add(new Vector2(w4 * 1, h2 * 1));
    _uvs.Add(new Vector2(w4 * 3, h2 * 1));
    _uvs.Add(new Vector2(w4 * 0, h2 * 0));
    _uvs.Add(new Vector2(w4 * 2, h2 * 2));
    _uvs.Add(new Vector2(w4 * 4, h2 * 0));

    _topLevel[0] = new Triangle(0, 1, 2, null); //A
    _topLevel[1] = new Triangle(1, 0, 3, null); //B
    _topLevel[2] = new Triangle(4, 2, 1, null); //C
    _topLevel[3] = new Triangle(2, 5, 0, null); //D

    foreach (var triangle in _topLevel)
    {
      triangle.ConstructGreatCircleArcs(_vertices);
    }

    _topLevel[0].SetNeighbours(_topLevel[1], _topLevel[2], _topLevel[3]);
    _topLevel[1].SetNeighbours(_topLevel[0], _topLevel[3], _topLevel[2]);
    _topLevel[2].SetNeighbours(_topLevel[3], _topLevel[0], _topLevel[1]);
    _topLevel[3].SetNeighbours(_topLevel[2], _topLevel[1], _topLevel[1]);

    _topLevel[0].Subdivide(n, _vertices, _uvs, a);
    _topLevel[1].Subdivide(n, _vertices, _uvs, a);
    _topLevel[2].Subdivide(n, _vertices, _uvs, a);
    _topLevel[3].Subdivide(n, _vertices, _uvs, a);

    _topLevel[0].LinkChildren();
    _topLevel[1].LinkChildren();
    _topLevel[2].LinkChildren();
    _topLevel[3].LinkChildren();
  }

  public void DrawDebug()
  {
    //Debug.DrawLine(Vertices[0], Vertices[0] * 1.04f, Color.red);
    //Debug.DrawLine(Vertices[1], Vertices[1] * 1.04f, Color.green);
    //Debug.DrawLine(Vertices[2], Vertices[2] * 1.04f, Color.blue);
    //Debug.DrawLine(Vertices[3], Vertices[3] * 1.04f, Color.yellow);

   //DrawDebugArc(TopLevel[0], 0, Color.black);
   //DrawDebugArc(TopLevel[0], 1, Color.white);
   //DrawDebugArc(TopLevel[0], 2, Color.yellow);

    //DrawDebugArc(TopLevel[0].Children[0], 0, Color.blue);
    //DrawDebugArc(TopLevel[0].Children[0], 1, Color.red);
    //DrawDebugArc(TopLevel[0].Children[0], 2, Color.blue);
    //
    //DrawDebugArc(TopLevel[0].Children[1], 0, Color.red);
    //DrawDebugArc(TopLevel[0].Children[1], 1, Color.red);
    //DrawDebugArc(TopLevel[0].Children[1], 2, Color.red);
    //
    //DrawDebugArc(TopLevel[0].Children[2], 0, Color.green);
    //DrawDebugArc(TopLevel[0].Children[2], 1, Color.green);
    //DrawDebugArc(TopLevel[0].Children[2], 2, Color.green);

    //DrawDebugArc(TopLevel[0].Children[3], 0, Color.blue);
    //DrawDebugArc(TopLevel[0].Children[3], 1, Color.red);
    //DrawDebugArc(TopLevel[0].Children[3], 2, Color.green);
    //
    //DrawDebugArc(TopLevel[0].Children[3].Children[3], 0, Color.black);
    //DrawDebugArc(TopLevel[0].Children[3].Children[3], 1, Color.black);
    //DrawDebugArc(TopLevel[0].Children[3].Children[3], 2, Color.black);
  }

  private void DrawBigArc(Triangle t, int s, int d,  Color arcColor)
  {
    Vector3 point = _vertices[t.indices[s]];
    Vector3 point2 = _vertices[t.indices[d]];
    Quaternion r = Quaternion.FromToRotation(point, point2);
    float angle;
    Vector3 axis;
    r.ToAngleAxis(out angle, out axis);
    angle *= 1.33333333333f;
    float da = angle / 16f;
    axis = _vertices[t.indices[0]];
    Quaternion step = Quaternion.AngleAxis(da, axis);
    Debug.DrawLine(Vector3.zero, axis*1.1f, arcColor);
    for (int i = 0; i < 16; i++)
    {
      var nextPoint = step * point;
      Debug.DrawLine(point, nextPoint, arcColor);
      point = nextPoint;
    }
  }

  private void DrawDebugArc(Triangle t, int arcId, Color arcColor,float duration =0.0f)
  {
    Vector3 point = _vertices[t.indices[arcId]];
    float angle;
    Vector3 axis;
    t.arcs[arcId].ToAngleAxis(out angle, out axis);
    float da = angle/16f;
    Quaternion step = Quaternion.AngleAxis(da, axis);
    Debug.DrawLine(Vector3.zero, axis*1.1f, arcColor, duration);
    Debug.DrawLine(Vector3.zero, -axis*1.1f, arcColor, duration);
    for (int i = 0; i < 16; i++)
    {
      var nextPoint = step*point;
      Debug.DrawLine(point, nextPoint, arcColor, duration);
      point = nextPoint;
    }
  }

  public int[] GetMeshIndices()
  {
    List<int> result = new List<int>();
    foreach (var triangle in _topLevel)
    {
      triangle.GetIndices(result);
    }
    return result.ToArray();
  }

  public List<Vector3> GetVertices()
  {
    return _vertices.ToList();
  }

  public List<Vector2> GetUVs()
  {
    return _uvs.ToList();
  }
}
