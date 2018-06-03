using System.Collections.Generic;
using UnityEngine;

class PointEvent : VoronoiEvent
{
  public readonly Vector2 point;

  public PointEvent(Vector2 point)
  {
    this.point = point;
  }

  public override float GetLat()
  {
    return point.y;
  }

  public override void Process(LinkedList<BeachLineArc> beachLine, PriorityQueue<VoronoiEvent> events, VoronoiEdgesList result)
  {
    float phi = point.x > 0 ? point.x : point.x + 360;

    if (beachLine.Count == 0)
    {
      var singleArc = new BeachLineArc(point);
      var n = beachLine.AddFirst(singleArc);
      singleArc.SetNodes(n);
      return;
    }

    if (beachLine.Count == 1)
    {
      var newArc = new BeachLineArc(point);
      if (point.x > beachLine.First.Value.focus.x)
      {
        beachLine.AddAfter(beachLine.First, newArc);
      }
      else
      {
        beachLine.AddBefore(beachLine.First, newArc);
      }
      beachLine.First.Value.SetNodes(beachLine.First);
      beachLine.Last.Value.SetNodes(beachLine.Last);
      return;
    }

    var selected = FindArc(beachLine, phi, true) ?? FindArc(beachLine, phi, false);

    if (selected != null)
    {
      CutInArc(beachLine, events, selected);
    }
    else
    {
      Debug.LogErrorFormat("VERY ERROR - TRY ALTER, point at {0} discarded", point.ToString());
      //Dump(phi, beachLine);
    }
  }

  private LinkedListNode<BeachLineArc> FindArc(LinkedList<BeachLineArc> beachLine, float phi, bool strict)
  {
    List<LinkedListNode<BeachLineArc>> selected = new List<LinkedListNode<BeachLineArc>>();
    LinkedListNode<BeachLineArc> cur = beachLine.First;
    while (cur != null)
    {
      var leftPhi = cur.Value.GetLeftPhi(point.y);
      var rightPhi = cur.Value.GetRightPhi(point.y);
      bool inside = MathS.IsOnArc(phi, leftPhi, rightPhi);

      if (inside)
      {
        if ((leftPhi * 65536 >= rightPhi * 65536) && strict)
        {
          if (MathS.IsOnArc(phi, rightPhi, leftPhi))
          {
            Debug.LogWarning("candidate accepted");
          }
          else
          {
            //Debug.LogWarning("candidate discarded");
          }
        }
        else
        {
          selected.Add(cur);
        }
      }
      cur = cur.Next;
    }

    if (selected.Count == 0)
    {
      return null;
    }

    if (selected.Count == 1)
    {
      return selected[0];
    }

    //Debug.LogWarningFormat("{0} candidates", selected.Count);
    int minIndex = 0;
    var firstLeftPhi = selected[0].Value.GetLeftPhi(point.y);
    var firstRightPhi = selected[0].Value.GetRightPhi(point.y);
    float mindelta = Mathf.Abs(Mathf.DeltaAngle(phi, firstLeftPhi)) + Mathf.Abs(Mathf.DeltaAngle(phi, firstRightPhi));
//Debug.LogFormat("candidate [{0} - {1}] : {2}", firstLeftPhi, firstRightPhi, phi);
    for (int index = 1; index < selected.Count; index++)
    {
      var node = selected[index];
      var leftPhi = node.Value.GetLeftPhi(point.y);
      var rightPhi = node.Value.GetRightPhi(point.y);

      var delta = Mathf.Abs(Mathf.DeltaAngle(phi, leftPhi)) + Mathf.Abs(Mathf.DeltaAngle(phi, rightPhi));
      if (delta < mindelta)
      {
        mindelta = delta;
        minIndex = index;
      }

      //Debug.LogFormat("candidate [{0} - {1}] : {2}", leftPhi, rightPhi, phi);
    }
   // Debug.LogFormat("selected {0}", minIndex);
    return selected[minIndex];
  }

  private void Dump(float phi, LinkedList<BeachLineArc> beachLine)
  {
    LinkedListNode<BeachLineArc> cur = beachLine.First;
    while (cur != null)
    {
      var leftPhi = cur.Value.GetLeftPhi(point.y);
      var rightPhi = cur.Value.GetRightPhi(point.y);
      bool inside = MathS.IsOnArc(phi, leftPhi, rightPhi);
      Debug.LogFormat("full arc [{0}_{1}] : {2} = {3}", leftPhi, rightPhi, phi, inside);
      cur = cur.Next;
    }
    Debug.LogError("DUMP END");
  }

  private void CutInArc(LinkedList<BeachLineArc> beachLine, PriorityQueue<VoronoiEvent> events, LinkedListNode<BeachLineArc> cur)
  {
    BeachLineArc currentArc = cur.Value;
    //insert new arc in the current one:
    //current arc is split in two
    //new arc inserts between them

    var newMid = new BeachLineArc(point);
    var newLeft = new BeachLineArc(currentArc.focus);
    var newRight = new BeachLineArc(currentArc.focus);

    if (currentArc.Event != null)
    {
      cur.Value.Event.MarkDeleted();
    }

    var lnode = beachLine.AddBefore(cur, newLeft);
    var rnode = beachLine.AddAfter(cur, newRight);
    var cnode = beachLine.AddBefore(rnode, newMid);

    beachLine.Remove(cur);

    newMid.SetNodes(cnode);
    newLeft.SetNodes(lnode);
    newRight.SetNodes(rnode);

    BeachLineArc.MakeEvents(events, newLeft, newRight);
  }


  public override void DrawDebug()
  {
    DebugHelper.DrawPoint(MathS.SphToCartesian(point), 1, Color.blue);
  }
}