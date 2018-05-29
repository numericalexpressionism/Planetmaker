using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoronoiSph : MonoBehaviour
{
  class VoronoiEventComparer : Comparer<VoronoiEvent>
  {
    static VoronoiEventComparer()
    {
      Instance = new VoronoiEventComparer();
    }

    public static VoronoiEventComparer Instance { get; private set; }
    public override int Compare(VoronoiEvent x, VoronoiEvent y)
    {
      return Comparer<float>.Default.Compare(x.GetLat(), y.GetLat());
    }
  }

  public List<Vector2> points = new List<Vector2>();

  void OnEnable()
  {
    _result = new VoronoiGraph();
    for (int index = 0; index < points.Count; index++)
    {
      points[index] = MathS.CartesianToSph(Random.onUnitSphere);
    }
    StartCoroutine(BuildVoronoi(_result));
  }

  private PriorityQueue<VoronoiEvent> _events = new PriorityQueue<VoronoiEvent>(p=>p.GetLat());
  private LinkedList<PointEvent> _beachLine = new LinkedList<PointEvent>();
  private VoronoiGraph _result;

  private IEnumerator BuildVoronoi(VoronoiGraph result)
  {
    //Events = new PriorityQueue<float, VoronoiEvent>();
    //BeachLine.Clear();
    //if (Points.Count <= 3) yield break;
    //
    //foreach (var point in Points)
    //{
    //  Events.Enqueue(new PointEvent(point), e => e.GetLat());
    //}
    //
    //foreach (var Evt in Events)
    //{
    //  Evt.DrawDebug();
    //}
    //foreach (var point in BeachLine)
    //{
    //  DebugHelper.DrawPoint(MathS.SphToCartesian(point.Point), 1, Color.black);
    //}
    //result.DrawDebug();
    //yield return null;
    //
    //while (Events.Count > 0)
    //{
    //  var nextEvent = Events.Dequeue();
    //  DebugHelper.DrawGeodesic(nextEvent.GetLat(), Color.magenta);
    //  yield return null;
    //  nextEvent.Process(BeachLine, Events, result);
    //  foreach (var Evt in Events)
    //  {
    //    Evt.DrawDebug();
    //  }
    //
    //  if (BeachLine.Count > 1)
    //  {
    //    var curr = BeachLine.First;
    //
    //    while (curr.Next != null)
    //    {
    //      DebugHelper.DrawArc(MathS.SphToCartesian(curr.Value.Point), MathS.SphToCartesian(curr.Next.Value.Point), Color.black);
    //      DebugHelper.DrawPoint(MathS.SphToCartesian(curr.Value.Point), 2, Color.black);
    //      curr = curr.Next;
    //    }
    //    DebugHelper.DrawArc(MathS.SphToCartesian(curr.Value.Point), MathS.SphToCartesian(BeachLine.First.Value.Point), Color.black);
    //  }
    //
    //  result.DrawDebug();
    //}
    //Debug.Log("DONE");
    yield break;
  }

  void Update()
  {

  }
}