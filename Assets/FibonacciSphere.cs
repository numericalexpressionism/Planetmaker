using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class DataPoint
{
  public DataPoint(Vector2 geoPos)
  {
    this.geoPos = geoPos;
  }

  public Vector2 geoPos;
  public Vector3 motionAxis;
  public Vector3 spd;
  public HashSet<DataPoint> links = new HashSet<DataPoint>();
}

public class FibonacciSphere : MonoBehaviour
{
  private List<DataPoint> _points;
  public int num;
  public int bandCount;
  public bool randomize;

  public float r = 4.0f;
  public float l = 4.0f;
  private List<List<DataPoint>> _sortedPoints;

  // Use this for initialization
  void OnEnable()
  {
    //_points = CreatePoints(Num, Randomize);
    _points = RandomPoints(num, randomize);
    float bandWidth = 180f / bandCount;

    _sortedPoints = new List<List<DataPoint>>(bandCount);
    for (int i = 0; i < bandCount; i++)
    {
      _sortedPoints.Add(new List<DataPoint>());
    }

    for (int index = 0; index < _points.Count; index++)
    {
      var point = _points[index];
      int bandId = Mathf.FloorToInt((point.geoPos.x + 90f) / bandWidth);
      _sortedPoints[bandId].Add(point);
    }

    for (int i = 0; i < bandCount; i++)
    {
      _sortedPoints[i].Sort((a, b) => a.geoPos.y.CompareTo(b.geoPos.y));
    }

    StartCoroutine(BuildLinks());
  }

  class Triangle
  {
    
  }

  public IEnumerator BuildLinks()
  {
    yield return new WaitForEndOfFrame();
  }

  private List<DataPoint> RandomPoints(int num, bool randomize)
  {
    var randomPoints = new List<DataPoint>();
    for (int i = 0; i < num; i++)
    {
      randomPoints.Add(new DataPoint(MathS.CartesianToSph(UnityEngine.Random.onUnitSphere)));
    }
    return randomPoints;
  }

  public List<Vector3> CreatePoints(int samples, bool randomize)
  {
    var rnd = 1.0f;
    if (randomize)
    {
      rnd = UnityEngine.Random.Range(1, samples);
    }

    List<Vector3> points = new List<Vector3>();
    double offset = 2.0 / samples;
    double increment = Mathf.PI * (3.0 - Mathf.Sqrt(5));

    for (int i = 0; i < samples; i++)
    {
      double y = ((i * offset) - 1) + (offset / 2);
      double r = Math.Sqrt(1 - Math.Pow(y, 2));

      double phi = ((i + rnd) % samples) * increment;

      double x = Math.Cos(phi) * r;
      double z = Math.Sin(phi) * r;

      points.Add(new Vector3((float)x, (float)y, (float)z));
    }
    return points;
  }

  // Update is called once per frame
  void Update()
  {
    for (int b = 0; b < _sortedPoints.Count; b++)
    {
      var band = _sortedPoints[b];
      Color bandColor = Color.red * b / _sortedPoints.Count;

      for (int i = 0; i < band.Count; i++)
      {
        var point = band[i];
        Color pointColor = Color.blue*((float)i/band.Count);
        Vector3 p = MathS.SphToCartesian(point.geoPos);
        var color = bandColor + pointColor;
        color.a = 1.0f;
        Debug.DrawLine(p, p*1.01f, color);
      }

      //foreach (var oth in node.Links)
      //{
      //  Debug.DrawLine(node.Pos, oth.Pos, Color.blue);
      //}
    }


    // DrawCircleAroundPoint();
  }

  private void DrawCircleAroundPoint()
  {
    var tp = UnityEngine.Random.onUnitSphere;
    Debug.DrawLine(tp, tp * 1.5f, Color.red);

    var center = Quaternion.FromToRotation(Vector3.forward, tp);
    float angle;
    Vector3 axis;
    center.ToAngleAxis(out angle, out axis); //axis now lies on big circle
    Debug.DrawLine(axis, axis * 1.40f, Color.green);

    Vector3 circlePointV = Quaternion.AngleAxis(r + angle, axis) * Vector3.forward;
    Debug.DrawLine(circlePointV, circlePointV * 1.40f, Color.magenta);

    var step = Quaternion.AngleAxis(360 / 16f, tp);

    for (int i = 0; i <= 16; i++)
    {
      var next = step * circlePointV;
      Debug.DrawLine(circlePointV, next, Color.blue);
      circlePointV = next;
    }
  }
}
