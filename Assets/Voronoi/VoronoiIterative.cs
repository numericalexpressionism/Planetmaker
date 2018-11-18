using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class VoronoiIterative : MonoBehaviour
{
  public int pointCount = 1000;
  public float SelectorR = 12;
  public float PlateNum = 12;
  private readonly List<Vector2> _points = new List<Vector2>();
  private readonly LinkedList<BeachLineArc> _beachline = new LinkedList<BeachLineArc>();
  private readonly PriorityQueue<VoronoiEvent> _eventQueue = new PriorityQueue<VoronoiEvent>(p => p.GetLat());

  private readonly List<HashSet<VoronoiNode>> clusters = new List<HashSet<VoronoiNode>>();

  private readonly Stopwatch _sw = new Stopwatch();

  private VoronoiEdgesList _result;
  private VoronoiGraph _graph;
  private VoronoiNode _currentselection;
  private List<VoronoiNode> _neighbours;
  private int _nextSelectionIndex = 1;
  private Coroutine _buildHandle;

  [UsedImplicitly]
  void OnEnable()
  {
    _result = new VoronoiEdgesList();
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

    var steps = 12;
    GetComponent<MeshFilter>().mesh = MeridianSphere.Create(0.5f, steps * 4, steps * 2 - 1);


    _sw.Start();
    _buildHandle = StartCoroutine(BuildVoronoi(_result));
  }

  [UsedImplicitly]
  void OnDisable()
  {
    if (_buildHandle != null) StopCoroutine(_buildHandle);
  }

  private IEnumerator BuildVoronoi(VoronoiEdgesList result)
  {
    const float dt = 1 / 15f; //15fps
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
    Debug.Log("processed in msec:" + _sw.ElapsedMilliseconds);

    _graph = _result.CompileGraph();
    _currentselection = _graph.First();

    _graph.SetValue(GetNoise);

    var mainTexture = new Texture2D(256, 128) { filterMode = FilterMode.Point };
    _graph.RenderEquirectangular(mainTexture);
    GetComponent<MeshRenderer>().material.mainTexture = mainTexture;

    HashSet<VoronoiNode> seen = new HashSet<VoronoiNode>();
    for (int i = 0; i < PlateNum; i++)
    {
      var n = Random.Range(0, _points.Count);
      var newCluster = new HashSet<VoronoiNode> { _graph._nodes[_points[n]] };
      clusters.Add(newCluster);
      seen.UnionWith(newCluster);
    }

    for (int safe = 0; safe < 100; safe++)
    {
      if (seen.Count >= _points.Count)
      {
        Debug.Log("Done in "+safe+" steps");
        break;
      }
      yield return new WaitForSeconds(0.0f);
      ClusterizeGreedy(seen);
    }
   
    
    //
    //foreach (var node in _graph._nodes)
    //{
    //  clusters.Add(new HashSet<VoronoiNode>() { node.Value });
    //}

  }

  private void ClusterizeGreedy(HashSet<VoronoiNode> seen)
  {
    var frontiers = new Dictionary<HashSet<VoronoiNode>, HashSet<VoronoiNode>>();
    var tmpNewNeighbors = new HashSet<VoronoiNode>();
    foreach (var cluster in clusters)
    {
      frontiers[cluster] = new HashSet<VoronoiNode>(cluster);//set frontier to the one poly, each cluster has
    }
    foreach (var cluster in clusters)
    {
      tmpNewNeighbors.Clear();
      tmpNewNeighbors.UnionWith(frontiers[cluster].SelectMany(c=>c.GetNeighbors()));
      tmpNewNeighbors.ExceptWith(cluster);
      tmpNewNeighbors.ExceptWith(seen);
      frontiers[cluster].Clear();
      frontiers[cluster].UnionWith(tmpNewNeighbors);
      cluster.UnionWith(tmpNewNeighbors);
      seen.UnionWith(tmpNewNeighbors);
    }
  }

  private void ClusterizeStep(List<HashSet<VoronoiNode>> list)
  {
    var seen = new HashSet<VoronoiNode>();
    var ClusterizationQueue = new Dictionary<HashSet<VoronoiNode>, HashSet<VoronoiNode>>();
    foreach (var cluster in list)
    {
      Vector3 ClusterMove = Vector3.zero;
      foreach (var node in cluster)
      {
        ClusterMove += node.data.Velocity * MathS.SphToCartesian(node.centre) - MathS.SphToCartesian(node.centre);

        foreach (var neighbor in node.GetNeighbors())
        {
          if (!cluster.Contains(neighbor) && !seen.Contains(neighbor))
          {
            seen.Add(neighbor);
            //this is a cluster neihghbor
            var NeighborMove = neighbor.data.Velocity * MathS.SphToCartesian(neighbor.centre) - MathS.SphToCartesian(neighbor.centre);

            var metric = Vector3.Dot(NeighborMove.normalized, ClusterMove.normalized);

            if (metric > 0.5f) //angle between vectors is less than 30`
            {
              if (!ClusterizationQueue.ContainsKey(cluster))
              {
                ClusterizationQueue.Add(cluster, new HashSet<VoronoiNode>());
              }
              ClusterizationQueue[cluster].Add(neighbor);
            }
          }
        }
      }
    }

    foreach (var pair in ClusterizationQueue)
    {
      var desiredCluster = pair.Key;
      var nodesToAddTo = pair.Value;

      foreach (var cluster in list)
      {
        if (cluster != desiredCluster && cluster.Overlaps(nodesToAddTo))
        {
          nodesToAddTo.UnionWith(cluster);
          cluster.Clear();
        }
      }

      list.RemoveAll(c => c.Count == 0);

      desiredCluster.UnionWith(nodesToAddTo);
    }
  }

  private NodeData GetNoise(Vector3 p)
  {
    var angle = Random.Range(0f, 30f) * 1.5f; //degree per 100000 y
    var axis = Random.onUnitSphere;
    return new NodeData();
    //{ Velocity = Quaternion.AngleAxis(angle, axis) };
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
      //_result.DrawDebug();
    }

    var currentClusterEdges = new HashSet<VectorPair>();
    foreach (var cluster in clusters)
    {

      foreach (var node in cluster)
      {
        currentClusterEdges.SymmetricExceptWith(node.GetEdges());
      }

      foreach (var edge in currentClusterEdges)
      {
        DebugHelper.DrawArc(edge._one, edge._other, Color.magenta);
      }
      currentClusterEdges.Clear();
    }

    if (_graph != null)
    {
      foreach (var value in _graph._nodes.Values)
      {
        foreach (var partVelocity in value.data.PartVelocities)
        {
          // DebugHelper.DrawQuatArrow(partVelocity, MathS.SphToCartesian(value.centre), 0.325f, Color.blue);
        }
        DebugHelper.DrawQuatArrow(value.data.Velocity, MathS.SphToCartesian(value.centre), 0.325f, Color.black, 6);
      }
    }
  }
}
