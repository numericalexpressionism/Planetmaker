using Assets;
using UnityEngine;

public class SphereParabolaView : MonoBehaviour
{
  [Range(0, 180)]
  public float focucTheta1 = 30;
  [Range(0, 360)]
  public float focucPhi1 = 0;

  [Range(0, 180)]
  public float focucTheta2 = 30;
  [Range(0, 360)]
  public float focucPhi2 = 0;

  [Range(0, 180)]
  public float focucTheta3 = 30;
  [Range(0, 360)]
  public float focucPhi3 = 0;

  [Range(0, 180)]
  public float directrixTheta = 45;

  [Range(0, 180)]
  public float directrixTheta2 = 45;

  [Range(0, 180)]
  public float circleTheta = 45;
  [Range(0, 360)]
  public float circlePhi = 45;

  Vector3 SphToCartesian(float phi, float theta)
  {
    return SphToCartesianRad(phi * Mathf.Deg2Rad, theta * Mathf.Deg2Rad);
  }

  Vector3 SphToCartesianRad(float phi, float theta)
  {
    return new Vector3(
      Mathf.Sin(theta) * Mathf.Cos(phi),
      Mathf.Sin(theta) * Mathf.Sin(phi),
      Mathf.Cos(theta)
      );
  }

  // Use this for initialization
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    var dPhi = 5;
    for (int i = 0; i <= 360; i += dPhi)
    {
      float phiA = i - dPhi;
      float phiB = i;

      var f1 = SphToCartesian(focucPhi1, focucTheta1);
      var f2 = SphToCartesian(focucPhi2, focucTheta2);
      var f3 = SphToCartesian(focucPhi3, focucTheta3);
      var d1 = SphToCartesian(phiA, directrixTheta);
      var d2 = SphToCartesian(phiB, directrixTheta);

      var p11 = CalcParabolaPoint(d1, f1);
      var p21 = CalcParabolaPoint(d2, f1);

      var p12 = CalcParabolaPoint(focucTheta2, focucPhi2, directrixTheta, phiA);
      var p22 = CalcParabolaPoint(focucTheta2, focucPhi2, directrixTheta, phiB);

      var p13 = CalcParabolaPoint(d1, f3);
      var p23 = CalcParabolaPoint(d2, f3);

      Vector3 ip1;
      Vector3 ip2;

      CalcParabolaIntersections(focucTheta1, focucTheta2, directrixTheta, focucPhi1, focucPhi2);

      // DebugDrawPoint(IP1, 3, Color.red);
      // DebugDrawPoint(IP2, 3, Color.magenta);

      DebugHelper.DrawArc(MathS.SphToCartesian(0, 0), MathS.SphToCartesian(focucPhi1, directrixTheta), Color.black);
      DebugHelper.DrawArc(MathS.SphToCartesian(0, 0), MathS.SphToCartesian(focucPhi2, directrixTheta), Color.black);
      DebugHelper.DrawArc(MathS.SphToCartesian(0, 0), MathS.SphToCartesian(circlePhi, directrixTheta), Color.black);
      DebugHelper.DrawPoint(MathS.SphToCartesian(circlePhi, circleTheta), 2, Color.black);
      DebugHelper.DrawCircle(MathS.SphToCartesian(circlePhi, circleTheta), directrixTheta - circleTheta, Color.black, 32);
      DebugDrawPoint(MathS.SphToCartesian(0, 0), 2, Color.black);
      DebugDrawPoint(f1, 2, Color.blue);
      DebugDrawPoint(f2, 2, Color.green);
      //DebugDrawPoint(F3, 2, Color.black);

      Debug.DrawLine(p11, p21, Color.blue);
      Debug.DrawLine(p12, p22, Color.green);
      Debug.DrawLine(p13, p23, Color.black);

      Debug.DrawLine(d1, d2, Color.red);

      var b1 = SphToCartesian(phiA, directrixTheta2);
      var b2 = SphToCartesian(phiB, directrixTheta2);
      Debug.DrawLine(b1, b2, Color.magenta);
    }
  }

  private void CalcParabolaIntersections(float focusTh, float focusTh2, float directrixTh, float focusPhi, float focusPhi2)
  {
    var eps = directrixTh * Mathf.Deg2Rad;
    var th1 = focusTh * Mathf.Deg2Rad;
    var th2 = focusTh2 * Mathf.Deg2Rad;
    var ph1 = focusPhi * Mathf.Deg2Rad;
    var ph2 = focusPhi2 * Mathf.Deg2Rad;

    var a = Mathf.Cos(th1);
    var b = Mathf.Cos(th2);
    var c = Mathf.Sin(th1);
    var d = Mathf.Sin(th2);
    var e = Mathf.Sin(ph1);
    var f = Mathf.Sin(ph2);
    var g = Mathf.Cos(ph1);
    var h = Mathf.Cos(ph2);
    var n = Mathf.Cos(eps);
    var m = Mathf.Sin(eps);

    var p = d * (n - a);
    var q = c * (n - b);
    var t = m * (b - a);

    var u = p * f - q * e;
    var v = p * h - q * g;

    var s = Mathf.Sqrt(u * u + v * v - t * t);

    var r1 = 2 * Mathf.Atan2(u + s, t + v);
    var r2 = 2 * Mathf.Atan2(u - s, t + v);

    var x1 = CalcParabolaPoint(focusTh2, focusPhi2, directrixTheta, r1 * Mathf.Rad2Deg);
    var x2 = CalcParabolaPoint(focusTh2, focusPhi2, directrixTheta, r2 * Mathf.Rad2Deg);

    DebugHelper.DrawPoint(x1, 3, Color.red);
    DebugHelper.DrawPoint(x2, 3, Color.blue);
  }

  private void DebugDrawBigCircle(Vector3 a, Vector3 b, Color color, int steps = 32)
  {
    var q = Quaternion.FromToRotation(a, b);
    float an;
    Vector3 ax;
    q.ToAngleAxis(out an, out ax);
    float step = 360f / steps;
    q = Quaternion.AngleAxis(step, ax);

    Vector3 s = a;
    for (int i = 0; i < steps; i++)
    {
      Vector3 d = q * s;
      Debug.DrawLine(s, d, color);
      s = d;
    }
  }

  private void DebugDrawPoint(Vector3 p, float a, Color color)
  {
    var fullTurn = Quaternion.FromToRotation(p, Vector3.up);
    float angle;
    Vector3 axis;
    fullTurn.ToAngleAxis(out angle, out axis);
    var tangent = Quaternion.AngleAxis(a, axis);
    var bitangent = Quaternion.AngleAxis(a, Vector3.Cross(axis, p));
    Debug.DrawLine(p, tangent * p, color);
    Debug.DrawLine(p, bitangent * p, color);
    Debug.DrawLine(p, Quaternion.Inverse(tangent) * p, color);
    Debug.DrawLine(p, Quaternion.Inverse(bitangent) * p, color);
  }

  private static Vector3 CalcParabolaPoint(float focusTh, float focusPhi, float directrixTh, float directrixPhi)
  {
    var eps = directrixTh * Mathf.Deg2Rad;
    var th1 = focusTh * Mathf.Deg2Rad;
    var ph1 = focusPhi * Mathf.Deg2Rad;
    var phd = directrixPhi * Mathf.Deg2Rad;

    var y = Mathf.Cos(eps) - Mathf.Cos(th1);
    var x = Mathf.Sin(th1) * Mathf.Cos(phd - ph1) - Mathf.Sin(eps);
    var th = Mathf.Atan2(y, x);
    return -MathS.SphToCartesianRad(phd, th);
  }

  private static Vector3 CalcParabolaPoint(Vector3 d, Vector3 f)
  {
    var a = d.y / d.x;
    var q = f - d;
    var b = (q.x + a * q.y) / (-q.z);
    var r = 1 / Mathf.Sqrt(1 + a * a + b * b);
    if (d.x < 0) { r *= -1; }
    return new Vector3(1, a, b) * r;
  }
}
