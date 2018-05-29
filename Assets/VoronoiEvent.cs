using System.Collections.Generic;

internal abstract class VoronoiEvent
{
  public bool IsDeleted { get; private set; }
  public abstract float GetLat();
  public abstract void Process(LinkedList<BeachLineArc> beachLine, PriorityQueue<VoronoiEvent> events, VoronoiGraph result);
  public abstract void DrawDebug();

  public void MarkDeleted()
  {
    IsDeleted = true;
  }
}