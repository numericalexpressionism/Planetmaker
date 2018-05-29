﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class VoronoiIterative : MonoBehaviour
{
  public int pointCount = 1000;
  private readonly List<Vector2> _points = new List<Vector2>();
  private readonly LinkedList<BeachLineArc> _beachline = new LinkedList<BeachLineArc>();
  private readonly PriorityQueue<VoronoiEvent> _eventQueue = new PriorityQueue<VoronoiEvent>(p=>p.GetLat());

  private readonly Stopwatch _sw = new Stopwatch();

  private VoronoiGraph _result;
  private Coroutine _buildHandle;

  [UsedImplicitly]
  void OnEnable()
  {
    _result = new VoronoiGraph();
    for (int index = 0; index < pointCount; index++)
    {
      _points.Add(MathS.CartesianToSph(Random.onUnitSphere));
    }
    _sw.Start();
    _points.Sort((p1, p2) => Comparer<float>.Default.Compare(p1.y, p2.y));
    _sw.Stop();

    Debug.LogFormat("sorted {0} points in {1} msec", _points.Count, _sw.ElapsedMilliseconds);

    foreach (var site in _points)
    {
      _eventQueue.Enqueue(new PointEvent(site));
    }

    _sw.Reset();
    _sw.Start();
    _buildHandle = StartCoroutine(BuildVoronoi(_result));
  }

  [UsedImplicitly]
  void OnDisable()
  {
    if(_buildHandle != null) StopCoroutine(_buildHandle);
  }

  private IEnumerator BuildVoronoi(VoronoiGraph result)
  {
    const float dt = 1/15f; //15fps
    yield return new WaitForSeconds(0.1f);
    float lastYieldTime = Time.realtimeSinceStartup;
    while (_eventQueue.Count > 0)
    {
      var nextEvent = _eventQueue.Dequeue();

      if (nextEvent.IsDeleted)
      {
        continue;
      }

      nextEvent.Process(_beachline, _eventQueue, result);

      var elapsed = Time.realtimeSinceStartup - lastYieldTime;
      if (elapsed <= dt)
      {
        continue;
      }
      lastYieldTime = Time.realtimeSinceStartup;

      var directrixTh = nextEvent.GetLat();

      if (pointCount > 10000)
      {
        DebugHelper.DrawGeodesic(directrixTh, Color.magenta);
      }
      else
      {
        foreach (var arc in _beachline)
        {
          arc.DrawDebug(directrixTh, dt, Color.red);
        }
      }

      yield return new WaitForSeconds(0.0f);
    }
    _buildHandle = null;
    _sw.Stop();
    Debug.Log("processed in msec:"+_sw.ElapsedMilliseconds);
  }

  // Update is called once per frame
  [UsedImplicitly]
  void Update()
  {
    if (_buildHandle == null)
    {
      foreach (var point in _points)
      {
        DebugHelper.DrawPoint(MathS.SphToCartesian(point), 0.1f, Color.black);
      }
      _result.DrawDebug();
    }
  }
}
