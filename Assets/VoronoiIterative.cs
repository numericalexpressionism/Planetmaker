using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class VoronoiIterative : MonoBehaviour
{
  public int PointCount = 1000;
  private List<Vector2> points = new List<Vector2>();
  private readonly LinkedList<BeachLineArc> _beachline = new LinkedList<BeachLineArc>();
  private readonly PriorityQueue<VoronoiEvent> _eventQueue = new PriorityQueue<VoronoiEvent>(p=>p.GetLat());

  private VoronoiGraph _result;
  private Coroutine _buildHandle;

  public float lstep = 0.1f;
  public float tstep = 0.1f;
  public int circRes = 48;
  public bool regenerate;

  private Stopwatch sw = new Stopwatch();

  [UsedImplicitly]
  void OnEnable()
  {
    _result = new VoronoiGraph();
    if (regenerate || true)
    {
      for (int index = 0; index < PointCount; index++)
      {
        points.Add(MathS.CartesianToSph(Random.onUnitSphere));
      }
    }
    points.Sort((p1, p2) => Comparer<float>.Default.Compare(p1.y, p2.y));

    Debug.LogFormat("processing {0} points", points.Count);

    foreach (var site in points)
    {
      _eventQueue.Enqueue(new PointEvent(site), true);
    }

    sw.Start();
    _buildHandle = StartCoroutine(BuildVoronoi(_result));
  }

  [UsedImplicitly]
  void OnDisable()
  {
    if(_buildHandle != null) StopCoroutine(_buildHandle);
  }

  private IEnumerator BuildVoronoi(VoronoiGraph result)
  {
    yield return new WaitForSeconds(0.1f);
    float LastTime = Time.realtimeSinceStartup;
    while (_eventQueue.Count > 0)
    {
      var nextEvent = _eventQueue.Dequeue();

      if (nextEvent._deleted)
      {
        continue;
      }

     // nextEvent.DrawDebug();
      nextEvent.Process(_beachline, _eventQueue, result);

      float passed = Time.realtimeSinceStartup - LastTime;
      if (passed > 0.03333333f)
      {
        LastTime = Time.realtimeSinceStartup;

        var directrixTh = nextEvent.GetLat();

        if (PointCount > 10000)
        {
          DebugHelper.DrawGeodesic(directrixTh, Color.magenta);
        }
        else
        {
          foreach (var arc in _beachline)
          {
            arc.DrawDebug(directrixTh, tstep, Color.red);
          }
        }

        yield return new WaitForSeconds(0.0f);
      }
      
    }
    _buildHandle = null;
    sw.Stop();
    Debug.Log("processed in msec:"+sw.ElapsedMilliseconds);
  }

  // Update is called once per frame
  [UsedImplicitly]
  void Update()
  {
    if (_buildHandle == null)
    {
      foreach (var point in points)
      {
        DebugHelper.DrawPoint(MathS.SphToCartesian(point), 0.1f, Color.black);
      }
      _result.DrawDebug();
    }
  }
}
