using System;
using Assets;
using UnityEngine;

public static class MathS
{

  // y-up solutions
  //public static Vector3 LatLngToCartesian(Vector2 latlng)
  //{
  //  float latR = latlng.x * Mathf.Deg2Rad;
  //  float lngR = latlng.y * Mathf.Deg2Rad;
  //  return new Vector3(
  //    Mathf.Cos(latR) * Mathf.Cos(lngR),
  //    Mathf.Sin(latR),
  //    Mathf.Cos(latR) * Mathf.Sin(lngR)
  //    );
  //}
  //
  //public static Vector2 CartesianToLatLng(Vector3 cartesian)
  //{
  //  float theta = Mathf.Acos(cartesian.y);
  //  float phi = Mathf.Atan2(cartesian.z, cartesian.x);
  //  return new Vector2(90 - theta * Mathf.Rad2Deg, phi * Mathf.Rad2Deg);


  //z-up solutions

  public static Vector3 GetCircleCentreCartesian(Vector2 p1, Vector2 p2, Vector2 p3, out float r)
  {
    var v0 = SphToCartesian(p1);
    var v1 = SphToCartesian(p2);
    var v2 = SphToCartesian(p3);
    var cc = Vector3.Cross((v0 - v1) * 65536, (v2 - v0) * 65536);
    var c = cc.normalized;
    r = Mathf.Acos(Vector3.Dot(c, v1)) * Mathf.Rad2Deg;
    //if (r > 90)
    //{
    //  c = -c;//select nearest point from two;
    //  r = 180 - r;
    //}
    return c;
  }

  public static Vector2 GetCircleCentre(Vector2 p1, Vector2 p2, Vector2 p3)
  {
    float r;
    var c = GetCircleCentreCartesian(p1, p2, p3, out r);
    return CartesianToSph(c);
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

    //var x1 = CalcParabolaPoint(focusTh2, focusPhi2, directrixTh, r1);
    //var x2 = CalcParabolaPoint(focusTh2, focusPhi2, directrixTh, r2);
    //
    //DebugHelper.DrawPoint(x1, 3, Color.red);
    //DebugHelper.DrawPoint(x2, 3, Color.blue);
  }

  public static void CalcParabolaIntersectionsV(float focusTh1, float focusTh2, float directrixTh, float focusPhi1, float focusPhi2, out float r1, out float r2)
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

    var a = Mathf.Cos(th1); //p1.z
    var b = Mathf.Cos(th2); //p2.z
    var c = Mathf.Sin(th1); //k1
    var d = Mathf.Sin(th2); //k2
    var e = Mathf.Sin(ph1); //p1.y / k1
    var f = Mathf.Sin(ph2); //p2.y / k2
    var g = Mathf.Cos(ph1); //p1.x / k1
    var h = Mathf.Cos(ph2); //p2.x / k2
    var n = Mathf.Cos(eps); //directrix.z
    var m = Mathf.Sin(eps); //dirR

    var p = d * (n - a); //k2 * (dirz - p1.z)
    var q = c * (n - b); //k1 * (dirz - p2.z)
    var t = m * (b - a); //dirK * (p2.z - p1.z)

    var u = p * f - q * e; //   
    var v = p * h - q * g;

    var s = Mathf.Sqrt(u * u + v * v - t * t);

    r1 = 2 * Mathf.Atan2(u + s, t + v) * Mathf.Rad2Deg;
    r2 = 2 * Mathf.Atan2(u - s, t + v) * Mathf.Rad2Deg;

    //var x1 = CalcParabolaPoint(focusTh2, focusPhi2, directrixTh, r1);
    //var x2 = CalcParabolaPoint(focusTh2, focusPhi2, directrixTh, r2);
    //
    //DebugHelper.DrawPoint(x1, 3, Color.red);
    //DebugHelper.DrawPoint(x2, 3, Color.blue);
  }

  public static void CalcParabolaIntersectionsOld(float focusTh, float focusTh2, float directrixTh, float focusPhi, float focusPhi2, out float r1, out float r2)
  {
    if (Mathf.Approximately(focusTh, focusTh2) && Mathf.Approximately(focusPhi, focusPhi2))
    {
      //points are equal - all ellips is intersection point
      r1 = focusPhi - 90;
      r2 = focusPhi + 90;
    }

    if (Mathf.Approximately(focusTh, directrixTh))
    {
      r1 = r2 = focusPhi;
      return;
    }

    if (Mathf.Approximately(focusTh2, directrixTh))
    {
      r1 = r2 = focusPhi2;
      return;
    }

    const double Rad2Deg = 180 / Math.PI;
    const double DegToRad = 180 / Math.PI;

    double eps = directrixTh * DegToRad;
    double th1 = focusTh * DegToRad;
    double th2 = focusTh2 * DegToRad;
    double ph1 = focusPhi * DegToRad;
    double ph2 = focusPhi2 * DegToRad;

    double a = Math.Cos(th1);
    double b = Math.Cos(th2);
    double c = Math.Sin(th1);
    double d = Math.Sin(th2);
    double e = Math.Sin(ph1);
    double f = Math.Sin(ph2);
    double g = Math.Cos(ph1);
    double h = Math.Cos(ph2);
    double n = Math.Cos(eps);
    double m = Math.Sin(eps);

    double p = d * (n - a);
    double q = c * (n - b);
    double t = m * (b - a);

    double u = p * f - q * e;
    double v = p * h - q * g;

    var k = u * u + v * v - t * t;
    if (k < 0)
    {
      Debug.LogError("K < 0");
    }
    double s = Math.Sqrt(k);

    r1 = NormalizeAngle((float)(2 * Math.Atan2(u + s, t + v) * Rad2Deg));
    r2 = NormalizeAngle((float)(2 * Math.Atan2(u - s, t + v) * Rad2Deg));
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
    
    var epsilon = Mathf.Epsilon;
    if (Mathf.Abs(start - end) < epsilon)
    {
      return Mathf.Abs(v - end) < epsilon || Mathf.Abs(v - start) < epsilon;
    }

    var d = MathS.DeltaAngle(start, end);
    if (d < 0) { d += 360; }
    var d2 = MathS.DeltaAngle(start, v);
    if (d2 < 0) { d2 += 360; }
    return d2 <= d;
  }

  public static float DeltaAngle(float start, float end)
  {
    //mathf DeltaAngle loses precision due to divisions
    var num = end - start;
    while (num < 0) num += 360;
    while (num > 180) num -= 360;
    return num;
  }

  public static Vector3 CalcParabolaPoint(Vector2 focus, float directrixTheta, float phi)
  {
    return CalcParabolaPoint(focus.y, focus.x, directrixTheta, phi);
  }

  public static float CalcParabolaTheta(Vector2 focus, float directrixTheta, float phi)
  {
    return CalcParabolaThetaRad(focus.y, focus.x, directrixTheta, phi) * Mathf.Rad2Deg;
  }
  public static Vector3 CalcParabolaPoint(float focusTh, float focusPhi, float directrixTheta, float directrixPhi)
  {
    var th = CalcParabolaThetaRad(focusTh, focusPhi, directrixTheta, directrixPhi);
    return -SphToCartesianRad(directrixPhi * Mathf.Deg2Rad, th);
  }

  private static float CalcParabolaThetaRad(float focusTh, float focusPhi, float directrixTheta,
    float directrixPhi)
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
