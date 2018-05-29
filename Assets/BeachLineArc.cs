using System.Collections.Generic;
using Assets;
using UnityEngine;

class BeachLineArc
{
  public readonly Vector2 focus;
  public LinkedListNode<BeachLineArc> Left { get { return Centre.Previous ?? Centre.List.Last; } }
  public LinkedListNode<BeachLineArc> Right { get { return Centre.Next ?? Centre.List.First; } }
  public LinkedListNode<BeachLineArc> Centre { get; private set; }
  public CircleEvent Event { get; private set; }
  public void SetNodes(LinkedListNode<BeachLineArc> Node)
  {
    Centre = Node;
  }
  public BeachLineArc(Vector2 focus)
  {
    this.focus = focus;
  }

  public void MakePointEvent()
  {
    //DebugHelper.DrawPoint(MathS.SphToCartesian(Left.Value.focus), 5, Color.black);
    //DebugHelper.DrawPoint(MathS.SphToCartesian(Centre.Value.focus), 5, Color.black);
    //DebugHelper.DrawPoint(MathS.SphToCartesian(Right.Value.focus), 5, Color.black);

    Event = CircleEvent.Create(Left, Centre, Right);
  }
  public static void MakeEvents(PriorityQueue<VoronoiEvent> events, BeachLineArc right, BeachLineArc left)
  {
    if (right.Event != null)
    {
      right.Event.MarkDeleted();
    }
    if (left.Event != null)
    {
      left.Event.MarkDeleted();
    }

    left.MakePointEvent();
    right.MakePointEvent();

    if (left.Event == right.Event)
    {
      right.Event = null;
    }
    else if (left.Event == CircleEvent.Inverse(right.Event))
    {
      if (right.Event.r <= left.Event.r)
      {
        left.Event = null;
      }
      else
      {
        right.Event = null;
      }
    }

    if (right.Event != null)
    {
      events.Enqueue(right.Event, true);
    }
    if (left.Event != null)
    {
      events.Enqueue(left.Event, true);
    }
  }

  public float GetLeftPhi(float directrixTh)
  {
    if (Left == Centre)
    {
      return MathS.NormalizeAngle(focus.x - 180);
    }
    return MathS.ParabolaIntersectionPointFi(Left.Value.focus, focus, directrixTh);
  }

  public float GetRightPhi(float directrixTh)
  {
    if (Right == Centre)
    {
      return MathS.NormalizeAngle(focus.x + 180);
    }
    return Right.Value.GetLeftPhi(directrixTh);
  }

  public void DrawDebug(float directrixTh, float dt, Color color)
  {
    float l = GetLeftPhi(directrixTh);
    float r = GetRightPhi(directrixTh);

    if (Mathf.Abs(directrixTh - focus.y) < Mathf.Epsilon)
    {
      var start = MathS.SphToCartesian(focus.x, directrixTh);
      var intersection = Left.Value.focus != Centre.Value.focus
        ? MathS.CalcParabolaPoint(Left.Value.focus, directrixTh, l)
        : MathS.SphToCartesian(0, 0);
      DebugHelper.DrawArc(start, intersection, Color.white, color, 32);
      return;
    }

    while (r < l)
    {
      r += 360;
    }

    float dPhi = Mathf.Clamp((r - l) / 12f, 0.1f, 5.0f);

    for (float f1 = l; f1 < r; f1 += dPhi)
    {
      float f2 = Mathf.Clamp(f1 + dPhi, l, r);
      var p1 = MathS.CalcParabolaPoint(focus, directrixTh, f1);
      var p2 = MathS.CalcParabolaPoint(focus, directrixTh, f2);
      var c = Color.Lerp(color, color, (f1-l)/(r - l));
      Debug.DrawLine(p1, p2, c, dt);
    }
  }
}