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

  public static Vector3 SphToCartesian(float phi, float theta)
  {
    return SphToCartesianRad(phi * Mathf.Deg2Rad, theta * Mathf.Deg2Rad);
  }

  public static Vector3 SphToCartesianRad(float phi, float theta)
  {
    return new Vector3(
      Mathf.Sin(theta) * Mathf.Cos(phi),
      Mathf.Sin(theta) * Mathf.Sin(phi),
      Mathf.Cos(theta)
      );
  }
  public static Vector2 CartesianToSph(Vector3 c)
  {
    return new Vector2(Mathf.Atan2(c.y, c.x) * Mathf.Rad2Deg, Mathf.Acos(c.z) * Mathf.Rad2Deg);
  }

  public static void CalcParabolaIntersections(Vector2 focusL, Vector2 focusR, float directrixTh, out float x1, out float x2)
  {
    CalcParabolaIntersections(focusL.y, focusR.y, directrixTh, focusL.x, focusR.x, out x1, out x2);
  }
  public static void CalcParabolaIntersections(float focusTh1, float focusTh2, float directrixTh, float focusPhi1, float focusPhi2, out float r1, out float r2)
  {
    if (Mathf.Approximately(focusTh1, focusTh2) && Mathf.Approximately(focusPhi1, focusPhi2))
    {
      //points are equal - all ellips is intersection point
      r1 = focusPhi1 - 90;
      r2 = focusPhi1 + 90;
      return;
    }

    if (Mathf.Approximately(focusTh1, directrixTh))
    {
      r1 = r2 = focusPhi1;
      return;
    }

    if (Mathf.Approximately(focusTh2, directrixTh))
    {
      r1 = r2 = focusPhi2;
      return;
    }

    var eps = directrixTh * Mathf.Deg2Rad;
    var th1 = focusTh1 * Mathf.Deg2Rad;
    var th2 = focusTh2 * Mathf.Deg2Rad;
    var ph1 = focusPhi1 * Mathf.Deg2Rad;
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

    r1 = 2 * Mathf.Atan2(u + s, t + v) * Mathf.Rad2Deg;
    r2 = 2 * Mathf.Atan2(u - s, t + v) * Mathf.Rad2Deg;
  }

  public static float ParabolaIntersectionPointFi(Vector2 l, Vector2 r, float th)
  {
    float x1, x2;

    //this orders point so the intersecting one comes first
    CalcParabolaIntersections(l, r, th, out x1, out x2);

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

  public static Vector3 CalcParabolaPoint(Vector2 focus, float directrixTheta, float phi)
  {
    return CalcParabolaPoint(focus.y, focus.x, directrixTheta, phi);
  }

  public static Vector3 CalcParabolaPoint(float focusTh, float focusPhi, float directrixTheta, float directrixPhi)
  {
    var th = CalcParabolaThetaRad(focusTh, focusPhi, directrixTheta, directrixPhi);
    return -SphToCartesianRad(directrixPhi * Mathf.Deg2Rad, th);
  }

  private static float CalcParabolaThetaRad(float focusTh, float focusPhi, float directrixTheta, float directrixPhi)
  {
    var eps = directrixTheta * Mathf.Deg2Rad;
    var th1 = focusTh * Mathf.Deg2Rad;
    var ph1 = focusPhi * Mathf.Deg2Rad;
    var phd = directrixPhi * Mathf.Deg2Rad;

    var y = Mathf.Cos(eps) - Mathf.Cos(th1);
    var x = Mathf.Sin(th1) * Mathf.Cos(phd - ph1) - Mathf.Sin(eps);
    var th = Mathf.Atan2(y, x);
    return th;
  }
}
