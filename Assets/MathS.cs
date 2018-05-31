using System;
using UnityEngine;

public static class MathS
{
  //z-up solutions. In unity everything will be aligned with front view. But i don't care.

  public static Vector3 GetCircleCentreCartesian(Vector2 p1, Vector2 p2, Vector2 p3, out float r)
  {
    var v0 = SphToCartesian(p1);
    var v1 = SphToCartesian(p2);
    var v2 = SphToCartesian(p3);
    var cc = Vector3.Cross((v0 - v1) * 65536, (v2 - v0) * 65536);
    var c = cc.normalized;
    r = Mathf.Acos(Vector3.Dot(c, v1)) * Mathf.Rad2Deg;
    return c;
  }

  public static Vector3 SphToCartesian(Vector2 ploar)
  {
    return SphToCartesianRad(ploar.x * Mathf.Deg2Rad, ploar.y * Mathf.Deg2Rad);
  }

  public static Vector3 SphToCartesian(double phi, double theta)
  {
    return SphToCartesianRad(phi * Constants.Deg2Rad, theta * Constants.Deg2Rad);
  }

  public static Vector3 SphToCartesianRad(double phi, double theta)
  {
    return new Vector3(
      (float)(Math.Sin(theta) * Math.Cos(phi)),
      (float)(Math.Sin(theta) * Math.Sin(phi)),
      (float)(Math.Cos(theta))
      );
  }
  public static Vector2 CartesianToSph(Vector3 c)
  {
    return new Vector2((float)(Math.Atan2(c.y, c.x) * Mathf.Rad2Deg), (float)(Math.Acos(c.z) * Mathf.Rad2Deg));
  }

  public static void CalcParabolaIntersections(Vector2 focusL, Vector2 focusR, float directrixTh, out float x1, out float x2)
  {
    CalcParabolaIntersections(focusL.y, focusR.y, directrixTh, focusL.x, focusR.x, out x1, out x2);
  }

  public static float CalcLeftParabolaIntersection(Vector2 focusL, Vector2 focusR, float directrixTh)
  {
    return CalcLeftParabolaIntersection(focusL.y, focusR.y, directrixTh, focusL.x, focusR.x);
  }

  public static float CalcLeftParabolaIntersection(float focusTh1, float focusTh2, float directrixTh, float focusPhi1, float focusPhi2)
  {
    if ((Math.Abs(focusTh1 - focusTh2) < Constants.Epsilon) && (Math.Abs(focusPhi1 - focusPhi2) < Constants.Epsilon))
    {
      //points are equal - all ellips is intersection point
      return focusPhi1 - 90;
    }

    if ((Math.Abs(focusTh1 - directrixTh) < Constants.Epsilon) || (Math.Abs(focusTh2 - directrixTh) < Constants.Epsilon))
    {
      return focusPhi1;
    }

    var eps = directrixTh * Constants.Deg2Rad;
    var th1 = focusTh1 * Constants.Deg2Rad;
    var th2 = focusTh2 * Constants.Deg2Rad;
    var ph1 = focusPhi1 * Constants.Deg2Rad;
    var ph2 = focusPhi2 * Constants.Deg2Rad;

    var a = Math.Cos(th1);
    var b = Math.Cos(th2);
    var c = Math.Sin(th1);
    var d = Math.Sin(th2);
    var e = Math.Sin(ph1);
    var f = Math.Sin(ph2);
    var g = Math.Cos(ph1);
    var h = Math.Cos(ph2);
    var n = Math.Cos(eps);
    var m = Math.Sin(eps);

    var p = d * (n - a);
    var q = c * (n - b);
    var t = m * (b - a);

    var u = p * f - q * e;
    var v = p * h - q * g;

    var s = Math.Sqrt(u * u + v * v - t * t);

    return (float)(2* Math.Atan2(u + s, t + v) * Constants.Rad2Deg);
  }

  public static void CalcParabolaIntersections(float focusTh1, float focusTh2, float directrixTh, float focusPhi1, float focusPhi2, out float r1, out float r2)
  {
    if ((Math.Abs(focusTh1 - focusTh2) < Constants.Epsilon) && (Math.Abs(focusPhi1 - focusPhi2) < Constants.Epsilon))
    {
      //points are equal - all ellips is intersection point
      r1 = focusPhi1 - 90;
      r2 = focusPhi1 + 90;
      return;
    }

    if ((Math.Abs(focusTh1 - directrixTh) < Constants.Epsilon) || (Math.Abs(focusTh2 - directrixTh) < Constants.Epsilon))
    {
      r1 = r2 = focusPhi1;
      return;
    }

    var eps = directrixTh * Mathf.Deg2Rad;
    var th1 = focusTh1 * Mathf.Deg2Rad;
    var th2 = focusTh2 * Mathf.Deg2Rad;
    var ph1 = focusPhi1 * Mathf.Deg2Rad;
    var ph2 = focusPhi2 * Mathf.Deg2Rad;

    var a = Math.Cos(th1);
    var b = Math.Cos(th2);
    var c = Math.Sin(th1);
    var d = Math.Sin(th2);
    var e = Math.Sin(ph1);
    var f = Math.Sin(ph2);
    var g = Math.Cos(ph1);
    var h = Math.Cos(ph2);
    var n = Math.Cos(eps);
    var m = Math.Sin(eps);

    var p = d * (n - a);
    var q = c * (n - b);
    var t = m * (b - a);

    var u = p * f - q * e;
    var v = p * h - q * g;

    var s = Math.Sqrt(u * u + v * v - t * t);

    r1 = (float)(2 * Math.Atan2(u + s, t + v) * Constants.Rad2Deg);
    r2 = (float)(2 * Math.Atan2(u - s, t + v) * Constants.Rad2Deg);
  }

  public static float ParabolaIntersectionPointFi(Vector2 l, Vector2 r, float th)
  {
    //this orders point so the intersecting one comes first
    float x1 = CalcLeftParabolaIntersection(l, r, th);

    while (x1 < 0) x1 += 360f;
    while (x1 > 360f) x1 -= 360f;

    return x1;
  }

  public static float NormalizeAngle(float p)
  {
    while (p < 0)
    {
      p += 360;
    }
    while (p > 360)
    {
      p -= 360;
    }
    return p;
  }

  public static bool IsOnArc(float v, float start, float end)
  {
    float f = start;
    float t = end;
    if (f < 0) f += 360;
    while (t < f) t += 360;
    while (v < f) v += 360;
    if (v > 720) v -= 360;
    return v >= f && v < t;
  }

  public static Vector3 CalcParabolaPoint(Vector2 focus, double directrixTheta, double phi)
  {
    return CalcParabolaPoint(focus.y, focus.x, directrixTheta, phi);
  }

  public static Vector3 CalcParabolaPoint(double focusTh, double focusPhi, double directrixTheta, double directrixPhi)
  {
    var th = CalcParabolaThetaRad(focusTh, focusPhi, directrixTheta, directrixPhi);
    return -SphToCartesianRad(directrixPhi * Constants.Deg2Rad, th);
  }

  private static double CalcParabolaThetaRad(double focusTh, double focusPhi, double directrixTheta, double directrixPhi)
  {
    var eps = directrixTheta * Constants.Deg2Rad;
    var th1 = focusTh * Constants.Deg2Rad;
    var ph1 = focusPhi * Constants.Deg2Rad;
    var phd = directrixPhi * Constants.Deg2Rad;

    var y = Math.Cos(eps) - Math.Cos(th1);
    var x = Math.Sin(th1) * Math.Cos(phd - ph1) - Math.Sin(eps);
    var th = Math.Atan2(y, x);
    return th;
  }
}
