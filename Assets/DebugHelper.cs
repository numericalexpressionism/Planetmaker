using UnityEngine;
static class DebugHelper
{
  public static void DrawPoint(Vector3 p, float a, Color color, float t = 0)
  {
    var fullTurn = Quaternion.FromToRotation(p, Vector3.forward);
    float angle;
    Vector3 axis;
    fullTurn.ToAngleAxis(out angle, out axis);
    var tangent = Quaternion.AngleAxis(a, axis);
    var bitangent = Quaternion.AngleAxis(a, Vector3.Cross(axis, p));
    Debug.DrawLine(p, tangent * p, color, t);
    Debug.DrawLine(p, bitangent * p, color, t);
    Debug.DrawLine(p, Quaternion.Inverse(tangent) * p, color, t);
    Debug.DrawLine(p, Quaternion.Inverse(bitangent) * p, color, t);
  }

  public static void DrawCircle(Vector3 p, float r, Color color, float steps = 16, float t = 0)
  {
    var fullTurn = Quaternion.FromToRotation(p, Vector3.forward);
    float angle;
    Vector3 axis;
    fullTurn.ToAngleAxis(out angle, out axis);
    var tangent = Quaternion.AngleAxis(r, axis);
    var rq = Quaternion.AngleAxis(360 / steps, p);
    var p1 = tangent * p;
    for (int i = 0; i < steps; i++)
    {
      var p2 = rq * p1;
      Debug.DrawLine(p1, p2, color);
      p1 = p2;
    }
  }

  public static void DrawArc(Vector3 start, Vector3 end, Color color, float steps = 16, float t = 0)
  {
    DrawArc(start, end, color, color, steps, t);
  }

  public static void DrawArc(Vector3 start, Vector3 end, Color colorf, Color colort, float steps = 16, float t = 0)
  {
    var q = Quaternion.FromToRotation(start, end);
    float angle;
    Vector3 axis;
    q.ToAngleAxis(out angle, out axis);
    q = Quaternion.AngleAxis(angle / steps, axis);
    var p1 = start;
    for (int i = 0; i < steps; i++)
    {
      var p2 = q * p1;
      Debug.DrawLine(p1, p2, Color.Lerp(colorf, colort, i / steps), t);
      p1 = p2;
    }
  }

  public static void DrawGeodesic(float th, Color color, int n = 36, float t = 0)
  {
    var q = Quaternion.AngleAxis(360f / n, Vector3.forward);
    var p1 = MathS.SphToCartesian(0, th);
    for (int i = 0; i < n; i++)
    {
      var p2 = q * p1;
      Debug.DrawLine(p1, p2, color, t);
      p1 = p2;
    }
  }

  public static void DrawQuatArrow(Quaternion quat, Vector3 Start, float arrowSize, Color color, int n = 2, float t = 0)
  {
    Vector3 Axis;
    float angle;
    quat.ToAngleAxis(out angle, out Axis);
    Vector3 end = quat * Start;

    Quaternion ArrowHead1Q = Quaternion.AngleAxis(30, end);
    Quaternion ArrowHead2Q = Quaternion.AngleAxis(-30, end);
    Quaternion ArrowheadQuat = Quaternion.AngleAxis(-arrowSize, Axis);
    Vector3 ArrowHeadEnd = ArrowheadQuat * end;

    DrawArc(end, ArrowHead1Q* ArrowHeadEnd, color, 1, t);//line body
    DrawArc(end, ArrowHead2Q* ArrowHeadEnd, color, 1, t);//line body

    DrawArc(Start, end, color, n, t);//line body
  }
}